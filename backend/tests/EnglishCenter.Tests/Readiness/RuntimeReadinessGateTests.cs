using EnglishCenter.Api.Infrastructure.Readiness;
using Xunit;

namespace EnglishCenter.Tests.Readiness;

public sealed class RuntimeReadinessGateTests
{
    private sealed class Probe(ReadinessDependency dependency) : IRuntimeReadinessProbe
    {
        public ReadinessDependency Dependency { get; set; } = dependency;
        public int Calls { get; private set; }
        public Func<CancellationToken, Task<ReadinessProbeStatus>> Run { get; set; }
            = _ => Task.FromResult(ReadinessProbeStatus.Pass);
        public Task<ReadinessProbeStatus> CheckAsync(RuntimeProfile profile, CancellationToken cancellationToken)
        { Calls++; return Run(cancellationToken); }
    }
    private static Probe[] Complete() => Enum.GetValues<ReadinessDependency>().Select(value => new Probe(value)).ToArray();

    [Fact]
    public async Task EmptyGateCannotTurnDescriptorsIntoRuntimeEvidence()
    {
        var result = await new RuntimeReadinessGate(null).EvaluateAsync(ProfileFixture.Valid, TestContext.Current.CancellationToken);
        Assert.Equal(ReadinessFailure.ProbeConfiguration, result.Failure);
        Assert.Equal(503, result.StatusCode);
        Assert.Equal("not_ready", result.Body);
    }

    [Fact]
    public async Task InvalidProfileStopsBeforeAnyProbe()
    {
        var probes = Complete();
        var result = await new RuntimeReadinessGate(probes).EvaluateAsync(null, TestContext.Current.CancellationToken);
        Assert.Equal(ReadinessFailure.InvalidProfile, result.Failure);
        Assert.Equal(ProfileIssue.MissingProfile, result.ProfileIssue);
        Assert.All(probes, probe => Assert.Equal(0, probe.Calls));
    }

    [Theory]
    [InlineData("missing")] [InlineData("duplicate")] [InlineData("unknown")] [InlineData("null")]
    public async Task IncompleteAmbiguousOrUnknownEvidenceRejects(string variant)
    {
        IRuntimeReadinessProbe[] probes = Complete().Cast<IRuntimeReadinessProbe>().ToArray();
        if (variant == "missing") probes = probes[..^1];
        if (variant == "duplicate") probes[^1] = probes[0];
        if (variant == "unknown") probes[^1] = new Probe((ReadinessDependency)999);
        if (variant == "null") probes[^1] = null!;
        Assert.Equal(ReadinessFailure.ProbeConfiguration,
            (await new RuntimeReadinessGate(probes).EvaluateAsync(ProfileFixture.Valid, TestContext.Current.CancellationToken)).Failure);
    }

    [Fact]
    public async Task GateSnapshotsRegistrationCollectionButRechecksCategories()
    {
        var probes = Complete();
        var original = probes[0];
        var gate = new RuntimeReadinessGate(probes);
        probes[0] = new Probe((ReadinessDependency)999);
        Assert.True((await gate.EvaluateAsync(ProfileFixture.Valid, TestContext.Current.CancellationToken)).IsReady);
        original.Dependency = (ReadinessDependency)999;
        Assert.Equal(ReadinessFailure.ProbeConfiguration,
            (await gate.EvaluateAsync(ProfileFixture.Valid, TestContext.Current.CancellationToken)).Failure);
    }

    [Fact]
    public async Task EveryProbeRunsFreshAndNoPriorSuccessSurvivesFailure()
    {
        var probes = Complete();
        var gate = new RuntimeReadinessGate(probes);
        var first = await gate.EvaluateAsync(ProfileFixture.Valid, TestContext.Current.CancellationToken);
        Assert.True(first.IsReady);
        Assert.Equal(200, first.StatusCode);
        Assert.Equal("ready", first.Body);
        Assert.True((await gate.EvaluateAsync(ProfileFixture.Valid, TestContext.Current.CancellationToken)).IsReady);
        Assert.All(probes, probe => Assert.Equal(2, probe.Calls));
        probes[0].Run = _ => Task.FromResult(ReadinessProbeStatus.NotReady);
        Assert.False((await gate.EvaluateAsync(ProfileFixture.Valid, TestContext.Current.CancellationToken)).IsReady);
    }

