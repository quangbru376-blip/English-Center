namespace EnglishCenter.Api.Common.Security;

/// <summary>
/// Computes immutable initial session issuance windows for normal and restricted sessions.
/// </summary>
public sealed class SessionIssuanceWindow
{
    public const string NormalScope = "NORMAL";
    public const string PasswordChangeScope = "PASSWORD_CHANGE";

    public string Scope { get; }
    public DateTimeOffset IssuedAt { get; }
    public DateTimeOffset AbsoluteExpiresAt { get; }
    public DateTimeOffset EffectiveExpiresAt { get; }

    private SessionIssuanceWindow(
        string scope,
        DateTimeOffset issuedAt,
        DateTimeOffset absoluteExpiresAt,
        DateTimeOffset effectiveExpiresAt)
    {
        Scope = scope;
        IssuedAt = issuedAt;
        AbsoluteExpiresAt = absoluteExpiresAt;
        EffectiveExpiresAt = effectiveExpiresAt;
    }

    public static SessionIssuanceWindow Normal(DateTimeOffset now)
    {
        var issuedAt = TruncateToMilliseconds(now);
        var absoluteExpiresAt = issuedAt.AddHours(2);
        var candidateEffective = issuedAt.AddMinutes(30);
        var effectiveExpiresAt = candidateEffective < absoluteExpiresAt
            ? candidateEffective
            : absoluteExpiresAt;

        return new SessionIssuanceWindow(
            NormalScope,
            issuedAt,
            absoluteExpiresAt,
            effectiveExpiresAt);
    }

    public static SessionIssuanceWindow Restricted(DateTimeOffset now, DateTimeOffset temporaryExpiresAt)
    {
        if (temporaryExpiresAt <= now)
        {
            throw new ArgumentException(
                "Temporary expiration deadline must be strictly after the current time.",
                nameof(temporaryExpiresAt));
        }

        var issuedAt = TruncateToMilliseconds(now);
        var normalizedTempDeadline = TruncateToMilliseconds(temporaryExpiresAt);

        if (normalizedTempDeadline <= issuedAt)
        {
            throw new ArgumentException(
                "Normalized temporary expiration deadline must be strictly after the normalized issued timestamp.",
                nameof(temporaryExpiresAt));
        }

        var maxRestrictedExpiresAt = issuedAt.AddMinutes(10);
        var expiresAt = normalizedTempDeadline < maxRestrictedExpiresAt
            ? normalizedTempDeadline
            : maxRestrictedExpiresAt;

        return new SessionIssuanceWindow(
            PasswordChangeScope,
            issuedAt,
            expiresAt,
            expiresAt);
    }

    private static DateTimeOffset TruncateToMilliseconds(DateTimeOffset instant)
    {
        var utc = instant.ToUniversalTime();
        long truncatedTicks = utc.Ticks - (utc.Ticks % TimeSpan.TicksPerMillisecond);
        return new DateTimeOffset(truncatedTicks, TimeSpan.Zero);
    }
}
