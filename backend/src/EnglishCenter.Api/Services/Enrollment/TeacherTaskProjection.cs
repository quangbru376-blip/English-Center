namespace EnglishCenter.Api.Services.Enrollment;

using System.Text.Json;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Services.Academics;
using EnglishCenter.Api.Domain.Admissions;

public static class TeacherTaskProjection
{
    // Assignment is checked before exposing the task, even for completed history.
    public static void RequireAssignment(RegistrationState state, EntryTestBooking booking, int actor)
    { if (state.Session(booking.SessionId).GraderId != actor) throw new ApiErrorException(ApiErrorCode.NOT_FOUND); }

    public static bool CanInitial(RegistrationState state, EntryTestBooking booking, DateTimeOffset now) =>
        EnrollmentProjection.PreConfirmation(state) && state.Session(booking.SessionId).Status != "CANCELLED" &&
        state.Session(booking.SessionId).EndAt <= now.UtcDateTime && booking.Status == "SCHEDULED" &&
        state.Attempt(booking.AttemptId).Score is null && state.Attempt(booking.AttemptId).CompletedBookingId is null;

    public static bool CanCorrect(RegistrationState state, EntryTestBooking booking, int actor, DateTimeOffset now)
    {
        var attempt = state.Attempt(booking.AttemptId);
        return state.Registration.Status is "TESTING" or "BAND_ASSIGNED" && state.Band is not null &&
            state.Session(booking.SessionId).Status != "CANCELLED" && state.Session(booking.SessionId).EndAt <= now.UtcDateTime &&
            booking.Status == "COMPLETED" && attempt.CompletedBookingId == booking.BookingId && attempt.Score is not null &&
            attempt.GradedBy == actor && state.Session(booking.SessionId).GraderId == actor;
    }

    public static TeacherTaskDetail Detail(RegistrationState state, EntryTestBooking booking, int actor, DateTimeOffset now)
    {
        RequireAssignment(state, booking, actor);
        var attempt = state.Attempt(booking.AttemptId);
        var initial = CanInitial(state, booking, now);
        var correction = CanCorrect(state, booking, actor, now);
        var actions = new List<string>(); if (initial) actions.AddRange(["GRADE", "MARK_ABSENT"]); if (correction) actions.Add("CORRECT_SCORE");
        var contact = state.Set.Contact;
        return new(EnrollmentProjection.Booking(state, booking, now),
            new(state.Set.Student.StudentId, contact.FullName, contact.Email, contact.Phone ?? throw AcademicGuard.Conflict()),
            EnrollmentProjection.Course(state), state.Registration.Status, EnrollmentProjection.Rv(state.Registration.RowVersion),
            state.Attempts.Select(a => new RelatedAttempt(a.AttemptId, a.AttemptNumber, EnrollmentProjection.Rv(a.RowVersion),
                a.Score?.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture), EnrollmentProjection.At(a.GradedAt), a.CompletedBookingId)).ToArray(),
            EnrollmentProjection.Band(state),
            booking.Status == "COMPLETED" && attempt.CompletedBookingId == booking.BookingId && attempt.Score is { } score && attempt.GradedAt is { } at && attempt.GradedBy is { } grader
                ? new { score = score.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture), gradedAt = AcademicGuard.Instant(at), originalGraderId = grader } : null,
            booking.Status == "ABSENT" && booking.AbsentAt is { } absent && booking.AbsentBy is { } by ? new { at = AcademicGuard.Instant(absent), actorId = by } : null,
            booking.CancelledAt is { } cancelled ? new { at = AcademicGuard.Instant(cancelled), reason = booking.CancellationReason } : null,
            actions, new[] { "GRADE", "MARK_ABSENT", "CORRECT_SCORE" }.Except(actions).ToDictionary(x => x, _ => "Trạng thái, phân công hoặc thời điểm hiện tại chưa cho phép."),
            initial ? Manifest(state, booking, 70) : null, initial ? Manifest(state, booking, 71) : null, correction ? Manifest(state, booking, 72) : null);
    }

    public static object Manifest(RegistrationState state, EntryTestBooking booking, int api)
    {
        var versions = new Dictionary<string, object?>
        {
            ["session"] = EnrollmentProjection.Rv(state.Session(booking.SessionId).RowVersion),
            ["registration"] = EnrollmentProjection.Rv(state.Registration.RowVersion),
            ["attempt"] = EnrollmentProjection.Rv(state.Attempt(booking.AttemptId).RowVersion),
            ["booking"] = EnrollmentProjection.Rv(booking.RowVersion),
            ["relatedAttempts"] = state.Attempts.Select(a => new VersionedId(a.AttemptId, EnrollmentProjection.Rv(a.RowVersion))).ToArray()
        };
        var set = new Dictionary<string, object?>
        {
            ["attemptIds"] = state.Attempts.Select(a => a.AttemptId).Order().ToArray(),
            ["currentBookingId"] = api == 72 ? null : booking.BookingId,
            ["completedBookingId"] = api == 72 ? booking.BookingId : null
        };
        if (api != 71) { versions["bandAssignment"] = state.Band is { } b ? new VersionedId(b.BandAssignmentId, EnrollmentProjection.Rv(b.RowVersion)) : null; set["bandAssignmentId"] = state.Band?.BandAssignmentId; }
        return new { expectedVersions = versions, expectedSet = set };
    }

    public static void CheckManifest(RegistrationState state, EntryTestBooking booking, JsonElement body, int api)
    {
        var v = body.GetProperty("expectedVersions"); var set = body.GetProperty("expectedSet"); var a = state.Attempt(booking.AttemptId);
        AcademicGuard.Version(v.GetProperty("session"), state.Session(booking.SessionId).RowVersion);
        AcademicGuard.Version(v.GetProperty("registration"), state.Registration.RowVersion);
        AcademicGuard.Version(v.GetProperty("attempt"), a.RowVersion); AcademicGuard.Version(v.GetProperty("booking"), booking.RowVersion);
        AcademicGuard.Set(set.GetProperty("attemptIds"), state.Attempts.Select(x => x.AttemptId));
        AcademicGuard.Versions(v.GetProperty("relatedAttempts"), state.Attempts.Select(x => new VersionedId(x.AttemptId, EnrollmentProjection.Rv(x.RowVersion))));
        int? Id(JsonElement value) => value.ValueKind == JsonValueKind.Null ? null : value.GetInt32();
        if (Id(set.GetProperty("currentBookingId")) != (api == 72 ? null : booking.BookingId) || Id(set.GetProperty("completedBookingId")) != (api == 72 ? booking.BookingId : null)) throw AcademicGuard.Conflict();
        if (api == 71) return;
        var band = v.GetProperty("bandAssignment");
        if (Id(set.GetProperty("bandAssignmentId")) != state.Band?.BandAssignmentId || (band.ValueKind == JsonValueKind.Null) != (state.Band is null)) throw AcademicGuard.Conflict();
        if (state.Band is { } current) { if (band.GetProperty("id").GetInt32() != current.BandAssignmentId) throw AcademicGuard.Conflict(); AcademicGuard.Version(band.GetProperty("version"), current.RowVersion); }
    }

    public static EntryTestAttempt BestCompleted(RegistrationState state) => state.Attempts
        .Where(a => a.Score is not null && a.CompletedBookingId is { } id && state.Bookings.Any(b => b.BookingId == id && b.AttemptId == a.AttemptId && b.Status == "COMPLETED"))
        .OrderByDescending(a => a.Score).ThenBy(a => a.AttemptNumber).FirstOrDefault() ?? throw AcademicGuard.Conflict();
}
