using System.Security.Cryptography;
using System.Xml.Linq;
using EnglishCenter.Api.Infrastructure.Readiness;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EnglishCenter.Tests.Readiness;

public static class WindowsKeyProbeGate
{
    public static bool Enabled => OperatingSystem.IsWindows();
}
public sealed class ProtectedCsrfProbeTests
{
    private readonly ProbeClock clock = new();
    private static CancellationToken Ct => TestContext.Current.CancellationToken;
    private ProtectedCsrfReadinessProbe Probe(ProbeFiles files, int count = 10, int bytes = 65536) =>
        new("synthetic-csrf", files.Root, count, bytes, clock);

    private IServiceProvider Provision(ProbeFiles files, bool encrypted = true)
    {
        var services = new ServiceCollection();
        var builder = services.AddDataProtection().PersistKeysToFileSystem(new DirectoryInfo(files.Root)).DisableAutomaticKeyGeneration();
        if (encrypted)
        {
            if (!OperatingSystem.IsWindows()) throw new PlatformNotSupportedException();
            builder.ProtectKeysWithDpapi(protectToLocalMachine: false);
        }
        var provider = services.BuildServiceProvider();
        provider.GetRequiredService<IKeyManager>().CreateNewKey(clock.Now.AddMinutes(-1), clock.Now.AddDays(1));
        return provider;
    }
    private static string Digest(ProbeFiles files) => string.Join("|", Directory.GetFiles(files.Root).Order().Select(path =>
        Path.GetFileName(path) + ":" + Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)))));
    [Fact(Skip = "Windows DPAPI fixture only", SkipUnless = nameof(WindowsKeyProbeGate.Enabled), SkipType = typeof(WindowsKeyProbeGate))]
    public async Task ExistingEncryptedKeysDecryptAfterProviderRestartWithoutRepositoryWrites()
    {
        using var files = new ProbeFiles(); using (var provider = (IDisposable)Provision(files)) { }
        var before = Digest(files);
        Assert.Equal(ReadinessProbeStatus.Pass, await Probe(files).CheckAsync(ProfileFixture.Valid, Ct));
        Assert.Equal(ReadinessProbeStatus.Pass, await Probe(files).CheckAsync(ProfileFixture.Valid, Ct));
        Assert.Equal(before, Digest(files));
    }
    [Fact]
    public async Task PlaintextAndEmptyStoresFailWithoutGeneratingKeys()
    {
        using var files = new ProbeFiles();
        Assert.Equal(ReadinessProbeStatus.NotReady, await Probe(files).CheckAsync(ProfileFixture.Valid, Ct));
        Assert.Empty(Directory.GetFiles(files.Root));
        using var provider = (IDisposable)Provision(files, encrypted: false);
        var before = Digest(files);
        Assert.Equal(ReadinessProbeStatus.NotReady, await Probe(files).CheckAsync(ProfileFixture.Valid, Ct));
        Assert.Equal(before, Digest(files));
    }
    [Theory(Skip = "Windows DPAPI fixture only", SkipUnless = nameof(WindowsKeyProbeGate.Enabled), SkipType = typeof(WindowsKeyProbeGate))]
    [InlineData("corrupt")] [InlineData("revoked")] [InlineData("future")] [InlineData("expired")]
    [InlineData("decryptor")] [InlineData("duplicate")] [InlineData("size")] [InlineData("count")]
    public async Task ExistingStoreFailureIsFreshAndReadOnly(string change)
    {
        using var files = new ProbeFiles(); using var provision = (IDisposable)Provision(files);
        Assert.Equal(ReadinessProbeStatus.Pass, await Probe(files).CheckAsync(ProfileFixture.Valid, Ct));
        var path = Directory.GetFiles(files.Root, "key-*.xml").Single(); var element = XElement.Load(path);
        if (change == "revoked") ((IServiceProvider)provision).GetRequiredService<IKeyManager>().RevokeAllKeys(clock.Now.AddHours(1));
        else if (change == "future") { element.Element("activationDate")!.Value = clock.Now.AddMinutes(1).ToString("O"); element.Save(path); }
        else if (change == "expired") { element.Element("expirationDate")!.Value = clock.Now.ToString("O"); element.Save(path); }
        else if (change == "decryptor") { element.Descendants(XName.Get("encryptedSecret", "http://schemas.asp.net/2015/03/dataProtection")).Single().SetAttributeValue("decryptorType", "untrusted"); element.Save(path); }
        else if (change == "duplicate") File.Copy(path, Path.Combine(files.Root, "duplicate.xml"));
        else if (change == "corrupt") File.WriteAllText(path, "<key>");
        var before = Digest(files);
        Assert.Equal(ReadinessProbeStatus.NotReady,
            await Probe(files, count: change == "count" ? 0 : 10, bytes: change == "size" ? 1 : 65536).CheckAsync(ProfileFixture.Valid, Ct));
        Assert.Equal(before, Digest(files));
    }
    [Fact]
    public async Task BadBindingAndCancellationDoNotReadOrGenerate()
    {
        using var files = new ProbeFiles();
        var profile = ProfileFixture.Valid with { Auth = ProfileFixture.Valid.Auth! with { ProtectedPersistedCsrfStoreReference = "other" } };
        Assert.Equal(ReadinessProbeStatus.NotReady, await Probe(files).CheckAsync(profile, Ct));
        using var cancel = new CancellationTokenSource(); cancel.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => Probe(files).CheckAsync(ProfileFixture.Valid, cancel.Token));
        Assert.Empty(Directory.GetFiles(files.Root));
    }
}
