namespace EnglishCenter.Api.Services.Auth;

using System.Globalization;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Common.Security;
using EnglishCenter.Api.Common.Serialization;
using EnglishCenter.Api.Common.Time;
using EnglishCenter.Api.Domain.Identity;

public interface ICurrentSessionRepository
{
    Task<Account?> LockAccountAsync(int accountId, CancellationToken cancellationToken);
    Task<AuthSession?> LockSessionAsync(byte[] sid, CancellationToken cancellationToken);
    Task<string?> RoleCodeAsync(int roleId, CancellationToken cancellationToken);
    Task SaveSessionAsync(CancellationToken cancellationToken);
}
public sealed class CurrentSessionSnapshot(AuthContextView context, DateTimeOffset effectiveDeadline)
{
    public AuthContextView Context { get; } = context;
    public DateTimeOffset EffectiveDeadline { get; } = effectiveDeadline;
    public override string ToString() => nameof(CurrentSessionSnapshot);
}
public sealed record CurrentSessionRevocation(int VerifiedActor, DateTimeOffset ValidUntil);
public interface ICurrentSessionReader
{
    Task<CurrentSessionSnapshot> ReadAsync(VerifiedSessionToken token, bool requiresNormal, CancellationToken cancellationToken);
}
public sealed class CurrentSessionService(ICurrentSessionRepository repository, BusinessClock clock)
{
    private static ApiErrorException Denied() => new(ApiErrorCode.AUTHENTICATION_REQUIRED);
    private async Task<(Account Account, AuthSession Session, string Role, DateTimeOffset Deadline)> RequireAsync(VerifiedSessionToken token, bool normal, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(token); ct.ThrowIfCancellationRequested();
        var account = await repository.LockAccountAsync(token.AccountId, ct) ?? throw Denied();
        var session = await repository.LockSessionAsync(token.CopySid(), ct) ?? throw Denied();
        if (account.Status != "ACTIVE" || session.AccountId != account.AccountId || account.SecurityVersion != token.SecurityVersion || session.SecurityVersion != token.SecurityVersion ||
            session.RevokedAt is not null || session.SessionScope != token.Scope || !session.Sid.SequenceEqual(token.CopySid())) throw Denied();
        var issued = Utc(session.IssuedAt); var expires = Utc(session.ExpiresAt); var activity = Utc(session.LastActivityAt);
        if (issued.ToUnixTimeSeconds() != token.IssuedAt.ToUnixTimeSeconds() || token.ExpiresAt > expires || clock.IsBefore(issued) || clock.IsBefore(activity)) throw Denied();
        DateTimeOffset? deadline;
        if (token.Scope == "NORMAL")
        {
            if (account.MustChangePassword) throw Denied();
            deadline = SessionDeadlineMath.Normal(issued, expires, activity, token.ExpiresAt);
        }
        else
        {
            if (!account.MustChangePassword || account.TempIssuedAt is null || account.TempExpiresAt is null || clock.IsBefore(Utc(account.TempIssuedAt.Value))) throw Denied();
            var tempIssued = Utc(account.TempIssuedAt.Value); if (tempIssued > issued) throw Denied();
            DateTimeOffset tempMaximum; try { tempMaximum = tempIssued.AddHours(24); } catch (ArgumentOutOfRangeException) { throw Denied(); }
            if (Utc(account.TempExpiresAt.Value) != tempMaximum) throw Denied();
            var tempDeadline = tempMaximum;
            deadline = SessionDeadlineMath.Restricted(issued, expires, token.ExpiresAt, tempDeadline);
        }
        if (deadline is null || !clock.IsBefore(deadline.Value)) throw Denied();
        var role = await repository.RoleCodeAsync(account.RoleId, ct);
        if (role is null || !FixedRoleCodes.All.Contains(role, StringComparer.Ordinal)) throw Denied();
        if (!clock.IsBefore(deadline.Value)) throw Denied();
        if (normal && token.Scope != "NORMAL") throw new ApiErrorException(ApiErrorCode.FORBIDDEN);
        return (account, session, role, deadline.Value);
    }
    public async Task<CurrentSessionSnapshot> ReadAsync(VerifiedSessionToken token, bool requiresNormal, CancellationToken ct)
    {
        var facts = await RequireAsync(token, requiresNormal, ct);
        var context = new AuthContextView(facts.Account.AccountId, facts.Account.FullName, facts.Account.Email, facts.Role, token.Scope,
            RowVersionCodec.Encode(facts.Account.RowVersion), Instant(clock.GetUtcNow()), Instant(Utc(facts.Session.ExpiresAt)), Instant(facts.Deadline));
        return new(context, facts.Deadline);
    }
    // Caller holds domain gates, then these Account/Session locks until its transaction completes.
    // Invoke only after an authorized interactive business operation, never from request headers.
    public async Task TouchApprovedInteractiveActivityAsync(VerifiedSessionToken token, CancellationToken ct)
    {
        var facts = await RequireAsync(token, true, ct);
        if (!clock.IsBefore(facts.Deadline)) throw Denied();
        var at = clock.GetUtcNow().UtcDateTime; if (at < facts.Session.LastActivityAt || !clock.IsBefore(facts.Deadline)) throw Denied();
        facts.Session.LastActivityAt = at;
        await repository.SaveSessionAsync(ct);
        if (!clock.IsBefore(facts.Deadline)) throw Denied();
    }
    public async Task<CurrentSessionRevocation> RevokeCurrentAsync(VerifiedSessionToken token, CancellationToken ct)
    {
        var facts = await RequireAsync(token, false, ct);
        if (!clock.IsBefore(facts.Deadline)) throw Denied();
        var at = clock.GetUtcNow().UtcDateTime; if (at < facts.Session.IssuedAt || !clock.IsBefore(facts.Deadline)) throw Denied();
        facts.Session.RevokedAt = at;
        await repository.SaveSessionAsync(ct);
        return new(facts.Account.AccountId, facts.Deadline);
    }
    private static DateTimeOffset Utc(DateTime value) => new(DateTime.SpecifyKind(value, DateTimeKind.Utc));
    private static string Instant(DateTimeOffset value) => value.UtcDateTime.ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'", CultureInfo.InvariantCulture);
}
