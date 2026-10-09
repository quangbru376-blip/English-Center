namespace EnglishCenter.Api.Services.Academics;

using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Common.Serialization;
using EnglishCenter.Api.Common.Time;
using EnglishCenter.Api.Domain.Admissions;
using EnglishCenter.Api.Domain.Identity;
using System.Text.Json;

public sealed record SessionWindow(DateOnly Date, TimeOnly StartTime, TimeOnly EndTime, string Room)
{
    public DateTime StartAt => VietnamTime.ToUtc(Date, StartTime).UtcDateTime;
    public DateTime EndAt => VietnamTime.ToUtc(Date, EndTime).UtcDateTime;
    public static SessionWindow Parse(JsonElement body)
    { try { var result = new SessionWindow(StrictScalars.ParseLocalDate(body.GetProperty("date").GetString()), StrictScalars.ParseMinuteTime(body.GetProperty("startTime").GetString()), StrictScalars.ParseMinuteTime(body.GetProperty("endTime").GetString()), body.TryGetProperty("room", out var room) ? room.GetString()! : ""); if (result.StartTime >= result.EndTime) throw ApiErrorException.Validation("endTime"); _ = result.StartAt; return result; } catch (ArgumentException) { throw ApiErrorException.Validation(); } }
    public static SessionWindow From(EntryTestSession session)
    { var start = DateTime.SpecifyKind(session.StartAt, DateTimeKind.Utc).AddHours(7); var end = DateTime.SpecifyKind(session.EndAt, DateTimeKind.Utc).AddHours(7); return new(DateOnly.FromDateTime(start), TimeOnly.FromDateTime(start), TimeOnly.FromDateTime(end), session.Room); }
}
public sealed record GraderReference(int AccountId, string DisplayName);
public sealed record GraderOption(int AccountId, string DisplayName, bool Eligible, IReadOnlyList<string> BlockedReasons);
public sealed record GraderOptions(GraderOption? CurrentGrader, IReadOnlyList<GraderOption> Items, int Page, int PageSize, long TotalCount);
public sealed record EntryTaskCounts(int Scheduled, int Completed, int Absent, int Cancelled);
public record AcademicSessionView(int Id, string Date, string StartTime, string EndTime, string StartAt, string EndAt, string Room, int Capacity, string Status, string EffectiveStatus, GraderReference Grader, bool GraderAvailable, IReadOnlyList<string> AvailabilityReasons, int ScheduledBookingCount, int AvailableSlots, EntryTaskCounts TaskCounts, string Version);
public sealed record AcademicSessionDetail(int Id, string Date, string StartTime, string EndTime, string StartAt, string EndAt, string Room, int Capacity, string Status, string EffectiveStatus, GraderReference Grader, bool GraderAvailable, IReadOnlyList<string> AvailabilityReasons, int ScheduledBookingCount, int AvailableSlots, EntryTaskCounts TaskCounts, string Version, object? Cancellation, bool HasBookingHistory, IReadOnlyList<string> AllowedActions, IReadOnlyDictionary<string, string> DisabledReasons) : AcademicSessionView(Id, Date, StartTime, EndTime, StartAt, EndAt, Room, Capacity, Status, EffectiveStatus, Grader, GraderAvailable, AvailabilityReasons, ScheduledBookingCount, AvailableSlots, TaskCounts, Version);
public sealed record SessionCommandResult(AcademicSessionDetail Session, Guid OperationId)
{ [System.Text.Json.Serialization.JsonIgnore] public bool Replayed { get; init; } }
public sealed record SessionBookingBinding(int BookingId, int AttemptId, int RegistrationId);
public sealed record SessionState(EntryTestSession Session, Account Grader, bool TeacherRole, IReadOnlyList<EntryTestBooking> Bookings, IReadOnlyList<EntryTestAttempt> Attempts, IReadOnlyList<Registration> Registrations);
public sealed record SafeScheduleConflict(string Resource, string Message);
public sealed record SessionCancellationPreview(int SessionId, bool Eligible, IReadOnlyList<string> BlockedReasons, string AsOfUtc, IReadOnlyList<AcademicRosterRow> Effects, object? Manifest);
public sealed record GraderChangePreview(int SessionId, GraderReference OldGrader, GraderReference NewGrader, bool Eligible, IReadOnlyList<string> BlockedReasons, IReadOnlyList<SafeScheduleConflict> Conflicts, string AsOfUtc, object? Manifest);
public sealed record StudentContactReference(int StudentId, string FullName, string Email, string Phone);
public sealed record AcademicRosterRow(int BookingId, int AttemptId, int RegistrationId, int AttemptNumber, string BookedAt, string BookingStatus, string RegistrationStatus, StudentContactReference Student, AcademicCourse Course, string? TargetScore, string? TargetGradedAt, AcademicBand? CurrentBand, string? InvoiceStatus);
public static class SessionProjection
{
    public static SessionBookingBinding[] Bindings(SessionState state) => state.Bookings.Where(x => x.Status == "SCHEDULED").Select(x => new SessionBookingBinding(x.BookingId, x.AttemptId, state.Attempts.Single(a => a.AttemptId == x.AttemptId).RegistrationId)).OrderBy(x => x.BookingId).ToArray();
    public static object Manifest(SessionState state, Account? candidate = null)
    {
        var bindings = Bindings(state); var attemptIds = bindings.Select(x => x.AttemptId).ToHashSet(); var regIds = bindings.Select(x => x.RegistrationId).ToHashSet();
        var versions = new Dictionary<string, object> { { "session", RowVersionCodec.Encode(state.Session.RowVersion) }, { "bookings", state.Bookings.Where(x => x.Status == "SCHEDULED").Select(x => new VersionedId(x.BookingId, RowVersionCodec.Encode(x.RowVersion))).ToArray() }, { "attempts", state.Attempts.Where(x => attemptIds.Contains(x.AttemptId)).Select(x => new VersionedId(x.AttemptId, RowVersionCodec.Encode(x.RowVersion))).ToArray() }, { "registrations", state.Registrations.Where(x => regIds.Contains(x.RegistrationId)).Select(x => new VersionedId(x.RegistrationId, RowVersionCodec.Encode(x.RowVersion))).ToArray() } };
        var set = new Dictionary<string, object> { { "scheduledBookingIds", bindings.Select(x => x.BookingId).ToArray() }, { "bindings", bindings } };
        if (candidate is not null) { versions["grader"] = RowVersionCodec.Encode(candidate.RowVersion); set["currentGraderId"] = state.Session.GraderId; }
        return new { expectedVersions = versions, expectedSet = set };
    }
    public static bool HasResults(SessionState state) => state.Bookings.Any(x => x.Status is "ABSENT" or "COMPLETED") || state.Attempts.Any(x => x.CompletedBookingId is { } id && state.Bookings.Any(b => b.BookingId == id) && x.Score is not null);
    public static AcademicSessionDetail Detail(SessionState state, DateTimeOffset now, IReadOnlyList<SafeScheduleConflict> conflicts)
    {
        var s = state.Session; var window = SessionWindow.From(s); var future = s.StartAt > now.UtcDateTime; var cancelled = s.Status == "CANCELLED"; var available = state.Grader.Status == "ACTIVE" && state.TeacherRole && !conflicts.Any(x => x.Resource == "GRADER");
        var scheduled = state.Bookings.Count(x => x.Status == "SCHEDULED"); var actions = new Dictionary<string, bool> { { "EDIT_SCHEDULE", !cancelled && future && state.Bookings.Count == 0 }, { "OPEN", s.Status == "CLOSED" && future && available && conflicts.Count == 0 && scheduled <= s.Capacity }, { "CLOSE", s.Status == "OPEN" && future }, { "CANCEL", !cancelled && future }, { "CHANGE_GRADER", !cancelled && future && !HasResults(state) }, { "VIEW_ROSTER", true } };
        return new(s.SessionId, window.Date.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture), window.StartTime.ToString("HH:mm", System.Globalization.CultureInfo.InvariantCulture), window.EndTime.ToString("HH:mm", System.Globalization.CultureInfo.InvariantCulture), AcademicGuard.Instant(s.StartAt), AcademicGuard.Instant(s.EndAt), s.Room, s.Capacity, s.Status, cancelled ? "CANCELLED" : future ? s.Status : "CLOSED", new(s.GraderId, state.Grader.FullName), available, available ? [] : ["Giáo viên không hoạt động hoặc trùng lịch."], scheduled, Math.Max(0, s.Capacity - scheduled), new(scheduled, state.Bookings.Count(x => x.Status == "COMPLETED"), state.Bookings.Count(x => x.Status == "ABSENT"), state.Bookings.Count(x => x.Status == "CANCELLED")), RowVersionCodec.Encode(s.RowVersion), s.CancelledAt is { } at ? new { at = AcademicGuard.Instant(at), reason = s.CancellationReason } : null, state.Bookings.Count > 0, actions.Where(x => x.Value).Select(x => x.Key).ToArray(), actions.Where(x => !x.Value).ToDictionary(x => x.Key, _ => "Thời gian, trạng thái hoặc lịch sử không cho phép thao tác."));
    }
}
