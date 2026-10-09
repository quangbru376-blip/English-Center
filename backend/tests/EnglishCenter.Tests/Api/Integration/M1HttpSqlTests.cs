namespace EnglishCenter.Tests.Api.Integration;

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using EnglishCenter.Api.Api.Administration;
using EnglishCenter.Api.Api.Auth;
using EnglishCenter.Api.Api.Catalogue;
using EnglishCenter.Api.Api.Identity;
using EnglishCenter.Api.Api.Academics;
using EnglishCenter.Api.Infrastructure.Academics;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Common.Security;
using EnglishCenter.Api.Infrastructure.Administration;
using EnglishCenter.Api.Infrastructure.Catalogue;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Logging;
using Xunit;

[Collection("V01 HTTP SQL")]
public sealed class M1HttpSqlTests
{
    private static CancellationToken Ct=>OwnedSqlFixture.Ct;
    private const string OptIn="M1 HTTP to isolated owned SQL requires explicit gates.";
    [Fact(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    public async Task StaffHttp_ExactFirstReplayAndCsrfRoleGuards()
    {
        await using var fixture=await OwnedSqlFixture.CreateAsync();await using var host=await Host.Create(fixture);
        await host.Login("Admin");var body=new{fullName="New Teacher",email="http@example.invalid",role="Teacher"};var key=Guid.NewGuid();
        using var denied=await host.Send(HttpMethod.Post,"/api/v1/admin/staff-accounts",body,key,csrf:false);Assert.Equal(HttpStatusCode.Forbidden,denied.StatusCode);
        using var first=await host.Send(HttpMethod.Post,"/api/v1/admin/staff-accounts",body,key);Assert.Equal(HttpStatusCode.Created,first.StatusCode);
        var initial=await first.Content.ReadFromJsonAsync<JsonElement>(Ct);Assert.Equal(7,initial.EnumerateObject().Count());
        using var replay=await host.Send(HttpMethod.Post,"/api/v1/admin/staff-accounts",body,key);Assert.Equal(HttpStatusCode.OK,replay.StatusCode);
        var completed=await replay.Content.ReadFromJsonAsync<JsonElement>(Ct);Assert.Equal("SECRET_RESULT_UNAVAILABLE",completed.GetProperty("resultMode").GetString());Assert.False(completed.TryGetProperty("temporaryPassword",out _));
        using var search=await host.Send(HttpMethod.Post,"/api/v1/admin/accounts/search",new{email="http@"});Assert.Equal(HttpStatusCode.OK,search.StatusCode);
        var page=await search.Content.ReadFromJsonAsync<JsonElement>(Ct);Assert.Equal(1,page.GetProperty("totalCount").GetInt32());
        await host.Login("AcademicAffairs");using var wrongRole=await host.Send(HttpMethod.Post,"/api/v1/admin/accounts/search",new{});Assert.Equal(HttpStatusCode.Forbidden,wrongRole.StatusCode);
    }
    [Fact(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    public async Task CourseHttp_StatusCodesReplayExactShapeAndAllowlist()
    {
        await using var fixture=await OwnedSqlFixture.CreateAsync();await using var host=await Host.Create(fixture);await host.Login("AcademicAffairs");
        var body=new{code="TOEIC",name="TOEIC",basePrice="123456789012345678",status="INACTIVE"};var key=Guid.NewGuid();
        using var first=await host.Send(HttpMethod.Post,"/api/v1/academics/courses",body,key);Assert.Equal(HttpStatusCode.Created,first.StatusCode);
        using var replay=await host.Send(HttpMethod.Post,"/api/v1/academics/courses",body,key);Assert.Equal(HttpStatusCode.OK,replay.StatusCode);
        var result=await replay.Content.ReadFromJsonAsync<JsonElement>(Ct);Assert.Equal(2,result.EnumerateObject().Count());Assert.False(result.TryGetProperty("replayed",out _));
        using var list=await host.Send(HttpMethod.Get,"/api/v1/academics/courses?status=INACTIVE");Assert.Equal(HttpStatusCode.OK,list.StatusCode);
        using var malformed=await host.Send(HttpMethod.Get,"/api/v1/academics/courses?name=");Assert.Equal(HttpStatusCode.BadRequest,malformed.StatusCode);
        using var lookup=await host.Send(HttpMethod.Get,$"/api/v1/academics/command-results/{key:D}?action=ACADEMIC.COURSE.CREATE");Assert.Equal(HttpStatusCode.OK,lookup.StatusCode);
        using var missingProgram=await host.Send(HttpMethod.Get,$"/api/v1/academics/command-results/{key:D}?action=ACADEMIC.PROGRAM.CREATE");Assert.Equal(HttpStatusCode.NotFound,missingProgram.StatusCode);
        await host.Login("Admin");using var wrongRole=await host.Send(HttpMethod.Get,"/api/v1/academics/courses");Assert.Equal(HttpStatusCode.Forbidden,wrongRole.StatusCode);
    }
    internal sealed class Host(WebApplication app,HttpClient client) : IAsyncDisposable
    {
        private readonly Dictionary<string,string> cookies=[];private string? csrf;
        public static async Task<Host> Create(OwnedSqlFixture f)
        {
            var builder=WebApplication.CreateBuilder();builder.WebHost.UseTestServer();builder.Logging.ClearProviders();builder.Services.AddCsrfBoundary(new EphemeralDataProtectionProvider());
            var app=builder.Build();app.UseApiErrorMiddleware();app.UseRouting();app.UseAuthJsonBoundary();app.UseCsrfBoundary("https://localhost",new LiveCsrfIdentityResolver(f.Validator,f.Current,f.Audit,f.Clock));
            app.MapCsrfContext();app.MapLogin(f.Login,new LoginIpLimiter(),new Ip(),f.Audit,f.Clock,4096);app.MapCurrentSession(f.Current,f.Clock);
            app.MapAdminAccountPilot(f.Admin,4096);app.MapStaffAccounts(new SqlStaffAccountExecutor(()=>f.Db(),f.Clock,f.Hasher,new TemporaryPasswordGenerator(new PasswordPolicy(OfflinePasswordBlocklist.Load(Path.Combine(AppContext.BaseDirectory,"Resources","PasswordBlocklist")))),10,1000),4096);
            app.MapCourseCatalogue(new SqlCourseExecutor(()=>f.Db(),f.Clock,10,1000),4096);app.MapTrainingPrograms(new SqlProgramExecutor(()=>f.Db(),f.Clock,10,1000),4096);app.MapEntrySessions(new SqlEntrySessionExecutor(()=>f.Db(),f.Clock,10,1000),4096);await app.StartAsync(Ct);var client=app.GetTestClient();client.BaseAddress=new Uri("https://localhost");return new(app,client){fixturePassword=f.Password};
        }
        public async Task Login(string role)
        {
            cookies.Clear();csrf=null;await Csrf();using var response=await Send(HttpMethod.Post,"/api/v1/auth/login",new{email=role.ToLowerInvariant()+"@example.invalid",password=fixturePassword});Assert.Equal(HttpStatusCode.OK,response.StatusCode);csrf=null;await Csrf();
        }
        private string fixturePassword="";
        private async Task Csrf(){using var response=await Send(HttpMethod.Get,"/api/v1/auth/csrf");Assert.Equal(HttpStatusCode.OK,response.StatusCode);csrf=(await response.Content.ReadFromJsonAsync<JsonElement>(Ct)).GetProperty("requestToken").GetString();}
        public async Task<HttpResponseMessage> Send(HttpMethod method,string path,object? body=null,Guid? key=null,bool csrf=true)
        {
            using var request=new HttpRequestMessage(method,path);if(body is not null)request.Content=JsonContent.Create(body);
            if(method!=HttpMethod.Get){request.Headers.Add("Origin","https://localhost");if(csrf&&this.csrf is not null)request.Headers.Add("X-CSRF-TOKEN",this.csrf);}
            if(key is {} id)request.Headers.Add("Idempotency-Key",id.ToString("D"));if(cookies.Count>0)request.Headers.Add("Cookie",string.Join("; ",cookies.Values));
            var response=await client.SendAsync(request,Ct);if(response.Headers.TryGetValues("Set-Cookie",out var values))foreach(var value in values){var pair=value.Split(';')[0];cookies[pair.Split('=')[0]]=pair;}return response;
        }
        public async ValueTask DisposeAsync(){client.Dispose();await app.StopAsync(CancellationToken.None);await app.DisposeAsync();}
        private sealed class Ip : ITrustedLoginIpResolver{public IPAddress? Resolve(HttpContext context)=>IPAddress.Loopback;}
    }
}
