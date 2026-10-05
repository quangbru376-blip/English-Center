namespace EnglishCenter.Api.Domain.Training;

using EnglishCenter.Api.Domain.Identity;

public sealed class Schedule
{
    public int ScheduleId { get; set; }

    public int ClassId { get; set; }

    public string SessionType { get; set; } = string.Empty;

    public Guid ObligationKey { get; set; }

    public int? ReplacesScheduleId { get; set; }

    public bool IsCurrent { get; set; } = true;

    public DateOnly SessionDate { get; set; }

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public string Room { get; set; } = string.Empty;

    public string OccurrenceState { get; set; } = string.Empty;

    public int? HeldBy { get; set; }

    public DateTime? HeldAt { get; set; }

    public int? NotHeldBy { get; set; }

    public DateTime? NotHeldAt { get; set; }

    public string? NotHeldReason { get; set; }

    public Guid? ReplacementOperationId { get; set; }

    public byte[] RowVersion { get; set; } = [];

    public Class? Class { get; set; }

    public Schedule? ReplacesSchedule { get; set; }

    public Account? HeldByAccount { get; set; }

    public Account? NotHeldByAccount { get; set; }
}
