namespace EnglishCenter.Api.Services.Replay;

using System.Globalization;
using System.Security.Cryptography;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Common.Replay;
using EnglishCenter.Api.Common.Time;
using EnglishCenter.Api.Domain.Administration;

public interface IReplayRepository
{
    Task AcquireAsync(string resource,CancellationToken ct);
    Task<ReplayOperation?> FindAsync(int actor,string action,Guid request,CancellationToken ct);
    Task AppendAsync(ReplayOperation completion,CancellationToken ct);
}
public sealed class ReplayService(IReplayRepository repository,BusinessClock clock)
{
    public static string RequestResource(int actor,CanonicalAction action,Guid request)
    {
        if(actor<=0||request==Guid.Empty)throw new ArgumentException("Invalid replay partition.");
        return "EC:REQUEST:"+actor.ToString(CultureInfo.InvariantCulture)+":"+action.Code+":"+request.ToString("D");
    }
    public static void RequireGateResult(int result)
    {
        if(result>=0)return;
        throw new ApiErrorException(result is -1 or -2 or -3?ApiErrorCode.SERVICE_UNAVAILABLE:ApiErrorCode.INTERNAL_ERROR);
    }
    private SafeCommandCompletion Read(ReplayOperation row,CanonicalAction action,int actor,Guid request)
    {
        try
        {
            if(row.ActorAccountId!=actor||row.ActionCode!=action.Code||row.RequestId!=request||row.InputDigest.Length!=32||
               row.CompletedAt.Ticks%TimeSpan.TicksPerMillisecond!=0||row.ExpiresAt.Ticks%TimeSpan.TicksPerMillisecond!=0||
               ReplayWindow.ExpiresAt(row.CompletedAt.Ticks)!=row.ExpiresAt.Ticks||clock.IsBefore(Utc(row.CompletedAt)))
                throw new ArgumentException("Invalid completion.");
            return SafeCommandCompletion.Decode(action,row.OperationId,row.ResultMode,row.ResultReference);
        }
        catch(Exception ex) when(ex is ArgumentException or ApiErrorException or System.Text.Json.JsonException or InvalidOperationException)
        { throw new ApiErrorException(ApiErrorCode.INTERNAL_ERROR); }
    }
    private static DateTimeOffset Utc(DateTime value)=>new(DateTime.SpecifyKind(value,DateTimeKind.Utc));
    public async Task<SafeCommandCompletion?> BeginAsync(int actor,Guid request,CanonicalIntent intent,
        Func<SafeCommandCompletion,CancellationToken,Task<bool>> currentReadScope,CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(currentReadScope);
        await repository.AcquireAsync(RequestResource(actor,intent.Action,request),ct);
        var row=await repository.FindAsync(actor,intent.Action.Code,request,ct);
        if(row is null)return null;
        var completion=Read(row,intent.Action,actor,request);
        if(!clock.IsBefore(Utc(row.ExpiresAt)))throw new ApiErrorException(ApiErrorCode.CONFLICT);
        if(!await currentReadScope(completion,ct))throw new ApiErrorException(ApiErrorCode.NOT_FOUND);
        if(!CryptographicOperations.FixedTimeEquals(row.InputDigest,intent.CopyDigest()))
            throw new ApiErrorException(ApiErrorCode.IDEMPOTENCY_CONTENT_MISMATCH);
        if(!clock.IsBefore(Utc(row.ExpiresAt)))throw new ApiErrorException(ApiErrorCode.CONFLICT);
        return completion;
    }
    public async Task<SafeCommandCompletion> CompleteAsync(int actor,Guid request,CanonicalIntent intent,Guid operation,
        IReadOnlyDictionary<string,int> serverReferences,CancellationToken ct)
    {
        await repository.AcquireAsync(RequestResource(actor,intent.Action,request),ct);
        if(await repository.FindAsync(actor,intent.Action.Code,request,ct) is not null)throw new ApiErrorException(ApiErrorCode.CONFLICT);
        var completion=SafeCommandCompletion.Create(intent.Action,operation,serverReferences);
        var now=clock.GetUtcNow().UtcDateTime;
        var expiry=ReplayWindow.ExpiresAt(now.Ticks)??throw new ApiErrorException(ApiErrorCode.INTERNAL_ERROR);
        await repository.AppendAsync(new ReplayOperation { ActorAccountId=actor,ActionCode=intent.Action.Code,RequestId=request,
            InputDigest=intent.CopyDigest(),OperationId=operation,ResultMode=completion.ResultMode,ResultReference=completion.EncodeReference(),
            CompletedAt=now,ExpiresAt=new DateTime(expiry,DateTimeKind.Utc) },ct);
        return completion;
    }
    public async Task<SafeCommandCompletion> LookupAsync(int lookupApi,string role,int actor,string actionCode,Guid request,
        Func<SafeCommandCompletion,CancellationToken,Task<bool>> currentReadScope,CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(currentReadScope);
        CanonicalAction action;
        try { action=CanonicalActionRegistry.ForCode(actionCode); } catch(ArgumentException) { throw ApiErrorException.Validation("action"); }
        if(!CanonicalActionRegistry.AllowsLookup(lookupApi,role,action.Code))throw new ApiErrorException(ApiErrorCode.FORBIDDEN);
        await repository.AcquireAsync(RequestResource(actor,action,request),ct);
        var row=await repository.FindAsync(actor,action.Code,request,ct);
        if(row is null)throw new ApiErrorException(ApiErrorCode.RESULT_NOT_CONFIRMED);
        var completion=Read(row,action,actor,request);
        if(!clock.IsBefore(Utc(row.ExpiresAt))||!await currentReadScope(completion,ct)||!clock.IsBefore(Utc(row.ExpiresAt)))
            throw new ApiErrorException(ApiErrorCode.RESULT_NOT_CONFIRMED);
        return completion;
    }
}
