namespace EnglishCenter.Api.Services.Academics;

using EnglishCenter.Api.Common.Serialization;
using EnglishCenter.Api.Domain.Academics;

public sealed record AcademicCourse(int Id, string Code, string Name, string Status);
public sealed record AcademicBand(int Id, string Code, string MinScore);
public record ProgramSummary(int Id, string Name, AcademicCourse Course, AcademicBand Band, string Status, int ExpectedLessonCount, bool IsConfigurationFrozen, string Version);
public sealed record ProgramComponent(int Id, string Name, string Weight, string Kind, string Version);
public sealed record ConfigurationIssue(string Field, string Message);
public sealed record ConfigurationValidation(string TotalWeight, int FinalExamCount, bool MeetsActivationRules, IReadOnlyList<ConfigurationIssue> Issues);
public sealed record ProgramDetail(int Id, string Name, AcademicCourse Course, AcademicBand Band, string Status, int ExpectedLessonCount, bool IsConfigurationFrozen, string Version,
    IReadOnlyList<ProgramComponent> Components, object RelationVersions, ConfigurationValidation Validation, IReadOnlyList<string> AllowedActions, IReadOnlyDictionary<string, string> DisabledReasons,
    object? ConfigurationManifest, object? ActivationManifest) : ProgramSummary(Id, Name, Course, Band, Status, ExpectedLessonCount, IsConfigurationFrozen, Version);
public sealed record ProgramCommandResult(ProgramDetail Program, Guid OperationId)
{ [System.Text.Json.Serialization.JsonIgnore] public bool Replayed { get; init; } }
public sealed record ProgramActivationPreview(ProgramSummary Program, ConfigurationValidation Validation, bool Eligible, IReadOnlyList<string> BlockedReasons, object? Manifest);
public sealed record ProgramState(TrainingProgram Program, Course Course, Band Band, IReadOnlyList<ScoreComponentConfig> Components, bool Frozen);
public static class ProgramProjection
{
    public static ProgramSummary Summary(ProgramState state)
    { var p = state.Program; return new(p.ProgramId, p.ProgramName, new(state.Course.CourseId, state.Course.CourseCode, state.Course.CourseName, state.Course.Status), new(state.Band.BandId, state.Band.BandCode, ExactNumbers.FormatScore(state.Band.MinScore)), p.Status, p.ExpectedLessonCount, state.Frozen, RowVersionCodec.Encode(p.RowVersion)); }
    public static ConfigurationValidation Validate(ProgramState state)
    {
        var sum = state.Components.Sum(x => x.Weight); var exams = state.Components.Count(x => x.Kind == "FINAL_EXAM"); var issues = new List<ConfigurationIssue>();
        if (sum != 100) issues.Add(new("components", "Tổng trọng số phải bằng 100."));
        if (exams != 1) issues.Add(new("components", "Cần đúng một thành phần thi cuối khóa."));
        if (state.Program.ExpectedLessonCount <= 0) issues.Add(new("expectedLessonCount", "Số buổi phải dương."));
        return new(sum.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture), exams, issues.Count == 0, issues);
    }
    public static object ActivationManifest(ProgramState state) => new { expectedVersions = new { program = RowVersionCodec.Encode(state.Program.RowVersion), course = RowVersionCodec.Encode(state.Course.RowVersion), band = RowVersionCodec.Encode(state.Band.RowVersion), components = References(state) }, expectedSet = new { courseId = state.Course.CourseId, bandId = state.Band.BandId, componentIds = state.Components.Select(x => x.ComponentId).ToArray() } };
    public static VersionedId[] References(ProgramState state) => state.Components.Select(x => new VersionedId(x.ComponentId, RowVersionCodec.Encode(x.RowVersion))).ToArray();
    public static ProgramDetail Detail(ProgramState state)
    {
        var s = Summary(state); var valid = Validate(state); var editable = s.Status == "DRAFT" && !s.IsConfigurationFrozen; var activate = s.Status != "ACTIVE" && valid.MeetsActivationRules;
        var available = new Dictionary<string, bool> { { "EDIT_CONFIGURATION", editable }, { "RENAME", true }, { "PREVIEW_ACTIVATION", s.Status != "ACTIVE" }, { "ACTIVATE", activate }, { "RETURN_TO_DRAFT", s.Status == "ACTIVE" && !s.IsConfigurationFrozen }, { "DEACTIVATE", s.Status == "ACTIVE" } };
        return new(s.Id, s.Name, s.Course, s.Band, s.Status, s.ExpectedLessonCount, s.IsConfigurationFrozen, s.Version,
            state.Components.Select(x => new ProgramComponent(x.ComponentId, x.ComponentName, ExactNumbers.FormatWeight(x.Weight), x.Kind, RowVersionCodec.Encode(x.RowVersion))).ToArray(),
            new { course = RowVersionCodec.Encode(state.Course.RowVersion), band = RowVersionCodec.Encode(state.Band.RowVersion) }, valid, available.Where(x => x.Value).Select(x => x.Key).ToArray(), available.Where(x => !x.Value).ToDictionary(x => x.Key, _ => "Trạng thái hoặc cấu hình không cho phép thao tác."),
            editable ? new { expectedVersions = new { program = s.Version, course = RowVersionCodec.Encode(state.Course.RowVersion), band = RowVersionCodec.Encode(state.Band.RowVersion), existingComponents = References(state) }, expectedSet = new { existingComponentIds = state.Components.Select(x => x.ComponentId).ToArray() } } : null,
            activate ? ActivationManifest(state) : null);
    }
}
