namespace EnglishCenter.Api.Infrastructure.Enrollment;

using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Domain.Academics;
using EnglishCenter.Api.Domain.Admissions;
using EnglishCenter.Api.Infrastructure.Academics;
using EnglishCenter.Api.Infrastructure.Identity;
using EnglishCenter.Api.Services.Academics;
using EnglishCenter.Api.Services.Enrollment;
using Microsoft.EntityFrameworkCore;

public sealed partial class SqlEnrollmentRepository(SqlAuthDeadlineScope scope) : SqlAcademicRepository(scope)
{
    public async Task<int> StudentAccountAsync(int registration, CancellationToken ct) =>
        await (from r in Db.Registrations
               join s in Db.StudentProfiles on r.StudentId equals s.StudentId
               where r.RegistrationId == registration
               select (int?)s.AccountId).SingleOrDefaultAsync(ct) ?? throw new ApiErrorException(ApiErrorCode.NOT_FOUND);
    public async Task<int> OwnStudentAsync(int actor, CancellationToken ct) =>
        await Db.StudentProfiles.AsNoTracking().Where(x => x.AccountId == actor).Select(x => (int?)x.StudentId).SingleOrDefaultAsync(ct)
            ?? throw new ApiErrorException(ApiErrorCode.NOT_FOUND);

    public async Task RequireOwnRegistrationAsync(int actor, int registration, int? booking, CancellationToken ct)
    {
        var own = await (from r in Db.Registrations
                         join s in Db.StudentProfiles on r.StudentId equals s.StudentId
                         where r.RegistrationId == registration && s.AccountId == actor
                         select r.RegistrationId).AnyAsync(ct);
        if (!own || booking is { } id && !await (from b in Db.EntryTestBookings
                                                 join a in Db.EntryTestAttempts on b.AttemptId equals a.AttemptId
                                                 where b.BookingId == id && a.RegistrationId == registration
                                                 select b.BookingId).AnyAsync(ct)) throw new ApiErrorException(ApiErrorCode.NOT_FOUND);
    }

    public async Task<(int Registration, int Booking)> RequireTeacherAsync(int actor, int target, bool correction, CancellationToken ct)
    {
        var rows = from b in Db.EntryTestBookings
                   join a in Db.EntryTestAttempts on b.AttemptId equals a.AttemptId
                   join s in Db.EntryTestSessions on b.SessionId equals s.SessionId
                   where s.GraderId == actor && (!correction ? b.BookingId == target : a.AttemptId == target && a.CompletedBookingId == b.BookingId && a.GradedBy == actor)
                   select new { a.RegistrationId, b.BookingId };
        var row = await rows.SingleOrDefaultAsync(ct) ?? throw new ApiErrorException(ApiErrorCode.NOT_FOUND);
        return (row.RegistrationId, row.BookingId);
    }

