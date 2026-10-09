using System.Net;
using System.Text.Json;
using EnglishCenter.Api.Infrastructure.Operator;
using Xunit;

namespace EnglishCenter.Tests.Readiness;

public sealed class ActualM0RuntimeHostTests
{
    [Fact(Skip = "Actual all-nine HTTPS host opt-in", SkipUnless = nameof(M0MaintenanceProofGate.RuntimeLive), SkipType = typeof(M0MaintenanceProofGate))]
    public async Task RealHttpsHostChecksAllNineAndServesOnlyBuiltUiReadOnly()
    {
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken); deadline.CancelAfter(15000);
        await using var host = await LocalRuntimeHost.CreateAsync(deadline.Token);
        Assert.NotNull(host);
        await host!.Application.StartAsync(deadline.Token);
        try
        {
            using var client = new HttpClient(new HttpClientHandler { UseCookies = true })
            { BaseAddress = new("https://localhost:8443"), Timeout = TimeSpan.FromSeconds(8) };
            using var ready = await client.GetAsync("/health/ready", deadline.Token);
            Assert.Equal(HttpStatusCode.OK, ready.StatusCode);
            Assert.Equal("ready", await ready.Content.ReadAsStringAsync(deadline.Token));
            Assert.True(ready.Headers.CacheControl?.NoStore == true);
            Assert.True((await host.Readiness.EvaluateAsync(deadline.Token)).IsReady);
            using var live = await client.GetAsync("/health/live", deadline.Token);
            Assert.Equal("alive", await live.Content.ReadAsStringAsync(deadline.Token));
            using var csrf = await client.GetAsync("/api/v1/auth/csrf", deadline.Token);
            Assert.Equal(HttpStatusCode.OK, csrf.StatusCode);
            using var body = JsonDocument.Parse(await csrf.Content.ReadAsStringAsync(deadline.Token));
            Assert.True(body.RootElement.GetProperty("requestToken").GetString()?.Length > 0, "Missing request token");
            Assert.True(csrf.Headers.GetValues("Set-Cookie").Any(cookie => cookie.Contains("secure", StringComparison.OrdinalIgnoreCase) &&
                cookie.Contains("httponly", StringComparison.OrdinalIgnoreCase) && cookie.Contains("samesite=strict", StringComparison.OrdinalIgnoreCase)), "Invalid CSRF cookie policy");
            using var html = await client.GetAsync("/login", deadline.Token);
            Assert.Equal(HttpStatusCode.OK, html.StatusCode); Assert.Equal("text/html", html.Content.Headers.ContentType?.MediaType);
            Assert.True(html.Headers.CacheControl?.NoStore == true);
            using var sourceAttempt = await client.GetAsync("/docs/PROJECT_CONTEXT.md", deadline.Token);
            Assert.Equal(HttpStatusCode.NotFound, sourceAttempt.StatusCode);
            Assert.True((await sourceAttempt.Content.ReadAsStringAsync(deadline.Token)).Length == 0, "Source path must not be served");
            using var unknownApi = await client.GetAsync("/api/v1/unmounted", deadline.Token);
            Assert.Equal(HttpStatusCode.ServiceUnavailable, unknownApi.StatusCode);
            Assert.Equal("application/json", unknownApi.Content.Headers.ContentType?.MediaType);
            using var invalid = await client.GetAsync("/api/v1/auth/csrf?extra=1", deadline.Token);
            Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);
            Assert.False(host.Startup.HasRefusedStartup);
        }
        finally { await host.Application.StopAsync(CancellationToken.None); }
    }
}
