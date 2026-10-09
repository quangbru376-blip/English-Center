namespace EnglishCenter.Api.Services.Administration;

using System.Text.Json;
using System.Text.Json.Nodes;
using EnglishCenter.Api.Common.Auditing;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Common.Replay;
using EnglishCenter.Api.Common.Security;
using EnglishCenter.Api.Common.Serialization;
using EnglishCenter.Api.Common.Time;
using EnglishCenter.Api.Domain.Identity;
using EnglishCenter.Api.Services.Auth;
using EnglishCenter.Api.Services.Replay;

public sealed class AdminLockInput
{
    public int TargetId { get; }
    public Guid RequestId { get; }
    public CanonicalIntent Intent { get; }
    private readonly byte[] _version;
    public byte[] CopyVersion()=>_version.ToArray();
    private AdminLockInput(int target,Guid request,CanonicalIntent intent,byte[] version)
    { TargetId=target;RequestId=request;Intent=intent;_version=version.ToArray(); }
    public static AdminLockInput Parse(int target,Guid request,string json,int maxBytes)
    {
        if(request==Guid.Empty)throw ApiErrorException.Validation();
        var intent=CanonicalIntent.Bind(10,json,new Dictionary<string,int>{{"accountId",target}},maxBytes);
        using var doc=JsonDocument.Parse(json);
        var v=StrictJsonObject.Parse(doc.RootElement.GetProperty("expectedVersions").GetRawText(),["account"],["account"]);
        return new(target,request,intent,v.ReadParsed("account",RowVersionCodec.Decode));
    }
    public override string ToString()=>nameof(AdminLockInput);
}
public interface IAdminAccountRepository : IPasswordChangeRepository
{
    Task PrepareAsync(int actorId,IReadOnlyCollection<int> targetIds,CancellationToken ct);
    Task<int> CountActiveAdminsAsync(CancellationToken ct);
}
public interface IAdminAccountExecutor
{
    Task<AccountAdminView> DetailAsync(VerifiedSessionToken token,int target,CancellationToken ct);
    Task<AdminAccountCommandResult> LockAsync(VerifiedSessionToken token,AdminLockInput input,CancellationToken ct);
    Task<SafeCommandCompletion> LookupAsync(VerifiedSessionToken token,string action,Guid request,CancellationToken ct);
}
public sealed partial class AdminAccountService(IAdminAccountRepository repository,IReplayRepository markers,BusinessClock clock)
{
    private async Task<CurrentSessionSnapshot> RequireAdmin(VerifiedSessionToken token,CancellationToken ct)
    {
        var current=await new CurrentSessionService(repository,clock).ReadAsync(token,true,ct);
        if(current.Context.Role!="Admin")throw new ApiErrorException(ApiErrorCode.FORBIDDEN);
        return current;
    }
    private async Task<AccountAdminView?> View(int actor,int target,CancellationToken ct)
    {
        var account=await repository.LockAccountAsync(target,ct);
        if(account is null)return null;
        var role=await repository.RoleCodeAsync(account.RoleId,ct);
        if(role is null||!FixedRoleCodes.All.Contains(role,StringComparer.Ordinal))throw new ApiErrorException(ApiErrorCode.INTERNAL_ERROR);
        return AdminAccountProjection.Map(account,role,actor,await repository.CountActiveAdminsAsync(ct));
    }
    public async Task<CurrentSessionSnapshot> AuthorizeAsync(VerifiedSessionToken token,CancellationToken ct)
    {
        await markers.AcquireAsync("EC:ADMINSET",ct);
        await repository.PrepareAsync(token.AccountId,[token.AccountId],ct);
        return await RequireAdmin(token,ct);
    }
    public async Task<AccountAdminView> DetailAsync(VerifiedSessionToken token,int target,CancellationToken ct)
    {
        await markers.AcquireAsync("EC:ADMINSET",ct);
        await repository.PrepareAsync(token.AccountId,[target],ct);
        await RequireAdmin(token,ct);
        var view=await View(token.AccountId,target,ct)??throw new ApiErrorException(ApiErrorCode.NOT_FOUND);
        await RequireAdmin(token,ct);
        return view;
    }
    public async Task<AdminAccountCommandResult> LockAsync(VerifiedSessionToken token,AdminLockInput input,CancellationToken ct)
    {
        await markers.AcquireAsync(ReplayService.RequestResource(token.AccountId,input.Intent.Action,input.RequestId),ct);
        await markers.AcquireAsync("EC:ADMINSET",ct);
        var discovered=await markers.FindAsync(token.AccountId,input.Intent.Action.Code,input.RequestId,ct);
        var storedTarget=DiscoverTarget(discovered,input.Intent.Action)??input.TargetId;
        await repository.PrepareAsync(token.AccountId,[input.TargetId,storedTarget],ct);
        await RequireAdmin(token,ct);
        var replay=new ReplayService(markers,clock); AccountAdminView? replayView=null;
        var completion=await replay.BeginAsync(token.AccountId,input.RequestId,input.Intent,async (stored,cancel)=>{
            await markers.AcquireAsync("EC:ADMINSET",cancel);
            await RequireAdmin(token,cancel);
            replayView=await View(token.AccountId,stored.Target["accountId"],cancel);
            return replayView is not null;
        },ct);
        if(completion is not null)
        {
            if(replayView!.Id!=input.TargetId)throw new ApiErrorException(ApiErrorCode.INTERNAL_ERROR);
            await RequireAdmin(token,ct);return new(replayView,completion.OperationId);
        }
        await markers.AcquireAsync("EC:ADMINSET",ct);
        await repository.PrepareAsync(token.AccountId,[input.TargetId,storedTarget],ct);
        var current=await RequireAdmin(token,ct);
        var target=await repository.LockAccountAsync(input.TargetId,ct)??throw new ApiErrorException(ApiErrorCode.NOT_FOUND);
        var role=await repository.RoleCodeAsync(target.RoleId,ct);
        if(role is null||!FixedRoleCodes.All.Contains(role,StringComparer.Ordinal))throw new ApiErrorException(ApiErrorCode.INTERNAL_ERROR);
        if(!PasswordChangeVersionGuard.Matches(input.CopyVersion(),target.RowVersion))throw new ApiErrorException(ApiErrorCode.CONFLICT);
        if(!AdminLockGuard.CanLock(token.AccountId,target.AccountId,role=="Admin",await repository.CountActiveAdminsAsync(ct),target.Status))
            throw new ApiErrorException(ApiErrorCode.STATE_CONFLICT);
        var version=PasswordChangeVersionGuard.Next(target.SecurityVersion)??throw new ApiErrorException(ApiErrorCode.STATE_CONFLICT);
        var sessions=await repository.LockAllSessionsAsync(target.AccountId,ct);
        if(sessions.Any(s=>s.AccountId!=target.AccountId||s.IssuedAt>clock.GetUtcNow().UtcDateTime))throw new ApiErrorException(ApiErrorCode.STATE_CONFLICT);
        if(!clock.IsBefore(current.EffectiveDeadline))throw new ApiErrorException(ApiErrorCode.AUTHENTICATION_REQUIRED);
        var now=clock.GetUtcNow();var operation=Guid.NewGuid();var count=sessions.Count(s=>s.RevokedAt is null);
        var audit=AuditEntry.Mandatory("ADMIN.ACCOUNT.LOCK",token.AccountId,"Account",target.AccountId,operation,now,
            new AuditChanges(new JsonObject{["status"]=target.Status,["securityVersion"]=target.SecurityVersion},
                new JsonObject{["status"]="LOCKED",["securityVersion"]=version,["revokedSessionCount"]=count}));
        target.Status="LOCKED";target.SecurityVersion=version;
        foreach(var s in sessions)if(s.RevokedAt is null)s.RevokedAt=now.UtcDateTime;
        await repository.SaveChangeAsync(audit,ct);
        await replay.CompleteAsync(token.AccountId,input.RequestId,input.Intent,operation,new Dictionary<string,int>{{"accountId",target.AccountId}},ct);
        await new CurrentSessionService(repository,clock).TouchApprovedInteractiveActivityAsync(token,ct);
        await RequireAdmin(token,ct);
        return new((await View(token.AccountId,target.AccountId,ct))!,operation);
    }
    private static int? DiscoverTarget(EnglishCenter.Api.Domain.Administration.ReplayOperation? row,CanonicalAction action)
    {
        if(row is null)return null;
        try{return SafeCommandCompletion.Decode(action,row.OperationId,row.ResultMode,row.ResultReference).Target["accountId"];}
        catch(Exception ex) when(ex is ArgumentException or ApiErrorException or JsonException or InvalidOperationException)
        { return null; } // Malformed storage is diagnosed after current authority is locked/rechecked.
    }
    public async Task<SafeCommandCompletion> LookupAsync(VerifiedSessionToken token,string action,Guid request,CancellationToken ct)
    {
        // Preflight proof is held by the adapter; live role is checked again under sorted locks.
        var replay=new ReplayService(markers,clock);
        CanonicalAction selected;try{selected=CanonicalActionRegistry.ForCode(action);}catch(ArgumentException){throw ApiErrorException.Validation("action");}
        if(!CanonicalActionRegistry.AllowsLookup(14,"Admin",selected.Code))throw new ApiErrorException(ApiErrorCode.FORBIDDEN);
        await markers.AcquireAsync(ReplayService.RequestResource(token.AccountId,selected,request),ct);
        // This pre-lock read only discovers IDs. It grants no authority and is reloaded below.
        var discovered=await markers.FindAsync(token.AccountId,action,request,ct);
        var targetId=DiscoverTarget(discovered,selected)??token.AccountId;
        await markers.AcquireAsync("EC:ADMINSET",ct);
        await repository.PrepareAsync(token.AccountId,[targetId],ct);
        await RequireAdmin(token,ct);
        var result=await replay.LookupAsync(14,"Admin",token.AccountId,action,request,async (stored,cancel)=>{
            var target=stored.Target["accountId"];
            await markers.AcquireAsync("EC:ADMINSET",cancel);
            await RequireAdmin(token,cancel);
            return await View(token.AccountId,target,cancel) is not null;
        },ct);
        await RequireAdmin(token,ct);return result;
    }
}
