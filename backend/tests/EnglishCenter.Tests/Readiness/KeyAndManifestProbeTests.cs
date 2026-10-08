using System.Collections.Immutable;
using System.Reflection;
using System.Security.Cryptography;
using EnglishCenter.Api.Infrastructure.Readiness;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace EnglishCenter.Tests.Readiness;

internal sealed class ProbeClock : TimeProvider
{
    internal DateTimeOffset Now = new(2026, 10, 8, 0, 0, 0, TimeSpan.Zero);
    public override DateTimeOffset GetUtcNow() => Now;
}
internal sealed class ProbeFiles : IDisposable
{
    internal readonly string Root = Path.Combine(Path.GetTempPath(), "ec-v01-jkl", Guid.NewGuid().ToString("N"));
    internal ProbeFiles() => Directory.CreateDirectory(Root);
    public void Dispose()
    {
        var prefix = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "ec-v01-jkl")) + Path.DirectorySeparatorChar;
        var full = Path.GetFullPath(Root);
        if (!full.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) ||
            !Guid.TryParseExact(Path.GetFileName(full), "N", out _)) throw new InvalidOperationException("Fixture ownership mismatch");
        Directory.Delete(full, recursive: true);
    }
}

public sealed class KeyAndManifestProbeTests
{
    private readonly ProbeClock clock = new();
    private static Version Loaded(Assembly assembly) => Version.Parse(
        assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion.Split('+')[0]);
    private static readonly string[] Paths = ["backend/global.json",
        "backend/src/EnglishCenter.Api/EnglishCenter.Api.csproj",
        "backend/src/EnglishCenter.Api/packages.lock.json", "frontend/package.json", "frontend/package-lock.json"];
    private RuntimePatchManifest Manifest(ProbeFiles fixture)
    {
        var root = AppContext.BaseDirectory;
        while (!File.Exists(Path.Combine(root, "backend/global.json")))
            root = Directory.GetParent(root)?.FullName ?? throw new InvalidOperationException("Repository unavailable");
        var pins = Paths.Select(relative =>
        {
            var bytes = File.ReadAllBytes(Path.Combine(root, relative));
            var copy = Path.Combine(fixture.Root, relative);
            Directory.CreateDirectory(Path.GetDirectoryName(copy)!); File.WriteAllBytes(copy, bytes);
            return new RuntimeArtifactPin(relative, bytes.LongLength, Convert.ToHexString(SHA256.HashData(bytes)));
        }).ToImmutableArray();
        return new(Environment.Version, Loaded(typeof(WebApplication).Assembly), Loaded(typeof(DbContext).Assembly),
            new Version(17, 0, 1000, 7), new(2026, 10, 8), new(2027, 1, 8), pins);
    }
    private RuntimeManifestReadinessProbe Probe(ProbeFiles files, RuntimePatchManifest manifest,
        Func<CancellationToken, ValueTask<Version?>>? resolve = null) =>
        new("synthetic-patches", files.Root, manifest, clock, "synthetic-connection-reference", resolve ?? (_ => ValueTask.FromResult<Version?>(manifest.Sql)));
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task RealLoadedRuntimeAndCopiedShippedArtifactHashesPassWithSyntheticSqlVersion()
    {
        using var files = new ProbeFiles(); var manifest = Manifest(files);
        Assert.Equal(ReadinessProbeStatus.Pass, await Probe(files, manifest).CheckAsync(ProfileFixture.Valid, Ct));
        Assert.Equal("RuntimePatchManifest", manifest.ToString());
        Assert.Equal("RuntimeArtifactPin", manifest.Artifacts[0].ToString());
    }
    [Theory]
    [InlineData("duplicate")] [InlineData("extra")] [InlineData("missing")] [InlineData("hash")]
    [InlineData("length")] [InlineData("path")] [InlineData("default")] [InlineData("null")]
    [InlineData("version")] [InlineData("nopatch")] [InlineData("future")] [InlineData("overdue")]
    public void InvalidApprovalPolicyFailsClosed(string change)
    {
        using var files = new ProbeFiles(); var manifest = Manifest(files);
        manifest = change switch
        {
            "duplicate" => manifest with { Artifacts = manifest.Artifacts.SetItem(1, manifest.Artifacts[0]) },
            "extra" => manifest with { Artifacts = manifest.Artifacts.Add(manifest.Artifacts[0]) },
            "missing" => manifest with { Artifacts = manifest.Artifacts.RemoveAt(0) },
            "hash" => manifest with { Artifacts = manifest.Artifacts.SetItem(0, manifest.Artifacts[0] with { Sha256 = new string('g', 64) }) },
            "length" => manifest with { Artifacts = manifest.Artifacts.SetItem(0, manifest.Artifacts[0] with { Length = 0 }) },
            "path" => manifest with { Artifacts = manifest.Artifacts.SetItem(0, manifest.Artifacts[0] with { RelativePath = "../outside" }) },
            "default" => manifest with { Artifacts = default },
            "null" => manifest with { Artifacts = manifest.Artifacts.SetItem(0, null!) },
            "version" => manifest with { Ef = new(9, 0, 1) },
            "nopatch" => manifest with { Sql = new(17, 0) },
            "future" => manifest with { Reviewed = new(2026, 10, 9) },
            _ => manifest with { NextReview = new(2026, 10, 8) }
        };
        Assert.False(RuntimePatchPolicy.IsCurrent(manifest, new(2026, 10, 8)));
    }
    [Theory]
    [InlineData("tamper")] [InlineData("delete")] [InlineData("patch")] [InlineData("sql")] [InlineData("reference")]
    public async Task FreshManifestProbeRejectsDrift(string change)
    {
        using var files = new ProbeFiles(); var manifest = Manifest(files); var profile = ProfileFixture.Valid;
        var probe = Probe(files, manifest);
        Assert.Equal(ReadinessProbeStatus.Pass, await probe.CheckAsync(profile, Ct));
        if (change == "tamper") File.AppendAllText(Path.Combine(files.Root, Paths[0]), " ");
        if (change == "delete") File.Delete(Path.Combine(files.Root, Paths[0]));
        if (change == "patch") probe = Probe(files, manifest with { DotNet = new(10, 0, 999) });
        if (change == "sql") probe = Probe(files, manifest, _ => ValueTask.FromResult<Version?>(new(17, 0, 1000, 8)));
        if (change == "reference") profile = profile with { Runtime = profile.Runtime! with { PatchManifestReference = "other" } };
        Assert.Equal(ReadinessProbeStatus.NotReady, await probe.CheckAsync(profile, Ct));
    }
    [Fact]
    public async Task ManifestResolverIsNotCalledForInvalidReferenceAndCancellationPropagates()
    {
        using var files = new ProbeFiles(); var manifest = Manifest(files); var calls = 0;
        var probe = Probe(files, manifest, _ => { calls++; return ValueTask.FromResult<Version?>(manifest.Sql); });
        var profile = ProfileFixture.Valid;
        Assert.Equal(ReadinessProbeStatus.NotReady, await probe.CheckAsync(profile with { Runtime = null }, Ct));
        Assert.Equal(0, calls);
        using var cancel = new CancellationTokenSource(); cancel.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => probe.CheckAsync(profile, cancel.Token));
    }
    [Fact]
    public async Task UncooperativeManifestVersionResolverIsBoundedAndLateFaultObserved()
    {
        using var files = new ProbeFiles(); var manifest = Manifest(files);
        var late = new TaskCompletionSource<Version?>(TaskCreationOptions.RunContinuationsAsynchronously);
        var profile = ProfileFixture.Valid with { Execution = ProfileFixture.Valid.Execution! with { ReadinessMs = 30 } };
        Assert.Equal(ReadinessProbeStatus.NotReady, await Probe(files, manifest, _ => new(late.Task)).CheckAsync(profile, Ct));
        late.SetException(new InvalidOperationException("Synthetic failure"));
    }
    private JwtReadinessMaterial Material(SecurityKey key, string algorithm) =>
        new("synthetic-jwt", new SigningCredentials(key, algorithm), "synthetic-issuer", "synthetic-audience",
            [new("synthetic-jwt", key, algorithm, clock.Now.AddDays(-1), clock.Now.AddDays(1))]);

    [Theory]
    [InlineData("hmac")] [InlineData("rsa")] [InlineData("ecdsa")]
    public async Task ActualCryptographicSigningAndValidationWork(string kind)
    {
        using var rsa = RSA.Create(2048); using var ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        SecurityKey key = kind switch { "rsa" => new RsaSecurityKey(rsa), "ecdsa" => new ECDsaSecurityKey(ecdsa),
            _ => new SymmetricSecurityKey(RandomNumberGenerator.GetBytes(32)) };
        var algorithm = kind switch { "rsa" => SecurityAlgorithms.RsaSha256, "ecdsa" => SecurityAlgorithms.EcdsaSha256,
            _ => SecurityAlgorithms.HmacSha256 };
        var material = Material(key, algorithm);
        var probe = new JwtKeyReadinessProbe(clock, _ => ValueTask.FromResult<JwtReadinessMaterial?>(material));
        Assert.Equal(ReadinessProbeStatus.Pass, await probe.CheckAsync(ProfileFixture.Valid, Ct));
        Assert.Equal("JwtReadinessMaterial", material.ToString());
        Assert.Equal("JwtValidationMaterial", material.ValidationKeys[0].ToString());
    }
    [Theory]
    [InlineData("wrongkey")] [InlineData("weak")] [InlineData("issuer")] [InlineData("audience")]
    [InlineData("reference")] [InlineData("duplicate")] [InlineData("missing")]
    [InlineData("future")] [InlineData("expired")] [InlineData("short")] [InlineData("algorithm")]
    public async Task UnusableOrMisboundKeyRingFails(string change)
    {
        var key = new SymmetricSecurityKey(RandomNumberGenerator.GetBytes(change == "weak" ? 16 : 32));
        var material = Material(key, SecurityAlgorithms.HmacSha256);
        var active = material.ValidationKeys[0];
        material = change switch
        {
            "wrongkey" => material with { ValidationKeys = [active with { Key = new SymmetricSecurityKey(RandomNumberGenerator.GetBytes(32)) }] },
            "issuer" => material with { Issuer = "other" }, "audience" => material with { Audience = "other" },
            "reference" => material with { SigningReference = "other" },
            "duplicate" => material with { ValidationKeys = [active, active] },
            "missing" => material with { ValidationKeys = [] },
            "future" => material with { ValidationKeys = [active with { ValidFrom = clock.Now.AddTicks(1) }] },
            "expired" => material with { ValidationKeys = [active with { ValidUntil = clock.Now }] },
            "short" => material with { ValidationKeys = [active with { ValidUntil = clock.Now.AddHours(2).AddTicks(-1) }] },
            "algorithm" => material with { ValidationKeys = [active with { Algorithm = "none" }] },
            _ => material
        };
        var probe = new JwtKeyReadinessProbe(clock, _ => ValueTask.FromResult<JwtReadinessMaterial?>(material));
        Assert.Equal(ReadinessProbeStatus.NotReady, await probe.CheckAsync(ProfileFixture.Valid, Ct));
    }
    [Fact]
    public async Task RotationIsFreshAndRetiringKeyCannotSubstituteForWrongActiveKey()
    {
        var old = new SymmetricSecurityKey(RandomNumberGenerator.GetBytes(32));
        var key = new SymmetricSecurityKey(RandomNumberGenerator.GetBytes(32)); var material = Material(key, SecurityAlgorithms.HmacSha256);
        material = material with { ValidationKeys = material.ValidationKeys.Add(new("retiring", old,
            SecurityAlgorithms.HmacSha256, clock.Now.AddDays(-1), clock.Now.AddMinutes(5))) };
        var profile = ProfileFixture.Valid with { Auth = ProfileFixture.Valid.Auth! with { JwtKeySecretReferences = ["synthetic-jwt", "retiring"] } };
        var calls = 0;
        var probe = new JwtKeyReadinessProbe(clock, _ => { calls++; return ValueTask.FromResult<JwtReadinessMaterial?>(material); });
        Assert.Equal(ReadinessProbeStatus.Pass, await probe.CheckAsync(profile, Ct));
        material = material with { Signing = new(old, SecurityAlgorithms.HmacSha256) };
        Assert.Equal(ReadinessProbeStatus.NotReady, await probe.CheckAsync(profile, Ct)); Assert.Equal(2, calls);
    }
    [Fact]
    public async Task JwtCancellationAndResolverFailureAreSanitized()
    {
        using var cancel = new CancellationTokenSource(); cancel.Cancel();
        var probe = new JwtKeyReadinessProbe(clock, _ => throw new InvalidOperationException("Synthetic secret"));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => probe.CheckAsync(ProfileFixture.Valid, cancel.Token));
        Assert.Equal(ReadinessProbeStatus.NotReady, await probe.CheckAsync(ProfileFixture.Valid, Ct));
    }
}
