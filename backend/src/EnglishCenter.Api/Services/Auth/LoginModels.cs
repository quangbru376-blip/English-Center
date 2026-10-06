namespace EnglishCenter.Api.Services.Auth;

using System.Text.Json.Serialization;
using EnglishCenter.Api.Common.Serialization;
using EnglishCenter.Api.Common.Validation;

public sealed class LoginInput
{
    public string Email { get; }
    public string Password { get; }
    private LoginInput(string email, string password) { Email = email; Password = password; }
    public static LoginInput Parse(string json)
    {
        var body = StrictJsonObject.Parse(json, ["email", "password"], ["email", "password"]);
        return new(body.ReadParsed("email", ContactNormalizer.NormalizeEmail), body.ReadString("password"));
    }
    public override string ToString() => nameof(LoginInput);
}
public sealed record AuthContextView(
    [property: JsonPropertyName("accountId")] int AccountId,
    [property: JsonPropertyName("fullName")] string FullName,
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("role")] string Role,
    [property: JsonPropertyName("scope")] string Scope,
    [property: JsonPropertyName("accountVersion")] string AccountVersion,
    [property: JsonPropertyName("serverTime")] string ServerTime,
    [property: JsonPropertyName("absoluteExpiresAt")] string AbsoluteExpiresAt,
    [property: JsonPropertyName("effectiveExpiresAt")] string EffectiveExpiresAt);
public sealed class LoginGrant(AuthContextView context, string cookieToken, DateTimeOffset cookieExpiresAt)
{
    public AuthContextView Context { get; } = context;
    [JsonIgnore] public string CookieToken { get; } = cookieToken;
    [JsonIgnore] public DateTimeOffset CookieExpiresAt { get; } = cookieExpiresAt;
    public override string ToString() => nameof(LoginGrant);
}
public sealed class LoginDecision
{
    public LoginGrant? Grant { get; }
    public bool Accepted => Grant is not null;
    private LoginDecision(LoginGrant? grant) { Grant = grant; }
    public static LoginDecision Rejected() => new(null);
    public static LoginDecision Success(LoginGrant grant) => new(grant ?? throw new ArgumentNullException(nameof(grant)));
    public override string ToString() => nameof(LoginDecision);
}
public interface ILoginExecutor
{
    // Both rejected password counter changes and successful issuance commit before returning.
    Task<LoginDecision> ExecuteAsync(LoginInput input, CancellationToken cancellationToken);
}
