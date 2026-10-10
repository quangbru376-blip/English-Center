namespace EnglishCenter.Api.Services.Enrollment;

using System.Globalization;
using EnglishCenter.Api.Common.Serialization;
using EnglishCenter.Api.Domain.Admissions;
using EnglishCenter.Api.Services.Academics;
public static class EnrollmentProjection
{
    public static string Rv(byte[] v) => RowVersionCodec.Encode(v);
    public static string? At(DateTime? v) => v is { } time ? AcademicGuard.Instant(time) : null;
    public static OwnProfileView Profile(EnrollmentSet s) => new(s.Contact.AccountId, s.Student.StudentId, s.Contact.FullName, s.Contact.Email, s.Student.DateOfBirth.ToString("yyyy-MM-dd"), s.Contact.Phone ?? throw AcademicGuard.Conflict(), s.Student.Address, new { account = Rv(s.Contact.RowVersion), profile = Rv(s.Student.RowVersion) });
    public static OwnCourseReference Course(RegistrationState s) { var c = s.Set.Courses.Single(x => x.CourseId == s.Registration.CourseId); return new(c.CourseId, c.CourseCode, c.CourseName, c.Status); }
    public static OwnBandAssignment? Band(RegistrationState s) { if (s.Band is not { } a) return null; var b = s.Set.Bands.Single(x => x.BandId == a.BandId); return new(a.BandAssignmentId, a.BestAttemptId, new(b.BandId, b.BandCode), AcademicGuard.Instant(a.AssignedAt), Rv(a.RowVersion)); }
    public static OwnSessionView Session(EntryTestSession s, DateTimeOffset now) => new(s.SessionId, AcademicGuard.Instant(s.StartAt), AcademicGuard.Instant(s.EndAt), s.Room, s.Status, s.Status == "OPEN" && s.StartAt <= now.UtcDateTime ? "CLOSED" : s.Status, Rv(s.RowVersion));
    public static OwnBookingView Booking(RegistrationState s, EntryTestBooking b, DateTimeOffset now) { var a = s.Attempt(b.AttemptId); return new(b.BookingId, s.Registration.RegistrationId, a.AttemptId, b.SessionId, a.AttemptNumber, b.Status, AcademicGuard.Instant(b.BookedAt), Rv(b.RowVersion), Session(s.Session(b.SessionId), now)); }
    public static OwnEntryContext Entry(RegistrationState s, DateTimeOffset now) => new(s.Attempts.Select(a => new OwnAttemptView(a.AttemptId, a.AttemptNumber, Rv(a.RowVersion), a.Score?.ToString("0.00", CultureInfo.InvariantCulture), At(a.GradedAt), a.CompletedBookingId,
     s.Bookings.SingleOrDefault(b => b.AttemptId == a.AttemptId && b.Status == "SCHEDULED") is { } current ? Booking(s, current, now) : null,
     s.Bookings.Where(b => b.AttemptId == a.AttemptId).OrderByDescending(b => b.BookedAt).ThenByDescending(b => b.BookingId).FirstOrDefault() is { } last ? Booking(s, last, now) : null)).ToArray(), Band(s), s.Band?.BestAttemptId);
    public static bool PreConfirmation(RegistrationState s) => s.Registration.Status is "PENDING" or "TESTING" or "BAND_ASSIGNED";
    public static BookContext? BookAttempt(RegistrationState s)
    {
        if (!PreConfirmation(s) || s.Bookings.Any(x => x.Status == "SCHEDULED") || s.Attempts.Count > 2) return null;
        var first = s.Attempts.FirstOrDefault(); if (first is null) return new(1, null, null);
        if (first.AttemptNumber != 1 || s.Attempts.Skip(1).Any(x => x.AttemptNumber != 2)) return null;
        if (first.Score is null) return new(1, first.AttemptId, Rv(first.RowVersion));
        if (first.CompletedBookingId is not { } completed || !s.Bookings.Any(x => x.BookingId == completed && x.AttemptId == first.AttemptId && x.Status == "COMPLETED")) return null;
        var second = s.Attempts.Skip(1).FirstOrDefault(); return second is null ? new(2, null, null) : second.Score is null ? new(2, second.AttemptId, Rv(second.RowVersion)) : null;
    }
    public static bool CanConfirm(RegistrationState s)
    {
        if (s.Registration.Status != "BAND_ASSIGNED" || s.Band is null || s.Bookings.Any(x => x.Status == "SCHEDULED")) return false;
        var best = s.Attempts.Where(a => a.Score is not null && a.CompletedBookingId is { } id && s.Bookings.Any(b => b.BookingId == id && b.AttemptId == a.AttemptId && b.Status == "COMPLETED"))
         .OrderByDescending(a => a.Score).ThenBy(a => a.AttemptNumber).FirstOrDefault();
        return best is not null && best.AttemptId == s.Band.BestAttemptId && s.Set.Bands.Where(x => x.MinScore <= best.Score).OrderByDescending(x => x.MinScore).FirstOrDefault()?.BandId == s.Band.BandId;
    }
    public static BookContext? RescheduleAttempt(RegistrationState s, EntryTestBooking booking, DateTimeOffset now)
    {
        var attempt = s.Attempt(booking.AttemptId); var source = s.Session(booking.SessionId);
        if (!PreConfirmation(s) || booking.Status != "SCHEDULED" || source.Status != "OPEN" || source.StartAt <= now.UtcDateTime ||
         attempt.Score is not null || attempt.CompletedBookingId is not null || s.Attempts.Count > 2 || s.Bookings.Count(x => x.Status == "SCHEDULED") != 1) return null;
        if (attempt.AttemptNumber == 2 && !s.Attempts.Any(a => a.AttemptNumber == 1 && a.Score is not null && a.CompletedBookingId is { } id && s.Bookings.Any(b => b.BookingId == id && b.AttemptId == a.AttemptId && b.Status == "COMPLETED"))) return null;
        return new(attempt.AttemptNumber, attempt.AttemptId, Rv(attempt.RowVersion), booking.BookingId);
    }
    public static EntryTestBooking[] Future(RegistrationState s, DateTimeOffset now) => s.Bookings.Where(x => x.Status == "SCHEDULED" && s.Session(x.SessionId).StartAt > now.UtcDateTime).OrderBy(x => x.BookingId).ToArray();
    public static bool CanCancel(RegistrationState s) => PreConfirmation(s) && s.Invoice?.Status != "PAID" && !s.Set.Placements.Any(x => x.RegistrationId == s.Registration.RegistrationId);
    public static OwnInvoiceReference? Invoice(RegistrationState s, DateTimeOffset now) => s.Invoice is { } i ? new(i.InvoiceId, i.InvoiceNumber, i.Amount.ToString("0", CultureInfo.InvariantCulture), AcademicGuard.Instant(i.IssuedAt), i.DueDate.ToString("yyyy-MM-dd"), i.Status, i.Status == "UNPAID" && i.DueDate < DateOnly.FromDateTime(now.UtcDateTime.AddHours(7))) : null;
    public static OwnRegistrationSummary Summary(RegistrationState s) { var r = s.Registration; return new(r.RegistrationId, Course(s), r.Status, AcademicGuard.Instant(r.CreatedAt), At(r.AdmissionConfirmedAt), Band(s)?.Band, s.Invoice?.InvoiceId, s.Set.Placements.SingleOrDefault(x => x.RegistrationId == r.RegistrationId)?.ClassId, Rv(r.RowVersion)); }
    public static OwnRegistrationDetail Detail(RegistrationState s, DateTimeOffset now)
    {
        var r = s.Registration; var allowed = new List<string> { "VIEW_ENTRY_RESULTS" }; if (BookAttempt(s) is not null) allowed.Add("BOOK_ENTRY_TEST"); if (CanCancel(s)) allowed.Add("CANCEL_REGISTRATION"); if (CanConfirm(s)) allowed.Add("CONFIRM_REGISTRATION");
        if (PreConfirmation(s) && s.Bookings.Any(x => x.Status == "SCHEDULED" && s.Session(x.SessionId).Status == "OPEN" && s.Session(x.SessionId).StartAt > now.UtcDateTime)) { allowed.Add("CANCEL_ENTRY_BOOKING"); allowed.Add("RESCHEDULE_ENTRY_BOOKING"); }
        var p = s.Set.Placements.SingleOrDefault(x => x.RegistrationId == r.RegistrationId); object? cls = null; if (p is not null) { var c = s.Set.Classes.Single(x => x.ClassId == p.ClassId); cls = new { classId = c.ClassId, assignmentId = p.ClassAssignmentId, code = c.ClassCode, status = c.Status }; allowed.Add("VIEW_CLASS"); }
        if (s.Invoice is not null) allowed.Add("VIEW_INVOICE");
        if (p is not null && s.Set.Reports.Any(x => x.ClassAssignmentId == p.ClassAssignmentId && x.Status == "FINALIZED")) allowed.Add("VIEW_FINALIZED_REPORT");
        var disabled = new[] { "VIEW_ENTRY_RESULTS", "BOOK_ENTRY_TEST", "CANCEL_ENTRY_BOOKING", "RESCHEDULE_ENTRY_BOOKING", "CANCEL_REGISTRATION", "CONFIRM_REGISTRATION", "VIEW_INVOICE", "VIEW_CLASS", "VIEW_FINALIZED_REPORT" }.Except(allowed).ToDictionary(x => x, _ => "Trạng thái hồ sơ, kết quả hoặc thời điểm hiện tại chưa cho phép.");
        return new(r.RegistrationId, Course(s), r.Status, AcademicGuard.Instant(r.CreatedAt), At(r.AdmissionConfirmedAt), Band(s)?.Band, s.Invoice?.InvoiceId, p?.ClassId, Rv(r.RowVersion), Entry(s, now), Invoice(s, now), cls, r.RejectedAt is { } reject ? new { at = AcademicGuard.Instant(reject), reason = r.RejectionReason } : null, r.CancelledAt is { } cancel ? new { at = AcademicGuard.Instant(cancel), reason = r.CancellationReason } : null, At(r.AdmissionApprovedAt), allowed, disabled);
    }
    public static object CancellationManifest(RegistrationState s, DateTimeOffset now)
    { var future = Future(s, now); return new { expectedVersions = new { registration = Rv(s.Registration.RowVersion), bookings = future.Select(x => new VersionedId(x.BookingId, Rv(x.RowVersion))).ToArray(), sessions = future.Select(x => s.Session(x.SessionId)).DistinctBy(x => x.SessionId).OrderBy(x => x.SessionId).Select(x => new VersionedId(x.SessionId, Rv(x.RowVersion))).ToArray(), invoice = s.Invoice is { } i ? new VersionedId(i.InvoiceId, Rv(i.RowVersion)) : null }, expectedSet = new { futureBookingIds = future.Select(x => x.BookingId).ToArray(), invoiceId = s.Invoice?.InvoiceId } }; }
    public static object ConfirmationManifest(RegistrationState s) => new { expectedVersions = new { registration = Rv(s.Registration.RowVersion), bandAssignment = Rv(s.Band!.RowVersion), attempts = s.Attempts.Select(x => new VersionedId(x.AttemptId, Rv(x.RowVersion))).ToArray(), bookings = s.Bookings.Where(x => x.Status == "SCHEDULED").OrderBy(x => x.BookingId).Select(x => new VersionedId(x.BookingId, Rv(x.RowVersion))).ToArray() }, expectedSet = new { bandAssignmentId = s.Band.BandAssignmentId, bestAttemptId = s.Band.BestAttemptId, attemptIds = s.Attempts.Select(x => x.AttemptId).Order().ToArray(), scheduledBookingIds = s.Bookings.Where(x => x.Status == "SCHEDULED").Select(x => x.BookingId).Order().ToArray() } };
}
