namespace EnglishCenter.Tests.Api.Auth;

using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Json;
using EnglishCenter.Api.Api.Auth;
using EnglishCenter.Api.Api.Identity;
using EnglishCenter.Api.Common.Auditing;
using EnglishCenter.Api.Common.Security;
using EnglishCenter.Api.Common.Time;
using EnglishCenter.Api.Services.Auth;
using EnglishCenter.Api.Services.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Xunit;

public class AuthOperationHostingTests
{
    private const string Origin = "https://ec.example.invalid";
    private static string Secret() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(24));
    private static string Signup() => JsonSerializer.Serialize(new { fullName = "Synthetic student", dateOfBirth = "2000-01-01", email = "student@example.invalid", phone = "0912345678", password = Secret() });
    private static string Login() => JsonSerializer.Serialize(new { email = "account@example.invalid", password = Secret() });
    private sealed class State : IStudentSignupExecutor, ILoginExecutor, ITrustedLoginIpResolver, ISecurityAuditStore
    {
        internal bool FailCommit, RejectLogin, NoIp, FailAudit, PauseCommit, Body = true, Csrf = true;
        internal int SignupCalls, LoginCalls, AuditAttempts;
        internal readonly TaskCompletionSource Started = new(TaskCreationOptions.RunContinuationsAsynchronously);
        internal readonly TaskCompletionSource Commit = new(TaskCreationOptions.RunContinuationsAsynchronously);
        internal readonly BusinessClock Clock = new(TimeProvider.System);
        internal readonly LoginIpLimiter Limiter = new();
        public async Task<RegisterStudentResult> ExecuteAsync(StudentSignupInput input, CancellationToken ct)
        { SignupCalls++; await Finish(ct); return RegisterStudentResult.Created(); }
        public async Task<LoginDecision> ExecuteAsync(LoginInput input, CancellationToken ct)
        {
            LoginCalls++; await Finish(ct); if (RejectLogin) return LoginDecision.Rejected();
            var now = Clock.GetUtcNow(); var window = SessionIssuanceWindow.Normal(now); var signer = new JwtSessionIssuer(new SigningCredentials(new SymmetricSecurityKey(RandomNumberGenerator.GetBytes(32)), SecurityAlgorithms.HmacSha256), "fixture", "fixture");
            var token = signer.Issue(42, 1, RandomNumberGenerator.GetBytes(32), window);
            var view = new AuthContextView(42, "Synthetic account", "account@example.invalid", "Teacher", "NORMAL", Convert.ToBase64String(new byte[8]), now.UtcDateTime.ToString("O"), window.AbsoluteExpiresAt.UtcDateTime.ToString("O"), window.EffectiveExpiresAt.UtcDateTime.ToString("O"));
            return LoginDecision.Success(new(view, token, window.AbsoluteExpiresAt));
        }
        private async Task Finish(CancellationToken ct)
        { Started.TrySetResult(); if (PauseCommit) await Commit.Task.WaitAsync(ct); if (FailCommit) throw new InvalidOperationException("Synthetic commit failure."); }
        public IPAddress? Resolve(HttpContext context) => NoIp ? null : IPAddress.Parse("192.0.2.1");
        public Task AppendAsync(AuditEntry entry, CancellationToken ct)
        { AuditAttempts++; return FailAudit ? Task.FromException(new InvalidOperationException("Synthetic audit failure.")) : Task.CompletedTask; }
    }
    private static WebApplicationFactory<Program> Create(State state) => new WebApplicationFactory<Program>().WithWebHostBuilder(b => b.ConfigureServices(services =>
    {
        services.AddCsrfBoundary(new EphemeralDataProtectionProvider()); services.AddSingleton<IStartupFilter>(new Mount(state));
    }));
    private sealed class Mount(State state) : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) => app =>
        {
            next(app); app.UseRouting(); if (state.Body) app.UseAuthJsonBoundary(); if (state.Csrf) app.UseCsrfBoundary(Origin, new Resolver());
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapCsrfContext(); endpoints.MapStudentSignup(state, 2048);
                endpoints.MapLogin(state, state.Limiter, state, new SecurityAuditRecorder(state, new AuditIncidentCounter()), state.Clock, 2048);
            });
        };
    }
    private sealed class Resolver : ICsrfIdentityResolver
    {
        public Task<VerifiedCsrfIdentity?> ResolveAsync(HttpContext context, CancellationToken ct)
        {
            var mode = context.Request.Headers["Test-Identity"].ToString();
            return Task.FromResult<VerifiedCsrfIdentity?>(mode is "normal" or "restricted" ? new(new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, "42")], "FixtureOnly")), mode == "restricted") : null);
        }
    }
    private static HttpClient Client(WebApplicationFactory<Program> factory) => factory.CreateClient(new() { BaseAddress = new Uri(Origin), HandleCookies = true, AllowAutoRedirect = false });
    private static async Task<string> Token(HttpClient client, string? identity = null)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/auth/csrf"); if (identity is not null) request.Headers.Add("Test-Identity", identity);
        using var response = await client.SendAsync(request, TestContext.Current.CancellationToken); Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken)); return json.RootElement.GetProperty("requestToken").GetString()!;
    }
    private static async Task<HttpResponseMessage> Send(HttpClient client, string route, string body, string? token, string? identity = null)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/auth/" + route) { Content = new StringContent(body, System.Text.Encoding.UTF8, "application/json") };
        request.Headers.Add("Origin", Origin); if (token is not null) request.Headers.Add("X-CSRF-TOKEN", token); if (identity is not null) request.Headers.Add("Test-Identity", identity);
        return await client.SendAsync(request, TestContext.Current.CancellationToken);
    }
    private static async Task Code(HttpResponseMessage response, int status, string code)
    {
        Assert.Equal(status, (int)response.StatusCode); Assert.True(response.Headers.CacheControl!.NoStore);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken)); Assert.Equal(code, json.RootElement.GetProperty("code").GetString());
        Assert.False(response.Headers.TryGetValues("Set-Cookie", out var cookies) && cookies.Any(c => c.StartsWith(SafeCookiePolicy.AuthCookieName, StringComparison.Ordinal)));
    }
    [Fact]
    public async Task InvalidLoginDtoCannotBypassPerIpRequestCapAfterVerifiedCsrf()
    {
        var state=new State();using var factory=Create(state);using var client=Client(factory);var token=await Token(client);
        for(var i=0;i<100;i++)
        {
            using var response=await Send(client,"login","{}",token);
            Assert.Equal(HttpStatusCode.BadRequest,response.StatusCode);
        }
        using var limited=await Send(client,"login","{}",token);
        Assert.Equal(HttpStatusCode.TooManyRequests,limited.StatusCode);Assert.Equal(0,state.LoginCalls);
        Assert.NotNull(limited.Headers.RetryAfter);
    }
    [Fact]
    public async Task Signup201IsOnlyOutcomeMessageAndNeverAutoLogsIn()
    {
        var state = new State(); using var factory = Create(state); using var client = Client(factory); var token = await Token(client);
        using var response = await Send(client, "register", Signup(), token); Assert.Equal(HttpStatusCode.Created, response.StatusCode); Assert.True(response.Headers.CacheControl!.NoStore); Assert.Null(response.Headers.Location);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken)); Assert.Equal(2, body.RootElement.EnumerateObject().Count()); Assert.Equal("CREATED", body.RootElement.GetProperty("outcome").GetString()); Assert.False(response.Headers.Contains("Set-Cookie")); Assert.Equal(1, state.SignupCalls); Assert.Equal(0, state.LoginCalls);
    }
    [Theory]
    [InlineData("register")]
    [InlineData("login")]
    public async Task CsrfPrecedesStrictFieldErrorsAndNeverCallsExecutor(string route)
    {
        var state = new State(); using var factory = Create(state); using var client = Client(factory);
        using (var invalid = await Send(client, route, "{\"role\":\"Admin\"}", null)) await Code(invalid, 403, "CSRF_INVALID");
        var token = await Token(client); using (var invalid = await Send(client, route, "{\"role\":\"Admin\"}", token)) await Code(invalid, 400, "VALIDATION_ERROR");
        Assert.Equal(0, state.SignupCalls + state.LoginCalls);
    }
    [Theory]
    [InlineData("register")]
    [InlineData("login")]
    public async Task CommitFailureNeverReturnsSuccessOrSetsAuthCookie(string route)
    {
        var state = new State { FailCommit = true }; using var factory = Create(state); using var client = Client(factory); var token = await Token(client);
        using var response = await Send(client, route, route == "login" ? Login() : Signup(), token); await Code(response, 500, "INTERNAL_ERROR");
    }
    [Theory]
    [InlineData("register")]
    [InlineData("login")]
    public async Task NoResponseOrCookieBeforeExecutorConfirmsCommit(string route)
    {
        var state = new State { PauseCommit = true }; using var factory = Create(state); using var client = Client(factory); var token = await Token(client);
        var responseTask = Send(client, route, route == "login" ? Login() : Signup(), token); await state.Started.Task.WaitAsync(TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken); Assert.False(responseTask.IsCompleted); state.Commit.TrySetResult();
        using var response = await responseTask; Assert.True(response.IsSuccessStatusCode);
    }
    [Fact]
    public async Task LoginResponseIsExactlyNineFieldsAndJwtOnlySecureCookie()
    {
        var state = new State(); using var factory = Create(state); using var client = Client(factory); var token = await Token(client);
        using var response = await Send(client, "login", Login(), token); Assert.Equal(HttpStatusCode.OK, response.StatusCode); Assert.True(response.Headers.CacheControl!.NoStore);
        var cookie = Assert.Single(response.Headers.GetValues("Set-Cookie")); var hasAuthName = cookie.StartsWith(SafeCookiePolicy.AuthCookieName, StringComparison.Ordinal); Assert.True(hasAuthName); Assert.Contains("httponly", cookie); Assert.Contains("secure", cookie); Assert.Contains("samesite=strict", cookie); Assert.DoesNotContain("domain=", cookie); Assert.Contains("expires=", cookie);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken)); Assert.Equal(new[] { "accountId", "accountVersion", "absoluteExpiresAt", "effectiveExpiresAt", "email", "fullName", "role", "scope", "serverTime" }.Order(), body.RootElement.EnumerateObject().Select(x => x.Name).Order());
    }
    [Fact]
    public async Task GenericLoginFailureAndMissingTrustedIpHaveNoCookieOrAccountDetails()
    {
        var state = new State { RejectLogin = true }; using var factory = Create(state); using var client = Client(factory); var token = await Token(client);
        using (var denied = await Send(client, "login", Login(), token)) await Code(denied, 401, "AUTHENTICATION_FAILED");
        state.NoIp = true; using (var missing = await Send(client, "login", Login(), token)) await Code(missing, 503, "SERVICE_UNAVAILABLE"); Assert.Equal(1, state.LoginCalls);
    }
    [Fact]
    public async Task Ip101Is429WithRetryAfterAndAuditFailureCannotBypassLimit()
    {
        var state = new State { RejectLogin = true, FailAudit = true }; using var factory = Create(state); using var client = Client(factory); var token = await Token(client);
        for (var i = 0; i < 100; i++) { using var response = await Send(client, "login", Login(), token); Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode); }
        using var limited = await Send(client, "login", Login(), token); await Code(limited, 429, "RATE_LIMITED"); Assert.NotNull(limited.Headers.RetryAfter); Assert.Equal(100, state.LoginCalls); Assert.Equal(1, state.AuditAttempts);
    }
    [Fact]
    public async Task AuthAndScopeGuardsPrecedeSignupAndRestrictedLoginShapeDiagnostics()
    {
        var state = new State(); using var factory = Create(state); using var client = Client(factory);
        var token = await Token(client, "normal"); using (var forbidden = await Send(client, "register", "{}", token, "normal")) await Code(forbidden, 403, "FORBIDDEN");
        token = await Token(client, "restricted"); using (var forbidden = await Send(client, "login", "{}", token, "restricted")) await Code(forbidden, 403, "FORBIDDEN"); Assert.Equal(0, state.SignupCalls + state.LoginCalls);
    }
    [Fact]
    public async Task OversizeRequestFailsBeforeCsrfAndNeverSpillsOrCallsExecutor()
    {
        var state = new State(); using var factory = Create(state); using var client = Client(factory);
        using var response = await Send(client, "register", new string('x', 2049), null); await Code(response, 413, "REQUEST_TOO_LARGE"); Assert.Equal(0, state.SignupCalls);
    }
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task MissingRequiredBoundaryFailsClosed(bool body)
    {
        var state = new State { Body = body, Csrf = !body }; using var factory = Create(state); using var client = Client(factory);
        var token = state.Csrf ? await Token(client) : null; using var response = await Send(client, "register", Signup(), token); await Code(response, 503, "SERVICE_UNAVAILABLE"); Assert.Equal(0, state.SignupCalls);
    }
}
