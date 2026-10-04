namespace EnglishCenter.Api.Domain.Identity;

public sealed class StudentProfile
{
    public int StudentId { get; set; }
    public int AccountId { get; set; }
    public DateOnly DateOfBirth { get; set; }
    public string? Address { get; set; }
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    public Account? Account { get; set; }
}
