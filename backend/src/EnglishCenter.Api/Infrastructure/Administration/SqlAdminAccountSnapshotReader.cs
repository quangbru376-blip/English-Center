namespace EnglishCenter.Api.Infrastructure.Administration;

using System.Data;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Common.Security;
using EnglishCenter.Api.Services.Administration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

// This reader buffers data only; a separate live transaction authorizes its delivery.
public sealed class SqlAdminAccountSnapshotReader(EnglishCenterDbContext context,int sqlSeconds)
{
    public async Task<AccountAdminView?> ReadAsync(int actor,int target,CancellationToken ct)
    {
        if(actor<=0||target<=0||sqlSeconds<=0)throw new ArgumentException("Invalid read profile.");
        var tx=context.Database.CurrentTransaction?.GetDbTransaction();
        if(tx?.Connection is null||tx.IsolationLevel!=IsolationLevel.Snapshot)throw new ApiErrorException(ApiErrorCode.STATE_CONFLICT);
        var previous=context.Database.GetCommandTimeout();context.Database.SetCommandTimeout(sqlSeconds);
        try
        {
            var account=await context.Accounts.AsNoTracking().SingleOrDefaultAsync(a=>a.AccountId==target,ct);
            if(account is null)return null;
            var role=await context.Roles.AsNoTracking().Where(r=>r.RoleId==account.RoleId).Select(r=>r.RoleCode).SingleAsync(ct);
            if(!FixedRoleCodes.All.Contains(role,StringComparer.Ordinal))throw new ApiErrorException(ApiErrorCode.INTERNAL_ERROR);
            var count=await context.Accounts.AsNoTracking().CountAsync(a=>a.Status=="ACTIVE"&&context.Roles.Any(r=>r.RoleId==a.RoleId&&r.RoleCode=="Admin"),ct);
            return AdminAccountProjection.Map(account,role,actor,count);
        }
        finally{context.Database.SetCommandTimeout(previous);}
    }
}
