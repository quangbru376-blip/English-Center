namespace EnglishCenter.Api.Infrastructure.Academics;

using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Common.Replay;
using EnglishCenter.Api.Common.Security;
using EnglishCenter.Api.Common.Time;
using EnglishCenter.Api.Infrastructure.Replay;
using EnglishCenter.Api.Services.Academics;
using EnglishCenter.Api.Services.Replay;
using Microsoft.EntityFrameworkCore;

public sealed class SqlProgramExecutor(Func<EnglishCenterDbContext> factory, BusinessClock clock, int sqlSeconds, int lockMs)
{
    private readonly SqlAcademicTransactions transactions = new(factory, clock, sqlSeconds, lockMs);
    public Task<ProgramCommandResult> CommandAsync(VerifiedSessionToken token, AcademicInput input, CancellationToken ct) => transactions.Write((scope, c) => new ProgramService(new SqlProgramRepository(scope), new SqlReplayRepository(scope, lockMs), clock).CommandAsync(token, input, c), ct);
    public Task<ProgramDetail> DetailAsync(VerifiedSessionToken token, int id, CancellationToken ct) => transactions.Read(token, async (db, c) => ProgramProjection.Detail(await SqlProgramRepository.ReadState(db, id, c)), ct);
    public Task<ProgramActivationPreview> PreviewAsync(VerifiedSessionToken token, int id, CancellationToken ct) => transactions.Read(token, async (db, c) => { var state = await SqlProgramRepository.ReadState(db, id, c); var detail = ProgramProjection.Detail(state); var eligible = detail.ActivationManifest is not null; return new ProgramActivationPreview(ProgramProjection.Summary(state), detail.Validation, eligible, eligible ? [] : ["Trạng thái hoặc cấu hình chưa đủ điều kiện kích hoạt."], detail.ActivationManifest); }, ct);
    public Task<AcademicPage<ProgramSummary>> SearchAsync(VerifiedSessionToken token, IQueryCollection query, CancellationToken ct)
    {
        var filters = new AcademicQuery(query, "courseId", "bandId", "name", "status", "sort", "page", "pageSize"); var course = filters.Id("courseId"); var band = filters.Id("bandId"); var name = filters.Text("name", 150); var status = filters.Get("status"); if (status is not null) filters.Choice("status", "DRAFT", "DRAFT", "ACTIVE", "INACTIVE"); var sort = filters.Choice("sort", "NAME_ASC", "NAME_ASC", "NAME_DESC"); var page = filters.Number("page"); var size = filters.Number("pageSize", 20, 100);
        return transactions.Read(token, async (db, c) =>
        {
            var rows = db.TrainingPrograms.AsNoTracking(); if (course is { } courseId) rows = rows.Where(x => x.CourseId == courseId); if (band is { } bandId) rows = rows.Where(x => x.BandId == bandId); if (name is not null) rows = rows.Where(x => x.ProgramName.Contains(name)); if (status is not null) rows = rows.Where(x => x.Status == status);
            var count = await rows.LongCountAsync(c); var sorted = sort == "NAME_ASC" ? rows.OrderBy(x => x.ProgramName).ThenBy(x => x.ProgramId) : rows.OrderByDescending(x => x.ProgramName).ThenByDescending(x => x.ProgramId); var offset = (long)(page - 1) * size;
            var data = offset > int.MaxValue ? [] : await sorted.Include(x => x.Course).Include(x => x.Band).Skip((int)offset).Take(size).ToListAsync(c); var ids = data.Select(x => x.ProgramId).ToArray(); var frozen = await db.Classes.Where(x => ids.Contains(x.ProgramId)).Select(x => x.ProgramId).Distinct().ToListAsync(c);
            return new AcademicPage<ProgramSummary>(data.Select(x => ProgramProjection.Summary(new(x, x.Course!, x.Band!, [], frozen.Contains(x.ProgramId)))).ToArray(), page, size, count);
        }, ct);
    }
    public Task<SafeCommandCompletion> LookupAsync(VerifiedSessionToken token, string action, Guid request, CancellationToken ct) => transactions.Write(async (scope, c) =>
    {
        CanonicalAction selected; try { selected = CanonicalActionRegistry.ForCode(action); } catch (ArgumentException) { throw ApiErrorException.Validation("action"); }
        if (selected.ApiId is not (82 or 84 or 85 or 87 or 88 or 89)) throw new ApiErrorException(ApiErrorCode.FORBIDDEN);
        var markers = new SqlReplayRepository(scope, lockMs); await markers.AcquireAsync(ReplayService.RequestResource(token.AccountId, selected, request), c); var repository = new SqlProgramRepository(scope); await AcademicAuthority.RequireAsync(repository, token, clock, c);
        return await new ReplayService(markers, clock).LookupAsync(90, "AcademicAffairs", token.AccountId, action, request, async (stored, cancel) => { await AcademicAuthority.RequireAsync(repository, token, clock, cancel); await repository.StateAsync(stored.Target["programId"], cancel); return true; }, c);
    }, ct);
}
