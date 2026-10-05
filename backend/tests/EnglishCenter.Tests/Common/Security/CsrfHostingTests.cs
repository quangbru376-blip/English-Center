namespace EnglishCenter.Tests.Common.Security;

using System.Net;
using System.Security.Claims;
using System.Text.Json;
using EnglishCenter.Api.Api.Auth;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Common.Security;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

public class CsrfHostingTests
{
    private const string Origin="https://ec.example.invalid";
    private static WebApplicationFactory<Program> Create() => new WebApplicationFactory<Program>().WithWebHostBuilder(b=>b.ConfigureServices(s=>
    {
        s.AddCsrfBoundary(new EphemeralDataProtectionProvider());s.AddSingleton<IStartupFilter,BoundaryFixture>();
    }));
    private static HttpClient Client(WebApplicationFactory<Program> factory) => factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress=new Uri(Origin),AllowAutoRedirect=false,HandleCookies=true });
    private static async Task<(string Token,HttpResponseMessage Response)> Token(HttpClient client,string? identity=null)
    {
        using var request=new HttpRequestMessage(HttpMethod.Get,"/api/v1/auth/csrf");
        if(identity is not null) request.Headers.Add("Test-Identity",identity);
        var response=await client.SendAsync(request,TestContext.Current.CancellationToken);
        using var json=JsonDocument.Parse(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        Assert.Equal(HttpStatusCode.OK,response.StatusCode);Assert.Single(json.RootElement.EnumerateObject());
        return (json.RootElement.GetProperty("requestToken").GetString()!,response);
    }
    private static async Task<HttpResponseMessage> Post(HttpClient client,string? token,string? origin=Origin,string? identity=null,string path="/api/v1/test/anonymous")
    {
        using var request=new HttpRequestMessage(HttpMethod.Post,path) { Content=new StringContent("{}",System.Text.Encoding.UTF8,"application/json") };
        if(token is not null) request.Headers.Add("X-CSRF-TOKEN",token);
        if(origin is not null) request.Headers.Add("Origin",origin);
        if(identity is not null) request.Headers.Add("Test-Identity",identity);
        return await client.SendAsync(request,TestContext.Current.CancellationToken);
    }
    private static async Task Code(HttpResponseMessage response,int status,string code)
    {
        Assert.Equal(status,(int)response.StatusCode);
        using var json=JsonDocument.Parse(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));Assert.Equal(code,json.RootElement.GetProperty("code").GetString());
        Assert.True(response.Headers.CacheControl!.NoStore);
    }

    [Fact]
    public async Task FrameworkRoundTripReturnsOnlyTokenAndSecureHostCookieAndNoStore()
    {
        using var factory=Create();using var client=Client(factory);var (token,context)=await Token(client);using(context)
        {
            Assert.True(context.Headers.CacheControl!.NoStore);var cookie=Assert.Single(context.Headers.GetValues("Set-Cookie"));
            Assert.Contains(SafeCookiePolicy.CsrfCookieName,cookie);Assert.Contains("secure",cookie);Assert.Contains("httponly",cookie);Assert.Contains("samesite=strict",cookie);Assert.DoesNotContain("domain=",cookie);
            using var write=await Post(client,token);Assert.Equal(HttpStatusCode.NoContent,write.StatusCode);Assert.True(write.Headers.CacheControl!.NoStore);
        }
    }
    [Theory]
    [InlineData("missing-token")] [InlineData("bad-token")] [InlineData("missing-origin")] [InlineData("foreign-origin")]
    [InlineData("duplicate-token")] [InlineData("duplicate-origin")]
    public async Task MissingBadAndDuplicateTokensOrOriginsNeverReachHandler(string mode)
    {
        using var factory=Create();using var client=Client(factory);var (token,context)=await Token(client);context.Dispose();
        using var request=new HttpRequestMessage(HttpMethod.Post,"/api/v1/test/anonymous");
        if(mode!="missing-token") request.Headers.TryAddWithoutValidation("X-CSRF-TOKEN",mode=="duplicate-token" ? new[]{token,token} : new[]{mode=="bad-token"?"invalid":token});
        if(mode!="missing-origin") request.Headers.TryAddWithoutValidation("Origin",mode=="duplicate-origin" ? new[]{Origin,Origin} : new[]{mode=="foreign-origin"?"https://evil.invalid":Origin});
        using var response=await client.SendAsync(request,TestContext.Current.CancellationToken);await Code(response,403,"CSRF_INVALID");
    }
    [Fact]
    public async Task IdentityTransitionRequiresFreshFrameworkTokenAndNoSessionCookieIsIssued()
    {
        using var factory=Create();using var client=Client(factory);var (old,anonymous)=await Token(client);anonymous.Dispose();
        using(var rejected=await Post(client,old,identity:"normal",path:"/api/v1/test/protected")) await Code(rejected,403,"CSRF_INVALID");
        var (fresh,current)=await Token(client,"normal");using(current)
        {
            if(current.Headers.TryGetValues("Set-Cookie",out var cookies)) Assert.DoesNotContain(cookies,c=>c.Contains(SafeCookiePolicy.AuthCookieName,StringComparison.Ordinal));
            using var accepted=await Post(client,fresh,identity:"normal",path:"/api/v1/test/protected");Assert.Equal(HttpStatusCode.NoContent,accepted.StatusCode);
        }
    }
    [Fact]
    public async Task VerifiedSessionPrecedesCsrfAndRestrictedScopeFollowsCsrf()
    {
        using var factory=Create();using var client=Client(factory);
        using(var invalid=await Post(client,null,origin:null,identity:"invalid",path:"/api/v1/test/protected")) await Code(invalid,401,"AUTHENTICATION_REQUIRED");
        var (token,context)=await Token(client,"restricted");context.Dispose();
        using(var missing=await Post(client,null,identity:"restricted",path:"/api/v1/test/protected")) await Code(missing,403,"CSRF_INVALID");
        using(var denied=await Post(client,token,identity:"restricted",path:"/api/v1/test/protected")) await Code(denied,403,"FORBIDDEN");
    }
    [Fact]
    public async Task UnverifiedCookieAndInvalidTransportFailClosedWithoutContextOrForwardedFallback()
    {
        using var factory=Create();using var client=Client(factory);
        using var stale=new HttpRequestMessage(HttpMethod.Get,"/api/v1/auth/csrf");stale.Headers.Add("Cookie",SafeCookiePolicy.AuthCookieName+"=synthetic");
        using(var response=await client.SendAsync(stale,TestContext.Current.CancellationToken)) await Code(response,401,"AUTHENTICATION_REQUIRED");
        using var http=new HttpRequestMessage(HttpMethod.Get,"http://ec.example.invalid/api/v1/auth/csrf");http.Headers.Add("X-Forwarded-Proto","https");
        using(var response=await client.SendAsync(http,TestContext.Current.CancellationToken)) await Code(response,503,"SERVICE_UNAVAILABLE");
    }

    [Theory]
    [InlineData("PUT")] [InlineData("PATCH")] [InlineData("DELETE")]
    public async Task EveryUnsafeMethodRequiresHeaderTokenEvenWithValidFrameworkFormToken(string method)
    {
        using var factory=Create();using var client=Client(factory);var (token,context)=await Token(client);context.Dispose();
        using var request=new HttpRequestMessage(new HttpMethod(method),"/api/v1/test/all-unsafe") { Content=new FormUrlEncodedContent(new Dictionary<string,string> { ["__RequestVerificationToken"]=token }) };
        request.Headers.Add("Origin",Origin);
        using var rejected=await client.SendAsync(request,TestContext.Current.CancellationToken);await Code(rejected,403,"CSRF_INVALID");
        using var valid=new HttpRequestMessage(new HttpMethod(method),"/api/v1/test/all-unsafe");valid.Headers.Add("Origin",Origin);valid.Headers.Add("X-CSRF-TOKEN",token);
        using var accepted=await client.SendAsync(valid,TestContext.Current.CancellationToken);Assert.Equal(HttpStatusCode.NoContent,accepted.StatusCode);
    }
    [Fact]
    public async Task UnclassifiedApiEndpointFailsClosed()
    {
        using var factory=Create();using var client=Client(factory);
        using var request=new HttpRequestMessage(HttpMethod.Post,"/api/v1/test/unclassified");
        using var response=await client.SendAsync(request,TestContext.Current.CancellationToken);Assert.Equal(HttpStatusCode.ServiceUnavailable,response.StatusCode);
    }

    [Fact]
    public async Task MappingContextWithoutVerifiedBoundaryCannotMintToken()
    {
        using var factory=new WebApplicationFactory<Program>().WithWebHostBuilder(b=>b.ConfigureServices(services=>
        {
            services.AddCsrfBoundary(new EphemeralDataProtectionProvider());services.AddSingleton<IStartupFilter,UnprotectedFixture>();
        }));
        using var client=Client(factory);using var response=await client.GetAsync("/api/v1/auth/csrf",TestContext.Current.CancellationToken);
        await Code(response,503,"SERVICE_UNAVAILABLE");Assert.False(response.Headers.Contains("Set-Cookie"));
    }
    private sealed class UnprotectedFixture : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)=>app=>
        {
            next(app);app.UseRouting();app.UseEndpoints(endpoints=>endpoints.MapCsrfContext());
        };
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public async Task ContextRejectsBusinessQueryOrRequestBody(bool body)
    {
        using var factory=Create();using var client=Client(factory);
        using var request=new HttpRequestMessage(HttpMethod.Get,body?"/api/v1/auth/csrf":"/api/v1/auth/csrf?accountId=1");
        if(body) request.Content=new StringContent("{}");
        using var response=await client.SendAsync(request,TestContext.Current.CancellationToken);await Code(response,400,"VALIDATION_ERROR");
        Assert.False(response.Headers.Contains("Set-Cookie"));
    }

    private sealed class Resolver : ICsrfIdentityResolver
    {
        public Task<VerifiedCsrfIdentity?> ResolveAsync(HttpContext context,CancellationToken cancellationToken)
        {
            var mode=context.Request.Headers["Test-Identity"].ToString();
            if(mode=="invalid") throw new ApiErrorException(ApiErrorCode.AUTHENTICATION_REQUIRED);
            if(mode is "normal" or "restricted") return Task.FromResult<VerifiedCsrfIdentity?>(new(new ClaimsPrincipal(new ClaimsIdentity(new[]{new Claim(ClaimTypes.NameIdentifier,"1")},"TestOnlyVerifiedSession")),mode=="restricted"));
            return Task.FromResult<VerifiedCsrfIdentity?>(null);
        }
    }
    private sealed class BoundaryFixture : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) => app=>
        {
            next(app);app.UseRouting();app.UseCsrfBoundary(Origin,new Resolver());
            app.UseEndpoints(endpoints=>
            {
                endpoints.MapCsrfContext();
                endpoints.MapMethods("/api/v1/test/all-unsafe",new[]{"PUT","PATCH","DELETE"},()=>Results.NoContent()).WithMetadata(new CsrfRoutePolicy(CsrfAccess.AnonymousOnly)).AllowAnonymous();
                endpoints.MapPost("/api/v1/test/unclassified",()=>Results.NoContent()).AllowAnonymous();
                endpoints.MapPost("/api/v1/test/anonymous",()=>Results.NoContent()).WithMetadata(new CsrfRoutePolicy(CsrfAccess.AnonymousOnly)).AllowAnonymous();
                endpoints.MapPost("/api/v1/test/protected",()=>Results.NoContent()).WithMetadata(new CsrfRoutePolicy(CsrfAccess.Authenticated,true)).AllowAnonymous();
            });
        };
    }
}
