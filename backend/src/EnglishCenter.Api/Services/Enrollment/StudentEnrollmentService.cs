namespace EnglishCenter.Api.Services.Enrollment;

using System.Text.Json.Nodes;
using EnglishCenter.Api.Common.Auditing;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Common.Replay;
using EnglishCenter.Api.Common.Security;
using EnglishCenter.Api.Common.Time;
using EnglishCenter.Api.Domain.Admissions;
using EnglishCenter.Api.Infrastructure.Enrollment;
using EnglishCenter.Api.Services.Academics;
using EnglishCenter.Api.Services.Auth;
using EnglishCenter.Api.Services.Replay;

public sealed class StudentEnrollmentService(SqlEnrollmentRepository repository, IReplayRepository markers, BusinessClock clock)
{
    public async Task<object> CommandAsync(VerifiedSessionToken token, EnrollmentInput input, CancellationToken ct)
    {
        if (input.Api is not (25 or 28 or 30 or 31 or 32 or 36)) throw new ApiErrorException(ApiErrorCode.FORBIDDEN);
        await markers.AcquireAsync(ReplayService.RequestResource(token.AccountId, input.Intent.Action, input.RequestId), ct);
        var replay = new ReplayService(markers, clock);
        if (input.Api == 25) return await Create(token, input, replay, ct);
        await repository.RequireOwnRegistrationAsync(token.AccountId, input.RegistrationId, input.Api is 31 or 32 ? input.TargetId : null, ct);
        int? target = input.Api is 30 or 32 ? input.Body.GetProperty("targetSessionId").GetInt32() : null;
        var resources = await repository.ResourcesAsync(input.RegistrationId, target, input.Api == 28, ct);
        var gates = new List<string> { "EC:REG:" + input.RegistrationId };
        if (input.Api != 36) gates.Add("EC:TIMETABLE");
        if (resources.Invoice is { } invoice) gates.Add("EC:INVOICE:" + invoice);
        gates.AddRange(resources.Classes.Select(x => "EC:CLASS:" + x));
        foreach (var gate in gates.Order(StringComparer.Ordinal)) await markers.AcquireAsync(gate, ct);
        await repository.PrepareAccountsAsync(token, resources.Graders, ct);
        await EnrollmentAuthority.RequireAsync(repository, token, clock, "Student", ct);
        await repository.RequireOwnRegistrationAsync(token.AccountId, input.RegistrationId, input.Api is 31 or 32 ? input.TargetId : null, ct);
        var state = await repository.LockStateAsync(input.RegistrationId, target is { } session ? [session] : [], false, ct, input.Api == 28);
        SqlEnrollmentRepository.RequireResources(state, resources, input.Api == 28);
        var repeated = await replay.BeginAsync(token.AccountId, input.RequestId, input.Intent, async (stored, c) =>
        {
            await EnrollmentAuthority.RequireAsync(repository, token, clock, "Student", c);
            await repository.RequireOwnRegistrationAsync(token.AccountId, stored.Target["registrationId"], null, c);
            return stored.Target["registrationId"] == input.RegistrationId && (input.Api is not (31 or 32) ||
                stored.Target[input.Api == 32 ? "oldBookingId" : "bookingId"] == input.TargetId);
        }, ct);
        if (repeated is not null) return await Result(token, state, input, repeated.OperationId, repeated.Target, true, ct);
        await repository.RequireUnfinishedAsync(state.Registration, ct);
        var now = clock.GetUtcNow(); var operation = Guid.NewGuid();
        var references = new Dictionary<string, int> { ["registrationId"] = state.Registration.RegistrationId };
        if (input.Api == 28) await CancelRegistration(token, state, input, operation, now, ct);
        else if (input.Api == 36) await Confirm(token, state, input, operation, now, ct);
        else await Booking(token, state, input, operation, now, references, ct);
        await replay.CompleteAsync(token.AccountId, input.RequestId, input.Intent, operation, references, ct);
        await new CurrentSessionService(repository, clock).TouchApprovedInteractiveActivityAsync(token, ct);
        // The membership at command time remains relevant after its RVs change.
        if (input.Api == 28)
        {
            var expected = input.Body.GetProperty("expectedSet").GetProperty("futureBookingIds").EnumerateArray().Select(x => x.GetInt32()).ToArray();
            if (expected.Any(id => state.Session(state.Booking(id).SessionId).StartAt <= clock.GetUtcNow().UtcDateTime)) throw AcademicGuard.Conflict();
        }
        if (input.Api is 30 or 31 or 32)
        {
            var sessions = target is { } id ? new[] { id } : Array.Empty<int>();
            if (input.Api is 31 or 32) sessions = sessions.Append(state.Booking(input.TargetId).SessionId).ToArray();
            if (sessions.Any(id => state.Session(id).Status != "OPEN" || state.Session(id).StartAt <= clock.GetUtcNow().UtcDateTime)) throw AcademicGuard.Conflict();
        }
        return await Result(token, state, input, operation, references, false, ct);
    }

