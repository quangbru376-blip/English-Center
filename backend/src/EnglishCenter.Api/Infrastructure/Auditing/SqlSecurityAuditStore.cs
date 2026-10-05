namespace EnglishCenter.Api.Infrastructure.Auditing;

using EnglishCenter.Api.Common.Auditing;
using Microsoft.Data.SqlClient;

// The composition root supplies a fresh independent connection. No runtime profile is configured here.
public sealed class SqlSecurityAuditStore(Func<SqlConnection> createConnection,int timeoutSeconds) : ISecurityAuditStore
{
    private readonly Func<SqlConnection> _createConnection=createConnection ?? throw new ArgumentNullException(nameof(createConnection));
    private readonly int _timeoutSeconds=timeoutSeconds > 0 ? timeoutSeconds : throw new ArgumentOutOfRangeException(nameof(timeoutSeconds));
    public async Task AppendAsync(AuditEntry entry,CancellationToken cancellationToken)
    {
        if (entry.Category != "SECURITY") throw new ArgumentException("Security audit entry required.");
        await using var connection = _createConnection();
        await connection.OpenAsync(cancellationToken);
        await SqlAuditAppender.AppendAsync(connection,null,entry,_timeoutSeconds,cancellationToken);
    }
}
