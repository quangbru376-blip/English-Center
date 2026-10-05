namespace EnglishCenter.SqlTests;

using EnglishCenter.Api.Domain.Academics;
using EnglishCenter.Api.Domain.Training;
using EnglishCenter.Api.Infrastructure.Compatibility;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Xunit;

public class CompatibilityTests
{
    private static Task<DatabaseCompatibilityResult> Inspect(SqlFixture f, bool runtime = false) =>
        new DatabaseCompatibilityInspector().InspectAsync(f.Context, 10, runtime, TestContext.Current.CancellationToken);

    [Fact]
    public async Task CompleteD12CatalogIsCompatibleWithoutClaimingOperatorIsRuntime()
    {
        await using var f = await SqlFixture.CreateAsync();
        var healthy = await Inspect(f);
        Assert.True(healthy.IsCompatible, healthy.Cause); Assert.Equal("COMPATIBLE", healthy.Cause);
        var unsafePrincipal = await Inspect(f, true);
        Assert.False(unsafePrincipal.IsCompatible); Assert.Equal("UNSAFE_PRINCIPAL", unsafePrincipal.Cause);
    }

    [Theory]
    [InlineData("view")] [InlineData("function")] [InlineData("procedure")] [InlineData("trigger")]
    [InlineData("column")] [InlineData("check")] [InlineData("index")] [InlineData("fk")]
    [InlineData("missing")] [InlineData("grant")] [InlineData("check-body")] [InlineData("index-body")]
    public async Task SameCountsOrMissingObjectNeverHideActualSignatureDrift(string drift)
    {
        await using var f = await SqlFixture.CreateAsync();
        var sql = drift switch
        {
            "view" => "ALTER VIEW dbo.vw_CurrentScheduleOccurrences AS SELECT schedule_id AS changed FROM dbo.Schedule;",
            "function" => "ALTER FUNCTION dbo.ufn_InvoiceDebtFlags(@status varchar(16),@dueDate date,@todayVn date) RETURNS TABLE AS RETURN SELECT CAST(0 AS bit) AS isOverdue,CAST(0 AS int) AS daysOverdue;",
            "procedure" => "ALTER PROCEDURE dbo.usp_GetAdmissionsReport @changed int AS BEGIN SELECT 1 AS changed; END;",
            "trigger" => "DISABLE TRIGGER dbo.trg_Component_UsedConfigurationGuard ON dbo.ScoreComponentConfig;",
            "column" => "ALTER TABLE dbo.Course ALTER COLUMN course_name nvarchar(301) NOT NULL;",
            "check" => "ALTER TABLE dbo.Course NOCHECK CONSTRAINT CK_Course_Price;",
            "index" => "DROP INDEX IX_Audit_Operation ON dbo.AuditLog;",
            "fk" => "ALTER TABLE dbo.AuditLog NOCHECK CONSTRAINT FK_AuditLog_Account_actor_account_id;",
            "missing" => "DROP PROCEDURE dbo.usp_GetAdmissionsReport;",
            "grant" => "GRANT DELETE ON dbo.Invoice TO ec_app_runtime;",
            "check-body" => "ALTER TABLE dbo.Course DROP CONSTRAINT CK_Course_Price; ALTER TABLE dbo.Course ADD CONSTRAINT CK_Course_Price CHECK (base_price>=0 AND base_price<=10);",
            "index-body" => "DROP INDEX IX_Audit_Operation ON dbo.AuditLog; CREATE INDEX IX_Audit_Operation ON dbo.AuditLog(occurred_at);",
            _ => throw new ArgumentOutOfRangeException(nameof(drift))
        };
        await SqlRows.ReadAsync(f, sql);
        var result = await Inspect(f);
        Assert.False(result.IsCompatible); Assert.Equal("CATALOG_DRIFT", result.Cause);
    }

    [Fact]
    public async Task RuntimeRoleWithIncompleteCatalogVisibilityFailsClosed()
    {
        await using var f = await SqlFixture.CreateAsync();
        await SqlRows.ReadAsync(f, "IF DATABASE_PRINCIPAL_ID('D12_fixture_user_0') IS NOT NULL THROW 51912,'Fixture exists',1; CREATE USER D12_fixture_user_0 WITHOUT LOGIN; ALTER ROLE ec_app_runtime ADD MEMBER D12_fixture_user_0; EXECUTE AS USER='D12_fixture_user_0';");
        try
        {
            var result = await Inspect(f);
            Assert.False(result.IsCompatible); Assert.Equal("UNREADABLE_CATALOG", result.Cause);
        }
        finally { await SqlRows.ReadAsync(f, "REVERT;"); }
    }

