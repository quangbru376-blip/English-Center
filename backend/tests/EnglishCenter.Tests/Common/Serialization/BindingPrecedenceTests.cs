namespace EnglishCenter.Tests.Common.Serialization;

using System.Net.Http;
using System.Text;
using System.Text.Json;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Common.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

public class BindingPrecedenceTests
{
    [Theory]
    [InlineData("auth",401,"AUTHENTICATION_REQUIRED",false)]
    [InlineData("scope",403,"FORBIDDEN",false)]
    [InlineData("csrf",403,"CSRF_INVALID",false)]
    [InlineData("allowed",400,"VALIDATION_ERROR",true)]
    public async Task GuardFailures_PrecedeMalformedBinding(string guard,int status,string code,bool bound)
    {
        var filter=new Guards(guard);
        using var root=new WebApplicationFactory<Program>();
        using var factory=root.WithWebHostBuilder(b=>b.UseEnvironment("Production").ConfigureServices(s=>s.AddSingleton<IStartupFilter>(filter)));
        using var client=factory.CreateClient();
        using var content=new StringContent("{PRIVATE_INPUT",Encoding.UTF8,"application/json");
        using var response=await client.PostAsync("/test/bind",content,TestContext.Current.CancellationToken);
        Assert.Equal(status,(int)response.StatusCode);
        Assert.Equal(bound,filter.BindingReached);
        var body=await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        using var json=JsonDocument.Parse(body);
        Assert.Equal(code,json.RootElement.GetProperty("code").GetString());
        Assert.Empty(json.RootElement.GetProperty("fieldErrors").EnumerateObject());
        Assert.DoesNotContain("PRIVATE_INPUT",body);
    }
    private sealed class Guards(string guard) : IStartupFilter
    {
        public bool BindingReached {get;private set;}
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)=>app=>
        {
            next(app);
            app.Use(async(context,following)=>
            {
                if(context.Request.Path!="/test/bind") {await following(context);return;}
                if(guard=="auth") throw new ApiErrorException(ApiErrorCode.AUTHENTICATION_REQUIRED);
                if(guard=="scope") throw new ApiErrorException(ApiErrorCode.FORBIDDEN);
                if(guard=="csrf") throw new ApiErrorException(ApiErrorCode.CSRF_INVALID);
                BindingReached=true;
                using var reader=new StreamReader(context.Request.Body);
                var body=await reader.ReadToEndAsync(context.RequestAborted);
                StrictJsonObject.Parse(body,new[]{"name"},new[]{"name"});
            });
        };
    }
}