    [Theory]
    [InlineData(1)] [InlineData(2)] [InlineData(3)] [InlineData(4)] [InlineData(5)]
    [InlineData(6)] [InlineData(7)] [InlineData(8)] [InlineData(9)]
    public async Task AnyEssentialCategoryFailureBlocksReadiness(int failed)
    {
        var probes = Complete();
        probes.Single(probe => (int)probe.Dependency == failed).Run = _ => Task.FromResult(ReadinessProbeStatus.NotReady);
        Assert.Equal(ReadinessFailure.DependencyUnavailable,
            (await new RuntimeReadinessGate(probes).EvaluateAsync(ProfileFixture.Valid, TestContext.Current.CancellationToken)).Failure);
    }

    [Theory]
    [InlineData("sync")] [InlineData("async")] [InlineData("status")] [InlineData("null")]
    [InlineData("self-cancel")]
    public async Task ExceptionsAndMalformedEvidenceNeverLeakDetailsOrBecomeReady(string variant)
    {
        var probes = Complete();
        probes[0].Run = variant switch
        {
            "sync" => _ => throw new InvalidOperationException("private-source-and-key"),
            "async" => _ => Task.FromException<ReadinessProbeStatus>(new Exception("private-source-and-key")),
            "status" => _ => Task.FromResult((ReadinessProbeStatus)999),
            "self-cancel" => _ => Task.FromCanceled<ReadinessProbeStatus>(new CancellationToken(true)),
            _ => _ => null!
        };
        var result = await new RuntimeReadinessGate(probes).EvaluateAsync(ProfileFixture.Valid, TestContext.Current.CancellationToken);
        Assert.Equal(ReadinessFailure.DependencyUnavailable, result.Failure);
        Assert.Equal("not_ready", result.ToString());
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public async Task OverallDeadlineBoundsAsyncProbeEvenWhenItIgnoresCancellation(bool cooperative)
    {
        var probes = Complete();
        var pending = new TaskCompletionSource<ReadinessProbeStatus>(TaskCreationOptions.RunContinuationsAsynchronously);
        CancellationToken observed = default;
        probes[0].Run = token => { observed = token; return cooperative ? WaitForCancellation(token) : pending.Task; };
        var profile = ProfileFixture.Valid with { Execution = ProfileFixture.Valid.Execution! with { ReadinessMs = 30 } };
        var result = await new RuntimeReadinessGate(probes).EvaluateAsync(profile, TestContext.Current.CancellationToken).WaitAsync(TimeSpan.FromSeconds(3), TestContext.Current.CancellationToken);
        Assert.Equal(ReadinessFailure.Deadline, result.Failure);
        Assert.True(observed.IsCancellationRequested);
        Assert.Equal(0, probes[1].Calls);
        pending.TrySetException(new InvalidOperationException("late-private-fault"));
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public async Task CallerCancellationPropagatesBeforeOrDuringProbe(bool during)
    {
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        var probes = Complete();
        if (!during) cancellation.Cancel();
        else probes[0].Run = token => { cancellation.Cancel(); return WaitForCancellation(token); };
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            new RuntimeReadinessGate(probes).EvaluateAsync(ProfileFixture.Valid, cancellation.Token));
        Assert.Equal(during ? 1 : 0, probes[0].Calls);
    }

    [Fact]
    public async Task CancellationCannotBeConvertedToDependencyFailureByThrowingProbe()
    {
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        var probes = Complete();
        probes[0].Run = _ => { cancellation.Cancel(); throw new Exception("private-source-and-key"); };
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            new RuntimeReadinessGate(probes).EvaluateAsync(ProfileFixture.Valid, cancellation.Token));
    }

    private sealed class InvalidDescriptorProbe : IRuntimeReadinessProbe
    {
        public ReadinessDependency Dependency => throw new Exception("private-config-detail");
        public Task<ReadinessProbeStatus> CheckAsync(RuntimeProfile profile, CancellationToken token)
            => throw new InvalidOperationException("must-not-run");
    }

    [Fact]
    public async Task ThrowingDependencyDescriptorIsSanitizedWithoutInvokingProbe()
    {
        IRuntimeReadinessProbe[] probes = Complete().Cast<IRuntimeReadinessProbe>().ToArray();
        probes[0] = new InvalidDescriptorProbe();
        var result = await new RuntimeReadinessGate(probes).EvaluateAsync(ProfileFixture.Valid, TestContext.Current.CancellationToken);
        Assert.Equal(ReadinessFailure.ProbeConfiguration, result.Failure);
        Assert.Equal("not_ready", result.Body);
    }

    private static async Task<ReadinessProbeStatus> WaitForCancellation(CancellationToken token)
    { await Task.Delay(Timeout.InfiniteTimeSpan, token); return ReadinessProbeStatus.Pass; }
}
