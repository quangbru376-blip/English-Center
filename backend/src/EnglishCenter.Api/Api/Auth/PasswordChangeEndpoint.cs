namespace EnglishCenter.Api.Api.Auth;

using EnglishCenter.Api.Api.Identity;
using EnglishCenter.Api.Common.Security;
using EnglishCenter.Api.Services.Auth;

public static class PasswordChangeEndpoint
{
    public static IEndpointConventionBuilder MapPasswordChange(this IEndpointRouteBuilder endpoints, IPasswordChangeExecutor executor, int maxBodyBytes)
    {
        ArgumentNullException.ThrowIfNull(executor);
        if (maxBodyBytes <= 0) throw new ArgumentOutOfRangeException(nameof(maxBodyBytes));
        return endpoints.MapPost("/api/v1/auth/change-password", async (HttpContext context) =>
        {
            var proof = CurrentSessionRequest.Require(context);
            var input = PasswordChangeInput.Parse(AuthJsonBoundary.Read(context));
            await executor.ExecuteAsync(proof, input, context.RequestAborted);
            context.Response.Cookies.Delete(SafeCookiePolicy.AuthCookieName, SafeCookiePolicy.AuthOptions());
            return Results.NoContent();
        }).WithName("ChangePassword").Produces(StatusCodes.Status204NoContent)
            .WithMetadata(new AuthBodyPolicy(maxBodyBytes), new CsrfRoutePolicy(CsrfAccess.Authenticated)).AllowAnonymous();
    }
}
