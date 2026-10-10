namespace EnglishCenter.Api.Infrastructure.Enrollment;

using System.Globalization;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Common.Replay;
using EnglishCenter.Api.Common.Security;
using EnglishCenter.Api.Common.Time;
using EnglishCenter.Api.Infrastructure.Replay;
using EnglishCenter.Api.Services.Academics;
using EnglishCenter.Api.Services.Enrollment;
using EnglishCenter.Api.Services.Replay;
using Microsoft.EntityFrameworkCore;

public sealed class SqlStudentEnrollmentReader(Func<EnglishCenterDbContext> factory, BusinessClock clock, int sqlSeconds, int lockMs)
{
    private readonly SqlEnrollmentTransactions transactions = new(factory, clock, sqlSeconds, lockMs);
    private static async Task<int> Student(EnglishCenterDbContext db, int actor, CancellationToken ct) =>
        await db.StudentProfiles.AsNoTracking().Where(x => x.AccountId == actor).Select(x => (int?)x.StudentId).SingleOrDefaultAsync(ct) ?? throw new ApiErrorException(ApiErrorCode.NOT_FOUND);
    private static async Task<RegistrationState> Own(EnglishCenterDbContext db, int actor, int registration, CancellationToken ct)
    {
        var student = await Student(db, actor, ct);
        if (!await db.Registrations.AnyAsync(x => x.RegistrationId == registration && x.StudentId == student, ct)) throw new ApiErrorException(ApiErrorCode.NOT_FOUND);
        var set = await SqlEnrollmentRepository.ReadSet(db, student, [registration], ct); return new(set, set.Registrations.Single());
    }
    private Task<T> Read<T>(VerifiedSessionToken token, Func<EnglishCenterDbContext, CancellationToken, Task<T>> work, CancellationToken ct) => transactions.Read(token, "Student", work, ct);
    private static CourseChoice Course(Domain.Academics.Course c) => new(c.CourseId, c.CourseCode, c.CourseName, c.BasePrice.ToString("0", CultureInfo.InvariantCulture), c.Status);

    public Task<AcademicPage<CourseChoice>> CoursesAsync(VerifiedSessionToken token, IQueryCollection query, CancellationToken ct)
    {
        var q = new AcademicQuery(query, "page", "pageSize"); var page = q.Number("page"); var size = q.Number("pageSize", 20, 100);
        return Read(token, async (db, c) =>
        {
            await Student(db, token.AccountId, c); var rows = db.Courses.AsNoTracking().Where(x => x.Status == "ACTIVE"); var count = await rows.LongCountAsync(c); var offset = (long)(page - 1) * size;
            var data = offset > int.MaxValue ? [] : await rows.OrderBy(x => x.CourseCode).ThenBy(x => x.CourseId).Skip((int)offset).Take(size).ToArrayAsync(c);
            return new AcademicPage<CourseChoice>(data.Select(Course).ToArray(), page, size, count);
        }, ct);
    }
    public Task<CourseChoice> CourseAsync(VerifiedSessionToken token, int id, CancellationToken ct) => Read(token, async (db, c) =>
    { await Student(db, token.AccountId, c); return Course(await db.Courses.AsNoTracking().SingleOrDefaultAsync(x => x.CourseId == id && x.Status == "ACTIVE", c) ?? throw new ApiErrorException(ApiErrorCode.NOT_FOUND)); }, ct);

