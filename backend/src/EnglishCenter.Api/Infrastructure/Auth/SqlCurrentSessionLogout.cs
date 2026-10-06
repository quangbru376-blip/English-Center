namespace EnglishCenter.Api.Infrastructure.Auth;

using System.Data;
using EnglishCenter.Api.Common.Auditing;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Common.Security;
using EnglishCenter.Api.Common.Time;
using EnglishCenter.Api.Infrastructure.Identity;
using EnglishCenter.Api.Services.Auth;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

public sealed class SqlCurrentSessionLogout : ICurrentSessionLogout
{
    private readonly Func<EnglishCenterDbContext> _factory;
    private readonly BusinessClock _clock;
    private readonly SecurityAuditRecorder _audit;
    private readonly int _seconds, _lockMs;
    public SqlCurrentSessionLogout(Func<EnglishCenterDbContext> factory, BusinessClock clock, SecurityAuditRecorder audit, int sqlSeconds, int lockMs)
    {
        ArgumentNullException.ThrowIfNull(factory); ArgumentNullException.ThrowIfNull(clock); ArgumentNullException.ThrowIfNull(audit);
        if (sqlSeconds <= 0 || lockMs <= 0 || lockMs >= (long)sqlSeconds * 1000) throw new ArgumentException("Invalid auth deadline profile.");
        _factory = factory; _clock = clock; _audit = audit; _seconds = sqlSeconds; _lockMs = lockMs;
    }
    public async Task ExecuteAsync(VerifiedSessionToken token, CancellationToken ct)
    {
        CurrentSessionRevocation proof;
        await using (var context = _factory())
        {
            if (context.Database.CurrentTransaction is not null) throw new ApiErrorException(ApiErrorCode.STATE_CONFLICT);
            await context.Database.OpenConnectionAsync(ct); await using var tx = await context.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);
            await using var scope = await SqlAuthDeadlineScope.EnterAsync(context, _seconds, _lockMs, ct);
            try
            {
                proof = await new CurrentSessionService(new SqlCurrentSessionRepository(scope), _clock).RevokeCurrentAsync(token, ct);
                if (!_clock.IsBefore(proof.ValidUntil)) throw new ApiErrorException(ApiErrorCode.AUTHENTICATION_REQUIRED);
                await tx.CommitAsync(ct);
            }
            catch (SqlException error) when (error.Number is 1205 or 1222) { throw new ApiErrorException(ApiErrorCode.CONFLICT); }
            catch (DbUpdateException error) when (error.InnerException is SqlException sql && sql.Number is 1205 or 1222) { throw new ApiErrorException(ApiErrorCode.CONFLICT); }
        }
        await LogoutAuditCompletion.AfterCommitAsync(proof.VerifiedActor, _audit, _clock, ct);
    }
}
