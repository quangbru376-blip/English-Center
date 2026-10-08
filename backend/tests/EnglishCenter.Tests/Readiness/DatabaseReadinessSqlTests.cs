using EnglishCenter.Api.Infrastructure.Readiness;
using Xunit;

namespace EnglishCenter.Tests.Readiness;

public static class ReadinessSqlGate
{
    public static bool Enabled => Environment.GetEnvironmentVariable("EC_V01_READINESS_SQL") == "1";
}

[Collection("V01 HTTP SQL")]
public sealed class DatabaseReadinessSqlTests
{
    private static DatabaseReadinessProbe DiagnosticProbe(Action? resolved = null) => new("synthetic-connection-reference", token =>
    {
        token.ThrowIfCancellationRequested();
        resolved?.Invoke();
        return ValueTask.FromResult<string?>("Server=lpc:.\\SQLEXPRESS;Database=EnglishCenter_D07_20261005;Integrated Security=True;Encrypt=True;TrustServerCertificate=True");
    });
    private static RuntimeProfile Profile => DatabaseReadinessProbeTests.ValidProfile with
    { Execution = DatabaseReadinessProbeTests.ValidProfile.Execution! with { SqlSeconds = 10, RequestMs = 15000, ReadinessMs = 12000 } };

    [Fact(Skip = "V01-I read-only SQL requires opt-in", SkipUnless = nameof(ReadinessSqlGate.Enabled), SkipType = typeof(ReadinessSqlGate))]
    public async Task ActualDiagnosticConnectionCannotBecomeRuntimeEvidence()
    {
        var probe = DiagnosticProbe();
        Assert.Equal(ReadinessProbeStatus.NotReady, await probe.CheckAsync(Profile, TestContext.Current.CancellationToken));
        Assert.Equal(1, probe.CompletedPrincipalChecks);
    }

    [Fact(Skip = "V01-I read-only SQL requires opt-in", SkipUnless = nameof(ReadinessSqlGate.Enabled), SkipType = typeof(ReadinessSqlGate))]
    public async Task RepeatedProbeUsesFreshResolutionAndNeverCachesDiagnosticSuccess()
    {
        var calls = 0;
        var probe = DiagnosticProbe(() => calls++);
        Assert.Equal(ReadinessProbeStatus.NotReady, await probe.CheckAsync(Profile, TestContext.Current.CancellationToken));
        Assert.Equal(ReadinessProbeStatus.NotReady, await probe.CheckAsync(Profile, TestContext.Current.CancellationToken));
        Assert.Equal(2, calls);
        Assert.Equal(2, probe.CompletedPrincipalChecks);
    }

    [Fact(Skip = "V01-I read-only SQL requires opt-in", SkipUnless = nameof(ReadinessSqlGate.Enabled), SkipType = typeof(ReadinessSqlGate))]
    public async Task GateReturnsOnlyNotReady503WhenDiagnosticDatabaseProofFails()
    {
        var database = DiagnosticProbe();
        var probes = Enum.GetValues<ReadinessDependency>().Select(dependency =>
            dependency == ReadinessDependency.DatabaseAuthorityAndCatalog
                ? (IRuntimeReadinessProbe)database : new SyntheticPass(dependency));
        var result = await new RuntimeReadinessGate(probes).EvaluateAsync(Profile, TestContext.Current.CancellationToken);
        Assert.False(result.IsReady);
        Assert.Equal(ReadinessFailure.DependencyUnavailable, result.Failure);
        Assert.Equal(503, result.StatusCode);
        Assert.Equal("not_ready", result.Body);
        Assert.Equal(1, database.CompletedPrincipalChecks);
    }
    // Other categories are explicit synthetic seams; these tests prove diagnostic denial only.
    private sealed class SyntheticPass(ReadinessDependency dependency) : IRuntimeReadinessProbe
    {
        public ReadinessDependency Dependency => dependency;
        public Task<ReadinessProbeStatus> CheckAsync(RuntimeProfile profile, CancellationToken token)
        { token.ThrowIfCancellationRequested(); return Task.FromResult(ReadinessProbeStatus.Pass); }
    }
}
