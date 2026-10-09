namespace EnglishCenter.Api.Infrastructure.Academics;

using System.Text.Json;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Common.Replay;
using EnglishCenter.Api.Common.Security;
using EnglishCenter.Api.Common.Time;
using EnglishCenter.Api.Infrastructure.Replay;
using EnglishCenter.Api.Services.Academics;
using EnglishCenter.Api.Services.Replay;
using Microsoft.EntityFrameworkCore;

public sealed class SqlEntrySessionExecutor(Func<EnglishCenterDbContext> factory, BusinessClock clock, int sqlSeconds, int lockMs)
{
    private readonly SqlAcademicTransactions transactions = new(factory, clock, sqlSeconds, lockMs);
    public Task<SessionCommandResult> CommandAsync(VerifiedSessionToken token, AcademicInput input, CancellationToken ct) => transactions.Write((scope, c) => new EntrySessionService(new SqlEntrySessionRepository(scope), new SqlReplayRepository(scope, lockMs), clock).CommandAsync(token, input, c), ct);
    private async Task<AcademicSessionDetail> Detail(EnglishCenterDbContext db, int id, DateTimeOffset now, CancellationToken ct)
    { var state = await SqlEntrySessionRepository.ReadState(db, id, ct); return SessionProjection.Detail(state, now, await SqlEntrySessionRepository.FindConflicts(db, SessionWindow.From(state.Session), state.Session.GraderId, id, ct)); }
    public Task<AcademicSessionDetail> DetailAsync(VerifiedSessionToken token, int id, CancellationToken ct) => transactions.Read(token, (db, c) => Detail(db, id, clock.GetUtcNow(), c), ct);
    public Task<AcademicPage<AcademicSessionView>> SearchAsync(VerifiedSessionToken token, IQueryCollection query, CancellationToken ct)
    {
        var q = new AcademicQuery(query, "fromDate", "toDate", "effectiveStatus", "graderId", "sort", "page", "pageSize"); var from = q.Date("fromDate"); var to = q.Date("toDate"); if (from > to) throw ApiErrorException.Validation("toDate"); var status = q.Get("effectiveStatus"); if (status is not null) q.Choice("effectiveStatus", "CLOSED", "OPEN", "CLOSED", "CANCELLED"); var grader = q.Id("graderId"); var sort = q.Choice("sort", "START_ASC", "START_ASC", "START_DESC"); var page = q.Number("page"); var size = q.Number("pageSize", 20, 100);
        return transactions.Read(token, async (db, c) => { var now = clock.GetUtcNow(); var utc = now.UtcDateTime; var rows = db.EntryTestSessions.AsNoTracking(); if (from is { } f) { var lower = VietnamTime.ToUtc(f, TimeOnly.MinValue).UtcDateTime; rows = rows.Where(x => x.StartAt >= lower); } if (to is { } t) { var upper = VietnamTime.ToUtc(t, TimeOnly.MaxValue).UtcDateTime; rows = rows.Where(x => x.StartAt <= upper); } if (grader is { } g) rows = rows.Where(x => x.GraderId == g); if (status is not null) rows = rows.Where(x => (x.Status == "CANCELLED" ? "CANCELLED" : x.StartAt <= utc ? "CLOSED" : x.Status) == status); var count = await rows.LongCountAsync(c); var sorted = sort == "START_ASC" ? rows.OrderBy(x => x.StartAt).ThenBy(x => x.SessionId) : rows.OrderByDescending(x => x.StartAt).ThenByDescending(x => x.SessionId); var offset = (long)(page - 1) * size; var ids = offset > int.MaxValue ? [] : await sorted.Skip((int)offset).Take(size).Select(x => x.SessionId).ToArrayAsync(c); var result = await SqlEntrySessionSearch.Project(db, ids, now, c); return new AcademicPage<AcademicSessionView>(result, page, size, count); }, ct);
    }
    public Task<SessionCancellationPreview> CancellationAsync(VerifiedSessionToken token, int id, CancellationToken ct) => transactions.Read(token, async (db, c) =>
    {
        var now = clock.GetUtcNow(); var state = await SqlEntrySessionRepository.ReadState(db, id, c); var eligible = state.Session.Status != "CANCELLED" && state.Session.StartAt > now.UtcDateTime; var effects = new List<AcademicRosterRow>(); var page = 1; AcademicPage<AcademicRosterRow> chunk;
        do { chunk = await SqlSessionRoster.Read(db, id, token.AccountId, now, new(null, null, null, "SCHEDULED", "current", "BOOKED_ASC", page++, 100), c); effects.AddRange(chunk.Items); } while (effects.Count < chunk.TotalCount && chunk.Items.Count > 0);
        if (effects.Count != chunk.TotalCount) throw new InvalidOperationException("Incomplete cancellation effects."); return new SessionCancellationPreview(id, eligible, eligible ? [] : ["Ca đã bắt đầu hoặc đã hủy."], now.ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'"), effects, eligible ? SessionProjection.Manifest(state) : null);
    }, ct);
    public Task<GraderChangePreview> GraderPreviewAsync(VerifiedSessionToken token, int id, int grader, CancellationToken ct) => transactions.Read(token, async (db, c) =>
    {
        var now = clock.GetUtcNow(); var state = await SqlEntrySessionRepository.ReadState(db, id, c); var candidate = await db.Accounts.AsNoTracking().SingleOrDefaultAsync(x => x.AccountId == grader, c) ?? throw new ApiErrorException(ApiErrorCode.NOT_FOUND); var conflicts = await SqlEntrySessionRepository.FindConflicts(db, SessionWindow.From(state.Session), grader, id, c); var eligible = state.Session.Status != "CANCELLED" && state.Session.StartAt > now.UtcDateTime && !SessionProjection.HasResults(state) && grader != state.Session.GraderId && candidate.Status == "ACTIVE" && await db.Roles.AnyAsync(x => x.RoleId == candidate.RoleId && x.RoleCode == "Teacher", c) && conflicts.Count == 0;
        return new GraderChangePreview(id, new(state.Grader.AccountId, state.Grader.FullName), new(candidate.AccountId, candidate.FullName), eligible, eligible ? [] : ["Trạng thái, giáo viên hoặc lịch không phù hợp."], conflicts, now.ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'"), eligible ? SessionProjection.Manifest(state, candidate) : null);
    }, ct);
    public Task<GraderOptions> OptionsAsync(VerifiedSessionToken token, IQueryCollection query, CancellationToken ct)
    {
        var q = new AcademicQuery(query, "date", "startTime", "endTime", "sessionId", "page", "pageSize"); var window = SessionWindow.Parse(JsonSerializer.SerializeToElement(new { date = q.Get("date"), startTime = q.Get("startTime"), endTime = q.Get("endTime") })); var id = q.Id("sessionId"); var page = q.Number("page"); var size = q.Number("pageSize", 20, 100);
        return transactions.Read(token, async (db, c) => { var current = id is { } target ? (await SqlEntrySessionRepository.ReadState(db, target, c)).Grader : null; var busySessions = db.EntryTestSessions.Where(x => x.SessionId != (id ?? 0) && x.Status != "CANCELLED" && x.StartAt < window.EndAt && x.EndAt > window.StartAt).Select(x => x.GraderId); var busyClasses = db.Schedules.Where(x => x.SessionDate == window.Date && x.StartTime < window.EndTime && x.EndTime > window.StartTime && x.IsCurrent && (x.OccurrenceState == "PLANNED" || x.OccurrenceState == "HELD") && x.Class!.Status != "CANCELLED").Select(x => x.Class!.TeacherId); var teachers = db.Accounts.AsNoTracking().Where(x => x.Status == "ACTIVE" && db.Roles.Any(r => r.RoleId == x.RoleId && r.RoleCode == "Teacher") && !busySessions.Contains(x.AccountId) && !busyClasses.Contains(x.AccountId)); var count = await teachers.LongCountAsync(c); var offset = (long)(page - 1) * size; var data = offset > int.MaxValue ? [] : await teachers.OrderBy(x => x.AccountId).Skip((int)offset).Take(size).ToListAsync(c); GraderOption? selected = null; if (current is not null) { var ok = await teachers.AnyAsync(x => x.AccountId == current.AccountId, c); selected = new(current.AccountId, current.FullName, ok, ok ? [] : ["Giáo viên không hoạt động hoặc trùng lịch."]); } return new GraderOptions(selected, data.Select(x => new GraderOption(x.AccountId, x.FullName, true, [])).ToArray(), page, size, count); }, ct);
    }
    public Task<AcademicPage<AcademicRosterRow>> RosterAsync(VerifiedSessionToken token, int id, SessionRosterFilter filter, CancellationToken ct) => transactions.Read(token, async (db, c) => { await SqlEntrySessionRepository.ReadState(db, id, c); return await SqlSessionRoster.Read(db, id, token.AccountId, clock.GetUtcNow(), filter, c); }, ct);
    public Task<SafeCommandCompletion> LookupAsync(VerifiedSessionToken token, string action, Guid request, CancellationToken ct) => transactions.Write(async (scope, c) =>
    {
        CanonicalAction selected; try { selected = CanonicalActionRegistry.ForCode(action); } catch (ArgumentException) { throw ApiErrorException.Validation("action"); }
        if (selected.ApiId is not (55 or 57 or 58 or 59 or 61 or 64)) throw new ApiErrorException(ApiErrorCode.FORBIDDEN); var markers = new SqlReplayRepository(scope, lockMs); await markers.AcquireAsync(ReplayService.RequestResource(token.AccountId, selected, request), c); var repository = new SqlEntrySessionRepository(scope); await AcademicAuthority.RequireAsync(repository, token, clock, c); return await new ReplayService(markers, clock).LookupAsync(45, "AcademicAffairs", token.AccountId, action, request, async (stored, cancel) => { await AcademicAuthority.RequireAsync(repository, token, clock, cancel); await repository.StateAsync(stored.Target["sessionId"], cancel); return true; }, c);
    }, ct);
}
