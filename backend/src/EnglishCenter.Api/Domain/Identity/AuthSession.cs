namespace EnglishCenter.Api.Domain.Identity;

public sealed class AuthSession
{
    public byte[] Sid { get; set; } = Array.Empty<byte>();
    public int AccountId { get; set; }
    public DateTime IssuedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime LastActivityAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public long SecurityVersion { get; set; }
    public string SessionScope { get; set; } = string.Empty;
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    public Account? Account { get; set; }
}
