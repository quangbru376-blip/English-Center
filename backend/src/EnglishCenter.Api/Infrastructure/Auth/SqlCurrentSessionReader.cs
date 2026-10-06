namespace EnglishCenter.Api.Infrastructure.Auth;

using System.Data;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Common.Security;
using EnglishCenter.Api.Common.Time;
using EnglishCenter.Api.Infrastructure.Identity;
using EnglishCenter.Api.Services.Auth;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

public sealed class SqlCurrentSessionReader : ICurrentSessionReader
{
    private readonly Func<EnglishCenterDbContext> _factory;
    private readonly BusinessClock _clock;
    private readonly int _seconds, _lockMs;
    public SqlCurrentSessionReader(Func<EnglishCenterDbContext> factory, BusinessClock clock, int sqlSeconds, int lockMs)
    {
        ArgumentNullException.ThrowIfNull(factory); ArgumentNullException.ThrowIfNull(clock);
        if (sqlSeconds <= 0 || lockMs <= 0 || lockMs >= (long)sqlSeconds * 1000) throw new ArgumentException("Invalid auth deadline profile.");
        _factory = factory; _clock = clock; _seconds = sqlSeconds; _lockMs = lockMs;
    }
    public async Task<CurrentSessionSnapshot> ReadAsync(VerifiedSessionToken token, bool requiresNormal, CancellationToken ct)
    {
        await using var context = _factory(); if (context.Database.CurrentTransaction is not null) throw new ApiErrorException(ApiErrorCode.STATE_CONFLICT);
        await context.Database.OpenConnectionAsync(ct); await using var tx = await context.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);
        await using var scope = await SqlAuthDeadlineScope.EnterAsync(context, _seconds, _lockMs, ct);
        try
        {
            var snapshot = await new CurrentSessionService(new SqlCurrentSessionRepository(scope), _clock).ReadAsync(token, requiresNormal, ct);
            if (!_clock.IsBefore(snapshot.EffectiveDeadline)) throw new ApiErrorException(ApiErrorCode.AUTHENTICATION_REQUIRED);
            await tx.CommitAsync(ct);
            if (!_clock.IsBefore(snapshot.EffectiveDeadline)) throw new ApiErrorException(ApiErrorCode.AUTHENTICATION_REQUIRED);
            return snapshot;
        }
        catch (SqlException error) when (error.Number is 1205 or 1222) { throw new ApiErrorException(ApiErrorCode.CONFLICT); }
    }
}
