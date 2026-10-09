using EnglishCenter.Api.Infrastructure.Maintenance;
using EnglishCenter.Api.Infrastructure.Operator;
using EnglishCenter.Api.Infrastructure.Readiness;
using Xunit;

namespace EnglishCenter.Tests.Readiness;

public sealed class MaintenanceProxyTests
{
    [Fact]
    public async Task OperatorIdentityCannotAttestOrRunMaintenanceHost()
    {
        Assert.Equal(ReadinessProbeStatus.NotReady, await new MaintenanceProxyReadinessProbe().CheckAsync(LocalRuntimeBindingTests.Bound, TestContext.Current.CancellationToken));
        Assert.Equal(1, await LocalMaintenanceHost.RunAsync(TestContext.Current.CancellationToken));
    }
    [Fact]
    public async Task CallerCancellationPropagatesBeforeReadingCustody()
    {
        using var cancellation = new CancellationTokenSource(); cancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => new MaintenanceProxyReadinessProbe().CheckAsync(LocalRuntimeBindingTests.Bound, cancellation.Token));
    }
}
