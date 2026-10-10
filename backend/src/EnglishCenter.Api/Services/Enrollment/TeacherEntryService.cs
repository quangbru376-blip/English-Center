namespace EnglishCenter.Api.Services.Enrollment;

using System.Globalization;
using System.Text.Json.Nodes;
using EnglishCenter.Api.Common.Auditing;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Common.Security;
using EnglishCenter.Api.Common.Time;
using EnglishCenter.Api.Domain.Admissions;
using EnglishCenter.Api.Infrastructure.Enrollment;
using EnglishCenter.Api.Services.Academics;
using EnglishCenter.Api.Services.Auth;
using EnglishCenter.Api.Services.Replay;

public sealed class TeacherEntryService(SqlEnrollmentRepository repository, IReplayRepository markers, BusinessClock clock)
{
    public async Task<TeacherCommandResult> CommandAsync(VerifiedSessionToken token, EnrollmentInput input, CancellationToken ct)
    {
        var action = input.Intent.Action;
        if (input.Api is not (70 or 71 or 72)) throw new ApiErrorException(ApiErrorCode.FORBIDDEN);
        await markers.AcquireAsync(ReplayService.RequestResource(token.AccountId, action, input.RequestId), ct);
        var target = await repository.RequireTeacherAsync(token.AccountId, input.TargetId, input.Api == 72, ct);
        foreach (var gate in new[] { "EC:BANDSET", "EC:REG:" + target.Registration }.Order(StringComparer.Ordinal)) await markers.AcquireAsync(gate, ct);
        var studentAccount = await repository.StudentAccountAsync(target.Registration, ct);
        await repository.PrepareAccountsAsync(token, [studentAccount], ct);
        await EnrollmentAuthority.RequireAsync(repository, token, clock, "Teacher", ct);
        var refreshed = await repository.RequireTeacherAsync(token.AccountId, input.TargetId, input.Api == 72, ct);
        if (refreshed != target) throw AcademicGuard.Conflict();
        var state = await repository.LockStateAsync(target.Registration, [], input.Api != 71, ct);
        var booking = state.Booking(target.Booking); TeacherTaskProjection.RequireAssignment(state, booking, token.AccountId);
        var replay = new ReplayService(markers, clock);
        var repeated = await replay.BeginAsync(token.AccountId, input.RequestId, input.Intent, async (stored, c) =>
        {
            await EnrollmentAuthority.RequireAsync(repository, token, clock, "Teacher", c);
            if (stored.Target["registrationId"] != state.Registration.RegistrationId || stored.Target["bookingId"] != booking.BookingId || stored.Target["attemptId"] != booking.AttemptId) return false;
            await repository.RequireTeacherAsync(token.AccountId, input.TargetId, input.Api == 72, c); return true;
        }, ct);
        if (repeated is not null)
        {
            var projection = TeacherTaskProjection.Detail(state, booking, token.AccountId, clock.GetUtcNow());
            var replayAuthority = await EnrollmentAuthority.RequireAsync(repository, token, clock, "Teacher", ct);
            if (!clock.IsBefore(replayAuthority.EffectiveDeadline)) throw new ApiErrorException(ApiErrorCode.AUTHENTICATION_REQUIRED);
            return new(projection, repeated.OperationId);
        }

        TeacherTaskProjection.CheckManifest(state, booking, input.Body, input.Api);
        RequireWindow(state, booking, token.AccountId, input.Api);
        var attempt = state.Attempt(booking.AttemptId); var now = clock.GetUtcNow(); var operation = Guid.NewGuid();
        var parent = new JsonObject { ["registrationId"] = state.Registration.RegistrationId, ["attemptId"] = attempt.AttemptId, ["bookingId"] = booking.BookingId };
        var oldAttempt = Attempt(attempt); var oldBooking = Booking(booking); var oldReg = Registration(state.Registration);
        var oldBand = state.Band is { } band ? Band(band) : null;
        var reason = input.Api == 72 ? input.Body.GetProperty("reason").GetString() : null;
        if (input.Api == 71)
        {
            booking.Status = "ABSENT"; booking.AbsentBy = token.AccountId; booking.AbsentAt = now.UtcDateTime; booking.AbsentOperationId = operation;
            await repository.SaveAsync(ct);
            await repository.AuditAsync(AuditEntry.Mandatory(action.Code, token.AccountId, "EntryTestBooking", booking.BookingId, operation, now, new(oldBooking, Booking(booking))), ct);
        }
        else
        {
            var score = decimal.Parse(input.Body.GetProperty("score").GetString()!, CultureInfo.InvariantCulture);
            if (input.Api == 72 && score == attempt.Score) throw ApiErrorException.Validation("score");
            attempt.Score = score;
            if (input.Api == 70) { attempt.GradedBy = token.AccountId; attempt.GradedAt = now.UtcDateTime; attempt.CompletedBookingId = booking.BookingId; booking.Status = "COMPLETED"; }
            var best = TeacherTaskProjection.BestCompleted(state); var bandId = await repository.BandForScoreAsync(best.Score!.Value, ct);
            var assigned = state.Band;
            if (assigned is null) { assigned = new() { RegistrationId = state.Registration.RegistrationId }; repository.AddBandAssignment(assigned); }
            assigned.BestAttemptId = best.AttemptId; assigned.BandId = bandId; assigned.AssignedAt = now.UtcDateTime;
            state.Registration.Status = "BAND_ASSIGNED";
            await repository.SaveAsync(ct);
            await repository.AuditAsync(AuditEntry.Mandatory(action.Code, token.AccountId, "EntryTestAttempt", attempt.AttemptId, operation, now, new(oldAttempt, Attempt(attempt), Reason: reason)), ct);
            if (input.Api == 70) await repository.AuditAsync(AuditEntry.Mandatory(action.Code + ".EFFECT", token.AccountId, "EntryTestBooking", booking.BookingId, operation, now, new(oldBooking, Booking(booking), parent)), ct);
            await repository.AuditAsync(AuditEntry.Mandatory(action.Code + ".EFFECT", token.AccountId, "BandAssignment", assigned.BandAssignmentId, operation, now, new(oldBand, Band(assigned), parent, reason)), ct);
            await repository.AuditAsync(AuditEntry.Mandatory(action.Code + ".EFFECT", token.AccountId, "Registration", state.Registration.RegistrationId, operation, now, new(oldReg, Registration(state.Registration), parent, reason)), ct);
        }
        await replay.CompleteAsync(token.AccountId, input.RequestId, input.Intent, operation,
            new Dictionary<string, int> { ["bookingId"] = booking.BookingId, ["attemptId"] = attempt.AttemptId, ["registrationId"] = state.Registration.RegistrationId }, ct);
        await new CurrentSessionService(repository, clock).TouchApprovedInteractiveActivityAsync(token, ct);
        await EnrollmentAuthority.RequireAsync(repository, token, clock, "Teacher", ct);
        await repository.RequireTeacherAsync(token.AccountId, input.TargetId, input.Api == 72, ct);
        if (state.Session(booking.SessionId).Status == "CANCELLED" || state.Session(booking.SessionId).EndAt > clock.GetUtcNow().UtcDateTime) throw AcademicGuard.Conflict();
        var final = await repository.LockStateAsync(target.Registration, [], input.Api != 71, ct);
        var projected = TeacherTaskProjection.Detail(final, final.Booking(target.Booking), token.AccountId, clock.GetUtcNow());
        var authority = await EnrollmentAuthority.RequireAsync(repository, token, clock, "Teacher", ct);
        if (!clock.IsBefore(authority.EffectiveDeadline)) throw new ApiErrorException(ApiErrorCode.AUTHENTICATION_REQUIRED);
        if (final.Session(booking.SessionId).EndAt > clock.GetUtcNow().UtcDateTime) throw AcademicGuard.Conflict();
        return new(projected, operation);
    }

    private void RequireWindow(RegistrationState state, EntryTestBooking booking, int actor, int api)
    {
        if (!(api == 72 ? TeacherTaskProjection.CanCorrect(state, booking, actor, clock.GetUtcNow()) : TeacherTaskProjection.CanInitial(state, booking, clock.GetUtcNow()))) throw AcademicGuard.Conflict();
    }
    private static JsonObject Attempt(EntryTestAttempt a) => new() { ["attemptNumber"] = a.AttemptNumber, ["score"] = a.Score, ["gradedBy"] = a.GradedBy, ["gradedAt"] = EnrollmentProjection.At(a.GradedAt), ["completedBookingId"] = a.CompletedBookingId };
    private static JsonObject Booking(EntryTestBooking b) => new() { ["status"] = b.Status, ["absentBy"] = b.AbsentBy, ["absentAt"] = EnrollmentProjection.At(b.AbsentAt), ["absentOperationId"] = b.AbsentOperationId?.ToString("D") };
    private static JsonObject Band(BandAssignment b) => new() { ["bestAttemptId"] = b.BestAttemptId, ["bandId"] = b.BandId };
    private static JsonObject Registration(Registration r) => new() { ["status"] = r.Status, ["isUnfinished"] = r.IsUnfinished };
}
