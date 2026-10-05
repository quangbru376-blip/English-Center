namespace EnglishCenter.SqlTests;

using System.Data;
using System.Runtime.CompilerServices;
using EnglishCenter.Api.Infrastructure;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Xunit;

public class RolePermissionTests
{
    private static readonly string[] Roles = ["ec_app_runtime", "ec_finance_reporting", "ec_academics_reporting", "ec_director_reporting"];
    private static readonly ConditionalWeakTable<SqlFixture, Principal> Principals = new();
    private sealed record Principal(string Name);
    private static readonly (string Table, string Rights)[] Tables =
    [
        ("Role", "S"), ("Band", "S"), ("Account", "SIU"), ("StudentProfile", "SIU"),
        ("AuthSession", "SIU"), ("Course", "SIU"), ("TrainingProgram", "SIU"),
        ("ScoreComponentConfig", "SIUD"), ("Registration", "SIU"), ("EntryTestSession", "SIU"),
        ("EntryTestAttempt", "SIU"), ("EntryTestBooking", "SIU"), ("BandAssignment", "SIU"),
        ("Class", "SIU"), ("Schedule", "SIU"), ("StudentClassAssignment", "SI"),
        ("Attendance", "SIU"), ("Score", "SIU"), ("GradeReport", "SIU"),
        ("Invoice", "SIU"), ("Payment", "SIU"), ("AuditLog", "SI"),
        ("ReplayOperation", "SI"), ("InvoiceNumberCounter", "")
    ];
    private static readonly string[] Views = ["vw_AdmissionsRegistrationFacts", "vw_EntryTestBookingFacts", "vw_CurrentScheduleOccurrences", "vw_TrainingClassFacts", "vw_FinalizedGradeFacts", "vw_FinanceInvoiceFacts"];

    public static IEnumerable<object[]> TableCases()
    {
        foreach (var (table, rights) in Tables)
            foreach (var operation in "SIUD")
                yield return [table, operation.ToString(), rights.Contains(operation)];
    }

    public static IEnumerable<object[]> ViewCases()
    {
        for (var role = 0; role < Roles.Length; role++)
            for (var view = 0; view < Views.Length; view++)
                yield return [role, view, role == 0 || role == 1 && view == 5 || role == 2 && view is 2 or 3];
    }

    private static async Task<SqlFixture> Fixture(int role)
    {
        var fixture = await SqlFixture.CreateAsync(isolation: IsolationLevel.Snapshot);
        try
        {
            // Static names from the accepted role matrix, never request input.
            var name = "D12_fixture_user_" + role;
            await SqlRows.ReadAsync(fixture, "IF DATABASE_PRINCIPAL_ID('" + name + "') IS NOT NULL THROW 51912,'Fixture user exists',1; CREATE USER [" + name + "] WITHOUT LOGIN; ALTER ROLE [" + Roles[role] + "] ADD MEMBER [" + name + "];");
            Principals.Add(fixture, new(name));
            return fixture;
        }
        catch { await fixture.DisposeAsync(); throw; }
    }

    private static async Task<List<SqlResult>> AsUser(SqlFixture fixture, string sql, params (string Name, object? Value)[] parameters)
    {
        await SqlRows.ReadAsync(fixture, "EXECUTE AS USER='" + Principals.GetValue(fixture, _ => throw new InvalidOperationException()).Name + "';");
        try { return await SqlRows.ReadAsync(fixture, sql, parameters); }
        finally { await SqlRows.ReadAsync(fixture, "REVERT;"); }
    }

    private static async Task Denied(SqlFixture fixture, string sql, params int[] expectedErrors)
    {
        var error = await Assert.ThrowsAsync<SqlException>(() => AsUser(fixture, sql));
        Assert.True((expectedErrors.Length == 0 ? new[] { 229, 262, 15151, 15247 } : expectedErrors).Contains(error.Number), sql + " -> " + error.Number + ": " + error.Message);
        var context = await SqlRows.ReadAsync(fixture, "SELECT USER_NAME() AS principalName;");
        Assert.NotEqual(Principals.GetValue(fixture, _ => throw new InvalidOperationException()).Name, context[0].Value(0, "principalName"));
    }

