namespace EnglishCenter.Api.Infrastructure.Finance;

using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Domain.Admissions;
using EnglishCenter.Api.Domain.Finance;
using EnglishCenter.Api.Infrastructure.Enrollment;
using EnglishCenter.Api.Infrastructure.Identity;
using EnglishCenter.Api.Services.Academics;
using Microsoft.EntityFrameworkCore;

public sealed record FinanceLink(int InvoiceId, int RegistrationId, int StudentAccountId);
public sealed record FinanceState(Registration Registration, Invoice Invoice, Payment? Payment);
public sealed class SqlFinanceRepository(SqlAuthDeadlineScope scope)
{
    public SqlEnrollmentRepository Shared { get; } = new(scope);
    private EnglishCenterDbContext Db => scope.Context;
    public async Task<FinanceLink> ResolveAsync(int api, int id, CancellationToken ct)
    {
        var invoice = api == 51 ? id : await Db.Payments.AsNoTracking().Where(p => p.PaymentId == id).Select(p => (int?)p.InvoiceId).SingleOrDefaultAsync(ct)
            ?? throw new ApiErrorException(ApiErrorCode.NOT_FOUND);
        return await (from i in Db.Invoices join r in Db.Registrations on i.RegistrationId equals r.RegistrationId
            join s in Db.StudentProfiles on r.StudentId equals s.StudentId where i.InvoiceId == invoice
            select new FinanceLink(i.InvoiceId, r.RegistrationId, s.AccountId)).SingleOrDefaultAsync(ct) ?? throw new ApiErrorException(ApiErrorCode.NOT_FOUND);
    }
    public async Task<FinanceState> LockAsync(FinanceLink link, int api, int id, CancellationToken ct)
    {
        var r = await Db.Registrations.FromSqlInterpolated($"SELECT * FROM dbo.Registration WITH(UPDLOCK,HOLDLOCK) WHERE registration_id={link.RegistrationId}").SingleAsync(ct);
        var i = await Db.Invoices.FromSqlInterpolated($"SELECT * FROM dbo.Invoice WITH(UPDLOCK,HOLDLOCK) WHERE invoice_id={link.InvoiceId}").SingleAsync(ct);
        var p = await Db.Payments.FromSqlInterpolated($"SELECT * FROM dbo.Payment WITH(UPDLOCK,HOLDLOCK) WHERE invoice_id={link.InvoiceId}").SingleOrDefaultAsync(ct);
        var current = await ResolveAsync(api, id, ct);
        if (current != link || i.RegistrationId != r.RegistrationId || api == 52 && p?.PaymentId != id) throw AcademicGuard.Conflict();
        return new(r, i, p);
    }
    public void Add(Payment payment) => Db.Payments.Add(payment);
    public Task<Services.Finance.InvoiceAccountantView> DetailAsync(int id, DateOnly today, CancellationToken ct) => SqlFinanceReader.Detail(Db, id, today, ct);
    public Task<bool> MatchesAsync(int invoice, int payment, CancellationToken ct) => Db.Payments.AsNoTracking().AnyAsync(p => p.PaymentId == payment && p.InvoiceId == invoice, ct);
}
