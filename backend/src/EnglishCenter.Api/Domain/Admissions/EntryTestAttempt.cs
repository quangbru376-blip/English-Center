namespace EnglishCenter.Api.Domain.Admissions;

using EnglishCenter.Api.Domain.Identity;

public sealed class EntryTestAttempt
{
    public int AttemptId { get; set; }

    public int RegistrationId { get; set; }

    public int AttemptNumber { get; set; }

    public decimal? Score { get; set; }

    public int? GradedBy { get; set; }

    public DateTime? GradedAt { get; set; }

    public int? CompletedBookingId { get; set; }

    public byte[] RowVersion { get; set; } = [];

    public Registration? Registration { get; set; }

    public Account? GradedByAccount { get; set; }

    public EntryTestBooking? CompletedBooking { get; set; }
}