    [Theory]
    [MemberData(nameof(TableCases))]
    public async Task Runtime_ExactTableOperationMatrix(string table, string operation, bool allowed)
    {
        await using var fixture = await Fixture(0);
        var metadata = await SqlRows.ReadAsync(fixture, "SELECT name FROM sys.columns WHERE object_id=OBJECT_ID(@table) AND is_identity=0 AND is_computed=0 AND system_type_id<>189 ORDER BY column_id;", ("@table", "dbo." + table));
        var column = (string)metadata[0].Rows[0][0]!;
        var target = "[dbo].[" + table + "]";
        var sql = operation switch
        {
            "S" => "SELECT TOP(1) * FROM " + target + ";",
            "I" => "INSERT " + target + " ([" + column + "]) SELECT [" + column + "] FROM " + target + " WHERE 1=0;",
            "U" => "UPDATE " + target + " SET [" + column + "]=[" + column + "] WHERE 1=0;",
            "D" => "DELETE FROM " + target + " WHERE 1=0;",
            _ => throw new ArgumentOutOfRangeException(nameof(operation))
        };
        if (allowed) await AsUser(fixture, sql); else await Denied(fixture, sql);
    }

    [Theory]
    [MemberData(nameof(ViewCases))]
    public async Task Views_ExactAllowlistAndOwnershipChain(int role, int view, bool allowed)
    {
        await using var fixture = await Fixture(role);
        var sql = "SELECT * FROM dbo.[" + Views[view] + "];";
        if (!allowed) { await Denied(fixture, sql); return; }
        var expected = await SqlRows.ReadAsync(fixture, sql);
        var actual = await AsUser(fixture, sql);
        Assert.Equal(expected[0].Columns, actual[0].Columns);
        Assert.Equal(expected[0].Rows.Count, actual[0].Rows.Count);
        for (var i = 0; i < expected[0].Rows.Count; i++) Assert.Equal(expected[0].Rows[i], actual[0].Rows[i]);
        if (role != 0)
            Assert.DoesNotContain(actual[0].Columns, x => new[] { "passwordHash", "email", "phone", "fullName", "paymentReference", "paymentMethod" }.Contains(x));
    }

    [Theory]
    [InlineData(1)] [InlineData(2)] [InlineData(3)]
    public async Task Reporting_NoBaseTableReadsOrMutations(int role)
    {
        await using var fixture = await Fixture(role);
        foreach (var (table, _) in Tables)
        {
            await Denied(fixture, "SELECT * FROM dbo.[" + table + "];");
            await Denied(fixture, "DELETE FROM dbo.[" + table + "] WHERE 1=0;");
            var metadata = await SqlRows.ReadAsync(fixture, "SELECT TOP(1) name FROM sys.columns WHERE object_id=OBJECT_ID(@table) AND is_identity=0 AND is_computed=0 AND system_type_id<>189 ORDER BY column_id;", ("@table", "dbo." + table));
            var column = (string)metadata[0].Rows[0][0]!;
            await Denied(fixture, "UPDATE dbo.[" + table + "] SET [" + column + "]=[" + column + "] WHERE 1=0;");
        }
        var denies = await SqlRows.ReadAsync(fixture, "SELECT OBJECT_NAME(major_id) AS objectName FROM sys.database_permissions WHERE grantee_principal_id=DATABASE_PRINCIPAL_ID(@role) AND permission_name='SELECT' AND state='D' ORDER BY objectName;", ("@role", Roles[role]));
        Assert.Equal(new[] { "Account", "GradeReport", "Score", "StudentProfile" }, denies[0].Rows.Select(x => (string)x[0]!));
    }

