namespace EnglishCenter.Api.Api.Finance;

using EnglishCenter.Api.Api.Administration;
using EnglishCenter.Api.Api.Auth;
using EnglishCenter.Api.Api.Enrollment;
using EnglishCenter.Api.Api.Identity;
using EnglishCenter.Api.Infrastructure.Finance;

public static class FinanceEndpoints
{
    public static void MapFinance(this IEndpointRouteBuilder routes, SqlFinanceExecutor executor, int bytes)
    {
        const string root = "/api/v1/finance"; var policy = new CsrfRoutePolicy(CsrfAccess.Authenticated, true);
        routes.MapPost(root + "/invoices/search", async (HttpContext context) =>
        {
            var token = EnrollmentEndpoints.RequireRole(context, "Accountant"); AdminAccountEndpoints.RequireNoQuery(context);
            return Results.Ok(await executor.SearchAsync(token, FinanceSearchFilter.Parse(AuthJsonBoundary.Read(context)), context.RequestAborted));
        }).WithMetadata(policy, new AuthBodyPolicy(bytes)).AllowAnonymous();
        routes.MapGet(root + "/invoices/{id}", async (HttpContext context, string id) =>
        {
            var token = EnrollmentEndpoints.RequireRole(context, "Accountant"); AdminAccountEndpoints.RequireEmptyBody(context); AdminAccountEndpoints.RequireNoQuery(context);
            return Results.Ok(await executor.DetailAsync(token, AdminAccountEndpoints.Id(id), context.RequestAborted));
        }).WithMetadata(policy).AllowAnonymous();
        routes.MapPost(root + "/invoices/{id}/collection-preview", async (HttpContext context, string id) =>
        {
            var token = EnrollmentEndpoints.RequireRole(context, "Accountant"); AdminAccountEndpoints.RequireEmptyBody(context); AdminAccountEndpoints.RequireNoQuery(context);
            return Results.Ok(await executor.PreviewAsync(token, AdminAccountEndpoints.Id(id), context.RequestAborted));
        }).WithMetadata(policy).AllowAnonymous();
        foreach (var (api, method, path) in new[] { (51, "POST", "/invoices/{id}/collect"), (52, "PUT", "/payments/{id}/metadata") }) routes.MapMethods(root + path, [method], async (HttpContext context, string id) =>
        {
            var token = EnrollmentEndpoints.RequireRole(context, "Accountant"); AdminAccountEndpoints.RequireNoQuery(context);
            var outcome = await executor.CommandAsync(token, api, AdminAccountEndpoints.Id(id), AdminAccountEndpoints.HeaderKey(context), AuthJsonBoundary.Read(context), bytes, context.RequestAborted);
            return api == 51 && !outcome.Replayed ? Results.Json(outcome.Result, statusCode: 201) : Results.Ok(outcome.Result);
        }).WithMetadata(policy, new AuthBodyPolicy(bytes)).AllowAnonymous();
        routes.MapGet(root + "/command-results/{requestId}", async (HttpContext context, string requestId) =>
        {
            var token = EnrollmentEndpoints.RequireRole(context, "Accountant"); AdminAccountEndpoints.RequireEmptyBody(context);
            var q = new Services.Academics.AcademicQuery(context.Request.Query, "action"); var action = q.Get("action") ?? throw Common.Errors.ApiErrorException.Validation("action");
            return Results.Ok(await executor.LookupAsync(token, action, AdminAccountEndpoints.Uuid(requestId), context.RequestAborted));
        }).WithMetadata(policy).AllowAnonymous();
    }
}
