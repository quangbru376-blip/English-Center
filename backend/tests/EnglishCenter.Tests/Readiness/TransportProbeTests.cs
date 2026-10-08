using System.Net;
using System.Diagnostics;
using System.Text.Json;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using EnglishCenter.Api.Api.Auth;
using EnglishCenter.Api.Api.Runtime;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Infrastructure.Readiness;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EnglishCenter.Tests.Readiness;

public sealed class TransportProbeTests
{
    [Theory]
    [InlineData("valid", 1)] [InlineData("wrong-pin", 0)] [InlineData("wrong-san", 0)]
    [InlineData("expired", 0)] [InlineData("no-eku", 0)]
    public async Task RealTlsChecksPinSanTimeAndServerUsageBeforeSql(string mode, long completed)
    {
        using var rsa = RSA.Create(2048);
        var request = new CertificateRequest("CN=localhost", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        var san = new SubjectAlternativeNameBuilder(); san.AddDnsName(mode == "wrong-san" ? "other.invalid" : "localhost");
        request.CertificateExtensions.Add(san.Build());
        if (mode != "no-eku")
            request.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension(
                new OidCollection { new("1.3.6.1.5.5.7.3.1") }, false));
        var now = DateTimeOffset.UtcNow;
        using var cert = request.CreateSelfSigned(now.AddDays(-2), mode == "expired" ? now.AddDays(-1) : now.AddDays(1));
        var start = new ProcessStartInfo("node") { RedirectStandardInput = true, RedirectStandardOutput = true,
            RedirectStandardError = true, UseShellExecute = false, CreateNoWindow = true };
        start.ArgumentList.Add("-e");
        start.ArgumentList.Add("const tls=require('tls'),rl=require('readline').createInterface({input:process.stdin});rl.once('line',l=>{const v=JSON.parse(l);const s=tls.createServer({cert:v.cert,key:v.key,minVersion:'TLSv1.2'},c=>c.on('error',()=>{}));s.on('tlsClientError',()=>{});s.listen(0,'127.0.0.1',()=>process.stdout.write(s.address().port+'\\n'));});");
        using var server = Process.Start(start) ?? throw new InvalidOperationException("Fixture TLS process unavailable");
        var token = TestContext.Current.CancellationToken;
        var stderr = server.StandardError.ReadToEndAsync(token);
        try
        {
        await server.StandardInput.WriteLineAsync(JsonSerializer.Serialize(new { cert = cert.ExportCertificatePem(), key = rsa.ExportPkcs8PrivateKeyPem() }));
        var announced = await server.StandardOutput.ReadLineAsync(token).AsTask().WaitAsync(TimeSpan.FromSeconds(5), token);
        var port = int.Parse(announced!, System.Globalization.CultureInfo.InvariantCulture);
        var profile = ProfileFixture.Valid with { Web = ProfileFixture.Valid.Web! with
            { Origin = "https://localhost:" + port, ListenPort = port },
            Execution = ProfileFixture.Valid.Execution! with { ReadinessMs = 2500 } };
        using var services = Services();
        var binding = LocalTransportBinding.Install(new ApplicationBuilder(services), profile, new NoIdentity());
        using var lifetime = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        lifetime.CancelAfter(5000);

        var pin = mode == "wrong-pin" ? new string('0', 64) : Convert.ToHexString(SHA256.HashData(cert.RawData));
        var probe = new TransportReadinessProbe(binding, TimeProvider.System, profile.Database!.ConnectionSecretReference!,
            _ => ValueTask.FromResult<LocalTlsTrust?>(new(profile.Web!.CertificateSecretReference!,
                profile.Web.ClientTrustReference!, pin)),
            _ => ValueTask.FromResult<string?>(null));
        try
        {
            // Web TLS can pass; absent real SQL trust still denies the whole category.
            Assert.Equal(ReadinessProbeStatus.NotReady, await probe.CheckAsync(profile, lifetime.Token));
            Assert.Equal(completed, probe.CompletedTlsChecks);
        }
        finally { lifetime.Cancel(); }
        }
        finally { if (!server.HasExited) server.Kill(entireProcessTree: true); await server.WaitForExitAsync(token); await stderr; }
    }