    private async Task<object> Create(VerifiedSessionToken token, EnrollmentInput input, ReplayService replay, CancellationToken ct)
    {
        var course = input.Body.GetProperty("courseId").GetInt32();
        var student = await repository.OwnStudentAsync(token.AccountId, ct);
        var unfinished = await repository.UnfinishedAsync(student, course, ct);
        var stored = await markers.FindAsync(token.AccountId, input.Intent.Action.Code, input.RequestId, ct);
        int? replayRegistration = null;
        if (stored is not null)
        {
            try { replayRegistration = SafeCommandCompletion.Decode(input.Intent.Action, stored.OperationId, stored.ResultMode, stored.ResultReference).Target["registrationId"]; }
            catch (Exception ex) when (ex is ArgumentException or System.Text.Json.JsonException or InvalidOperationException or ApiErrorException)
            { throw new ApiErrorException(ApiErrorCode.INTERNAL_ERROR); }
        }
        foreach (var gate in unfinished.Concat(replayRegistration is { } existing ? [existing] : []).Distinct().Select(x => "EC:REG:" + x).Append("EC:TIMETABLE").Order(StringComparer.Ordinal)) await markers.AcquireAsync(gate, ct);
        await repository.PrepareAccountsAsync(token, [], ct);
        await EnrollmentAuthority.RequireAsync(repository, token, clock, "Student", ct);
        var repeated = await replay.BeginAsync(token.AccountId, input.RequestId, input.Intent, async (completion, c) =>
        { await repository.RequireOwnRegistrationAsync(token.AccountId, completion.Target["registrationId"], null, c); return true; }, ct);
        if (repeated is not null)
        {
            var state = await repository.LockStateAsync(repeated.Target["registrationId"], [], false, ct);
            return await Result(token, state, input, repeated.OperationId, repeated.Target, true, ct);
        }
        var created = await repository.CreateAsync(token.AccountId, course, clock.GetUtcNow(), unfinished, ct);
        var operation = Guid.NewGuid();
        await repository.AuditAsync(AuditEntry.Mandatory(input.Intent.Action.Code, token.AccountId, "Registration", created.Registration.RegistrationId,
            operation, clock.GetUtcNow(), new(null, Registration(created.Registration), new JsonObject { ["courseId"] = course })), ct);
        var references = new Dictionary<string, int> { ["registrationId"] = created.Registration.RegistrationId };
        await replay.CompleteAsync(token.AccountId, input.RequestId, input.Intent, operation, references, ct);
        await new CurrentSessionService(repository, clock).TouchApprovedInteractiveActivityAsync(token, ct);
        return await Result(token, created, input, operation, references, false, ct);
    }

