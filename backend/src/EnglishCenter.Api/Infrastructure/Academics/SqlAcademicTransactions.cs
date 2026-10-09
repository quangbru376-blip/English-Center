namespace EnglishCenter.Api.Infrastructure.Academics;

using System.Data;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Common.Security;
using EnglishCenter.Api.Common.Time;
using EnglishCenter.Api.Infrastructure.Identity;
using EnglishCenter.Api.Services.Academics;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

public sealed class SqlAcademicTransactions(Func<EnglishCenterDbContext> factory, BusinessClock clock, int sqlSeconds, int lockMs)
{
    public async Task<T> Write<T>(Func<SqlAuthDeadlineScope, CancellationToken, Task<T>> work, CancellationToken ct)
    {
        await using var db = factory(); if (db.Database.CurrentTransaction is not null) throw AcademicConflict();
        await db.Database.OpenConnectionAsync(ct); await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);
        await using var scope = await SqlAuthDeadlineScope.EnterAsync(db, sqlSeconds, lockMs, ct);
        try { var result = await work(scope, ct); await tx.CommitAsync(ct); return result; }
        catch (DbUpdateConcurrencyException) { throw new ApiErrorException(ApiErrorCode.CONFLICT); }
        catch (DbUpdateException e) when (e.InnerException is SqlException s && s.Number is 2601 or 2627) { throw new ApiErrorException(ApiErrorCode.CONFLICT); }
        catch (SqlException e) when (e.Number is 1205 or 1222) { throw new ApiErrorException(ApiErrorCode.SERVICE_UNAVAILABLE); }
        catch (DbUpdateException e) when (e.InnerException is SqlException s && s.Number is 1205 or 1222) { throw new ApiErrorException(ApiErrorCode.SERVICE_UNAVAILABLE); }
    }
    public async Task<T> Read<T>(VerifiedSessionToken token, Func<EnglishCenterDbContext, CancellationToken, Task<T>> read, CancellationToken ct)
    {
        await Write((s, c) => AcademicAuthority.RequireAsync(new SqlAcademicRepository(s), token, clock, c), ct); T result;
        await using (var db = factory()) { db.Database.SetCommandTimeout(sqlSeconds); await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Snapshot, ct); result = await read(db, ct); await tx.CommitAsync(ct); }
        await Write((s, c) => AcademicAuthority.RequireAsync(new SqlAcademicRepository(s), token, clock, c), ct); return result;
    }
    private static ApiErrorException AcademicConflict() => new(ApiErrorCode.STATE_CONFLICT);
}
