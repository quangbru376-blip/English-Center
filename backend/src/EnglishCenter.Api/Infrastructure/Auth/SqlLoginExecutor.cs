namespace EnglishCenter.Api.Infrastructure.Auth;

using System.Data;
using System.Security.Cryptography;
using EnglishCenter.Api.Common.Auditing;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Common.Security;
using EnglishCenter.Api.Common.Time;
using EnglishCenter.Api.Infrastructure.Identity;
using EnglishCenter.Api.Services.Auth;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

public sealed class SqlLoginExecutor : ILoginExecutor
{
    private readonly Func<EnglishCenterDbContext> _factory;
    private readonly CredentialHasher _hasher;
    private readonly BusinessClock _clock;
    private readonly IJwtSessionIssuer _signer;
    private readonly SecurityAuditRecorder _audit;
    private readonly int _seconds, _lockMs;
    private readonly string _dummyHash;
    public SqlLoginExecutor(Func<EnglishCenterDbContext> factory, CredentialHasher hasher, BusinessClock clock, IJwtSessionIssuer signer, SecurityAuditRecorder audit, int sqlSeconds, int lockMs)
    {
        ArgumentNullException.ThrowIfNull(factory); ArgumentNullException.ThrowIfNull(hasher); ArgumentNullException.ThrowIfNull(clock); ArgumentNullException.ThrowIfNull(signer); ArgumentNullException.ThrowIfNull(audit);
        if (sqlSeconds <= 0 || lockMs <= 0 || lockMs >= (long)sqlSeconds * 1000) throw new ArgumentException("Invalid auth deadline profile.");
        _factory = factory; _hasher = hasher; _clock = clock; _signer = signer; _audit = audit; _seconds = sqlSeconds; _lockMs = lockMs;
        _dummyHash = hasher.Hash(Base64UrlEncoder.Encode(RandomNumberGenerator.GetBytes(32)));
    }
    public async Task<LoginDecision> ExecuteAsync(LoginInput input, CancellationToken cancellationToken)
    {
        LoginDecision decision;
        await using (var context = _factory())
        {
            if (context.Database.CurrentTransaction is not null) throw new ApiErrorException(ApiErrorCode.STATE_CONFLICT);
            await context.Database.OpenConnectionAsync(cancellationToken);
            await using var tx = await context.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
            await using var scope = await SqlAuthDeadlineScope.EnterAsync(context, _seconds, _lockMs, cancellationToken);
            try
            {
                decision = await new LoginService(new SqlLoginRepository(scope), _hasher, _clock, _signer, _dummyHash).LoginAsync(input, cancellationToken);
                if (decision.Grant is LoginGrant grant && !_clock.IsBefore(grant.CookieExpiresAt)) throw new ApiErrorException(ApiErrorCode.AUTHENTICATION_FAILED);
                await tx.CommitAsync(cancellationToken);
            }
            catch (SqlException e) when (e.Number is 1205 or 1222) { throw new ApiErrorException(ApiErrorCode.CONFLICT); }
            catch (DbUpdateException e) when (e.InnerException is SqlException sql && sql.Number is 1205 or 1222) { throw new ApiErrorException(ApiErrorCode.CONFLICT); }
        }
        // Pure auth history is independent, even when bad-password counters committed.
        return await LoginAuditCompletion.AfterCommitAsync(decision, _audit, _clock, cancellationToken);
    }
}
