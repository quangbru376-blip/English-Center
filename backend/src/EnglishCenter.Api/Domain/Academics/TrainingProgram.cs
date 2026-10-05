namespace EnglishCenter.Api.Domain.Academics;

public sealed class TrainingProgram
{
    public int ProgramId { get; set; }

    public int CourseId { get; set; }

    public int BandId { get; set; }

    public string ProgramName { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public int ExpectedLessonCount { get; set; }

    public byte[] RowVersion { get; set; } = [];

    public Course? Course { get; set; }

    public Band? Band { get; set; }
}
