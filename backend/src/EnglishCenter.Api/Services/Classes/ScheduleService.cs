namespace EnglishCenter.Api.Services.Classes;

using System.Text.Json;
using System.Text.Json.Nodes;
using EnglishCenter.Api.Common.Auditing;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Common.Security;
using EnglishCenter.Api.Common.Time;
using EnglishCenter.Api.Domain.Training;
using EnglishCenter.Api.Infrastructure.Classes;
using EnglishCenter.Api.Services.Academics;
using EnglishCenter.Api.Services.Auth;
using EnglishCenter.Api.Services.Replay;

public sealed class ScheduleService(SqlClassRepository repository, IReplayRepository markers, BusinessClock clock, int weeklyBudget)
{
    public async Task<ClassCommandOutcome> Command(VerifiedSessionToken token, ScheduleInput input, CancellationToken ct)
    {
        await markers.AcquireAsync(ReplayService.RequestResource(token.AccountId, input.Intent.Action, input.Request), ct);
        var oldReference = input.Api == 120 ? null : await repository.DiscoverSchedule(input.Id, ct) ?? throw new ApiErrorException(ApiErrorCode.NOT_FOUND);
        var classId = oldReference?.ClassId ?? input.Id;
        var reference = await repository.Discover(classId, ct) ?? throw new ApiErrorException(ApiErrorCode.NOT_FOUND);
        var registrations = await repository.Registrations(classId, null, ct);
        var gates = registrations.Select(x => "EC:REG:" + x).Concat(new[] { "EC:BANDSET", "EC:PROGRAM:" + reference.ProgramId, "EC:TIMETABLE", "EC:CLASS:" + classId });
        foreach (var gate in gates.Distinct().Order(StringComparer.Ordinal)) await markers.AcquireAsync(gate, ct);
        await repository.PrepareAccountsAsync(token, await repository.Accounts(registrations, reference.TeacherId, null, ct), ct);
        await AcademicAuthority.RequireAsync(repository, token, clock, ct);
        var replay = new ReplayService(markers, clock);
        var repeated = await replay.BeginAsync(token.AccountId, input.Request, input.Intent, async (stored, c) =>
        {
            await AcademicAuthority.RequireAsync(repository, token, clock, c);
            return stored.Target["classId"] == classId &&
                (input.Api == 120 || stored.Target[input.Api is 122 or 126 ? "scheduleId" : "oldScheduleId"] == input.Id) && await repository.Matches(stored.Target, c);
        }, ct);
        if (repeated is not null) return new(await Result(token, input.Api, repeated.Target, repeated.OperationId, ct), true);

        if(input.Api==120)await repository.CheckWeeklySize(classId,weeklyBudget,ct);
        var state = await repository.LockState(classId, registrations, null, ct);
        if(state.Class.TeacherId!=reference.TeacherId)throw AcademicGuard.Conflict();
        var now = clock.GetUtcNow();
        var operation = Guid.NewGuid();
        var target = new Dictionary<string, int> { ["classId"] = classId };
        ScheduleCandidate[] windows;
        DateTime? oldStart = null;
        if (input.Api == 120)
        {
            var rule = WeeklyRule.Parse(input.Body.GetProperty("rule"));
            windows = rule.Expand(new(weeklyBudget));
            var sent = input.Body.GetProperty("candidates").EnumerateArray().Select(ScheduleCandidate.Parse).ToArray();
            if (windows.Length == 0 || !windows.SequenceEqual(sent)) throw ApiErrorException.Validation("candidates");
            var preview = await repository.WeeklyPreview(state, rule, now.UtcDateTime, weeklyBudget, ct);
            input.RequireManifest(preview["manifest"] as JsonObject ?? throw AcademicGuard.Conflict());
            foreach (var candidate in windows)
            {
                var row = Create(classId, candidate, Guid.NewGuid(), null);
                repository.Add(row); await repository.SaveAsync(ct);
                await Audit(input, token, row, null, operation, now, new() { ["classId"] = classId }, null, false, ct);
            }
        }
        else if(input.Api==126)
        {
            var row=state.Schedules.SingleOrDefault(x=>x.ScheduleId==input.Id)??throw new ApiErrorException(ApiErrorCode.NOT_FOUND);
            input.RequireManifest(TeachingHeld.Manifest(state,row));
            if(!TeachingHeld.CanConfirm(state,row,now.UtcDateTime))throw AcademicGuard.Conflict();
            var before=Snapshot(row);row.OccurrenceState="HELD";row.HeldBy=token.AccountId;row.HeldAt=now.UtcDateTime;
            await repository.SaveAsync(ct);await Audit(input,token,row,before,operation,now,new(){["classId"]=classId},null,false,ct);
            target["scheduleId"]=row.ScheduleId;windows=[];
        }
        else
        {
            var old = state.Schedules.SingleOrDefault(x => x.ScheduleId == input.Id) ?? throw new ApiErrorException(ApiErrorCode.NOT_FOUND);
            oldStart = ClassWire.Window(old).StartAt;
            var body = JsonNode.Parse(input.Body.GetRawText())!.AsObject(); body.Remove("expectedVersions"); body.Remove("expectedSet");
            using var draft = JsonDocument.Parse(body.ToJsonString());
            var candidate = ScheduleRules.Destination(draft.RootElement, old.SessionType, input.Api == 124);
            var reason = input.Api == 124 ? ScheduleRules.Reason(draft.RootElement) : null;
            var preview = await repository.EditPreview(state, old, candidate, reason, now.UtcDateTime, ct);
            input.RequireManifest(preview["manifest"] as JsonObject ?? throw AcademicGuard.Conflict());
            windows = [candidate];
            var before = Snapshot(old);
            if (input.Api == 122)
            {
                if (ClassWire.Window(old) == candidate) throw ApiErrorException.Validation("newDate");
                old.SessionDate = candidate.Date; old.StartTime = candidate.StartTime; old.EndTime = candidate.EndTime; old.Room = candidate.Room;
                await repository.SaveAsync(ct);
                await Audit(input, token, old, before, operation, now, new() { ["classId"] = classId }, null, false, ct);
                target["scheduleId"] = old.ScheduleId;
            }
            else
            {
                old.OccurrenceState = "NOT_HELD"; old.IsCurrent = false;
                old.NotHeldBy = token.AccountId; old.NotHeldAt = now.UtcDateTime; old.NotHeldReason = reason; old.ReplacementOperationId = operation;
                // Release the filtered current-key index in this transaction before inserting the successor.
                await repository.SaveAsync(ct);
                var next = Create(classId, candidate, old.ObligationKey, old.ScheduleId);
                repository.Add(next); await repository.SaveAsync(ct);
                var parent = new JsonObject { ["classId"] = classId, ["oldScheduleId"] = old.ScheduleId, ["newScheduleId"] = next.ScheduleId };
                await Audit(input, token, old, before, operation, now, parent, reason, false, ct);
                await Audit(input, token, next, null, operation, now, parent, reason, true, ct);
                target["oldScheduleId"] = old.ScheduleId; target["newScheduleId"] = next.ScheduleId;
            }
        }
        await replay.CompleteAsync(token.AccountId, input.Request, input.Intent, operation, target, ct);
        await new CurrentSessionService(repository, clock).TouchApprovedInteractiveActivityAsync(token, ct);
        var result = await Result(token, input.Api, target, operation, ct);
        var finalNow = clock.GetUtcNow().UtcDateTime;
        if (windows.Any(x => x.StartAt <= finalNow) || input.Api == 122 && oldStart <= finalNow) throw AcademicGuard.Conflict();
        return new(result, false);
    }

