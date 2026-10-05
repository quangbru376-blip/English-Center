namespace EnglishCenter.Tests.Infrastructure;

using EnglishCenter.Api.Domain.Finance;
using EnglishCenter.Api.Domain.Administration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Xunit;

public class FinanceInfrastructureMappingTests
{
    private static readonly Dictionary<string, string> Expected = new()
    {
        ["Invoice"] = "invoice_id:int invoice_number:varchar(16) registration_id:int amount:decimal(18,0) issued_at:datetime2(3) due_date:date status:varchar(16) cancelled_by:int? cancelled_at:datetime2(3)? cancellation_reason:nvarchar(2000)? cancelled_from_status:varchar(16)? cancellation_operation_id:uniqueidentifier? row_version:rowversion",
        ["Payment"] = "payment_id:int invoice_id:int amount_paid:decimal(18,0) method:varchar(16) transaction_reference:nvarchar(200)? confirmed_by:int paid_at:datetime2(3) last_corrected_by:int? last_corrected_at:datetime2(3)? last_correction_reason:nvarchar(2000)? row_version:rowversion",
        ["AuditLog"] = "audit_id:int actor_account_id:int? action:varchar(80) entity_type:varchar(64) entity_id:varchar(100)? result:varchar(16) details:nvarchar(max)? occurred_at:datetime2(3) ip_address:varchar(45)? operation_id:uniqueidentifier audit_category:varchar(16) security_expires_at:datetime2(3)? ip_expires_at:datetime2(3)?",
        ["ReplayOperation"] = "replay_id:int actor_account_id:int action_code:varchar(80) request_id:uniqueidentifier input_digest:binary(32) operation_id:uniqueidentifier result_mode:varchar(32) result_reference:nvarchar(max) completed_at:datetime2(3) expires_at:datetime2(3)",
        ["InvoiceNumberCounter"] = "issue_year:smallint last_value:int row_version:rowversion"
    };

    [Theory]
    [InlineData("Invoice", "invoice_id")]
    [InlineData("Payment", "payment_id")]
    [InlineData("AuditLog", "audit_id")]
    [InlineData("ReplayOperation", "replay_id")]
    [InlineData("InvoiceNumberCounter", "issue_year")]
    public void PhysicalDictionary_MatchesAllFiftyColumnsAndKeyGeneration(string table, string id)
    {
        using var context = RoleMappingTests.Create();
        var entity = Entity(context.GetService<IDesignTimeModel>().Model, table);
        var columns = entity.GetProperties().ToDictionary(p => p.GetColumnName());
        var expected = Expected[table].Split(' ').Select(x => x.Split(':')).ToDictionary(x => x[0], x => x[1]);
        Assert.Equal(expected.Keys.Order(), columns.Keys.Order());
        Assert.Equal("dbo", entity.GetSchema());
        foreach (var (name, type) in expected)
        {
            var column = columns[name]; var sql = type.TrimEnd('?');
            Assert.Equal(sql, column.GetColumnType());
            Assert.Equal(type.EndsWith('?'), column.IsNullable);
            Assert.False(column.IsShadowProperty());
            if (sql.StartsWith("varchar", StringComparison.Ordinal)) Assert.False(column.IsUnicode());
            if (sql.StartsWith("nvarchar", StringComparison.Ordinal)) Assert.True(column.IsUnicode());
            if (sql.Contains("char", StringComparison.Ordinal) && !sql.Contains("max", StringComparison.Ordinal))
                Assert.Equal(int.Parse(sql.Split('(')[1].TrimEnd(')')), column.GetMaxLength());
            if (sql == "datetime2(3)") Assert.Equal(3, column.GetPrecision());
            if (sql == "decimal(18,0)") { Assert.Equal(18, column.GetPrecision()); Assert.Equal(0, column.GetScale()); Assert.Equal(typeof(decimal), column.ClrType); }
            if (sql == "date") Assert.Equal(typeof(DateOnly), column.ClrType);
            if (sql == "nvarchar(max)") Assert.Equal(typeof(string), column.ClrType);
        }
        var key = Assert.Single(entity.FindPrimaryKey()!.Properties);
        Assert.Equal(id, key.GetColumnName());
        if (table == "InvoiceNumberCounter")
        {
            Assert.Equal(typeof(short), key.ClrType);
            Assert.Equal(ValueGenerated.Never, key.ValueGenerated);
            Assert.NotEqual(SqlServerValueGenerationStrategy.IdentityColumn, key.GetValueGenerationStrategy());
        }
        else
        {
            Assert.Equal(SqlServerValueGenerationStrategy.IdentityColumn, key.GetValueGenerationStrategy());
            Assert.Equal(1L, key.GetIdentitySeed()); Assert.Equal(1, key.GetIdentityIncrement());
            Assert.Equal(ValueGenerated.OnAdd, key.ValueGenerated);
        }
        Assert.Single(entity.GetKeys()); Assert.Empty(entity.GetSeedData());
    }

