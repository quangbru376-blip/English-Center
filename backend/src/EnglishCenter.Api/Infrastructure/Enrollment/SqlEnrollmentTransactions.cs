namespace EnglishCenter.Api.Infrastructure.Enrollment;

using System.Data;
using EnglishCenter.Api.Common.Security;
using EnglishCenter.Api.Common.Time;
using EnglishCenter.Api.Infrastructure.Academics;
using EnglishCenter.Api.Infrastructure.Identity;
using EnglishCenter.Api.Services.Enrollment;
using Microsoft.EntityFrameworkCore;

public sealed class SqlEnrollmentTransactions(Func<EnglishCenterDbContext> factory, BusinessClock clock, int sqlSeconds, int lockMs)
{
    private readonly SqlAcademicTransactions writes = new(factory, clock, sqlSeconds, lockMs);
    public Task<T> Write<T>(Func<SqlAuthDeadlineScope, CancellationToken, Task<T>> work, CancellationToken ct) => writes.Write(work, ct);
    public async Task<T> Read<T>(VerifiedSessionToken token, string role, Func<EnglishCenterDbContext, CancellationToken, Task<T>> read, CancellationToken ct)
    {
        await Write(async (s, c) =>
        {
            var authority = await EnrollmentAuthority.RequireAsync(new SqlEnrollmentRepository(s), token, clock, role, c);
            if (!clock.IsBefore(authority.EffectiveDeadline)) throw new Common.Errors.ApiErrorException(Common.Errors.ApiErrorCode.AUTHENTICATION_REQUIRED);
            return authority;
        }, ct);
        T result;
        await using (var db = factory())
        {
            db.Database.SetCommandTimeout(sqlSeconds);
            await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Snapshot, ct);
            result = await read(db, ct); await tx.CommitAsync(ct);
        }
        await Write(async (s, c) =>
        {
            var authority = await EnrollmentAuthority.RequireAsync(new SqlEnrollmentRepository(s), token, clock, role, c);
            if (!clock.IsBefore(authority.EffectiveDeadline)) throw new Common.Errors.ApiErrorException(Common.Errors.ApiErrorCode.AUTHENTICATION_REQUIRED);
            return authority;
        }, ct);
        return result;
    }
}
