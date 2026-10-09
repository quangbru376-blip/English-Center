using EnglishCenter.Api.Infrastructure.Operator;
using EnglishCenter.Api.Infrastructure.Readiness;
using Xunit;

namespace EnglishCenter.Tests.Readiness;

public sealed class ActualM0RuntimeBindingTests
{
    [Fact(Skip = "Actual M0 runtime binding opt-in", SkipUnless = nameof(M0SqlTlsGate.Runtime), SkipType = typeof(M0SqlTlsGate))]
    public async Task ExistingRuntimeKeysBindReadOnlyAndDisposedStoreRefusesResolution()
    {
        using var keys = LocalRuntimeKeyStore.Open();
        var token = TestContext.Current.CancellationToken;
        Assert.Equal(ReadinessProbeStatus.Pass, await new JwtKeyReadinessProbe(TimeProvider.System, keys.ResolveJwt)
            .CheckAsync(LocalRuntimeBindingTests.Bound, token));
        Assert.NotNull(await keys.ResolveTrust(token));
        keys.Dispose();
        Assert.Null(await keys.ResolveJwt(token)); Assert.Null(await keys.ResolveTrust(token));
    }
    [Fact(Skip = "Actual M0 profile/manifest opt-in", SkipUnless = nameof(M0SqlTlsGate.Runtime), SkipType = typeof(M0SqlTlsGate))]
    public async Task InstalledProfileAndPinsMatchLoadedRuntimeAndFreshStrictSqlVersion()
    {
        var token = TestContext.Current.CancellationToken;
        var profile = await LocalRuntimeReferences.LoadProfileAsync(LocalRuntimeReferences.RuntimeRoot, token);
        Assert.NotNull(profile);
        var manifest = LocalRuntimeManifest.Load(LocalRuntimeReferences.RuntimeRoot);
        var probe = new RuntimeManifestReadinessProbe(LocalRuntimeReferences.Patch, LocalRuntimeReferences.Workspace,
            manifest, TimeProvider.System, LocalRuntimeReferences.RuntimeConnection, LocalRuntimeManifest.ReadSqlVersionAsync);
        Assert.Equal(ReadinessProbeStatus.Pass, await probe.CheckAsync(profile!, token));
        Assert.Equal(ReadinessProbeStatus.Pass, await probe.CheckAsync(profile!, token));
    }
    [Fact(Skip = "Actual M0 custody separation opt-in", SkipUnless = nameof(M0CustodyGate.NonRuntime), SkipType = typeof(M0CustodyGate))]
    public void OtherSqlIdentitiesCannotOpenRuntimeBindings()
        => Assert.Throws<InvalidOperationException>(() => LocalRuntimeKeyStore.Open());
}
