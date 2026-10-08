using System.Net;
using EnglishCenter.Api.Api.Runtime;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Infrastructure.Readiness;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EnglishCenter.Tests.Readiness;

public sealed class ReadinessBoundaryTests
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;
    private sealed class Probe(ReadinessDependency dependency) : IRuntimeReadinessProbe
    {
        public ReadinessDependency Dependency => dependency;
        public int Calls { get; private set; }
        public ReadinessProbeStatus Status { get; set; } = ReadinessProbeStatus.Pass;
        public Task<ReadinessProbeStatus> CheckAsync(RuntimeProfile profile, CancellationToken token)
        { token.ThrowIfCancellationRequested(); Calls++; return Task.FromResult(Status); }
    }
    private static Probe[] Probes() => Enum.GetValues<ReadinessDependency>().Select(d => new Probe(d)).ToArray();

    [Theory]
    [InlineData(1)] [InlineData(2)] [InlineData(3)] [InlineData(4)] [InlineData(5)]
    [InlineData(6)] [InlineData(7)] [InlineData(8)] [InlineData(9)]
    public async Task EveryDependencyCanStopActualApiBeforeCaller(int failed)
    {
        var probes = Probes(); probes[failed - 1].Status = ReadinessProbeStatus.NotReady;
        var boundary = NewBoundary(probes);
        await using var server = await Server(boundary); using var client = server.GetTestClient();
        using var response = await client.GetAsync("/api/v1/accounts", Ct);
        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        Assert.False(response.Headers.Contains("X-Caller"));
        Assert.Contains("SERVICE_UNAVAILABLE", await response.Content.ReadAsStringAsync(Ct));
        Assert.Equal(1, probes[failed - 1].Calls);
    }

    [Fact]
    public async Task SuccessfulHealthIsNotCachedAcrossProfileOrDependencyDrift()
    {
        var probes = Probes(); RuntimeProfile? current = ProfileFixture.Valid; var loads = 0;
        var boundary = new RuntimeReadinessBoundary(new(probes),
            _ => { loads++; return Task.FromResult<RuntimeProfile?>(current); }, 1000);
        await using var server = await Server(boundary); using var client = server.GetTestClient();
        using var ready = await client.GetAsync("/health/ready", Ct);
        Assert.Equal(HttpStatusCode.OK, ready.StatusCode);
        Assert.Equal("ready", await ready.Content.ReadAsStringAsync(Ct));
        Assert.All(probes, p => Assert.Equal(1, p.Calls));
        current = null;
        using var invalid = await client.GetAsync("/health/ready", Ct);
        Assert.Equal(HttpStatusCode.ServiceUnavailable, invalid.StatusCode);
        Assert.Equal("not_ready", await invalid.Content.ReadAsStringAsync(Ct));
        current = ProfileFixture.Valid; probes[0].Status = ReadinessProbeStatus.NotReady;
        using var denied = await client.GetAsync("/api/v1/auth/login", Ct);
        Assert.Equal(HttpStatusCode.ServiceUnavailable, denied.StatusCode);
        Assert.Equal(3, loads); Assert.False(denied.Headers.Contains("X-Caller"));
    }

    [Theory]
    [InlineData("GET", "/health/live", 200, "alive")]
    [InlineData("HEAD", "/health/live", 200, "")]
    [InlineData("POST", "/health/live", 405, "")]
    [InlineData("HEAD", "/health/ready", 503, "")]
    [InlineData("POST", "/health/ready", 405, "")]
    public async Task HealthMethodBodiesAndNoStoreHaveNoBusinessAuthority(string method, string path, int status, string body)
    {
        var loads = 0;
        var boundary = new RuntimeReadinessBoundary(new(null),
            _ => { loads++; return Task.FromResult<RuntimeProfile?>(null); }, 1000);
        await using var server = await Server(boundary); using var client = server.GetTestClient();
        using var request = new HttpRequestMessage(new HttpMethod(method), path);
        using var response = await client.SendAsync(request, Ct);
        Assert.Equal(status, (int)response.StatusCode);
        Assert.Equal(body, await response.Content.ReadAsStringAsync(Ct));
        Assert.True(response.Headers.CacheControl!.NoStore);
        Assert.False(response.Headers.Contains("X-Caller"));
        Assert.Equal(path == "/health/ready" && method == "HEAD" ? 1 : 0, loads);
        if (status == 405) Assert.Equal(["GET", "HEAD"], response.Content.Headers.Allow);
    }

    [Fact]
    public async Task PrefixBoundaryDoesNotConfuseNonApiPathOrLeakSourceException()
    {
        var boundary = new RuntimeReadinessBoundary(new(null),
            _ => throw new InvalidOperationException("synthetic-private-source-detail"), 1000);
        await using var server = await Server(boundary); using var client = server.GetTestClient();
        using var nonApi = await client.GetAsync("/api/v10", Ct);
        Assert.True(nonApi.Headers.Contains("X-Caller"));
        using var failed = await client.GetAsync("/health/ready", Ct);
        Assert.Equal("not_ready", await failed.Content.ReadAsStringAsync(Ct));
        Assert.Equal(HttpStatusCode.ServiceUnavailable, failed.StatusCode);
    }

    [Fact]
    public async Task TimedOutProfileLoaderCannotEscapeGateOrLeaveUnobservedFault()
    {
        var pending = new TaskCompletionSource<RuntimeProfile?>(TaskCreationOptions.RunContinuationsAsynchronously);
        var probes = Probes();
        var boundary = new RuntimeReadinessBoundary(new(probes), _ => pending.Task, 50);
        try
        {
            Assert.Equal(ReadinessFailure.Deadline, (await boundary.EvaluateAsync(Ct)).Failure);
            Assert.All(probes, p => Assert.Equal(0, p.Calls));
        }
        finally { pending.TrySetException(new InvalidOperationException("synthetic-late-failure")); }
    }

    [Fact]
    public async Task CallerCancellationPropagatesBeforeProfileAndStartupCheckCannotBypassReadiness()
    {
        var loads = 0;
        var boundary = new RuntimeReadinessBoundary(new(null),
            _ => { loads++; return Task.FromResult<RuntimeProfile?>(null); }, 1000);
        using var canceled = CancellationTokenSource.CreateLinkedTokenSource(Ct); canceled.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => boundary.EvaluateAsync(canceled.Token));
        Assert.Equal(0, loads);
        Assert.Equal(503, (await Assert.ThrowsAsync<ApiErrorException>(() => boundary.RequireReadyAsync(Ct))).StatusCode);
    }

    [Theory]
    [InlineData("/api/v1/live", "/health/ready")]
    [InlineData("/health/live", "/HEALTH/LIVE")]
    [InlineData("/health/../live", "/health/ready")]
    [InlineData("/health/live?x", "/health/ready")]
    [InlineData("/health/live/", "/health/ready")]
    [InlineData("/health/%72eady", "/health/live")]
    public void UnsafeAmbiguousHealthPathsAreRejectedBeforeInstallation(string live, string ready)
    {
        using var services = new ServiceCollection().BuildServiceProvider();
        Assert.Throws<ArgumentException>(() => NewBoundary(Probes())
            .Install(new ApplicationBuilder(services), live, ready));
    }

    [Fact]
    public async Task HealthyApiCallsAllNineProbesFreshBeforeEveryCaller()
    {
        var probes = Probes();
        await using var server = await Server(NewBoundary(probes)); using var client = server.GetTestClient();
        for (var attempt = 0; attempt < 2; attempt++)
        {
            using var response = await client.GetAsync("/api/v1/auth/me", Ct);
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            Assert.True(response.Headers.Contains("X-Caller"));
            Assert.All(probes, p => Assert.Equal(attempt + 1, p.Calls));
        }
    }

    [Fact]
    public async Task InFlightProfileLoadPropagatesCallerCancellation()
    {
        var boundary = new RuntimeReadinessBoundary(new(Probes()), async token =>
        {
            await Task.Delay(Timeout.Infinite, token); return ProfileFixture.Valid;
        }, 1000);
        using var canceled = CancellationTokenSource.CreateLinkedTokenSource(Ct); canceled.CancelAfter(50);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => boundary.EvaluateAsync(canceled.Token));
    }

    [Theory]
    [InlineData(0)] [InlineData(-1)]
    public void ProfileLoadCannotHaveInfiniteOrZeroDeadline(int deadline)
        => Assert.Throws<ArgumentOutOfRangeException>(() => new RuntimeReadinessBoundary(new(Probes()),
            _ => Task.FromResult<RuntimeProfile?>(ProfileFixture.Valid), deadline));
    private static RuntimeReadinessBoundary NewBoundary(Probe[] probes)
        => new(new(probes), _ => Task.FromResult<RuntimeProfile?>(ProfileFixture.Valid), 1000);
    private static async Task<WebApplication> Server(RuntimeReadinessBoundary boundary)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { ContentRootPath = AppContext.BaseDirectory });
        builder.WebHost.UseTestServer();
        var app = builder.Build();
        app.UseApiErrorMiddleware();
        boundary.Install(app, "/health/live", "/health/ready");
        app.Run(context =>
        {
            context.Response.Headers["X-Caller"] = "reached";
            context.Response.StatusCode = 204; return Task.CompletedTask;
        });
        await app.StartAsync(Ct);
        return app;
    }
}