    private async Task Booking(VerifiedSessionToken token, RegistrationState state, EnrollmentInput input, Guid operation, DateTimeOffset now, Dictionary<string, int> references, CancellationToken ct)
    {
        StudentEnrollmentManifest.Booking(state, input.Body, input.Api, input.TargetId);
        if (!EnrollmentProjection.PreConfirmation(state)) throw AcademicGuard.Conflict();
        var before = Registration(state.Registration);
        EntryTestBooking? old = input.Api == 30 ? null : state.Booking(input.TargetId);
        if (old is not null && (old.Status != "SCHEDULED" || state.Session(old.SessionId).Status != "OPEN" || state.Session(old.SessionId).StartAt <= now.UtcDateTime)) throw AcademicGuard.Conflict();
        var oldBooking = old is null ? null : BookingState(old);
        EntryTestAttempt? attempt = old is null ? null : state.Attempt(old.AttemptId);
        if (attempt is not null && (attempt.Score is not null || attempt.CompletedBookingId is not null)) throw AcademicGuard.Conflict();
        var attemptCreated = false;
        if (input.Api is 30 or 32)
        {
            var target = state.Session(input.Body.GetProperty("targetSessionId").GetInt32());
            if (old?.SessionId == target.SessionId) throw ApiErrorException.Validation("targetSessionId");
            await repository.RequireSeatAsync(state.Registration.StudentId, target, old?.BookingId, now, ct);
            if (input.Api == 30)
            {
                var context = EnrollmentProjection.BookAttempt(state) ?? throw AcademicGuard.Conflict();
                attempt = context.AttemptId is { } id ? state.Attempt(id) : new() { RegistrationId = state.Registration.RegistrationId, AttemptNumber = context.AttemptNumber };
                if (context.AttemptId is null) { repository.AddAttempt(attempt); await repository.SaveAsync(ct); attemptCreated = true; }
            }
        }
        if (old is not null) Cancel(old, token.AccountId, operation, now);
        EntryTestBooking? added = null;
        if (input.Api is 30 or 32)
        {
            added = new() { AttemptId = attempt!.AttemptId, SessionId = input.Body.GetProperty("targetSessionId").GetInt32(), Status = "SCHEDULED", BookedAt = now.UtcDateTime };
            repository.AddBooking(added);
            if (state.Registration.Status == "PENDING") state.Registration.Status = "TESTING";
        }
        await repository.SaveAsync(ct);
        var root = old ?? added!;
        var parent = new JsonObject { ["registrationId"] = state.Registration.RegistrationId, ["attemptId"] = attempt!.AttemptId };
        await repository.AuditAsync(AuditEntry.Mandatory(input.Intent.Action.Code, token.AccountId, "EntryTestBooking", root.BookingId, operation, now, new(oldBooking, BookingState(root), parent)), ct);
        if (added is not null && old is not null) await repository.AuditAsync(AuditEntry.Mandatory(input.Intent.Action.Code + ".EFFECT", token.AccountId, "EntryTestBooking", added.BookingId, operation, now, new(null, BookingState(added), parent)), ct);
        if (attemptCreated) await repository.AuditAsync(AuditEntry.Mandatory(input.Intent.Action.Code + ".EFFECT", token.AccountId, "EntryTestAttempt", attempt.AttemptId, operation, now, new(null, new JsonObject { ["attemptNumber"] = attempt.AttemptNumber }, parent)), ct);
        if (before["status"]!.GetValue<string>() != state.Registration.Status) await repository.AuditAsync(AuditEntry.Mandatory(input.Intent.Action.Code + ".EFFECT", token.AccountId, "Registration", state.Registration.RegistrationId, operation, now, new(before, Registration(state.Registration), parent)), ct);
        if (input.Api == 32) { references["oldBookingId"] = old!.BookingId; references["newBookingId"] = added!.BookingId; }
        else references["bookingId"] = root.BookingId;
    }

    private async Task CancelRegistration(VerifiedSessionToken token, RegistrationState state, EnrollmentInput input, Guid operation, DateTimeOffset now, CancellationToken ct)
    {
        StudentEnrollmentManifest.Cancellation(state, input.Body, now);
        if (!EnrollmentProjection.CanCancel(state)) throw AcademicGuard.Conflict();
        var before = Registration(state.Registration); var future = EnrollmentProjection.Future(state, now);
        state.Registration.CancelledFromStatus = state.Registration.Status;
        state.Registration.Status = "CANCELLED"; state.Registration.IsUnfinished = false;
        state.Registration.CancelledBy = token.AccountId; state.Registration.CancelledAt = now.UtcDateTime; state.Registration.CancellationOperationId = operation;
        var effects = future.Select(x => (Booking: x, Before: BookingState(x))).ToArray();
        foreach (var b in future) Cancel(b, token.AccountId, operation, now);
        var invoice = state.Invoice; JsonObject? oldInvoice = null;
        if (invoice?.Status == "UNPAID")
        {
            oldInvoice = new() { ["status"] = invoice.Status }; invoice.CancelledFromStatus = invoice.Status; invoice.Status = "CANCELLED";
            invoice.CancelledBy = token.AccountId; invoice.CancelledAt = now.UtcDateTime; invoice.CancellationOperationId = operation;
        }
        await repository.SaveAsync(ct);
        await repository.AuditAsync(AuditEntry.Mandatory(input.Intent.Action.Code, token.AccountId, "Registration", state.Registration.RegistrationId, operation, now, new(before, Registration(state.Registration))), ct);
        var parent = new JsonObject { ["registrationId"] = state.Registration.RegistrationId };
        foreach (var (b, previous) in effects) await repository.AuditAsync(AuditEntry.Mandatory(input.Intent.Action.Code + ".EFFECT", token.AccountId, "EntryTestBooking", b.BookingId, operation, now, new(previous, BookingState(b), parent)), ct);
        if (oldInvoice is not null) await repository.AuditAsync(AuditEntry.Mandatory(input.Intent.Action.Code + ".EFFECT", token.AccountId, "Invoice", invoice!.InvoiceId, operation, now, new(oldInvoice, new JsonObject { ["status"] = invoice.Status }, parent)), ct);
    }

