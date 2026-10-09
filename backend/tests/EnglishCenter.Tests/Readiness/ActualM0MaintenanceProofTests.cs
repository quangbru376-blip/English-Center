using EnglishCenter.Api.Infrastructure.Maintenance;
using EnglishCenter.Api.Infrastructure.Operator;
using EnglishCenter.Api.Infrastructure.Readiness;
using Xunit;

namespace EnglishCenter.Tests.Readiness;

public static class M0MaintenanceProofGate
{
    public static bool MaintLive => M0SqlTlsGate.Maintenance && Environment.GetEnvironmentVariable("EC_M0_MAINT_PROOF") == "LIVE";
    public static bool RuntimeLive => M0SqlTlsGate.Runtime && Environment.GetEnvironmentVariable("EC_M0_MAINT_PROOF") == "LIVE";
    public static bool RuntimeStopped => M0SqlTlsGate.Runtime && Environment.GetEnvironmentVariable("EC_M0_MAINT_PROOF") == "STOPPED";
}
public sealed class ActualM0MaintenanceProofTests
{
    [Fact(Skip = "Actual bounded maintenance host opt-in", SkipUnless = nameof(M0MaintenanceProofGate.MaintLive), SkipType = typeof(M0MaintenanceProofGate))]
    public async Task GenuineMaintenanceHostRunsAndStopsWithOwnCancellation()
    {
        using var lifetime = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        lifetime.CancelAfter(20000);
        Assert.Equal(0, await LocalMaintenanceHost.RunAsync(lifetime.Token));
        Assert.True(lifetime.IsCancellationRequested);
    }
    [Fact(Skip = "Actual live maintenance proof opt-in", SkipUnless = nameof(M0MaintenanceProofGate.RuntimeLive), SkipType = typeof(M0MaintenanceProofGate))]
    public async Task RuntimeVerifiesThreeFreshOwnMaintenanceProofs()
    {
        var token = TestContext.Current.CancellationToken;
        var profile = await LocalRuntimeReferences.LoadProfileAsync(LocalRuntimeReferences.RuntimeRoot, token);
        Assert.NotNull(profile);
        var probe = new MaintenanceProxyReadinessProbe();
        for (var index = 0; index < 3; index++)
            Assert.Equal(ReadinessProbeStatus.Pass, await probe.CheckAsync(profile!, token));
    }
    [Fact(Skip = "Actual stopped maintenance proof opt-in", SkipUnless = nameof(M0MaintenanceProofGate.RuntimeStopped), SkipType = typeof(M0MaintenanceProofGate))]
    public async Task MissingWorkerCannotAttestCachedReady()
    {
        var token = TestContext.Current.CancellationToken;
        var profile = await LocalRuntimeReferences.LoadProfileAsync(LocalRuntimeReferences.RuntimeRoot, token);
        Assert.NotNull(profile);
        Assert.Equal(ReadinessProbeStatus.NotReady, await new MaintenanceProxyReadinessProbe().CheckAsync(profile!, token));
    }
}
