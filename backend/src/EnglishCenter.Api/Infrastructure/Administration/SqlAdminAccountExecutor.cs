namespace EnglishCenter.Api.Infrastructure.Administration;

using System.Data;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Common.Replay;
using EnglishCenter.Api.Common.Security;
using EnglishCenter.Api.Common.Time;
using EnglishCenter.Api.Infrastructure.Identity;
using EnglishCenter.Api.Infrastructure.Replay;
using EnglishCenter.Api.Services.Administration;
using EnglishCenter.Api.Services.Auth;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

public sealed class SqlAdminAccountExecutor : IAdminAccountExecutor
{
    private readonly Func<EnglishCenterDbContext> _factory;private readonly BusinessClock _clock;
    private readonly int _seconds,_lockMs;
    public SqlAdminAccountExecutor(Func<EnglishCenterDbContext> factory,BusinessClock clock,int sqlSeconds,int lockMs)
    {
        ArgumentNullException.ThrowIfNull(factory);ArgumentNullException.ThrowIfNull(clock);
        if(sqlSeconds<=0||lockMs<=0||lockMs>=(long)sqlSeconds*1000)throw new ArgumentException("Invalid transaction deadline profile.");
        _factory=factory;_clock=clock;_seconds=sqlSeconds;_lockMs=lockMs;
    }
    public async Task<AccountAdminView> DetailAsync(VerifiedSessionToken token,int target,CancellationToken ct)
    {
        if(target<=0)throw ApiErrorException.Validation("accountId");
        await Execute(token,(s,c)=>s.AuthorizeAsync(token,c),ct);
        AccountAdminView? buffered;
        await using(var context=_factory())
        {
            if(context.Database.CurrentTransaction is not null)throw new ApiErrorException(ApiErrorCode.STATE_CONFLICT);
            await context.Database.OpenConnectionAsync(ct);
            await using var data=await context.Database.BeginTransactionAsync(IsolationLevel.Snapshot,ct);
            buffered=await new SqlAdminAccountSnapshotReader(context,_seconds).ReadAsync(token.AccountId,target,ct);
            // No connection or snapshot survives into the live authorization transaction.
            await data.CommitAsync(ct);
        }
        await Execute(token,async(s,c)=>{
            await s.DetailAsync(token,target,c);
            if(buffered is null)throw new ApiErrorException(ApiErrorCode.NOT_FOUND);
            return buffered;
        },ct);
        return buffered!;
    }
    public Task<AdminAccountCommandResult> LockAsync(VerifiedSessionToken token,AdminLockInput input,CancellationToken ct)=>
        Execute(token,(s,c)=>s.LockAsync(token,input,c),ct);
    public Task<SafeCommandCompletion> LookupAsync(VerifiedSessionToken token,string action,Guid request,CancellationToken ct)=>
        Execute(token,(s,c)=>s.LookupAsync(token,action,request,c),ct);
    private async Task<T> Execute<T>(VerifiedSessionToken token,Func<AdminAccountService,CancellationToken,Task<T>> operation,CancellationToken ct)
    {
        await using var context=_factory();
        if(context.Database.CurrentTransaction is not null)throw new ApiErrorException(ApiErrorCode.STATE_CONFLICT);
        await context.Database.OpenConnectionAsync(ct);
        await using var tx=await context.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted,ct);
        await using var scope=await SqlAuthDeadlineScope.EnterAsync(context,_seconds,_lockMs,ct);
        var repo=new SqlAdminAccountRepository(scope);
        try
        {
            var result=await operation(new(repo,new SqlReplayRepository(scope,_lockMs),_clock),ct);
            await new CurrentSessionService(repo,_clock).ReadAsync(token,true,ct);
            await tx.CommitAsync(ct);
            return result;
        }
        catch(DbUpdateConcurrencyException){throw new ApiErrorException(ApiErrorCode.CONFLICT);}
        catch(SqlException ex)when(ex.Number is 1205 or 1222){throw new ApiErrorException(ApiErrorCode.SERVICE_UNAVAILABLE);}
        catch(DbUpdateException ex)when(ex.InnerException is SqlException sql && sql.Number is 1205 or 1222)
        {throw new ApiErrorException(ApiErrorCode.SERVICE_UNAVAILABLE);}
    }
}
