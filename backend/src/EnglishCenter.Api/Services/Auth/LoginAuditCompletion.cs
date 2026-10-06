namespace EnglishCenter.Api.Services.Auth;

using EnglishCenter.Api.Common.Auditing;
using EnglishCenter.Api.Common.Time;

public static class LoginAuditCompletion
{
    // Call only after commit: audit storage failure cannot compensate the committed outcome.
    public static async Task<LoginDecision> AfterCommitAsync(LoginDecision decision, SecurityAuditRecorder audit, BusinessClock clock, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(decision);
        ArgumentNullException.ThrowIfNull(audit);
        ArgumentNullException.ThrowIfNull(clock);
        await audit.TryRecordAsync(SecurityAuditKind.Login, decision.Grant?.Context.AccountId, decision.Accepted, Guid.NewGuid(), clock.GetUtcNow(), null, cancellationToken);
        return decision;
    }
}
