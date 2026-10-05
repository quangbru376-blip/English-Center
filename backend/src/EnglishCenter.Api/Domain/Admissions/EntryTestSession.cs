namespace EnglishCenter.Api.Domain.Admissions;

using EnglishCenter.Api.Domain.Identity;

public sealed class EntryTestSession
{
    public int SessionId { get; set; }
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public string Room { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public int GraderId { get; set; }
    public string Status { get; set; } = string.Empty;
    public int? CancelledBy { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string? CancellationReason { get; set; }
    public Guid? CancellationOperationId { get; set; }
    public byte[] RowVersion { get; set; } = [];

    public Account? Grader { get; set; }
    public Account? CancelledByAccount { get; set; }
}
