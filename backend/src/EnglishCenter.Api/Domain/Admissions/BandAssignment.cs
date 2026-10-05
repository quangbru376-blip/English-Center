namespace EnglishCenter.Api.Domain.Admissions;

using EnglishCenter.Api.Domain.Academics;

public sealed class BandAssignment
{
    public int BandAssignmentId { get; set; }
    public int RegistrationId { get; set; }
    public int BestAttemptId { get; set; }
    public int BandId { get; set; }
    public DateTime AssignedAt { get; set; }
    public byte[] RowVersion { get; set; } = [];

    public Registration? Registration { get; set; }
    public EntryTestAttempt? BestAttempt { get; set; }
    public Band? Band { get; set; }
}
