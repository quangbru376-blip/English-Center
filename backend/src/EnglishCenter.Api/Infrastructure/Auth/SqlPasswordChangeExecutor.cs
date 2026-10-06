namespace EnglishCenter.Api.Infrastructure.Auth;

using System.Data;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Common.Security;
using EnglishCenter.Api.Common.Time;
using EnglishCenter.Api.Infrastructure.Auditing;
using EnglishCenter.Api.Infrastructure.Identity;
using EnglishCenter.Api.Services.Auth;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

public sealed class SqlPasswordChangeExecutor : IPasswordChangeExecutor
{
    private readonly Func<EnglishCenterDbContext> _factory;
    private readonly CredentialHasher _hasher;
    private readonly BusinessClock _clock;
    private readonly int _seconds, _lockMs;
    public SqlPasswordChangeExecutor(Func<EnglishCenterDbContext> factory, CredentialHasher hasher, BusinessClock clock, int sqlSeconds, int lockMs)
    {
        ArgumentNullException.ThrowIfNull(factory); ArgumentNullException.ThrowIfNull(hasher); ArgumentNullException.ThrowIfNull(clock);
        if (sqlSeconds <= 0 || lockMs <= 0 || lockMs >= (long)sqlSeconds * 1000) throw new ArgumentException("Invalid auth deadline profile.");
        _factory = factory; _hasher = hasher; _clock = clock; _seconds = sqlSeconds; _lockMs = lockMs;
    }
    public async Task ExecuteAsync(VerifiedSessionToken token, PasswordChangeInput input, CancellationToken ct)
    {
        await using var context = _factory();
        if (context.Database.CurrentTransaction is not null) throw new ApiErrorException(ApiErrorCode.STATE_CONFLICT);
        await context.Database.OpenConnectionAsync(ct);
        await using var tx = await context.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);
        await using var scope = await SqlAuthDeadlineScope.EnterAsync(context, _seconds, _lockMs, ct);
        try
        {
            var deadline = await new PasswordChangeService(new SqlPasswordChangeRepository(scope, new()), _hasher, _clock).ChangeAsync(token, input, ct);
            if (!_clock.IsBefore(deadline)) throw new ApiErrorException(ApiErrorCode.AUTHENTICATION_REQUIRED);
            await tx.CommitAsync(ct);
        }
        catch (DbUpdateConcurrencyException) { throw new ApiErrorException(ApiErrorCode.CONFLICT); }
        catch (SqlException ex) when (ex.Number is 1205 or 1222) { throw new ApiErrorException(ApiErrorCode.CONFLICT); }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException sql && sql.Number is 1205 or 1222) { throw new ApiErrorException(ApiErrorCode.CONFLICT); }
    }
}
