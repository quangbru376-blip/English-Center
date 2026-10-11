namespace EnglishCenter.Api.Infrastructure.Finance;

using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Common.Replay;
using EnglishCenter.Api.Common.Security;
using EnglishCenter.Api.Common.Time;
using EnglishCenter.Api.Infrastructure.Enrollment;
using EnglishCenter.Api.Infrastructure.Replay;
using EnglishCenter.Api.Services.Academics;
using EnglishCenter.Api.Services.Enrollment;
using EnglishCenter.Api.Services.Finance;
using EnglishCenter.Api.Services.Replay;

public sealed class SqlFinanceExecutor(Func<EnglishCenterDbContext> factory, BusinessClock clock, int sqlSeconds, int lockMs)
{
    private readonly SqlEnrollmentTransactions transactions = new(factory, clock, sqlSeconds, lockMs);
    public Task<AcademicPage<InvoiceAccountantView>> SearchAsync(VerifiedSessionToken token, FinanceSearchFilter filter, CancellationToken ct) =>
        transactions.Read(token, "Accountant", (db, c) => SqlFinanceReader.Search(db, filter, clock.GetToday(), c), ct);
    public Task<InvoiceAccountantView> DetailAsync(VerifiedSessionToken token, int id, CancellationToken ct) =>
        transactions.Read(token, "Accountant", (db, c) => SqlFinanceReader.Detail(db, id, clock.GetToday(), c), ct);
    public Task<CollectionPreview> PreviewAsync(VerifiedSessionToken token, int id, CancellationToken ct) => transactions.Read(token, "Accountant", async (db, c) =>
    {
        var now = clock.GetUtcNow(); var view = await SqlFinanceReader.Detail(db, id, clock.GetToday(), c); var eligible = view.AllowedActions.Contains("COLLECT");
        return new CollectionPreview(id, eligible, eligible ? [] : ["Chỉ thu hóa đơn chưa thanh toán của hồ sơ đã duyệt và chưa có Payment."], AcademicGuard.Instant(now.UtcDateTime), view.Amount,
            eligible ? new(view.Versions, new(view.RegistrationId, null)) : null);
    }, ct);
    public async Task<FinanceCommandOutcome> CommandAsync(VerifiedSessionToken token, int api, int id, Guid request, string json, int bytes, CancellationToken ct)
    {
        await transactions.Write(async (scope, c) =>
        {
            var repo = new SqlFinanceRepository(scope); await EnrollmentAuthority.RequireAsync(repo.Shared, token, clock, "Accountant", c);
            await repo.ResolveAsync(api, id, c); return true;
        }, ct);
        var input = FinanceInput.Parse(api, id, request, json, bytes);
        return await transactions.Write((scope, c) => new FinanceService(new(scope), new SqlReplayRepository(scope, lockMs), clock).CommandAsync(token, input, c), ct);
    }
    public Task<SafeCommandCompletion> LookupAsync(VerifiedSessionToken token, string action, Guid request, CancellationToken ct) => transactions.Write(async (scope, c) =>
    {
        var repo = new SqlFinanceRepository(scope);
        CanonicalAction selected; try { selected = CanonicalActionRegistry.ForCode(action); } catch (ArgumentException) { throw ApiErrorException.Validation("action"); }
        if (selected.ApiId is not (51 or 52)) throw ApiErrorException.Validation("action");
        var markers = new SqlReplayRepository(scope, lockMs); await markers.AcquireAsync(ReplayService.RequestResource(token.AccountId, selected, request), c);
        await repo.Shared.PrepareAccountsAsync(token, [], c); await EnrollmentAuthority.RequireAsync(repo.Shared, token, clock, "Accountant", c);
        var result = await new ReplayService(markers, clock).LookupAsync(53, "Accountant", token.AccountId, action, request,
            (stored, cancel) => repo.MatchesAsync(stored.Target["invoiceId"], stored.Target["paymentId"], cancel), c);
        var authority = await EnrollmentAuthority.RequireAsync(repo.Shared, token, clock, "Accountant", c);
        if (!clock.IsBefore(authority.EffectiveDeadline)) throw new ApiErrorException(ApiErrorCode.AUTHENTICATION_REQUIRED); return result;
    }, ct);
}
