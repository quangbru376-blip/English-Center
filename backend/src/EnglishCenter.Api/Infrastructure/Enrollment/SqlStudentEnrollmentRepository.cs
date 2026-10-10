namespace EnglishCenter.Api.Infrastructure.Enrollment;

using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Common.Security;
using EnglishCenter.Api.Domain.Admissions;
using EnglishCenter.Api.Services.Academics;
using EnglishCenter.Api.Services.Enrollment;
using Microsoft.EntityFrameworkCore;

public sealed record EnrollmentResources(int[] Graders, int? Invoice, int[] Classes);

public sealed partial class SqlEnrollmentRepository
{
    public async Task<int[]> UnfinishedAsync(int student, int course, CancellationToken ct) =>
        await Db.Registrations.AsNoTracking().Where(x => x.StudentId == student && x.CourseId == course && x.IsUnfinished)
            .OrderBy(x => x.RegistrationId).Select(x => x.RegistrationId).ToArrayAsync(ct);

    public async Task<RegistrationState> CreateAsync(int actor, int courseId, DateTimeOffset now, int[] discovered, CancellationToken ct)
    {
        var studentId = await OwnStudentAsync(actor, ct);
        var student = await Db.StudentProfiles.FromSqlInterpolated($"SELECT * FROM dbo.StudentProfile WITH(UPDLOCK,HOLDLOCK) WHERE student_id={studentId}").SingleAsync(ct);
        var course = await Db.Courses.FromSqlInterpolated($"SELECT * FROM dbo.Course WITH(UPDLOCK,HOLDLOCK) WHERE course_id={courseId}").SingleOrDefaultAsync(ct);
        if (course is null) throw new ApiErrorException(ApiErrorCode.NOT_FOUND);
        if (course.Status != "ACTIVE") throw AcademicGuard.Conflict();
        // A serializable predicate locks the filtered unique key even when absent.
        var unfinished = await Db.Registrations.FromSqlInterpolated($"SELECT * FROM dbo.Registration WITH(UPDLOCK,HOLDLOCK) WHERE student_id={studentId} AND course_id={courseId} AND is_unfinished=1 ORDER BY registration_id").ToArrayAsync(ct);
        if (!discovered.SequenceEqual(unfinished.Select(x => x.RegistrationId))) throw AcademicGuard.Conflict();
        foreach (var registration in unfinished) await RequireUnfinishedAsync(registration, ct);
        var history = await Db.Registrations.FromSqlInterpolated($"SELECT * FROM dbo.Registration WITH(UPDLOCK,HOLDLOCK) WHERE student_id={studentId} AND course_id={courseId} AND is_unfinished=0 ORDER BY registration_id").ToArrayAsync(ct);
        foreach (var registration in history) await RequireUnfinishedAsync(registration, ct);
        if (unfinished.Length != 0) throw AcademicGuard.Conflict();
        var reg = new Registration { StudentId = studentId, CourseId = courseId, Status = "PENDING", IsUnfinished = true, CreatedAt = now.UtcDateTime };
        AddRegistration(reg); await SaveAsync(ct);
        var account = await Db.Accounts.SingleAsync(x => x.AccountId == actor, ct);
        return new(new(student, account, [course], [reg], [], [], [], [], [], [], [], [], []), reg);
    }

    public async Task RequireUnfinishedAsync(Registration registration, CancellationToken ct)
    {
        var values = await Db.Database.SqlQuery<bool>($"SELECT isUnfinished AS Value FROM dbo.ufn_RegistrationUnfinished({registration.RegistrationId})").ToArrayAsync(ct);
        if (values.Length != 1 || values[0] != registration.IsUnfinished) throw AcademicGuard.Conflict();
    }

