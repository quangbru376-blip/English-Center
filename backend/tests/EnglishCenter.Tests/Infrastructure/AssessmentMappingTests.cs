namespace EnglishCenter.Tests.Infrastructure;

using EnglishCenter.Api.Domain.Training;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Xunit;

public class AssessmentMappingTests
{
    private static readonly Dictionary<string, string> Expected = new()
    {
        ["Attendance"] = "attendance_id:int schedule_id:int class_assignment_id:int present:bit entered_by:int entered_at:datetime2(3) row_version:rowversion",
        ["Score"] = "score_id:int class_assignment_id:int component_id:int schedule_id:int? score_value:decimal(5,2) entered_by:int entered_at:datetime2(3) row_version:rowversion",
        ["GradeReport"] = "report_id:int class_assignment_id:int final_score:decimal(5,2)? status:varchar(16) teacher_comment:nvarchar(2000)? submitted_by:int? submitted_at:datetime2(3)? submitted_operation_id:uniqueidentifier? reviewed_by:int? reviewed_at:datetime2(3)? review_operation_id:uniqueidentifier? returned_reason:nvarchar(2000)? finalized_at:datetime2(3)? row_version:rowversion"
    };

    [Theory]
    [InlineData("Attendance", "attendance_id")]
    [InlineData("Score", "score_id")]
    [InlineData("GradeReport", "report_id")]
    public void PhysicalColumns_MatchDictionaryWithoutShadowsOrExtraState(string table, string id)
    {
        using var context = RoleMappingTests.Create();
        var entity = Entity(context.GetService<IDesignTimeModel>().Model, table);
        var columns = entity.GetProperties().ToDictionary(p => p.GetColumnName());
        var expected = Expected[table].Split(' ').Select(x => x.Split(':')).ToDictionary(x => x[0], x => x[1]);
        Assert.Equal(expected.Keys.Order(), columns.Keys.Order());
        Assert.Equal("dbo", entity.GetSchema());
        foreach (var (name, type) in expected)
        {
            var column = columns[name];
            var sql = type.TrimEnd('?');
            Assert.Equal(sql, column.GetColumnType());
            Assert.Equal(type.EndsWith('?'), column.IsNullable);
            Assert.False(column.IsShadowProperty());
            if (sql == "decimal(5,2)")
            {
                Assert.Equal(5, column.GetPrecision());
                Assert.Equal(2, column.GetScale());
                Assert.Equal(type.EndsWith('?') ? typeof(decimal?) : typeof(decimal), column.ClrType);
            }
            if (sql == "datetime2(3)") Assert.Equal(3, column.GetPrecision());
            if (sql == "varchar(16)") { Assert.False(column.IsUnicode()); Assert.Equal(16, column.GetMaxLength()); }
            if (sql == "nvarchar(2000)") { Assert.True(column.IsUnicode()); Assert.Equal(2000, column.GetMaxLength()); }
            if (sql == "uniqueidentifier") Assert.Equal(typeof(Guid?), column.ClrType);
        }
        var key = Assert.Single(entity.FindPrimaryKey()!.Properties);
        Assert.Equal(id, key.GetColumnName());
        Assert.Equal(SqlServerValueGenerationStrategy.IdentityColumn, key.GetValueGenerationStrategy());
        Assert.Equal(1L, key.GetIdentitySeed());
        Assert.Equal(1, key.GetIdentityIncrement());
        Assert.Equal(ValueGenerated.OnAdd, key.ValueGenerated);
        Assert.Single(entity.GetKeys());
        Assert.Empty(entity.GetSeedData());
    }

    [Fact]
    public void Rowversions_ProtectAllThreeMutableRecords()
    {
        using var context = RoleMappingTests.Create();
        var model = context.GetService<IDesignTimeModel>().Model;
        foreach (var table in Expected.Keys)
        {
            var rowversion = Column(Entity(model, table), "row_version");
            Assert.True(rowversion.IsConcurrencyToken);
            Assert.Equal(ValueGenerated.OnAddOrUpdate, rowversion.ValueGenerated);
            Assert.Equal(PropertySaveBehavior.Ignore, rowversion.GetBeforeSaveBehavior());
            Assert.Equal(PropertySaveBehavior.Ignore, rowversion.GetAfterSaveBehavior());
        }
    }

