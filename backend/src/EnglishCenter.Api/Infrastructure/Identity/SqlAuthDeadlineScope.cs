namespace EnglishCenter.Api.Infrastructure.Identity;

using System.Data;
using EnglishCenter.Api.Common.Errors;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

// Connection settings are scoped, including caller-owned fixture connections.
public sealed class SqlAuthDeadlineScope : IAsyncDisposable
{
    public EnglishCenterDbContext Context { get; }
    public int SqlSeconds { get; }
    private readonly int _previousLockMs;
    private readonly int? _previousSqlSeconds;
    private bool _disposed;
    private SqlAuthDeadlineScope(EnglishCenterDbContext context, int seconds, int previousLockMs, int? previousSqlSeconds)
    { Context = context; SqlSeconds = seconds; _previousLockMs = previousLockMs; _previousSqlSeconds = previousSqlSeconds; }
    public static async Task<SqlAuthDeadlineScope> EnterAsync(EnglishCenterDbContext context, int sqlSeconds, int lockMs, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(context);
        if (sqlSeconds <= 0 || lockMs <= 0 || lockMs >= (long)sqlSeconds * 1000) throw new ArgumentException("Invalid auth deadline profile.");
        var tx = context.Database.CurrentTransaction?.GetDbTransaction();
        if (tx is null || tx.Connection is null || tx.IsolationLevel is not (IsolationLevel.ReadCommitted or IsolationLevel.Serializable)) throw new ApiErrorException(ApiErrorCode.STATE_CONFLICT);
        await using var cmd = tx.Connection.CreateCommand(); cmd.Transaction = tx; cmd.CommandTimeout = sqlSeconds;
        cmd.CommandText = "SELECT @@LOCK_TIMEOUT;"; var previous = Convert.ToInt32(await cmd.ExecuteScalarAsync(cancellationToken), System.Globalization.CultureInfo.InvariantCulture);
        cmd.CommandText = "SET LOCK_TIMEOUT " + lockMs.ToString(System.Globalization.CultureInfo.InvariantCulture) + ";";
        await cmd.ExecuteNonQueryAsync(cancellationToken);
        var scope = new SqlAuthDeadlineScope(context, sqlSeconds, previous, context.Database.GetCommandTimeout()); context.Database.SetCommandTimeout(sqlSeconds); return scope;
    }
    public void RequireActive()
    {
        if (_disposed || Context.Database.CurrentTransaction?.GetDbTransaction().Connection is null) throw new ApiErrorException(ApiErrorCode.STATE_CONFLICT);
    }
    public async ValueTask DisposeAsync()
    {
        if (_disposed) return; _disposed = true; Context.Database.SetCommandTimeout(_previousSqlSeconds);
        var connection = Context.Database.GetDbConnection(); if (connection.State != ConnectionState.Open) return;
        await using var cmd = connection.CreateCommand(); cmd.CommandTimeout = SqlSeconds;
        var tx = Context.Database.CurrentTransaction?.GetDbTransaction(); if (tx?.Connection is not null) cmd.Transaction = tx;
        cmd.CommandText = "SET LOCK_TIMEOUT " + _previousLockMs.ToString(System.Globalization.CultureInfo.InvariantCulture) + ";";
        await cmd.ExecuteNonQueryAsync(CancellationToken.None);
    }
}
