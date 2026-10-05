namespace EnglishCenter.Api.Domain.Training;

using EnglishCenter.Api.Domain.Academics;
using EnglishCenter.Api.Domain.Identity;

public sealed class Score
{
    public int ScoreId { get; set; }

    public int ClassAssignmentId { get; set; }

    public int ComponentId { get; set; }

    public int? ScheduleId { get; set; }

    public decimal ScoreValue { get; set; }

    public int EnteredBy { get; set; }

    public DateTime EnteredAt { get; set; }

    public byte[] RowVersion { get; set; } = [];

    public StudentClassAssignment? ClassAssignment { get; set; }

    public ScoreComponentConfig? Component { get; set; }

    public Schedule? Schedule { get; set; }

    public Account? EnteredByAccount { get; set; }
}
