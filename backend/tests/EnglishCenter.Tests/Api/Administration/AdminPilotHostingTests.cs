namespace EnglishCenter.Tests.Api.Administration;

using System.Net;
using System.Security.Cryptography;
using System.Text.Json;
using EnglishCenter.Api.Api.Administration;
using EnglishCenter.Api.Api.Auth;
using EnglishCenter.Api.Api.Identity;
using EnglishCenter.Api.Common.Auditing;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Common.Replay;
using EnglishCenter.Api.Common.Security;
using EnglishCenter.Api.Common.Serialization;
using EnglishCenter.Api.Common.Time;
using EnglishCenter.Api.Domain.Administration;
using EnglishCenter.Api.Infrastructure.Auditing;
using EnglishCenter.Api.Domain.Identity;
using EnglishCenter.Api.Services.Administration;
using EnglishCenter.Api.Services.Auth;
using EnglishCenter.Api.Services.Replay;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Xunit;

public class AdminPilotHostingTests
{
    private static CancellationToken Ct=>TestContext.Current.CancellationToken;
    private const string Origin="https://localhost";
    private sealed class Clock:TimeProvider { internal DateTimeOffset Now=DateTimeOffset.FromUnixTimeSeconds(DateTimeOffset.UtcNow.ToUnixTimeSeconds());public override DateTimeOffset GetUtcNow()=>Now; }
    private sealed class Data:IAdminAccountRepository,IReplayRepository
    {
        internal List<Account> Accounts=[];internal List<AuthSession> Sessions=[];internal List<ReplayOperation> Markers=[];
        internal int Audits;internal bool AuditFails,StorageFails;
        internal string ActorRole="Admin";
        internal Data Copy()=>new(){Accounts=JsonSerializer.Deserialize<List<Account>>(JsonSerializer.Serialize(Accounts))!,
            Sessions=JsonSerializer.Deserialize<List<AuthSession>>(JsonSerializer.Serialize(Sessions))!,
            Markers=JsonSerializer.Deserialize<List<ReplayOperation>>(JsonSerializer.Serialize(Markers))!,Audits=Audits,
            AuditFails=AuditFails,StorageFails=StorageFails,ActorRole=ActorRole};
        public Task PrepareAsync(int actor,IReadOnlyCollection<int> targets,CancellationToken ct)=>Task.CompletedTask;
        public Task<int> CountActiveAdminsAsync(CancellationToken ct)=>Task.FromResult(Accounts.Count(a=>a.RoleId==1&&a.Status=="ACTIVE"));
        public Task<Account?> LockAccountAsync(int id,CancellationToken ct)=>Task.FromResult(Accounts.SingleOrDefault(a=>a.AccountId==id));
        public Task<AuthSession?> LockSessionAsync(byte[] sid,CancellationToken ct)=>Task.FromResult(Sessions.SingleOrDefault(s=>s.Sid.SequenceEqual(sid)));
        public Task<string?> RoleCodeAsync(int id,CancellationToken ct)=>Task.FromResult<string?>(id==1?ActorRole:"Teacher");
        public Task SaveSessionAsync(CancellationToken ct)=>Task.CompletedTask;
        public Task<IReadOnlyList<AuthSession>> LockAllSessionsAsync(int id,CancellationToken ct)=>Task.FromResult<IReadOnlyList<AuthSession>>(Sessions.Where(s=>s.AccountId==id).ToArray());
        public Task SaveChangeAsync(AuditEntry audit,CancellationToken ct)
        {
            if(AuditFails)throw new MandatoryAuditException();
            Audits++;Accounts.Single(a=>a.AccountId==2).RowVersion=[0,0,0,0,0,0,0,2];return Task.CompletedTask;
        }
        public Task AcquireAsync(string resource,CancellationToken ct)=>Task.CompletedTask;
        public Task<ReplayOperation?> FindAsync(int actor,string action,Guid request,CancellationToken ct)=>
            Task.FromResult(Markers.SingleOrDefault(m=>m.ActorAccountId==actor&&m.ActionCode==action&&m.RequestId==request));
        public Task AppendAsync(ReplayOperation marker,CancellationToken ct)
        { if(StorageFails)throw new ApiErrorException(ApiErrorCode.INTERNAL_ERROR);Markers.Add(marker);return Task.CompletedTask; }
    }
    private sealed class State:ICurrentSessionReader,IAdminAccountExecutor,ISecurityAuditStore
    {
        internal readonly Clock Time=new();internal BusinessClock Clock=>new(Time);
        internal Data Data=new();internal readonly JwtSessionValidator Validator;internal readonly string Token;
        internal bool FailCommit,PauseCommit;internal TaskCompletionSource Paused=new(TaskCreationOptions.RunContinuationsAsynchronously),Resume=new(TaskCreationOptions.RunContinuationsAsynchronously);
        internal int Calls,Commits;
        internal State(string role="Admin",bool restricted=false)
        {
            Data.ActorRole=role;
            Data.Accounts.Add(new(){AccountId=1,RoleId=1,FullName="Synthetic actor",Email="actor@example.test",Status="ACTIVE",SecurityVersion=1,
                RowVersion=[0,0,0,0,0,0,0,1],CreatedAt=Time.Now.UtcDateTime,MustChangePassword=restricted,
                TempIssuedAt=restricted?Time.Now.UtcDateTime:null,TempExpiresAt=restricted?Time.Now.AddHours(24).UtcDateTime:null});
            Data.Accounts.Add(new(){AccountId=2,RoleId=2,FullName="Synthetic target",Email="target@example.test",Status="ACTIVE",SecurityVersion=1,RowVersion=[0,0,0,0,0,0,0,1],CreatedAt=Time.Now.UtcDateTime});
            var sid=RandomNumberGenerator.GetBytes(32);var window=restricted?SessionIssuanceWindow.Restricted(Time.Now,Time.Now.AddHours(24)):SessionIssuanceWindow.Normal(Time.Now);
            Data.Sessions.Add(new(){Sid=sid,AccountId=1,SecurityVersion=1,SessionScope=window.Scope,IssuedAt=Time.Now.UtcDateTime,ExpiresAt=window.AbsoluteExpiresAt.UtcDateTime,LastActivityAt=Time.Now.UtcDateTime});
            Data.Sessions.Add(new(){Sid=RandomNumberGenerator.GetBytes(32),AccountId=2,SecurityVersion=1,SessionScope="NORMAL",IssuedAt=Time.Now.UtcDateTime,ExpiresAt=Time.Now.AddHours(2).UtcDateTime,LastActivityAt=Time.Now.UtcDateTime});
            var key=new SymmetricSecurityKey(RandomNumberGenerator.GetBytes(32));
            Token=new JwtSessionIssuer(new(key,"HS256"),"host","host").Issue(1,1,sid,window);
            Validator=new([key],["HS256"],"host","host",Clock,4096);
        }
        public Task AppendAsync(AuditEntry entry,CancellationToken ct)=>Task.CompletedTask;
        public Task<CurrentSessionSnapshot> ReadAsync(VerifiedSessionToken token,bool normal,CancellationToken ct)=>
            new CurrentSessionService(Data,Clock).ReadAsync(token,normal,ct);
        public Task<AccountAdminView> DetailAsync(VerifiedSessionToken token,int target,CancellationToken ct)
        { Calls++;return new AdminAccountService(Data,Data,Clock).DetailAsync(token,target,ct); }
        public Task<SafeCommandCompletion> LookupAsync(VerifiedSessionToken token,string action,Guid request,CancellationToken ct)
        { Calls++;return new AdminAccountService(Data,Data,Clock).LookupAsync(token,action,request,ct); }
        public async Task<AdminAccountCommandResult> LockAsync(VerifiedSessionToken token,AdminLockInput input,CancellationToken ct)
        {
            Calls++;var working=Data.Copy();var result=await new AdminAccountService(working,working,Clock).LockAsync(token,input,ct);
            if(PauseCommit){Paused.TrySetResult();await Resume.Task.WaitAsync(ct);}
            if(FailCommit)throw new ApiErrorException(ApiErrorCode.INTERNAL_ERROR);
            await new CurrentSessionService(working,Clock).ReadAsync(token,true,ct);
            Data=working;Commits++;return result;
        }
    }
    private sealed class Mount(State state):IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)=>app=>{
            next(app);app.UseRouting();app.UseAuthJsonBoundary();
            app.UseCsrfBoundary(Origin,new LiveCsrfIdentityResolver(state.Validator,state,new(state,new()),state.Clock));
            app.UseEndpoints(e=>{e.MapCsrfContext();e.MapAdminAccountPilot(state,4096);});
        };
    }
    private static WebApplicationFactory<Program> Factory(State state)=>new WebApplicationFactory<Program>().WithWebHostBuilder(
        b=>b.ConfigureServices(s=>{s.AddCsrfBoundary(new EphemeralDataProtectionProvider());s.AddSingleton<IStartupFilter>(new Mount(state));}));
    private static HttpClient Client(WebApplicationFactory<Program> f)=>f.CreateClient(new(){BaseAddress=new(Origin),HandleCookies=false,AllowAutoRedirect=false});
    private static HttpRequestMessage Request(State s,HttpMethod method,string path,string? cookie=null)
    {
        var request=new HttpRequestMessage(method,path);request.Headers.Add("Cookie",SafeCookiePolicy.AuthCookieName+"="+s.Token+(cookie is null?"":"; "+cookie));return request;
    }
    private static async Task<(string Token,string Cookie)> Csrf(State s,HttpClient c)
    {
        using var req=Request(s,HttpMethod.Get,"/api/v1/auth/csrf");using var response=await c.SendAsync(req,Ct);
        using var json=JsonDocument.Parse(await response.Content.ReadAsStringAsync(Ct));
        return (json.RootElement.GetProperty("requestToken").GetString()!,response.Headers.GetValues("Set-Cookie").Single().Split(';')[0]);
    }
    private const string Body="""{"expectedVersions":{"account":"AAAAAAAAAAE="}}""";
    private static async Task<HttpResponseMessage> Lock(State s,HttpClient c,(string Token,string Cookie) csrf,Guid key,string body=Body,string suffix="",bool origin=true,bool token=true,bool header=true)
    {
        using var request=Request(s,HttpMethod.Post,"/api/v1/admin/accounts/2/lock"+suffix,csrf.Cookie);
        request.Content=new StringContent(body,System.Text.Encoding.UTF8,"application/json");
        if(origin)request.Headers.Add("Origin",Origin);if(token)request.Headers.Add("X-CSRF-TOKEN",csrf.Token);
        if(header)request.Headers.Add("Idempotency-Key",key.ToString("D"));
        return await c.SendAsync(request,Ct);
    }
    [Fact] public async Task HttpCommitThenCurrentReplayAndSafeLookupPreserveExactShapes()
    {
        var s=new State();using var f=Factory(s);using var c=Client(f);var csrf=await Csrf(s,c);var key=Guid.NewGuid();
        using var first=await Lock(s,c,csrf,key);Assert.Equal(HttpStatusCode.OK,first.StatusCode);Assert.Equal(1,s.Commits);
        using var json=JsonDocument.Parse(await first.Content.ReadAsStringAsync(Ct));var op=json.RootElement.GetProperty("operationId").GetString();
        Assert.Equal(12,json.RootElement.GetProperty("account").EnumerateObject().Count());Assert.Equal(2,json.RootElement.EnumerateObject().Count());
        using var replay=await Lock(s,c,csrf,key);Assert.Equal(HttpStatusCode.OK,replay.StatusCode);Assert.Equal(1,s.Data.Audits);Assert.Single(s.Data.Markers);
        using var request=Request(s,HttpMethod.Get,"/api/v1/admin/command-results/"+key+"?action=ADMIN.ACCOUNT.LOCK");
        using var lookup=await c.SendAsync(request,Ct);Assert.Equal(HttpStatusCode.OK,lookup.StatusCode);
        using var safe=JsonDocument.Parse(await lookup.Content.ReadAsStringAsync(Ct));
        Assert.Equal(4,safe.RootElement.EnumerateObject().Count());Assert.Equal(op,safe.RootElement.GetProperty("operationId").GetString());
        Assert.Equal("COMPLETED",safe.RootElement.GetProperty("outcome").GetString());Assert.DoesNotContain("target@example.test",safe.RootElement.GetRawText());
        Assert.Equal("no-store",lookup.Headers.CacheControl!.ToString());
    }
    [Theory][InlineData("Teacher",false)][InlineData("Student",false)][InlineData("Admissions",false)]
    [InlineData("AcademicAffairs",false)][InlineData("Accountant",false)][InlineData("Director",false)][InlineData("Admin",true)]
    public async Task WrongRoleOrRestrictedDeniesBeforeDetailedBody(string role,bool restricted)
    {
        var s=new State(role,restricted);using var f=Factory(s);using var c=Client(f);var csrf=await Csrf(s,c);
        using var response=await Lock(s,c,csrf,Guid.NewGuid(),"{}",suffix:"?unknown=x");
        Assert.Equal(HttpStatusCode.Forbidden,response.StatusCode);Assert.Equal(0,s.Calls);
    }
    [Theory][InlineData("csrf",403)][InlineData("origin",403)][InlineData("header",400)][InlineData("body",400)]
    [InlineData("query",400)][InlineData("audit",500)][InlineData("storage",500)][InlineData("commit",500)]
    public async Task FailureCannotAcknowledgeOrPublishEffects(string fault,int status)
    {
        var s=new State();s.Data.AuditFails=fault=="audit";s.Data.StorageFails=fault=="storage";s.FailCommit=fault=="commit";
        using var f=Factory(s);using var c=Client(f);var csrf=await Csrf(s,c);
        using var response=await Lock(s,c,csrf,Guid.NewGuid(),fault=="body"?"{}":Body,fault=="query"?"?x=1":"",
            origin:fault!="origin",token:fault!="csrf",header:fault!="header");
        Assert.Equal(status,(int)response.StatusCode);Assert.Equal(0,s.Commits);Assert.Equal("ACTIVE",s.Data.Accounts[1].Status);
        Assert.Equal(0,s.Data.Audits);Assert.Empty(s.Data.Markers);
    }
    [Theory][InlineData("bad","action=ADMIN.ACCOUNT.LOCK",400)][InlineData("00000000-0000-0000-0000-000000000001","action=UNKNOWN",400)]
    [InlineData("00000000-0000-0000-0000-000000000001","action=ADMIN.ACCOUNT.LOCK&action=ADMIN.ACCOUNT.LOCK",400)]
    [InlineData("00000000-0000-0000-0000-000000000001","action=STUDENT.REGISTRATION.CREATE",403)]
    [InlineData("00000000-0000-0000-0000-000000000001","action=ADMIN.ACCOUNT.LOCK",404)]
    public async Task LookupSelectorsNeverInferCompletion(string key,string query,int status)
    {
        var s=new State();using var f=Factory(s);using var c=Client(f);using var request=Request(s,HttpMethod.Get,"/api/v1/admin/command-results/"+key+"?"+query);
        using var response=await c.SendAsync(request,Ct);Assert.Equal(status,(int)response.StatusCode);Assert.Empty(s.Data.Markers);
    }
    [Fact] public async Task ExpiryDuringPausedFixtureCommitDiscardsResultAndAllEffects()
    {
        var s=new State{PauseCommit=true};using var f=Factory(s);using var c=Client(f);var csrf=await Csrf(s,c);
        var pending=Lock(s,c,csrf,Guid.NewGuid());await s.Paused.Task.WaitAsync(TimeSpan.FromSeconds(10),Ct);
        s.Time.Now=s.Time.Now.AddMinutes(30);s.Resume.TrySetResult();using var response=await pending;
        Assert.Equal(HttpStatusCode.Unauthorized,response.StatusCode);Assert.Equal(0,s.Commits);Assert.Empty(s.Data.Markers);Assert.Equal("ACTIVE",s.Data.Accounts[1].Status);
    }
    [Fact] public async Task DetailReadIsExactAndDoesNotRenewActivity()
    {
        var s=new State();var before=s.Data.Sessions[0].LastActivityAt;using var f=Factory(s);using var c=Client(f);
        using var request=Request(s,HttpMethod.Get,"/api/v1/admin/accounts/2");using var response=await c.SendAsync(request,Ct);
        Assert.Equal(HttpStatusCode.OK,response.StatusCode);using var json=JsonDocument.Parse(await response.Content.ReadAsStringAsync(Ct));
        Assert.Equal(12,json.RootElement.EnumerateObject().Count());Assert.Equal(before,s.Data.Sessions[0].LastActivityAt);Assert.Equal(0,s.Commits);
    }
}