    [Fact]
    public void Rowversions_ExistOnlyForInvoicePaymentAndCounter()
    {
        using var context = RoleMappingTests.Create(); var model = context.GetService<IDesignTimeModel>().Model;
        foreach (var table in new[] { "Invoice", "Payment", "InvoiceNumberCounter" })
        {
            var rv = Column(Entity(model, table), "row_version");
            Assert.True(rv.IsConcurrencyToken); Assert.Equal(ValueGenerated.OnAddOrUpdate, rv.ValueGenerated);
            Assert.Equal(PropertySaveBehavior.Ignore, rv.GetBeforeSaveBehavior()); Assert.Equal(PropertySaveBehavior.Ignore, rv.GetAfterSaveBehavior());
        }
        foreach (var table in new[] { "AuditLog", "ReplayOperation" }) Assert.DoesNotContain(Entity(model, table).GetProperties(), p => p.IsConcurrencyToken);
    }

    [Fact]
    public void Defaults_ContainOnlyInvoiceStatusAndInitialCounterValue()
    {
        using var context = RoleMappingTests.Create(); var model = context.GetService<IDesignTimeModel>().Model;
        foreach (var table in Expected.Keys)
        {
            var allowed = table switch { "Invoice" => new[] { "status" }, "InvoiceNumberCounter" => new[] { "last_value" }, _ => [] };
            var defaults = Entity(model, table).GetProperties().Where(p => p.FindAnnotation(RelationalAnnotationNames.DefaultValue) != null || p.GetDefaultValueSql() != null);
            Assert.Equal(allowed, defaults.Select(p => p.GetColumnName()));
        }
        var status = Column(Entity(model, "Invoice"), "status");
        Assert.Equal("UNPAID", status.GetDefaultValue()); Assert.Equal(string.Empty, status.Sentinel);
        Assert.Equal(0, Column(Entity(model, "InvoiceNumberCounter"), "last_value").GetDefaultValue());
        foreach (var table in new[] { "AuditLog", "ReplayOperation" })
            Assert.All(Entity(model, table).GetProperties().Where(p => !p.IsPrimaryKey()), p => Assert.Equal(ValueGenerated.Never, p.ValueGenerated));
        Assert.Null(new Invoice().CancelledAt); Assert.Null(new Payment().LastCorrectedAt);
        Assert.Empty(new ReplayOperation().InputDigest);
    }

    [Fact]
    public void Indexes_PreserveFinanceUniquenessAndAuditPagingDirections()
    {
        using var context = RoleMappingTests.Create(); var model = context.GetService<IDesignTimeModel>().Model;
        var i = Entity(model, "Invoice");
        Index(i, "UQ_Invoice_Number", true, "invoice_number"); Index(i, "UQ_Invoice_Registration", true, "registration_id");
        Index(i, "IX_Invoice_StateDue", false, "status", "due_date", "invoice_id"); Index(i, "IX_Invoice_Issued", false, "issued_at", "invoice_id"); Index(i, "IX_Invoice_CancelledBy", false, "cancelled_by"); Assert.Equal(5, i.GetIndexes().Count());
        var p = Entity(model, "Payment");
        Index(p, "UQ_Payment_Invoice", true, "invoice_id"); Index(p, "IX_Payment_Paid", false, "paid_at", "payment_id"); Index(p, "IX_Payment_ConfirmedBy", false, "confirmed_by"); Index(p, "IX_Payment_LastCorrectedBy", false, "last_corrected_by"); Assert.Equal(4, p.GetIndexes().Count());
        Assert.DoesNotContain(p.GetIndexes(), x => x.Properties.Any(c => c.GetColumnName() == "transaction_reference"));
        var a = Entity(model, "AuditLog");
        Direction(Index(a, "IX_Audit_Time", false, "occurred_at", "audit_id"), true, true);
        Direction(Index(a, "IX_Audit_Operation", false, "operation_id", "audit_id"), false, false);
        Direction(Index(a, "IX_Audit_ActorTime", false, "actor_account_id", "occurred_at", "audit_id"), false, true, true);
        Direction(Index(a, "IX_Audit_EntityTime", false, "entity_type", "entity_id", "occurred_at", "audit_id"), false, false, true, true);
        Assert.Equal(4, a.GetIndexes().Count());
        var r = Entity(model, "ReplayOperation");
        Index(r, "UQ_Replay_ActorActionRequest", true, "actor_account_id", "action_code", "request_id"); Index(r, "IX_Replay_Expiry", false, "expires_at"); Index(r, "IX_Replay_Operation", false, "operation_id"); Assert.Equal(3, r.GetIndexes().Count());
        Assert.Empty(Entity(model, "InvoiceNumberCounter").GetIndexes());
    }

