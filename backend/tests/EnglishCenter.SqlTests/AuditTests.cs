namespace EnglishCenter.SqlTests;

using System.Text.Json.Nodes;
using EnglishCenter.Api.Common.Auditing;
using EnglishCenter.Api.Infrastructure.Auditing;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Xunit;

public class AuditTests
{
    private static SqlConnection Connection(SqlFixture f) => (SqlConnection)f.Context.Database.GetDbConnection();
    private static SqlTransaction Transaction(SqlFixture f) => (SqlTransaction)f.Context.Database.CurrentTransaction!.GetDbTransaction();
    private static AuditEntry LockEntry(SqlFixture f,int? actor=null) => AuditEntry.Mandatory("ADMIN.ACCOUNT.LOCK",actor ?? f.Teacher.AccountId,"Account",f.Teacher.AccountId,Guid.NewGuid(),new DateTimeOffset(SqlFixture.Instant),new(New:new JsonObject { ["status"]="LOCKED" }));

    [Fact]
    public async Task MandatoryAppendAndGroupedEffectShareExistingTransactionAndNoCommit()
    {
        await using var f=await SqlFixture.CreateAsync();var op=Guid.NewGuid();
        await f.ExecuteAsync($"UPDATE dbo.TrainingProgram SET expected_lesson_count=3 WHERE program_id={f.UnusedProgram.ProgramId};");
        var component=new EnglishCenter.Api.Domain.Academics.ScoreComponentConfig { ProgramId=f.UnusedProgram.ProgramId,ComponentName="New configuration",Kind="GENERAL",Weight=100 };
        f.Context.Add(component);await f.Context.SaveChangesAsync(TestContext.Current.CancellationToken);
        var entry=AuditEntry.Mandatory("ACADEMIC.PROGRAM.UPDATE_CONFIGURATION",f.Teacher.AccountId,"TrainingProgram",f.UnusedProgram.ProgramId,op,new DateTimeOffset(SqlFixture.Instant),new(Old:new JsonObject { ["expectedLessonCount"]=2 },New:new JsonObject { ["expectedLessonCount"]=3 }));
        var writer=new SqlAuditAppender();await writer.AppendMandatoryAsync(Connection(f),Transaction(f),entry,10,TestContext.Current.CancellationToken);
        var effect=AuditEntry.Mandatory("ACADEMIC.PROGRAM.UPDATE_CONFIGURATION.EFFECT",f.Teacher.AccountId,"ScoreComponentConfig",component.ComponentId,op,new DateTimeOffset(SqlFixture.Instant),new(New:new JsonObject { ["componentName"]="New configuration",["kind"]="GENERAL",["weight"]=100m },Parent:new JsonObject { ["programId"]=f.UnusedProgram.ProgramId }));
        await writer.AppendMandatoryAsync(Connection(f),Transaction(f),effect,10,TestContext.Current.CancellationToken);
        var rows=await SqlRows.ReadAsync(f,"SELECT COUNT(*) AS count,COUNT(DISTINCT operation_id) AS operations FROM dbo.AuditLog; SELECT @@TRANCOUNT AS count;");
        Assert.Equal(2,rows[0].Value(0,"count"));Assert.Equal(1,rows[0].Value(0,"operations"));Assert.Equal(1,rows[1].Value(0,"count"));
        await f.Context.Database.CurrentTransaction!.RollbackAsync(TestContext.Current.CancellationToken);
        Assert.Equal(0,await f.Context.AuditLogs.CountAsync(TestContext.Current.CancellationToken));
        Assert.Equal(0,await f.Context.Accounts.CountAsync(TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public async Task AuditFkFailureOrCancellationPropagatesAndCallerRollsBackAllChanges(bool canceled)
    {
        await using var f=await SqlFixture.CreateAsync();
        await f.ExecuteAsync($"UPDATE dbo.Account SET status='LOCKED',security_version=security_version+1 WHERE account_id={f.Teacher.AccountId}; INSERT dbo.AuthSession(sid,account_id,issued_at,expires_at,last_activity_at,revoked_at,security_version,session_scope) VALUES(CONVERT(binary(32),1),{f.Teacher.AccountId},{SqlFixture.Instant},{SqlFixture.Instant.AddHours(1)},{SqlFixture.Instant},{SqlFixture.Instant},2,'NORMAL'); INSERT dbo.InvoiceNumberCounter(issue_year,last_value) VALUES(2026,1); INSERT dbo.ReplayOperation(actor_account_id,action_code,request_id,input_digest,operation_id,result_mode,result_reference,completed_at,expires_at) VALUES({f.Teacher.AccountId},'ADMIN.ACCOUNT.LOCK',NEWID(),CONVERT(binary(32),1),NEWID(),'SAFE_REFERENCE','{{}}',{SqlFixture.Instant},{SqlFixture.Instant.AddDays(1)});");
        var writer=new SqlAuditAppender();
        if (canceled)
            await Assert.ThrowsAnyAsync<OperationCanceledException>(()=>writer.AppendMandatoryAsync(Connection(f),Transaction(f),LockEntry(f),10,new CancellationToken(true)));
        else
        {
            var exception=await Assert.ThrowsAsync<MandatoryAuditException>(()=>writer.AppendMandatoryAsync(Connection(f),Transaction(f),LockEntry(f,int.MaxValue),10,TestContext.Current.CancellationToken));
            Assert.Null(exception.InnerException);Assert.Equal("Mandatory audit persistence failed.",exception.Message);
        }
        await f.Context.Database.CurrentTransaction!.RollbackAsync(TestContext.Current.CancellationToken);f.Context.ChangeTracker.Clear();
        Assert.Equal(0,await f.Context.Accounts.CountAsync(TestContext.Current.CancellationToken));
        Assert.Equal(0,await f.Context.AuthSessions.CountAsync(TestContext.Current.CancellationToken));
        Assert.Equal(0,await f.Context.InvoiceNumberCounters.CountAsync(TestContext.Current.CancellationToken));
        Assert.Equal(0,await f.Context.AuditLogs.CountAsync(TestContext.Current.CancellationToken));
        Assert.Equal(0,await f.Context.ReplayOperations.CountAsync(TestContext.Current.CancellationToken));
        Assert.Equal(0,await f.Context.Registrations.CountAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task TemporaryDeniedAuditInsertAbortsMandatoryMutationWithoutLeakingSql()
    {
        await using var f=await SqlFixture.CreateAsync();var entry=LockEntry(f);
        await SqlRows.ReadAsync(f,"IF DATABASE_PRINCIPAL_ID('D12_fixture_user_0') IS NOT NULL THROW 51912,'Fixture exists',1; CREATE USER D12_fixture_user_0 WITHOUT LOGIN; ALTER ROLE ec_app_runtime ADD MEMBER D12_fixture_user_0; DENY INSERT ON dbo.AuditLog TO ec_app_runtime; EXECUTE AS USER='D12_fixture_user_0';");
        try
        {
            await f.ExecuteAsync($"UPDATE dbo.Account SET status='LOCKED' WHERE account_id={f.Teacher.AccountId};");
            await Assert.ThrowsAsync<MandatoryAuditException>(()=>new SqlAuditAppender().AppendMandatoryAsync(Connection(f),Transaction(f),entry,10,TestContext.Current.CancellationToken));
        }
        finally { await SqlRows.ReadAsync(f,"REVERT;"); }
        await f.Context.Database.CurrentTransaction!.RollbackAsync(TestContext.Current.CancellationToken);
        Assert.Equal(0,await f.Context.Accounts.CountAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task SecurityNullActorAndIpExpiryPersistUsingIndependentAuditTransaction()
    {
        // Only the audit transaction exists here; no business transaction is supplied to the recorder.
        await using var f=await SqlFixture.CreateAsync();var store=new RollbackAuditStore(f);var incidents=new AuditIncidentCounter();
        Assert.True(await new SecurityAuditRecorder(store,incidents).TryRecordAsync(SecurityAuditKind.Login,null,false,Guid.NewGuid(),new DateTimeOffset(SqlFixture.Instant),"192.0.2.1",TestContext.Current.CancellationToken));
        var row=await f.Context.AuditLogs.AsNoTracking().SingleAsync(TestContext.Current.CancellationToken);
        Assert.Null(row.ActorAccountId);Assert.Null(row.Details);Assert.Equal("SECURITY",row.AuditCategory);
        Assert.Equal(row.OccurredAt.AddDays(90),row.SecurityExpiresAt);Assert.Equal(row.SecurityExpiresAt,row.IpExpiresAt);
        Assert.Equal(0,incidents.GetFailureCount(SecurityAuditKind.Login));
    }

    [Fact]
    public async Task DeniedIndependentSecurityAuditCannotUndoRevokedSession()
    {
        await using var f=await SqlFixture.CreateAsync();
        await f.ExecuteAsync($"INSERT dbo.AuthSession(sid,account_id,issued_at,expires_at,last_activity_at,revoked_at,security_version,session_scope) VALUES(CONVERT(binary(32),1),{f.Teacher.AccountId},{SqlFixture.Instant},{SqlFixture.Instant.AddHours(1)},{SqlFixture.Instant},{SqlFixture.Instant},1,'NORMAL');");
        // Supply an independent audit connection/transaction; business revocation is on f.
        await using var audit=new SqlConnection(Connection(f).ConnectionString);
        await audit.OpenAsync(TestContext.Current.CancellationToken);
        await using var auditTx=(SqlTransaction)await audit.BeginTransactionAsync(TestContext.Current.CancellationToken);
        await using (var setup=audit.CreateCommand())
        {
            setup.Transaction=auditTx;setup.CommandTimeout=10;
            setup.CommandText="IF DATABASE_PRINCIPAL_ID('D12_fixture_user_0') IS NOT NULL THROW 51912,'Fixture exists',1; CREATE USER D12_fixture_user_0 WITHOUT LOGIN; ALTER ROLE ec_app_runtime ADD MEMBER D12_fixture_user_0; DENY INSERT ON dbo.AuditLog TO ec_app_runtime; EXECUTE AS USER='D12_fixture_user_0';";
            await setup.ExecuteNonQueryAsync(TestContext.Current.CancellationToken);
        }
        var incidents=new AuditIncidentCounter();
        try
        {
            Assert.False(await new SecurityAuditRecorder(new SeparateRollbackStore(audit,auditTx),incidents).TryRecordAsync(SecurityAuditKind.Logout,null,true,Guid.NewGuid(),new DateTimeOffset(SqlFixture.Instant),null,TestContext.Current.CancellationToken));
        }
        finally
        {
            await using var revert=audit.CreateCommand();revert.Transaction=auditTx;revert.CommandText="REVERT;";revert.CommandTimeout=10;
            await revert.ExecuteNonQueryAsync(TestContext.Current.CancellationToken);
            await auditTx.RollbackAsync(TestContext.Current.CancellationToken);
        }
        Assert.Equal(1,incidents.GetFailureCount(SecurityAuditKind.Logout));
        Assert.NotNull((await f.Context.AuthSessions.AsNoTracking().SingleAsync(TestContext.Current.CancellationToken)).RevokedAt);
        Assert.Equal(0,await f.Context.AuditLogs.CountAsync(TestContext.Current.CancellationToken));
    }

    private sealed class SeparateRollbackStore(SqlConnection connection,SqlTransaction transaction) : ISecurityAuditStore
    {
        public Task AppendAsync(AuditEntry entry,CancellationToken cancellationToken) => SqlAuditAppender.AppendAsync(connection,transaction,entry,10,cancellationToken);
    }

    private sealed class RollbackAuditStore(SqlFixture fixture) : ISecurityAuditStore
    {
        public async Task AppendAsync(AuditEntry entry,CancellationToken cancellationToken)
        {
            // Same insert contract as independent SQL security store, but rollback-only test connection.
            await SqlAuditAppender.AppendAsync(Connection(fixture),Transaction(fixture),entry,10,cancellationToken);
        }
    }
}
