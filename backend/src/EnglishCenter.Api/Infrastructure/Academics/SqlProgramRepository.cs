namespace EnglishCenter.Api.Infrastructure.Academics;

using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Domain.Academics;
using EnglishCenter.Api.Infrastructure.Identity;
using EnglishCenter.Api.Services.Academics;
using Microsoft.EntityFrameworkCore;

public sealed class SqlProgramRepository(SqlAuthDeadlineScope scope) : SqlAcademicRepository(scope), IProgramRepository
{
    public Task<TrainingProgram?> LockProgramAsync(int id, CancellationToken ct) => Db.TrainingPrograms.FromSqlInterpolated($"SELECT * FROM dbo.TrainingProgram WITH(UPDLOCK,HOLDLOCK) WHERE program_id={id}").SingleOrDefaultAsync(ct);
    public Task<Course?> CourseAsync(int id, CancellationToken ct) => Db.Courses.FromSqlInterpolated($"SELECT * FROM dbo.Course WITH(UPDLOCK,HOLDLOCK) WHERE course_id={id}").SingleOrDefaultAsync(ct);
    public Task<Band?> BandAsync(int id, CancellationToken ct) => Db.Bands.FromSqlInterpolated($"SELECT * FROM dbo.Band WITH(UPDLOCK,HOLDLOCK) WHERE band_id={id}").SingleOrDefaultAsync(ct);
    public Task<bool> PairExistsAsync(int course, int band, int excluded, CancellationToken ct) => Db.TrainingPrograms.AsNoTracking().AnyAsync(x => x.CourseId == course && x.BandId == band && x.ProgramId != excluded, ct);
    public async Task<ProgramState> StateAsync(int id, CancellationToken ct, int? targetCourse = null, int? targetBand = null)
    {
        var reference = await Db.TrainingPrograms.AsNoTracking().Where(x => x.ProgramId == id).Select(x => new { x.CourseId, x.BandId }).SingleOrDefaultAsync(ct) ?? throw new ApiErrorException(ApiErrorCode.NOT_FOUND);
        // Lock the complete source/target union before moving to the next table.
        var courses = new Dictionary<int, Course>();
        foreach (var key in new[] { reference.CourseId, targetCourse ?? reference.CourseId }.Distinct().Order())
            courses[key] = await CourseAsync(key, ct) ?? throw new ApiErrorException(ApiErrorCode.NOT_FOUND);
        var bands = new Dictionary<int, Band>();
        foreach (var key in new[] { reference.BandId, targetBand ?? reference.BandId }.Distinct().Order())
            bands[key] = await BandAsync(key, ct) ?? throw new ApiErrorException(ApiErrorCode.NOT_FOUND);
        var program = await LockProgramAsync(id, ct) ?? throw new ApiErrorException(ApiErrorCode.NOT_FOUND);
        if (program.CourseId != reference.CourseId || program.BandId != reference.BandId) throw AcademicGuard.Conflict();
        var rows = await Db.ScoreComponentConfigs.FromSqlInterpolated($"SELECT * FROM dbo.ScoreComponentConfig WITH(UPDLOCK,HOLDLOCK) WHERE program_id={id}").OrderBy(x => x.ComponentId).ToListAsync(ct);
        return new(program, courses[reference.CourseId], bands[reference.BandId], rows, await Db.Classes.AnyAsync(x => x.ProgramId == id, ct));
    }
    public async Task InsertAsync(TrainingProgram program, CancellationToken ct) { Db.TrainingPrograms.Add(program); await SaveAsync(ct); }
    public async Task ReplaceComponentsAsync(int program, IReadOnlyList<ScoreComponentConfig> original, IReadOnlyList<ComponentDesired> desired, CancellationToken ct)
    {
        // Free unique names and the filtered exam index without losing existing IDs.
        // These staging values are never visible outside the caller transaction.
        var nonce = Guid.NewGuid().ToString("N");
        foreach (var row in original) { row.ComponentName = "__EC_" + nonce + "_" + row.ComponentId; row.Kind = "GENERAL"; }
        await SaveAsync(ct);
        var retained = desired.Where(x => x.Id is not null).Select(x => x.Id!.Value).ToHashSet();
        Db.ScoreComponentConfigs.RemoveRange(original.Where(x => !retained.Contains(x.ComponentId))); await SaveAsync(ct);
        foreach (var value in desired)
        { var row = value.Id is { } id ? original.Single(x => x.ComponentId == id) : new ScoreComponentConfig { ProgramId = program }; row.ComponentName = value.Name; row.Weight = value.Weight; row.Kind = value.Kind; if (value.Id is null) Db.ScoreComponentConfigs.Add(row); }
        await SaveAsync(ct);
    }
    public static async Task<ProgramState> ReadState(EnglishCenterDbContext db, int id, CancellationToken ct)
    { var p = await db.TrainingPrograms.AsNoTracking().SingleOrDefaultAsync(x => x.ProgramId == id, ct) ?? throw new ApiErrorException(ApiErrorCode.NOT_FOUND); return new(p, await db.Courses.AsNoTracking().SingleAsync(x => x.CourseId == p.CourseId, ct), await db.Bands.AsNoTracking().SingleAsync(x => x.BandId == p.BandId, ct), await db.ScoreComponentConfigs.AsNoTracking().Where(x => x.ProgramId == id).OrderBy(x => x.ComponentId).ToListAsync(ct), await db.Classes.AnyAsync(x => x.ProgramId == id, ct)); }
}
