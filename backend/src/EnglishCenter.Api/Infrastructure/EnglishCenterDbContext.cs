namespace EnglishCenter.Api.Infrastructure;

using EnglishCenter.Api.Domain.Identity;
using EnglishCenter.Api.Domain.Academics;
using EnglishCenter.Api.Domain.Admissions;
using EnglishCenter.Api.Domain.Training;
using EnglishCenter.Api.Infrastructure.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

public class EnglishCenterDbContext : DbContext
{
    public EnglishCenterDbContext(DbContextOptions<EnglishCenterDbContext> options)
        : base(options)
    {
    }

    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<StudentProfile> StudentProfiles => Set<StudentProfile>();
    public DbSet<AuthSession> AuthSessions => Set<AuthSession>();

    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Band> Bands => Set<Band>();
    public DbSet<TrainingProgram> TrainingPrograms => Set<TrainingProgram>();
    public DbSet<ScoreComponentConfig> ScoreComponentConfigs => Set<ScoreComponentConfig>();

    public DbSet<Registration> Registrations => Set<Registration>();
    public DbSet<EntryTestSession> EntryTestSessions => Set<EntryTestSession>();
    public DbSet<EntryTestAttempt> EntryTestAttempts => Set<EntryTestAttempt>();
    public DbSet<EntryTestBooking> EntryTestBookings => Set<EntryTestBooking>();
    public DbSet<BandAssignment> BandAssignments => Set<BandAssignment>();

    public DbSet<Class> Classes => Set<Class>();
    public DbSet<StudentClassAssignment> StudentClassAssignments => Set<StudentClassAssignment>();
    public DbSet<Schedule> Schedules => Set<Schedule>();

    public DbSet<Attendance> Attendances => Set<Attendance>();
    public DbSet<Score> Scores => Set<Score>();
    public DbSet<GradeReport> GradeReports => Set<GradeReport>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);
        // FK lookup indexes are explicit; the composite cycle otherwise creates a duplicate.
        configurationBuilder.Conventions.Remove(typeof(ForeignKeyIndexConvention));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("dbo");
        modelBuilder.UseCollation("Latin1_General_100_CI_AS_SC");

        modelBuilder.ApplyConfiguration(new RoleConfiguration());

        modelBuilder.ApplyConfiguration(new AccountConfiguration());
        modelBuilder.ApplyConfiguration(new StudentProfileConfiguration());
        modelBuilder.ApplyConfiguration(new AuthSessionConfiguration());
        modelBuilder.ApplyConfiguration(new CourseConfiguration());
        modelBuilder.ApplyConfiguration(new BandConfiguration());
        modelBuilder.ApplyConfiguration(new TrainingProgramConfiguration());
        modelBuilder.ApplyConfiguration(new ScoreComponentConfigConfiguration());
        modelBuilder.ApplyConfiguration(new RegistrationConfiguration());
        modelBuilder.ApplyConfiguration(new EntryTestSessionConfiguration());
        modelBuilder.ApplyConfiguration(new EntryTestAttemptConfiguration());
        modelBuilder.ApplyConfiguration(new EntryTestBookingConfiguration());
        modelBuilder.ApplyConfiguration(new BandAssignmentConfiguration());
        modelBuilder.ApplyConfiguration(new ClassConfiguration());
        modelBuilder.ApplyConfiguration(new StudentClassAssignmentConfiguration());
        modelBuilder.ApplyConfiguration(new ScheduleConfiguration());
        modelBuilder.ApplyConfiguration(new AttendanceConfiguration());
        modelBuilder.ApplyConfiguration(new ScoreConfiguration());
        modelBuilder.ApplyConfiguration(new GradeReportConfiguration());

    }
}
