namespace EnglishCenter.Api.Infrastructure.Academics;

using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Domain.Admissions;
using EnglishCenter.Api.Domain.Identity;
using EnglishCenter.Api.Infrastructure.Identity;
using EnglishCenter.Api.Services.Academics;
using Microsoft.EntityFrameworkCore;

public sealed class SqlEntrySessionRepository(SqlAuthDeadlineScope scope) : SqlAcademicRepository(scope), IEntrySessionRepository
{
    public async Task<int[]> LinkedRegistrationsAsync(int id, CancellationToken ct) => await (from b in Db.EntryTestBookings where b.SessionId == id && b.Status == "SCHEDULED" join a in Db.EntryTestAttempts on b.AttemptId equals a.AttemptId select a.RegistrationId).Distinct().ToArrayAsync(ct);
    public async Task<int?> DiscoverGraderAsync(int id, CancellationToken ct) => await Db.EntryTestSessions.AsNoTracking().Where(x => x.SessionId == id).Select(x => (int?)x.GraderId).SingleOrDefaultAsync(ct);
    public async Task<SessionState> StateAsync(int id, CancellationToken ct)
    {
        // Discover under business gates, then lock T09 -> T10 -> T11 -> T12.
        var attemptIds = await Db.EntryTestBookings.AsNoTracking().Where(x => x.SessionId == id).Select(x => x.AttemptId).Distinct().ToArrayAsync(ct);
        var registrationIds = await Db.EntryTestAttempts.AsNoTracking().Where(x => attemptIds.Contains(x.AttemptId)).Select(x => x.RegistrationId).Distinct().ToArrayAsync(ct);
        var registrations = new List<Registration>(); foreach (var key in registrationIds.Order()) registrations.Add(await Db.Registrations.FromSqlInterpolated($"SELECT * FROM dbo.Registration WITH(UPDLOCK,HOLDLOCK) WHERE registration_id={key}").SingleAsync(ct));
        var session = await Db.EntryTestSessions.FromSqlInterpolated($"SELECT * FROM dbo.EntryTestSession WITH(UPDLOCK,HOLDLOCK) WHERE session_id={id}").SingleOrDefaultAsync(ct) ?? throw new ApiErrorException(ApiErrorCode.NOT_FOUND);
        var grader = await Db.Accounts.SingleAsync(x => x.AccountId == session.GraderId, ct); var role = await Db.Roles.AnyAsync(x => x.RoleId == grader.RoleId && x.RoleCode == "Teacher", ct);
        var attempts = new List<EntryTestAttempt>(); foreach (var key in attemptIds.Order()) attempts.Add(await Db.EntryTestAttempts.FromSqlInterpolated($"SELECT * FROM dbo.EntryTestAttempt WITH(UPDLOCK,HOLDLOCK) WHERE attempt_id={key}").SingleAsync(ct));
        var bookings = await Db.EntryTestBookings.FromSqlInterpolated($"SELECT * FROM dbo.EntryTestBooking WITH(UPDLOCK,HOLDLOCK) WHERE session_id={id}").OrderBy(x => x.BookingId).ToListAsync(ct);
        if (bookings.Any(x => !attemptIds.Contains(x.AttemptId))) throw AcademicGuard.Conflict();
        return new(session, grader, role, bookings, attempts, registrations);
    }
    public async Task<Account> TeacherAsync(int id, CancellationToken ct)
    { var account = await Db.Accounts.SingleOrDefaultAsync(x => x.AccountId == id, ct) ?? throw new ApiErrorException(ApiErrorCode.NOT_FOUND); return account; }
    public Task<bool> TeacherRoleAsync(int role, CancellationToken ct) => Db.Roles.AnyAsync(x => x.RoleId == role && x.RoleCode == "Teacher", ct);
    public async Task InsertAsync(EntryTestSession session, CancellationToken ct) { Db.EntryTestSessions.Add(session); await SaveAsync(ct); }
    public Task<IReadOnlyList<SafeScheduleConflict>> ConflictsAsync(SessionWindow window, int grader, int excluded, CancellationToken ct) => FindConflicts(Db, window, grader, excluded, ct);
    public static async Task<IReadOnlyList<SafeScheduleConflict>> FindConflicts(EnglishCenterDbContext db, SessionWindow window, int grader, int excluded, CancellationToken ct)
    {
        var start = window.StartAt; var end = window.EndAt; var conflicts = new List<SafeScheduleConflict>();
        var sessions = db.EntryTestSessions.Where(x => x.SessionId != excluded && x.Status != "CANCELLED" && x.StartAt < end && x.EndAt > start);
        var schedules = db.Schedules.Where(x => x.SessionDate == window.Date && x.StartTime < window.EndTime && x.EndTime > window.StartTime && x.IsCurrent && (x.OccurrenceState == "PLANNED" || x.OccurrenceState == "HELD") && x.Class!.Status != "CANCELLED");
        if (window.Room.Length > 0 && (await sessions.AnyAsync(x => x.Room == window.Room, ct) || await schedules.AnyAsync(x => x.Room == window.Room, ct))) conflicts.Add(new("ROOM", "Phòng đã có lịch trong khoảng giờ này."));
        if (await sessions.AnyAsync(x => x.GraderId == grader, ct) || await schedules.AnyAsync(x => x.Class!.TeacherId == grader, ct)) conflicts.Add(new("GRADER", "Giáo viên đã có lịch trong khoảng giờ này."));
        return conflicts;
    }
    public static async Task<SessionState> ReadState(EnglishCenterDbContext db, int id, CancellationToken ct)
    { var session = await db.EntryTestSessions.AsNoTracking().SingleOrDefaultAsync(x => x.SessionId == id, ct) ?? throw new ApiErrorException(ApiErrorCode.NOT_FOUND); var grader = await db.Accounts.AsNoTracking().SingleAsync(x => x.AccountId == session.GraderId, ct); var bookings = await db.EntryTestBookings.AsNoTracking().Where(x => x.SessionId == id).OrderBy(x => x.BookingId).ToListAsync(ct); var ids = bookings.Select(x => x.AttemptId).Distinct().ToArray(); var attempts = await db.EntryTestAttempts.AsNoTracking().Where(x => ids.Contains(x.AttemptId)).ToListAsync(ct); var regs = attempts.Select(x => x.RegistrationId).Distinct().ToArray(); return new(session, grader, await db.Roles.AnyAsync(x => x.RoleId == grader.RoleId && x.RoleCode == "Teacher", ct), bookings, attempts, await db.Registrations.AsNoTracking().Where(x => regs.Contains(x.RegistrationId)).ToListAsync(ct)); }
}
