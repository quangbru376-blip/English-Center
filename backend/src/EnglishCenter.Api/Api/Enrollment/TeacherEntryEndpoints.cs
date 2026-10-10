namespace EnglishCenter.Api.Api.Enrollment;

using EnglishCenter.Api.Api.Administration;
using EnglishCenter.Api.Api.Auth;
using EnglishCenter.Api.Api.Identity;
using EnglishCenter.Api.Infrastructure.Academics;
using EnglishCenter.Api.Infrastructure.Enrollment;

public static class TeacherEntryEndpoints
{
    public static void MapTeacherEntry(this IEndpointRouteBuilder routes, SqlTeacherEntryReader reader, SqlTeacherEntryExecutor executor, int bytes)
    {
        const string root = "/api/v1/admissions/teachers/me"; var policy = new CsrfRoutePolicy(CsrfAccess.Authenticated, true);
        routes.MapGet(root + "/entry-test-sessions", async (HttpContext context) => { var token = EnrollmentEndpoints.RequireRole(context, "Teacher"); AdminAccountEndpoints.RequireEmptyBody(context); return Results.Ok(await reader.SearchAsync(token, context.Request.Query, context.RequestAborted)); }).WithMetadata(policy).AllowAnonymous();
        routes.MapGet(root + "/entry-test-sessions/{id}", async (HttpContext context, string id) => { var token = EnrollmentEndpoints.RequireRole(context, "Teacher"); AdminAccountEndpoints.RequireEmptyBody(context); AdminAccountEndpoints.RequireNoQuery(context); return Results.Ok(await reader.DetailAsync(token, AdminAccountEndpoints.Id(id), context.RequestAborted)); }).WithMetadata(policy).AllowAnonymous();
        routes.MapPost(root + "/entry-test-sessions/{id}/roster/search", async (HttpContext context, string id) =>
        {
            var token = EnrollmentEndpoints.RequireRole(context, "Teacher"); AdminAccountEndpoints.RequireNoQuery(context); var session = AdminAccountEndpoints.Id(id);
            await reader.DetailAsync(token, session, context.RequestAborted);
            return Results.Ok(await reader.RosterAsync(token, session, SessionRosterFilter.Parse(AuthJsonBoundary.Read(context)), context.RequestAborted));
        }).WithMetadata(policy, new AuthBodyPolicy(bytes)).AllowAnonymous();
        routes.MapGet(root + "/entry-test-bookings/{id}", async (HttpContext context, string id) => { var token = EnrollmentEndpoints.RequireRole(context, "Teacher"); AdminAccountEndpoints.RequireEmptyBody(context); AdminAccountEndpoints.RequireNoQuery(context); return Results.Ok(await reader.TaskAsync(token, AdminAccountEndpoints.Id(id), context.RequestAborted)); }).WithMetadata(policy).AllowAnonymous();
        foreach (var (api, method, path) in new[] { (70, "POST", "entry-test-bookings/{id}/score"), (71, "POST", "entry-test-bookings/{id}/absence"), (72, "PUT", "entry-test-attempts/{id}/score") }) routes.MapMethods(root + "/" + path, [method], async (HttpContext context, string id) =>
        { var token = EnrollmentEndpoints.RequireRole(context, "Teacher"); AdminAccountEndpoints.RequireNoQuery(context); return Results.Ok(await executor.CommandAsync(token, api, AdminAccountEndpoints.Id(id), AdminAccountEndpoints.HeaderKey(context), AuthJsonBoundary.Read(context), bytes, context.RequestAborted)); }).WithMetadata(policy, new AuthBodyPolicy(bytes)).AllowAnonymous();
        routes.MapGet(root + "/command-results/{requestId}", async (HttpContext context, string requestId) => { var token = EnrollmentEndpoints.RequireRole(context, "Teacher"); var q = EnrollmentEndpoints.Lookup(context, requestId); return Results.Ok(await reader.LookupAsync(token, q.Action, q.Request, context.RequestAborted)); }).WithMetadata(policy).AllowAnonymous();
    }
}