    // All children are fetched in bounded batches, not one query per summary row.
    public static async Task<EnrollmentSet> ReadSet(EnglishCenterDbContext db, int student, int[]? registrations, CancellationToken ct, bool ownRelations = true)
    {
        var profile = await db.StudentProfiles.AsNoTracking().SingleOrDefaultAsync(x => x.StudentId == student, ct) ?? throw new ApiErrorException(ApiErrorCode.NOT_FOUND);
        var contact = await db.Accounts.AsNoTracking().SingleAsync(x => x.AccountId == profile.AccountId, ct);
        var regs = await db.Registrations.AsNoTracking().Where(x => x.StudentId == student && (registrations == null || registrations.Contains(x.RegistrationId))).ToArrayAsync(ct);
        var ids = regs.Select(x => x.RegistrationId).ToArray(); var courseIds = regs.Select(x => x.CourseId).Distinct().ToArray();
        var attempts = await db.EntryTestAttempts.AsNoTracking().Where(x => ids.Contains(x.RegistrationId)).ToArrayAsync(ct);
        var attemptIds = attempts.Select(x => x.AttemptId).ToArray();
        var bookings = await db.EntryTestBookings.AsNoTracking().Where(x => attemptIds.Contains(x.AttemptId)).ToArrayAsync(ct);
        var sessionIds = bookings.Select(x => x.SessionId).Distinct().ToArray();
        var assignments = await db.BandAssignments.AsNoTracking().Where(x => ids.Contains(x.RegistrationId)).ToArrayAsync(ct);
        var bandIds = assignments.Select(x => x.BandId).Distinct().ToArray();
        var placements = ownRelations ? await db.StudentClassAssignments.AsNoTracking().Where(x => ids.Contains(x.RegistrationId)).ToArrayAsync(ct) : [];
        var classIds = placements.Select(x => x.ClassId).Distinct().ToArray();
        var placementIds = placements.Select(x => x.ClassAssignmentId).ToArray();
        // Only published reference columns, never draft/submitted report values.
        var reports = ownRelations ? await db.GradeReports.AsNoTracking().Where(x => placementIds.Contains(x.ClassAssignmentId) && x.Status == "FINALIZED")
            .Select(x => new Domain.Training.GradeReport { ReportId = x.ReportId, ClassAssignmentId = x.ClassAssignmentId, Status = "FINALIZED" }).ToArrayAsync(ct) : [];
        return new EnrollmentSet(profile, contact, await db.Courses.AsNoTracking().Where(x => courseIds.Contains(x.CourseId)).ToArrayAsync(ct), regs,
            await db.EntryTestSessions.AsNoTracking().Where(x => sessionIds.Contains(x.SessionId)).ToArrayAsync(ct), attempts, bookings, assignments,
            await db.Bands.AsNoTracking().Where(x => ownRelations && assignments.Length > 0 || bandIds.Contains(x.BandId)).ToArrayAsync(ct),
            ownRelations ? await db.Invoices.AsNoTracking().Where(x => ids.Contains(x.RegistrationId)).ToArrayAsync(ct) : [], placements,
            await db.Classes.AsNoTracking().Where(x => classIds.Contains(x.ClassId)).ToArrayAsync(ct),
            await db.Schedules.AsNoTracking().Where(x => classIds.Contains(x.ClassId)).ToArrayAsync(ct))
        { Reports = reports };
    }