    [Fact]
    public void ForeignKeys_KeepUniqueFinanceLinksAndNullableUnverifiedAuditActor()
    {
        using var context = RoleMappingTests.Create(); var model = context.GetService<IDesignTimeModel>().Model;
        var i = Entity(model, "Invoice"); Foreign(i, "FK_Invoice_Registration_registration_id", "Registration", true, true, "registration_id", "registration_id"); Foreign(i, "FK_Invoice_Account_cancelled_by", "Account", false, false, "cancelled_by", "account_id"); Assert.Equal(2, i.GetForeignKeys().Count());
        var p = Entity(model, "Payment"); Foreign(p, "FK_Payment_Invoice_invoice_id", "Invoice", true, true, "invoice_id", "invoice_id"); Foreign(p, "FK_Payment_Account_confirmed_by", "Account", true, false, "confirmed_by", "account_id"); Foreign(p, "FK_Payment_Account_last_corrected_by", "Account", false, false, "last_corrected_by", "account_id"); Assert.Equal(3, p.GetForeignKeys().Count());
        Foreign(Entity(model, "AuditLog"), "FK_AuditLog_Account_actor_account_id", "Account", false, false, "actor_account_id", "account_id"); Assert.Single(Entity(model, "AuditLog").GetForeignKeys());
        Foreign(Entity(model, "ReplayOperation"), "FK_ReplayOperation_Account_actor_account_id", "Account", true, false, "actor_account_id", "account_id"); Assert.Single(Entity(model, "ReplayOperation").GetForeignKeys());
        Assert.Empty(Entity(model, "InvoiceNumberCounter").GetForeignKeys());
        Assert.Equal(4, Expected.Keys.SelectMany(t => Entity(model, t).GetForeignKeys()).Count(f => f.IsRequired));
    }

    [Theory]
    [InlineData("Invoice", "CK_Invoice_Status", "status", "UNPAID,PAID,CANCELLED")]
    [InlineData("Payment", "CK_Payment_Method", "method", "CASH,TRANSFER")]
    [InlineData("AuditLog", "CK_AuditLog_Result", "result", "SUCCESS,FAILURE")]
    [InlineData("AuditLog", "CK_AuditLog_Category", "audit_category", "MANDATORY,SECURITY")]
    [InlineData("ReplayOperation", "CK_ReplayOperation_Mode", "result_mode", "SAFE_REFERENCE,SECRET_RESULT_UNAVAILABLE")]
    public void ExactEnums_RejectUnknownCaseAndPadding(string table, string check, string column, string literals)
    {
        using var context = RoleMappingTests.Create(); var sql = Check(context.GetService<IDesignTimeModel>().Model, table, check);
        foreach (var literal in literals.Split(',')) Assert.Contains("'" + literal + "'", sql);
        Assert.Equal(literals.Split(',').Length * 2, sql.Count(c => c == '\''));
        Assert.Contains("COLLATE Latin1_General_100_BIN2", sql); Assert.Contains("DATALENGTH([" + column + "]) = LEN([" + column + "])", sql);
    }

    [Fact]
    public void InvoiceNumberAndCancellation_KeepPatternAndOptionalParentReason()
    {
        using var context = RoleMappingTests.Create(); var model = context.GetService<IDesignTimeModel>().Model;
        Assert.Contains("[amount] > 0", Check(model, "Invoice", "CK_Invoice_Amount"));
        var pattern = Check(model, "Invoice", "CK_Invoice_NumberPattern");
        Assert.Contains("DATALENGTH([invoice_number]) = 16", pattern); Assert.Contains("DATALENGTH([invoice_number]) = LEN([invoice_number])", pattern);
        Assert.Contains("COLLATE Latin1_General_100_BIN2", pattern); Assert.Contains("'HD-[0-9][0-9][0-9][0-9]-[0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]'", pattern);
        var sql = Check(model, "Invoice", "CK_Invoice_CancelledBundle");
        foreach (var member in new[] { "cancelled_by", "cancelled_at", "cancelled_from_status", "cancellation_operation_id" }) { Assert.Contains("[" + member + "] IS NOT NULL", sql); Assert.Contains("[" + member + "] IS NULL", sql); }
        Assert.Contains("[cancellation_reason] IS NULL", sql); Assert.DoesNotContain("[cancellation_reason] IS NOT NULL", sql);
        Assert.Contains("[status] = 'CANCELLED'", sql); Assert.Contains("[status] <> 'CANCELLED'", sql);
        Assert.Contains("[cancelled_from_status] COLLATE Latin1_General_100_BIN2 = 'UNPAID'", sql);
        Assert.Contains("DATALENGTH([cancelled_from_status]) = LEN([cancelled_from_status])", sql);
    }

