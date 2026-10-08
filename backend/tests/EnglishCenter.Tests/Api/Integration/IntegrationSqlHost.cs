namespace EnglishCenter.Tests.Api.Integration;

using System.Data;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using EnglishCenter.Api.Api.Administration;
using EnglishCenter.Api.Api.Auth;
using EnglishCenter.Api.Api.Identity;
using EnglishCenter.Api.Common.Auditing;
using EnglishCenter.Api.Common.Replay;
using EnglishCenter.Api.Common.Security;
using EnglishCenter.Api.Common.Time;
using EnglishCenter.Api.Domain.Identity;
using EnglishCenter.Api.Infrastructure;
using EnglishCenter.Api.Infrastructure.Administration;
using EnglishCenter.Api.Infrastructure.Auditing;
using EnglishCenter.Api.Infrastructure.Auth;
using EnglishCenter.Api.Infrastructure.Identity;
using EnglishCenter.Api.Infrastructure.Replay;
using EnglishCenter.Api.Services.Administration;
using EnglishCenter.Api.Services.Auth;
using EnglishCenter.Api.Services.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Xunit;

// The outer rollback transaction replaces owned COMMIT executors only in this fixture.
internal sealed class IntegrationSqlHost : IAsyncDisposable, IStudentSignupExecutor, ILoginExecutor,
    ICurrentSessionReader, ICurrentSessionLogout, IPasswordChangeExecutor, IAdminAccountExecutor,
    ISecurityAuditStore, ITrustedLoginIpResolver
{
    internal const string Origin = "https://localhost";
    internal static CancellationToken Ct => TestContext.Current.CancellationToken;
    internal EnglishCenterDbContext Db { get; } = new(new DbContextOptionsBuilder<EnglishCenterDbContext>()
        .UseSqlServer("Server=lpc:.\\SQLEXPRESS;Database=EnglishCenter_D07_20261005;Integrated Security=True;Encrypt=True;TrustServerCertificate=True", s => s.UseCompatibilityLevel(160)).Options);
    private IDbContextTransaction? _tx;
    private WebApplicationFactory<Program>? _factory;
    private readonly SemaphoreSlim _serial = new(1, 1);
    private readonly TestClock _time = new();
    internal BusinessClock Clock { get; }
    internal CredentialHasher Hasher { get; }
    internal string Password { get; }
    internal JwtSessionValidator Validator { get; }
    private readonly IJwtSessionIssuer _signer;
    private readonly string _dummyHash;
    internal readonly LoginIpLimiter Limiter = new();
    internal readonly AuditIncidentCounter Incidents = new();
    internal bool FailAfterWrite;
    internal readonly Dictionary<string, int> Accounts = new(StringComparer.Ordinal);
    private sealed class TestClock : TimeProvider
    {
        internal DateTimeOffset Now = DateTimeOffset.FromUnixTimeSeconds(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        public override DateTimeOffset GetUtcNow() => Now;
    }
    private IntegrationSqlHost()
    {
        Clock = new(_time);
        var policy = new PasswordPolicy(OfflinePasswordBlocklist.Load(Path.Combine(AppContext.BaseDirectory, "Resources", "PasswordBlocklist")));
        Hasher = new(policy); Password = new TemporaryPasswordGenerator(policy).Generate();
        _dummyHash = Hasher.Hash(new TemporaryPasswordGenerator(policy).Generate());
        var key = new SymmetricSecurityKey(RandomNumberGenerator.GetBytes(32));
        _signer = new JwtSessionIssuer(new(key, "HS256"), "v01-fixture", "v01-fixture");
        Validator = new([key], ["HS256"], "v01-fixture", "v01-fixture", Clock, 4096);
    }
    internal static async Task<IntegrationSqlHost> CreateAsync(bool mountHttp = true)
    {
        if (!IntegrationSqlGate.Enabled) throw new InvalidOperationException("SQL fixture requires explicit opt-in.");
        var f = new IntegrationSqlHost();
        try
        {
            await f.Db.Database.OpenConnectionAsync(Ct);
            Assert.Equal("EnglishCenter_D07_20261005", f.Db.Database.GetDbConnection().Database);
            f._tx = await f.Db.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, Ct);
            var hash = f.Hasher.Hash(f.Password);
            foreach (var code in FixedRoleCodes.All)
            {
                var role = new Role { RoleCode = code, RoleName = code }; f.Db.Roles.Add(role); await f.Db.SaveChangesAsync(Ct);
                var account = new Account { RoleId = role.RoleId, FullName = "Synthetic " + code, Email = code.ToLowerInvariant() + "@example.invalid",
                    PasswordHash = hash, Status = "ACTIVE", SecurityVersion = 1, CreatedAt = f.Clock.GetUtcNow().UtcDateTime };
                f.Db.Accounts.Add(account); await f.Db.SaveChangesAsync(Ct); f.Accounts.Add(code, account.AccountId);
            }
            f.Db.StudentProfiles.Add(new StudentProfile { AccountId = f.Accounts["Student"], DateOfBirth = new DateOnly(2000, 1, 1) }); await f.Db.SaveChangesAsync(Ct);
            f.Db.ChangeTracker.Clear();
            if (mountHttp) f._factory = new WebApplicationFactory<Program>().WithWebHostBuilder(b => b.UseContentRoot(AppContext.BaseDirectory).ConfigureServices(s =>
            { s.AddCsrfBoundary(new EphemeralDataProtectionProvider()); s.AddSingleton<IStartupFilter>(new Mount(f)); }));
            return f;
        }
        catch { await f.DisposeAsync(); throw; }
    }
    internal void ConfigureRoutes(IApplicationBuilder app, string origin)
    {
        app.UseRouting(); app.UseAuthJsonBoundary();
        app.UseCsrfBoundary(origin, new LiveCsrfIdentityResolver(Validator, this, new(this, Incidents), Clock));
        app.UseEndpoints(e => { e.MapCsrfContext(); e.MapStudentSignup(this, 4096); e.MapLogin(this, Limiter, this, new(this, Incidents), Clock, 4096);
            e.MapCurrentSession(this, Clock); e.MapCurrentSessionLogout(this); e.MapPasswordChange(this, 4096); e.MapAdminAccountPilot(this, 4096); });
    }
    private sealed class Mount(IntegrationSqlHost f) : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) => app =>
        { next(app); f.ConfigureRoutes(app, Origin); };
    }
    internal void Advance(TimeSpan elapsed) => _time.Now += elapsed;
    internal HttpClient Client() => _factory!.CreateClient(new() { BaseAddress = new(Origin), HandleCookies = true, AllowAutoRedirect = false });
    internal async Task<string> Csrf(HttpClient client)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/auth/csrf"); request.Headers.Add("Origin", Origin);
        using var response = await client.SendAsync(request, Ct); Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync(Ct)); return body.RootElement.GetProperty("requestToken").GetString()!;
    }
    internal async Task<HttpResponseMessage> Send(HttpClient client, HttpMethod method, string path, object? body = null, string? csrf = null, Guid? key = null)
    {
        using var request = new HttpRequestMessage(method, path); request.Headers.Add("Origin", Origin);
        if (csrf is not null) request.Headers.Add("X-CSRF-Token", csrf);
        if (key is not null) request.Headers.Add("Idempotency-Key", key.Value.ToString("D"));
        if (body is not null) request.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
        return await client.SendAsync(request, Ct);
    }
    internal async Task<JsonElement> Login(HttpClient client, string role)
    {
        using var response = await Send(client, HttpMethod.Post, "/api/v1/auth/login", new { email = role.ToLowerInvariant() + "@example.invalid", password = Password }, await Csrf(client));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync(Ct)); return json.RootElement.Clone();
    }
    internal static async Task<JsonElement> Json(HttpResponseMessage response)
    { using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync(Ct)); return body.RootElement.Clone(); }
    private async Task<T> Operation<T>(Func<SqlAuthDeadlineScope, Task<T>> operation, CancellationToken ct, bool write = false)
    {
        await _serial.WaitAsync(ct);
        try
        {
            Db.ChangeTracker.Clear(); await _tx!.CreateSavepointAsync("V01Operation", ct);
            try
            {
                await using var scope = await SqlAuthDeadlineScope.EnterAsync(Db, 10, 1000, ct);
                var result = await operation(scope);
                if (write && FailAfterWrite) throw new MandatoryAuditException();
                await _tx.ReleaseSavepointAsync("V01Operation", ct); return result;
            }
            catch { await _tx.RollbackToSavepointAsync("V01Operation", CancellationToken.None); Db.ChangeTracker.Clear(); throw; }
        }
        finally { _serial.Release(); }
    }
    public Task<RegisterStudentResult> ExecuteAsync(StudentSignupInput input, CancellationToken ct) =>
        Operation(s => new StudentSignupService(new SqlStudentSignupRepository(s), Hasher, Clock).RegisterAsync(input, ct), ct, true);
    public Task<LoginDecision> ExecuteAsync(LoginInput input, CancellationToken ct) =>
        Operation(s => new LoginService(new SqlLoginRepository(s), Hasher, Clock, _signer, _dummyHash).LoginAsync(input, ct), ct, true);
    public Task<CurrentSessionSnapshot> ReadAsync(VerifiedSessionToken token, bool normal, CancellationToken ct) =>
        Operation(s => new CurrentSessionService(new SqlCurrentSessionRepository(s), Clock).ReadAsync(token, normal, ct), ct);
    async Task ICurrentSessionLogout.ExecuteAsync(VerifiedSessionToken token, CancellationToken ct) =>
        _ = await Operation(s => new CurrentSessionService(new SqlCurrentSessionRepository(s), Clock).RevokeCurrentAsync(token, ct), ct, true);
    public async Task ExecuteAsync(VerifiedSessionToken token, PasswordChangeInput input, CancellationToken ct) =>
        _ = await Operation(s => new PasswordChangeService(new SqlPasswordChangeRepository(s, new()), Hasher, Clock).ChangeAsync(token, input, ct), ct, true);
    private AdminAccountService Admin(SqlAuthDeadlineScope s) => new(new SqlAdminAccountRepository(s), new SqlReplayRepository(s, 1000), Clock);
    public Task<AccountAdminView> DetailAsync(VerifiedSessionToken token, int target, CancellationToken ct) => Operation(s => Admin(s).DetailAsync(token, target, ct), ct);
    public Task<AdminAccountCommandResult> LockAsync(VerifiedSessionToken token, AdminLockInput input, CancellationToken ct) => Operation(s => Admin(s).LockAsync(token, input, ct), ct, true);
    public Task<SafeCommandCompletion> LookupAsync(VerifiedSessionToken token, string action, Guid request, CancellationToken ct) => Operation(s => Admin(s).LookupAsync(token, action, request, ct), ct);
    public Task AppendAsync(AuditEntry entry, CancellationToken ct) => Task.CompletedTask;
    public IPAddress? Resolve(Microsoft.AspNetCore.Http.HttpContext context) => IPAddress.Loopback;
    internal async Task Restricted(string role)
    {
        var account = await Db.Accounts.SingleAsync(a => a.AccountId == Accounts[role], Ct);
        account.MustChangePassword = true; account.TempIssuedAt = Clock.GetUtcNow().UtcDateTime; account.TempExpiresAt = Clock.GetUtcNow().AddHours(24).UtcDateTime;
        await Db.SaveChangesAsync(Ct); Db.ChangeTracker.Clear();
    }
    public async ValueTask DisposeAsync()
    {
        if (_factory is not null) await _factory.DisposeAsync();
        try { if (_tx is not null) await _tx.RollbackAsync(CancellationToken.None); }
        finally { if (_tx is not null) await _tx.DisposeAsync(); await Db.DisposeAsync(); _serial.Dispose(); }
    }
}