    private async Task<AcademicPage<T>> Registrations<T>(VerifiedSessionToken token, IQueryCollection query, bool home, Func<RegistrationState, DateTimeOffset, T> project, CancellationToken ct)
    {
        var q = home ? new AcademicQuery(query, "page", "pageSize") : new AcademicQuery(query, "courseId", "status", "page", "pageSize");
        var page = q.Number("page"); var size = q.Number("pageSize", 20, 100); var course = home ? null : q.Id("courseId"); var status = home ? null : q.Get("status");
        if (status is not null) q.Choice("status", "PENDING", "PENDING", "TESTING", "BAND_ASSIGNED", "CONFIRMED", "ADMITTED", "REJECTED", "CANCELLED");
        return await Read(token, async (db, c) =>
        {
            var student = await Student(db, token.AccountId, c); var rows = db.Registrations.AsNoTracking().Where(x => x.StudentId == student);
            if (course is { } id) rows = rows.Where(x => x.CourseId == id); if (status is not null) rows = rows.Where(x => x.Status == status);
            var count = await rows.LongCountAsync(c); var offset = (long)(page - 1) * size;
            var ids = offset > int.MaxValue ? [] : await rows.OrderByDescending(x => x.CreatedAt).ThenByDescending(x => x.RegistrationId).Skip((int)offset).Take(size).Select(x => x.RegistrationId).ToArrayAsync(c);
            var set = await SqlEnrollmentRepository.ReadSet(db, student, ids, c); var now = clock.GetUtcNow();
            return new AcademicPage<T>(ids.Select(id => project(new(set, set.Registrations.Single(x => x.RegistrationId == id)), now)).ToArray(), page, size, count);
        }, ct);
    }
    public Task<AcademicPage<OwnRegistrationSummary>> RegistrationsAsync(VerifiedSessionToken token, IQueryCollection query, CancellationToken ct) => Registrations(token, query, false, (s, _) => EnrollmentProjection.Summary(s), ct);
    public Task<AcademicPage<StudentHomeCard>> HomeAsync(VerifiedSessionToken token, IQueryCollection query, CancellationToken ct) => Registrations(token, query, true, StudentHomeProjection.Home, ct);
    public Task<OwnRegistrationDetail> DetailAsync(VerifiedSessionToken token, int id, CancellationToken ct) => Read(token, async (db, c) => EnrollmentProjection.Detail(await Own(db, token.AccountId, id, c), clock.GetUtcNow()), ct);
    public Task<object> ResultsAsync(VerifiedSessionToken token, int id, CancellationToken ct) => Read<object>(token, async (db, c) =>
    { var s = await Own(db, token.AccountId, id, c); var entry = EnrollmentProjection.Entry(s, clock.GetUtcNow()); return new { registrationId = id, registrationStatus = s.Registration.Status, registrationVersion = EnrollmentProjection.Rv(s.Registration.RowVersion), entry.Attempts, entry.BandAssignment, entry.BestAttemptId }; }, ct);

    public Task<OwnCancellationPreview> CancellationAsync(VerifiedSessionToken token, int id, CancellationToken ct) => Read(token, async (db, c) =>
    {
        var s = await Own(db, token.AccountId, id, c); var now = clock.GetUtcNow(); var eligible = EnrollmentProjection.CanCancel(s);
        return new OwnCancellationPreview(eligible, eligible ? [] : ["Trạng thái hồ sơ, học phí hoặc lớp đã xếp chưa cho phép hủy."], AcademicGuard.Instant(now.UtcDateTime),
            new { futureBookings = EnrollmentProjection.Future(s, now).Select(b => EnrollmentProjection.Booking(s, b, now)).ToArray(), invoice = EnrollmentProjection.Invoice(s, now) }, eligible ? EnrollmentProjection.CancellationManifest(s, now) : null);
    }, ct);
    public Task<OwnConfirmationPreview> ConfirmationAsync(VerifiedSessionToken token, int id, CancellationToken ct) => Read(token, async (db, c) =>
    {
        var s = await Own(db, token.AccountId, id, c); var now = clock.GetUtcNow(); var eligible = EnrollmentProjection.CanConfirm(s);
        if (eligible)
        {
            var best = TeacherTaskProjection.BestCompleted(s); var band = await db.Database.SqlQuery<int>($"SELECT bandId AS Value FROM dbo.ufn_BandForScore({best.Score!.Value})").ToArrayAsync(c);
            eligible = best.AttemptId == s.Band!.BestAttemptId && band.Length == 1 && band[0] == s.Band.BandId;
        }
        return new OwnConfirmationPreview(eligible, eligible ? [] : ["Cần kết quả hợp lệ và hoàn tất mọi lượt thi đang chờ."], AcademicGuard.Instant(now.UtcDateTime), EnrollmentProjection.Entry(s, now), eligible ? EnrollmentProjection.ConfirmationManifest(s) : null);
    }, ct);

