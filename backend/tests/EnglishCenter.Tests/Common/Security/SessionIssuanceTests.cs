namespace EnglishCenter.Tests.Common.Security;

using System.Net;
using System.Security.Cryptography;
using System.IdentityModel.Tokens.Jwt;
using EnglishCenter.Api.Common.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Xunit;

public class SessionIssuanceTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 6, 0, 0, 0, TimeSpan.Zero);
    [Fact]
    public void NormalAndRestrictedInitialDeadlinesPreserveUtcAndPrecision()
    {
        var offset = Now.ToOffset(TimeSpan.FromHours(7)).AddTicks(12345); var window = SessionIssuanceWindow.Normal(offset);
        Assert.Equal(TimeSpan.Zero, window.IssuedAt.Offset); Assert.Equal(Now.AddMilliseconds(1), window.IssuedAt);
        Assert.Equal(window.IssuedAt.AddHours(2), window.AbsoluteExpiresAt); Assert.Equal(window.IssuedAt.AddMinutes(30), window.EffectiveExpiresAt);
        var restricted = SessionIssuanceWindow.Restricted(offset, Now.AddMinutes(24)); Assert.Equal("PASSWORD_CHANGE", restricted.Scope);
        Assert.Equal(window.IssuedAt.AddMinutes(10), restricted.AbsoluteExpiresAt); Assert.Equal(restricted.AbsoluteExpiresAt, restricted.EffectiveExpiresAt);
        var near = SessionIssuanceWindow.Restricted(offset, Now.AddSeconds(2)); Assert.Equal(Now.AddSeconds(2), near.AbsoluteExpiresAt);
    }
    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(1)]
    public void ExpiredOrUnrepresentableSubmillisecondWindowFailsClosed(int ticks)
    {
        Assert.Throws<ArgumentException>(() => SessionIssuanceWindow.Restricted(Now, Now.AddTicks(ticks)));
    }
    [Fact]
    public void DateOverflowIsRejectedRatherThanClipped()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => SessionIssuanceWindow.Normal(DateTimeOffset.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => SessionIssuanceWindow.Restricted(DateTimeOffset.MaxValue.AddMinutes(-1), DateTimeOffset.MaxValue));
    }
    [Theory]
    [InlineData("HS256", 32)]
    [InlineData("RS256", 0)]
    [InlineData("ES256", 0)]
    public void RealSignatureAudienceIssuerAndExactPrivateClaimsAreVerified(string algorithm, int bytes)
    {
        using var rsa = RSA.Create(2048); using var ec = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        SecurityKey key = algorithm switch { "RS256" => new RsaSecurityKey(rsa), "ES256" => new ECDsaSecurityKey(ec), _ => new SymmetricSecurityKey(RandomNumberGenerator.GetBytes(bytes)) };
        var issuer = new JwtSessionIssuer(new SigningCredentials(key, algorithm), "test-issuer", "test-audience"); var sid = RandomNumberGenerator.GetBytes(32); var window = SessionIssuanceWindow.Normal(Now.AddMilliseconds(123));
        var token = issuer.Issue(42, 9, sid, window); var handler = new JwtSecurityTokenHandler { MapInboundClaims = false };
        var parameters = new TokenValidationParameters { ValidateIssuerSigningKey = true, IssuerSigningKey = key, ValidAlgorithms = [algorithm], ValidateIssuer = true, ValidIssuer = "test-issuer", ValidateAudience = true, ValidAudience = "test-audience", ValidateLifetime = true, ClockSkew = TimeSpan.Zero, LifetimeValidator = (start, end, _, _) => start <= Now.AddSeconds(1).UtcDateTime && Now.AddSeconds(1).UtcDateTime < end };
        var principal = handler.ValidateToken(token, parameters, out var verified);
        Assert.Equal("42", principal.FindFirst("sub")?.Value); Assert.Equal("9", principal.FindFirst("ver")?.Value); Assert.True(principal.FindFirst("sid")?.Value == Base64UrlEncoder.Encode(sid));
        Assert.Equal("NORMAL", principal.FindFirst("scope")?.Value); Assert.Null(principal.FindFirst("email")); Assert.Null(principal.FindFirst("role"));
        Assert.True(verified.ValidTo <= window.AbsoluteExpiresAt.UtcDateTime); Assert.Equal(window.AbsoluteExpiresAt.ToUnixTimeSeconds(), new DateTimeOffset(verified.ValidTo).ToUnixTimeSeconds());
        parameters.ValidAudience = "wrong"; Assert.ThrowsAny<SecurityTokenException>(() => handler.ValidateToken(token, parameters, out _));
        parameters.ValidAudience = "test-audience"; parameters.ValidIssuer = "wrong"; Assert.ThrowsAny<SecurityTokenException>(() => handler.ValidateToken(token, parameters, out _));
    }
    [Fact]
    public void MissingOrWeakOrUnsupportedSigningProfileCannotIssue()
    {
        var good = new SigningCredentials(new SymmetricSecurityKey(RandomNumberGenerator.GetBytes(32)), SecurityAlgorithms.HmacSha256);
        Assert.Throws<ArgumentException>(() => new JwtSessionIssuer(good, "", "audience"));
        Assert.Throws<ArgumentException>(() => new JwtSessionIssuer(new SigningCredentials(new SymmetricSecurityKey(RandomNumberGenerator.GetBytes(16)), SecurityAlgorithms.HmacSha256), "issuer", "audience"));
        Assert.Throws<ArgumentException>(() => new JwtSessionIssuer(new SigningCredentials(good.Key, SecurityAlgorithms.HmacSha512), "issuer", "audience"));
    }
    [Fact]
    public void IpLimitIsIndependentAndMappedAddressCannotBypass100()
    {
        using var limiter = new LoginIpLimiter(); var ip = IPAddress.Parse("192.0.2.1");
        for (var i = 0; i < 100; i++) Assert.True(limiter.TryAcquire(ip, out _));
        Assert.False(limiter.TryAcquire(ip.MapToIPv6(), out var retry)); Assert.InRange(retry, 1, 60);
        Assert.True(limiter.TryAcquire(IPAddress.Parse("192.0.2.2"), out _));
    }
    [Fact]
    public void RehashVerifiedLegacyCredentialDoesNotReapplyNewPasswordPolicy()
    {
        var policy = new PasswordPolicy(OfflinePasswordBlocklist.Load(Path.Combine(AppContext.BaseDirectory, "Resources", "PasswordBlocklist"))); var verifier = new CredentialHasher(policy);
        var raw = Convert.ToBase64String(RandomNumberGenerator.GetBytes(24))[..5];
        var legacy = new PasswordHasher<object>(Options.Create(new PasswordHasherOptions { CompatibilityMode = PasswordHasherCompatibilityMode.IdentityV3, IterationCount = 10000 })).HashPassword(new object(), raw);
        Assert.True(verifier.Verify(legacy, raw) == PasswordVerificationResult.SuccessRehashNeeded);
        Assert.Throws<EnglishCenter.Api.Common.Errors.ApiErrorException>(() => verifier.Hash(raw));
        var current = verifier.RehashVerified(legacy, raw); Assert.True(verifier.Verify(current, raw) == PasswordVerificationResult.Success);
        Assert.Throws<ArgumentException>(() => verifier.RehashVerified(current, raw));
    }
}
