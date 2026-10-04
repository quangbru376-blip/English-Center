namespace EnglishCenter.Tests.Common.Errors;

using System.Text.Json;
using EnglishCenter.Api.Common.Errors;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

public class ApiErrorHostingTests
{
    [Theory]
    [InlineData("Development","/test/validation",400,"VALIDATION_ERROR")]
    [InlineData("Production","/test/validation",400,"VALIDATION_ERROR")]
    [InlineData("Development","/test/unexpected",500,"INTERNAL_ERROR")]
    [InlineData("Production","/test/unexpected",500,"INTERNAL_ERROR")]
    public async Task ActualProgram_HandlesTestHostErrors(string environment,string path,int status,string code)
    {
        using var root=new WebApplicationFactory<Program>();
        using var factory=root.WithWebHostBuilder(b=>b.UseEnvironment(environment).ConfigureServices(s=>s.AddSingleton<IStartupFilter,TestErrors>()));
        using var client=factory.CreateClient();
        using var response=await client.GetAsync(path,TestContext.Current.CancellationToken);
        Assert.Equal(status,(int)response.StatusCode);
        var body=await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        using var json=JsonDocument.Parse(body);
        Assert.Equal(code,json.RootElement.GetProperty("code").GetString());
        Assert.DoesNotContain("PRIVATE_INPUT",body);
        Assert.Equal("application/json",response.Content.Headers.ContentType!.MediaType);
        if(status==400) Assert.True(json.RootElement.GetProperty("fieldErrors").TryGetProperty("name",out _));
    }
    private sealed class TestErrors : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)=> app=>
        {
            next(app);
            app.Use(async(context,following)=>
            {
                if(context.Request.Path=="/test/validation") throw ApiErrorException.Validation("name");
                if(context.Request.Path=="/test/unexpected") throw new InvalidOperationException("PRIVATE_INPUT");
                await following(context);
            });
        };
    }
}
