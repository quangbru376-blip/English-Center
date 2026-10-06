namespace EnglishCenter.Tests.Common.Replay;

using System.Text.Json;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Common.Replay;
using EnglishCenter.Api.Common.Security;
using EnglishCenter.Api.Common.Time;
using EnglishCenter.Api.Domain.Administration;
using EnglishCenter.Api.Services.Administration;
using EnglishCenter.Api.Services.Replay;
using Xunit;

public class ReplayProtocolTests
{
    private static CancellationToken Ct=>TestContext.Current.CancellationToken;
    private sealed class Clock:TimeProvider { public DateTimeOffset Now=new(2026,10,6,0,0,0,TimeSpan.Zero);public override DateTimeOffset GetUtcNow()=>Now; }
    private sealed class Store:IReplayRepository
    {
        internal ReplayOperation? Row;internal List<string> Gates=[];internal int Probes,Appends;
        public Task AcquireAsync(string resource,CancellationToken ct){Gates.Add(resource);return Task.CompletedTask;}
        public Task<ReplayOperation?> FindAsync(int actor,string action,Guid key,CancellationToken ct){Probes++;return Task.FromResult(Row is {} r&&r.ActorAccountId==actor&&r.ActionCode==action&&r.RequestId==key?r:null);}
        public Task AppendAsync(ReplayOperation row,CancellationToken ct){Appends++;Row=row;return Task.CompletedTask;}
    }
    private static CanonicalIntent Intent(string rv="AAAAAAAAAAE=")=>CanonicalIntent.Bind(10,JsonSerializer.Serialize(new{expectedVersions=new{account=rv}}),new Dictionary<string,int>{{"accountId",2}},4096);
    [Fact] public void ExactWindowAndOverflowUseRawTicks()
    {
        var at=new DateTime(2026,10,6).Ticks;var expiry=at+TimeSpan.TicksPerDay;
        Assert.Equal(expiry,ReplayWindow.ExpiresAt(at));Assert.True(ReplayWindow.IsActive(at,expiry,at));
        Assert.True(ReplayWindow.IsActive(at,expiry,expiry-1));Assert.False(ReplayWindow.IsActive(at,expiry,expiry));
        Assert.False(ReplayWindow.IsActive(at,expiry,at-1));Assert.False(ReplayWindow.IsActive(at,expiry+1,at));
        Assert.Null(ReplayWindow.ExpiresAt(-1));Assert.Null(ReplayWindow.ExpiresAt(long.MaxValue));
        Assert.Equal(DateTime.MaxValue.Ticks,ReplayWindow.ExpiresAt(DateTime.MaxValue.Ticks-TimeSpan.TicksPerDay));
        Assert.Null(ReplayWindow.ExpiresAt(DateTime.MaxValue.Ticks-TimeSpan.TicksPerDay+1));
    }
    public static IEnumerable<object[]> Actions()=>CanonicalActionRegistry.All.Select(a=>new object[]{a.ApiId});
    [Theory][MemberData(nameof(Actions))]
    public void All55SafeReferencesModesAreExactImmutableAndRejectAdditionalFields(int api)
    {
        var action=CanonicalActionRegistry.ForApi(api);var op=Guid.NewGuid();
        var refs=action.ResultKeys.ToDictionary(k=>k.TrimEnd('*'),_=>1,StringComparer.Ordinal);
        var c=SafeCommandCompletion.Create(action,op,refs);refs.Clear();
        Assert.Equal(action.ResultKeys.Count,c.Target.Count);
        var decoded=SafeCommandCompletion.Decode(action,op,action.ResultMode,c.EncodeReference());
        Assert.Equal(c.Target,decoded.Target);Assert.Equal(op,decoded.OperationId);
        Assert.Throws<NotSupportedException>(()=>((IDictionary<string,int>)c.Target).Add("secret",1));
        Assert.ThrowsAny<Exception>(()=>SafeCommandCompletion.Decode(action,op,action.ResultMode,"""{"outcome":"COMPLETED","target":{},"password":"x"}"""));
        var bad=c.Target.ToDictionary(p=>p.Key,p=>p.Value);bad["rawBody"]=1;
        Assert.Throws<ArgumentException>(()=>SafeCommandCompletion.Create(action,op,bad));
        Assert.Throws<ArgumentException>(()=>SafeCommandCompletion.Decode(action,op,"OTHER",c.EncodeReference()));
    }
    [Theory][InlineData(-1,503)][InlineData(-2,503)][InlineData(-3,503)][InlineData(-999,500)][InlineData(-4,500)]
    public void NegativeGateReturnAlwaysAborts(int result,int status)=>Assert.Equal(status,Assert.Throws<ApiErrorException>(()=>ReplayService.RequireGateResult(result)).StatusCode);
    [Fact] public async Task MatchedReplayBypassesOldRvAndNeverAppendsAgain()
    {
        var store=new Store();var time=new Clock();var service=new ReplayService(store,new(time));var key=Guid.NewGuid();var intent=Intent();
        var op=Guid.NewGuid();await service.CompleteAsync(1,key,intent,op,new Dictionary<string,int>{{"accountId",2}},Ct);
        Assert.Equal(time.Now.AddHours(24).UtcDateTime,store.Row!.ExpiresAt);
        var result=await service.BeginAsync(1,key,intent,(_,_)=>Task.FromResult(true),Ct);
        Assert.Equal(op,result!.OperationId);Assert.Equal(1,store.Appends);
        Assert.Equal("EC:REQUEST:1:ADMIN.ACCOUNT.LOCK:"+key.ToString("D"),store.Gates[0]);
        Assert.Equal(ApiErrorCode.CONFLICT,(await Assert.ThrowsAsync<ApiErrorException>(()=>service.CompleteAsync(1,key,intent,Guid.NewGuid(),new Dictionary<string,int>{{"accountId",2}},Ct))).Code);
    }
    [Fact] public async Task CurrentScopeFailureHidesDigestMismatchAndExpiryHasNoReuse()
    {
        var store=new Store();var time=new Clock();var service=new ReplayService(store,new(time));var key=Guid.NewGuid();
        await service.CompleteAsync(1,key,Intent(),Guid.NewGuid(),new Dictionary<string,int>{{"accountId",2}},Ct);
        Assert.Equal(ApiErrorCode.NOT_FOUND,(await Assert.ThrowsAsync<ApiErrorException>(()=>service.BeginAsync(1,key,Intent("AAAAAAAAAAI="),(_,_)=>Task.FromResult(false),Ct))).Code);
        Assert.Equal(ApiErrorCode.IDEMPOTENCY_CONTENT_MISMATCH,(await Assert.ThrowsAsync<ApiErrorException>(()=>service.BeginAsync(1,key,Intent("AAAAAAAAAAI="),(_,_)=>Task.FromResult(true),Ct))).Code);
        time.Now=time.Now.AddHours(24);
        Assert.Equal(ApiErrorCode.CONFLICT,(await Assert.ThrowsAsync<ApiErrorException>(()=>service.BeginAsync(1,key,Intent(),(_,_)=>Task.FromResult(true),Ct))).Code);
        Assert.Equal(ApiErrorCode.RESULT_NOT_CONFIRMED,(await Assert.ThrowsAsync<ApiErrorException>(()=>service.LookupAsync(14,"Admin",1,"ADMIN.ACCOUNT.LOCK",key,(_,_)=>Task.FromResult(true),Ct))).Code);
        Assert.Equal(1,store.Appends);
    }
    [Theory][MemberData(nameof(Actions))]
    public async Task LookupAllowlistChecksBeforeMarkerProbeAndScopeFailsClosed(int api)
    {
        var action=CanonicalActionRegistry.ForApi(api);var store=new Store();var time=new Clock();var service=new ReplayService(store,new(time));var key=Guid.NewGuid();
        Assert.Equal(ApiErrorCode.FORBIDDEN,(await Assert.ThrowsAsync<ApiErrorException>(()=>service.LookupAsync(action.LookupApiId,"Director",1,action.Code,key,(_,_)=>Task.FromResult(true),Ct))).Code);
        Assert.Equal(0,store.Probes);
        store.Row=new(){ActorAccountId=1,ActionCode=action.Code,RequestId=key,OperationId=Guid.NewGuid(),InputDigest=new byte[32],
            CompletedAt=time.Now.UtcDateTime,ExpiresAt=time.Now.AddHours(24).UtcDateTime,ResultMode=action.ResultMode,
            ResultReference=SafeCommandCompletion.Create(action,Guid.NewGuid(),action.ResultKeys.ToDictionary(k=>k.TrimEnd('*'),_=>2)).EncodeReference()};
        Assert.Equal(ApiErrorCode.RESULT_NOT_CONFIRMED,(await Assert.ThrowsAsync<ApiErrorException>(()=>service.LookupAsync(action.LookupApiId,action.Role,1,action.Code,key,(_,_)=>Task.FromResult(false),Ct))).Code);
        var c=await service.LookupAsync(action.LookupApiId,action.Role,1,action.Code,key,(_,_)=>Task.FromResult(true),Ct);Assert.Equal("COMPLETED",c.Outcome);
        Assert.Equal(ApiErrorCode.RESULT_NOT_CONFIRMED,(await Assert.ThrowsAsync<ApiErrorException>(()=>service.LookupAsync(action.LookupApiId,action.Role,2,action.Code,key,(_,_)=>Task.FromResult(true),Ct))).Code);
    }
    [Theory][InlineData("future")][InlineData("expiry")][InlineData("digest")][InlineData("secret")][InlineData("precision")]
    public async Task MalformedStoredMarkerIsNeverServedOrRepaired(string fault)
    {
        var store=new Store();var time=new Clock();var service=new ReplayService(store,new(time));var key=Guid.NewGuid();
        await service.CompleteAsync(1,key,Intent(),Guid.NewGuid(),new Dictionary<string,int>{{"accountId",2}},Ct);
        switch(fault){case "future":store.Row!.CompletedAt=time.Now.AddSeconds(1).UtcDateTime;store.Row.ExpiresAt=store.Row.CompletedAt.AddHours(24);break;
            case "expiry":store.Row!.ExpiresAt=store.Row.ExpiresAt.AddHours(1);break;case "digest":store.Row!.InputDigest=[];break;
            case "secret":store.Row!.ResultReference="""{"password":"x"}""";break;case "precision":store.Row!.CompletedAt=store.Row.CompletedAt.AddTicks(1);break;}
        Assert.Equal(ApiErrorCode.INTERNAL_ERROR,(await Assert.ThrowsAsync<ApiErrorException>(()=>service.BeginAsync(1,key,Intent(),(_,_)=>Task.FromResult(true),Ct))).Code);
        Assert.Equal(1,store.Appends);
    }
    [Theory][InlineData(1,2,false,0,"ACTIVE",true)][InlineData(1,2,true,2,"ACTIVE",true)]
    [InlineData(1,1,true,2,"ACTIVE",false)][InlineData(1,2,true,1,"ACTIVE",false)]
    [InlineData(0,2,false,2,"ACTIVE",false)][InlineData(1,2,false,-1,"ACTIVE",false)]
    [InlineData(1,2,false,2,"active",false)][InlineData(1,2,false,2,null,false)]
    public void AdminSelfLastActiveAndExactStatusGuard(int actor,int target,bool admin,int count,string? status,bool expected)=>
        Assert.Equal(expected,AdminLockGuard.CanLock(actor,target,admin,count,status));
    [Theory][InlineData("{}")][InlineData("""{"expectedVersions":null}""")][InlineData("""{"expectedVersions":{"account":null}}""")]
    [InlineData("""{"expectedVersions":{"account":"AAAAAAAAAAE="},"reason":"x"}""")]
    public void AdminLockShapeNeverAcceptsExtraOrMissingFacts(string json)=>Assert.Throws<ApiErrorException>(()=>AdminLockInput.Parse(2,Guid.NewGuid(),json,4096));
}
