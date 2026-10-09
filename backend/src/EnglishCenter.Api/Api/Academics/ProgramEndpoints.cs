namespace EnglishCenter.Api.Api.Academics;

using EnglishCenter.Api.Api.Administration;
using EnglishCenter.Api.Api.Auth;
using EnglishCenter.Api.Api.Identity;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Common.Security;
using EnglishCenter.Api.Infrastructure.Academics;
using EnglishCenter.Api.Services.Academics;

public static class ProgramEndpoints
{
    internal static VerifiedSessionToken RequireAcademic(HttpContext context)
    { var token = CurrentSessionRequest.Require(context); if (!context.User.IsInRole("AcademicAffairs")) throw new ApiErrorException(ApiErrorCode.FORBIDDEN); return token; }
    public static void MapTrainingPrograms(this IEndpointRouteBuilder routes, SqlProgramExecutor executor, int bytes)
    {
        var policy = new CsrfRoutePolicy(CsrfAccess.Authenticated, true);
        routes.MapGet("/api/v1/academics/training-programs", async (HttpContext context) => { var token = RequireAcademic(context); AdminAccountEndpoints.RequireEmptyBody(context); return Results.Ok(await executor.SearchAsync(token, context.Request.Query, context.RequestAborted)); }).WithMetadata(policy).AllowAnonymous();
        routes.MapGet("/api/v1/academics/training-programs/{id}", async (HttpContext context, string id) => { var token = RequireAcademic(context); AdminAccountEndpoints.RequireEmptyBody(context); AdminAccountEndpoints.RequireNoQuery(context); return Results.Ok(await executor.DetailAsync(token, AdminAccountEndpoints.Id(id), context.RequestAborted)); }).WithMetadata(policy).AllowAnonymous();
        routes.MapPost("/api/v1/academics/training-programs/{id}/activation-preview", async (HttpContext context, string id) => { var token = RequireAcademic(context); AdminAccountEndpoints.RequireEmptyBody(context); AdminAccountEndpoints.RequireNoQuery(context); return Results.Ok(await executor.PreviewAsync(token, AdminAccountEndpoints.Id(id), context.RequestAborted)); }).WithMetadata(policy).AllowAnonymous();
        routes.MapPost("/api/v1/academics/training-programs", async (HttpContext context) => { var token = RequireAcademic(context); AdminAccountEndpoints.RequireNoQuery(context); var input = AcademicInput.Parse(82, 0, AdminAccountEndpoints.HeaderKey(context), AuthJsonBoundary.Read(context), bytes); var result = await executor.CommandAsync(token, input, context.RequestAborted); return Results.Json(result, statusCode: result.Replayed ? 200 : 201); }).WithMetadata(policy, new AuthBodyPolicy(bytes)).AllowAnonymous();
        foreach (var (api, verb, path) in new[] { (84, "PUT", "configuration"), (85, "PUT", "name"), (87, "POST", "activate"), (88, "POST", "return-to-draft"), (89, "POST", "deactivate") })
            routes.MapMethods("/api/v1/academics/training-programs/{id}/" + path, [verb], async (HttpContext context, string id) => { var token = RequireAcademic(context); AdminAccountEndpoints.RequireNoQuery(context); var input = AcademicInput.Parse(api, AdminAccountEndpoints.Id(id), AdminAccountEndpoints.HeaderKey(context), AuthJsonBoundary.Read(context), bytes); return Results.Ok(await executor.CommandAsync(token, input, context.RequestAborted)); }).WithMetadata(policy, new AuthBodyPolicy(bytes)).AllowAnonymous();
    }
}
