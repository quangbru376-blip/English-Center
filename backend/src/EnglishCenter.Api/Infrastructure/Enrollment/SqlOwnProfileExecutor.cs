namespace EnglishCenter.Api.Infrastructure.Enrollment;

using EnglishCenter.Api.Common.Auditing;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Common.Security;
using EnglishCenter.Api.Common.Time;
using EnglishCenter.Api.Services.Academics;
using EnglishCenter.Api.Services.Auth;
using EnglishCenter.Api.Services.Enrollment;
using Microsoft.EntityFrameworkCore;

public sealed class SqlOwnProfileExecutor(Func<EnglishCenterDbContext> factory, BusinessClock clock, int sqlSeconds, int lockMs)
{
    private readonly SqlEnrollmentTransactions transactions = new(factory, clock, sqlSeconds, lockMs);
    public Task<OwnProfileView> ReadAsync(VerifiedSessionToken token, CancellationToken ct) => transactions.Read(token, "Student", async (db, c) =>
    {
        var student = await db.StudentProfiles.AsNoTracking().Where(x => x.AccountId == token.AccountId).Select(x => (int?)x.StudentId).SingleOrDefaultAsync(c) ?? throw new ApiErrorException(ApiErrorCode.NOT_FOUND);
        return EnrollmentProjection.Profile(await SqlEnrollmentRepository.ReadSet(db, student, [], c));
    }, ct);

    public Task<OwnProfileView> UpdateAsync(VerifiedSessionToken token, string json, CancellationToken ct) => transactions.Write(async (scope, c) =>
    {
        var repository = new SqlEnrollmentRepository(scope);
        await repository.PrepareAccountsAsync(token, [], c);
        await EnrollmentAuthority.RequireAsync(repository, token, clock, "Student", c);
        var studentId = await repository.OwnStudentAsync(token.AccountId, c);
        // Scope is established before interpreting client business versions.
        var input = OwnProfileInput.Parse(json);
        if (input.DateOfBirth > clock.GetToday()) throw ApiErrorException.Validation("dateOfBirth");
        var account = await scope.Context.Accounts.SingleAsync(x => x.AccountId == token.AccountId, c);
        var profile = await scope.Context.StudentProfiles.FromSqlInterpolated($"SELECT * FROM dbo.StudentProfile WITH(UPDLOCK,HOLDLOCK) WHERE student_id={studentId}").SingleAsync(c);
        if (profile.AccountId != token.AccountId || !input.CopyAccountVersion().AsSpan().SequenceEqual(account.RowVersion) || !input.CopyProfileVersion().AsSpan().SequenceEqual(profile.RowVersion)) throw AcademicGuard.Conflict();
        var changed = new List<string>();
        if (account.FullName != input.FullName) changed.Add("fullName"); if (account.Phone != input.Phone) changed.Add("phone");
        if (profile.DateOfBirth != input.DateOfBirth) changed.Add("dateOfBirth"); if (profile.Address != input.Address) changed.Add("address");
        account.FullName = input.FullName; account.Phone = input.Phone; profile.DateOfBirth = input.DateOfBirth; profile.Address = input.Address;
        var now = clock.GetUtcNow(); var operation = Guid.NewGuid();
        await repository.SaveAsync(c);
        await repository.AuditAsync(AuditEntry.OwnProfileChange(token.AccountId, studentId, changed, operation, now), c);
        await new CurrentSessionService(repository, clock).TouchApprovedInteractiveActivityAsync(token, c);
        await EnrollmentAuthority.RequireAsync(repository, token, clock, "Student", c);
        // No security-version increment, session revocation, or replay completion.
        return new OwnProfileView(account.AccountId, profile.StudentId, account.FullName, account.Email, profile.DateOfBirth.ToString("yyyy-MM-dd"), account.Phone!, profile.Address,
            new { account = EnrollmentProjection.Rv(account.RowVersion), profile = EnrollmentProjection.Rv(profile.RowVersion) });
    }, ct);
}
