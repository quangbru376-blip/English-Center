namespace EnglishCenter.Api.Common.Security;

using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.Json;
using EnglishCenter.Api.Common.Time;
using Microsoft.IdentityModel.Tokens;

public sealed class JwtSessionValidator
{
    private readonly TokenValidationParameters _parameters;
    private readonly BusinessClock _clock;
    private readonly int _maxBytes;
    public JwtSessionValidator(IEnumerable<SecurityKey> keys, IEnumerable<string> algorithms, string issuer, string audience, BusinessClock clock, int maxTokenBytes)
    {
        ArgumentNullException.ThrowIfNull(keys); ArgumentNullException.ThrowIfNull(algorithms); ArgumentNullException.ThrowIfNull(clock);
        var keySet = keys.ToArray(); var algSet = algorithms.Distinct(StringComparer.Ordinal).ToArray();
        if (keySet.Length == 0 || algSet.Length == 0 || maxTokenBytes <= 0 || string.IsNullOrWhiteSpace(issuer) || string.IsNullOrWhiteSpace(audience)) throw new ArgumentException("Missing JWT validation profile.");
        if (algSet.Any(a => a is not (SecurityAlgorithms.HmacSha256 or SecurityAlgorithms.RsaSha256 or SecurityAlgorithms.EcdsaSha256)) ||
            keySet.Any(k => k is null || !algSet.Any(a => Compatible(k, a))) || algSet.Any(a => !keySet.Any(k => Compatible(k, a)))) throw new ArgumentException("Incompatible JWT validation profile.");
        _clock = clock; _maxBytes = maxTokenBytes;
        _parameters = new TokenValidationParameters
        {
            IssuerSigningKeys = keySet,
            ValidAlgorithms = algSet,
            ValidIssuer = issuer,
            ValidAudience = audience,
            ValidateIssuer = true,
            ValidateAudience = true,
            RequireAudience = true,
            RequireSignedTokens = true,
            ValidateIssuerSigningKey = true,
            RequireExpirationTime = true,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            ValidTypes = ["JWT"],
            IncludeTokenOnFailedValidation = false,
            LogValidationExceptions = false,
            LifetimeValidator = (notBefore, expires, _, _) => notBefore is not null && expires is not null &&
                !_clock.IsBefore(Utc(notBefore.Value)) && _clock.IsBefore(Utc(expires.Value))
        };
    }
    private static bool Compatible(SecurityKey key, string algorithm) =>
        key.CryptoProviderFactory.IsSupportedAlgorithm(algorithm, key) && key.KeySize >= (algorithm == SecurityAlgorithms.RsaSha256 ? 2048 : 256) &&
        (algorithm != SecurityAlgorithms.EcdsaSha256 || key.KeySize == 256);
    public VerifiedSessionToken? Validate(string? encoded)
    {
        if (string.IsNullOrEmpty(encoded) || Encoding.UTF8.GetByteCount(encoded) > _maxBytes) return null;
        try
        {
            var handler = new JwtSecurityTokenHandler { MapInboundClaims = false, MaximumTokenSizeInBytes = _maxBytes };
            handler.ValidateToken(encoded, _parameters, out var validated);
            if (validated is not JwtSecurityToken jwt) return null;
            using var body = JsonDocument.Parse(Base64UrlEncoder.DecodeBytes(jwt.RawPayload));
            var properties = body.RootElement.EnumerateObject().ToArray();
            if (properties.Select(p => p.Name).Distinct(StringComparer.Ordinal).Count() != properties.Length) return null;
            var sub = String(body.RootElement, "sub"); var ver = String(body.RootElement, "ver"); var scope = String(body.RootElement, "scope"); var sidText = String(body.RootElement, "sid");
            if (!int.TryParse(sub, NumberStyles.None, CultureInfo.InvariantCulture, out var accountId) || accountId <= 0 || sub != accountId.ToString(CultureInfo.InvariantCulture) ||
                !long.TryParse(ver, NumberStyles.None, CultureInfo.InvariantCulture, out var version) || version <= 0 || ver != version.ToString(CultureInfo.InvariantCulture) ||
                scope is not ("NORMAL" or "PASSWORD_CHANGE") || sidText is null) return null;
            var sid = Base64UrlEncoder.DecodeBytes(sidText); if (sid.Length != 32 || Base64UrlEncoder.Encode(sid) != sidText) return null;
            if (!NumericDate(body.RootElement, "iat", out var issued) || !NumericDate(body.RootElement, "nbf", out var nbf) || !NumericDate(body.RootElement, "exp", out var expiry) ||
                issued != nbf || expiry <= issued || _clock.IsBefore(issued) || !_clock.IsBefore(expiry)) return null;
            return new(accountId, version, scope, sid, issued, expiry);
        }
        catch (SecurityTokenException) { return null; }
        catch (ArgumentException) { return null; }
        catch (JsonException) { return null; }
        catch (FormatException) { return null; }
    }
    private static string? String(JsonElement body, string name) => body.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String ? value.GetString() : null;
    private static bool NumericDate(JsonElement body, string name, out DateTimeOffset instant)
    {
        instant = default;
        if (!body.TryGetProperty(name, out var value) || value.ValueKind != JsonValueKind.Number || !value.TryGetInt64(out var seconds)) return false;
        try { instant = DateTimeOffset.FromUnixTimeSeconds(seconds); return true; } catch (ArgumentOutOfRangeException) { return false; }
    }
    private static DateTimeOffset Utc(DateTime value) => new(DateTime.SpecifyKind(value, DateTimeKind.Utc));
}
