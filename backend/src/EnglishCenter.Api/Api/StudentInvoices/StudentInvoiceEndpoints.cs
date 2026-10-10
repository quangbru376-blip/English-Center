namespace EnglishCenter.Api.Api.StudentInvoices;

using EnglishCenter.Api.Api.Administration;
using EnglishCenter.Api.Api.Auth;
using EnglishCenter.Api.Api.Enrollment;
using EnglishCenter.Api.Infrastructure.StudentInvoices;

public static class StudentInvoiceEndpoints
{
    public static void MapStudentInvoices(this IEndpointRouteBuilder routes, SqlStudentInvoiceReader reader)
    {
        var policy = new CsrfRoutePolicy(CsrfAccess.Authenticated, true);
        routes.MapGet("/api/v1/students/me/invoices", async (HttpContext context) =>
        {
            var token = EnrollmentEndpoints.RequireRole(context, "Student"); AdminAccountEndpoints.RequireEmptyBody(context);
            return Results.Ok(await reader.SearchAsync(token, context.Request.Query, context.RequestAborted));
        }).WithMetadata(policy).AllowAnonymous();
        routes.MapGet("/api/v1/students/me/invoices/{id}", async (HttpContext context, string id) =>
        {
            var token = EnrollmentEndpoints.RequireRole(context, "Student"); AdminAccountEndpoints.RequireEmptyBody(context); AdminAccountEndpoints.RequireNoQuery(context);
            return Results.Ok(await reader.DetailAsync(token, AdminAccountEndpoints.Id(id), context.RequestAborted));
        }).WithMetadata(policy).AllowAnonymous();
    }
}
