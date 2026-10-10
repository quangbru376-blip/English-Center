namespace EnglishCenter.Api.Api.Admissions;

using EnglishCenter.Api.Api.Administration;
using EnglishCenter.Api.Api.Auth;
using EnglishCenter.Api.Api.Enrollment;
using EnglishCenter.Api.Api.Identity;
using EnglishCenter.Api.Infrastructure.Admissions;

public static class AdmissionsEndpoints
{
    public static void MapAdmissions(this IEndpointRouteBuilder routes, SqlAdmissionsExecutor executor, int bytes)
    {
        const string root = "/api/v1/admissions/registrations"; var policy = new CsrfRoutePolicy(CsrfAccess.Authenticated, true);
        routes.MapPost(root + "/search", async (HttpContext context) =>
        {
            var token = EnrollmentEndpoints.RequireRole(context, "Admissions"); AdminAccountEndpoints.RequireNoQuery(context);
            return Results.Ok(await executor.SearchAsync(token, AdmissionsSearchFilter.Parse(AuthJsonBoundary.Read(context)), context.RequestAborted));
        }).WithMetadata(policy, new AuthBodyPolicy(bytes)).AllowAnonymous();
        routes.MapGet(root + "/{id}", async (HttpContext context, string id) =>
        {
            var token = EnrollmentEndpoints.RequireRole(context, "Admissions"); AdminAccountEndpoints.RequireEmptyBody(context); AdminAccountEndpoints.RequireNoQuery(context);
            return Results.Ok(await executor.DetailAsync(token, AdminAccountEndpoints.Id(id), context.RequestAborted));
        }).WithMetadata(policy).AllowAnonymous();
        foreach (var kind in new[] { "approval-preview", "cancellation-preview" }) routes.MapPost(root + "/{id}/" + kind, async (HttpContext context, string id) =>
        {
            var token = EnrollmentEndpoints.RequireRole(context, "Admissions"); AdminAccountEndpoints.RequireEmptyBody(context); AdminAccountEndpoints.RequireNoQuery(context);
            var target = AdminAccountEndpoints.Id(id);
            return kind == "approval-preview" ? Results.Ok(await executor.ApprovalAsync(token, target, context.RequestAborted)) : Results.Ok(await executor.CancellationAsync(token, target, context.RequestAborted));
        }).WithMetadata(policy).AllowAnonymous();
        foreach (var (api, name) in new[] { (41, "approve"), (42, "reject"), (44, "cancel") }) routes.MapPost(root + "/{id}/" + name, async (HttpContext context, string id) =>
        {
            var token = EnrollmentEndpoints.RequireRole(context, "Admissions"); AdminAccountEndpoints.RequireNoQuery(context);
            return Results.Ok(await executor.CommandAsync(token, api, AdminAccountEndpoints.Id(id), AdminAccountEndpoints.HeaderKey(context), AuthJsonBoundary.Read(context), bytes, context.RequestAborted));
        }).WithMetadata(policy, new AuthBodyPolicy(bytes)).AllowAnonymous();
    }
}
