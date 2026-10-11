namespace EnglishCenter.Api.Infrastructure.Finance;

using System.Globalization;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Services.Academics;
using EnglishCenter.Api.Services.Enrollment;
using EnglishCenter.Api.Services.Finance;
using Microsoft.EntityFrameworkCore;

internal static class SqlFinanceReader
{
    private sealed class Row
    {
        public int Id { get; set; } public int RegistrationId { get; set; } public int StudentId { get; set; }
        public int CourseId { get; set; } public string Code { get; set; } = ""; public string Name { get; set; } = ""; public string CourseStatus { get; set; } = "";
        public string Number { get; set; } = ""; public decimal Amount { get; set; } public DateTime IssuedAt { get; set; } public DateOnly DueDate { get; set; }
        public string Status { get; set; } = ""; public bool IsOverdue { get; set; } public int OverdueDays { get; set; }
        public string FullName { get; set; } = ""; public string Email { get; set; } = ""; public string Phone { get; set; } = "";
        public string RegistrationStatus { get; set; } = ""; public byte[] RegistrationVersion { get; set; } = []; public byte[] InvoiceVersion { get; set; } = [];
        public int? PaymentId { get; set; } public decimal? PaidAmount { get; set; } public DateTime? PaidAt { get; set; }
        public string? Method { get; set; } public string? TransactionReference { get; set; } public int? ConfirmedBy { get; set; } public string? ConfirmerName { get; set; }
        public int? LastCorrectedBy { get; set; } public string? CorrectorName { get; set; } public DateTime? LastCorrectedAt { get; set; } public string? LastCorrectionReason { get; set; }
        public byte[]? PaymentVersion { get; set; }
    }
    private static IQueryable<Row> Rows(EnglishCenterDbContext db, DateOnly today) => db.Database.SqlQuery<Row>($"""
        SELECT f.invoiceId AS Id,f.registrationId AS RegistrationId,f.studentId AS StudentId,f.courseId AS CourseId,
            f.courseCode AS Code,f.courseName AS Name,f.courseStatus AS CourseStatus,f.invoiceNumber AS Number,
            f.amount AS Amount,f.issuedAt AS IssuedAt,f.dueDate AS DueDate,f.invoiceStatus AS Status,
            d.isOverdue AS IsOverdue,d.daysOverdue AS OverdueDays,a.full_name AS FullName,a.email AS Email,a.phone AS Phone,
            r.status AS RegistrationStatus,r.row_version AS RegistrationVersion,i.row_version AS InvoiceVersion,
            f.paymentId AS PaymentId,f.paidAmount AS PaidAmount,f.paidAt AS PaidAt,p.method AS Method,p.transaction_reference AS TransactionReference,
            p.confirmed_by AS ConfirmedBy,confirmer.full_name AS ConfirmerName,p.last_corrected_by AS LastCorrectedBy,
            corrector.full_name AS CorrectorName,p.last_corrected_at AS LastCorrectedAt,p.last_correction_reason AS LastCorrectionReason,p.row_version AS PaymentVersion
        FROM dbo.vw_FinanceInvoiceFacts f JOIN dbo.Invoice i ON i.invoice_id=f.invoiceId
        JOIN dbo.Registration r ON r.registration_id=f.registrationId JOIN dbo.StudentProfile s ON s.student_id=f.studentId
        JOIN dbo.Account a ON a.account_id=s.account_id LEFT JOIN dbo.Payment p ON p.payment_id=f.paymentId
        LEFT JOIN dbo.Account confirmer ON confirmer.account_id=p.confirmed_by LEFT JOIN dbo.Account corrector ON corrector.account_id=p.last_corrected_by
        CROSS APPLY dbo.ufn_InvoiceDebtFlags(f.invoiceStatus,f.dueDate,{today}) d
        """);
    private static InvoiceAccountantView Project(Row r)
    {
        if ((r.Status == "PAID") != r.PaymentId.HasValue || r.PaymentId.HasValue && r.PaidAmount != r.Amount) throw new ApiErrorException(ApiErrorCode.INTERNAL_ERROR);
        var payment = r.PaymentId is { } pid ? new FinancePaymentView(pid, r.Id, Money(r.PaidAmount!.Value), r.Method!, r.TransactionReference,
            new(r.ConfirmedBy!.Value, r.ConfirmerName!), AcademicGuard.Instant(r.PaidAt!.Value), r.LastCorrectedBy is { } actor ?
                new(new(actor, r.CorrectorName!), AcademicGuard.Instant(r.LastCorrectedAt!.Value), r.LastCorrectionReason!) : null, EnrollmentProjection.Rv(r.PaymentVersion!)) : null;
        string[] allowed = r.Status == "UNPAID" && r.RegistrationStatus == "ADMITTED" && payment is null ? ["COLLECT"] : r.Status == "PAID" && r.RegistrationStatus == "ADMITTED" && payment is not null ? ["CORRECT_PAYMENT_METADATA"] : [];
        return new(r.Id, r.RegistrationId, new(r.CourseId, r.Code, r.Name, r.CourseStatus), r.Number, Money(r.Amount), AcademicGuard.Instant(r.IssuedAt),
            r.DueDate.ToString("yyyy-MM-dd"), r.Status, r.IsOverdue, r.OverdueDays, new(r.StudentId, r.FullName, r.Email, r.Phone), r.RegistrationStatus, payment,
            new(EnrollmentProjection.Rv(r.RegistrationVersion), EnrollmentProjection.Rv(r.InvoiceVersion)), allowed,
            new[] { "COLLECT", "CORRECT_PAYMENT_METADATA" }.Except(allowed).ToDictionary(x => x, _ => "Trạng thái hồ sơ, hóa đơn hoặc thanh toán chưa cho phép."));
    }
    private static string Money(decimal value) => value.ToString("0", CultureInfo.InvariantCulture);
    public static async Task<InvoiceAccountantView> Detail(EnglishCenterDbContext db, int id, DateOnly today, CancellationToken ct) =>
        Project(await Rows(db, today).SingleOrDefaultAsync(x => x.Id == id, ct) ?? throw new ApiErrorException(ApiErrorCode.NOT_FOUND));
    public static async Task<AcademicPage<InvoiceAccountantView>> Search(EnglishCenterDbContext db, FinanceSearchFilter f, DateOnly today, CancellationToken ct)
    {
        var q = Rows(db, today);
        if (f.InvoiceNumber is { } number) q = q.Where(x => x.Number.StartsWith(number));
        if (f.Name is { } name) q = q.Where(x => x.FullName.Contains(name));
        if (f.Email is { } email) q = q.Where(x => x.Email.Contains(email));
        if (f.Phone is { } phone) q = q.Where(x => x.Phone == phone);
        if (f.CourseId is { } course) q = q.Where(x => x.CourseId == course);
        if (f.Status is { } status) q = q.Where(x => x.Status == status);
        if (f.FromUtc is { } from) q = q.Where(x => x.IssuedAt >= from);
        if (f.ToExclusiveUtc is { } to) q = q.Where(x => x.IssuedAt < to);
        if (f.OverdueOnly) q = q.Where(x => x.IsOverdue);
        var count = await q.LongCountAsync(ct); var offset = (long)(f.Page - 1) * f.PageSize;
        var ordered = f.Sort == "ISSUED_ASC" ? q.OrderBy(x => x.IssuedAt).ThenBy(x => x.Id) : q.OrderByDescending(x => x.IssuedAt).ThenByDescending(x => x.Id);
        var items = offset > int.MaxValue ? [] : await ordered.Skip((int)offset).Take(f.PageSize).ToArrayAsync(ct);
        return new(items.Select(Project).ToArray(), f.Page, f.PageSize, count);
    }
}
