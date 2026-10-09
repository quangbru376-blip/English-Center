namespace EnglishCenter.Api.Infrastructure.Administration;

using System.Data;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Common.Security;
using EnglishCenter.Api.Common.Time;
using EnglishCenter.Api.Infrastructure.Identity;
using EnglishCenter.Api.Infrastructure.Replay;
using EnglishCenter.Api.Services.Administration;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

public sealed class SqlStaffAccountExecutor(Func<EnglishCenterDbContext> factory, BusinessClock clock,
    CredentialHasher hasher,TemporaryPasswordGenerator generator,int sqlSeconds,int lockMs) : IStaffAccountExecutor
{
    public Task<object> ExecuteAsync(VerifiedSessionToken token,StaffAccountInput input,CancellationToken ct)=>
        Execute(async(service,cancel)=>await service.ExecuteStaffAsync(token,input,hasher,generator,cancel),ct);
    public async Task<AccountPage> SearchAsync(VerifiedSessionToken token,AccountSearch search,CancellationToken ct)
    {
        await Execute((s,c)=>s.AuthorizeAsync(token,c),ct);
        AccountPage page;
        await using(var db=factory())
        {
            db.Database.SetCommandTimeout(sqlSeconds);await db.Database.OpenConnectionAsync(ct);
            await using var tx=await db.Database.BeginTransactionAsync(IsolationLevel.Snapshot,ct);
            var rows=from a in db.Accounts.AsNoTracking() join r in db.Roles.AsNoTracking() on a.RoleId equals r.RoleId select new{Account=a,Role=r.RoleCode};
            if(search.Name is not null)rows=rows.Where(x=>x.Account.FullName.Contains(search.Name));
            if(search.Email is not null)rows=rows.Where(x=>x.Account.Email.Contains(search.Email));
            if(search.Phone is not null)rows=rows.Where(x=>x.Account.Phone==search.Phone);
            if(search.Role is not null)rows=rows.Where(x=>x.Role==search.Role);
            if(search.Status is not null)rows=rows.Where(x=>x.Account.Status==search.Status);
            var count=await rows.CountAsync(ct);
            var offset=(long)(search.Page-1)*search.PageSize;
            var data=offset>int.MaxValue?[]:await rows.OrderBy(x=>x.Account.AccountId).Skip((int)offset).Take(search.PageSize).ToListAsync(ct);
            var admins=await db.Accounts.CountAsync(a=>a.Status=="ACTIVE"&&db.Roles.Any(r=>r.RoleId==a.RoleId&&r.RoleCode=="Admin"),ct);
            page=new(data.Select(x=>AdminAccountProjection.Map(x.Account,x.Role,token.AccountId,admins)).ToArray(),search.Page,search.PageSize,count);
            await tx.CommitAsync(ct);
        }
        await Execute((s,c)=>s.AuthorizeAsync(token,c),ct);return page;
    }
    private async Task<T> Execute<T>(Func<AdminAccountService,CancellationToken,Task<T>> work,CancellationToken ct)
    {
        await using var db=factory();if(db.Database.CurrentTransaction is not null)throw new ApiErrorException(ApiErrorCode.STATE_CONFLICT);
        await db.Database.OpenConnectionAsync(ct);await using var tx=await db.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted,ct);
        await using var scope=await SqlAuthDeadlineScope.EnterAsync(db,sqlSeconds,lockMs,ct);
        try{var result=await work(new(new SqlAdminAccountRepository(scope),new SqlReplayRepository(scope,lockMs),clock),ct);await tx.CommitAsync(ct);return result;}
        catch(DbUpdateConcurrencyException){throw new ApiErrorException(ApiErrorCode.CONFLICT);}
        catch(DbUpdateException e)when(e.InnerException is SqlException s&&s.Number is 2601 or 2627){throw new ApiErrorException(ApiErrorCode.CONFLICT);}
        catch(SqlException e)when(e.Number is 1205 or 1222){throw new ApiErrorException(ApiErrorCode.SERVICE_UNAVAILABLE);}
        catch(DbUpdateException e)when(e.InnerException is SqlException s&&s.Number is 1205 or 1222){throw new ApiErrorException(ApiErrorCode.SERVICE_UNAVAILABLE);}
    }
}
