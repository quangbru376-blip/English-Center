using System.Data;
using System.Reflection;
using EnglishCenter.Api.Domain.Administration;
using EnglishCenter.Api.Infrastructure.Maintenance;
using EnglishCenter.Tests.Api.Integration;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Xunit;

namespace EnglishCenter.Tests.Readiness;

[Collection("V01 HTTP SQL")]
public sealed class MaintenanceSqlTests
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;
    private static string Sql(string name) => (string)(typeof(SqlMaintenanceService)
        .GetField(name, BindingFlags.NonPublic | BindingFlags.Static)?.GetRawConstantValue()
        ?? throw new InvalidOperationException("Production maintenance SQL unavailable"));
    private const string Grants = """
        IF USER_ID('V01T_maintenance_fixture_0') IS NOT NULL THROW 51000,'Fixture already exists',1;
        CREATE USER V01T_maintenance_fixture_0 WITHOUT LOGIN;
        GRANT DELETE ON dbo.AuditLog TO V01T_maintenance_fixture_0;
        GRANT DELETE ON dbo.ReplayOperation TO V01T_maintenance_fixture_0;
        GRANT SELECT ON dbo.AuditLog(audit_category,security_expires_at,ip_expires_at) TO V01T_maintenance_fixture_0;
        GRANT SELECT ON dbo.ReplayOperation(expires_at) TO V01T_maintenance_fixture_0;
        GRANT UPDATE ON dbo.AuditLog(ip_address,ip_expires_at) TO V01T_maintenance_fixture_0;
        """;

    [Theory(Skip = "V01-T synthetic scratch SQL requires owned gates",
        SkipUnless = nameof(OwnedSqlGate.Enabled), SkipType = typeof(OwnedSqlGate))]
    [InlineData(0)] [InlineData(1)] [InlineData(2)] [InlineData(3)]
    public async Task ActualPrincipalQueryExcludesDelegationBusinessRightsAndRuntimeRole(int variant)
    {
        await using var fixture = await OwnedSqlFixture.CreateAsync();
        await using var db = fixture.Db();
        await using var transaction = await db.Database.BeginTransactionAsync(Ct);
        var connection = (SqlConnection)db.Database.GetDbConnection();
        var tx = (SqlTransaction)transaction.GetDbTransaction();
        try
        {
            await Execute(connection, tx, Grants);
            var extra = variant switch
            {
                1 => "GRANT DELETE ON dbo.AuditLog TO V01T_maintenance_fixture_0 WITH GRANT OPTION;",
                2 => "GRANT SELECT ON dbo.Account TO V01T_maintenance_fixture_0;",
                3 => "ALTER ROLE ec_app_runtime ADD MEMBER V01T_maintenance_fixture_0;",
                _ => ""
            };
            if (extra.Length > 0) await Execute(connection, tx, extra);
            await Execute(connection, tx, "EXECUTE AS USER='V01T_maintenance_fixture_0';");
            try
            {
                await using var command = connection.CreateCommand();
                command.Transaction = tx; command.CommandTimeout = 10; command.CommandText = Sql("PrincipalSql");
                await using var reader = await command.ExecuteReaderAsync(Ct);
                Assert.True(await reader.ReadAsync(Ct)); Assert.Equal(9, reader.FieldCount);
                Assert.Equal(0, reader.GetInt32(0));
                Assert.Equal(variant == 0 ? 0 : 1, reader.GetInt32(5));
                Assert.Equal(variant == 3 ? 1 : 0, reader.GetInt32(4));
                for (var i = 6; i < 9; i++) Assert.Equal(1, reader.GetInt32(i));
                Assert.False(await reader.ReadAsync(Ct)); Assert.False(await reader.NextResultAsync(Ct));
                // WITHOUT LOGIN cannot establish server authority or runtime authentication.
            }
            finally { await Execute(connection, tx, "REVERT;"); }
        }
        finally { await transaction.RollbackAsync(CancellationToken.None); }
        await AssertNoRetainedFixture(fixture);
    }

    [Fact(Skip = "V01-T synthetic scratch SQL requires owned gates",
        SkipUnless = nameof(OwnedSqlGate.Enabled), SkipType = typeof(OwnedSqlGate))]
    public async Task ExactCleanupPreservesMandatoryFutureAndUnrelatedRowsWithRollbackRestoration()
    {
        await using var fixture = await OwnedSqlFixture.CreateAsync();
        await using var db = fixture.Db();
        await using var transaction = await db.Database.BeginTransactionAsync(Ct);
        var connection = (SqlConnection)db.Database.GetDbConnection();
        var tx = (SqlTransaction)transaction.GetDbTransaction();
        var now = new DateTime(2026, 10, 8, 0, 0, 0, DateTimeKind.Utc);
        var auditIds = new Dictionary<(string Category, int Offset), int>();
        var replayIds = new Dictionary<int, int>();
        try
        {
            foreach (var category in new[] { "MANDATORY", "SECURITY" })
            foreach (var offset in new[] { -1, 0, 1 })
            {
                var expiry = now.AddMilliseconds(offset);
                var row = new AuditLog { Action = "V01T_SYNTHETIC", EntityType = "Account",
                    Result = "SUCCESS", OperationId = Guid.NewGuid(), AuditCategory = category,
                    OccurredAt = expiry.AddDays(-90), IpAddress = "192.0.2.1", IpExpiresAt = expiry,
                    SecurityExpiresAt = category == "SECURITY" ? expiry : null };
                db.AuditLogs.Add(row); await db.SaveChangesAsync(Ct);
                auditIds.Add((category, offset), row.AuditId);
            }
            foreach (var offset in new[] { -1, 0, 1 })
            {
                var expiry = now.AddMilliseconds(offset);
                var row = new ReplayOperation { ActorAccountId = fixture.Accounts["Student"],
                    ActionCode = "V01T_SYNTHETIC", RequestId = Guid.NewGuid(), InputDigest = new byte[32],
                    OperationId = Guid.NewGuid(), ResultMode = "SAFE_REFERENCE", ResultReference = "{}",
                    CompletedAt = expiry.AddHours(-24), ExpiresAt = expiry };
                db.ReplayOperations.Add(row); await db.SaveChangesAsync(Ct); replayIds.Add(offset, row.ReplayId);
            }
            await Execute(connection, tx, Grants);
            await Execute(connection, tx, "SAVE TRANSACTION before_cleanup;");
            await Execute(connection, tx, "EXECUTE AS USER='V01T_maintenance_fixture_0';");
            try
            {
                await Execute(connection, tx, Sql("AuditCleanupSql"), now);
                await Execute(connection, tx, Sql("AuditCleanupSql"), now);
            }
            finally { await Execute(connection, tx, "REVERT;"); }
            var audits = await db.AuditLogs.AsNoTracking().ToDictionaryAsync(a => a.AuditId, Ct);
            Assert.Equal(4, audits.Count);
            foreach (var offset in new[] { -1, 0, 1 })
            {
                var mandatory = audits[auditIds[("MANDATORY", offset)]];
                Assert.Equal(offset > 0 ? "192.0.2.1" : null, mandatory.IpAddress);
                Assert.Equal(offset > 0 ? now.AddMilliseconds(offset) : (DateTime?)null, mandatory.IpExpiresAt);
                Assert.Null(mandatory.SecurityExpiresAt); Assert.Equal("V01T_SYNTHETIC", mandatory.Action);
                Assert.Equal(offset > 0, audits.ContainsKey(auditIds[("SECURITY", offset)]));
            }
            Assert.Equal("192.0.2.1", audits[auditIds[("SECURITY", 1)]].IpAddress);
            Assert.Equal(3, await db.ReplayOperations.CountAsync(Ct));
            await Execute(connection, tx, "EXECUTE AS USER='V01T_maintenance_fixture_0';");
            try
            {
                await Execute(connection, tx, Sql("ReplayCleanupSql"), now);
                await Execute(connection, tx, Sql("ReplayCleanupSql"), now);
            }
            finally { await Execute(connection, tx, "REVERT;"); }
            Assert.Equal(replayIds[1], (await db.ReplayOperations.AsNoTracking().SingleAsync(Ct)).ReplayId);
            Assert.Equal(4, await db.AuditLogs.CountAsync(Ct));
            Assert.Equal(8, await db.Accounts.CountAsync(Ct));
            await Execute(connection, tx, "ROLLBACK TRANSACTION before_cleanup;");
            Assert.Equal(6, await db.AuditLogs.CountAsync(Ct));
            Assert.Equal(6, await db.AuditLogs.CountAsync(a => a.IpAddress != null && a.IpExpiresAt != null, Ct));
            Assert.Equal(3, await db.ReplayOperations.CountAsync(Ct));
        }
        finally { await transaction.RollbackAsync(CancellationToken.None); }
        await AssertNoRetainedFixture(fixture);
    }

    private static async Task Execute(SqlConnection connection, SqlTransaction tx, string sql, DateTime? now = null)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = tx; command.CommandTimeout = 10; command.CommandText = sql;
        if (now.HasValue) command.Parameters.Add("@now", SqlDbType.DateTime2).Value = now.Value;
        await command.ExecuteNonQueryAsync(Ct);
    }
    private static async Task AssertNoRetainedFixture(OwnedSqlFixture fixture)
    {
        await using var db = fixture.Db();
        Assert.Equal(0, await db.AuditLogs.CountAsync(Ct));
        Assert.Equal(0, await db.ReplayOperations.CountAsync(Ct));
        var connection = (SqlConnection)db.Database.GetDbConnection(); await connection.OpenAsync(Ct);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT USER_ID('V01T_maintenance_fixture_0');"; command.CommandTimeout = 10;
        Assert.Equal(DBNull.Value, await command.ExecuteScalarAsync(Ct));
    }
}

