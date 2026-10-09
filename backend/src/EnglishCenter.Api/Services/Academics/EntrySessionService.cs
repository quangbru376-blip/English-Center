namespace EnglishCenter.Api.Services.Academics;

using System.Text.Json;
using System.Text.Json.Nodes;
using EnglishCenter.Api.Common.Auditing;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Common.Security;
using EnglishCenter.Api.Common.Serialization;
using EnglishCenter.Api.Common.Time;
using EnglishCenter.Api.Domain.Admissions;
using EnglishCenter.Api.Domain.Identity;
using EnglishCenter.Api.Services.Auth;
using EnglishCenter.Api.Services.Replay;

public interface IEntrySessionRepository : IAcademicRepository
{
    Task<int[]> LinkedRegistrationsAsync(int id, CancellationToken ct);
    Task<int?> DiscoverGraderAsync(int id, CancellationToken ct);
    Task<SessionState> StateAsync(int id, CancellationToken ct);
    Task<Account> TeacherAsync(int id, CancellationToken ct);
    Task<bool> TeacherRoleAsync(int role, CancellationToken ct);
    Task InsertAsync(EntryTestSession session, CancellationToken ct);
    Task<IReadOnlyList<SafeScheduleConflict>> ConflictsAsync(SessionWindow window, int grader, int excluded, CancellationToken ct);
}
public sealed class EntrySessionService(IEntrySessionRepository repository, IReplayRepository markers, BusinessClock clock)
{
    public async Task<SessionCommandResult> CommandAsync(VerifiedSessionToken token, AcademicInput input, CancellationToken ct)
    {
        var action = input.Intent.Action; var body = input.Body;
        await markers.AcquireAsync(ReplayService.RequestResource(token.AccountId, action, input.RequestId), ct);
        var regs = input.TargetId > 0 ? await repository.LinkedRegistrationsAsync(input.TargetId, ct) : [];
        foreach (var resource in regs.Select(x => "EC:REG:" + x).Append("EC:TIMETABLE").Order(StringComparer.Ordinal)) await markers.AcquireAsync(resource, ct);
        var currentGrader = input.TargetId > 0 ? await repository.DiscoverGraderAsync(input.TargetId, ct) : null;
        var targetGrader = action.ApiId is 55 or 64 ? body.GetProperty("graderId").GetInt32() : currentGrader;
        await repository.PrepareAccountsAsync(token, new[] { currentGrader, targetGrader }.Where(x => x is not null).Select(x => x!.Value), ct);
        var current = await AcademicAuthority.RequireAsync(repository, token, clock, ct); var replay = new ReplayService(markers, clock); SessionState? repeated = null;
        var done = await replay.BeginAsync(token.AccountId, input.RequestId, input.Intent, async (stored, c) => { await AcademicAuthority.RequireAsync(repository, token, clock, c); repeated = await repository.StateAsync(stored.Target["sessionId"], c); return true; }, ct);
        if (done is not null) return new(SessionProjection.Detail(repeated!, clock.GetUtcNow(), await repository.ConflictsAsync(SessionWindow.From(repeated!.Session), repeated.Session.GraderId, repeated.Session.SessionId, ct)), done.OperationId) { Replayed = true };
        var now = clock.GetUtcNow(); var operation = Guid.NewGuid(); SessionState? source = null; EntryTestSession session;
        if (action.ApiId == 55)
        { var window = SessionWindow.Parse(body); await ValidateResources(window, targetGrader!.Value, 0, now, ct); session = new() { StartAt = window.StartAt, EndAt = window.EndAt, Room = window.Room, Capacity = body.GetProperty("capacity").GetInt32(), GraderId = targetGrader.Value, Status = "CLOSED" }; await repository.InsertAsync(session, ct); }
        else
        {
            source = await repository.StateAsync(input.TargetId, ct); session = source.Session;
            if (session.GraderId != currentGrader || !regs.Order().SequenceEqual(SessionProjection.Bindings(source).Select(x => x.RegistrationId).Distinct().Order())) throw AcademicGuard.Conflict();
            AcademicGuard.Version(body.GetProperty("expectedVersions").GetProperty("session"), session.RowVersion);
            if (session.Status == "CANCELLED" || session.StartAt <= now.UtcDateTime) throw AcademicGuard.Conflict();
        }
        var old = source is null ? null : State(session);
        if (action.ApiId == 57)
        { if (source!.Bookings.Count > 0) throw AcademicGuard.Conflict(); var window = SessionWindow.Parse(body); await ValidateResources(window, session.GraderId, session.SessionId, now, ct); session.StartAt = window.StartAt; session.EndAt = window.EndAt; session.Room = window.Room; }
        else if (action.ApiId == 58)
        { if (session.Status != "CLOSED" || source!.Bookings.Count(x => x.Status == "SCHEDULED") > session.Capacity) throw AcademicGuard.Conflict(); await ValidateResources(SessionWindow.From(session), session.GraderId, session.SessionId, now, ct); session.Status = "OPEN"; }
        else if (action.ApiId == 59)
        { if (session.Status != "OPEN") throw AcademicGuard.Conflict(); session.Status = "CLOSED"; }
        else if (action.ApiId is 61 or 64)
        {
            CheckManifest(source!, body);
            if (action.ApiId == 61)
                await CancelMembership(source!, body.GetProperty("reason").GetString()!, token.AccountId, operation, now, action.Code, ct);
            else
                await ChangeGrader(source!, body, targetGrader!.Value, now, ct);
        }
        // Re-sample immediately before persisting; previews never freeze the clock.
        if (!clock.IsBefore(current.EffectiveDeadline)) throw new ApiErrorException(ApiErrorCode.AUTHENTICATION_REQUIRED);
        if (session.StartAt <= clock.GetUtcNow().UtcDateTime) throw AcademicGuard.Conflict();
        await repository.SaveAsync(ct); await repository.AuditAsync(AuditEntry.Mandatory(action.Code, token.AccountId, "EntryTestSession", session.SessionId, operation, now, new(old, State(session), Reason: action.ApiId is 61 or 64 ? body.GetProperty("reason").GetString() : null)), ct);
        await replay.CompleteAsync(token.AccountId, input.RequestId, input.Intent, operation, new Dictionary<string, int> { { "sessionId", session.SessionId } }, ct);
        await new CurrentSessionService(repository, clock).TouchApprovedInteractiveActivityAsync(token, ct); await AcademicAuthority.RequireAsync(repository, token, clock, ct);
        if (!clock.IsBefore(current.EffectiveDeadline)) throw new ApiErrorException(ApiErrorCode.AUTHENTICATION_REQUIRED);
        if (session.StartAt <= clock.GetUtcNow().UtcDateTime) throw AcademicGuard.Conflict();
        var result = await repository.StateAsync(session.SessionId, ct);
        var conflicts = await repository.ConflictsAsync(SessionWindow.From(session), session.GraderId, session.SessionId, ct);
        if (!clock.IsBefore(current.EffectiveDeadline)) throw new ApiErrorException(ApiErrorCode.AUTHENTICATION_REQUIRED);
        if (session.StartAt <= clock.GetUtcNow().UtcDateTime) throw AcademicGuard.Conflict();
        return new(SessionProjection.Detail(result, clock.GetUtcNow(), conflicts), operation);
    }
    private async Task ChangeGrader(SessionState source, JsonElement body, int candidateId, DateTimeOffset now, CancellationToken ct)
    {
        if (SessionProjection.HasResults(source)) throw AcademicGuard.Conflict();
        if (body.GetProperty("expectedSet").GetProperty("currentGraderId").GetInt32() != source.Session.GraderId) throw AcademicGuard.Conflict();
        var candidate = await repository.TeacherAsync(candidateId, ct);
        AcademicGuard.Version(body.GetProperty("expectedVersions").GetProperty("grader"), candidate.RowVersion);
        await ValidateResources(SessionWindow.From(source.Session), candidate.AccountId, source.Session.SessionId, now, ct);
        if (candidate.AccountId == source.Session.GraderId) throw ApiErrorException.Validation("graderId");
        source.Session.GraderId = candidate.AccountId;
    }
    private async Task CancelMembership(SessionState source, string reason, int actor, Guid operation, DateTimeOffset now, string action, CancellationToken ct)
    {
        var session = source.Session;
        session.Status = "CANCELLED";
        session.CancelledBy = actor;
        session.CancelledAt = now.UtcDateTime;
        session.CancellationReason = reason;
        session.CancellationOperationId = operation;
        foreach (var booking in source.Bookings.Where(x => x.Status == "SCHEDULED"))
        {
            var before = BookingState(booking);
            booking.CancelledFromStatus = booking.Status;
            booking.Status = "CANCELLED";
            booking.CancelledBy = actor;
            booking.CancelledAt = now.UtcDateTime;
            booking.CancellationReason = reason;
            booking.CancellationOperationId = operation;
            await repository.AuditAsync(AuditEntry.Mandatory(action + ".EFFECT", actor, "EntryTestBooking", booking.BookingId, operation, now,
                new(before, BookingState(booking), Parent: new JsonObject { ["sessionId"] = session.SessionId })), ct);
        }
    }
    private async Task ValidateResources(SessionWindow window, int grader, int excluded, DateTimeOffset now, CancellationToken ct)
    { var account = await repository.TeacherAsync(grader, ct); if (window.StartAt <= now.UtcDateTime || account.Status != "ACTIVE" || !await repository.TeacherRoleAsync(account.RoleId, ct) || (await repository.ConflictsAsync(window, grader, excluded, ct)).Count > 0) throw AcademicGuard.Conflict(); }
    public static void CheckManifest(SessionState source, JsonElement body)
    {
        var versions = body.GetProperty("expectedVersions"); var set = body.GetProperty("expectedSet"); var bindings = SessionProjection.Bindings(source);
        AcademicGuard.Set(set.GetProperty("scheduledBookingIds"), bindings.Select(x => x.BookingId));
        var supplied = set.GetProperty("bindings").EnumerateArray().Select(x => new SessionBookingBinding(x.GetProperty("bookingId").GetInt32(), x.GetProperty("attemptId").GetInt32(), x.GetProperty("registrationId").GetInt32())).OrderBy(x => x.BookingId).ToArray(); if (!supplied.SequenceEqual(bindings)) throw AcademicGuard.Conflict();
        AcademicGuard.Versions(versions.GetProperty("bookings"), source.Bookings.Where(x => x.Status == "SCHEDULED").Select(x => new VersionedId(x.BookingId, RowVersionCodec.Encode(x.RowVersion))));
        var attemptIds = bindings.Select(x => x.AttemptId).ToHashSet(); var registrationIds = bindings.Select(x => x.RegistrationId).ToHashSet();
        AcademicGuard.Versions(versions.GetProperty("attempts"), source.Attempts.Where(x => attemptIds.Contains(x.AttemptId)).Select(x => new VersionedId(x.AttemptId, RowVersionCodec.Encode(x.RowVersion))));
        AcademicGuard.Versions(versions.GetProperty("registrations"), source.Registrations.Where(x => registrationIds.Contains(x.RegistrationId)).Select(x => new VersionedId(x.RegistrationId, RowVersionCodec.Encode(x.RowVersion))));
    }
    private static JsonObject State(EntryTestSession s) => new() { ["startAt"] = AcademicGuard.Instant(s.StartAt), ["endAt"] = AcademicGuard.Instant(s.EndAt), ["room"] = s.Room, ["capacity"] = s.Capacity, ["graderId"] = s.GraderId, ["status"] = s.Status };
    private static JsonObject BookingState(EntryTestBooking b) => new() { ["status"] = b.Status, ["cancelledBy"] = b.CancelledBy, ["cancelledAt"] = b.CancelledAt is { } at ? AcademicGuard.Instant(at) : null, ["cancelledFromStatus"] = b.CancelledFromStatus, ["cancellationOperationId"] = b.CancellationOperationId?.ToString("D") };
}
