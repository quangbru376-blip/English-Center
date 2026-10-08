using System.Data;
using EnglishCenter.Api.Infrastructure.Readiness;
using Microsoft.Data.SqlClient;

namespace EnglishCenter.Api.Infrastructure.Maintenance;

public sealed class SqlMaintenanceService : BackgroundService
{
    private readonly Func<CancellationToken, ValueTask<string?>> resolve;
    private readonly TimeProvider clock;
    private long failures;
    public MaintenanceProfile Configuration { get; }
    public ExecutionProfile Execution { get; }
    public int AuditIntervalSeconds { get; }
    public long FailedRuns => Interlocked.Read(ref failures);
    public bool HasFiniteSchedules => Configuration.ReplayCleanupIntervalSeconds > 0 &&
        Configuration.ReplayCleanupIntervalSeconds * 1000L <= int.MaxValue &&
        AuditIntervalSeconds > 0 && AuditIntervalSeconds * 1000L <= int.MaxValue;
    public SqlMaintenanceService(MaintenanceProfile configuration, ExecutionProfile execution, int auditIntervalSeconds,
        TimeProvider clock, Func<CancellationToken, ValueTask<string?>> resolve)
    {
        Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        Execution = execution ?? throw new ArgumentNullException(nameof(execution)); AuditIntervalSeconds = auditIntervalSeconds;
        this.clock = clock ?? throw new ArgumentNullException(nameof(clock)); this.resolve = resolve ?? throw new ArgumentNullException(nameof(resolve));
    }
    public override string ToString() => nameof(SqlMaintenanceService);
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!HasFiniteSchedules) throw new InvalidOperationException("Invalid maintenance schedules");
        return Task.WhenAll(LoopAsync(Configuration.ReplayCleanupIntervalSeconds, false, stoppingToken),
            LoopAsync(AuditIntervalSeconds, true, stoppingToken));
    }
    private async Task LoopAsync(int seconds, bool audit, CancellationToken token)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(seconds), clock);
        while (await timer.WaitForNextTickAsync(token))
        {
            try { await RunOnceAsync(audit, token); }
            catch (OperationCanceledException) when (token.IsCancellationRequested) { throw; }
            catch (Exception) { Interlocked.Increment(ref failures); }
        }
    }
    internal async Task<SqlConnection?> OpenAsync(CancellationToken token)
    {
        var pending = resolve(token).AsTask(); ProbeWait.Observe(pending);
        var resolved = await pending.WaitAsync(token);
        if (string.IsNullOrWhiteSpace(resolved)) return null;
        var prepared = LocalSqlReadinessConnection.Prepare(resolved, Execution, requireCertificateValidation: true);
        if (prepared is null) return null;
        var connection = new SqlConnection(prepared.ConnectionString);
        try { await connection.OpenAsync(token); return connection; }
        catch { await connection.DisposeAsync(); throw; }
    }
    internal async Task<bool> HasSafePrincipalAsync(SqlConnection connection, CancellationToken token)
    {
        await using var command = connection.CreateCommand(); command.CommandTimeout = Execution.SqlSeconds;
        command.CommandText = PrincipalSql;
        await using var reader = await command.ExecuteReaderAsync(token);
        if (reader.FieldCount != 9 || !await reader.ReadAsync(token)) return false;
        var facts = new bool?[9];
        for (var index = 0; index < facts.Length; index++)
            facts[index] = reader.IsDBNull(index) ? null : reader.GetInt32(index) switch { 0 => false, 1 => true, _ => null };
        if (await reader.ReadAsync(token) || await reader.NextResultAsync(token)) return false;
        return MaintenancePrincipalPolicy.IsSafe(facts[0], facts[1], facts[2], facts[3], facts[4], facts[5], facts[6], facts[7], facts[8]);
    }
    public async Task RunReplayOnceAsync(CancellationToken token) => await RunOnceAsync(false, token);
    public async Task RunAuditOnceAsync(CancellationToken token) => await RunOnceAsync(true, token);
    private async Task RunOnceAsync(bool audit, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        if (!HasFiniteSchedules || !DeadlineBudgets.IsValid(Execution.LockMs, Execution.SqlSeconds,
            Execution.RequestMs, Execution.CsvMs, Execution.ReadinessMs)) throw new InvalidOperationException("Invalid maintenance profile");
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(token); deadline.CancelAfter(Execution.RequestMs);
        await using var connection = await OpenAsync(deadline.Token) ?? throw new InvalidOperationException("Maintenance unavailable");
        if (!await HasSafePrincipalAsync(connection, deadline.Token)) throw new InvalidOperationException("Maintenance authority unavailable");
        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync(IsolationLevel.ReadCommitted, deadline.Token);
        await using var command = connection.CreateCommand(); command.Transaction = transaction; command.CommandTimeout = Execution.SqlSeconds;
        command.CommandText = audit
            ? "UPDATE dbo.AuditLog SET ip_address=NULL,ip_expires_at=NULL WHERE ip_expires_at<=@now; DELETE FROM dbo.AuditLog WHERE audit_category='SECURITY' AND security_expires_at<=@now;"
            : "DELETE FROM dbo.ReplayOperation WHERE expires_at<=@now;";
        var now = clock.GetUtcNow(); var ticks = now.UtcTicks - now.UtcTicks % TimeSpan.TicksPerMillisecond;
        command.Parameters.Add("@now", SqlDbType.DateTime2).Value = new DateTime(ticks, DateTimeKind.Utc);
        await command.ExecuteNonQueryAsync(deadline.Token); await transaction.CommitAsync(deadline.Token);
        // Disposal rolls back any uncommitted transaction; no automatic retry after unknown COMMIT.
    }
    private const string PrincipalSql = """
        SELECT CASE WHEN USER_NAME()='dbo' THEN 1 ELSE 0 END,
          IS_SRVROLEMEMBER('sysadmin'), HAS_PERMS_BY_NAME(DB_NAME(),'DATABASE','CONTROL'),
          CASE WHEN HAS_PERMS_BY_NAME(NULL,NULL,'CONTROL SERVER') IS NULL THEN NULL
               WHEN HAS_PERMS_BY_NAME(NULL,NULL,'CONTROL SERVER')=1 OR
                 EXISTS(SELECT 1 FROM sys.login_token WHERE type='SERVER ROLE' AND name<>'public') OR EXISTS(SELECT 1 FROM sys.server_permissions p JOIN sys.login_token t ON p.grantee_principal_id=t.principal_id WHERE t.name<>'public' AND p.state IN('G','W') AND p.permission_name<>'CONNECT SQL') THEN 1 ELSE 0 END,
          IS_ROLEMEMBER('ec_app_runtime'),
          CASE WHEN IS_ROLEMEMBER('db_ddladmin') IS NULL THEN NULL
            WHEN IS_ROLEMEMBER('db_ddladmin')=1 OR
              EXISTS(SELECT 1 FROM sys.user_token WHERE type='DATABASE ROLE' AND name<>'public') OR
              EXISTS(SELECT 1 FROM sys.database_permissions p JOIN sys.user_token t ON p.grantee_principal_id=t.principal_id
                WHERE t.name<>'public' AND p.state IN('G','W') AND NOT
                  ((p.class=0 AND p.permission_name='CONNECT') OR
                   (p.class=1 AND p.major_id=OBJECT_ID('dbo.ReplayOperation') AND ((p.minor_id=0 AND p.permission_name='DELETE') OR (p.permission_name='SELECT' AND p.minor_id=COLUMNPROPERTY(OBJECT_ID('dbo.ReplayOperation'),'expires_at','ColumnId')))) OR
                   (p.class=1 AND p.major_id=OBJECT_ID('dbo.AuditLog') AND
                      ((p.minor_id=0 AND p.permission_name='DELETE') OR
                       (p.permission_name='SELECT' AND p.minor_id IN (COLUMNPROPERTY(OBJECT_ID('dbo.AuditLog'),'audit_category','ColumnId'), COLUMNPROPERTY(OBJECT_ID('dbo.AuditLog'),'security_expires_at','ColumnId'), COLUMNPROPERTY(OBJECT_ID('dbo.AuditLog'),'ip_expires_at','ColumnId'))) OR (p.permission_name='UPDATE' AND p.minor_id IN
                         (COLUMNPROPERTY(OBJECT_ID('dbo.AuditLog'),'ip_address','ColumnId'),
                          COLUMNPROPERTY(OBJECT_ID('dbo.AuditLog'),'ip_expires_at','ColumnId')))))))
            THEN 1 ELSE 0 END,
          CASE WHEN HAS_PERMS_BY_NAME('dbo.AuditLog','OBJECT','DELETE')=1 AND HAS_PERMS_BY_NAME('dbo.AuditLog','OBJECT','SELECT','audit_category','COLUMN')=1 AND HAS_PERMS_BY_NAME('dbo.AuditLog','OBJECT','SELECT','security_expires_at','COLUMN')=1 THEN 1 ELSE 0 END,
          CASE WHEN HAS_PERMS_BY_NAME('dbo.ReplayOperation','OBJECT','DELETE')=1 AND HAS_PERMS_BY_NAME('dbo.ReplayOperation','OBJECT','SELECT','expires_at','COLUMN')=1 THEN 1 ELSE 0 END,
          CASE WHEN HAS_PERMS_BY_NAME('dbo.AuditLog','OBJECT','UPDATE','ip_address','COLUMN') IS NULL OR
                    HAS_PERMS_BY_NAME('dbo.AuditLog','OBJECT','UPDATE','ip_expires_at','COLUMN') IS NULL THEN NULL
               WHEN HAS_PERMS_BY_NAME('dbo.AuditLog','OBJECT','UPDATE','ip_address','COLUMN')=1 AND
                    HAS_PERMS_BY_NAME('dbo.AuditLog','OBJECT','UPDATE','ip_expires_at','COLUMN')=1 AND HAS_PERMS_BY_NAME('dbo.AuditLog','OBJECT','SELECT','ip_expires_at','COLUMN')=1 THEN 1 ELSE 0 END;
        """;
}