    public Task<EntryOptions> OptionsAsync(VerifiedSessionToken token, int id, IQueryCollection query, CancellationToken ct)
    {
        var q = new AcademicQuery(query, "fromDate", "toDate", "bookingId", "page", "pageSize"); var from = q.Date("fromDate"); var to = q.Date("toDate"); if (from > to) throw ApiErrorException.Validation("toDate"); var bookingId = q.Id("bookingId"); var page = q.Number("page"); var size = q.Number("pageSize", 20, 100);
        return Read(token, async (db, c) =>
        {
            var state = await Own(db, token.AccountId, id, c);
            var source = bookingId is { } selected ? state.Bookings.SingleOrDefault(x => x.BookingId == selected) ?? throw new ApiErrorException(ApiErrorCode.NOT_FOUND) : null;
            var context = source is null ? EnrollmentProjection.BookAttempt(state) : EnrollmentProjection.RescheduleAttempt(state, source, clock.GetUtcNow());
            if (context is null) return new EntryOptions(id, EnrollmentProjection.Rv(state.Registration.RowVersion), false, ["Chưa thể đặt lượt thi mới trong trạng thái hiện tại."], null, [], page, size, 0);
            var now = clock.GetUtcNow(); var utc = now.UtcDateTime;
            var rows = db.EntryTestSessions.AsNoTracking().Where(s => s.Status == "OPEN" && s.StartAt > utc && db.Accounts.Any(a => a.AccountId == s.GraderId && a.Status == "ACTIVE" && db.Roles.Any(r => r.RoleId == a.RoleId && r.RoleCode == "Teacher")));
            if (source is not null) rows = rows.Where(x => x.SessionId != source.SessionId);
            if (from is { } f) { var lower = VietnamTime.ToUtc(f, TimeOnly.MinValue).UtcDateTime; rows = rows.Where(x => x.StartAt >= lower); }
            if (to is { } t) { var upper = VietnamTime.ToUtc(t, TimeOnly.MaxValue).UtcDateTime; rows = rows.Where(x => x.StartAt <= upper); }
            rows = rows.Where(s => db.EntryTestBookings.Count(b => b.SessionId == s.SessionId && b.Status == "SCHEDULED") < s.Capacity);
            var own = await SqlEnrollmentRepository.ReadSet(db, state.Registration.StudentId, null, c);
            var intervals = own.Bookings.Where(x => x.Status == "SCHEDULED" && x.BookingId != bookingId).Select(b => own.Sessions.Single(s => s.SessionId == b.SessionId)).Where(s => s.Status != "CANCELLED").Select(s => (s.StartAt, s.EndAt)).ToList();
            foreach (var s in own.Schedules.Where(x => x.IsCurrent && x.OccurrenceState is "PLANNED" or "HELD" && own.Classes.Single(a => a.ClassId == x.ClassId).Status != "CANCELLED")) intervals.Add((StudentHomeProjection.Utc(s.SessionDate, s.StartTime), StudentHomeProjection.Utc(s.SessionDate, s.EndTime)));
            foreach (var (start, end) in intervals) { var lower = start; var upper = end; rows = rows.Where(x => !(x.StartAt < upper && lower < x.EndAt)); }
            var count = await rows.LongCountAsync(c); var offset = (long)(page - 1) * size;
            var items = offset > int.MaxValue ? [] : await rows.OrderBy(x => x.StartAt).ThenBy(x => x.SessionId).Skip((int)offset).Take(size)
                .Select(s => new { Session = s, Available = s.Capacity - db.EntryTestBookings.Count(b => b.SessionId == s.SessionId && b.Status == "SCHEDULED") }).ToArrayAsync(c);
            return new EntryOptions(id, EnrollmentProjection.Rv(state.Registration.RowVersion), true, [], context, items.Select(x => { var s = EnrollmentProjection.Session(x.Session, now); return new EntryOption(s.Id, s.StartAt, s.EndAt, s.Room, s.Status, s.EffectiveStatus, s.Version, x.Available); }).ToArray(), page, size, count);
        }, ct);
    }

