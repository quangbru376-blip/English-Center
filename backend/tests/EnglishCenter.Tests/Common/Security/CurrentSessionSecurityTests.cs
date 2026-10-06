namespace EnglishCenter.Tests.Common.Security;

using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using EnglishCenter.Api.Common.Security;
using EnglishCenter.Api.Common.Time;
using Microsoft.IdentityModel.Tokens;
using Xunit;

public class CurrentSessionSecurityTests
{
    private static readonly DateTimeOffset At = new(2026, 10, 6, 0, 0, 0, TimeSpan.Zero);
    private sealed class Clock : TimeProvider
    { internal DateTimeOffset Now = At; public override DateTimeOffset GetUtcNow() => Now; }
    private static Dictionary<string, object> Claims() => new() { ["sub"] = "7", ["ver"] = "1", ["sid"] = Base64UrlEncoder.Encode(RandomNumberGenerator.GetBytes(32)), ["scope"] = "NORMAL", ["iat"] = At.ToUnixTimeSeconds(), ["nbf"] = At.ToUnixTimeSeconds(), ["exp"] = At.AddHours(2).ToUnixTimeSeconds(), ["iss"] = "fixture-issuer", ["aud"] = "fixture-audience" };
    private static string Signed(string payload, byte[] key, string algorithm = "HS256")
    {
        var header = Base64UrlEncoder.Encode(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new { alg = algorithm, typ = "JWT" })));
        var body = header + "." + Base64UrlEncoder.Encode(Encoding.UTF8.GetBytes(payload));
        var signature = algorithm == "HS512" ? HMACSHA512.HashData(key, Encoding.ASCII.GetBytes(body)) : HMACSHA256.HashData(key, Encoding.ASCII.GetBytes(body));
        return body + "." + Base64UrlEncoder.Encode(signature);
    }
    private static JwtSessionValidator Validator(byte[] key, Clock? clock = null, int max = 4096) => new([new SymmetricSecurityKey(key)], [SecurityAlgorithms.HmacSha256], "fixture-issuer", "fixture-audience", new BusinessClock(clock ?? new()), max);
    [Theory]
    [InlineData("HS256")]
    [InlineData("RS256")]
    [InlineData("ES256")]
    public void ApprovedAlgorithmsVerifyWithoutRoleSnapshotAndSidCopiesDoNotMutateProof(string algorithm)
    {
        using var rsa = RSA.Create(2048); using var ec = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        SecurityKey key = algorithm == "HS256" ? new SymmetricSecurityKey(RandomNumberGenerator.GetBytes(32)) : algorithm == "RS256" ? new RsaSecurityKey(rsa) : new ECDsaSecurityKey(ec);
        var credentials = new SigningCredentials(key, algorithm); var now = At.AddTicks(12345);
        var encoded = new JwtSessionIssuer(credentials, "fixture-issuer", "fixture-audience").Issue(7, 1, RandomNumberGenerator.GetBytes(32), SessionIssuanceWindow.Normal(now));
        var validator = new JwtSessionValidator([key], [algorithm], "fixture-issuer", "fixture-audience", new(new Clock { Now = now }), 4096);
        var token = validator.Validate(encoded); Assert.NotNull(token); Assert.Equal(7, token.AccountId); Assert.Equal("NORMAL", token.Scope); Assert.Equal(nameof(VerifiedSessionToken), token.ToString());
        var copied = token.CopySid(); copied[0] ^= 255; Assert.False(copied.SequenceEqual(token.CopySid()));
        Assert.Null(validator.Validate(encoded + "x"));
    }
    [Theory]
    [InlineData("sub", "0")]
    [InlineData("sub", "07")]
    [InlineData("sub", "7x")]
    [InlineData("ver", "0")]
    [InlineData("ver", "01")]
    [InlineData("ver", "9223372036854775808")]
    [InlineData("scope", "normal")]
    [InlineData("scope", "OTHER")]
    [InlineData("sid", "bad")]
    [InlineData("iss", "other")]
    [InlineData("aud", "other")]
    public void SignedInvalidSelectorsAndIssuerAudienceNeverBecomeIdentity(string field, string value)
    {
        var key = RandomNumberGenerator.GetBytes(32); var claims = Claims(); claims[field] = value;
        Assert.Null(Validator(key).Validate(Signed(JsonSerializer.Serialize(claims), key)));
    }
    [Theory]
    [InlineData("sub")]
    [InlineData("ver")]
    [InlineData("sid")]
    [InlineData("scope")]
    [InlineData("iat")]
    [InlineData("nbf")]
    [InlineData("exp")]
    public void SignedMissingRequiredClaimsFailClosed(string field)
    {
        var key = RandomNumberGenerator.GetBytes(32); var claims = Claims(); claims.Remove(field);
        Assert.Null(Validator(key).Validate(Signed(JsonSerializer.Serialize(claims), key)));
    }
    [Fact]
    public void DuplicateTypedClaimsFutureNotBeforeExpiryAndAlgorithmConfusionFailClosed()
    {
        var key = RandomNumberGenerator.GetBytes(64); var claims = Claims(); var validator = Validator(key);
        var valid = JsonSerializer.Serialize(claims); Assert.Null(validator.Validate(Signed(valid[..^1] + ",\"sub\":\"8\"}", key)));
        claims["sub"] = 7; Assert.Null(validator.Validate(Signed(JsonSerializer.Serialize(claims), key))); claims["sub"] = "7";
        claims["iat"] = At.AddSeconds(1).ToUnixTimeSeconds(); claims["nbf"] = claims["iat"]; Assert.Null(validator.Validate(Signed(JsonSerializer.Serialize(claims), key)));
        claims = Claims(); claims["exp"] = At.ToUnixTimeSeconds(); Assert.Null(validator.Validate(Signed(JsonSerializer.Serialize(claims), key)));
        claims = Claims(); claims["iat"] = At.AddSeconds(-1).ToUnixTimeSeconds(); Assert.Null(validator.Validate(Signed(JsonSerializer.Serialize(claims), key)));
        Assert.Null(validator.Validate(Signed(valid, key, "HS512")));
        Assert.Null(Validator(RandomNumberGenerator.GetBytes(32)).Validate(Signed(valid, key)));
        Assert.Null(Validator(key, max: 128).Validate(Signed(valid, key)));
    }
    [Fact]
    public void JwtLifetimeUsesRawClockWithNoDefaultSkewAtExactSecondDeadline()
    {
        var key = RandomNumberGenerator.GetBytes(32); var clock = new Clock(); var claims = Claims(); claims["exp"] = At.AddSeconds(1).ToUnixTimeSeconds(); var encoded = Signed(JsonSerializer.Serialize(claims), key); var validator = Validator(key, clock);
        clock.Now = At.AddSeconds(1).AddTicks(-1); Assert.NotNull(validator.Validate(encoded)); clock.Now = clock.Now.AddTicks(1); Assert.Null(validator.Validate(encoded));
    }
    [Fact]
    public void EffectiveDeadlinesPreserveTicksAndUseAllIndependentLimitsWithoutOverflow()
    {
        var issued = At.AddTicks(12345); var activity = issued.AddMinutes(90); var jwt = issued.AddHours(2).AddTicks(-3);
        Assert.Equal(jwt, SessionDeadlineMath.Normal(issued.ToOffset(TimeSpan.FromHours(7)), issued.AddHours(3), activity, jwt));
        Assert.Equal(issued.AddMinutes(30), SessionDeadlineMath.Normal(issued, issued.AddHours(2), issued, issued.AddHours(2)));
        Assert.Equal(issued.AddSeconds(45), SessionDeadlineMath.Restricted(issued, issued.AddMinutes(10), issued.AddMinutes(10), issued.AddSeconds(45)));
        Assert.Equal(issued.AddMinutes(10), SessionDeadlineMath.Restricted(issued, issued.AddHours(1), issued.AddHours(1), issued.AddHours(2)));
        Assert.Null(SessionDeadlineMath.Normal(issued, issued, issued, jwt)); Assert.Null(SessionDeadlineMath.Normal(issued, issued.AddHours(2), issued.AddTicks(-1), jwt));
        Assert.Null(SessionDeadlineMath.Normal(issued, issued.AddHours(2), issued.AddHours(3), jwt)); Assert.Null(SessionDeadlineMath.Restricted(issued, issued.AddHours(1), jwt, issued));
        var near = DateTimeOffset.MaxValue.AddSeconds(-1); Assert.Null(SessionDeadlineMath.Normal(near, DateTimeOffset.MaxValue, near, DateTimeOffset.MaxValue)); Assert.Null(SessionDeadlineMath.Restricted(near, DateTimeOffset.MaxValue, DateTimeOffset.MaxValue, DateTimeOffset.MaxValue));
    }
    [Fact]
    public void MissingWeakOrUnsupportedValidationProfileCannotStart()
    {
        var key = new SymmetricSecurityKey(RandomNumberGenerator.GetBytes(32)); var clock = new BusinessClock(new Clock());
        Assert.Throws<ArgumentException>(() => new JwtSessionValidator([], [], "fixture", "fixture", clock, 4096));
        Assert.Throws<ArgumentException>(() => new JwtSessionValidator([new SymmetricSecurityKey(RandomNumberGenerator.GetBytes(16))], ["HS256"], "fixture", "fixture", clock, 4096));
        Assert.Throws<ArgumentException>(() => new JwtSessionValidator([key], ["HS512"], "fixture", "fixture", clock, 4096));
        Assert.Throws<ArgumentException>(() => new JwtSessionValidator([key], ["HS256"], "", "fixture", clock, 4096));
    }
}
