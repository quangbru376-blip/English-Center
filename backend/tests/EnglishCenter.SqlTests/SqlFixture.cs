namespace EnglishCenter.SqlTests;

using EnglishCenter.Api.Domain.Academics;
using EnglishCenter.Api.Domain.Admissions;
using EnglishCenter.Api.Domain.Identity;
using EnglishCenter.Api.Domain.Training;
using EnglishCenter.Api.Infrastructure;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Xunit;



internal sealed class SqlFixture : IAsyncDisposable
{
    internal EnglishCenterDbContext Context { get; }
    private IDbContextTransaction? transaction;
    internal Account Teacher { get; private set; } = null!;
    internal Course Course { get; private set; } = null!;
    internal Band Band { get; private set; } = null!;
    internal Band OtherBand { get; private set; } = null!;
    internal TrainingProgram Program { get; private set; } = null!;
    internal TrainingProgram UnusedProgram { get; private set; } = null!;
    internal Class Class { get; private set; } = null!;
    internal Class OtherClass { get; private set; } = null!;
    internal StudentClassAssignment Assignment { get; private set; } = null!;
    internal Registration Registration { get; private set; } = null!;
    internal ScoreComponentConfig General { get; private set; } = null!;
    internal ScoreComponentConfig Lesson { get; private set; } = null!;
    internal Schedule Schedule { get; private set; } = null!;
    internal Schedule Exam { get; private set; } = null!;
    internal Schedule OtherSchedule { get; private set; } = null!;
    internal GradeReport Report { get; private set; } = null!;
    internal static readonly DateTime Instant = new(2026, 10, 5, 0, 0, 0, DateTimeKind.Utc);

    private SqlFixture()
    {
        Context = new(new DbContextOptionsBuilder<EnglishCenterDbContext>()
            .UseSqlServer("Server=lpc:.\\SQLEXPRESS;Database=EnglishCenter_D07_20261005;Integrated Security=True;Encrypt=True;TrustServerCertificate=True", sql => sql.UseCompatibilityLevel(160)).Options);
    }

    internal static async Task<SqlFixture> CreateAsync(decimal generalWeight = 50m, decimal lessonWeight = 50m, string generalKind = "GENERAL", System.Data.IsolationLevel isolation = System.Data.IsolationLevel.ReadCommitted)
    {
        var fixture = new SqlFixture();
        try
        {
            await fixture.Context.Database.OpenConnectionAsync(TestContext.Current.CancellationToken);
            Assert.Equal("EnglishCenter_D07_20261005", fixture.Context.Database.GetDbConnection().Database);
            fixture.transaction = await fixture.Context.Database.BeginTransactionAsync(isolation, TestContext.Current.CancellationToken);
            await fixture.LockAsync("EC:BANDSET");
            await fixture.SetupAsync(generalWeight, lessonWeight, generalKind);
            return fixture;
        }
        catch
        {
            await fixture.DisposeAsync();
            throw;
        }
    }

    internal async Task<int> ExecuteAsync(FormattableString sql) =>
        await Context.Database.ExecuteSqlInterpolatedAsync(sql, TestContext.Current.CancellationToken);

    internal async Task LockAsync(string resource)
    {
        await ExecuteAsync($"DECLARE @result int; EXEC @result=sys.sp_getapplock @Resource={resource},@LockMode='Exclusive',@LockOwner='Transaction',@DbPrincipal='public',@LockTimeout=1000; IF @result<0 THROW 51900,'Fixture gate failed',1;");
    }

    internal async Task ReleaseAsync(string resource) =>
        await ExecuteAsync($"DECLARE @result int; EXEC @result=sys.sp_releaseapplock @Resource={resource},@LockOwner='Transaction',@DbPrincipal='public'; IF @result<0 THROW 51900,'Fixture release failed',1;");

    private async Task SaveAsync() => await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