    [Fact]
    public async Task PreCanceledProbeDoesNotTouchTransactionOrData()
    {
        await using var f = await SqlFixture.CreateAsync();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => new DatabaseCompatibilityInspector().InspectAsync(f.Context, 10, false, new CancellationToken(true)));
        var tx = await SqlRows.ReadAsync(f, "SELECT @@TRANCOUNT AS count;");
        Assert.Equal(1, tx[0].Value(0, "count"));
        Assert.True((await Inspect(f)).IsCompatible);
    }

    [Theory]
    [InlineData("Band")] [InlineData("TrainingProgram")] [InlineData("ScoreComponentConfig")]
    [InlineData("Attendance")] [InlineData("Score")]
    public async Task EfTriggerTablesReceiveIdentityAndFreshRowVersionOnInsertAndUpdate(string table)
    {
        await using var f = await SqlFixture.CreateAsync();
        object entity;
        if (table == "Band")
        {
            entity = new Band { BandCode = "D13", MinScore = 25 }; f.Context.Add(entity);
        }
        else if (table == "TrainingProgram")
        {
            entity = new TrainingProgram { CourseId = f.Course.CourseId, BandId = f.Band.BandId, ProgramName = "D13", Status = "DRAFT", ExpectedLessonCount = 3 };
            // Fixture's Course/Band pair is already used; use the newly inserted Band.
            var band = new Band { BandCode = "D13_PROGRAM", MinScore = 25 }; f.Context.Add(band);
            await f.Context.SaveChangesAsync(TestContext.Current.CancellationToken);
            ((TrainingProgram)entity).BandId = band.BandId; f.Context.Add(entity);
        }
        else if (table == "ScoreComponentConfig")
        {
            entity = new ScoreComponentConfig { ProgramId = f.UnusedProgram.ProgramId, ComponentName = "D13", Kind = "GENERAL", Weight = 100 }; f.Context.Add(entity);
        }
        else if (table == "Attendance")
        {
            entity = new Attendance { ClassAssignmentId = f.Assignment.ClassAssignmentId, ScheduleId = f.Schedule.ScheduleId, Present = true, EnteredBy = f.Teacher.AccountId, EnteredAt = SqlFixture.Instant }; f.Context.Add(entity);
        }
        else
        {
            entity = new Score { ClassAssignmentId = f.Assignment.ClassAssignmentId, ComponentId = f.General.ComponentId, ScoreValue = 50, EnteredBy = f.Teacher.AccountId, EnteredAt = SqlFixture.Instant }; f.Context.Add(entity);
        }
        await f.Context.SaveChangesAsync(TestContext.Current.CancellationToken);
        var entry = f.Context.Entry(entity);
        var key = Assert.Single(entry.Metadata.FindPrimaryKey()!.Properties);
        Assert.True((int)entry.Property(key.Name).CurrentValue! > 0);
        var version = Assert.Single(entry.Metadata.GetProperties(), p => p.IsConcurrencyToken);
        var before = ((byte[])entry.Property(version.Name).CurrentValue!).ToArray(); Assert.Equal(8, before.Length);
        if (entity is TrainingProgram program) await f.LockAsync("EC:PROGRAM:" + program.ProgramId);
        var changedProperty = table switch { "Band" => "BandCode", "TrainingProgram" => "ProgramName", "ScoreComponentConfig" => "ComponentName", "Attendance" => "Present", _ => "ScoreValue" };
        entry.Property(changedProperty).CurrentValue = table switch { "Attendance" => false, "Score" => 51m, _ => "D13_UPDATED" };
        await f.Context.SaveChangesAsync(TestContext.Current.CancellationToken);
        var after = (byte[])entry.Property(version.Name).CurrentValue!; Assert.Equal(8, after.Length); Assert.NotEqual(before, after);
        await entry.ReloadAsync(TestContext.Current.CancellationToken);
        Assert.Equal(after, entry.Property(version.Name).CurrentValue);
    }

    [Theory]
    [InlineData("Band",51022)] [InlineData("TrainingProgram",51002)] [InlineData("ScoreComponentConfig",51012)]
    [InlineData("Attendance",51032)] [InlineData("Score",51042)]
    public async Task InvalidEfWritesRollBackFixtureAndDiscardGeneratedTrackedState(string table, int error)
    {
        await using var f = await SqlFixture.CreateAsync();
        object entity;
        if (table == "Band") { await f.CompletedAttemptAsync(); f.Band.MinScore = 1; entity = f.Band; }
        else if (table == "TrainingProgram") { f.Program.ExpectedLessonCount = 4; entity = f.Program; }
        else if (table == "ScoreComponentConfig") { f.General.Weight = 40; entity = f.General; }
        else if (table == "Attendance")
        {
            entity = new Attendance { ClassAssignmentId=f.Assignment.ClassAssignmentId,ScheduleId=f.Exam.ScheduleId,Present=true,EnteredBy=f.Teacher.AccountId,EnteredAt=SqlFixture.Instant }; f.Context.Add(entity);
        }
        else
        {
            entity = new Score { ClassAssignmentId=f.Assignment.ClassAssignmentId,ComponentId=f.General.ComponentId,ScheduleId=f.Schedule.ScheduleId,ScoreValue=50,EnteredBy=f.Teacher.AccountId,EnteredAt=SqlFixture.Instant }; f.Context.Add(entity);
        }
        var exception = await Assert.ThrowsAsync<DbUpdateException>(() => f.Context.SaveChangesAsync(TestContext.Current.CancellationToken));
        Assert.Equal(error, Assert.IsType<SqlException>(exception.InnerException).Number);
        f.Context.ChangeTracker.Clear();
        Assert.Empty(f.Context.ChangeTracker.Entries());
        await f.Context.Database.CurrentTransaction!.DisposeAsync();
        Assert.Equal(0, await f.Context.Courses.CountAsync(TestContext.Current.CancellationToken));
        // The failed context/transaction is disposed; never reuse stale generated IDs/RVs.
    }
}
