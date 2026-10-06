namespace EnglishCenter.Api.Api.Identity;

using EnglishCenter.Api.Api.Auth;
using EnglishCenter.Api.Services.Identity;

public static class StudentSignupEndpoint
{
    public static IEndpointConventionBuilder MapStudentSignup(this IEndpointRouteBuilder endpoints, IStudentSignupExecutor executor, int maxBodyBytes)
    {
        ArgumentNullException.ThrowIfNull(executor); if (maxBodyBytes <= 0) throw new ArgumentOutOfRangeException(nameof(maxBodyBytes));
        return endpoints.MapPost("/api/v1/auth/register", async (HttpContext context) =>
        {
            var input = StudentSignupInput.Parse(AuthJsonBoundary.Read(context));
            var result = await executor.ExecuteAsync(input, context.RequestAborted);
            return Results.Json(result, statusCode: StatusCodes.Status201Created);
        }).WithName("RegisterStudent").Produces<RegisterStudentResult>(StatusCodes.Status201Created)
            .WithMetadata(new AuthBodyPolicy(maxBodyBytes), new CsrfRoutePolicy(CsrfAccess.AnonymousOnly)).AllowAnonymous();
    }
}
