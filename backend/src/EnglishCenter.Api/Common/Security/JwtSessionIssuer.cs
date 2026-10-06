namespace EnglishCenter.Api.Common.Security;

using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;

public interface IJwtSessionIssuer
{
    string Issue(int accountId, long securityVersion, byte[] sid, SessionIssuanceWindow window);
}
public sealed class JwtSessionIssuer : IJwtSessionIssuer
{
    private readonly SigningCredentials _credentials;
    private readonly string _issuer;
    private readonly string _audience;
    public JwtSessionIssuer(SigningCredentials credentials, string issuer, string audience)
    {
        ArgumentNullException.ThrowIfNull(credentials);
        if (string.IsNullOrWhiteSpace(issuer) || string.IsNullOrWhiteSpace(audience)) throw new ArgumentException("Missing JWT profile.");
        if (credentials.Algorithm is not (SecurityAlgorithms.HmacSha256 or SecurityAlgorithms.RsaSha256 or SecurityAlgorithms.EcdsaSha256)) throw new ArgumentException("Unsupported signing profile.");
        if (credentials.Key.KeySize < (credentials.Algorithm == SecurityAlgorithms.RsaSha256 ? 2048 : 256)) throw new ArgumentException("Invalid signing key profile.");
        if (!credentials.Key.CryptoProviderFactory.IsSupportedAlgorithm(credentials.Algorithm, credentials.Key) ||
            (credentials.Algorithm == SecurityAlgorithms.EcdsaSha256 && credentials.Key.KeySize != 256)) throw new ArgumentException("Incompatible signing profile.");
        _credentials = credentials; _issuer = issuer; _audience = audience;
    }
    public string Issue(int accountId, long securityVersion, byte[] sid, SessionIssuanceWindow window)
    {
        if (accountId <= 0 || securityVersion <= 0 || sid is null || sid.Length != 32) throw new ArgumentException("Invalid session reference.");
        ArgumentNullException.ThrowIfNull(window);
        var expires = DateTimeOffset.FromUnixTimeSeconds(window.AbsoluteExpiresAt.ToUnixTimeSeconds());
        if (expires <= window.IssuedAt) throw new ArgumentException("Session interval is not representable as NumericDate.");
        return new JwtSecurityTokenHandler { SetDefaultTimesOnTokenCreation = false }.CreateEncodedJwt(new SecurityTokenDescriptor
        {
            Issuer = _issuer,
            Audience = _audience,
            SigningCredentials = _credentials,
            IssuedAt = window.IssuedAt.UtcDateTime,
            NotBefore = window.IssuedAt.UtcDateTime,
            Expires = expires.UtcDateTime,
            Claims = new Dictionary<string, object> { ["sub"] = accountId.ToString(CultureInfo.InvariantCulture), ["sid"] = Base64UrlEncoder.Encode(sid), ["ver"] = securityVersion.ToString(CultureInfo.InvariantCulture), ["scope"] = window.Scope }
        });
    }
}