    [Fact]
    public void PaymentReferenceAndCorrection_KeepCashOptionalAndFullCorrectionBundle()
    {
        using var context = RoleMappingTests.Create(); var model = context.GetService<IDesignTimeModel>().Model;
        Assert.Contains("[amount_paid] > 0", Check(model, "Payment", "CK_Payment_Amount"));
        var reference = Check(model, "Payment", "CK_Payment_Reference");
        Assert.Contains("[method] <> 'TRANSFER'", reference); Assert.Contains("[transaction_reference] IS NOT NULL", reference); Assert.Contains("LEN([transaction_reference]) > 0", reference); Assert.DoesNotContain("[transaction_reference] IS NULL", reference);
        var correction = Check(model, "Payment", "CK_Payment_CorrectionBundle");
        foreach (var member in new[] { "last_corrected_by", "last_corrected_at", "last_correction_reason" }) { Assert.Contains("[" + member + "] IS NULL", correction); Assert.Contains("[" + member + "] IS NOT NULL", correction); }
        Assert.Contains("LEN([last_correction_reason]) > 0", correction); Assert.Contains(" OR ", correction);
    }

    [Fact]
    public void AuditRetention_KeepsMandatoryRowsWhileIpCanExpire()
    {
        using var context = RoleMappingTests.Create(); var model = context.GetService<IDesignTimeModel>().Model;
        var retention = Check(model, "AuditLog", "CK_AuditLog_SecurityExpiry");
        Assert.Contains("[audit_category] = 'SECURITY'", retention); Assert.Contains("[security_expires_at] IS NOT NULL", retention);
        Assert.Contains("[security_expires_at] >= DATEADD(day, 90, [occurred_at])", retention);
        Assert.Contains("[audit_category] = 'MANDATORY'", retention); Assert.Contains("[security_expires_at] IS NULL", retention);
        var ip = Check(model, "AuditLog", "CK_AuditLog_IPBundle");
        foreach (var member in new[] { "ip_address", "ip_expires_at" }) { Assert.Contains("[" + member + "] IS NOT NULL", ip); Assert.Contains("[" + member + "] IS NULL", ip); }
        Assert.Contains("[ip_expires_at] = DATEADD(day, 90, [occurred_at])", ip); Assert.DoesNotContain("[audit_category]", ip);
    }

    [Fact]
    public void JsonDigestAndReplayExpiry_KeepStringStorageAndMinimumRetention()
    {
        using var context = RoleMappingTests.Create(); var model = context.GetService<IDesignTimeModel>().Model;
        var details = Check(model, "AuditLog", "CK_AuditLog_Details"); Assert.Contains("[details] IS NULL", details); Assert.Contains("ISJSON([details]) = 1", details);
        Assert.Contains("ISJSON([result_reference]) = 1", Check(model, "ReplayOperation", "CK_ReplayOperation_ResultReference"));
        Assert.Contains("[expires_at] >= DATEADD(hour, 24, [completed_at])", Check(model, "ReplayOperation", "CK_ReplayOperation_Expiry"));
        var digest = Column(Entity(model, "ReplayOperation"), "input_digest"); Assert.Equal(typeof(byte[]), digest.ClrType); Assert.Equal(32, digest.GetMaxLength()); Assert.True(digest.IsFixedLength());
    }

    [Fact]
    public void CounterRanges_DoNotUseIdentityAsInvoiceSequence()
    {
        using var context = RoleMappingTests.Create(); var model = context.GetService<IDesignTimeModel>().Model;
        Assert.Contains("[issue_year] BETWEEN 1 AND 9999", Check(model, "InvoiceNumberCounter", "CK_InvoiceNumberCounter_Year"));
        Assert.Contains("[last_value] BETWEEN 0 AND 99999999", Check(model, "InvoiceNumberCounter", "CK_InvoiceNumberCounter_Value"));
    }

