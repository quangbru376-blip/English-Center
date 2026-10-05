namespace EnglishCenter.Tests.Common.Security;

using EnglishCenter.Api.Common.Security;
using Xunit;

public class SameOriginTests
{
    [Theory]
    [InlineData("https://ec.example.invalid",true)] [InlineData("https://ec.example.invalid:8443",true)]
    [InlineData("http://ec.example.invalid",false)] [InlineData("https://ec.example.invalid/",false)]
    [InlineData("https://ec.example.invalid:443",false)] [InlineData("https://EC.example.invalid",false)]
    [InlineData("https://ec.example.invalid/path",false)] [InlineData("https://ec.example.invalid?x=1",false)]
    [InlineData("https://user@ec.example.invalid",false)] [InlineData("https://*.example.invalid",false)]
    [InlineData("https://é.example.invalid",false)] [InlineData("null",false)] [InlineData("https://ec.example.invalid https://evil.invalid",false)]
    public void OriginHasNoAmbientHeaderOrCanonicalizationFallback(string input,bool valid) => Assert.Equal(valid,SameOrigin.IsCanonicalHttpsOrigin(input));
    [Fact]
    public void ExactSingleOriginRequiredAndJwtCookieFlagsAreSafe()
    {
        const string origin="https://ec.example.invalid";
        Assert.True(SameOrigin.Matches(origin,[origin]));Assert.False(SameOrigin.Matches(origin,[]));
        Assert.False(SameOrigin.Matches(origin,[origin,origin]));Assert.False(SameOrigin.Matches(origin,["https://evil.invalid"]));
        var cookie=SafeCookiePolicy.AuthOptions();Assert.True(cookie.HttpOnly);Assert.True(cookie.Secure);
        Assert.Equal(Microsoft.AspNetCore.Http.SameSiteMode.Strict,cookie.SameSite);Assert.Null(cookie.Domain);Assert.Equal("/",cookie.Path);
    }
}
