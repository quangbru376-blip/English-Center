namespace EnglishCenter.Api.Api.Auth;

using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Common.Security;
using EnglishCenter.Api.Common.Time;
using EnglishCenter.Api.Services.Auth;
using Microsoft.AspNetCore.Http.Features;

public static class CurrentSessionEndpoints
{
    public static IEndpointConventionBuilder MapCurrentSession(this IEndpointRouteBuilder endpoints, ICurrentSessionReader reader, BusinessClock clock)
    {
        ArgumentNullException.ThrowIfNull(reader); ArgumentNullException.ThrowIfNull(clock);
        return endpoints.MapGet("/api/v1/auth/me", async (HttpContext context) =>
        {
            var token = CurrentSessionRequest.Require(context);
            if (context.Request.Query.Count != 0 || context.Request.ContentLength is > 0 || (context.Features.Get<IHttpRequestBodyDetectionFeature>()?.CanHaveBody ?? false)) throw ApiErrorException.Validation();
            var current = await reader.ReadAsync(token, false, context.RequestAborted);
            if (!clock.IsBefore(current.EffectiveDeadline)) throw new ApiErrorException(ApiErrorCode.AUTHENTICATION_REQUIRED);
            return Results.Json(current.Context);
        }).WithName("CurrentSession").Produces<AuthContextView>().WithMetadata(new CsrfRoutePolicy(CsrfAccess.Authenticated)).AllowAnonymous();
    }
    public static IEndpointConventionBuilder MapCurrentSessionLogout(this IEndpointRouteBuilder endpoints, ICurrentSessionLogout operation)
    {
        ArgumentNullException.ThrowIfNull(operation);
        return endpoints.MapPost("/api/v1/auth/logout", async (HttpContext context) =>
        {
            var token = CurrentSessionRequest.Require(context);
            if (!LogoutRequestShape.IsEmpty(context.Request.ContentLength, context.Features.Get<IHttpRequestBodyDetectionFeature>()?.CanHaveBody ?? false, context.Request.Query.Count)) throw ApiErrorException.Validation();
            await operation.ExecuteAsync(token, context.RequestAborted);
            context.Response.Cookies.Delete(SafeCookiePolicy.AuthCookieName, SafeCookiePolicy.AuthOptions());
            return Results.NoContent();
        }).WithName("LogoutCurrentSession").Produces(StatusCodes.Status204NoContent).WithMetadata(new CsrfRoutePolicy(CsrfAccess.Authenticated)).AllowAnonymous();
    }
}
