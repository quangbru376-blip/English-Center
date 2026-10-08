namespace EnglishCenter.Tests.Api.Integration;

using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using EnglishCenter.Api.Api.Auth;
using EnglishCenter.Api.Api.Identity;
using EnglishCenter.Api.Common.Errors;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Xunit;

public static class OwnedBrowserSqlGate
{
    public static bool Enabled => OwnedSqlGate.Enabled && Environment.GetEnvironmentVariable("EC_V01_BROWSER") == "1";
}
[Collection("V01 HTTP SQL")]
public sealed class OwnedBrowserSqlHostingTests
{
    [Theory(Skip = "V01-F requires SQL/owned/browser gates and temporary assets.", SkipUnless = nameof(OwnedBrowserSqlGate.Enabled), SkipType = typeof(OwnedBrowserSqlGate))]
    [InlineData(1)][InlineData(2)][InlineData(4)][InlineData(5)][InlineData(6)][InlineData(9)]
    public async Task RealBrowserHttpsToOwnedCommit(int group)
    {
        var ct = OwnedSqlFixture.Ct;
        var assets = Environment.GetEnvironmentVariable("EC_V01_ASSETS") ?? throw new InvalidOperationException("Temporary assets required.");
        var driver = Environment.GetEnvironmentVariable("EC_V01_DRIVER") ?? throw new InvalidOperationException("Browser driver required.");
        Assert.StartsWith(Path.GetFullPath(Path.GetTempPath()), Path.GetFullPath(assets), StringComparison.OrdinalIgnoreCase);
        await using var f = await OwnedSqlFixture.CreateAsync();
        if (group == 6) await f.Restricted("Teacher");
        await using var db = f.Db();
        var teacherBefore = await db.Accounts.AsNoTracking().SingleAsync(a => a.AccountId == f.Accounts["Teacher"], ct);
        using var rsa = RSA.Create(2048);
        var request = new CertificateRequest("CN=V01-F isolated fixture", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        var san = new SubjectAlternativeNameBuilder(); san.AddIpAddress(IPAddress.Loopback); request.CertificateExtensions.Add(san.Build());
        request.CertificateExtensions.Add(new X509BasicConstraintsExtension(false, false, 0, true));
        request.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.KeyEncipherment, true));
        using var certificate = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow.AddMinutes(20));
        var pin = Convert.ToBase64String(SHA256.HashData(rsa.ExportSubjectPublicKeyInfo()));
        var fingerprint = Convert.ToHexString(SHA256.HashData(certificate.RawData));
        // Reserve an OS-selected loopback port; any intervening bind race fails startup.
        var reservation = new TcpListener(IPAddress.Loopback, 0); reservation.Start();
        var port = ((IPEndPoint)reservation.LocalEndpoint).Port; reservation.Stop();
        var origin = $"https://127.0.0.1:{port}";
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { ContentRootPath = AppContext.BaseDirectory });
        builder.Logging.ClearProviders();
        builder.WebHost.ConfigureKestrel(o => o.Listen(IPAddress.Loopback, 0));
        builder.Services.AddCsrfBoundary(new EphemeralDataProtectionProvider());
        await using var app = builder.Build();
        var bridge = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        // The test-only TLS terminator strips client forwarding headers and supplies a RAM nonce.
        app.Use(async (context, next) =>
        {
            var supplied = context.Request.Headers["X-V01-Bridge"].ToString();
            if (!IPAddress.IsLoopback(context.Connection.RemoteIpAddress!) || supplied.Length != bridge.Length ||
                !CryptographicOperations.FixedTimeEquals(System.Text.Encoding.ASCII.GetBytes(supplied), System.Text.Encoding.ASCII.GetBytes(bridge)))
            { context.Response.StatusCode = 400; return; }
            context.Request.Headers.Remove("X-V01-Bridge"); context.Request.Scheme = "https";
            await next(context);
        });
        app.UseApiErrorMiddleware();
        app.UseStaticFiles(new StaticFileOptions { FileProvider = new PhysicalFileProvider(assets) });
        app.Use(async (context, next) =>
        {
            if (!context.Request.Path.StartsWithSegments("/api/v1") && !Path.HasExtension(context.Request.Path.Value))
            { context.Response.ContentType = "text/html"; await context.Response.SendFileAsync(Path.Combine(assets, "tests/browser/identity-entry.html"), ct); }
            else await next(context);
        });
        ConfigureRoutes(f, app, origin);
        await app.StartAsync(ct);
        try
        {
            var start = new ProcessStartInfo("node") { RedirectStandardInput = true, RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false, CreateNoWindow = true };
            start.ArgumentList.Add(driver);
            using var browser = Process.Start(start) ?? throw new InvalidOperationException("Browser process unavailable.");
            var stdout = browser.StandardOutput.ReadToEndAsync(ct); var stderr = browser.StandardError.ReadToEndAsync(ct);
            await browser.StandardInput.WriteLineAsync(JsonSerializer.Serialize(new { group, origin, pin, fingerprint, certificate = certificate.ExportCertificatePem(), privateKey = rsa.ExportPkcs8PrivateKeyPem(), bridge, backend = app.Urls.Single(), password = f.Password, accounts = f.Accounts }));
            browser.StandardInput.Close();
            try { using var deadline = CancellationTokenSource.CreateLinkedTokenSource(ct); deadline.CancelAfter(TimeSpan.FromMinutes(3)); await browser.WaitForExitAsync(deadline.Token); }
            finally { if (!browser.HasExited) { browser.Kill(entireProcessTree: true); await browser.WaitForExitAsync(CancellationToken.None); } }
            // Driver diagnostics contain fixed checkpoint identifiers only, never browser payloads.
            var output = await stdout; var errors = await stderr;
            var safeError = System.Text.RegularExpressions.Regex.IsMatch(errors, @"\AV01-D failed at [A-Za-z0-9_/?:. =-]+\z") ? errors : "Driver failed; raw diagnostic discarded.";
            Assert.True(browser.ExitCode == 0, "Browser fixture failed: " + safeError);
            using var result = JsonDocument.Parse(output); Assert.Equal(group, result.RootElement.GetProperty("group").GetInt32());
            db.ChangeTracker.Clear();
            Assert.True(result.RootElement.GetProperty("tlsPinned").GetBoolean()); Assert.True(result.RootElement.GetProperty("passed").GetBoolean());
            var security = await db.AuditLogs.Where(a => a.AuditCategory == "SECURITY").ToListAsync(ct);
            Assert.All(security, a => {
                Assert.Contains(a.Action, new[] { "AUTH.LOGIN", "AUTH.LOGOUT", "AUTH.ACCESS_DENIED" });
                Assert.True(a.ActorAccountId is null || f.Accounts.Values.Contains(a.ActorAccountId.Value), "Security actor must be verified fixture identity.");
                OwnedAuditAssertions.Security(a, a.Action, a.ActorAccountId, a.Action != "AUTH.ACCESS_DENIED", f.Clock.GetUtcNow().UtcDateTime);
            });
            if (group == 1)
            {
                Assert.Equal(11, await db.Accounts.CountAsync(ct)); Assert.Equal(4, await db.StudentProfiles.CountAsync(ct));
                Assert.Equal(3, await db.AuditLogs.CountAsync(a => a.AuditCategory == "MANDATORY", ct)); Assert.Equal(0, await db.AuthSessions.CountAsync(ct));
                Assert.True(security.Count >= 3, "Anonymous protected reads must record denials."); Assert.All(security, a => { Assert.Equal("AUTH.ACCESS_DENIED", a.Action); Assert.Null(a.ActorAccountId); });
                Assert.All(await db.AuditLogs.Where(a => a.AuditCategory == "MANDATORY").ToListAsync(ct), a => { Assert.Equal("STUDENT.ACCOUNT.CREATE", a.Action); Assert.Equal("MANDATORY", a.AuditCategory); Assert.Null(a.ActorAccountId); });
            }
            else
            {
                var expectedLogins = group switch { 2 => 7, 4 => 2, 5 or 6 => 3, 9 => 2, _ => throw new InvalidOperationException("Invalid owned browser group.") };
                Assert.Equal(expectedLogins, security.Count(a => a.Action == "AUTH.LOGIN" && a.Result == "SUCCESS"));
                Assert.Equal(0, f.Incidents.GetFailureCount(EnglishCenter.Api.Common.Auditing.SecurityAuditKind.Login));
            }
            if (group == 2) { Assert.Equal(7, await db.AuthSessions.CountAsync(s => s.RevokedAt == null, ct)); Assert.Equal(0, await db.AuditLogs.CountAsync(a => a.AuditCategory == "MANDATORY", ct));
                Assert.Equal(f.Accounts.Where(p => p.Key != "Admin2").Select(p => (int?)p.Value).Order().ToArray(), security.Where(a => a.Action == "AUTH.LOGIN").Select(a => a.ActorAccountId).Order().ToArray()); }
            if (group == 4)
            {
                Assert.Equal(2, await db.AuthSessions.CountAsync(ct)); Assert.Equal(1, await db.AuthSessions.CountAsync(s => s.RevokedAt == null, ct));
                Assert.Equal(1, await db.AuditLogs.CountAsync(a => a.Action == "AUTH.LOGOUT", ct)); Assert.Equal(0, await db.AuditLogs.CountAsync(a => a.AuditCategory == "MANDATORY", ct));
                Assert.Equal(0, f.Incidents.GetFailureCount(EnglishCenter.Api.Common.Auditing.SecurityAuditKind.Logout));
            }
            if (group is 5 or 6)
            {
                var after = await db.Accounts.AsNoTracking().SingleAsync(a => a.AccountId == teacherBefore.AccountId, ct);
                Assert.Equal(teacherBefore.SecurityVersion + 1, after.SecurityVersion); Assert.False(after.MustChangePassword); Assert.Null(after.TempIssuedAt); Assert.Null(after.TempExpiresAt);
                Assert.True(f.Hasher.Verify(after.PasswordHash, f.Password + "Z") != Microsoft.AspNetCore.Identity.PasswordVerificationResult.Failed, "Committed new credential must verify.");
                Assert.True(f.Hasher.Verify(after.PasswordHash, f.Password) == Microsoft.AspNetCore.Identity.PasswordVerificationResult.Failed, "Old credential must fail.");
                Assert.Equal(3, await db.AuthSessions.CountAsync(ct)); Assert.Equal(1, await db.AuthSessions.CountAsync(s => s.AccountId == after.AccountId && s.RevokedAt == null, ct));
                OwnedAuditAssertions.PasswordChange(await db.AuditLogs.SingleAsync(a => a.Action == "AUTH.PASSWORD_CHANGE", ct), after.AccountId, 1, 2, group == 6);
            }
            if (group == 9)
            {
                var after = await db.Accounts.AsNoTracking().SingleAsync(a => a.AccountId == teacherBefore.AccountId, ct);
                Assert.Equal("LOCKED", after.Status); Assert.Equal(2, after.SecurityVersion);
                Assert.Equal(0, await db.AuthSessions.CountAsync(s => s.AccountId == after.AccountId && s.RevokedAt == null, ct));
                Assert.Equal(1, await db.AuthSessions.CountAsync(s => s.AccountId == f.Accounts["Admin"] && s.RevokedAt == null, ct));
                Assert.Equal(1, await db.AuditLogs.CountAsync(a => a.AuditCategory == "MANDATORY", ct));
                OwnedBrowserAssertions.Lock(await db.AuditLogs.SingleAsync(a => a.Action == "ADMIN.ACCOUNT.LOCK", ct),
                    await db.ReplayOperations.SingleAsync(ct), f.Accounts["Admin"], after.AccountId, 1, f.Clock.GetUtcNow().UtcDateTime);
            }
        }
        finally { await app.StopAsync(CancellationToken.None); }
    }
    private static void ConfigureRoutes(OwnedSqlFixture f, WebApplication app, string origin)
    {
        app.UseRouting(); app.UseAuthJsonBoundary();
        app.UseCsrfBoundary(origin, new LiveCsrfIdentityResolver(f.Validator, f.Current, f.Audit, f.Clock));
        Microsoft.AspNetCore.Builder.IApplicationBuilder pipeline = app;
        pipeline.UseEndpoints(e => {
            e.MapCsrfContext(); EnglishCenter.Api.Api.Identity.StudentSignupEndpoint.MapStudentSignup(e, f.Signup, 4096);
            e.MapLogin(f.Login, new EnglishCenter.Api.Common.Security.LoginIpLimiter(), new TrustedIp(), f.Audit, f.Clock, 4096);
            e.MapCurrentSession(f.Current, f.Clock); e.MapCurrentSessionLogout(f.Logout); e.MapPasswordChange(f.Change, 4096);
            EnglishCenter.Api.Api.Administration.AdminAccountEndpoints.MapAdminAccountPilot(e, f.Admin, 4096);
        });
    }
    private sealed class TrustedIp : EnglishCenter.Api.Api.Auth.ITrustedLoginIpResolver
    { public IPAddress? Resolve(HttpContext context) => IPAddress.Loopback; }
}
