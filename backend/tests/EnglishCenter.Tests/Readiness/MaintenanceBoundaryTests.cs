using EnglishCenter.Api.Infrastructure.Maintenance;
using EnglishCenter.Api.Infrastructure.Readiness;
using Xunit;

namespace EnglishCenter.Tests.Readiness;

public sealed class MaintenanceBoundaryTests
{
    [Fact]
    public void OnlyAllKnownLeastPrivilegeFactsAreAccepted()
        => Assert.True(MaintenancePrincipalPolicy.IsSafe(false, false, false, false, false, false, true, true, true));

    [Theory]
    [InlineData(0)] [InlineData(1)] [InlineData(2)] [InlineData(3)] [InlineData(4)]
    [InlineData(5)] [InlineData(6)] [InlineData(7)] [InlineData(8)]
    public void EachUnknownOrUnsafeFactDenies(int index)
    {
        bool?[] facts = [false, false, false, false, false, false, true, true, true];
        facts[index] = null; Assert.False(Safe(facts));
        facts[index] = index < 6; Assert.False(Safe(facts));
    }

    [Theory]
    [InlineData(0)] [InlineData(-1)] [InlineData(int.MaxValue)]
    public void AuditIntervalMustBePositiveAndFinite(int seconds)
    {
        using var service = NewService(seconds, _ => ValueTask.FromResult<string?>(null));
        Assert.False(service.HasFiniteSchedules);
    }

    [Fact]
    public async Task ReadinessDoesNotRunCleanupAndResolvesFreshEachTime()
    {
        var resolves = 0;
        using var service = NewService(60, _ => { resolves++; return ValueTask.FromResult<string?>(null); });
        var probe = new MaintenanceReadinessProbe(service);
        var token = TestContext.Current.CancellationToken;
        Assert.Equal(ReadinessProbeStatus.NotReady, await probe.CheckAsync(ProfileFixture.Valid, token));
        Assert.Equal(ReadinessProbeStatus.NotReady, await probe.CheckAsync(ProfileFixture.Valid, token));
        Assert.Equal(2, resolves); Assert.Equal(0, service.FailedRuns);
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.RunReplayOnceAsync(token));
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.RunAuditOnceAsync(token));
    }

    [Theory]
    [InlineData("Server=remote;Database=fixture;Encrypt=True;TrustServerCertificate=False")]
    [InlineData("Server=.\\SQLEXPRESS;Database=fixture;Encrypt=True;TrustServerCertificate=True")]
    [InlineData("Server=.\\SQLEXPRESS;Database=fixture;Encrypt=False")]
    public async Task UnsafeConnectionNeverReachesSql(string supplied)
    {
        using var service = NewService(60, _ => ValueTask.FromResult<string?>(supplied));
        Assert.Equal(ReadinessProbeStatus.NotReady, await new MaintenanceReadinessProbe(service)
            .CheckAsync(ProfileFixture.Valid, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task MissingSeparateBindingAndCallerCancellationDenyBeforeResolution()
    {
        var token = TestContext.Current.CancellationToken;
        Assert.Equal(ReadinessProbeStatus.NotReady, await new MaintenanceReadinessProbe(null).CheckAsync(ProfileFixture.Valid, token));
        var resolves = 0;
        using var service = NewService(60, _ => { resolves++; return ValueTask.FromResult<string?>(null); });
        using var canceled = CancellationTokenSource.CreateLinkedTokenSource(token); canceled.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => new MaintenanceReadinessProbe(service)
            .CheckAsync(ProfileFixture.Valid, canceled.Token));
        Assert.Equal(0, resolves);
    }

    [Fact]
    public async Task ResolverDeadlineReturnsNotReadyEvenWhenResolverIgnoresCancellation()
    {
        var pending = new TaskCompletionSource<string?>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var service = NewService(60, _ => new ValueTask<string?>(pending.Task));
        try
        {
            Assert.Equal(ReadinessProbeStatus.NotReady, await new MaintenanceReadinessProbe(service)
                .CheckAsync(ProfileFixture.Valid, TestContext.Current.CancellationToken));
        }
        finally { pending.TrySetException(new InvalidOperationException("Synthetic late resolver failure")); }
    }

    [Fact]
    public async Task SameRuntimeAndMaintenanceReferenceDeniesBeforeResolution()
    {
        var profile = ProfileFixture.Valid;
        var maintenance = profile.Maintenance! with { MaintenancePrincipalReference = profile.Database!.ConnectionSecretReference };
        var resolves = 0;
        using var service = new SqlMaintenanceService(maintenance, profile.Execution!, 60, TimeProvider.System,
            _ => { resolves++; return ValueTask.FromResult<string?>(null); });
        Assert.Equal(ReadinessProbeStatus.NotReady, await new MaintenanceReadinessProbe(service)
            .CheckAsync(profile with { Maintenance = maintenance }, TestContext.Current.CancellationToken));
        Assert.Equal(0, resolves);
    }
    private static bool Safe(bool?[] f) => MaintenancePrincipalPolicy.IsSafe(f[0], f[1], f[2], f[3], f[4], f[5], f[6], f[7], f[8]);
    private static SqlMaintenanceService NewService(int auditSeconds, Func<CancellationToken, ValueTask<string?>> resolver)
        => new(ProfileFixture.Valid.Maintenance!, ProfileFixture.Valid.Execution!, auditSeconds, TimeProvider.System, resolver);
}