    [Fact]
    public void Defaults_KeepExplicitAbsenceAndZeroWithoutInventingMissingRows()
    {
        using var context = RoleMappingTests.Create();
        var model = context.GetService<IDesignTimeModel>().Model;
        foreach (var table in Expected.Keys)
        {
            var entity = Entity(model, table);
            var defaults = entity.GetProperties().Where(p => p.FindAnnotation(RelationalAnnotationNames.DefaultValue) != null || p.GetDefaultValueSql() != null);
            Assert.Equal(table == "GradeReport" ? new[] { "status" } : [], defaults.Select(p => p.GetColumnName()));
        }
        var status = Column(Entity(model, "GradeReport"), "status");
        Assert.Equal("DRAFT", status.GetDefaultValue());
        Assert.Equal(string.Empty, status.Sentinel);
        foreach (var (table, name) in new[] { ("Attendance", "present"), ("Score", "score_value"), ("Attendance", "entered_at"), ("Score", "entered_at"), ("GradeReport", "final_score") })
        {
            var property = Column(Entity(model, table), name);
            Assert.Equal(ValueGenerated.Never, property.ValueGenerated);
            Assert.Equal(PropertySaveBehavior.Save, property.GetBeforeSaveBehavior());
        }
        Assert.Null(new GradeReport().FinalScore);
        Assert.Null(new GradeReport().FinalizedAt);
        Assert.Null(new GradeReport().SubmittedAt);
        Assert.Null(new Score().ScheduleId);
    }

    [Fact]
    public void ExactIndexes_KeepRosterComponentAndNonNullScheduleLookups()
    {
        using var context = RoleMappingTests.Create();
        var model = context.GetService<IDesignTimeModel>().Model;
        var a = Entity(model, "Attendance");
        Index(a, "UQ_Attendance_ScheduleAssignment", true, null, "schedule_id", "class_assignment_id");
        Index(a, "IX_Attendance_Assignment", false, null, "class_assignment_id", "attendance_id");
        Index(a, "IX_Attendance_EnteredBy", false, null, "entered_by");
        Assert.Equal(3, a.GetIndexes().Count());
        var s = Entity(model, "Score");
        Index(s, "UQ_Score_AssignmentComponent", true, null, "class_assignment_id", "component_id");
        Index(s, "IX_Score_Component", false, null, "component_id", "score_id");
        Index(s, "IX_Score_Schedule", false, "[schedule_id] IS NOT NULL", "schedule_id", "score_id");
        Index(s, "IX_Score_EnteredBy", false, null, "entered_by");
        Assert.Equal(4, s.GetIndexes().Count());
        var g = Entity(model, "GradeReport");
        Index(g, "UQ_GradeReport_Assignment", true, null, "class_assignment_id");
        Index(g, "IX_GradeReport_StateFinalized", false, null, "status", "finalized_at", "report_id");
        Index(g, "IX_GradeReport_SubmittedBy", false, null, "submitted_by");
        Index(g, "IX_GradeReport_ReviewedBy", false, null, "reviewed_by");
        Assert.Equal(4, g.GetIndexes().Count());
    }

    [Fact]
    public void ForeignKeys_KeepScalarOwnershipShapesAndNoCascade()
    {
        using var context = RoleMappingTests.Create();
        var model = context.GetService<IDesignTimeModel>().Model;
        var a = Entity(model, "Attendance");
        Foreign(a, "FK_Attendance_Schedule_schedule_id", "Schedule", true, false, "schedule_id", "schedule_id");
        Foreign(a, "FK_Attendance_StudentClassAssignment_class_assignment_id", "StudentClassAssignment", true, false, "class_assignment_id", "class_assignment_id");
        Foreign(a, "FK_Attendance_Account_entered_by", "Account", true, false, "entered_by", "account_id");
        Assert.Equal(3, a.GetForeignKeys().Count());
        var s = Entity(model, "Score");
        Foreign(s, "FK_Score_StudentClassAssignment_class_assignment_id", "StudentClassAssignment", true, false, "class_assignment_id", "class_assignment_id");
        Foreign(s, "FK_Score_ScoreComponentConfig_component_id", "ScoreComponentConfig", true, false, "component_id", "component_id");
        Foreign(s, "FK_Score_Schedule_schedule_id", "Schedule", false, false, "schedule_id", "schedule_id");
        Foreign(s, "FK_Score_Account_entered_by", "Account", true, false, "entered_by", "account_id");
        Assert.Equal(4, s.GetForeignKeys().Count());
        var g = Entity(model, "GradeReport");
        Foreign(g, "FK_GradeReport_StudentClassAssignment_class_assignment_id", "StudentClassAssignment", true, true, "class_assignment_id", "class_assignment_id");
        Foreign(g, "FK_GradeReport_Account_submitted_by", "Account", false, false, "submitted_by", "account_id");
        Foreign(g, "FK_GradeReport_Account_reviewed_by", "Account", false, false, "reviewed_by", "account_id");
        Assert.Equal(3, g.GetForeignKeys().Count());
        Assert.Equal(7, Expected.Keys.SelectMany(t => Entity(model, t).GetForeignKeys()).Count(f => f.IsRequired));
    }

