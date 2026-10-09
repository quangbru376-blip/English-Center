using System.Text.Json;
using System.Text.Json.Serialization;
using EnglishCenter.Api.Infrastructure.Operator;
using EnglishCenter.Api.Infrastructure.Readiness;
using Xunit;

namespace EnglishCenter.Tests.Readiness;

public sealed class LocalRuntimeBindingTests
{
    internal static RuntimeProfile Bound => ProfileFixture.Valid with
    {
        Runtime = new(10, 10, 17, "Express", LocalRuntimeReferences.Patch),
        Database = new(LocalRuntimeReferences.RuntimeConnection, DatabaseReadinessProbe.SchemaManifestReference, 160, 160, true),
        Web = new("https://localhost:8443", "127.0.0.1", 8443, false, [], [], LocalRuntimeReferences.Https, LocalRuntimeReferences.HttpsTrust),
        Auth = new([LocalRuntimeReferences.Jwt], "english-center-m0-local", "english-center-m0-local-ui", LocalRuntimeReferences.Csrf),
        Execution = LocalRuntimeReferences.Execution, Maintenance = LocalRuntimeReferences.Maintenance,
        BlocklistManifestReference = LocalRuntimeReferences.Blocklist
    };
    [Fact]
    public async Task ExactSelectionLoadsFreshAndRefusesLaterProfileDrift()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N")); Directory.CreateDirectory(root);
        var path = Path.Combine(root, "profile.json");
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        options.Converters.Add(new JsonStringEnumConverter<RuntimeProfileKind>());
        try
        {
            await File.WriteAllTextAsync(path, JsonSerializer.Serialize(Bound, options), TestContext.Current.CancellationToken);
            Assert.NotNull(await LocalRuntimeReferences.LoadProfileAsync(root, TestContext.Current.CancellationToken));
            await File.WriteAllTextAsync(path, JsonSerializer.Serialize(Bound with { Execution = Bound.Execution! with { WeeklyPreviewBudget = 9999 } }, options), TestContext.Current.CancellationToken);
            Assert.Null(await LocalRuntimeReferences.LoadProfileAsync(root, TestContext.Current.CancellationToken));
            File.Delete(path);
            Assert.Null(await LocalRuntimeReferences.LoadProfileAsync(root, TestContext.Current.CancellationToken));
        }
        finally { if (File.Exists(path)) File.Delete(path); Directory.Delete(root); }
    }
    [Theory]
    [InlineData("kind")] [InlineData("database")] [InlineData("patch")] [InlineData("tls")]
    [InlineData("proxy")] [InlineData("keys")] [InlineData("issuer")] [InlineData("budget")]
    [InlineData("job")] [InlineData("timezone")] [InlineData("corpus")] [InlineData("null")]
    public void ValidGenericProfileCannotReplaceExactOperatorSelection(string field)
    {
        var profile = Bound;
        var changed = field switch
        {
            "kind" => profile with { Kind = RuntimeProfileKind.LOCAL_DEMO },
            "database" => profile with { Database = profile.Database! with { ConnectionSecretReference = "other" } },
            "patch" => profile with { Runtime = profile.Runtime! with { PatchManifestReference = "other" } },
            "tls" => profile with { Web = profile.Web! with { ClientTrustReference = "other" } },
            "proxy" => profile with { Web = profile.Web! with { KnownProxies = ["127.0.0.2"] } },
            "keys" => profile with { Auth = profile.Auth! with { JwtKeySecretReferences = [LocalRuntimeReferences.Jwt, "other"] } },
            "issuer" => profile with { Auth = profile.Auth! with { Issuer = "other" } },
            "budget" => profile with { Execution = profile.Execution! with { WeeklyPreviewBudget = 10001 } },
            "job" => profile with { Maintenance = profile.Maintenance! with { ReplayCleanupIntervalSeconds = 61 } },
            "timezone" => profile with { BusinessTimeZone = "UTC" },
            "corpus" => profile with { BlocklistManifestReference = "other" },
            _ => null
        };
        Assert.True(LocalRuntimeReferences.IsBound(profile)); Assert.False(LocalRuntimeReferences.IsBound(changed));
    }
    [Theory]
    [InlineData("{\"dotNet\":\"10.0.4\",\"dotNet\":\"10.0.5\"}")]
    [InlineData("{\"artifacts\":[{\"relativePath\":\"a\",\"relativePath\":\"b\"}]}")]
    [InlineData("{\"extra\":true}")] [InlineData("[]")]
    public void ManifestRejectsAmbiguousOrUnknownDescriptors(string json)
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N")); Directory.CreateDirectory(root);
        var path = Path.Combine(root, "patch-manifest.json");
        try { File.WriteAllText(path, json); Assert.ThrowsAny<Exception>(() => LocalRuntimeManifest.Load(root)); }
        finally { File.Delete(path); Directory.Delete(root); }
    }
    [Fact]
    public void OwnerProcessCannotOpenRuntimeKeys()
        => Assert.ThrowsAny<Exception>(() => LocalRuntimeKeyStore.Open());
}
