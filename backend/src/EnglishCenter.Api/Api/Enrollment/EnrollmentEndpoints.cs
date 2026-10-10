namespace EnglishCenter.Api.Api.Enrollment;

using EnglishCenter.Api.Api.Administration;
using EnglishCenter.Api.Api.Auth;
using EnglishCenter.Api.Api.Identity;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Common.Security;
using EnglishCenter.Api.Infrastructure.Enrollment;
using EnglishCenter.Api.Services.Enrollment;

public static class EnrollmentEndpoints
{
    internal static VerifiedSessionToken RequireRole(HttpContext context, string role)
    { var token = CurrentSessionRequest.Require(context); if (!context.User.IsInRole(role)) throw new ApiErrorException(ApiErrorCode.FORBIDDEN); return token; }
    internal static (string Action, Guid Request) Lookup(HttpContext context, string request)
    {
        AdminAccountEndpoints.RequireEmptyBody(context);
        if (context.Request.Query.Count != 1 || !context.Request.Query.TryGetValue("action", out var actions) || actions.Count != 1 || string.IsNullOrEmpty(actions[0])) throw ApiErrorException.Validation("action");
        return (actions[0]!, AdminAccountEndpoints.Uuid(request));
    }

    public static void MapStudentEnrollment(this IEndpointRouteBuilder routes, SqlOwnProfileExecutor profiles, SqlStudentEnrollmentReader reader, SqlStudentEnrollmentExecutor executor, int bytes)
    {
        const string root = "/api/v1/students/me"; var policy = new CsrfRoutePolicy(CsrfAccess.Authenticated, true);
        routes.MapGet(root + "/profile", async (HttpContext context) => { var token = RequireRole(context, "Student"); AdminAccountEndpoints.RequireEmptyBody(context); AdminAccountEndpoints.RequireNoQuery(context); return Results.Ok(await profiles.ReadAsync(token, context.RequestAborted)); }).WithMetadata(policy).AllowAnonymous();
        routes.MapPut(root + "/profile", async (HttpContext context) => { var token = RequireRole(context, "Student"); AdminAccountEndpoints.RequireNoQuery(context); return Results.Ok(await profiles.UpdateAsync(token, AuthJsonBoundary.Read(context), context.RequestAborted)); }).WithMetadata(policy, new AuthBodyPolicy(bytes)).AllowAnonymous();
        routes.MapGet(root + "/home", async (HttpContext context) => { var token = RequireRole(context, "Student"); AdminAccountEndpoints.RequireEmptyBody(context); return Results.Ok(await reader.HomeAsync(token, context.Request.Query, context.RequestAborted)); }).WithMetadata(policy).AllowAnonymous();
        routes.MapGet(root + "/courses", async (HttpContext context) => { var token = RequireRole(context, "Student"); AdminAccountEndpoints.RequireEmptyBody(context); return Results.Ok(await reader.CoursesAsync(token, context.Request.Query, context.RequestAborted)); }).WithMetadata(policy).AllowAnonymous();
        routes.MapGet(root + "/courses/{id}", async (HttpContext context, string id) => { var token = RequireRole(context, "Student"); AdminAccountEndpoints.RequireEmptyBody(context); AdminAccountEndpoints.RequireNoQuery(context); return Results.Ok(await reader.CourseAsync(token, AdminAccountEndpoints.Id(id), context.RequestAborted)); }).WithMetadata(policy).AllowAnonymous();
        routes.MapGet(root + "/registrations", async (HttpContext context) => { var token = RequireRole(context, "Student"); AdminAccountEndpoints.RequireEmptyBody(context); return Results.Ok(await reader.RegistrationsAsync(token, context.Request.Query, context.RequestAborted)); }).WithMetadata(policy).AllowAnonymous();
        routes.MapGet(root + "/registrations/{id}", async (HttpContext context, string id) => { var token = RequireRole(context, "Student"); AdminAccountEndpoints.RequireEmptyBody(context); AdminAccountEndpoints.RequireNoQuery(context); return Results.Ok(await reader.DetailAsync(token, AdminAccountEndpoints.Id(id), context.RequestAborted)); }).WithMetadata(policy).AllowAnonymous();
        routes.MapGet(root + "/registrations/{id}/entry-test-options", async (HttpContext context, string id) => { var token = RequireRole(context, "Student"); AdminAccountEndpoints.RequireEmptyBody(context); return Results.Ok(await reader.OptionsAsync(token, AdminAccountEndpoints.Id(id), context.Request.Query, context.RequestAborted)); }).WithMetadata(policy).AllowAnonymous();
        routes.MapGet(root + "/registrations/{id}/entry-results", async (HttpContext context, string id) => { var token = RequireRole(context, "Student"); AdminAccountEndpoints.RequireEmptyBody(context); AdminAccountEndpoints.RequireNoQuery(context); return Results.Ok(await reader.ResultsAsync(token, AdminAccountEndpoints.Id(id), context.RequestAborted)); }).WithMetadata(policy).AllowAnonymous();
        routes.MapGet(root + "/entry-test-bookings", async (HttpContext context) => { var token = RequireRole(context, "Student"); AdminAccountEndpoints.RequireEmptyBody(context); return Results.Ok(await reader.CalendarAsync(token, context.Request.Query, context.RequestAborted)); }).WithMetadata(policy).AllowAnonymous();
        routes.MapPost(root + "/registrations/{id}/cancellation-preview", async (HttpContext context, string id) => { var token = RequireRole(context, "Student"); AdminAccountEndpoints.RequireEmptyBody(context); AdminAccountEndpoints.RequireNoQuery(context); return Results.Ok(await reader.CancellationAsync(token, AdminAccountEndpoints.Id(id), context.RequestAborted)); }).WithMetadata(policy).AllowAnonymous();
        routes.MapPost(root + "/registrations/{id}/confirmation-preview", async (HttpContext context, string id) => { var token = RequireRole(context, "Student"); AdminAccountEndpoints.RequireEmptyBody(context); AdminAccountEndpoints.RequireNoQuery(context); return Results.Ok(await reader.ConfirmationAsync(token, AdminAccountEndpoints.Id(id), context.RequestAborted)); }).WithMetadata(policy).AllowAnonymous();
        routes.MapPost(root + "/registrations", async (HttpContext context) =>
        { var token = RequireRole(context, "Student"); AdminAccountEndpoints.RequireNoQuery(context); var result = (OwnRegistrationResult)await executor.CommandAsync(token, 25, 0, 0, AdminAccountEndpoints.HeaderKey(context), AuthJsonBoundary.Read(context), bytes, context.RequestAborted); return Results.Json(result, statusCode: result.Replayed ? 200 : 201); }).WithMetadata(policy, new AuthBodyPolicy(bytes)).AllowAnonymous();
        foreach (var (api, path) in new[] { (28, "cancel"), (36, "confirm"), (30, "entry-test-bookings") }) routes.MapPost(root + "/registrations/{id}/" + path, async (HttpContext context, string id) =>
        {
            var token = RequireRole(context, "Student"); AdminAccountEndpoints.RequireNoQuery(context);
            var result = await executor.CommandAsync(token, api, AdminAccountEndpoints.Id(id), 0, AdminAccountEndpoints.HeaderKey(context), AuthJsonBoundary.Read(context), bytes, context.RequestAborted);
            return Results.Json(result, statusCode: api == 30 && result is OwnBookingResult { Replayed: false } ? 201 : 200);
        }).WithMetadata(policy, new AuthBodyPolicy(bytes)).AllowAnonymous();
        foreach (var (api, path) in new[] { (31, "cancel"), (32, "reschedule") }) routes.MapPost(root + "/registrations/{id}/entry-test-bookings/{booking}/" + path, async (HttpContext context, string id, string booking) =>
        { var token = RequireRole(context, "Student"); AdminAccountEndpoints.RequireNoQuery(context); return Results.Ok(await executor.CommandAsync(token, api, AdminAccountEndpoints.Id(id), AdminAccountEndpoints.Id(booking), AdminAccountEndpoints.HeaderKey(context), AuthJsonBoundary.Read(context), bytes, context.RequestAborted)); }).WithMetadata(policy, new AuthBodyPolicy(bytes)).AllowAnonymous();
        routes.MapGet(root + "/command-results/{requestId}", async (HttpContext context, string requestId) => { var token = RequireRole(context, "Student"); var q = Lookup(context, requestId); return Results.Ok(await reader.LookupAsync(token, q.Action, q.Request, context.RequestAborted)); }).WithMetadata(policy).AllowAnonymous();
    }
}
