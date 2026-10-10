namespace EnglishCenter.Api.Services.Admissions;

using System.Globalization;
using System.Text.Json;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Common.Replay;
using EnglishCenter.Api.Services.Academics;
using EnglishCenter.Api.Services.Enrollment;

public sealed record AdmissionsContact(int StudentId, string FullName, string Email, string Phone);
public sealed record AdmissionsInvoice(int Id, string Number, string Amount, string Status);
public sealed record AdmissionsClass(int ClassId, int AssignmentId, string Code, string Status);
public sealed record AdmissionsSummary(int Id, AdmissionsContact Student, OwnCourseReference Course, string Status,
    string CreatedAt, string? AdmissionConfirmedAt, string? AdmissionApprovedAt, OwnBandReference? Band,
    AdmissionsInvoice? Invoice, AdmissionsClass? Class, string Version);
public sealed record AdmissionsDetail(int Id, AdmissionsContact Student, OwnCourseReference Course, string Status,
    string CreatedAt, string? AdmissionConfirmedAt, string? AdmissionApprovedAt, OwnBandReference? Band,
    AdmissionsInvoice? Invoice, AdmissionsClass? Class, string Version, OwnEntryContext Entry,
    object? Rejection, object? Cancellation, IReadOnlyList<string> AllowedActions, IReadOnlyDictionary<string, string> DisabledReasons);
public sealed record AdmissionsCommandResult(AdmissionsDetail Registration, Guid OperationId);
public sealed record ApprovalCourse(int Id, string Code, string Name, string Status, string Tuition, string Version);
public sealed record ApprovalPreview(int RegistrationId, bool Eligible, IReadOnlyList<string> BlockedReasons,
    string AsOfUtc, ApprovalCourse Course, OwnBandReference? Band, object? Manifest);
public sealed record AdmissionsCancellationPreview(int RegistrationId, bool Eligible, IReadOnlyList<string> BlockedReasons,
    string AsOfUtc, object Effects, object? Manifest);

public sealed class AdmissionsInput
{
    public int Api { get; }
    public int RegistrationId { get; }
    public Guid RequestId { get; }
    public CanonicalIntent Intent { get; }
    internal JsonElement Body { get; }
    private AdmissionsInput(int api, int id, Guid request, CanonicalIntent intent)
    {
        Api = api; RegistrationId = id; RequestId = request; Intent = intent;
        using var doc = JsonDocument.Parse(intent.CopyBytes()); Body = doc.RootElement.GetProperty("body").Clone();
    }
    public static AdmissionsInput Parse(int api, int id, Guid request, string json, int bytes)
    {
        if (api is not (41 or 42 or 44) || request == Guid.Empty) throw ApiErrorException.Validation();
        return new(api, id, request, CanonicalIntent.Bind(api, json, new Dictionary<string, int> { ["registrationId"] = id }, bytes));
    }
    public override string ToString() => nameof(AdmissionsInput);
}

public static class AdmissionsProjection
{
    public static bool ValidBand(RegistrationState s)
    {
        if (s.Band is null || s.Bookings.Any(b => b.Status == "SCHEDULED")) return false;
        var best = s.Attempts.Where(a => a.Score is not null && a.CompletedBookingId is { } id &&
            s.Bookings.Any(b => b.BookingId == id && b.AttemptId == a.AttemptId && b.Status == "COMPLETED"))
            .OrderByDescending(a => a.Score).ThenBy(a => a.AttemptNumber).FirstOrDefault();
        return best is not null && best.AttemptId == s.Band.BestAttemptId &&
            s.Set.Bands.Any(b => b.BandId == s.Band.BandId && b.MinScore <= best.Score);
    }
    public static bool CanDecide(RegistrationState s) => s.Registration.Status == "CONFIRMED" && ValidBand(s) && s.Invoice is null && s.Set.Placements.Count == 0;
    public static bool CanCancel(RegistrationState s, bool payment) => !payment && s.Set.Placements.Count == 0 &&
        (s.Registration.Status == "CONFIRMED" && s.Invoice is null || s.Registration.Status == "ADMITTED" && s.Invoice?.Status == "UNPAID");
    public static AdmissionsInvoice? Invoice(RegistrationState s) => s.Invoice is { } i ? new(i.InvoiceId, i.InvoiceNumber, i.Amount.ToString("0", CultureInfo.InvariantCulture), i.Status) : null;
    public static AdmissionsSummary Summary(RegistrationState s)
    {
        var r = s.Registration; var p = s.Set.Placements.SingleOrDefault(x => x.RegistrationId == r.RegistrationId);
        var cls = p is null ? null : s.Set.Classes.Single(x => x.ClassId == p.ClassId);
        return new(r.RegistrationId, new(s.Set.Student.StudentId, s.Set.Contact.FullName, s.Set.Contact.Email, s.Set.Contact.Phone!),
            EnrollmentProjection.Course(s), r.Status, AcademicGuard.Instant(r.CreatedAt), EnrollmentProjection.At(r.AdmissionConfirmedAt),
            EnrollmentProjection.At(r.AdmissionApprovedAt), EnrollmentProjection.Band(s)?.Band, Invoice(s),
            p is null ? null : new(p.ClassId, p.ClassAssignmentId, cls!.ClassCode, cls.Status), EnrollmentProjection.Rv(r.RowVersion));
    }
    public static AdmissionsDetail Detail(RegistrationState s, DateTimeOffset now, bool payment)
    {
        var v = Summary(s); var r = s.Registration; var allowed = new List<string>();
        if (CanDecide(s)) allowed.AddRange(["APPROVE", "REJECT"]);
        if (CanCancel(s, payment)) allowed.Add("CANCEL");
        return new(v.Id, v.Student, v.Course, v.Status, v.CreatedAt, v.AdmissionConfirmedAt, v.AdmissionApprovedAt, v.Band, v.Invoice, v.Class, v.Version,
            EnrollmentProjection.Entry(s, now), r.RejectedAt is { } rejected ? new { at = AcademicGuard.Instant(rejected), reason = r.RejectionReason } : null,
            r.CancelledAt is { } cancelled ? new { at = AcademicGuard.Instant(cancelled), reason = r.CancellationReason } : null, allowed,
            new[] { "APPROVE", "REJECT", "CANCEL" }.Except(allowed).ToDictionary(x => x, _ => "Trạng thái, kết quả, hóa đơn hoặc lớp chưa cho phép."));
    }
    public static object ApprovalManifest(RegistrationState s)
    {
        var c = s.Set.Courses.Single(); return new { expectedVersions = new { registration = EnrollmentProjection.Rv(s.Registration.RowVersion), course = EnrollmentProjection.Rv(c.RowVersion), bandAssignment = EnrollmentProjection.Rv(s.Band!.RowVersion) },
            expectedSet = new { courseId = c.CourseId, bandAssignmentId = s.Band.BandAssignmentId, invoiceId = (int?)null } };
    }
}