    private async Task SetupAsync(decimal generalWeight, decimal lessonWeight, string generalKind)
    {
        var role = new Role { RoleCode = "Teacher", RoleName = "SQL fixture" };
        Context.Roles.Add(role); await SaveAsync();
        Teacher = new Account { RoleId = role.RoleId, Email = "fixture@example.invalid", PasswordHash = "not-a-credential", FullName = "SQL fixture", Status = "ACTIVE", SecurityVersion = 1 };
        Course = new Course { CourseCode = "FIXTURE", CourseName = "SQL fixture", BasePrice = 1, Status = "ACTIVE" };
        Band = new Band { BandCode = "B0", MinScore = 0 };
        OtherBand = new Band { BandCode = "B1", MinScore = 50 };
        Context.AddRange(Teacher, Course, Band, OtherBand); await SaveAsync();
        Program = new TrainingProgram { CourseId = Course.CourseId, BandId = Band.BandId, ProgramName = "SQL fixture", ExpectedLessonCount = 2, Status = "DRAFT" };
        UnusedProgram = new TrainingProgram { CourseId = Course.CourseId, BandId = OtherBand.BandId, ProgramName = "Unused fixture", ExpectedLessonCount = 2, Status = "DRAFT" };
        Context.AddRange(Program, UnusedProgram); await SaveAsync();
        await LockAsync($"EC:PROGRAM:{Program.ProgramId}");
        await LockAsync($"EC:PROGRAM:{UnusedProgram.ProgramId}");
        General = new ScoreComponentConfig { ProgramId = Program.ProgramId, ComponentName = "General", Kind = generalKind, Weight = generalWeight };
        Lesson = new ScoreComponentConfig { ProgramId = Program.ProgramId, ComponentName = "Lesson", Kind = "LESSON", Weight = lessonWeight };
        Context.AddRange(General, Lesson); await SaveAsync();
        Class = new Class { ProgramId = Program.ProgramId, TeacherId = Teacher.AccountId, ClassCode = "FIXTURE1", Status = "ONGOING", MaxSize = 10 };
        OtherClass = new Class { ProgramId = Program.ProgramId, TeacherId = Teacher.AccountId, ClassCode = "FIXTURE2", Status = "OPEN", MaxSize = 10 };
        Context.AddRange(Class, OtherClass); await SaveAsync();
        foreach (var resource in new[] { $"EC:CLASS:{Class.ClassId}", $"EC:CLASS:{OtherClass.ClassId}" }.Order(StringComparer.Ordinal)) await LockAsync(resource);
        var student = new StudentProfile { AccountId = Teacher.AccountId, DateOfBirth = new DateOnly(2000, 1, 1) };
        Context.StudentProfiles.Add(student); await SaveAsync();
        Registration = new Registration { StudentId = student.StudentId, CourseId = Course.CourseId, Status = "PENDING" };
        Context.Registrations.Add(Registration); await SaveAsync();
        Assignment = new StudentClassAssignment { ClassId = Class.ClassId, RegistrationId = Registration.RegistrationId, AssignedBy = Teacher.AccountId, AssignedAt = Instant };
        Context.StudentClassAssignments.Add(Assignment); await SaveAsync();
        Report = new GradeReport { ClassAssignmentId = Assignment.ClassAssignmentId, Status = "DRAFT" };
        Schedule = NewSchedule(Class.ClassId, "LESSON", 1);
        Exam = NewSchedule(Class.ClassId, "FINAL_EXAM", 2);
        OtherSchedule = NewSchedule(OtherClass.ClassId, "LESSON", 3);
        Context.AddRange(Report, Schedule, Exam, OtherSchedule); await SaveAsync();
    }

    private static Schedule NewSchedule(int classId, string type, int day) => new()
    {
        ClassId = classId, SessionType = type, ObligationKey = Guid.NewGuid(), IsCurrent = true,
        SessionDate = new DateOnly(2026, 10, day), StartTime = new TimeOnly(8, 0), EndTime = new TimeOnly(9, 0), Room = "R1", OccurrenceState = "PLANNED"
    };

    internal async Task CompletedAttemptAsync()
    {
        var session = new EntryTestSession { StartAt = Instant, EndAt = Instant.AddHours(1), Room = "R1", Capacity = 10, GraderId = Teacher.AccountId, Status = "CLOSED" };
        var attempt = new EntryTestAttempt { RegistrationId = Registration.RegistrationId, AttemptNumber = 1 };
        Context.AddRange(session, attempt); await SaveAsync();
        var booking = new EntryTestBooking { AttemptId = attempt.AttemptId, SessionId = session.SessionId, Status = "COMPLETED", BookedAt = Instant };
        Context.Add(booking); await SaveAsync();
        attempt.Score = 50; attempt.GradedBy = Teacher.AccountId; attempt.GradedAt = Instant; attempt.CompletedBookingId = booking.BookingId;
        await SaveAsync();
    }

    internal async Task<StudentClassAssignment> OtherAssignmentAsync()
    {
        var account = new Account { RoleId=Teacher.RoleId,Email="other@example.invalid",PasswordHash="not-a-credential",FullName="Fixture",Status="ACTIVE",SecurityVersion=1 };
        Context.Add(account); await SaveAsync();
        var student = new StudentProfile { AccountId=account.AccountId,DateOfBirth=new DateOnly(2000,1,1) };
        Context.Add(student); await SaveAsync();
        var registration = new Registration { StudentId=student.StudentId,CourseId=Course.CourseId,Status="PENDING" };
        Context.Add(registration); await SaveAsync();
        var assignment = new StudentClassAssignment { ClassId=OtherClass.ClassId,RegistrationId=registration.RegistrationId,AssignedBy=Teacher.AccountId,AssignedAt=Instant };
        Context.Add(assignment); await SaveAsync();
        return assignment;
    }

    internal async Task ExpectViolationAsync(int error, FormattableString statement)
    {
        var exception = await Assert.ThrowsAsync<SqlException>(() => ExecuteAsync(statement));
        Assert.Equal(error, exception.Number);
        await transaction!.DisposeAsync(); transaction = null;
        // SQL Server trigger THROW rolls back the entire fixture, including setup.
        Assert.Equal(0, await Context.Database.SqlQueryRaw<int>("SELECT COUNT(*) AS Value FROM dbo.Course").SingleAsync(TestContext.Current.CancellationToken));
    }

    public async ValueTask DisposeAsync()
    {
        if (transaction is not null) await transaction.DisposeAsync();
        await Context.DisposeAsync();
    }
}
