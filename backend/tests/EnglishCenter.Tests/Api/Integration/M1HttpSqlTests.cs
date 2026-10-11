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
using EnglishCenter.Api.Infrastructure.Enrollment;
using EnglishCenter.Api.Api.Enrollment;
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
    [Fact(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    public async Task AdmissionsHttp_ApprovalReplayCsrfExactViewsAndRoleSplit045()
    {
        await using var f=await OwnedSqlFixture.CreateAsync(new M1AdmissionsSqlTests.Clock());var id=await M1AdmissionsSqlTests.Confirmed(f);await using var host=await Host.Create(f);await host.Login("Admissions");
        using var preview=await host.Send(HttpMethod.Post,$"/api/v1/admissions/registrations/{id}/approval-preview");Assert.Equal(HttpStatusCode.OK,preview.StatusCode);
        var body=(await preview.Content.ReadFromJsonAsync<JsonElement>(Ct)).GetProperty("manifest");var key=Guid.NewGuid();
        using var csrf=await host.Send(HttpMethod.Post,$"/api/v1/admissions/registrations/{id}/approve",body,key,csrf:false);Assert.Equal(HttpStatusCode.Forbidden,csrf.StatusCode);
        using var first=await host.Send(HttpMethod.Post,$"/api/v1/admissions/registrations/{id}/approve",body,key);Assert.Equal(HttpStatusCode.OK,first.StatusCode);
        var result=await first.Content.ReadFromJsonAsync<JsonElement>(Ct);Assert.Equal(2,result.EnumerateObject().Count());Assert.Equal(16,result.GetProperty("registration").EnumerateObject().Count());
        var invoice=result.GetProperty("registration").GetProperty("invoice");Assert.Equal(4,invoice.EnumerateObject().Count());
        using var repeated=await host.Send(HttpMethod.Post,$"/api/v1/admissions/registrations/{id}/approve",body,key);Assert.Equal(HttpStatusCode.OK,repeated.StatusCode);Assert.Equal(result.GetProperty("operationId").GetGuid(),(await repeated.Content.ReadFromJsonAsync<JsonElement>(Ct)).GetProperty("operationId").GetGuid());
        using var lookup=await host.Send(HttpMethod.Get,$"/api/v1/admissions/command-results/{key:D}?action=ADMISSIONS.REGISTRATION.APPROVE");Assert.Equal(HttpStatusCode.OK,lookup.StatusCode);Assert.Equal(2,(await lookup.Content.ReadFromJsonAsync<JsonElement>(Ct)).GetProperty("target").EnumerateObject().Count());
        using var otherFamily=await host.Send(HttpMethod.Get,$"/api/v1/admissions/command-results/{key:D}?action=ACADEMIC.ENTRY_SESSION.CREATE");Assert.Equal(HttpStatusCode.Forbidden,otherFamily.StatusCode);
        await host.Login("AcademicAffairs");using var academicFamily=await host.Send(HttpMethod.Get,$"/api/v1/admissions/command-results/{key:D}?action=ADMISSIONS.REGISTRATION.APPROVE");Assert.Equal(HttpStatusCode.Forbidden,academicFamily.StatusCode);
        await host.Login("Teacher");using var wrongRoleMalformed=await host.Send(HttpMethod.Get,"/api/v1/admissions/command-results/not-a-uuid");Assert.Equal(HttpStatusCode.Forbidden,wrongRoleMalformed.StatusCode);
        await host.Login("Student");using var own=await host.Send(HttpMethod.Get,$"/api/v1/students/me/invoices/{invoice.GetProperty("id").GetInt32()}");Assert.Equal(HttpStatusCode.OK,own.StatusCode);Assert.Equal(12,(await own.Content.ReadFromJsonAsync<JsonElement>(Ct)).EnumerateObject().Count());
    }
    [Fact(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    public async Task AdmissionsHttp_SearchPIIOnlyBodyAndFreshTargetBeforeMalformedBusinessBody()
    {
        await using var f=await OwnedSqlFixture.CreateAsync(new M1AdmissionsSqlTests.Clock());var id=await M1AdmissionsSqlTests.Confirmed(f);await using var host=await Host.Create(f);await host.Login("Admissions");
        using var query=await host.Send(HttpMethod.Post,"/api/v1/admissions/registrations/search?email=private",new{});Assert.Equal(HttpStatusCode.BadRequest,query.StatusCode);
        using var search=await host.Send(HttpMethod.Post,"/api/v1/admissions/registrations/search",new{phone="+84 912-345-678",status="CONFIRMED"});Assert.Equal(HttpStatusCode.OK,search.StatusCode);var page=await search.Content.ReadFromJsonAsync<JsonElement>(Ct);Assert.Equal(1,page.GetProperty("totalCount").GetInt32());Assert.Equal(11,page.GetProperty("items")[0].EnumerateObject().Count());
        using var missing=await host.Send(HttpMethod.Post,"/api/v1/admissions/registrations/2147483647/reject",new{},Guid.NewGuid());Assert.Equal(HttpStatusCode.NotFound,missing.StatusCode);
        using var noReason=await host.Send(HttpMethod.Post,$"/api/v1/admissions/registrations/{id}/reject",new{},Guid.NewGuid());Assert.Equal(HttpStatusCode.BadRequest,noReason.StatusCode);
        await host.Login("Admin");using var wrongRole=await host.Send(HttpMethod.Post,$"/api/v1/admissions/registrations/{id}/reject",new{},Guid.NewGuid());Assert.Equal(HttpStatusCode.Forbidden,wrongRole.StatusCode);
    }
    [Fact(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    public async Task StudentInvoiceHttp_ForeignOwn404Restricted403AndNoPaymentFields()
    {
        await using var f=await OwnedSqlFixture.CreateAsync(new M1AdmissionsSqlTests.Clock());var id=await M1AdmissionsSqlTests.Confirmed(f);var token=f.Token(await f.Grant("Admissions"));var ex=new EnglishCenter.Api.Infrastructure.Admissions.SqlAdmissionsExecutor(()=>f.Db(),f.Clock,10,1000);
        var p=await ex.ApprovalAsync(token,id,Ct);var approved=await ex.CommandAsync(token,41,id,Guid.NewGuid(),JsonSerializer.Serialize(p.Manifest),65536,Ct);var invoice=approved.Registration.Invoice!.Id;
        var input=EnglishCenter.Api.Services.Identity.StudentSignupInput.Parse(JsonSerializer.Serialize(new{fullName="Other Student",dateOfBirth="2000-01-01",email="signup@example.invalid",phone="0987654321",password=f.Password,address=(string?)null}));await f.Signup.ExecuteAsync(input,Ct);
        await using var host=await Host.Create(f);await host.Login("signup");
        using var foreign=await host.Send(HttpMethod.Get,$"/api/v1/students/me/invoices/{invoice}");using var absent=await host.Send(HttpMethod.Get,"/api/v1/students/me/invoices/2147483647");Assert.Equal(HttpStatusCode.NotFound,foreign.StatusCode);Assert.Equal(HttpStatusCode.NotFound,absent.StatusCode);var absentError=await absent.Content.ReadFromJsonAsync<JsonElement>(Ct);var foreignError=await foreign.Content.ReadFromJsonAsync<JsonElement>(Ct);foreach(var field in new[]{"code","message","fieldErrors"})Assert.Equal(absentError.GetProperty(field).GetRawText(),foreignError.GetProperty(field).GetRawText());Assert.True(Guid.TryParse(foreignError.GetProperty("traceId").GetString(),out _));
        using var filter=await host.Send(HttpMethod.Get,$"/api/v1/students/me/invoices?registrationId={id}");Assert.Equal(HttpStatusCode.NotFound,filter.StatusCode);
        using var empty=await host.Send(HttpMethod.Get,"/api/v1/students/me/invoices");Assert.Equal(HttpStatusCode.OK,empty.StatusCode);Assert.Equal(0,(await empty.Content.ReadFromJsonAsync<JsonElement>(Ct)).GetProperty("totalCount").GetInt32());
        await f.Restricted();await host.Login("Student");using var restricted=await host.Send(HttpMethod.Get,$"/api/v1/students/me/invoices/{invoice}");Assert.Equal(HttpStatusCode.Forbidden,restricted.StatusCode);
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
            app.MapCourseCatalogue(new SqlCourseExecutor(()=>f.Db(),f.Clock,10,1000),4096);app.MapTrainingPrograms(new SqlProgramExecutor(()=>f.Db(),f.Clock,10,1000),4096);app.MapEntrySessions(new SqlEntrySessionExecutor(()=>f.Db(),f.Clock,10,1000),4096,new(()=>f.Db(),f.Clock,10,1000),new(()=>f.Db(),f.Clock,10,1000));
            app.MapStudentEnrollment(new(()=>f.Db(),f.Clock,10,1000),new(()=>f.Db(),f.Clock,10,1000),new(()=>f.Db(),f.Clock,10,1000),4096);
            app.MapTeacherEntry(new(()=>f.Db(),f.Clock,10,1000),new(()=>f.Db(),f.Clock,10,1000),4096);
            EnglishCenter.Api.Api.Finance.FinanceEndpoints.MapFinance(app,new(()=>f.Db(),f.Clock,10,1000),4096);
            EnglishCenter.Api.Api.Classes.ClassEndpoints.MapClasses(app,new(()=>f.Db(),f.Clock,10,1000),65536);
            EnglishCenter.Api.Api.Admissions.AdmissionsEndpoints.MapAdmissions(app,new(()=>f.Db(),f.Clock,10,1000),4096);
            EnglishCenter.Api.Api.StudentInvoices.StudentInvoiceEndpoints.MapStudentInvoices(app,new(()=>f.Db(),f.Clock,10,1000));
            await app.StartAsync(Ct);var client=app.GetTestClient();client.BaseAddress=new Uri("https://localhost");return new(app,client){fixturePassword=f.Password};
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
