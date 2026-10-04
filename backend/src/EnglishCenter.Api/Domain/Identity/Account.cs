namespace EnglishCenter.Api.Domain.Identity;

public sealed class Account
{
    public int AccountId { get; set; }
    public int RoleId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public long SecurityVersion { get; set; }
    public int FailedLoginCount { get; set; }
    public DateTime? FailureWindowStartedAt { get; set; }
    public DateTime? LoginThrottledUntil { get; set; }
    public bool MustChangePassword { get; set; }
    public DateTime? TempIssuedAt { get; set; }
    public DateTime? TempExpiresAt { get; set; }
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    public Role? Role { get; set; }
    public StudentProfile? StudentProfile { get; set; }
}