    [Fact]
    public void ScoreRange_UsesDecimalZeroThroughHundredWithoutAttendanceStateCheck()
    {
        using var context = RoleMappingTests.Create();
        var model = context.GetService<IDesignTimeModel>().Model;
        Assert.Empty(Entity(model, "Attendance").GetCheckConstraints());
        var score = Assert.Single(Entity(model, "Score").GetCheckConstraints());
        Assert.Equal("CK_Score_Score", score.Name);
        Assert.Contains("[score_value] >= 0", score.Sql);
        Assert.Contains("[score_value] <= 100", score.Sql);
    }

    [Fact]
    public void GradeStatus_RejectsUnknownCaseAndPadding()
    {
        using var context = RoleMappingTests.Create();
        var sql = Check(context.GetService<IDesignTimeModel>().Model, "CK_GradeReport_Status");
        foreach (var state in new[] { "DRAFT", "SUBMITTED", "RETURNED", "FINALIZED" }) Assert.Contains("'" + state + "'", sql);
        Assert.Equal(8, sql.Count(c => c == '\''));
        Assert.Contains("COLLATE Latin1_General_100_BIN2", sql);
        Assert.Contains("DATALENGTH([status]) = LEN([status])", sql);
    }

    [Fact]
    public void FinalResult_RequiresScoreReviewTimeAndNullsBeforePublication()
    {
        using var context = RoleMappingTests.Create();
        var sql = Check(context.GetService<IDesignTimeModel>().Model, "CK_GradeReport_FinalResult");
        Assert.Contains("[status] = 'FINALIZED'", sql);
        Assert.Contains("[status] <> 'FINALIZED'", sql);
        foreach (var name in new[] { "final_score", "finalized_at", "reviewed_by", "reviewed_at", "review_operation_id" }) Assert.Contains("[" + name + "] IS NOT NULL", sql);
        Assert.Contains("[final_score] >= 0", sql);
        Assert.Contains("[final_score] <= 100", sql);
        Assert.Contains("[finalized_at] = [reviewed_at]", sql);
        Assert.Contains("[final_score] IS NULL", sql);
        Assert.Contains("[finalized_at] IS NULL", sql);
        Assert.Contains(" OR ", sql);
    }

    [Fact]
    public void SubmitBundle_RequiresLatestSubmitOutsideDraft()
    {
        using var context = RoleMappingTests.Create();
        var sql = Check(context.GetService<IDesignTimeModel>().Model, "CK_GradeReport_SubmitBundle");
        Assert.Contains("[status] = 'DRAFT'", sql);
        Bundle(sql, "submitted_by", "submitted_at", "submitted_operation_id");
    }

    [Fact]
    public void ReviewBundle_RejectsPartialReviewAndAllowsRetainedCompleteReview()
    {
        using var context = RoleMappingTests.Create();
        var sql = Check(context.GetService<IDesignTimeModel>().Model, "CK_GradeReport_ReviewBundle");
        Bundle(sql, "reviewed_by", "reviewed_at", "review_operation_id");
        Assert.DoesNotContain("[status]", sql);
    }

    [Fact]
    public void ReturnedReason_RequiresReviewWithoutClearingHistoryOnResubmit()
    {
        using var context = RoleMappingTests.Create();
        var sql = Check(context.GetService<IDesignTimeModel>().Model, "CK_GradeReport_ReturnedReason");
        Assert.Contains("[status] <> 'RETURNED'", sql);
        Assert.Contains("[returned_reason] IS NOT NULL", sql);
        Assert.Contains("LEN([returned_reason]) > 0", sql);
        foreach (var name in new[] { "reviewed_by", "reviewed_at", "review_operation_id" }) Assert.Contains("[" + name + "] IS NOT NULL", sql);
        Assert.DoesNotContain("[returned_reason] IS NULL", sql);
        Assert.Contains(" OR ", sql);
    }