    [Fact]
    public async Task ActualUnsafeAntiforgeryOptionsDenyBeforeSecretResolution()
    {
        using var services = Services(unsafeOptions: true);
        var profile = ProfileFixture.Valid with { Web = ProfileFixture.Valid.Web! with { Origin = "https://localhost" } };
        var binding = LocalTransportBinding.Install(new ApplicationBuilder(services), profile, new NoIdentity());
        var resolutions = 0;
        var probe = new TransportReadinessProbe(binding, TimeProvider.System, profile.Database!.ConnectionSecretReference!,
            _ => { resolutions++; return ValueTask.FromResult<LocalTlsTrust?>(null); },
            _ => { resolutions++; return ValueTask.FromResult<string?>(null); });
        Assert.Equal(ReadinessProbeStatus.NotReady, await probe.CheckAsync(profile, TestContext.Current.CancellationToken));
        Assert.Equal(0, resolutions);
    }

    [Theory]
    [InlineData("http")] [InlineData("host")]
    public async Task InstalledBoundaryRejectsWrongTransportBeforeCaller(string mode)
    {
        using var services = Services();
        var app = new ApplicationBuilder(services);
        LocalTransportBinding.Install(app, ProfileFixture.Valid with { Web = ProfileFixture.Valid.Web! with { Origin = "https://localhost" } }, new NoIdentity());
        var reached = false; app.Run(_ => { reached = true; return Task.CompletedTask; });
        var context = new DefaultHttpContext { RequestServices = services };
        context.Request.Scheme = mode == "http" ? "http" : "https";
        context.Request.Host = new HostString(mode == "host" ? "other.invalid:443" : "localhost");
        var error = await Assert.ThrowsAsync<ApiErrorException>(() => app.Build()(context));
        Assert.Equal(503, error.StatusCode); Assert.False(reached);
    }

    [Fact]
    public void DirectAdapterRejectsDifferentOriginAndListenerPorts()
    {
        using var services = Services();
        var profile = ProfileFixture.Valid with { Web = ProfileFixture.Valid.Web! with { ListenPort = 8443 } };
        Assert.Throws<ArgumentException>(() => LocalTransportBinding.Install(new ApplicationBuilder(services), profile, new NoIdentity()));
    }

    [Fact]
    public async Task MissingBindingAndChangedReferencesDenyBeforeResolution()
    {
        var profile = ProfileFixture.Valid with { Web = ProfileFixture.Valid.Web! with { Origin = "https://localhost" } };
        var calls = 0;
        LocalTlsTrust? Resolve(CancellationToken _) { calls++; return null; }
        var absent = new TransportReadinessProbe(null, TimeProvider.System, profile.Database!.ConnectionSecretReference!,
            ct => ValueTask.FromResult(Resolve(ct)), _ => ValueTask.FromResult<string?>(null));
        Assert.Equal(ReadinessProbeStatus.NotReady, await absent.CheckAsync(profile, TestContext.Current.CancellationToken));
        using var services = Services();
        var binding = LocalTransportBinding.Install(new ApplicationBuilder(services), profile, new NoIdentity());
        var wrongRef = new TransportReadinessProbe(binding, TimeProvider.System, "different-reference",
            ct => ValueTask.FromResult(Resolve(ct)), _ => ValueTask.FromResult<string?>(null));
        Assert.Equal(ReadinessProbeStatus.NotReady, await wrongRef.CheckAsync(profile, TestContext.Current.CancellationToken));
        Assert.Equal(0, calls);
    }
    private static ServiceProvider Services(bool unsafeOptions = false)
    {
        var services = new ServiceCollection(); services.AddLogging();
        services.AddCsrfBoundary(new EphemeralDataProtectionProvider());
        if (unsafeOptions) services.PostConfigure<AntiforgeryOptions>(options => options.Cookie.HttpOnly = false);
        return services.BuildServiceProvider();
    }

    private sealed class NoIdentity : ICsrfIdentityResolver
    {
        public Task<VerifiedCsrfIdentity?> ResolveAsync(HttpContext context, CancellationToken token)
            => Task.FromResult<VerifiedCsrfIdentity?>(null);
    }
}
