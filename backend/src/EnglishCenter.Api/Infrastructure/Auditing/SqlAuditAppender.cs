namespace EnglishCenter.Api.Infrastructure.Auditing;

using System.Data;
using EnglishCenter.Api.Common.Auditing;
using Microsoft.Data.SqlClient;

public sealed class MandatoryAuditException : Exception
{
    public MandatoryAuditException() : base("Mandatory audit persistence failed.") { }
}

public sealed class SqlAuditAppender
{
    public async Task AppendMandatoryAsync(SqlConnection connection,SqlTransaction transaction,AuditEntry entry,
        int timeoutSeconds,CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(connection);ArgumentNullException.ThrowIfNull(transaction);ArgumentNullException.ThrowIfNull(entry);
        if (entry.Category != "MANDATORY" || transaction.Connection != connection || connection.State != ConnectionState.Open)
            throw new ArgumentException("Audit requires the caller's active transaction.");
        try { await AppendAsync(connection,transaction,entry,timeoutSeconds,cancellationToken); }
        catch (SqlException) { throw new MandatoryAuditException(); }
        catch (InvalidOperationException) { throw new MandatoryAuditException(); }
    }

    internal static async Task AppendAsync(SqlConnection connection,SqlTransaction? transaction,AuditEntry entry,
        int timeoutSeconds,CancellationToken cancellationToken)
    {
        if (timeoutSeconds <= 0) throw new ArgumentOutOfRangeException(nameof(timeoutSeconds));
        await using var command = connection.CreateCommand();
        command.Transaction=transaction;command.CommandTimeout=timeoutSeconds;
        command.CommandText = "INSERT dbo.AuditLog(actor_account_id,action,entity_type,entity_id,result,details,occurred_at,ip_address,operation_id,audit_category,security_expires_at,ip_expires_at) VALUES(@actor,@action,@entity,@id,@result,@details,@at,@ip,@operation,@category,@securityExpiry,@ipExpiry);";
        Add(command,"@actor",SqlDbType.Int,entry.ActorAccountId);
        Add(command,"@action",SqlDbType.VarChar,entry.Action,80);Add(command,"@entity",SqlDbType.VarChar,entry.EntityType,64);
        Add(command,"@id",SqlDbType.VarChar,entry.EntityId,100);Add(command,"@result",SqlDbType.VarChar,entry.Result,16);
        Add(command,"@details",SqlDbType.NVarChar,entry.Details,-1);Add(command,"@at",SqlDbType.DateTime2,entry.OccurredAt);
        Add(command,"@ip",SqlDbType.VarChar,entry.IpAddress,45);Add(command,"@operation",SqlDbType.UniqueIdentifier,entry.OperationId);
        Add(command,"@category",SqlDbType.VarChar,entry.Category,16);Add(command,"@securityExpiry",SqlDbType.DateTime2,entry.SecurityExpiresAt);
        Add(command,"@ipExpiry",SqlDbType.DateTime2,entry.IpExpiresAt);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static void Add(SqlCommand command,string name,SqlDbType type,object? value,int size=0)
    {
        var parameter=new SqlParameter(name,type) { Value=value ?? DBNull.Value };
        if (size!=0) parameter.Size=size;
        if (type==SqlDbType.DateTime2) parameter.Scale=3;
        command.Parameters.Add(parameter);
    }
}
