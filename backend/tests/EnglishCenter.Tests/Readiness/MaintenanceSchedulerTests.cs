using System.Diagnostics;
using EnglishCenter.Api.Infrastructure.Maintenance;
using Xunit;

namespace EnglishCenter.Tests.Readiness;

public sealed class MaintenanceSchedulerTests
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;
    private static SqlMaintenanceService Service(ManualClock clock,
        Func<CancellationToken, ValueTask<string?>> resolver, int auditSeconds = 2)
        => new(ProfileFixture.Valid.Maintenance! with { ReplayCleanupIntervalSeconds = 1 },
            ProfileFixture.Valid.Execution!, auditSeconds, clock, resolver);

    [Fact]
    public async Task ExplicitIndependentPeriodsResolveFreshAndCountEachFailedRun()
    {
        var clock = new ManualClock(); var calls = 0;
        using var service = Service(clock, _ => { Interlocked.Increment(ref calls); return new((string?)null); });
        await service.StartAsync(Ct);
        try
        {
            await Until(() => clock.Timers.Length == 2);
            Assert.Equal(0, Volatile.Read(ref calls));
            Assert.Equal(new[] { TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2) },
                clock.Timers.Select(t => t.Period).Order().ToArray());
            Assert.All(clock.Timers, t => Assert.Equal(t.Period, t.Due));
            clock.Fire(1); await Until(() => service.FailedRuns == 1);
            Assert.Equal(1, Volatile.Read(ref calls));
            clock.Fire(2); await Until(() => service.FailedRuns == 2);
            clock.Fire(1); await Until(() => service.FailedRuns == 3);
            Assert.Equal(3, Volatile.Read(ref calls));
        }
        finally { await service.StopAsync(Ct); }
        Assert.True(service.ExecuteTask!.IsCompleted);
        Assert.All(clock.Timers, t => Assert.True(t.Disposed));
        clock.Fire(1); clock.Fire(2); Assert.Equal(3, Volatile.Read(ref calls));
    }

    [Fact]
    public async Task BusyJobsRemainIndependentAndRepeatedTicksCoalesceWithoutOverlap()
    {
        var clock = new ManualClock(); var calls = 0;
        var release = new TaskCompletionSource<string?>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var service = Service(clock, _ =>
        { Interlocked.Increment(ref calls); return new(release.Task); });
        await service.StartAsync(Ct);
        try
        {
            await Until(() => clock.Timers.Length == 2);
            clock.Fire(1); await Until(() => Volatile.Read(ref calls) == 1);
            clock.Fire(2); await Until(() => Volatile.Read(ref calls) == 2);
            for (var i = 0; i < 20; i++) { clock.Fire(1); clock.Fire(2); }
            Assert.Equal(2, Volatile.Read(ref calls)); Assert.Equal(0, service.FailedRuns);
            release.TrySetResult(null);
            await Until(() => service.FailedRuns == 4);
            Assert.Equal(4, Volatile.Read(ref calls));
        }
        finally { await service.StopAsync(Ct); release.TrySetResult(null); }
        Assert.All(clock.Timers, t => Assert.True(t.Disposed));
    }

    [Fact]
    public async Task ShutdownCancelsPendingUncooperativeResolversWithoutCountingHostStopAsFailure()
    {
        var clock = new ManualClock(); var calls = 0;
        var pending = new TaskCompletionSource<string?>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var service = Service(clock, _ =>
        { Interlocked.Increment(ref calls); return new(pending.Task); });
        await service.StartAsync(Ct);
        try
        {
            await Until(() => clock.Timers.Length == 2);
            clock.Fire(1); clock.Fire(2); await Until(() => Volatile.Read(ref calls) == 2);
        }
        finally
        {
            await service.StopAsync(Ct).WaitAsync(TimeSpan.FromSeconds(5), Ct);
            pending.TrySetException(new InvalidOperationException("Synthetic late resolver fault"));
        }
        Assert.True(service.ExecuteTask!.IsCompleted);
        Assert.Equal(0, service.FailedRuns);
        Assert.All(clock.Timers, t => Assert.True(t.Disposed));
        clock.Fire(1); clock.Fire(2); Assert.Equal(2, Volatile.Read(ref calls));
    }

    [Theory]
    [InlineData(0)] [InlineData(-1)] [InlineData(int.MaxValue)]
    public async Task InvalidScheduleFailsBackgroundExecutionBeforeTimersOrResolution(int auditSeconds)
    {
        var clock = new ManualClock(); var calls = 0;
        using var service = Service(clock, _ =>
        { Interlocked.Increment(ref calls); return new((string?)null); }, auditSeconds);
        await service.StartAsync(Ct);
        try
        {
            await Assert.ThrowsAsync<InvalidOperationException>(() => service.ExecuteTask!.WaitAsync(TimeSpan.FromSeconds(5), Ct));
            Assert.Empty(clock.Timers); Assert.Equal(0, Volatile.Read(ref calls));
        }
        finally { await service.StopAsync(Ct); }
    }

    private static async Task Until(Func<bool> condition)
    {
        var started = Stopwatch.StartNew();
        while (!condition())
        {
            Ct.ThrowIfCancellationRequested();
            if (started.Elapsed > TimeSpan.FromSeconds(5)) throw new TimeoutException("Scheduler observation timed out");
            await Task.Delay(10, Ct);
        }
    }

    private sealed class ManualClock : TimeProvider
    {
        private readonly Lock sync = new();
        private readonly List<ManualTimer> timers = [];
        internal ManualTimer[] Timers { get { lock (sync) return timers.ToArray(); } }
        public override DateTimeOffset GetUtcNow() => new(2026, 10, 9, 0, 0, 0, TimeSpan.Zero);
        public override ITimer CreateTimer(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period)
        {
            var timer = new ManualTimer(callback, state, dueTime, period);
            lock (sync) timers.Add(timer);
            return timer;
        }
        internal void Fire(int seconds)
        {
            foreach (var timer in Timers.Where(t => t.Period == TimeSpan.FromSeconds(seconds))) timer.Fire();
        }
    }
    private sealed class ManualTimer(TimerCallback callback, object? state, TimeSpan due, TimeSpan period) : ITimer
    {
        private int disposed;
        internal TimeSpan Due { get; private set; } = due;
        internal TimeSpan Period { get; private set; } = period;
        internal bool Disposed => Volatile.Read(ref disposed) == 1;
        public bool Change(TimeSpan dueTime, TimeSpan nextPeriod)
        {
            if (Disposed) return false;
            Due = dueTime; Period = nextPeriod; return true;
        }
        internal void Fire() { if (!Disposed) callback(state); }
        public void Dispose() => Interlocked.Exchange(ref disposed, 1);
        public ValueTask DisposeAsync() { Dispose(); return ValueTask.CompletedTask; }
    }
}

