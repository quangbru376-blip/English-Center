namespace EnglishCenter.Api.Infrastructure.StudentInvoices;

using System.Globalization;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Common.Security;
using EnglishCenter.Api.Common.Time;
using EnglishCenter.Api.Infrastructure.Enrollment;
using EnglishCenter.Api.Services.Academics;
using EnglishCenter.Api.Services.StudentInvoices;
using Microsoft.EntityFrameworkCore;

public sealed class SqlStudentInvoiceReader(Func<EnglishCenterDbContext> factory, BusinessClock clock, int sqlSeconds, int lockMs)
{
    private readonly SqlEnrollmentTransactions transactions = new(factory, clock, sqlSeconds, lockMs);
    // Database-only row; API projections never expose persistence or Payment data.
    private sealed class Row
    {
        public int Id { get; set; }
        public int RegistrationId { get; set; }
        public int CourseId { get; set; }
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
        public string CourseStatus { get; set; } = "";
        public string Number { get; set; } = "";
        public decimal Amount { get; set; }
        public DateTime IssuedAt { get; set; }
        public DateOnly DueDate { get; set; }
        public string Status { get; set; } = "";
        public bool IsOverdue { get; set; }
        public int OverdueDays { get; set; }
        public string CurrentStudentName { get; set; } = "";
    }
    private static IQueryable<Row> Rows(EnglishCenterDbContext db, int actor, DateOnly today) => db.Database.SqlQuery<Row>($"""
        SELECT i.invoice_id AS Id,i.registration_id AS RegistrationId,c.course_id AS CourseId,
            c.course_code AS Code,c.course_name AS Name,c.status AS CourseStatus,
            i.invoice_number AS Number,i.amount AS Amount,i.issued_at AS IssuedAt,i.due_date AS DueDate,
            i.status AS Status,d.isOverdue AS IsOverdue,d.daysOverdue AS OverdueDays,a.full_name AS CurrentStudentName
        FROM dbo.Invoice i JOIN dbo.Registration r ON r.registration_id=i.registration_id
        JOIN dbo.StudentProfile p ON p.student_id=r.student_id JOIN dbo.Account a ON a.account_id=p.account_id
        JOIN dbo.Course c ON c.course_id=r.course_id CROSS APPLY dbo.ufn_InvoiceDebtFlags(i.status,i.due_date,{today}) d
        WHERE a.account_id={actor}
        """);
    private static StudentInvoiceView Project(Row r) => new(r.Id, r.RegistrationId, new(r.CourseId, r.Code, r.Name, r.CourseStatus), r.Number,
        r.Amount.ToString("0", CultureInfo.InvariantCulture), AcademicGuard.Instant(r.IssuedAt), r.DueDate.ToString("yyyy-MM-dd"), r.Status, r.IsOverdue, r.OverdueDays);
    public Task<AcademicPage<StudentInvoiceView>> SearchAsync(VerifiedSessionToken token, IQueryCollection query, CancellationToken ct)
    {
        var q = new AcademicQuery(query, "registrationId", "status", "page", "pageSize"); var registration = q.Id("registrationId"); var status = q.Get("status");
        if (status is not null) q.Choice("status", "UNPAID", "UNPAID", "PAID", "CANCELLED"); var page = q.Number("page"); var size = q.Number("pageSize", 20, 100);
        return transactions.Read(token, "Student", async (db, c) =>
        {
            if (registration is { } id && !await (from r in db.Registrations from p in db.StudentProfiles where r.RegistrationId == id && p.StudentId == r.StudentId && p.AccountId == token.AccountId select r.RegistrationId).AnyAsync(c)) throw new ApiErrorException(ApiErrorCode.NOT_FOUND);
            var rows = Rows(db, token.AccountId, clock.GetToday()); if (registration is { } reg) rows = rows.Where(x => x.RegistrationId == reg); if (status is not null) rows = rows.Where(x => x.Status == status);
            var count = await rows.LongCountAsync(c); var offset = (long)(page - 1) * size;
            var items = offset > int.MaxValue ? [] : await rows.OrderByDescending(x => x.IssuedAt).ThenByDescending(x => x.Id).Skip((int)offset).Take(size).ToArrayAsync(c);
            return new AcademicPage<StudentInvoiceView>(items.Select(Project).ToArray(), page, size, count);
        }, ct);
    }
    public Task<StudentInvoiceDetail> DetailAsync(VerifiedSessionToken token, int id, CancellationToken ct) => transactions.Read(token, "Student", async (db, c) =>
    {
        var row = await Rows(db, token.AccountId, clock.GetToday()).SingleOrDefaultAsync(x => x.Id == id, c) ?? throw new ApiErrorException(ApiErrorCode.NOT_FOUND);
        var v = Project(row); return new StudentInvoiceDetail(v.Id, v.RegistrationId, v.Course, v.Number, v.Amount, v.IssuedAt, v.DueDate, v.Status, v.IsOverdue, v.OverdueDays,
            row.CurrentStudentName, new("Liên hệ trung tâm để thanh toán đủ học phí bằng tiền mặt hoặc chuyển khoản bên ngoài ứng dụng."));
    }, ct);
}
