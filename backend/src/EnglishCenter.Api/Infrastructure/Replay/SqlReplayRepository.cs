namespace EnglishCenter.Api.Infrastructure.Replay;

using System.Data;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Domain.Administration;
using EnglishCenter.Api.Infrastructure.Identity;
using EnglishCenter.Api.Services.Replay;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

public sealed class SqlReplayRepository : IReplayRepository
{
    private readonly SqlAuthDeadlineScope _scope;
    private readonly int _gateMs;
    public SqlReplayRepository(SqlAuthDeadlineScope scope,int gateMs)
    {
        ArgumentNullException.ThrowIfNull(scope);
        if(gateMs<=0||gateMs>=(long)scope.SqlSeconds*1000)throw new ArgumentOutOfRangeException(nameof(gateMs));
        _scope=scope;_gateMs=gateMs;
    }
    public async Task AcquireAsync(string resource,CancellationToken ct)
    {
        _scope.RequireActive();
        if(string.IsNullOrEmpty(resource)||resource.Length>255||resource.Any(c=>c>127))
            throw new ArgumentException("Invalid gate resource.");
        var context=_scope.Context;
        await using var cmd=(SqlCommand)context.Database.GetDbConnection().CreateCommand();
        cmd.Transaction=(SqlTransaction)context.Database.CurrentTransaction!.GetDbTransaction();cmd.CommandTimeout=_scope.SqlSeconds;
        cmd.CommandText="DECLARE @result int; EXEC @result=sys.sp_getapplock @Resource=@resource,@LockMode='Exclusive',@LockOwner='Transaction',@DbPrincipal='public',@LockTimeout=@timeout; SELECT @result;";
        cmd.Parameters.Add("@resource",SqlDbType.NVarChar,255).Value=resource;
        cmd.Parameters.Add("@timeout",SqlDbType.Int).Value=_gateMs;
        ReplayService.RequireGateResult(Convert.ToInt32(await cmd.ExecuteScalarAsync(ct),System.Globalization.CultureInfo.InvariantCulture));
    }
    public async Task<ReplayOperation?> FindAsync(int actor,string action,Guid request,CancellationToken ct)
    {
        _scope.RequireActive();
        return await _scope.Context.ReplayOperations.AsNoTracking().SingleOrDefaultAsync(
            r=>r.ActorAccountId==actor&&r.ActionCode==action&&r.RequestId==request,ct);
    }
    public async Task AppendAsync(ReplayOperation completion,CancellationToken ct)
    {
        _scope.RequireActive();
        // Caller already holds its request gate. Append-only grants/unique key are the backstop.
        _scope.Context.ReplayOperations.Add(completion);
        await _scope.Context.SaveChangesAsync(ct);
    }
}
