namespace EnglishCenter.Api.Services.Auth;

using System.Globalization;
using System.Security.Cryptography;
using EnglishCenter.Api.Common.Security;
using EnglishCenter.Api.Common.Serialization;
using EnglishCenter.Api.Common.Time;
using EnglishCenter.Api.Domain.Identity;
using Microsoft.AspNetCore.Identity;

public interface ILoginRepository
{
    Task<Account?> LockAccountAsync(string email, CancellationToken cancellationToken);
    Task<string?> RoleCodeAsync(int roleId, CancellationToken cancellationToken);
    Task SaveAccountAsync(CancellationToken cancellationToken);
    Task InsertSessionAsync(AuthSession session, CancellationToken cancellationToken);
}
public sealed class LoginService(ILoginRepository repository, CredentialHasher hasher, BusinessClock clock, IJwtSessionIssuer signer, string dummyHash)
{
    public async Task<LoginDecision> LoginAsync(LoginInput input, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input); cancellationToken.ThrowIfCancellationRequested();
        var account = await repository.LockAccountAsync(input.Email, cancellationToken);
        var verified = hasher.Verify(account?.PasswordHash ?? dummyHash, input.Password);
        if (account is null) return LoginDecision.Rejected();
        var now = clock.GetUtcNow(); ResetExpiredCounters(account);
        var blocked = account.LoginThrottledUntil is DateTime until && clock.IsBefore(Utc(until));
        if (account.Status != "ACTIVE" || blocked) { await repository.SaveAccountAsync(cancellationToken); return LoginDecision.Rejected(); }
        if (verified == PasswordVerificationResult.Failed)
        {
            account.FailureWindowStartedAt ??= now.UtcDateTime; account.FailedLoginCount++;
            if (account.FailedLoginCount >= 5) account.LoginThrottledUntil = now.AddMinutes(15).UtcDateTime;
            await repository.SaveAccountAsync(cancellationToken); return LoginDecision.Rejected();
        }
        if (account.MustChangePassword && (account.TempExpiresAt is null || !clock.IsBefore(Utc(account.TempExpiresAt.Value)))) return await RejectAsync(cancellationToken);
        var role = await repository.RoleCodeAsync(account.RoleId, cancellationToken);
        if (role is null || !FixedRoleCodes.All.Contains(role, StringComparer.Ordinal)) return await RejectAsync(cancellationToken);
        if (verified == PasswordVerificationResult.SuccessRehashNeeded) account.PasswordHash = hasher.RehashVerified(account.PasswordHash, input.Password);
        now = clock.GetUtcNow();
        if (account.MustChangePassword && !clock.IsBefore(Utc(account.TempExpiresAt!.Value))) return await RejectAsync(cancellationToken);
        if (account.MustChangePassword && !clock.IsBefore(DateTimeOffset.FromUnixTimeSeconds(Utc(account.TempExpiresAt!.Value).ToUnixTimeSeconds()))) return await RejectAsync(cancellationToken);
        var window = account.MustChangePassword ? SessionIssuanceWindow.Restricted(now, Utc(account.TempExpiresAt!.Value)) : SessionIssuanceWindow.Normal(now);
        var jwtExpiry = DateTimeOffset.FromUnixTimeSeconds(window.AbsoluteExpiresAt.ToUnixTimeSeconds());
        // NumericDate cannot extend the DB deadline; do not issue an already-expired JWT.
        if (!clock.IsBefore(jwtExpiry)) return await RejectAsync(cancellationToken);
        var sid = RandomNumberGenerator.GetBytes(32); var token = signer.Issue(account.AccountId, account.SecurityVersion, sid, window);
        account.FailedLoginCount = 0; account.FailureWindowStartedAt = null; account.LoginThrottledUntil = null;
        await repository.SaveAccountAsync(cancellationToken);
        if (!clock.IsBefore(jwtExpiry)) throw new EnglishCenter.Api.Common.Errors.ApiErrorException(EnglishCenter.Api.Common.Errors.ApiErrorCode.AUTHENTICATION_FAILED);
        await repository.InsertSessionAsync(new AuthSession { Sid = sid, AccountId = account.AccountId, IssuedAt = window.IssuedAt.UtcDateTime, ExpiresAt = window.AbsoluteExpiresAt.UtcDateTime, LastActivityAt = window.IssuedAt.UtcDateTime, SecurityVersion = account.SecurityVersion, SessionScope = window.Scope }, cancellationToken);
        var effective = window.EffectiveExpiresAt < jwtExpiry ? window.EffectiveExpiresAt : jwtExpiry;
        var context = new AuthContextView(account.AccountId, account.FullName, account.Email, role, window.Scope, RowVersionCodec.Encode(account.RowVersion), Instant(clock.GetUtcNow()), Instant(window.AbsoluteExpiresAt), Instant(effective));
        return LoginDecision.Success(new(context, token, jwtExpiry));
    }
    private async Task<LoginDecision> RejectAsync(CancellationToken cancellationToken)
    {
        await repository.SaveAccountAsync(cancellationToken); return LoginDecision.Rejected();
    }
    private void ResetExpiredCounters(Account account)
    {
        var expired = account.LoginThrottledUntil is DateTime until ? !clock.IsBefore(Utc(until)) :
            account.FailureWindowStartedAt is DateTime started && !clock.IsBefore(Utc(started).AddMinutes(15));
        if (expired) { account.FailedLoginCount = 0; account.FailureWindowStartedAt = null; account.LoginThrottledUntil = null; }
    }
    private static DateTimeOffset Utc(DateTime value) => new(DateTime.SpecifyKind(value, DateTimeKind.Utc));
    private static string Instant(DateTimeOffset value) => value.UtcDateTime.ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'", CultureInfo.InvariantCulture);
}
