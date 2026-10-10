namespace EnglishCenter.Api.Infrastructure.Enrollment;

using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Common.Replay;
using EnglishCenter.Api.Common.Security;
using EnglishCenter.Api.Common.Time;
using EnglishCenter.Api.Domain.Admissions;
using EnglishCenter.Api.Infrastructure.Academics;
using EnglishCenter.Api.Infrastructure.Replay;
using EnglishCenter.Api.Services.Academics;
using EnglishCenter.Api.Services.Enrollment;
using EnglishCenter.Api.Services.Replay;
using Microsoft.EntityFrameworkCore;

public sealed class SqlTeacherEntryReader(Func<EnglishCenterDbContext> factory, BusinessClock clock, int sqlSeconds, int lockMs)
{
    private readonly SqlEnrollmentTransactions transactions = new(factory, clock, sqlSeconds, lockMs);
    private static async Task<EntryTestSession> Assigned(EnglishCenterDbContext db, int actor, int session, CancellationToken ct) =>
        await db.EntryTestSessions.AsNoTracking().SingleOrDefaultAsync(x => x.SessionId == session && x.GraderId == actor, ct) ?? throw new ApiErrorException(ApiErrorCode.NOT_FOUND);

    private async Task FinalSessions(VerifiedSessionToken token, int[] ids, CancellationToken ct) => await transactions.Write(async (scope, c) =>
    {
        var authority = await EnrollmentAuthority.RequireAsync(new SqlEnrollmentRepository(scope), token, clock, "Teacher", c);
        var held = await scope.Context.EntryTestSessions.FromSqlInterpolated($"SELECT * FROM dbo.EntryTestSession WITH(UPDLOCK,HOLDLOCK) WHERE grader_id={token.AccountId}")
            .Where(x => ids.Contains(x.SessionId)).OrderBy(x => x.SessionId).Select(x => x.SessionId).ToArrayAsync(c);
        if (!held.SequenceEqual(ids.Distinct().Order())) throw new ApiErrorException(ApiErrorCode.NOT_FOUND);
        if (!clock.IsBefore(authority.EffectiveDeadline)) throw new ApiErrorException(ApiErrorCode.AUTHENTICATION_REQUIRED);
        return true;
    }, ct);

    public async Task<TeacherSessionDetail> DetailAsync(VerifiedSessionToken token, int id, CancellationToken ct)
    {
        var result = await transactions.Read(token, "Teacher", async (db, c) =>
        {
            var s = await Assigned(db, token.AccountId, id, c); var b = await db.EntryTestBookings.AsNoTracking().Where(x => x.SessionId == id).ToArrayAsync(c);
            var v = Project(s, b, clock.GetUtcNow());
            return new TeacherSessionDetail(v.Id, v.Date, v.StartTime, v.EndTime, v.StartAt, v.EndAt, v.Room, v.Status, v.EffectiveStatus, v.TaskCounts, v.Version,
                s.CancelledAt is { } at ? new { at = AcademicGuard.Instant(at), reason = s.CancellationReason } : null);
        }, ct);
        await FinalSessions(token, [id], ct); return result;
    }

    public async Task<AcademicPage<TeacherSessionView>> SearchAsync(VerifiedSessionToken token, IQueryCollection query, CancellationToken ct)
    {
        var q = new AcademicQuery(query, "fromDate", "toDate", "effectiveStatus", "sort", "page", "pageSize");
        var from = q.Date("fromDate"); var to = q.Date("toDate"); if (from > to) throw ApiErrorException.Validation("toDate");
        var status = q.Get("effectiveStatus"); if (status is not null) q.Choice("effectiveStatus", "CLOSED", "OPEN", "CLOSED", "CANCELLED");
        var sort = q.Choice("sort", "START_ASC", "START_ASC", "START_DESC"); var page = q.Number("page"); var size = q.Number("pageSize", 20, 100);
        var result = await transactions.Read(token, "Teacher", async (db, c) =>
        {
            var now = clock.GetUtcNow(); var utc = now.UtcDateTime; var rows = db.EntryTestSessions.AsNoTracking().Where(x => x.GraderId == token.AccountId);
            if (from is { } f) { var lower = VietnamTime.ToUtc(f, TimeOnly.MinValue).UtcDateTime; rows = rows.Where(x => x.StartAt >= lower); }
            if (to is { } t) { var upper = VietnamTime.ToUtc(t, TimeOnly.MaxValue).UtcDateTime; rows = rows.Where(x => x.StartAt <= upper); }
            if (status is not null) rows = rows.Where(x => (x.Status == "CANCELLED" ? "CANCELLED" : x.StartAt <= utc ? "CLOSED" : x.Status) == status);
            var count = await rows.LongCountAsync(c); var sorted = sort == "START_ASC" ? rows.OrderBy(x => x.StartAt).ThenBy(x => x.SessionId) : rows.OrderByDescending(x => x.StartAt).ThenByDescending(x => x.SessionId);
            var offset = (long)(page - 1) * size; var sessions = offset > int.MaxValue ? [] : await sorted.Skip((int)offset).Take(size).ToArrayAsync(c);
            var ids = sessions.Select(x => x.SessionId).ToArray(); var bookings = await db.EntryTestBookings.AsNoTracking().Where(x => ids.Contains(x.SessionId)).ToArrayAsync(c);
            return new AcademicPage<TeacherSessionView>(sessions.Select(s => Project(s, bookings.Where(b => b.SessionId == s.SessionId), now)).ToArray(), page, size, count);
        }, ct);
        await FinalSessions(token, result.Items.Select(x => x.Id).ToArray(), ct); return result;
    }

