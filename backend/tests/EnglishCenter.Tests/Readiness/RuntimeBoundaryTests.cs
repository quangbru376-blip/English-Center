using EnglishCenter.Api.Api.Runtime;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Infrastructure.Readiness;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EnglishCenter.Tests.Readiness;

public sealed class RuntimeBoundaryTests
{
    [Theory]
    [InlineData(true)] [InlineData(false)]
    public async Task OversizedBodyNeverReachesCallerAndRestoresContext(bool knownLength)
    {
        using var services = new ServiceCollection().BuildServiceProvider();
        var app = new ApplicationBuilder(services);
        var profile = ProfileFixture.Valid with { Execution = ProfileFixture.Valid.Execution! with { RequestBodyBytes = 4 } };
        ExecutionBoundaryBinding.Install(app, profile);
        var reached = false;
        app.Run(_ => { reached = true; return Task.CompletedTask; });
        using var source = new MemoryStream(new byte[5]);
        var context = new DefaultHttpContext(); context.Request.Body = source;
        context.Request.ContentLength = knownLength ? 5 : null;
        var token = TestContext.Current.CancellationToken; context.RequestAborted = token;
        var error = await Assert.ThrowsAsync<ApiErrorException>(() => app.Build()(context));
        Assert.Equal(413, error.StatusCode); Assert.False(reached);
        Assert.Same(source, context.Request.Body); Assert.Equal(token, context.RequestAborted);
    }

    [Fact]
    public async Task AcceptedBodyIsRewoundAndSensitiveBufferIsClearedAfterCaller()
    {
        using var services = new ServiceCollection().BuildServiceProvider();
        var app = new ApplicationBuilder(services);
        ExecutionBoundaryBinding.Install(app, ProfileFixture.Valid);
        byte[]? retained = null;
        app.Run(async context =>
        {
            var body = Assert.IsType<MemoryStream>(context.Request.Body);
            Assert.Equal(0, body.Position);
            retained = body.GetBuffer();
            Assert.Equal(3, await body.ReadAsync(new byte[3], context.RequestAborted));
        });
        using var source = new MemoryStream([1, 2, 3]);
        var context = new DefaultHttpContext(); context.Request.Body = source;
        context.RequestAborted = TestContext.Current.CancellationToken;
        await app.Build()(context);
        Assert.NotNull(retained); Assert.All(retained, item => Assert.Equal((byte)0, item));
        Assert.Same(source, context.Request.Body);
    }

    [Fact]
    public async Task CooperativeDeadlineMapsTo503AndRestoresCallerToken()
    {
        using var services = new ServiceCollection().BuildServiceProvider();
        var app = new ApplicationBuilder(services);
        var profile = ProfileFixture.Valid with { Execution = ProfileFixture.Valid.Execution! with
            { RequestMs = 1100, CsvMs = 1100 } };
        ExecutionBoundaryBinding.Install(app, profile);
        app.Run(context => Task.Delay(5000, context.RequestAborted));
        var context = new DefaultHttpContext(); var token = TestContext.Current.CancellationToken;
        context.RequestAborted = token;
        var error = await Assert.ThrowsAsync<ApiErrorException>(() => app.Build()(context));
        Assert.Equal(503, error.StatusCode); Assert.Equal(token, context.RequestAborted);
    }

    [Fact]
    public async Task CallerCancellationIsNeverReportedAsServiceDeadline()
    {
        using var services = new ServiceCollection().BuildServiceProvider();
        var app = new ApplicationBuilder(services);
        ExecutionBoundaryBinding.Install(app, ProfileFixture.Valid);
        app.Run(context => Task.Delay(5000, context.RequestAborted));
        using var canceled = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        canceled.Cancel();
        var context = new DefaultHttpContext(); context.RequestAborted = canceled.Token;
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => app.Build()(context));
        Assert.Equal(canceled.Token, context.RequestAborted);
    }

    [Fact]
    public async Task ReadinessRequiresActualInstalledMatchingBoundary()
    {
        var profile = ProfileFixture.Valid; var token = TestContext.Current.CancellationToken;
        Assert.Equal(ReadinessProbeStatus.NotReady, await new ExecutionLimitsReadinessProbe(null).CheckAsync(profile, token));
        using var services = new ServiceCollection().BuildServiceProvider();
        var installed = ExecutionBoundaryBinding.Install(new ApplicationBuilder(services), profile);
        var probe = new ExecutionLimitsReadinessProbe(installed);
        Assert.Equal(ReadinessProbeStatus.Pass, await probe.CheckAsync(profile, token));
        Assert.Equal(ReadinessProbeStatus.NotReady, await probe.CheckAsync(profile with
            { Execution = profile.Execution! with { RequestBodyBytes = 11 } }, token));
    }

    [Fact]
    public void ResponseOverflowCannotExposePartialSuccess()
    {
        using var buffer = new BoundedResponseBuffer(4);
        buffer.Append([1, 2, 3, 4]);
        Assert.Equal(new byte[] { 1, 2, 3, 4 }, buffer.ToArray());
        Assert.Equal(503, Assert.Throws<ApiErrorException>(() => buffer.Append([5])).StatusCode);
        Assert.Equal(4, buffer.ToArray().Length);
        Assert.Throws<ArgumentOutOfRangeException>(() => new BoundedResponseBuffer(0));
    }
}
