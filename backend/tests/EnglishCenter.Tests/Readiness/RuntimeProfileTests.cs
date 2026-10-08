using System.Collections.Immutable;
using EnglishCenter.Api.Infrastructure.Readiness;
using Xunit;

namespace EnglishCenter.Tests.Readiness;

internal static class ProfileFixture
{
    // Synthetic descriptors and budgets only; no active operator profile or loaded secrets.
    internal static RuntimeProfile Valid => new(RuntimeProfileKind.LOCAL_DEV,
        new(10, 10, 17, "Express", "synthetic-patches"),
        new("synthetic-connection-reference", "synthetic-schema", 160, 160, true),
        new("https://localhost:443", "127.0.0.1", 443, false, [], [], "synthetic-cert", "synthetic-trust"),
        new(["synthetic-jwt"], "synthetic-issuer", "synthetic-audience", "synthetic-csrf"),
        new(10, 1, 3000, 3000, 1000, 10000, 5_000_000, 100),
        new("synthetic-replay-job", 60, "synthetic-audit-job", "synthetic-maintenance-principal"),
        "Asia/Ho_Chi_Minh", "synthetic-blocklist");
}

public sealed class RuntimeProfileTests
{
    [Fact]
    public void CompleteSyntheticDescriptorsValidateWithoutLoadingAnything()
        => Assert.Equal(ProfileIssue.None, RuntimeProfileValidator.Validate(ProfileFixture.Valid));

    [Theory]
    [InlineData(0)] [InlineData(-1)] [InlineData(5)]
    public void UndefinedKindDoesNotSelectAnEnvironment(int kind)
        => Assert.Equal(ProfileIssue.ProfileKind,
            RuntimeProfileValidator.Validate(ProfileFixture.Valid with { Kind = (RuntimeProfileKind)kind }));

    [Theory]
    [InlineData("runtime")] [InlineData("database")] [InlineData("web")] [InlineData("auth")]
    [InlineData("execution")] [InlineData("maintenance")] [InlineData("timezone")] [InlineData("blocklist")]
    public void EachMissingGroupRejects(string group)
    {
        var value = ProfileFixture.Valid;
        var missing = group switch
        {
            "runtime" => value with { Runtime = null }, "database" => value with { Database = null },
            "web" => value with { Web = null }, "auth" => value with { Auth = null },
            "execution" => value with { Execution = null }, "maintenance" => value with { Maintenance = null },
            "timezone" => value with { BusinessTimeZone = null }, _ => value with { BlocklistManifestReference = null }
        };
        Assert.NotEqual(ProfileIssue.None, RuntimeProfileValidator.Validate(missing));
    }

    [Fact]
    public void NullProfileFailsClosed()
        => Assert.Equal(ProfileIssue.MissingProfile, RuntimeProfileValidator.Validate(null));

    [Theory]
    [InlineData(170, 160, true)] [InlineData(160, 170, true)] [InlineData(160, 160, false)]
    public void DatabaseCannotInheritProviderOrRcsiDefaults(int db, int ef, bool snapshot)
    {
        var value = ProfileFixture.Valid;
        Assert.Equal(ProfileIssue.Database, RuntimeProfileValidator.Validate(value with
        { Database = value.Database! with { DatabaseCompatibility = db, EfCompatibility = ef, SnapshotRequired = snapshot } }));
    }

    [Fact]
    public void RuntimeAndTimezoneCannotFallBackToHost()
    {
        var value = ProfileFixture.Valid;
        Assert.Equal(ProfileIssue.RuntimeTarget, RuntimeProfileValidator.Validate(value with
        { Runtime = value.Runtime! with { DotNetMajor = 9 } }));
        Assert.Equal(ProfileIssue.TimeZone, RuntimeProfileValidator.Validate(value with { BusinessTimeZone = "UTC" }));
    }

    [Theory]
    [InlineData("http://localhost")] [InlineData("https://user:secret@localhost")]
    [InlineData("https://localhost/path")] [InlineData("https://localhost?key=secret")]
    [InlineData(" https://localhost")] [InlineData("https://localhost#fragment")] [InlineData("https://example.invalid")]
    public void UnsafeOrNonlocalOriginRejects(string origin)
    {
        var value = ProfileFixture.Valid;
        Assert.Equal(ProfileIssue.Web, RuntimeProfileValidator.Validate(value with { Web = value.Web! with { Origin = origin } }));
    }