    private async Task Confirm(VerifiedSessionToken token, RegistrationState state, EnrollmentInput input, Guid operation, DateTimeOffset now, CancellationToken ct)
    {
        StudentEnrollmentManifest.Confirmation(state, input.Body);
        if (!EnrollmentProjection.CanConfirm(state)) throw AcademicGuard.Conflict();
        var best = TeacherTaskProjection.BestCompleted(state);
        if (state.Band!.BestAttemptId != best.AttemptId || state.Band.BandId != await repository.BandForScoreAsync(best.Score!.Value, ct)) throw AcademicGuard.Conflict();
        var before = Registration(state.Registration); state.Registration.Status = "CONFIRMED"; state.Registration.AdmissionConfirmedAt = now.UtcDateTime;
        await repository.SaveAsync(ct);
        await repository.AuditAsync(AuditEntry.Mandatory(input.Intent.Action.Code, token.AccountId, "Registration", state.Registration.RegistrationId, operation, now, new(before, Registration(state.Registration))), ct);
    }

    private async Task<object> Result(VerifiedSessionToken token, RegistrationState state, EnrollmentInput input, Guid operation, IReadOnlyDictionary<string, int> references, bool replayed, CancellationToken ct)
    {
        // Reload the server's current projection; replay stores references only.
        var set = await repository.ReadSetAsync(state.Registration.StudentId, [state.Registration.RegistrationId], ct);
        state = new(set, set.Registrations.Single());
        var now = clock.GetUtcNow(); object result;
        if (input.Api == 32) result = new OwnRescheduleResult(EnrollmentProjection.Booking(state, state.Booking(references["oldBookingId"]), now), EnrollmentProjection.Booking(state, state.Booking(references["newBookingId"]), now), EnrollmentProjection.Entry(state, now), state.Registration.Status, EnrollmentProjection.Rv(state.Registration.RowVersion), operation);
        else if (input.Api is 30 or 31) result = new OwnBookingResult(EnrollmentProjection.Booking(state, state.Booking(references["bookingId"]), now), EnrollmentProjection.Entry(state, now), state.Registration.Status, EnrollmentProjection.Rv(state.Registration.RowVersion), operation) { Replayed = replayed };
        else result = new OwnRegistrationResult(EnrollmentProjection.Detail(state, now), operation) { Replayed = replayed };
        var authority = await EnrollmentAuthority.RequireAsync(repository, token, clock, "Student", ct);
        if (!clock.IsBefore(authority.EffectiveDeadline)) throw new ApiErrorException(ApiErrorCode.AUTHENTICATION_REQUIRED);
        if (!replayed)
        {
            var cutoff = clock.GetUtcNow().UtcDateTime;
            if (input.Api == 28 && input.Body.GetProperty("expectedSet").GetProperty("futureBookingIds").EnumerateArray()
                .Any(x => state.Session(state.Booking(x.GetInt32()).SessionId).StartAt <= cutoff)) throw AcademicGuard.Conflict();
            if (input.Api is 30 or 31 or 32)
            {
                var source = input.Api is 31 or 32 ? state.Session(state.Booking(input.TargetId).SessionId) : null;
                var target = input.Api is 30 or 32 ? state.Session(input.Body.GetProperty("targetSessionId").GetInt32()) : null;
                if (new[] { source, target }.Any(x => x is not null && (x.Status != "OPEN" || x.StartAt <= cutoff))) throw AcademicGuard.Conflict();
            }
        }
        return result;
    }

    private static void Cancel(EntryTestBooking b, int actor, Guid operation, DateTimeOffset now)
    { b.CancelledFromStatus = b.Status; b.Status = "CANCELLED"; b.CancelledBy = actor; b.CancelledAt = now.UtcDateTime; b.CancellationOperationId = operation; }
    private static JsonObject Registration(Registration r) => new() { ["status"] = r.Status, ["isUnfinished"] = r.IsUnfinished, ["admissionConfirmedAt"] = EnrollmentProjection.At(r.AdmissionConfirmedAt), ["cancelledAt"] = EnrollmentProjection.At(r.CancelledAt) };
    private static JsonObject BookingState(EntryTestBooking b) => new() { ["attemptId"] = b.AttemptId, ["sessionId"] = b.SessionId, ["status"] = b.Status, ["bookedAt"] = EnrollmentProjection.At(b.BookedAt), ["cancelledAt"] = EnrollmentProjection.At(b.CancelledAt), ["cancelledBy"] = b.CancelledBy, ["cancelledFromStatus"] = b.CancelledFromStatus, ["cancellationOperationId"] = b.CancellationOperationId?.ToString("D") };
}
