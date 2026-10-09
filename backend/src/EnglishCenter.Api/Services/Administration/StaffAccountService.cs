namespace EnglishCenter.Api.Services.Administration;

using System.Globalization;
using System.Text.Json.Nodes;
using EnglishCenter.Api.Common.Auditing;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Common.Security;
using EnglishCenter.Api.Common.Serialization;
using EnglishCenter.Api.Domain.Identity;
using EnglishCenter.Api.Services.Auth;
using EnglishCenter.Api.Services.Replay;

public interface IStaffAccountRepository : IAdminAccountRepository
{
    Task<int> StaffRoleAsync(string role,CancellationToken ct);
    Task<bool> ContactExistsAsync(string email,string? phone,CancellationToken ct);
    Task InsertAsync(Account account,CancellationToken ct);
}
public interface IStaffAccountExecutor
{
    Task<AccountPage> SearchAsync(VerifiedSessionToken token,AccountSearch search,CancellationToken ct);
    Task<object> ExecuteAsync(VerifiedSessionToken token,StaffAccountInput input,CancellationToken ct);
}
public sealed partial class AdminAccountService
{
    public async Task<object> ExecuteStaffAsync(VerifiedSessionToken token,StaffAccountInput input,
        CredentialHasher hasher,TemporaryPasswordGenerator generator,CancellationToken ct)
    {
        var repo=repository as IStaffAccountRepository??throw new ApiErrorException(ApiErrorCode.INTERNAL_ERROR);
        var action=input.Intent.Action;
        await markers.AcquireAsync(ReplayService.RequestResource(token.AccountId,action,input.RequestId),ct);
        await markers.AcquireAsync("EC:ADMINSET",ct);
        var stored=await markers.FindAsync(token.AccountId,action.Code,input.RequestId,ct);
        var discovered=DiscoverTarget(stored,action)??(input.TargetId>0?input.TargetId:token.AccountId);
        await repository.PrepareAsync(token.AccountId,[discovered,..input.TargetId>0?new[]{input.TargetId}:Array.Empty<int>()],ct);
        var current=await RequireAdmin(token,ct);
        var replay=new ReplayService(markers,clock);AccountAdminView? view=null;
        var done=await replay.BeginAsync(token.AccountId,input.RequestId,input.Intent,async(result,cancel)=>{
            await RequireAdmin(token,cancel);view=await View(token.AccountId,result.Target["accountId"],cancel);return view is not null;
        },ct);
        if(done is not null)return action.ApiId is 8 or 9?done:new AdminAccountCommandResult(view!,done.OperationId);
        var now=clock.GetUtcNow();var operation=Guid.NewGuid();string? secret=null;
        Account account;string role;JsonObject? old=null;
        if(action.ApiId==8)
        {
            role=input.Body.GetProperty("role").GetString()!;
            var email=input.Body.GetProperty("email").GetString()!;var phone=input.Body.GetProperty("phone").GetString();
            if(await repo.ContactExistsAsync(email,phone,ct))throw new ApiErrorException(ApiErrorCode.CONFLICT);
            secret=generator.Generate();account=new(){RoleId=await repo.StaffRoleAsync(role,ct),
                FullName=input.Body.GetProperty("fullName").GetString()!,Email=email,Phone=phone,Status="ACTIVE",CreatedAt=now.UtcDateTime,
                SecurityVersion=1,MustChangePassword=true,TempIssuedAt=now.UtcDateTime,TempExpiresAt=now.AddHours(24).UtcDateTime,PasswordHash=hasher.Hash(secret)};
            await repo.InsertAsync(account,ct);
        }
        else
        {
            account=await repository.LockAccountAsync(input.TargetId,ct)??throw new ApiErrorException(ApiErrorCode.NOT_FOUND);
            role=await repository.RoleCodeAsync(account.RoleId,ct)??throw new ApiErrorException(ApiErrorCode.INTERNAL_ERROR);
            var version=RowVersionCodec.Decode(input.Body.GetProperty("expectedVersions").GetProperty("account").GetString());
            if(!PasswordChangeVersionGuard.Matches(version,account.RowVersion))throw new ApiErrorException(ApiErrorCode.CONFLICT);
            var projected=AdminAccountProjection.Map(account,role,token.AccountId,await repository.CountActiveAdminsAsync(ct));
            var capability=action.ApiId switch{9=>"RESET_PASSWORD",11=>"UNLOCK",12=>"DISABLE",13=>"RESTORE",_=>throw new ApiErrorException(ApiErrorCode.INTERNAL_ERROR)};
            if(!projected.AllowedActions.Contains(capability))throw new ApiErrorException(ApiErrorCode.STATE_CONFLICT);
            old=State(account);
            if(action.ApiId is 9 or 12)
            {
                account.SecurityVersion=PasswordChangeVersionGuard.Next(account.SecurityVersion)??throw new ApiErrorException(ApiErrorCode.STATE_CONFLICT);
                var sessions=await repository.LockAllSessionsAsync(account.AccountId,ct);
                if(sessions.Any(s=>s.AccountId!=account.AccountId||s.IssuedAt>now.UtcDateTime))throw new ApiErrorException(ApiErrorCode.STATE_CONFLICT);
                var revoked=sessions.Count(session=>session.RevokedAt is null);
                foreach(var session in sessions)if(session.RevokedAt is null)session.RevokedAt=now.UtcDateTime;
                if(action.ApiId==9){secret=generator.Generate();account.PasswordHash=hasher.Hash(secret);account.MustChangePassword=true;account.TempIssuedAt=now.UtcDateTime;account.TempExpiresAt=now.AddHours(24).UtcDateTime;}
                else account.Status="DISABLED";
                var changed=State(account);changed["revokedSessionCount"]=revoked;
                await repository.SaveChangeAsync(AuditEntry.Mandatory(action.Code,token.AccountId,"Account",account.AccountId,operation,now,new(old,changed)),ct);
            }
            else
            {
                account.Status="ACTIVE";
                await repository.SaveChangeAsync(AuditEntry.Mandatory(action.Code,token.AccountId,"Account",account.AccountId,operation,now,new(old,State(account))),ct);
            }
        }
        if(action.ApiId==8)await repository.SaveChangeAsync(AuditEntry.Mandatory(action.Code,token.AccountId,"Account",account.AccountId,operation,now,
            new(New:new JsonObject{["roleId"]=account.RoleId,["status"]=account.Status,["mustChangePassword"]=true,["tempIssuedAt"]=Instant(account.TempIssuedAt!.Value),["tempExpiresAt"]=Instant(account.TempExpiresAt!.Value)})),ct);
        await replay.CompleteAsync(token.AccountId,input.RequestId,input.Intent,operation,new Dictionary<string,int>{{"accountId",account.AccountId}},ct);
        // Self-reset legitimately revokes the caller. Its already verified deadline
        // still bounds commit, but it cannot be authorized using its revoked SID.
        if(account.AccountId!=token.AccountId){await new CurrentSessionService(repository,clock).TouchApprovedInteractiveActivityAsync(token,ct);await RequireAdmin(token,ct);}
        if(!clock.IsBefore(current.EffectiveDeadline))throw new ApiErrorException(ApiErrorCode.AUTHENTICATION_REQUIRED);
        if(secret is not null)return new CredentialIssueResult{AccountId=account.AccountId,Email=account.Email,Role=role,TemporaryPassword=secret,
            TemporaryPasswordExpiresAt=Instant(account.TempExpiresAt!.Value),OperationId=operation,AccountVersion=RowVersionCodec.Encode(account.RowVersion)};
        return new AdminAccountCommandResult((await View(token.AccountId,account.AccountId,ct))!,operation);
    }
    private static string Instant(DateTime value)=>value.ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'",CultureInfo.InvariantCulture);
    private static JsonObject State(Account a)=>new(){["status"]=a.Status,["mustChangePassword"]=a.MustChangePassword,
        ["securityVersion"]=a.SecurityVersion,["tempIssuedAt"]=a.TempIssuedAt is {} issued?Instant(issued):null,["tempExpiresAt"]=a.TempExpiresAt is {} expires?Instant(expires):null};
}
