using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using EnglishCenter.Api.Infrastructure.Readiness;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace EnglishCenter.Tests.Readiness;

public static class M0CustodyGate { public static bool NonRuntime => M0SqlTlsGate.Maintenance || M0SqlTlsGate.Schema; }

// Read-only actual runtime stores; no HTTP/SQL/business effects or key regeneration.
public sealed class ActualM0RuntimeCustodyTests
{
    private const string Root = @"C:\ProgramData\EnglishCenter\M0Runtime";
    private static RuntimeProfile Profile => ProfileFixture.Valid with
    {
        Auth = new(["m0-jwt-v1"], "english-center-m0-local", "english-center-m0-local-ui", "m0-csrf-current-user"),
        Execution = new(3000, 10, 30000, 25000, 5000, 262144, 5000000, 10000)
    };

    [Fact(Skip = "Actual M0 runtime custody opt-in", SkipUnless = nameof(M0SqlTlsGate.Runtime), SkipType = typeof(M0SqlTlsGate))]
    public async Task PersistedDpapiJwtSignsAndValidatesAfterProvisioningProcessExit()
    {
        if (!OperatingSystem.IsWindows()) throw new PlatformNotSupportedException();
        using var metadata = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(Root, "key-manifest.json")));
        var descriptor = metadata.RootElement;
        var buffers = new List<byte[]>();
        try
        {
            var probe = new JwtKeyReadinessProbe(TimeProvider.System, _ =>
            {
                if (!OperatingSystem.IsWindows()) return ValueTask.FromResult<JwtReadinessMaterial?>(null);
                var keyBytes = ProtectedData.Unprotect(File.ReadAllBytes(Path.Combine(Root, "jwt.dpapi")),
                    Encoding.UTF8.GetBytes("EnglishCenter.M0.Jwt.v1"), DataProtectionScope.CurrentUser);
                buffers.Add(keyBytes);
                Assert.Equal(32, keyBytes.Length);
                var key = new SymmetricSecurityKey(keyBytes)
                { CryptoProviderFactory = new CryptoProviderFactory { CacheSignatureProviders = false } };
                return ValueTask.FromResult<JwtReadinessMaterial?>(new("m0-jwt-v1", new(key, "HS256"),
                    Profile.Auth!.Issuer!, Profile.Auth.Audience!,
                    [new("m0-jwt-v1", key, "HS256", descriptor.GetProperty("jwtValidFrom").GetDateTimeOffset(),
                        descriptor.GetProperty("jwtValidUntil").GetDateTimeOffset())]));
            });
            Assert.Equal(ReadinessProbeStatus.Pass, await probe.CheckAsync(Profile, TestContext.Current.CancellationToken));
            Assert.Equal(ReadinessProbeStatus.Pass, await probe.CheckAsync(Profile, TestContext.Current.CancellationToken));
            Assert.Equal(2, buffers.Count);
        }
        finally { foreach (var buffer in buffers) CryptographicOperations.ZeroMemory(buffer); }
    }

    [Fact(Skip = "Actual M0 runtime custody opt-in", SkipUnless = nameof(M0SqlTlsGate.Runtime), SkipType = typeof(M0SqlTlsGate))]
    public async Task PersistedCsrfKeysDecryptReadOnlyAfterProvisioningProcessExit()
    {
        var directory = Path.Combine(Root, "csrf");
        var before = Digest(directory);
        var probe = new ProtectedCsrfReadinessProbe("m0-csrf-current-user", directory, 16, 65536, TimeProvider.System);
        Assert.Equal(ReadinessProbeStatus.Pass, await probe.CheckAsync(Profile, TestContext.Current.CancellationToken));
        Assert.Equal(ReadinessProbeStatus.Pass, await probe.CheckAsync(Profile, TestContext.Current.CancellationToken));
        Assert.Equal(before, Digest(directory));
        Assert.Single(Directory.GetFiles(directory, "key-*.xml"));
    }

    [Fact(Skip = "Actual M0 runtime custody opt-in", SkipUnless = nameof(M0SqlTlsGate.Runtime), SkipType = typeof(M0SqlTlsGate))]
    public void PersistedNonExportableHttpsKeyUsableAndPublicLeafTrusted()
    {
        if (!OperatingSystem.IsWindows()) throw new PlatformNotSupportedException();
        using var metadata = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(Root, "key-manifest.json")));
        var thumbprint = metadata.RootElement.GetProperty("httpsThumbprint").GetString()!;
        using var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
        store.Open(OpenFlags.ReadOnly);
        var certificates = store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, validOnly: false);
        Assert.Single(certificates.Cast<X509Certificate2>());
        using var certificate = certificates[0];
        Assert.True(certificate.HasPrivateKey);
        Assert.True(certificate.MatchesHostname("localhost", allowWildcards: false, allowCommonName: false));
        using var key = certificate.GetRSAPrivateKey();
        Assert.IsType<RSACng>(key);
        Assert.Equal(CngExportPolicies.None, ((RSACng)key!).Key.ExportPolicy);
        var challenge = RandomNumberGenerator.GetBytes(32);
        var signature = key!.SignData(challenge, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        using var publicKey = certificate.GetRSAPublicKey();
        Assert.True(publicKey!.VerifyData(challenge, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1));
        using var chain = new X509Chain();
        chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
        chain.ChainPolicy.DisableCertificateDownloads = true;
        Assert.True(chain.Build(certificate));
        CryptographicOperations.ZeroMemory(challenge);
    }

    [Fact(Skip = "Actual M0 maintenance separation opt-in", SkipUnless = nameof(M0CustodyGate.NonRuntime), SkipType = typeof(M0CustodyGate))]
    public void NonRuntimeSqlIdentitiesCannotReadRuntimeJwtOrCsrfStores()
    {
        Assert.Throws<UnauthorizedAccessException>(() => File.ReadAllBytes(Path.Combine(Root, "jwt.dpapi")));
        Assert.Throws<UnauthorizedAccessException>(() => Directory.GetFiles(Path.Combine(Root, "csrf")));
    }

    private static string Digest(string directory) => string.Join("|", Directory.GetFiles(directory).Order()
        .Select(path => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)))));
}
