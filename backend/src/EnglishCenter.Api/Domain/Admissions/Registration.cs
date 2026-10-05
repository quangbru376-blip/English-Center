namespace EnglishCenter.Api.Domain.Admissions;

using EnglishCenter.Api.Domain.Academics;
using EnglishCenter.Api.Domain.Identity;

public sealed class Registration
{
    public int RegistrationId { get; set; }
    public int StudentId { get; set; }
    public int CourseId { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsUnfinished { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? AdmissionConfirmedAt { get; set; }
    public int? AdmissionApprovedBy { get; set; }
    public DateTime? AdmissionApprovedAt { get; set; }
    public int? RejectedBy { get; set; }
    public DateTime? RejectedAt { get; set; }
    public string? RejectionReason { get; set; }
    public string? RejectedFromStatus { get; set; }
    public Guid? RejectionOperationId { get; set; }
    public int? CancelledBy { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string? CancellationReason { get; set; }
    public string? CancelledFromStatus { get; set; }
    public Guid? CancellationOperationId { get; set; }
    public byte[] RowVersion { get; set; } = [];

    public StudentProfile? Student { get; set; }
    public Course? Course { get; set; }
    public Account? AdmissionApprovedByAccount { get; set; }
    public Account? RejectedByAccount { get; set; }
    public Account? CancelledByAccount { get; set; }
}
