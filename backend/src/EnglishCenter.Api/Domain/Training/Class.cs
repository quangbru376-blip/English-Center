namespace EnglishCenter.Api.Domain.Training;

using EnglishCenter.Api.Domain.Academics;
using EnglishCenter.Api.Domain.Identity;

public sealed class Class
{
    public int ClassId { get; set; }
    public int ProgramId { get; set; }
    public int TeacherId { get; set; }
    public string ClassCode { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int MaxSize { get; set; }
    public DateTime CreatedAt { get; set; }
    public int? CancelledBy { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string? CancellationReason { get; set; }
    public Guid? CancellationOperationId { get; set; }
    public byte[] RowVersion { get; set; } = [];

    public TrainingProgram? Program { get; set; }
    public Account? Teacher { get; set; }
    public Account? CancelledByAccount { get; set; }
}
