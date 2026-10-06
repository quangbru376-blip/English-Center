namespace EnglishCenter.Api.Services.Auth;

using EnglishCenter.Api.Common.Auditing;
using EnglishCenter.Api.Common.Time;
using EnglishCenter.Api.Common.Security;

public interface ICurrentSessionLogout
{
    // Returns only after current-session revocation commits, then independent security audit.
    Task ExecuteAsync(VerifiedSessionToken token, CancellationToken cancellationToken);
}
public static class LogoutAuditCompletion
{
    public static async Task AfterCommitAsync(int verifiedActor, SecurityAuditRecorder audit, BusinessClock clock, CancellationToken ct)
    {
        if (verifiedActor <= 0) throw new ArgumentOutOfRangeException(nameof(verifiedActor));
        await audit.TryRecordAsync(SecurityAuditKind.Logout, verifiedActor, true, Guid.NewGuid(), clock.GetUtcNow(), null, ct);
    }
}
