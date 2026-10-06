namespace EnglishCenter.Api.Common.Security;

// Cryptographic proof only. Current Account/Session rights must still be checked in SQL.
public sealed class VerifiedSessionToken
{
    private readonly byte[] _sid;
    public int AccountId { get; }
    public long SecurityVersion { get; }
    public string Scope { get; }
    public DateTimeOffset IssuedAt { get; }
    public DateTimeOffset ExpiresAt { get; }
    internal VerifiedSessionToken(int accountId, long version, string scope, byte[] sid, DateTimeOffset issuedAt, DateTimeOffset expiresAt)
    { AccountId = accountId; SecurityVersion = version; Scope = scope; _sid = sid.ToArray(); IssuedAt = issuedAt; ExpiresAt = expiresAt; }
    public byte[] CopySid() => _sid.ToArray();
    public override string ToString() => nameof(VerifiedSessionToken);
}
