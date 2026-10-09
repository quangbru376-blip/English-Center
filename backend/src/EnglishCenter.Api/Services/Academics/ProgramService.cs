namespace EnglishCenter.Api.Services.Academics;

using System.Text.Json;
using System.Text.Json.Nodes;
using EnglishCenter.Api.Common.Auditing;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Common.Security;
using EnglishCenter.Api.Common.Serialization;
using EnglishCenter.Api.Common.Time;
using EnglishCenter.Api.Domain.Academics;
using EnglishCenter.Api.Services.Auth;
using EnglishCenter.Api.Services.Replay;

public sealed record ComponentDesired(int? Id, string Name, decimal Weight, string Kind);
public interface IProgramRepository : IAcademicRepository
{
    Task<TrainingProgram?> LockProgramAsync(int id, CancellationToken ct);
    Task<Course?> CourseAsync(int id, CancellationToken ct);
    Task<Band?> BandAsync(int id, CancellationToken ct);
    Task<bool> PairExistsAsync(int course, int band, int excluded, CancellationToken ct);
    Task<ProgramState> StateAsync(int id, CancellationToken ct, int? targetCourse = null, int? targetBand = null);
    Task InsertAsync(TrainingProgram program, CancellationToken ct);
    Task ReplaceComponentsAsync(int program, IReadOnlyList<ScoreComponentConfig> original, IReadOnlyList<ComponentDesired> desired, CancellationToken ct);
}
public sealed class ProgramService(IProgramRepository repository, IReplayRepository markers, BusinessClock clock)
{
    public async Task<ProgramCommandResult> CommandAsync(VerifiedSessionToken token, AcademicInput input, CancellationToken ct)
    {
        var action = input.Intent.Action; await markers.AcquireAsync(ReplayService.RequestResource(token.AccountId, action, input.RequestId), ct);
        if (action.ApiId != 85) await markers.AcquireAsync("EC:BANDSET", ct);
        if (input.TargetId > 0) await markers.AcquireAsync("EC:PROGRAM:" + input.TargetId, ct);
        await repository.PrepareAccountsAsync(token, [], ct); var current = await AcademicAuthority.RequireAsync(repository, token, clock, ct);
        var replay = new ReplayService(markers, clock); ProgramState? repeated = null;
        var done = await replay.BeginAsync(token.AccountId, input.RequestId, input.Intent, async (stored, c) => { await AcademicAuthority.RequireAsync(repository, token, clock, c); repeated = await repository.StateAsync(stored.Target["programId"], c); return true; }, ct);
        if (done is not null) return new(ProgramProjection.Detail(repeated!), done.OperationId) { Replayed = true };
        var body = input.Body; var versions = body.GetProperty("expectedVersions"); TrainingProgram program; ProgramState? source = null;
        var operation = Guid.NewGuid(); var now = clock.GetUtcNow();
        if (action.ApiId == 82)
        { program = new() { CourseId = body.GetProperty("courseId").GetInt32(), BandId = body.GetProperty("bandId").GetInt32(), ProgramName = body.GetProperty("name").GetString()!, ExpectedLessonCount = body.GetProperty("expectedLessonCount").GetInt32(), Status = "DRAFT" }; }
        else { source = await repository.StateAsync(input.TargetId, ct,
            action.ApiId == 84 ? body.GetProperty("courseId").GetInt32() : null,
            action.ApiId == 84 ? body.GetProperty("bandId").GetInt32() : null); program = source.Program; AcademicGuard.Version(versions.GetProperty("program"), program.RowVersion); }
        var old = source is null ? null : State(program);
        var oldComponents = source?.Components.ToDictionary(x => x.ComponentId, ComponentState) ?? new Dictionary<int, JsonObject>();
        if (action.ApiId is 82 or 84)
            await ApplyConfiguration(program, source, body, ct);
        else if (action.ApiId == 85)
        { var name = body.GetProperty("name").GetString()!; if (name == program.ProgramName) throw ApiErrorException.Validation("name"); program.ProgramName = name; }
        else if (action.ApiId == 87)
            ActivateSavedConfiguration(source!, body);
        else { if (program.Status != "ACTIVE" || action.ApiId == 88 && source!.Frozen) throw AcademicGuard.Conflict(); program.Status = action.ApiId == 88 ? "DRAFT" : "INACTIVE"; }
        await repository.SaveAsync(ct);
        await repository.AuditAsync(AuditEntry.Mandatory(action.Code, token.AccountId, "TrainingProgram", program.ProgramId, operation, now, new(old, State(program))), ct);
        var result = await repository.StateAsync(program.ProgramId, ct);
        if (action.ApiId is 82 or 84)
            await AuditComponents(result, oldComponents, action.Code, token.AccountId, operation, now, ct);
        await replay.CompleteAsync(token.AccountId, input.RequestId, input.Intent, operation, new Dictionary<string, int> { { "programId", program.ProgramId } }, ct);
        await new CurrentSessionService(repository, clock).TouchApprovedInteractiveActivityAsync(token, ct); await AcademicAuthority.RequireAsync(repository, token, clock, ct);
        if (!clock.IsBefore(current.EffectiveDeadline)) throw new ApiErrorException(ApiErrorCode.AUTHENTICATION_REQUIRED);
        return new(ProgramProjection.Detail(result), operation);
    }
    private async Task ApplyConfiguration(TrainingProgram program, ProgramState? source, JsonElement body, CancellationToken ct)
    {
        if (source is not null && (program.Status != "DRAFT" || source.Frozen)) throw AcademicGuard.Conflict();
        var versions = body.GetProperty("expectedVersions");
        var course = await repository.CourseAsync(body.GetProperty("courseId").GetInt32(), ct) ?? throw new ApiErrorException(ApiErrorCode.NOT_FOUND);
        var band = await repository.BandAsync(body.GetProperty("bandId").GetInt32(), ct) ?? throw new ApiErrorException(ApiErrorCode.NOT_FOUND);
        AcademicGuard.Version(versions.GetProperty("course"), course.RowVersion);
        AcademicGuard.Version(versions.GetProperty("band"), band.RowVersion);
        if (await repository.PairExistsAsync(course.CourseId, band.BandId, program.ProgramId, ct)) throw new ApiErrorException(ApiErrorCode.CONFLICT);
        var desired = body.GetProperty("components").EnumerateArray().Select(x => DesiredComponent(x, source is not null)).ToArray();
        if (source is not null) RequireCompleteSource(source, body, desired);
        program.CourseId = course.CourseId;
        program.BandId = band.BandId;
        program.ProgramName = body.GetProperty("name").GetString()!;
        program.ExpectedLessonCount = body.GetProperty("expectedLessonCount").GetInt32();
        if (source is null)
        {
            await repository.InsertAsync(program, ct);
            await markers.AcquireAsync("EC:PROGRAM:" + program.ProgramId, ct);
        }
        await repository.ReplaceComponentsAsync(program.ProgramId, source?.Components ?? [], desired, ct);
    }
    private static ComponentDesired DesiredComponent(JsonElement value, bool editing) => new(
        editing && value.GetProperty("id").ValueKind != JsonValueKind.Null ? value.GetProperty("id").GetInt32() : null,
        value.GetProperty("name").GetString()!, ExactNumbers.ParseWeight(value.GetProperty("weight").GetString()), value.GetProperty("kind").GetString()!);
    private static void RequireCompleteSource(ProgramState source, JsonElement body, ComponentDesired[] desired)
    {
        AcademicGuard.Set(body.GetProperty("expectedSet").GetProperty("existingComponentIds"), source.Components.Select(x => x.ComponentId));
        AcademicGuard.Versions(body.GetProperty("expectedVersions").GetProperty("existingComponents"), ProgramProjection.References(source));
        if (desired.Any(x => x.Id is { } id && !source.Components.Any(y => y.ComponentId == id))) throw new ApiErrorException(ApiErrorCode.NOT_FOUND);
    }
    private static void ActivateSavedConfiguration(ProgramState source, JsonElement body)
    {
        if (source.Program.Status == "ACTIVE" || !ProgramProjection.Validate(source).MeetsActivationRules) throw AcademicGuard.Conflict();
        var set = body.GetProperty("expectedSet"); var versions = body.GetProperty("expectedVersions");
        if (set.GetProperty("courseId").GetInt32() != source.Program.CourseId || set.GetProperty("bandId").GetInt32() != source.Program.BandId) throw AcademicGuard.Conflict();
        AcademicGuard.Version(versions.GetProperty("course"), source.Course.RowVersion);
        AcademicGuard.Version(versions.GetProperty("band"), source.Band.RowVersion);
        AcademicGuard.Set(set.GetProperty("componentIds"), source.Components.Select(x => x.ComponentId));
        AcademicGuard.Versions(versions.GetProperty("components"), ProgramProjection.References(source));
        source.Program.Status = "ACTIVE";
    }
    private async Task AuditComponents(ProgramState result, Dictionary<int,JsonObject> oldComponents, string action, int actor, Guid operation, DateTimeOffset now, CancellationToken ct)
    {
        var newComponents = result.Components.ToDictionary(x => x.ComponentId, ComponentState);
        foreach (var id in oldComponents.Keys.Union(newComponents.Keys).Order())
        {
            oldComponents.TryGetValue(id, out var before); newComponents.TryGetValue(id, out var after);
            if (before?.ToJsonString() == after?.ToJsonString()) continue;
            await repository.AuditAsync(AuditEntry.Mandatory(action + ".EFFECT", actor, "ScoreComponentConfig", id, operation, now,
                new(before, after, Parent: new JsonObject { ["programId"] = result.Program.ProgramId })), ct);
        }
    }
    private static JsonObject State(TrainingProgram p) => new() { ["courseId"] = p.CourseId, ["bandId"] = p.BandId, ["programName"] = p.ProgramName, ["status"] = p.Status, ["expectedLessonCount"] = p.ExpectedLessonCount };
    private static JsonObject ComponentState(ScoreComponentConfig c) => new() { ["componentName"] = c.ComponentName, ["kind"] = c.Kind, ["weight"] = c.Weight };
}