    [Theory]
    [InlineData(0)] [InlineData(1)] [InlineData(2)] [InlineData(3)]
    public async Task NoDboFixedRolesDdlGrantOrImpersonation(int role)
    {
        await using var fixture = await Fixture(role);
        var identity = await AsUser(fixture, "SELECT USER_NAME() AS principalName, IS_ROLEMEMBER('db_owner') AS ownerRole, IS_ROLEMEMBER('db_ddladmin') AS ddlRole, HAS_PERMS_BY_NAME('dbo.Course','OBJECT','ALTER') AS alterRight, HAS_PERMS_BY_NAME('dbo.Account','OBJECT','CONTROL') AS controlRight, HAS_PERMS_BY_NAME('dbo.__EFMigrationsHistory','OBJECT','DELETE') AS historyDeleteRight;");
        Assert.Equal("D12_fixture_user_" + role, identity[0].Value(0, "principalName"));
        Assert.Equal(0, identity[0].Value(0, "ownerRole")); Assert.Equal(0, identity[0].Value(0, "ddlRole"));
        Assert.Equal(0, identity[0].Value(0, "alterRight"));
        Assert.Equal(0, identity[0].Value(0, "controlRight"));
        Assert.Equal(0, identity[0].Value(0, "historyDeleteRight"));
        await Denied(fixture, "CREATE TABLE dbo.D12_forbidden(id int);");
        // This isolated owner context can report principal-resolution error 15517.
        // Check ALTER permission separately, so that error is not treated as permission proof.
        await Denied(fixture, "ALTER TABLE dbo.Course ADD D12_forbidden int;", 229, 15151, 15517);
        await Denied(fixture, "DELETE FROM dbo.__EFMigrationsHistory WHERE 1=0;", 229, 15517);
    }

    [Theory]
    [InlineData(0)] [InlineData(1)] [InlineData(2)] [InlineData(3)]
    public async Task NoRoleCanDelegateAccountReadGrant(int role)
    {
        await using var fixture = await Fixture(role);
        await Denied(fixture, "GRANT SELECT ON dbo.Account TO public;", 229, 15151, 15247, 15517);
    }

    [Theory]
    [InlineData(0)] [InlineData(1)] [InlineData(2)] [InlineData(3)]
    public async Task NoRoleCanImpersonateSeparateDatabasePrincipal(int role)
    {
        await using var fixture = await Fixture(role);
        var otherRole = (role + 1) % Roles.Length;
        var otherPrincipal = "D12_fixture_user_" + otherRole;
        await SqlRows.ReadAsync(fixture, "IF DATABASE_PRINCIPAL_ID('" + otherPrincipal + "') IS NOT NULL THROW 51912,'Probe exists',1; CREATE USER [" + otherPrincipal + "] WITHOUT LOGIN; ALTER ROLE [" + Roles[otherRole] + "] ADD MEMBER [" + otherPrincipal + "];");
        await Denied(fixture, "EXECUTE AS USER='" + otherPrincipal + "';", 15517, 15151);
    }

    private static string ReportSql(int sp) => sp switch
    {
        4 => "EXEC dbo.usp_GetFinancialReport @fromUtc='2026-10-05',@toExclusiveUtc='2026-10-06',@courseId=NULL,@overdueOnly=0,@todayVn='2026-10-05',@page=1,@pageSize=20;",
        5 => "EXEC dbo.usp_GetAdmissionsReport @fromUtc='2026-10-05',@toExclusiveUtc='2026-10-06',@courseId=NULL,@status=NULL,@page=1,@pageSize=20;",
        6 => "EXEC dbo.usp_GetTrainingReport @courseId=NULL,@bandId=NULL,@classId=NULL,@teacherId=NULL,@status=NULL,@finalizedFromUtc='2026-10-05',@finalizedToExclusiveUtc='2026-10-06',@page=1,@pageSize=20;",
        _ => throw new ArgumentOutOfRangeException(nameof(sp))
    };

    [Theory]
    [InlineData(0)] [InlineData(1)] [InlineData(2)] [InlineData(3)]
    public async Task Reports_AllowedOutputsEqualOperatorWithoutPrivilegedSelectors(int role)
    {
        await using var fixture = await Fixture(role);
        for (var sp = 4; sp <= 6; sp++)
        {
            var sql = ReportSql(sp);
            if (role == 1 && sp != 4 || role == 2 && sp != 6) { await Denied(fixture, sql); continue; }
            var expected = await SqlRows.ReadAsync(fixture, sql);
            var actual = await AsUser(fixture, sql);
            Assert.Equal(expected.Count, actual.Count);
            for (var set = 0; set < expected.Count; set++)
            {
                Assert.Equal(expected[set].Columns, actual[set].Columns);
                Assert.Equal(expected[set].Rows.Count, actual[set].Rows.Count);
                for (var row = 0; row < expected[set].Rows.Count; row++) Assert.Equal(expected[set].Rows[row], actual[set].Rows[row]);
                Assert.DoesNotContain(actual[set].Columns, x => new[] { "studentId", "registrationId", "invoiceId", "paymentId", "email", "phone", "passwordHash" }.Contains(x));
            }
        }
    }

