namespace EnglishCenter.Api.Api.Auth;

using System.Globalization;
using System.Security.Claims;
using EnglishCenter.Api.Common.Auditing;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Common.Security;
using EnglishCenter.Api.Common.Time;
using EnglishCenter.Api.Services.Auth;

public interface ICsrfDeniedObserver
{
    Task RecordAsync(VerifiedCsrfIdentity? identity, CancellationToken ct);
}
public sealed class LiveCsrfIdentityResolver(JwtSessionValidator validator, ICurrentSessionReader reader, SecurityAuditRecorder audit, BusinessClock clock) : ICsrfIdentityResolver, ICsrfDeniedObserver
{
    public async Task<VerifiedCsrfIdentity?> ResolveAsync(HttpContext context, CancellationToken ct)
    {
        if (!context.Request.Cookies.TryGetValue(SafeCookiePolicy.AuthCookieName, out var encoded)) return null;
        // Invalid cookies must not strand anonymous CSRF/login after idle expiry or revocation.
        // Preserve 401; clearing this unusable browser credential is not a server revoke.
        context.Response.OnStarting(() =>
        {
            if (context.Response.StatusCode == StatusCodes.Status401Unauthorized)
                context.Response.Cookies.Delete(SafeCookiePolicy.AuthCookieName, SafeCookiePolicy.AuthOptions());
            return Task.CompletedTask;
        });
        var token = validator.Validate(encoded) ?? throw new ApiErrorException(ApiErrorCode.AUTHENTICATION_REQUIRED);
        var current = await reader.ReadAsync(token, false, ct);
        if (!clock.IsBefore(current.EffectiveDeadline)) throw new ApiErrorException(ApiErrorCode.AUTHENTICATION_REQUIRED);
        CurrentSessionRequest.Remember(context, token);
        // Only current SQL facts become the antiforgery identity; claims are not durable rights.
        var principal = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, current.Context.AccountId.ToString(CultureInfo.InvariantCulture)), new Claim(ClaimTypes.Role, current.Context.Role)], "ec-session"));
        return new(principal, current.Context.Scope == "PASSWORD_CHANGE");
    }
    public Task RecordAsync(VerifiedCsrfIdentity? identity, CancellationToken ct)
    {
        int? actor = null;
        if (identity?.Principal.Identity?.IsAuthenticated == true && int.TryParse(identity.Principal.FindFirstValue(ClaimTypes.NameIdentifier), NumberStyles.None, CultureInfo.InvariantCulture, out var value) && value > 0) actor = value;
        return audit.TryRecordAsync(SecurityAuditKind.AccessDenied, actor, false, Guid.NewGuid(), clock.GetUtcNow(), null, ct);
    }
}
public static class CurrentSessionRequest
{
    private static readonly object ProofKey = new();
    internal static void Remember(HttpContext context, VerifiedSessionToken token) => context.Items[ProofKey] = token;
    public static VerifiedSessionToken Require(HttpContext context)
    {
        CsrfBoundary.RequireVerified(context);
        if (context.User.Identity?.IsAuthenticated != true) throw new ApiErrorException(ApiErrorCode.AUTHENTICATION_REQUIRED);
        if (!context.Items.TryGetValue(ProofKey, out var proof) || proof is not VerifiedSessionToken token) throw new ApiErrorException(ApiErrorCode.SERVICE_UNAVAILABLE);
        return token;
    }
}
