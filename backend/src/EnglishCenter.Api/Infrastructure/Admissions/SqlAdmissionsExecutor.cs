namespace EnglishCenter.Api.Infrastructure.Admissions;

using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Common.Replay;
using EnglishCenter.Api.Common.Security;
using EnglishCenter.Api.Common.Time;
using EnglishCenter.Api.Infrastructure.Enrollment;
using EnglishCenter.Api.Infrastructure.Replay;
using EnglishCenter.Api.Services.Admissions;
using EnglishCenter.Api.Services.Enrollment;
using EnglishCenter.Api.Services.Replay;
using EnglishCenter.Api.Services.Academics;
using Microsoft.EntityFrameworkCore;

public sealed class SqlAdmissionsExecutor(Func<EnglishCenterDbContext> factory, BusinessClock clock, int sqlSeconds, int lockMs)
{
    private readonly SqlEnrollmentTransactions transactions = new(factory, clock, sqlSeconds, lockMs);
    public async Task<AdmissionsCommandResult> CommandAsync(VerifiedSessionToken token, int api, int id, Guid request, string json, int bytes, CancellationToken ct)
    {
        await transactions.Write(async (scope, c) =>
        {
            var repo = new SqlAdmissionsRepository(scope); await EnrollmentAuthority.RequireAsync(repo.Shared, token, clock, "Admissions", c);
            await repo.RequireAsync(id, c); return true;
        }, ct);
        var input = AdmissionsInput.Parse(api, id, request, json, bytes);
        return await transactions.Write((scope, c) => new AdmissionsService(new(scope), new SqlReplayRepository(scope, lockMs), clock).CommandAsync(token, input, c), ct);
    }
    private Task<T> Read<T>(VerifiedSessionToken token, Func<EnglishCenterDbContext, CancellationToken, Task<T>> work, CancellationToken ct) => transactions.Read(token, "Admissions", work, ct);
    private static async Task<RegistrationState> State(EnglishCenterDbContext db, int id, CancellationToken ct)
    {
        var student = await db.Registrations.AsNoTracking().Where(x => x.RegistrationId == id).Select(x => (int?)x.StudentId).SingleOrDefaultAsync(ct) ?? throw new ApiErrorException(ApiErrorCode.NOT_FOUND);
        var set = await SqlEnrollmentRepository.ReadSet(db, student, [id], ct); return new(set, set.Registrations.Single());
    }
    private static Task<bool> Payment(EnglishCenterDbContext db, int registration, CancellationToken ct) =>
        (from i in db.Invoices from p in db.Payments where i.RegistrationId == registration && p.InvoiceId == i.InvoiceId select p.PaymentId).AnyAsync(ct);
    public Task<AcademicPage<AdmissionsSummary>> SearchAsync(VerifiedSessionToken token, AdmissionsSearchFilter filter, CancellationToken ct) => Read(token, (db, c) => SqlAdmissionsSearch.Read(db, token.AccountId, filter, c), ct);
    public Task<AdmissionsDetail> DetailAsync(VerifiedSessionToken token, int id, CancellationToken ct) => Read(token, async (db, c) =>
        AdmissionsProjection.Detail(await State(db, id, c), clock.GetUtcNow(), await Payment(db, id, c)), ct);
    public Task<ApprovalPreview> ApprovalAsync(VerifiedSessionToken token, int id, CancellationToken ct) => Read(token, async (db, c) =>
    {
        var s = await State(db, id, c); var course = s.Set.Courses.Single(); var now = clock.GetUtcNow();
        var eligible = AdmissionsProjection.CanDecide(s) && course.BasePrice > 0 && !await Payment(db, id, c);
        if (eligible)
        {
            var best = TeacherTaskProjection.BestCompleted(s); var bands = await db.Database.SqlQuery<int>($"SELECT bandId AS Value FROM dbo.ufn_BandForScore({best.Score!.Value})").ToArrayAsync(c);
            eligible = bands.Length == 1 && bands[0] == s.Band!.BandId;
        }
        return new ApprovalPreview(id, eligible, eligible ? [] : ["Cần hồ sơ đã xác nhận, Band hợp lệ và chưa có hóa đơn."], AcademicGuard.Instant(now.UtcDateTime),
            new(course.CourseId, course.CourseCode, course.CourseName, course.Status, course.BasePrice.ToString("0", System.Globalization.CultureInfo.InvariantCulture), EnrollmentProjection.Rv(course.RowVersion)),
            EnrollmentProjection.Band(s)?.Band, eligible ? AdmissionsProjection.ApprovalManifest(s) : null);
    }, ct);
    public Task<AdmissionsCancellationPreview> CancellationAsync(VerifiedSessionToken token, int id, CancellationToken ct) => Read(token, async (db, c) =>
    {
        var s = await State(db, id, c); var now = clock.GetUtcNow(); var eligible = AdmissionsProjection.CanCancel(s, await Payment(db, id, c));
        return new AdmissionsCancellationPreview(id, eligible, eligible ? [] : ["Chỉ hủy hồ sơ đã xác nhận hoặc đã duyệt nhưng chưa thanh toán, chưa xếp lớp."], AcademicGuard.Instant(now.UtcDateTime),
            new { futureBookings = EnrollmentProjection.Future(s, now).Select(b => EnrollmentProjection.Booking(s, b, now)).ToArray(), invoice = AdmissionsProjection.Invoice(s) }, eligible ? EnrollmentProjection.CancellationManifest(s, now) : null);
    }, ct);
    public Task<SafeCommandCompletion> LookupAsync(VerifiedSessionToken token, string action, Guid request, CancellationToken ct) => transactions.Write(async (scope, c) =>
    {
        CanonicalAction selected; try { selected = CanonicalActionRegistry.ForCode(action); } catch (ArgumentException) { throw ApiErrorException.Validation("action"); }
        if (selected.ApiId is not (41 or 42 or 44)) throw new ApiErrorException(ApiErrorCode.FORBIDDEN);
        var markers = new SqlReplayRepository(scope, lockMs); await markers.AcquireAsync(ReplayService.RequestResource(token.AccountId, selected, request), c);
        var repo = new SqlAdmissionsRepository(scope); await repo.Shared.PrepareAccountsAsync(token, [], c); await EnrollmentAuthority.RequireAsync(repo.Shared, token, clock, "Admissions", c);
        var result = await new ReplayService(markers, clock).LookupAsync(45, "Admissions", token.AccountId, action, request, async (stored, cancel) =>
        {
            await repo.RequireAsync(stored.Target["registrationId"], cancel);
            return !stored.Target.TryGetValue("invoiceId", out var invoice) || await scope.Context.Invoices.AnyAsync(i => i.InvoiceId == invoice && i.RegistrationId == stored.Target["registrationId"], cancel);
        }, c);
        var authority = await EnrollmentAuthority.RequireAsync(repo.Shared, token, clock, "Admissions", c);
        if (!clock.IsBefore(authority.EffectiveDeadline)) throw new ApiErrorException(ApiErrorCode.AUTHENTICATION_REQUIRED); return result;
    }, ct);
}