    [Theory]
    [InlineData(1)] [InlineData(2)] [InlineData(3)]
    public async Task Reporting_CannotExecuteOperationalReadsAllocatorOrDirectUdf(int role)
    {
        await using var fixture = await Fixture(role);
        await Denied(fixture, "EXEC dbo.usp_SearchAdmissions;");
        await Denied(fixture, "EXEC dbo.usp_GetEntryTestRoster;");
        await Denied(fixture, "EXEC dbo.usp_GetClassGradeReview;");
        await Denied(fixture, "EXEC dbo.usp_AllocateInvoiceNumber @issueYear=9998;");
        await Denied(fixture, "SELECT * FROM dbo.ufn_BandForScore(50);");
    }

    [Fact]
    public async Task Runtime_OperationalReadExecutionReachesBusinessScopeChecks()
    {
        await using var fixture = await Fixture(0);
        await SqlRows.ReadAsync(fixture, "UPDATE dbo.Role SET role_code='Admissions' WHERE role_id=@role;", ("@role", fixture.Teacher.RoleId));
        var admissions = await AsUser(fixture, "EXEC dbo.usp_SearchAdmissions @actorId=@actor,@courseId=NULL,@status=NULL,@createdFromUtc=NULL,@createdToExclusiveUtc=NULL,@name=NULL,@email=NULL,@phone=NULL,@sort='CREATED_ASC',@page=1,@pageSize=20;", ("@actor", fixture.Teacher.AccountId));
        Assert.Equal(2, admissions.Count);
        var roster = await Assert.ThrowsAsync<SqlException>(() => AsUser(fixture, "EXEC dbo.usp_GetEntryTestRoster @actorId=@actor,@sessionId=0,@asOfUtc='2026-10-05',@view='history',@sort='BOOKED_ASC',@page=1,@pageSize=20;", ("@actor", fixture.Teacher.AccountId)));
        Assert.Equal(51101, roster.Number);
        var grade = await Assert.ThrowsAsync<SqlException>(() => AsUser(fixture, "EXEC dbo.usp_GetClassGradeReview @actorId=@actor,@classId=@class;", ("@actor", fixture.Teacher.AccountId), ("@class", fixture.Class.ClassId)));
        Assert.Equal(51101, grade.Number);
    }

    private static Migration RolesMigration(SqlFixture fixture)
    {
        var assembly = fixture.Context.GetService<IMigrationsAssembly>();
        var type = Assert.Single(assembly.Migrations, x => x.Key.EndsWith("_LeastPrivilegeRoles", StringComparison.Ordinal)).Value;
        return assembly.CreateMigration(type, fixture.Context.Database.ProviderName!);
    }

    [Theory]
    [InlineData(0)] [InlineData(1)] [InlineData(2)] [InlineData(3)]
    public async Task ExistingRole_IsRejectedWithoutAdoptingItsPermissions(int role)
    {
        await using var fixture = await Fixture(role);
        var operation = Assert.IsType<SqlOperation>(RolesMigration(fixture).UpOperations[role]);
        var error = await Assert.ThrowsAsync<SqlException>(() => SqlRows.ReadAsync(fixture, operation.Sql));
        Assert.Equal(51212, error.Number);
    }

    [Fact]
    public async Task Down_RefusesAnyRoleMembershipBeforeDroppingRoles()
    {
        await using var fixture = await Fixture(0);
        var operation = Assert.IsType<SqlOperation>(Assert.Single(RolesMigration(fixture).DownOperations));
        var error = await Assert.ThrowsAsync<SqlException>(() => SqlRows.ReadAsync(fixture, operation.Sql));
        Assert.Equal(51213, error.Number);
        var roles = await SqlRows.ReadAsync(fixture, "SELECT name FROM sys.database_principals WHERE name IN ('ec_app_runtime','ec_finance_reporting','ec_academics_reporting','ec_director_reporting');");
        Assert.Equal(4, roles[0].Rows.Count);
    }

