using System.Diagnostics;
using System.Net;
using EnglishCenter.Api.Api.Runtime;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Infrastructure.Readiness;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace EnglishCenter.Tests.Readiness;

public sealed class StartupReadinessTests
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;
    private sealed class Probe(ReadinessDependency dependency, Func<bool> listening, bool fail = false) : IRuntimeReadinessProbe
    {
        public ReadinessDependency Dependency => dependency;
        public int Calls;
        public Task<ReadinessProbeStatus> CheckAsync(RuntimeProfile profile, CancellationToken token)
        {
            token.ThrowIfCancellationRequested(); Assert.True(listening()); Interlocked.Increment(ref Calls);
            return Task.FromResult(fail ? ReadinessProbeStatus.NotReady : ReadinessProbeStatus.Pass);
        }
    }
    [Theory]
    [InlineData(1)] [InlineData(2)] [InlineData(3)] [InlineData(4)] [InlineData(5)]
    [InlineData(6)] [InlineData(7)] [InlineData(8)] [InlineData(9)]
    public async Task EachFailedDependencyRefusesActualHostOnlyAfterListenerStarted(int failed)
    {
        var builder = WebApplication.CreateBuilder(); builder.WebHost.UseTestServer();
        IHostApplicationLifetime? lifetime = null;
        var probes = Enum.GetValues<ReadinessDependency>().Select(d => new Probe(d,
            () => lifetime!.ApplicationStarted.IsCancellationRequested, (int)d == failed)).ToArray();
        var boundary = new RuntimeReadinessBoundary(new(probes), _ => Task.FromResult<RuntimeProfile?>(ProfileFixture.Valid), 1000);
        builder.Services.AddSingleton<IHostedService>(sp => new StartupReadinessService(boundary, sp.GetRequiredService<IHostApplicationLifetime>()));
        await using var app = builder.Build(); lifetime = app.Lifetime;
        app.UseApiErrorMiddleware(); boundary.Install(app, "/health/live", "/health/ready");
        await app.StartAsync(Ct);
        try
        {
            await Until(() => lifetime.ApplicationStopping.IsCancellationRequested);
            var service = (StartupReadinessService)app.Services.GetServices<IHostedService>().Single(s => s is StartupReadinessService);
            await service.ExecuteTask!.WaitAsync(TimeSpan.FromSeconds(5), Ct);
            Assert.True(service.HasRefusedStartup);
            Assert.Equal(1, probes[failed - 1].Calls);
            Assert.All(probes.Skip(failed), p => Assert.Equal(0, p.Calls));
        }
        finally { await app.StopAsync(Ct); }
    }

    [Fact]
    public async Task ActualHostSuccessfulStartupCannotAuthorizeLaterApiProfileDrift()
    {
        var builder = WebApplication.CreateBuilder(); builder.WebHost.UseTestServer();
        IHostApplicationLifetime? lifetime = null; RuntimeProfile? current = ProfileFixture.Valid; var loads = 0;
        var probes = Enum.GetValues<ReadinessDependency>().Select(d => new Probe(d,
            () => lifetime!.ApplicationStarted.IsCancellationRequested)).ToArray();
        var boundary = new RuntimeReadinessBoundary(new(probes), _ =>
        { Interlocked.Increment(ref loads); return Task.FromResult<RuntimeProfile?>(current); }, 1000);
        builder.Services.AddSingleton<IHostedService>(sp => new StartupReadinessService(boundary, sp.GetRequiredService<IHostApplicationLifetime>()));
        await using var app = builder.Build(); lifetime = app.Lifetime;
        app.UseApiErrorMiddleware(); boundary.Install(app, "/health/live", "/health/ready");
        app.MapGet("/api/v1/probe", () => "synthetic caller");
        await app.StartAsync(Ct);
        try
        {
            var service = (StartupReadinessService)app.Services.GetServices<IHostedService>().Single(s => s is StartupReadinessService);
            await service.ExecuteTask!.WaitAsync(TimeSpan.FromSeconds(5), Ct);
            Assert.False(service.HasRefusedStartup); Assert.False(lifetime.ApplicationStopping.IsCancellationRequested);
            Assert.Equal(1, loads); Assert.All(probes, p => Assert.Equal(1, p.Calls));
            current = null;
            using var client = app.GetTestClient();
            using var denied = await client.GetAsync("/api/v1/probe", Ct);
            Assert.Equal(HttpStatusCode.ServiceUnavailable, denied.StatusCode);
            Assert.Contains("SERVICE_UNAVAILABLE", await denied.Content.ReadAsStringAsync(Ct));
            Assert.Equal(2, loads); Assert.All(probes, p => Assert.Equal(1, p.Calls));
        }
        finally { await app.StopAsync(Ct); }
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public async Task StopBeforeOrDuringSourceAbortsWithoutStartupRefusal(bool afterStarted)
    {
        using var lifetime = new Lifetime(); var loaded = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var pending = new TaskCompletionSource<RuntimeProfile?>(TaskCreationOptions.RunContinuationsAsynchronously);
        var boundary = new RuntimeReadinessBoundary(new(null), _ => { loaded.TrySetResult(); return pending.Task; }, 1000);
        using var service = new StartupReadinessService(boundary, lifetime);
        await service.StartAsync(Ct);
        try
        {
            if (afterStarted) { lifetime.Started.Cancel(); await loaded.Task.WaitAsync(TimeSpan.FromSeconds(5), Ct); }
            lifetime.StopApplication();
            await service.StopAsync(Ct).WaitAsync(TimeSpan.FromSeconds(5), Ct);
            Assert.False(service.HasRefusedStartup);
            Assert.Equal(afterStarted, loaded.Task.IsCompleted);
            Assert.True(service.ExecuteTask!.IsCompleted);
            Assert.Equal(1, lifetime.Stops);
        }
        finally { await service.StopAsync(Ct); pending.TrySetException(new InvalidOperationException("Synthetic late fault")); }
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public async Task MissingSourceOrFiniteDeadlineRefusesWithoutPrivateDiagnostics(bool timesOut)
    {
        using var lifetime = new Lifetime();
        var pending = new TaskCompletionSource<RuntimeProfile?>(TaskCreationOptions.RunContinuationsAsynchronously);
        var boundary = new RuntimeReadinessBoundary(new(null), _ => timesOut ? pending.Task :
            throw new InvalidOperationException("synthetic-private-secret"), 30);
        using var service = new StartupReadinessService(boundary, lifetime);
        await service.StartAsync(Ct); lifetime.Started.Cancel();
        try
        {
            await service.ExecuteTask!.WaitAsync(TimeSpan.FromSeconds(5), Ct);
            Assert.True(service.HasRefusedStartup); Assert.Equal(1, lifetime.Stops);
            Assert.Equal(nameof(StartupReadinessService), service.ToString());
        }
        finally { await service.StopAsync(Ct); pending.TrySetException(new InvalidOperationException("Synthetic late fault")); }
    }

    private static async Task Until(Func<bool> condition)
    {
        var elapsed = Stopwatch.StartNew();
        while (!condition())
        {
            if (elapsed.Elapsed > TimeSpan.FromSeconds(5)) throw new TimeoutException("Startup observation timed out");
            await Task.Delay(10, Ct);
        }
    }
    private sealed class Lifetime : IHostApplicationLifetime, IDisposable
    {
        internal readonly CancellationTokenSource Started = new();
        private readonly CancellationTokenSource stopping = new();
        private readonly CancellationTokenSource stopped = new();
        internal int Stops;
        public CancellationToken ApplicationStarted => Started.Token;
        public CancellationToken ApplicationStopping => stopping.Token;
        public CancellationToken ApplicationStopped => stopped.Token;
        public void StopApplication() { Interlocked.Increment(ref Stops); stopping.Cancel(); }
        public void Dispose() { Started.Dispose(); stopping.Dispose(); stopped.Dispose(); }
    }
}

