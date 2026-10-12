namespace EnglishCenter.Api.Api.Administration;
using EnglishCenter.Api.Api.Auth;
using EnglishCenter.Api.Services.Administration;
public static class AuditEndpoints
{
    public static void MapAuditViewer(this IEndpointRouteBuilder endpoints,IAuditViewerExecutor executor)
    {
        ArgumentNullException.ThrowIfNull(executor);var policy=new CsrfRoutePolicy(CsrfAccess.Authenticated,true);
        endpoints.MapGet("/api/v1/admin/audit-events",async(HttpContext c)=>
        {var t=AdminAccountEndpoints.RequireAdmin(c);AdminAccountEndpoints.RequireEmptyBody(c);return Results.Ok(await executor.List(t,c.Request.Query,c.RequestAborted));}).WithMetadata(policy).AllowAnonymous();
        endpoints.MapGet("/api/v1/admin/audit-events/{id}",async(HttpContext c,string id)=>
        {var t=AdminAccountEndpoints.RequireAdmin(c);AdminAccountEndpoints.RequireEmptyBody(c);AdminAccountEndpoints.RequireNoQuery(c);return Results.Ok(await executor.Detail(t,AdminAccountEndpoints.Id(id),c.RequestAborted));}).WithMetadata(policy).AllowAnonymous();
        endpoints.MapGet("/api/v1/admin/operations/{operationId}/audit-events",async(HttpContext c,string operationId)=>
        {var t=AdminAccountEndpoints.RequireAdmin(c);AdminAccountEndpoints.RequireEmptyBody(c);return Results.Ok(await executor.Operation(t,AdminAccountEndpoints.Uuid(operationId),c.Request.Query,c.RequestAborted));}).WithMetadata(policy).AllowAnonymous();
    }
}