    public async Task<EnrollmentResources> ResourcesAsync(int registration, int? targetSession, bool relations, CancellationToken ct)
    {
        var ids = await (from b in Db.EntryTestBookings
                         join a in Db.EntryTestAttempts on b.AttemptId equals a.AttemptId
                         where a.RegistrationId == registration
                         select b.SessionId).Distinct().ToArrayAsync(ct);
        var union = ids.Concat(targetSession is { } id ? [id] : Array.Empty<int>()).Distinct().ToArray();
        var graders = await Db.EntryTestSessions.AsNoTracking().Where(x => union.Contains(x.SessionId)).Select(x => x.GraderId).Distinct().Order().ToArrayAsync(ct);
        var invoice = relations ? await Db.Invoices.AsNoTracking().Where(x => x.RegistrationId == registration).Select(x => (int?)x.InvoiceId).SingleOrDefaultAsync(ct) : null;
        var classes = relations ? await Db.StudentClassAssignments.AsNoTracking().Where(x => x.RegistrationId == registration).Select(x => x.ClassId).Distinct().Order().ToArrayAsync(ct) : [];
        return new(graders, invoice, classes);
    }

    public static void RequireResources(RegistrationState state, EnrollmentResources resources, bool relations)
    {
        if (!resources.Graders.SequenceEqual(state.Set.Sessions.Select(x => x.GraderId).Distinct().Order()) ||
            relations && (resources.Invoice != state.Invoice?.InvoiceId || !resources.Classes.SequenceEqual(state.Set.Placements.Select(x => x.ClassId).Distinct().Order()))) throw AcademicGuard.Conflict();
    }

    public async Task RequireSeatAsync(int student, EntryTestSession target, int? excludedBooking, DateTimeOffset now, CancellationToken ct)
    {
        if (target.Status != "OPEN" || target.StartAt <= now.UtcDateTime) throw AcademicGuard.Conflict();
        var grader = await Db.Accounts.SingleOrDefaultAsync(x => x.AccountId == target.GraderId, ct);
        if (grader is null || grader.Status != "ACTIVE" || await RoleCodeAsync(grader.RoleId, ct) != "Teacher") throw AcademicGuard.Conflict();
        // TIMETABLE serializes every booking/schedule writer. A session RV alone
        // cannot prove capacity because booking inserts do not alter that RV.
        var occupied = await Db.EntryTestBookings.CountAsync(x => x.SessionId == target.SessionId && x.Status == "SCHEDULED" && x.BookingId != excludedBooking, ct);
        if (occupied >= target.Capacity) throw AcademicGuard.Conflict();
        var overlap = await (from b in Db.EntryTestBookings
                             join a in Db.EntryTestAttempts on b.AttemptId equals a.AttemptId
                             join r in Db.Registrations on a.RegistrationId equals r.RegistrationId
                             join s in Db.EntryTestSessions on b.SessionId equals s.SessionId
                             where r.StudentId == student && b.Status == "SCHEDULED" && b.BookingId != excludedBooking && s.Status != "CANCELLED" &&
                                 s.StartAt < target.EndAt && target.StartAt < s.EndAt
                             select b.BookingId).AnyAsync(ct);
        if (overlap) throw AcademicGuard.Conflict();
        var schedules = await (from p in Db.StudentClassAssignments
                               join r in Db.Registrations on p.RegistrationId equals r.RegistrationId
                               join cls in Db.Classes on p.ClassId equals cls.ClassId
                               join s in Db.Schedules on cls.ClassId equals s.ClassId
                               where r.StudentId == student && cls.Status != "CANCELLED" && s.IsCurrent && (s.OccurrenceState == "PLANNED" || s.OccurrenceState == "HELD")
                               select new { s.SessionDate, s.StartTime, s.EndTime }).ToArrayAsync(ct);
        foreach (var s in schedules)
        {
            var start = new DateTimeOffset(s.SessionDate.ToDateTime(s.StartTime), TimeSpan.FromHours(7)).UtcDateTime;
            var end = new DateTimeOffset(s.SessionDate.ToDateTime(s.EndTime), TimeSpan.FromHours(7)).UtcDateTime;
            if (start < target.EndAt && target.StartAt < end) throw AcademicGuard.Conflict();
        }
    }
}