    private async Task<JsonObject> Result(VerifiedSessionToken token, int api, IReadOnlyDictionary<string, int> target, Guid operation, CancellationToken ct)
    {
        var state = await repository.State(target["classId"], ct);
        JsonObject result;
        if (api == 120) result = ClassWire.Object(new { created = await repository.WeeklyCreated(operation, ct), context = ClassPreviews.SchedulingContext(state), operationId = operation });
        else if (api is 122 or 126) result = ClassWire.Object(new { schedule = await repository.ScheduleDetail(target["scheduleId"], clock.GetUtcNow().UtcDateTime, ct), operationId = operation });
        else result = ClassWire.Object(new { oldScheduleId = target["oldScheduleId"], newScheduleId = target["newScheduleId"], classId = target["classId"], obligationKey = state.Schedules.Single(x => x.ScheduleId == target["newScheduleId"]).ObligationKey, operationId = operation });
        var live = await AcademicAuthority.RequireAsync(repository, token, clock, ct);
        if (!clock.IsBefore(live.EffectiveDeadline)) throw new ApiErrorException(ApiErrorCode.AUTHENTICATION_REQUIRED);
        return result;
    }
    private static Schedule Create(int classId, ScheduleCandidate c, Guid key, int? predecessor) => new()
    { ClassId = classId, SessionType = c.Type, ObligationKey = key, ReplacesScheduleId = predecessor, IsCurrent = true,
        SessionDate = c.Date, StartTime = c.StartTime, EndTime = c.EndTime, Room = c.Room, OccurrenceState = "PLANNED" };
    private static JsonObject Snapshot(Schedule s) => ClassWire.Object(new { classId = s.ClassId, sessionType = s.SessionType,
        sessionDate = s.SessionDate.ToString("yyyy-MM-dd"), startTime = s.StartTime.ToString("HH:mm"), endTime = s.EndTime.ToString("HH:mm"), room = s.Room,
        obligationKey = s.ObligationKey, replacesScheduleId = s.ReplacesScheduleId, occurrenceState = s.OccurrenceState, isCurrent = s.IsCurrent,
        heldBy=s.HeldBy,heldAt=s.HeldAt is {} held?ClassWire.At(held):null,notHeldBy = s.NotHeldBy, notHeldAt = s.NotHeldAt is { } at ? ClassWire.At(at) : null, replacementOperationId = s.ReplacementOperationId });
    private Task Audit(ScheduleInput i, VerifiedSessionToken t, Schedule s, JsonObject? before, Guid operation, DateTimeOffset now,
        JsonObject parent, string? reason, bool effect, CancellationToken ct) => repository.AuditAsync(AuditEntry.Mandatory(
            i.Intent.Action.Code + (effect ? ".EFFECT" : ""), t.AccountId, "Schedule", s.ScheduleId, operation, now,
            new(before, Snapshot(s), parent, reason)), ct);
}