    public async Task<AcademicPage<TeacherRosterRow>> RosterAsync(VerifiedSessionToken token, int id, SessionRosterFilter filter, CancellationToken ct)
    {
        var result = await transactions.Read(token, "Teacher", async (db, c) =>
        { await Assigned(db, token.AccountId, id, c); return await SqlTeacherRoster.Read(db, id, token.AccountId, clock.GetUtcNow(), filter, c); }, ct);
        await FinalSessions(token, [id], ct); return result;
    }

    public async Task<TeacherTaskDetail> TaskAsync(VerifiedSessionToken token, int booking, CancellationToken ct)
    {
        var result = await transactions.Read(token, "Teacher", async (db, c) =>
        {
            var row = await (from b in db.EntryTestBookings
                             join a in db.EntryTestAttempts on b.AttemptId equals a.AttemptId
                             join s in db.EntryTestSessions on b.SessionId equals s.SessionId
                             join r in db.Registrations on a.RegistrationId equals r.RegistrationId
                             where b.BookingId == booking && s.GraderId == token.AccountId
                             select new { r.StudentId, r.RegistrationId }).SingleOrDefaultAsync(c) ?? throw new ApiErrorException(ApiErrorCode.NOT_FOUND);
            var set = await SqlEnrollmentRepository.ReadSet(db, row.StudentId, [row.RegistrationId], c, false); var state = new RegistrationState(set, set.Registrations.Single());
            return TeacherTaskProjection.Detail(state, state.Booking(booking), token.AccountId, clock.GetUtcNow());
        }, ct);
        await FinalSessions(token, [result.Booking.SessionId], ct); return result;
    }

    public Task<SafeCommandCompletion> LookupAsync(VerifiedSessionToken token, string action, Guid request, CancellationToken ct) => transactions.Write(async (scope, c) =>
    {
        CanonicalAction selected; try { selected = CanonicalActionRegistry.ForCode(action); } catch (ArgumentException) { throw ApiErrorException.Validation("action"); }
        if (selected.ApiId is not (70 or 71 or 72)) throw new ApiErrorException(ApiErrorCode.FORBIDDEN);
        var markers = new SqlReplayRepository(scope, lockMs); await markers.AcquireAsync(ReplayService.RequestResource(token.AccountId, selected, request), c);
        var repository = new SqlEnrollmentRepository(scope); await repository.PrepareAccountsAsync(token, [], c);
        await EnrollmentAuthority.RequireAsync(repository, token, clock, "Teacher", c);
        var result = await new ReplayService(markers, clock).LookupAsync(73, "Teacher", token.AccountId, action, request, async (stored, cancel) =>
        {
            var target = await repository.RequireTeacherAsync(token.AccountId, stored.Target["bookingId"], false, cancel);
            return target.Registration == stored.Target["registrationId"] && await scope.Context.EntryTestBookings.AnyAsync(x => x.BookingId == target.Booking && x.AttemptId == stored.Target["attemptId"], cancel);
        }, c);
        var authority = await EnrollmentAuthority.RequireAsync(repository, token, clock, "Teacher", c);
        var bookingSession = await scope.Context.EntryTestBookings.Where(x => x.BookingId == result.Target["bookingId"]).Select(x => x.SessionId).SingleAsync(c);
        if (!await scope.Context.EntryTestSessions.FromSqlInterpolated($"SELECT * FROM dbo.EntryTestSession WITH(UPDLOCK,HOLDLOCK) WHERE session_id={bookingSession} AND grader_id={token.AccountId}").AnyAsync(c)) throw new ApiErrorException(ApiErrorCode.RESULT_NOT_CONFIRMED);
        if (!clock.IsBefore(authority.EffectiveDeadline)) throw new ApiErrorException(ApiErrorCode.AUTHENTICATION_REQUIRED);
        return result;
    }, ct);

    private static TeacherSessionView Project(EntryTestSession s, IEnumerable<EntryTestBooking> bookings, DateTimeOffset now)
    {
        var window = SessionWindow.From(s); var b = bookings.ToArray();
        return new(s.SessionId, window.Date.ToString("yyyy-MM-dd"), window.StartTime.ToString("HH:mm"), window.EndTime.ToString("HH:mm"), AcademicGuard.Instant(s.StartAt), AcademicGuard.Instant(s.EndAt), s.Room,
            s.Status, s.Status == "CANCELLED" ? "CANCELLED" : s.StartAt <= now.UtcDateTime ? "CLOSED" : s.Status, new EntryTaskCounts(b.Count(x => x.Status == "SCHEDULED"), b.Count(x => x.Status == "COMPLETED"), b.Count(x => x.Status == "ABSENT"), b.Count(x => x.Status == "CANCELLED")), EnrollmentProjection.Rv(s.RowVersion));
    }
}
