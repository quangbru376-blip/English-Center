namespace EnglishCenter.Tests.Infrastructure;

using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

public class StartupTests
{
    [Theory]
    [InlineData("Development", "/")]
    [InlineData("Development", "/auth")]
    [InlineData("Development", "/api/auth/login")]
    [InlineData("Development", "/health")]
    [InlineData("Development", "/health/ready")]
    [InlineData("Production", "/")]
    [InlineData("Production", "/auth")]
    [InlineData("Production", "/api/auth/login")]
    [InlineData("Production", "/health")]
    [InlineData("Production", "/health/ready")]
    public async Task UnimplementedPath_ReturnsNotFoundWithEmptyBody_InGivenEnvironment(string environment, string path)
    {
        using var rootFactory = new WebApplicationFactory<Program>();
        using var factory = rootFactory.WithWebHostBuilder(builder => builder.UseEnvironment(environment));
        using var client = factory.CreateClient();

        using var response = await client.GetAsync(path, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.Empty(content);
    }
}