    [Theory]
    [InlineData("0.0.0.0", 443)] [InlineData("::", 443)] [InlineData("*", 443)]
    [InlineData("192.0.2.1", 443)] [InlineData("127.0.0.1", 0)] [InlineData("127.0.0.1", 65536)]
    public void ImplicitOrWildcardListenRejects(string address, int port)
    {
        var value = ProfileFixture.Valid;
        Assert.Equal(ProfileIssue.Web, RuntimeProfileValidator.Validate(value with
        { Web = value.Web! with { ListenAddress = address, ListenPort = port } }));
    }

    [Theory]
    [InlineData("0.0.0.0/0", false)] [InlineData("::/0", false)] [InlineData("192.0.2.1/24", false)]
    [InlineData("192.0.2.0/33", false)] [InlineData("bad/24", false)]
    [InlineData("192.0.2.0/24", true)] [InlineData("2001:db8::/32", true)]
    public void ForwardingRequiresCanonicalNarrowNetworks(string network, bool valid)
    {
        var value = ProfileFixture.Valid;
        var result = RuntimeProfileValidator.Validate(value with { Web = value.Web! with
        { ForwardingEnabled = true, KnownNetworks = [network] } });
        Assert.Equal(valid ? ProfileIssue.None : ProfileIssue.Web, result);
    }

    [Fact]
    public void ForwardingRequiresExplicitHopAndDirectModeRequiresEmptySet()
    {
        var value = ProfileFixture.Valid;
        Assert.Equal(ProfileIssue.Web, RuntimeProfileValidator.Validate(value with
        { Web = value.Web! with { ForwardingEnabled = true } }));
        Assert.Equal(ProfileIssue.Web, RuntimeProfileValidator.Validate(value with
        { Web = value.Web! with { KnownProxies = ["127.0.0.1"] } }));
        Assert.Equal(ProfileIssue.Web, RuntimeProfileValidator.Validate(value with
        { Web = value.Web! with { ForwardingEnabled = null } }));
        Assert.Equal(ProfileIssue.None, RuntimeProfileValidator.Validate(value with
        { Web = value.Web! with { ForwardingEnabled = true, KnownProxies = ["127.0.0.1"] } }));
    }

    [Fact]
    public void KeysRequireDistinctReferencesAndProtectedPersistentStore()
    {
        var value = ProfileFixture.Valid;
        foreach (var auth in new[] { value.Auth! with { JwtKeySecretReferences = [] },
            value.Auth! with { JwtKeySecretReferences = ["same", "same"] },
            value.Auth! with { ProtectedPersistedCsrfStoreReference = null },
            value.Auth! with { Issuer = "bad\nvalue" } })
            Assert.Equal(ProfileIssue.AuthKeys, RuntimeProfileValidator.Validate(value with { Auth = auth }));
    }

    [Theory]
    [InlineData(0, 5000000, 1)] [InlineData(1, 4999999, 1)] [InlineData(1, 5000000, 0)]
    public void ResourcesCannotDisableBodyBufferOrPreviewLimits(int body, int buffer, int preview)
    {
        var value = ProfileFixture.Valid;
        Assert.Equal(ProfileIssue.Limits, RuntimeProfileValidator.Validate(value with
        { Execution = value.Execution! with { RequestBodyBytes = body, ResponseBufferBytes = buffer, WeeklyPreviewBudget = preview } }));
    }

    [Fact]
    public void DescriptorsAreRedactedAndCollectionsImmutable()
    {
        var value = ProfileFixture.Valid;
        object[] descriptors = [value, value.Runtime!, value.Database!, value.Web!, value.Auth!, value.Execution!, value.Maintenance!];
        Assert.All(descriptors, item => Assert.Equal(item.GetType().Name, item.ToString()));
        var original = value.Auth!.JwtKeySecretReferences;
        var changed = original.Add("another-reference");
        Assert.Single(original);
        Assert.Equal(2, changed.Length);
    }
}
