namespace EnglishCenter.Tests.Api.Auth;

using System.Net;
using System.Security.Cryptography;
using System.Text.Json;
using EnglishCenter.Api.Api.Auth;
using EnglishCenter.Api.Api.Identity;
using EnglishCenter.Api.Common.Serialization;
using Microsoft.AspNetCore.Identity;
using EnglishCenter.Api.Common.Auditing;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Common.Security;
using EnglishCenter.Api.Common.Time;
using EnglishCenter.Api.Domain.Identity;
using EnglishCenter.Api.Services.Auth;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.IdentityModel.Tokens;
using Xunit;

public class CurrentSessionHostingTests
{
    private const string Origin = "https://localhost";
    private sealed class Clock : TimeProvider
    {
        internal DateTimeOffset Now = DateTimeOffset.FromUnixTimeSeconds(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        public override DateTimeOffset GetUtcNow() => Now;
    }
    private sealed class State : ICurrentSessionReader, ICurrentSessionLogout, IPasswordChangeExecutor, ISecurityAuditStore
    {
        internal readonly Clock Time = new(); internal BusinessClock Clock => new(Time);
        internal readonly AuditIncidentCounter Incidents = new();
        internal readonly JwtSessionValidator Validator;
        internal readonly Account Account;
        internal readonly AuthSession Session;
        internal readonly string Encoded;
        internal string Role;
        internal bool PauseCommit, FailCommit, AuditFails, AuditCanceled, RevokeOnSecondRead, Csrf = true, LegacyResolver;
        internal int Reads, Revokes, AuditAttempts;
        internal string? CurrentSecret, NewSecret;
        internal AuthSession? OtherOwn;
        internal bool MandatoryFails;
        internal int Changes;
        internal CredentialHasher Hasher => new(new(OfflinePasswordBlocklist.Load(Path.Combine(AppContext.BaseDirectory, "Resources", "PasswordBlocklist"))));
        internal void PreparePassword()
        {
            var generator = new TemporaryPasswordGenerator(new(OfflinePasswordBlocklist.Load(Path.Combine(AppContext.BaseDirectory, "Resources", "PasswordBlocklist"))));
            CurrentSecret = "  " + generator.Generate() + "😀  "; NewSecret = generator.Generate();
            Account.PasswordHash = Hasher.Hash(CurrentSecret);
            OtherOwn = new() { Sid = RandomNumberGenerator.GetBytes(32), AccountId = 42, SecurityVersion = 1, SessionScope = Session.SessionScope, IssuedAt = Session.IssuedAt, ExpiresAt = Session.ExpiresAt, LastActivityAt = Session.LastActivityAt };
        }
        public async Task ExecuteAsync(VerifiedSessionToken token, PasswordChangeInput input, CancellationToken ct)
        {
            Changes++; var repository = new Repository(this);
            var deadline = await new PasswordChangeService(repository, Hasher, Clock).ChangeAsync(token, input, ct);
            Started.TrySetResult(); if (PauseCommit) await Commit.Task.WaitAsync(ct);
            if (FailCommit) throw new InvalidOperationException("Synthetic commit failure.");
            if (!Clock.IsBefore(deadline)) throw new ApiErrorException(ApiErrorCode.AUTHENTICATION_REQUIRED);
            Account.PasswordHash = repository.Account!.PasswordHash; Account.SecurityVersion = repository.Account.SecurityVersion;
            Account.MustChangePassword = false; Account.TempIssuedAt = null; Account.TempExpiresAt = null;
            Session.RevokedAt = repository.Session!.RevokedAt; OtherOwn!.RevokedAt = repository.Other!.RevokedAt;
            LastAudit = repository.Pending;
        }
        internal AuditEntry? LastAudit;
        internal readonly TaskCompletionSource Started = new(TaskCreationOptions.RunContinuationsAsynchronously);
        internal readonly TaskCompletionSource Commit = new(TaskCreationOptions.RunContinuationsAsynchronously);
        internal State(string role = "Teacher", bool restricted = false)
        {
            Role = role; var now = Clock.GetUtcNow(); var key = new SymmetricSecurityKey(RandomNumberGenerator.GetBytes(32));
            Validator = new([key], [SecurityAlgorithms.HmacSha256], "fixture", "fixture", Clock, 4096);
            Account = new() { AccountId = 42, RoleId = 1, FullName = "Synthetic account", Email = "account@example.invalid", Status = "ACTIVE", SecurityVersion = 1, RowVersion = new byte[8], MustChangePassword = restricted, TempIssuedAt = restricted ? now.AddMinutes(20).AddHours(-24).UtcDateTime : null, TempExpiresAt = restricted ? now.AddMinutes(20).UtcDateTime : null };
            var window = restricted ? SessionIssuanceWindow.Restricted(now, now.AddMinutes(20)) : SessionIssuanceWindow.Normal(now);
            Session = new() { Sid = RandomNumberGenerator.GetBytes(32), AccountId = 42, SecurityVersion = 1, SessionScope = window.Scope, IssuedAt = window.IssuedAt.UtcDateTime, ExpiresAt = window.AbsoluteExpiresAt.UtcDateTime, LastActivityAt = window.IssuedAt.UtcDateTime, RowVersion = new byte[8] };
            Encoded = new JwtSessionIssuer(new(key, "HS256"), "fixture", "fixture").Issue(42, 1, Session.Sid, window);
        }
        public Task<CurrentSessionSnapshot> ReadAsync(VerifiedSessionToken token, bool normal, CancellationToken ct)
        {
            Reads++; if (RevokeOnSecondRead && Reads == 2) Session.RevokedAt = Clock.GetUtcNow().UtcDateTime;
            return new CurrentSessionService(new Repository(this), Clock).ReadAsync(token, normal, ct);
        }
        public async Task ExecuteAsync(VerifiedSessionToken token, CancellationToken ct)
        {
            Revokes++; var repository = new Repository(this);
            var proof = await new CurrentSessionService(repository, Clock).RevokeCurrentAsync(token, ct);
            Started.TrySetResult(); if (PauseCommit) await Commit.Task.WaitAsync(ct);
            if (FailCommit) throw new InvalidOperationException("Synthetic commit failure.");
            if (!Clock.IsBefore(proof.ValidUntil)) throw new ApiErrorException(ApiErrorCode.AUTHENTICATION_REQUIRED);
            Session.RevokedAt = repository.Session!.RevokedAt;
            await LogoutAuditCompletion.AfterCommitAsync(proof.VerifiedActor, new(this, Incidents), Clock, ct);
        }
        public Task AppendAsync(AuditEntry entry, CancellationToken ct)
        {
            AuditAttempts++; LastAudit = entry;
            return AuditFails || AuditCanceled ? Task.FromException(AuditCanceled ? new OperationCanceledException() : new InvalidOperationException("Synthetic audit failure.")) : Task.CompletedTask;
        }
    }
    private sealed class Repository(State state) : IPasswordChangeRepository
    {
        internal AuthSession? Session, Other;
        internal Account? Account;
        internal AuditEntry? Pending;
        public Task<Account?> LockAccountAsync(int id, CancellationToken ct) => Task.FromResult<Account?>(id == 42 ? (Account ??= new() { AccountId = 42, RoleId = 1, PasswordHash = state.Account.PasswordHash, FullName = state.Account.FullName, Email = state.Account.Email, Status = state.Account.Status, SecurityVersion = state.Account.SecurityVersion, MustChangePassword = state.Account.MustChangePassword, TempIssuedAt = state.Account.TempIssuedAt, TempExpiresAt = state.Account.TempExpiresAt, RowVersion = state.Account.RowVersion.ToArray() }) : null);
        public Task<AuthSession?> LockSessionAsync(byte[] sid, CancellationToken ct)
        {
            if (!sid.SequenceEqual(state.Session.Sid)) return Task.FromResult<AuthSession?>(null);
            if (Session is not null) return Task.FromResult<AuthSession?>(Session);
            var s = state.Session; Session = new() { Sid = s.Sid.ToArray(), AccountId = s.AccountId, SecurityVersion = s.SecurityVersion, SessionScope = s.SessionScope, IssuedAt = s.IssuedAt, ExpiresAt = s.ExpiresAt, LastActivityAt = s.LastActivityAt, RevokedAt = s.RevokedAt, RowVersion = s.RowVersion.ToArray() }; return Task.FromResult<AuthSession?>(Session);
        }
        public async Task<IReadOnlyList<AuthSession>> LockAllSessionsAsync(int id, CancellationToken ct)
        {
            _ = await LockSessionAsync(state.Session.Sid, ct);
            var s = state.OtherOwn!; Other = new() { Sid = s.Sid.ToArray(), AccountId = s.AccountId, SecurityVersion = s.SecurityVersion, SessionScope = s.SessionScope, IssuedAt = s.IssuedAt, ExpiresAt = s.ExpiresAt, LastActivityAt = s.LastActivityAt, RevokedAt = s.RevokedAt };
            return [Session!, Other];
        }
        public Task SaveChangeAsync(AuditEntry entry, CancellationToken ct)
        {
            if (state.MandatoryFails) throw new InvalidOperationException("Synthetic mandatory audit failure.");
            Pending = entry; return Task.CompletedTask;
        }
        public Task<string?> RoleCodeAsync(int id, CancellationToken ct) => Task.FromResult<string?>(state.Role);
        public Task SaveSessionAsync(CancellationToken ct) => Task.CompletedTask;
    }
    private sealed class Legacy : ICsrfIdentityResolver
    {
        public Task<VerifiedCsrfIdentity?> ResolveAsync(HttpContext context, CancellationToken ct) => Task.FromResult<VerifiedCsrfIdentity?>(new(new(new System.Security.Claims.ClaimsIdentity([new(System.Security.Claims.ClaimTypes.NameIdentifier, "42")], "fixture-only")), false));
    }
    private sealed class Mount(State state) : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) => app =>
        {
            next(app); app.UseRouting(); app.UseAuthJsonBoundary(); if (state.Csrf) app.UseCsrfBoundary(Origin, state.LegacyResolver ? new Legacy() : new LiveCsrfIdentityResolver(state.Validator, state, new(state, state.Incidents), state.Clock));
            app.UseEndpoints(e => { e.MapCsrfContext(); e.MapCurrentSession(state, state.Clock); e.MapCurrentSessionLogout(state); e.MapPasswordChange(state, 4096); });
        };
    }
    private static WebApplicationFactory<Program> Factory(State state) => new WebApplicationFactory<Program>().WithWebHostBuilder(b => b.ConfigureServices(s => { s.AddCsrfBoundary(new EphemeralDataProtectionProvider()); s.AddSingleton<IStartupFilter>(new Mount(state)); }));
    private static HttpClient Client(WebApplicationFactory<Program> factory) => factory.CreateClient(new() { BaseAddress = new(Origin), HandleCookies = false, AllowAutoRedirect = false });
    private static HttpRequestMessage Request(State state, HttpMethod method, string path, string? csrfCookie = null, bool auth = true)
    {
        var req = new HttpRequestMessage(method, "/api/v1/auth/" + path);
        var cookie = auth ? SafeCookiePolicy.AuthCookieName + "=" + state.Encoded : "";
        if (csrfCookie is not null) cookie += (cookie.Length > 0 ? "; " : "") + csrfCookie;
        if (cookie.Length > 0) req.Headers.Add("Cookie", cookie);
        return req;
    }
    private static async Task<(string Token, string Cookie)> Csrf(State state, HttpClient client)
    {
        using var req = Request(state, HttpMethod.Get, "csrf"); using var res = await client.SendAsync(req, TestContext.Current.CancellationToken); Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        using var json = JsonDocument.Parse(await res.Content.ReadAsStringAsync(TestContext.Current.CancellationToken)); return (json.RootElement.GetProperty("requestToken").GetString()!, res.Headers.GetValues("Set-Cookie").Single().Split(';')[0]);
    }
    private static async Task<HttpResponseMessage> Logout(State state, HttpClient client, (string Token, string Cookie) csrf, string? body = null, bool origin = true, bool token = true, string path = "logout")
    {
        using var req = Request(state, HttpMethod.Post, path, csrf.Cookie); if (body is not null) req.Content = new StringContent(body, System.Text.Encoding.UTF8, "application/json");
        if (origin) req.Headers.Add("Origin", Origin); if (token) req.Headers.Add("X-CSRF-TOKEN", csrf.Token);
        return await client.SendAsync(req, TestContext.Current.CancellationToken);
    }
    private static bool AuthCookie(HttpResponseMessage res) => res.Headers.TryGetValues("Set-Cookie", out var values) && values.Any(v => v.StartsWith(SafeCookiePolicy.AuthCookieName + "=", StringComparison.Ordinal));
    private static string ChangeBody(State state, string? current = null, string? next = null, string? rv = null) => JsonSerializer.Serialize(new { currentPassword = current ?? state.CurrentSecret, newPassword = next ?? state.NewSecret, expectedVersions = new { account = rv ?? RowVersionCodec.Encode(state.Account.RowVersion) } });
    [Theory]
    [InlineData("Admin", false)]
    [InlineData("Admissions", false)]
    [InlineData("AcademicAffairs", false)]
    [InlineData("Teacher", false)]
    [InlineData("Accountant", false)]
    [InlineData("Director", false)]
    [InlineData("Student", false)]
    [InlineData("Teacher", true)]
    public async Task PasswordChangeCommitsAllSessionsAndMandatoryAuditBefore204(string role, bool restricted)
    {
        var state = new State(role, restricted); state.PreparePassword();
        using var factory = Factory(state); using var client = Client(factory); var csrf = await Csrf(state, client);
        using var response = await Logout(state, client, csrf, ChangeBody(state), path: "change-password");
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode); Assert.True(AuthCookie(response)); Assert.Empty(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        Assert.Equal(2, state.Account.SecurityVersion); Assert.False(state.Account.MustChangePassword); Assert.Null(state.Account.TempExpiresAt);
        Assert.NotNull(state.Session.RevokedAt); Assert.NotNull(state.OtherOwn!.RevokedAt); Assert.Equal(state.Session.IssuedAt, state.Session.LastActivityAt);
        Assert.Equal(PasswordVerificationResult.Failed, state.Hasher.Verify(state.Account.PasswordHash, state.CurrentSecret));
        Assert.NotEqual(PasswordVerificationResult.Failed, state.Hasher.Verify(state.Account.PasswordHash, state.NewSecret));
        Assert.Equal("MANDATORY", state.LastAudit!.Category); Assert.Equal("AUTH.PASSWORD_CHANGE", state.LastAudit.Action);
        Assert.DoesNotContain(state.CurrentSecret!, state.LastAudit.Details!); Assert.DoesNotContain(state.NewSecret!, state.LastAudit.Details!); Assert.DoesNotContain(state.Account.PasswordHash, state.LastAudit.Details!);
        using var old = Request(state, HttpMethod.Get, "me"); using var expired = await client.SendAsync(old, TestContext.Current.CancellationToken); Assert.Equal(HttpStatusCode.Unauthorized, expired.StatusCode);
    }
    [Theory]
    [InlineData("wrong", HttpStatusCode.BadRequest)]
    [InlineData("blocked", HttpStatusCode.BadRequest)]
    [InlineData("stale", HttpStatusCode.Conflict)]
    [InlineData("unknown", HttpStatusCode.BadRequest)]
    [InlineData("duplicate", HttpStatusCode.BadRequest)]
    [InlineData("query", HttpStatusCode.BadRequest)]
    [InlineData("missing", HttpStatusCode.BadRequest)]
    [InlineData("null", HttpStatusCode.BadRequest)]
    [InlineData("origin", HttpStatusCode.Forbidden)]
    [InlineData("csrf", HttpStatusCode.Forbidden)]
    [InlineData("audit", HttpStatusCode.InternalServerError)]
    [InlineData("commit", HttpStatusCode.InternalServerError)]
    public async Task PasswordChangeFailuresDoNotClearCookieOrCommit(string fault, HttpStatusCode expected)
    {
        var state = new State(); state.PreparePassword(); var hash = state.Account.PasswordHash;
        using var factory = Factory(state); using var client = Client(factory); var csrf = await Csrf(state, client);
        var body = ChangeBody(state, fault == "wrong" ? "wrong raw credential" : null, fault == "blocked" ? "short" : null, fault == "stale" ? "AAECAwQFBgc=" : null);
        if (fault == "unknown") body = body[..^1] + ",\"actor\":42}";
        if (fault == "duplicate") body = body[..^1] + ",\"newPassword\":\"duplicate\"}";
        if (fault == "missing") body = "{}";
        if (fault == "null") body = "{\"currentPassword\":null,\"newPassword\":null,\"expectedVersions\":{\"account\":\"AAAAAAAAAAA=\"}}";
        state.MandatoryFails = fault == "audit"; state.FailCommit = fault == "commit";
        using var response = await Logout(state, client, csrf, body, origin: fault != "origin", token: fault != "csrf", path: fault == "query" ? "change-password?x=1" : "change-password");
        Assert.Equal(expected, response.StatusCode); Assert.False(AuthCookie(response)); Assert.Equal(hash, state.Account.PasswordHash); Assert.Equal(1, state.Account.SecurityVersion); Assert.Null(state.Session.RevokedAt);
        var text = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken); Assert.DoesNotContain(state.CurrentSecret!, text); Assert.DoesNotContain(state.NewSecret!, text);
    }
    [Fact]
    public async Task PasswordChangeWaitsForCommitAndDeadlineFailureDoesNotClearCookie()
    {
        var state = new State(restricted: true); state.PreparePassword(); state.PauseCommit = true;
        using var factory = Factory(state); using var client = Client(factory); var csrf = await Csrf(state, client);
        var pending = Logout(state, client, csrf, ChangeBody(state), path: "change-password");
        await state.Started.Task.WaitAsync(TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken); Assert.False(pending.IsCompleted); Assert.Null(state.Session.RevokedAt);
        state.Time.Now = state.Time.Now.AddMinutes(10); state.Commit.TrySetResult(); using var response = await pending;
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode); Assert.True(AuthCookie(response)); Assert.Equal(1, state.Account.SecurityVersion); Assert.Null(state.Session.RevokedAt);
    }
    [Fact]
    public async Task InvalidTemporaryBundleAndExpiredBoundaryNeverReturnOldContext()
    {
        var state = new State(restricted: true); state.Account.TempExpiresAt = state.Account.TempExpiresAt!.Value.AddMinutes(1);
        using var factory = Factory(state); using var client = Client(factory); using var req = Request(state, HttpMethod.Get, "me"); using var response = await client.SendAsync(req, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode); Assert.True(AuthCookie(response)); Assert.Null(state.LastAudit!.ActorAccountId);
    }
    [Fact]
    public async Task IdleExpiredCookieIsDeletedOn401SoDeliberateAnonymousCsrfCanResume()
    {
        var state = new State(); state.Time.Now = state.Time.Now.AddMinutes(30);
        using var factory = Factory(state); using var client = Client(factory); using var request = Request(state, HttpMethod.Get, "me"); using var response = await client.SendAsync(request, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode); Assert.True(AuthCookie(response)); Assert.Equal(0, state.Revokes); Assert.Null(state.Session.RevokedAt);
        using var anonymous = Request(state, HttpMethod.Get, "csrf", auth: false); using var fresh = await client.SendAsync(anonymous, TestContext.Current.CancellationToken); Assert.Equal(HttpStatusCode.OK, fresh.StatusCode); Assert.False(AuthCookie(fresh));
    }
    [Theory]
    [InlineData("Admin")]
    [InlineData("Admissions")]
    [InlineData("AcademicAffairs")]
    [InlineData("Teacher")]
    [InlineData("Accountant")]
    [InlineData("Director")]
    [InlineData("Student")]
    public async Task AllRolesMeReturnsCurrentNineFieldsWithoutRenewalAndLogoutClearsOnlyAfterCommit(string role)
    {
        var state = new State(role); using var factory = Factory(state); using var client = Client(factory); var before = state.Session.LastActivityAt;
        using var req = Request(state, HttpMethod.Get, "me"); using var me = await client.SendAsync(req, TestContext.Current.CancellationToken); Assert.Equal(HttpStatusCode.OK, me.StatusCode); Assert.True(me.Headers.CacheControl!.NoStore); Assert.False(AuthCookie(me));
        using var body = JsonDocument.Parse(await me.Content.ReadAsStringAsync(TestContext.Current.CancellationToken)); Assert.Equal(9, body.RootElement.EnumerateObject().Count()); Assert.Equal(role, body.RootElement.GetProperty("role").GetString()); Assert.False(body.RootElement.TryGetProperty("sid", out _)); Assert.False(body.RootElement.TryGetProperty("token", out _)); Assert.Equal(before, state.Session.LastActivityAt);
        var csrf = await Csrf(state, client); using var logout = await Logout(state, client, csrf); Assert.Equal(HttpStatusCode.NoContent, logout.StatusCode); Assert.Empty(await logout.Content.ReadAsByteArrayAsync(TestContext.Current.CancellationToken)); Assert.NotNull(state.Session.RevokedAt); Assert.Equal(before, state.Session.LastActivityAt);
        var cookie = logout.Headers.GetValues("Set-Cookie").Single(); Assert.True(cookie.StartsWith(SafeCookiePolicy.AuthCookieName + "=;", StringComparison.Ordinal) && cookie.Contains("secure", StringComparison.OrdinalIgnoreCase) && cookie.Contains("httponly", StringComparison.OrdinalIgnoreCase) && cookie.Contains("samesite=strict", StringComparison.OrdinalIgnoreCase) && cookie.Contains("path=/", StringComparison.OrdinalIgnoreCase) && cookie.Contains("expires=", StringComparison.OrdinalIgnoreCase) && !cookie.Contains("domain=", StringComparison.OrdinalIgnoreCase));
        using var second = await Logout(state, client, csrf); Assert.Equal(HttpStatusCode.Unauthorized, second.StatusCode); Assert.Equal(1, state.Revokes);
    }
    [Fact]
    public async Task RestrictedSessionCanReadMinimalMeAndLogoutWithoutScopeUpgradeOrIdleTouch()
    {
        var state = new State(restricted: true); using var factory = Factory(state); using var client = Client(factory); using var req = Request(state, HttpMethod.Get, "me"); using var me = await client.SendAsync(req, TestContext.Current.CancellationToken); Assert.Equal(HttpStatusCode.OK, me.StatusCode);
        using var body = JsonDocument.Parse(await me.Content.ReadAsStringAsync(TestContext.Current.CancellationToken)); Assert.Equal("PASSWORD_CHANGE", body.RootElement.GetProperty("scope").GetString()); var before = state.Session.LastActivityAt;
        using var logout = await Logout(state, client, await Csrf(state, client)); Assert.Equal(HttpStatusCode.NoContent, logout.StatusCode); Assert.Equal(before, state.Session.LastActivityAt); Assert.True(state.Account.MustChangePassword);
    }
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task PostcommitAuditFailureOrCancellationCannotRestoreLogoutOrPreventCookieDeletion(bool canceled)
    {
        var state = new State { AuditFails = !canceled, AuditCanceled = canceled }; using var factory = Factory(state); using var client = Client(factory); using var logout = await Logout(state, client, await Csrf(state, client));
        Assert.Equal(HttpStatusCode.NoContent, logout.StatusCode); Assert.NotNull(state.Session.RevokedAt); Assert.True(AuthCookie(logout)); Assert.Equal(1, state.Incidents.GetFailureCount(SecurityAuditKind.Logout)); Assert.Equal(42, state.LastAudit!.ActorAccountId); Assert.Null(state.LastAudit.Details); Assert.Equal("42", state.LastAudit.EntityId);
    }
    [Fact]
    public async Task PendingCommitDoesNotPublishCookieOrResponseAndFailedCommitKeepsSession()
    {
        var state = new State { PauseCommit = true }; using var factory = Factory(state); using var client = Client(factory); var csrf = await Csrf(state, client); var task = Logout(state, client, csrf); await state.Started.Task.WaitAsync(TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken); Assert.False(task.IsCompleted); Assert.Null(state.Session.RevokedAt); state.FailCommit = true; state.Commit.SetResult(); using var response = await task; Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode); Assert.False(AuthCookie(response)); Assert.Null(state.Session.RevokedAt); Assert.Equal(0, state.AuditAttempts);
    }
    [Theory]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public async Task MissingOriginOrCsrfCannotRevokeEvenWhenAuditStoreFails(bool origin, bool token)
    {
        var state = new State { AuditFails = true }; using var factory = Factory(state); using var client = Client(factory); using var response = await Logout(state, client, await Csrf(state, client), origin: origin, token: token);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode); Assert.Equal(0, state.Revokes); Assert.Null(state.Session.RevokedAt); Assert.False(AuthCookie(response)); Assert.Equal(1, state.Incidents.GetFailureCount(SecurityAuditKind.AccessDenied));
    }
    [Theory]
    [InlineData("{}", "logout")]
    [InlineData(null, "logout?accountId=99")]
    public async Task LogoutRejectsBusinessBodyOrSelectorsAfterVerifiedBoundary(string? body, string path)
    {
        var state = new State(); using var factory = Factory(state); using var client = Client(factory); using var response = await Logout(state, client, await Csrf(state, client), body, path: path); Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode); Assert.Equal(0, state.Revokes); Assert.False(AuthCookie(response));
    }
    [Fact]
    public async Task MissingCookieAndBearerSelectorNeverAuthenticateAndMeRechecksLateRevocation()
    {
        var state = new State { AuditFails = true }; using var factory = Factory(state); using var client = Client(factory); using var req = Request(state, HttpMethod.Get, "me", auth: false); req.Headers.Authorization = new("Bearer", state.Encoded); using var denied = await client.SendAsync(req, TestContext.Current.CancellationToken); Assert.Equal(HttpStatusCode.Unauthorized, denied.StatusCode); Assert.Equal(0, state.Reads); Assert.Null(state.LastAudit!.ActorAccountId);
        state.RevokeOnSecondRead = true; using var valid = Request(state, HttpMethod.Get, "me"); using var late = await client.SendAsync(valid, TestContext.Current.CancellationToken); Assert.Equal(HttpStatusCode.Unauthorized, late.StatusCode); Assert.True(AuthCookie(late));
    }
    [Theory]
    [InlineData(false, false)]
    [InlineData(true, true)]
    public async Task MissingMiddlewareOrCryptographicProofFailsClosedEvenWithClaimedIdentity(bool csrf, bool legacy)
    {
        var state = new State { Csrf = csrf, LegacyResolver = legacy }; using var factory = Factory(state); using var client = Client(factory); using var req = Request(state, HttpMethod.Get, "me"); using var response = await client.SendAsync(req, TestContext.Current.CancellationToken); Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode); Assert.Equal(0, state.Reads);
    }
    [Fact]
    public async Task MeRejectsQueryAndBodyButRepeatedCsrfAndMeNeverTouchIdle()
    {
        var state = new State(); using var factory = Factory(state); using var client = Client(factory); var before = state.Session.LastActivityAt;
        foreach (var path in new[] { "me?accountId=99", "me" })
        { using var req = Request(state, HttpMethod.Get, path); if (path == "me") req.Content = new StringContent("{}"); using var response = await client.SendAsync(req, TestContext.Current.CancellationToken); Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode); }
        await Csrf(state, client); await Csrf(state, client); Assert.Equal(before, state.Session.LastActivityAt);
    }
}
