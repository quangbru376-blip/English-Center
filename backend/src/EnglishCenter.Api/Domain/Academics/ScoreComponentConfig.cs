namespace EnglishCenter.Api.Domain.Academics;

public sealed class ScoreComponentConfig
{
    public int ComponentId { get; set; }

    public int ProgramId { get; set; }

    public string ComponentName { get; set; } = string.Empty;

    public decimal Weight { get; set; }

    public string Kind { get; set; } = string.Empty;

    public byte[] RowVersion { get; set; } = [];

    public TrainingProgram? Program { get; set; }
}