    [Fact]
    public void D01D05_SubsetRetainsNineteenReviewedTables()
    {
        using var context = RoleMappingTests.Create();
        var model = context.GetService<IDesignTimeModel>().Model;
        Assert.Equal(new[] { "Account", "Attendance", "AuthSession", "Band", "BandAssignment", "Class", "Course", "EntryTestAttempt", "EntryTestBooking", "EntryTestSession", "GradeReport", "Registration", "Role", "Schedule", "Score", "ScoreComponentConfig", "StudentClassAssignment", "StudentProfile", "TrainingProgram" }, Prior(model).Select(e => e.GetTableName()).Order());
        Assert.Equal(180, Prior(model).Sum(e => e.GetProperties().Count()));
        Assert.Equal(62, Prior(model).Sum(e => e.GetIndexes().Count()));
        Assert.Equal(52, Prior(model).Sum(e => e.GetCheckConstraints().Count()));
        Assert.Equal(43, Prior(model).Sum(e => e.GetForeignKeys().Count()));
        Assert.Equal(19, Prior(model).Sum(e => e.GetKeys().Count(k => k.IsPrimaryKey())));
        Assert.Equal(2, Prior(model).Sum(e => e.GetKeys().Count(k => !k.IsPrimaryKey())));
        Assert.Equal(5, Entity(model, "GradeReport").GetCheckConstraints().Count());
        var ddl = context.Database.GenerateCreateScript();
        Assert.Equal(19, Prior(model).Count());
        Assert.Equal(17, Prior(model).SelectMany(e => e.GetProperties()).Count(p => p.GetColumnType() == "rowversion"));
        Assert.Contains("[present] bit NOT NULL", ddl);
        Assert.Contains("[score_value] decimal(5,2) NOT NULL", ddl);
        Assert.Contains("[final_score] decimal(5,2) NULL", ddl);
        Assert.Contains("CREATE INDEX [IX_Score_Schedule] ON [dbo].[Score] ([schedule_id], [score_id]) WHERE [schedule_id] IS NOT NULL", ddl);
        Assert.DoesNotContain("ON DELETE CASCADE", ddl);
        Assert.DoesNotContain("__EFMigrationsHistory", ddl);
        Assert.DoesNotContain("CREATE TRIGGER", ddl);
        Assert.Equal(System.Data.ConnectionState.Closed, context.Database.GetDbConnection().State);
        Assert.False(context.Database.CreateExecutionStrategy().RetriesOnFailure);
    }

    private static IEnumerable<IEntityType> Prior(IModel model) => model.GetEntityTypes().Where(e => e.ClrType.Namespace is not ("EnglishCenter.Api.Domain.Finance" or "EnglishCenter.Api.Domain.Administration"));
    private static IEntityType Entity(IModel model, string table) => model.GetEntityTypes().Single(e => e.GetTableName() == table);
    private static IProperty Column(IEntityType entity, string name) => entity.GetProperties().Single(p => p.GetColumnName() == name);
    private static string Check(IModel model, string name) => Entity(model, "GradeReport").GetCheckConstraints().Single(c => c.Name == name).Sql;
    private static void Bundle(string sql, params string[] members)
    {
        foreach (var member in members)
        {
            Assert.Contains("[" + member + "] IS NOT NULL", sql);
            Assert.Contains("[" + member + "] IS NULL", sql);
        }
        Assert.Contains(" OR ", sql);
    }
    private static void Index(IEntityType entity, string name, bool unique, string? filter, params string[] columns)
    {
        var index = entity.GetIndexes().Single(i => i.GetDatabaseName() == name);
        Assert.Equal(unique, index.IsUnique);
        Assert.Equal(filter, index.GetFilter());
        Assert.Equal(columns, index.Properties.Select(p => p.GetColumnName()));
    }
    private static void Foreign(IEntityType entity, string name, string target, bool required, bool unique, string column, string principal)
    {
        var fk = entity.GetForeignKeys().Single(f => f.GetConstraintName() == name);
        Assert.Equal(target, fk.PrincipalEntityType.GetTableName());
        Assert.Equal(required, fk.IsRequired);
        Assert.Equal(unique, fk.IsUnique);
        Assert.Equal(DeleteBehavior.NoAction, fk.DeleteBehavior);
        Assert.Equal(column, Assert.Single(fk.Properties).GetColumnName());
        Assert.Equal(principal, Assert.Single(fk.PrincipalKey.Properties).GetColumnName());
    }
}