    [Fact]
    public async Task Runtime_AllocatorOwnershipChainWorksWithoutDirectCounterGrant()
    {
        await using var fixture = await Fixture(0);
        await Denied(fixture, "SELECT * FROM dbo.InvoiceNumberCounter;");
        var result = await AsUser(fixture, "EXEC dbo.usp_AllocateInvoiceNumber @issueYear=9998;");
        Assert.Equal("HD-9998-00000001", result[0].Value(0, "invoiceNumber"));
        await Denied(fixture, "UPDATE dbo.InvoiceNumberCounter SET last_value=0 WHERE issue_year=9998;");
    }

    [Fact]
    public async Task Runtime_AllSixUdfPermissionsExecuteActualReads()
    {
        await using var fixture = await Fixture(0);
        var scalar = await AsUser(fixture, "SELECT dbo.ufn_EffectiveEntryTestState('CANCELLED','2026-10-05','2026-10-06') AS state;");
        Assert.Equal("CANCELLED", scalar[0].Value(0, "state"));
        foreach (var sql in new[] { "SELECT * FROM dbo.ufn_BandForScore(50);", "SELECT * FROM dbo.ufn_RegistrationUnfinished(@id);", "SELECT * FROM dbo.ufn_ClassObligationProgress(@class);", "SELECT * FROM dbo.ufn_GradeTotal(@assignment);", "SELECT * FROM dbo.ufn_InvoiceDebtFlags('UNPAID','2026-10-04','2026-10-05');" })
            Assert.Single(await AsUser(fixture, sql, ("@id", fixture.Registration.RegistrationId), ("@class", fixture.Class.ClassId), ("@assignment", fixture.Assignment.ClassAssignmentId)));
    }

    [Fact]
    public async Task Runtime_ActualConfigurationWritesAndDraftComponentDeleteRollback()
    {
        await using var fixture = await Fixture(0);
        await AsUser(fixture, "UPDATE dbo.Course SET course_name=N'D12 permitted update' WHERE course_id=@course; INSERT dbo.ScoreComponentConfig(program_id,component_name,kind,weight) VALUES(@program,N'D12 draft component','GENERAL',100); DELETE FROM dbo.ScoreComponentConfig WHERE program_id=@program;", ("@course", fixture.Course.CourseId), ("@program", fixture.UnusedProgram.ProgramId));
        var course = await AsUser(fixture, "SELECT course_name FROM dbo.Course WHERE course_id=@course;", ("@course", fixture.Course.CourseId));
        Assert.Equal("D12 permitted update", course[0].Rows[0][0]);
        var components = await AsUser(fixture, "SELECT * FROM dbo.ScoreComponentConfig WHERE program_id=@program;", ("@program", fixture.UnusedProgram.ProgramId));
        Assert.Empty(components[0].Rows);
    }

    [Fact]
    public async Task Reporting_DenyOverridesDirectUserGrantWhileAllowedOwnershipChainWorks()
    {
        await using var fixture = await Fixture(1);
        await SqlRows.ReadAsync(fixture, "GRANT SELECT ON dbo.Account TO [D12_fixture_user_1];");
        await Denied(fixture, "SELECT * FROM dbo.Account;", 229);
        await AsUser(fixture, "SELECT * FROM dbo.vw_FinanceInvoiceFacts;");
    }

    [Fact]
    public async Task Reporting_RevokeRemovesSoleAllowedViewPathThenGrantRestoresIt()
    {
        await using var fixture = await Fixture(1);
        await AsUser(fixture, "SELECT * FROM dbo.vw_FinanceInvoiceFacts;");
        await SqlRows.ReadAsync(fixture, "REVOKE SELECT ON dbo.vw_FinanceInvoiceFacts FROM ec_finance_reporting;");
        await Denied(fixture, "SELECT * FROM dbo.vw_FinanceInvoiceFacts;");
        await SqlRows.ReadAsync(fixture, "GRANT SELECT ON dbo.vw_FinanceInvoiceFacts TO ec_finance_reporting;");
        await AsUser(fixture, "SELECT * FROM dbo.vw_FinanceInvoiceFacts;");
        await Denied(fixture, "SELECT * FROM dbo.Account;");
    }
}