    public async Task<RegistrationState> LockStateAsync(int registration, IEnumerable<int> additionalSessions, bool bandSet, CancellationToken ct, bool ownRelations = false)
    {
        var discovered = await Db.Registrations.AsNoTracking().SingleOrDefaultAsync(x => x.RegistrationId == registration, ct) ?? throw new ApiErrorException(ApiErrorCode.NOT_FOUND);
        var student = await Db.StudentProfiles.FromSqlInterpolated($"SELECT * FROM dbo.StudentProfile WITH(UPDLOCK,HOLDLOCK) WHERE student_id={discovered.StudentId}").SingleAsync(ct);
        var course = await Db.Courses.FromSqlInterpolated($"SELECT * FROM dbo.Course WITH(UPDLOCK,HOLDLOCK) WHERE course_id={discovered.CourseId}").SingleAsync(ct);
        var bands = bandSet ? await Db.Bands.FromSqlRaw("SELECT * FROM dbo.Band WITH(UPDLOCK,HOLDLOCK) ORDER BY band_id").ToArrayAsync(ct) : [];
        var reg = await Db.Registrations.FromSqlInterpolated($"SELECT * FROM dbo.Registration WITH(UPDLOCK,HOLDLOCK) WHERE registration_id={registration}").SingleAsync(ct);
        if (reg.StudentId != student.StudentId || reg.CourseId != course.CourseId) throw AcademicGuard.Conflict();
        var attemptIds = await Db.EntryTestAttempts.AsNoTracking().Where(x => x.RegistrationId == registration).Select(x => x.AttemptId).ToArrayAsync(ct);
        var sessionIds = await Db.EntryTestBookings.AsNoTracking().Where(x => attemptIds.Contains(x.AttemptId)).Select(x => x.SessionId).Distinct().ToArrayAsync(ct);
        var sessions = new List<EntryTestSession>();
        foreach (var id in sessionIds.Concat(additionalSessions).Distinct().Order()) sessions.Add(await Db.EntryTestSessions.FromSqlInterpolated($"SELECT * FROM dbo.EntryTestSession WITH(UPDLOCK,HOLDLOCK) WHERE session_id={id}").SingleOrDefaultAsync(ct) ?? throw new ApiErrorException(ApiErrorCode.NOT_FOUND));
        var attempts = await Db.EntryTestAttempts.FromSqlInterpolated($"SELECT * FROM dbo.EntryTestAttempt WITH(UPDLOCK,HOLDLOCK) WHERE registration_id={registration} ORDER BY attempt_id").ToArrayAsync(ct);
        var bookings = await Db.EntryTestBookings.FromSqlRaw("SELECT b.* FROM dbo.EntryTestBooking b WITH(UPDLOCK,HOLDLOCK) JOIN dbo.EntryTestAttempt a ON a.attempt_id=b.attempt_id WHERE a.registration_id={0} ORDER BY b.booking_id", registration).ToArrayAsync(ct);
        if (attempts.Any(x => !attemptIds.Contains(x.AttemptId)) || bookings.Any(x => !sessionIds.Contains(x.SessionId))) throw AcademicGuard.Conflict();
        var assigned = await Db.BandAssignments.FromSqlInterpolated($"SELECT * FROM dbo.BandAssignment WITH(UPDLOCK,HOLDLOCK) WHERE registration_id={registration}").ToArrayAsync(ct);
        if (!bandSet) { var ids = assigned.Select(x => x.BandId).ToArray(); bands = await Db.Bands.AsNoTracking().Where(x => ids.Contains(x.BandId)).ToArrayAsync(ct); }
        var placementRows = ownRelations ? await Db.StudentClassAssignments.AsNoTracking().Where(x => x.RegistrationId == registration).ToArrayAsync(ct) : [];
        var classes = new List<Domain.Training.Class>();
        foreach (var id in placementRows.Select(x => x.ClassId).Distinct().Order()) classes.Add(await Db.Classes.FromSqlInterpolated($"SELECT * FROM dbo.Class WITH(UPDLOCK,HOLDLOCK) WHERE class_id={id}").SingleAsync(ct));
        var placements = ownRelations ? await Db.StudentClassAssignments.FromSqlInterpolated($"SELECT * FROM dbo.StudentClassAssignment WITH(UPDLOCK,HOLDLOCK) WHERE registration_id={registration}").ToArrayAsync(ct) : [];
        if (!placementRows.Select(x => x.ClassAssignmentId).Order().SequenceEqual(placements.Select(x => x.ClassAssignmentId).Order())) throw AcademicGuard.Conflict();
        var invoices = ownRelations ? await Db.Invoices.FromSqlInterpolated($"SELECT * FROM dbo.Invoice WITH(UPDLOCK,HOLDLOCK) WHERE registration_id={registration}").ToArrayAsync(ct) : [];
        var contact = await Db.Accounts.SingleAsync(x => x.AccountId == student.AccountId, ct);
        var set = new EnrollmentSet(student, contact, [course], [reg], sessions, attempts, bookings, assigned, bands, invoices, placements, classes, []);
        return new(set, reg);
    }

    public void AddAttempt(EntryTestAttempt attempt) => Db.EntryTestAttempts.Add(attempt);
    public Task<EnrollmentSet> ReadSetAsync(int student, int[]? registrations, CancellationToken ct) => ReadSet(Db, student, registrations, ct);
    public void AddBooking(EntryTestBooking booking) => Db.EntryTestBookings.Add(booking);
    public void AddRegistration(Registration registration) => Db.Registrations.Add(registration);
    public void AddBandAssignment(BandAssignment assignment) => Db.BandAssignments.Add(assignment);

    public async Task<int> BandForScoreAsync(decimal score, CancellationToken ct)
    {
        var ids = await Db.Database.SqlQuery<int>($"SELECT bandId AS Value FROM dbo.ufn_BandForScore({score})").ToArrayAsync(ct);
        return ids.Length == 1 ? ids[0] : throw AcademicGuard.Conflict();
    }
}
