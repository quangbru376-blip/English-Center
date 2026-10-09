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
    [Fact(Skip = "Actual M0 custody separation opt-in", SkipUnless = nameof(M0CustodyGate.NonRuntime), SkipType = typeof(M0CustodyGate))]
    public void OtherSqlIdentitiesCannotOpenRuntimeBindings()
        => Assert.Throws<InvalidOperationException>(() => LocalRuntimeKeyStore.Open());
}