    [Fact]
    public void CompleteBaselineModel_ContainsTwentyFourTablesWithoutDatabaseIo()
    {
        using var context = RoleMappingTests.Create(); var model = context.GetService<IDesignTimeModel>().Model;
        Assert.Equal(new[] { "Account", "Attendance", "AuditLog", "AuthSession", "Band", "BandAssignment", "Class", "Course", "EntryTestAttempt", "EntryTestBooking", "EntryTestSession", "GradeReport", "Invoice", "InvoiceNumberCounter", "Payment", "Registration", "ReplayOperation", "Role", "Schedule", "Score", "ScoreComponentConfig", "StudentClassAssignment", "StudentProfile", "TrainingProgram" }, model.GetEntityTypes().Select(e => e.GetTableName()).Order());
        Assert.Equal(230, model.GetEntityTypes().Sum(e => e.GetProperties().Count())); Assert.Equal(78, model.GetEntityTypes().Sum(e => e.GetIndexes().Count()));
        Assert.Equal(70, model.GetEntityTypes().Sum(e => e.GetCheckConstraints().Count())); Assert.Equal(50, model.GetEntityTypes().Sum(e => e.GetForeignKeys().Count()));
        Assert.Equal(24, model.GetEntityTypes().Sum(e => e.GetKeys().Count(k => k.IsPrimaryKey()))); Assert.Equal(2, model.GetEntityTypes().Sum(e => e.GetKeys().Count(k => !k.IsPrimaryKey())));
        foreach (var (table, checks) in new[] { ("Invoice", 4), ("Payment", 4), ("AuditLog", 5), ("ReplayOperation", 3), ("InvoiceNumberCounter", 2) }) Assert.Equal(checks, Entity(model, table).GetCheckConstraints().Count());
        var ddl = context.Database.GenerateCreateScript(); Assert.Equal(24, ddl.Split("CREATE TABLE", StringSplitOptions.None).Length - 1); Assert.Equal(20, ddl.Split("rowversion NOT NULL", StringSplitOptions.None).Length - 1);
        Assert.Contains("[input_digest] binary(32) NOT NULL", ddl); Assert.Contains("[result_reference] nvarchar(max) NOT NULL", ddl); Assert.Contains("[issue_year] smallint NOT NULL", ddl); Assert.DoesNotContain("[issue_year] smallint NOT NULL IDENTITY", ddl);
        Assert.Contains("([occurred_at] DESC, [audit_id] DESC)", ddl); Assert.DoesNotContain("ON DELETE CASCADE", ddl); Assert.DoesNotContain("__EFMigrationsHistory", ddl); Assert.DoesNotContain("CREATE TRIGGER", ddl);
        Assert.Equal(System.Data.ConnectionState.Closed, context.Database.GetDbConnection().State); Assert.False(context.Database.CreateExecutionStrategy().RetriesOnFailure);
    }

    private static IEntityType Entity(IModel model, string table) => model.GetEntityTypes().Single(e => e.GetTableName() == table);
    private static IProperty Column(IEntityType entity, string name) => entity.GetProperties().Single(p => p.GetColumnName() == name);
    private static string Check(IModel model, string table, string name) => Entity(model, table).GetCheckConstraints().Single(c => c.Name == name).Sql;
    private static IIndex Index(IEntityType entity, string name, bool unique, params string[] columns)
    {
        var index = entity.GetIndexes().Single(i => i.GetDatabaseName() == name); Assert.Equal(unique, index.IsUnique); Assert.Null(index.GetFilter()); Assert.Equal(columns, index.Properties.Select(p => p.GetColumnName())); return index;
    }
    private static void Direction(IIndex index, params bool[] directions) => Assert.Equal(directions, index.IsDescending == null ? Enumerable.Repeat(false, directions.Length) : index.IsDescending.Count == 0 ? Enumerable.Repeat(true, directions.Length) : index.IsDescending);
    private static void Foreign(IEntityType entity, string name, string target, bool required, bool unique, string column, string principal)
    {
        var fk = entity.GetForeignKeys().Single(f => f.GetConstraintName() == name); Assert.Equal(target, fk.PrincipalEntityType.GetTableName()); Assert.Equal(required, fk.IsRequired); Assert.Equal(unique, fk.IsUnique); Assert.Equal(DeleteBehavior.NoAction, fk.DeleteBehavior); Assert.Equal(column, Assert.Single(fk.Properties).GetColumnName()); Assert.Equal(principal, Assert.Single(fk.PrincipalKey.Properties).GetColumnName());
    }
}
