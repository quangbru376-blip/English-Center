namespace EnglishCenter.Api.Domain.Admissions;

using EnglishCenter.Api.Domain.Identity;

public sealed class EntryTestBooking
{
    public int BookingId { get; set; }

    public int AttemptId { get; set; }

    public int SessionId { get; set; }

    public string Status { get; set; } = string.Empty;

    public DateTime BookedAt { get; set; }

    public int? AbsentBy { get; set; }

    public DateTime? AbsentAt { get; set; }

    public Guid? AbsentOperationId { get; set; }

    public int? CancelledBy { get; set; }

    public DateTime? CancelledAt { get; set; }

    public string? CancellationReason { get; set; }

    public string? CancelledFromStatus { get; set; }

    public Guid? CancellationOperationId { get; set; }

    public byte[] RowVersion { get; set; } = [];

    public EntryTestAttempt? Attempt { get; set; }

    public EntryTestSession? Session { get; set; }

    public Account? AbsentByAccount { get; set; }

    public Account? CancelledByAccount { get; set; }
}
