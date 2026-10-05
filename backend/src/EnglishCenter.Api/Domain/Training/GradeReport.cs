using EnglishCenter.Api.Domain.Identity;

namespace EnglishCenter.Api.Domain.Training;

public sealed class GradeReport
{
    public int ReportId { get; set; }

    public int ClassAssignmentId { get; set; }

    public decimal? FinalScore { get; set; }

    public string Status { get; set; } = string.Empty;

    public string? TeacherComment { get; set; }

    public int? SubmittedBy { get; set; }

    public DateTime? SubmittedAt { get; set; }

    public Guid? SubmittedOperationId { get; set; }

    public int? ReviewedBy { get; set; }

    public DateTime? ReviewedAt { get; set; }

    public Guid? ReviewOperationId { get; set; }

    public string? ReturnedReason { get; set; }

    public DateTime? FinalizedAt { get; set; }

    public byte[] RowVersion { get; set; } = [];

    public StudentClassAssignment? ClassAssignment { get; set; }

    public Account? SubmittedByAccount { get; set; }

    public Account? ReviewedByAccount { get; set; }
}
