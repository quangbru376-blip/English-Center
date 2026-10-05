namespace EnglishCenter.Api.Domain.Training;

using EnglishCenter.Api.Domain.Identity;

public sealed class Attendance
{
    public int AttendanceId { get; set; }

    public int ScheduleId { get; set; }

    public int ClassAssignmentId { get; set; }

    public bool Present { get; set; }

    public int EnteredBy { get; set; }

    public DateTime EnteredAt { get; set; }

    public byte[] RowVersion { get; set; } = [];

    public Schedule? Schedule { get; set; }

    public StudentClassAssignment? ClassAssignment { get; set; }

    public Account? EnteredByAccount { get; set; }
}