    public Task<AcademicPage<OwnBookingView>> CalendarAsync(VerifiedSessionToken token, IQueryCollection query, CancellationToken ct)
    {
        var q = new AcademicQuery(query, "fromDate", "toDate", "registrationId", "view", "page", "pageSize"); var from = q.Date("fromDate"); var to = q.Date("toDate"); if (from > to) throw ApiErrorException.Validation("toDate"); var id = q.Id("registrationId"); var view = q.Choice("view", "current", "current", "history"); var page = q.Number("page"); var size = q.Number("pageSize", 20, 100);
        return Read(token, async (db, c) =>
        {
            var student = await Student(db, token.AccountId, c); if (id is { } own) await Own(db, token.AccountId, own, c);
            var rows = from b in db.EntryTestBookings
                       join a in db.EntryTestAttempts on b.AttemptId equals a.AttemptId
                       join r in db.Registrations on a.RegistrationId equals r.RegistrationId
                       join s in db.EntryTestSessions on b.SessionId equals s.SessionId
                       where r.StudentId == student && (id == null || r.RegistrationId == id) && (view == "history" || b.Status == "SCHEDULED")
                       select new { b.BookingId, r.RegistrationId, s.StartAt };
            if (from is { } f) { var lower = VietnamTime.ToUtc(f, TimeOnly.MinValue).UtcDateTime; rows = rows.Where(x => x.StartAt >= lower); }
            if (to is { } t) { var upper = VietnamTime.ToUtc(t, TimeOnly.MaxValue).UtcDateTime; rows = rows.Where(x => x.StartAt <= upper); }
            var count = await rows.LongCountAsync(c); var offset = (long)(page - 1) * size; var data = offset > int.MaxValue ? [] : await rows.OrderBy(x => x.StartAt).ThenBy(x => x.BookingId).Skip((int)offset).Take(size).ToArrayAsync(c);
            var set = await SqlEnrollmentRepository.ReadSet(db, student, data.Select(x => x.RegistrationId).Distinct().ToArray(), c); var now = clock.GetUtcNow();
            return new AcademicPage<OwnBookingView>(data.Select(x => { var state = new RegistrationState(set, set.Registrations.Single(r => r.RegistrationId == x.RegistrationId)); return EnrollmentProjection.Booking(state, state.Booking(x.BookingId), now); }).ToArray(), page, size, count);
        }, ct);
    }

    public Task<SafeCommandCompletion> LookupAsync(VerifiedSessionToken token, string action, Guid request, CancellationToken ct) => transactions.Write(async (scope, c) =>
    {
        CanonicalAction selected; try { selected = CanonicalActionRegistry.ForCode(action); } catch (ArgumentException) { throw ApiErrorException.Validation("action"); }
        if (selected.ApiId is not (25 or 28 or 30 or 31 or 32 or 36)) throw new ApiErrorException(ApiErrorCode.FORBIDDEN);
        var markers = new SqlReplayRepository(scope, lockMs); await markers.AcquireAsync(ReplayService.RequestResource(token.AccountId, selected, request), c);
        var repository = new SqlEnrollmentRepository(scope); await repository.PrepareAccountsAsync(token, [], c); await EnrollmentAuthority.RequireAsync(repository, token, clock, "Student", c);
        var result = await new ReplayService(markers, clock).LookupAsync(37, "Student", token.AccountId, action, request, async (stored, cancel) =>
        {
            await repository.RequireOwnRegistrationAsync(token.AccountId, stored.Target["registrationId"], null, cancel);
            foreach (var key in new[] { "bookingId", "oldBookingId", "newBookingId" }) if (stored.Target.TryGetValue(key, out var booking)) await repository.RequireOwnRegistrationAsync(token.AccountId, stored.Target["registrationId"], booking, cancel);
            return true;
        }, c);
        var authority = await EnrollmentAuthority.RequireAsync(repository, token, clock, "Student", c);
        if (!clock.IsBefore(authority.EffectiveDeadline)) throw new ApiErrorException(ApiErrorCode.AUTHENTICATION_REQUIRED); return result;
    }, ct);
}
