namespace EnglishCenter.Tests.Api.Integration;

using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using EnglishCenter.Api.Api.Auth;
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

public static class BrowserSqlGate
{
    public static bool Enabled => IntegrationSqlGate.Enabled && Environment.GetEnvironmentVariable("EC_V01_BROWSER") == "1";
}
[Collection("V01 HTTP SQL")]
public sealed class BrowserSqlHostingTests
{
    [Theory(Skip = "V01-D requires explicit SQL/browser opt-in and temporary assets.", SkipUnless = nameof(BrowserSqlGate.Enabled), SkipType = typeof(BrowserSqlGate))]
    [InlineData(1)][InlineData(2)][InlineData(3)][InlineData(4)][InlineData(5)]
    [InlineData(6)][InlineData(7)][InlineData(8)][InlineData(9)][InlineData(10)]
    public async Task RealBrowserHttpsToCallerTransaction(int group)
    {
        var ct = IntegrationSqlHost.Ct;
        var assets = Environment.GetEnvironmentVariable("EC_V01_ASSETS") ?? throw new InvalidOperationException("Temporary assets required.");
        var driver = Environment.GetEnvironmentVariable("EC_V01_DRIVER") ?? throw new InvalidOperationException("Browser driver required.");
        Assert.StartsWith(Path.GetFullPath(Path.GetTempPath()), Path.GetFullPath(assets), StringComparison.OrdinalIgnoreCase);
        await using var f = await IntegrationSqlHost.CreateAsync(mountHttp: false);
        if (group == 6) await f.Restricted("Teacher");
        var teacherBefore = await f.Db.Accounts.AsNoTracking().SingleAsync(a => a.AccountId == f.Accounts["Teacher"], ct);
        using var rsa = RSA.Create(2048);
        var request = new CertificateRequest("CN=V01-D isolated fixture", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
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
        f.ConfigureRoutes(app, origin);
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
            f.Db.ChangeTracker.Clear();
            if (group == 2) Assert.Equal(7, await f.Db.AuthSessions.CountAsync(s => s.RevokedAt == null, ct));
            if (group == 4)
            {
                Assert.Equal(2, await f.Db.AuthSessions.CountAsync(ct));
                Assert.Equal(1, await f.Db.AuthSessions.CountAsync(s => s.RevokedAt == null, ct));
            }
            if (group == 1)
            {
                Assert.Equal(10, await f.Db.Accounts.CountAsync(ct)); Assert.Equal(4, await f.Db.StudentProfiles.CountAsync(ct));
                Assert.Equal(3, await f.Db.AuditLogs.CountAsync(ct)); Assert.Equal(0, await f.Db.AuthSessions.CountAsync(ct));
            }
            if (group is 3 or 7 or 10) Assert.Equal(0, await f.Db.AuditLogs.CountAsync(ct));
            if (group is 5 or 6)
            {
                var after = await f.Db.Accounts.AsNoTracking().SingleAsync(a => a.AccountId == teacherBefore.AccountId, ct);
                Assert.Equal(teacherBefore.SecurityVersion + 1, after.SecurityVersion); Assert.False(after.MustChangePassword); Assert.Null(after.TempExpiresAt);
                Assert.True(f.Hasher.Verify(after.PasswordHash, f.Password + "Z") != Microsoft.AspNetCore.Identity.PasswordVerificationResult.Failed);
                Assert.Equal(1, await f.Db.AuditLogs.CountAsync(ct));
                Assert.Equal(1, await f.Db.AuthSessions.CountAsync(s => s.AccountId == after.AccountId && s.RevokedAt == null, ct));
            }
            if (group is 3 or 7)
            {
                var after = await f.Db.Accounts.AsNoTracking().SingleAsync(a => a.AccountId == teacherBefore.AccountId, ct);
                Assert.True(teacherBefore.PasswordHash == after.PasswordHash); Assert.Equal(teacherBefore.SecurityVersion, after.SecurityVersion);
                Assert.Equal(1, await f.Db.AuthSessions.CountAsync(s => s.RevokedAt == null, ct));
            }
            if (group is 8 or 9)
            {
                var after = await f.Db.Accounts.AsNoTracking().SingleAsync(a => a.AccountId == teacherBefore.AccountId, ct);
                Assert.Equal("LOCKED", after.Status); Assert.Equal(2, after.SecurityVersion);
                Assert.Equal(0, await f.Db.AuthSessions.CountAsync(s => s.AccountId == after.AccountId && s.RevokedAt == null, ct));
                Assert.Equal(1, await f.Db.AuditLogs.CountAsync(ct)); Assert.Equal(1, await f.Db.ReplayOperations.CountAsync(ct));
            }
        }
        finally { await app.StopAsync(CancellationToken.None); }
    }
}
