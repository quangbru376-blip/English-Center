namespace EnglishCenter.Api.Api.Auth;

using System.Globalization;
using System.Net;
using EnglishCenter.Api.Api.Identity;
using EnglishCenter.Api.Common.Auditing;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Common.Security;
using EnglishCenter.Api.Common.Time;
using EnglishCenter.Api.Services.Auth;

public interface ITrustedLoginIpResolver { IPAddress? Resolve(HttpContext context); }
public static class LoginEndpoint
{
    public static IEndpointConventionBuilder MapLogin(this IEndpointRouteBuilder endpoints, ILoginExecutor executor, LoginIpLimiter limiter, ITrustedLoginIpResolver ips, SecurityAuditRecorder audit, BusinessClock clock, int maxBodyBytes)
    {
        ArgumentNullException.ThrowIfNull(executor); ArgumentNullException.ThrowIfNull(limiter); ArgumentNullException.ThrowIfNull(ips); ArgumentNullException.ThrowIfNull(audit); ArgumentNullException.ThrowIfNull(clock);
        if (maxBodyBytes <= 0) throw new ArgumentOutOfRangeException(nameof(maxBodyBytes));
        return endpoints.MapPost("/api/v1/auth/login", async (HttpContext context) =>
        {
            var json = AuthJsonBoundary.Read(context);
            var ip = ips.Resolve(context) ?? throw new ApiErrorException(ApiErrorCode.SERVICE_UNAVAILABLE);
            if (!limiter.TryAcquire(ip, out var retry)) return await RateLimited(context, audit, clock, ip, retry);
            var input = LoginInput.Parse(json);
            var decision = await executor.ExecuteAsync(input, context.RequestAborted);
            if (decision.Grant is not LoginGrant grant || !clock.IsBefore(grant.CookieExpiresAt)) throw new ApiErrorException(ApiErrorCode.AUTHENTICATION_FAILED);
            var options = SafeCookiePolicy.AuthOptions(); options.Expires = grant.CookieExpiresAt;
            context.Response.Cookies.Append(SafeCookiePolicy.AuthCookieName, grant.CookieToken, options);
            return Results.Json(grant.Context);
        }).WithName("Login").Produces<AuthContextView>(StatusCodes.Status200OK).Produces<ApiErrorResponse>(StatusCodes.Status429TooManyRequests)
            .WithMetadata(new AuthBodyPolicy(maxBodyBytes), new CsrfRoutePolicy(CsrfAccess.AnonymousOrCurrent, true)).AllowAnonymous();
    }
    private static async Task<IResult> RateLimited(HttpContext context, SecurityAuditRecorder audit, BusinessClock clock, IPAddress ip, int retry)
    {
        await audit.TryRecordAsync(SecurityAuditKind.Throttled, null, false, Guid.NewGuid(), clock.GetUtcNow(), ip.ToString(), context.RequestAborted);
        context.Response.Headers.RetryAfter = retry.ToString(CultureInfo.InvariantCulture);
        context.Response.Headers.CacheControl = "no-store";
        return Results.Json(new ApiErrorResponse { Code = ApiErrorCode.RATE_LIMITED.GetCode(), Message = ApiErrorCode.RATE_LIMITED.GetMessage(), TraceId = Guid.NewGuid().ToString("N") }, statusCode: StatusCodes.Status429TooManyRequests);
    }
}
