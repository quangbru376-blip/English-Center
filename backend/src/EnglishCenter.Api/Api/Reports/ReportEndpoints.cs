namespace EnglishCenter.Api.Api.Reports;
using System.Globalization;
using EnglishCenter.Api.Api.Administration;
using EnglishCenter.Api.Api.Auth;
using EnglishCenter.Api.Api.Enrollment;
using EnglishCenter.Api.Api.Identity;
using EnglishCenter.Api.Infrastructure.Reports;
using EnglishCenter.Api.Services.Reports;
public static class ReportEndpoints
{
    public static void MapReports(this IEndpointRouteBuilder routes,SqlReportsExecutor executor)
    {
        var policy=new CsrfRoutePolicy(CsrfAccess.Authenticated,true);
        foreach(var(api,role,path) in new[]{(150,"Accountant","accountant/finance"),(151,"Director","director/finance"),(152,"Director","director/admissions"),(153,"Director","director/training")})
            routes.MapGet("/api/v1/reports/"+path,async(HttpContext c)=>{var t=EnrollmentEndpoints.RequireRole(c,role);AdminAccountEndpoints.RequireEmptyBody(c);return Results.Ok(await executor.Report(t,api,c.Request.Query,c.RequestAborted));}).WithMetadata(policy).AllowAnonymous();
        routes.MapGet("/api/v1/reports/accountant/finance/csv",async(HttpContext c)=>{
            var t=EnrollmentEndpoints.RequireRole(c,"Accountant");AdminAccountEndpoints.RequireEmptyBody(c);var file=await executor.Csv(t,c.Request.Query,c.RequestAborted);SetHeaders(c,file);c.Response.Headers.ContentDisposition="attachment; filename=\""+file.FileName+"\"";return Results.Bytes(file.Bytes,"text/csv; charset=utf-8");
        }).WithMetadata(policy).AllowAnonymous();
    }
    private static void SetHeaders(HttpContext c,FinanceCsvFile file)
    {
        var q=file.Query;c.Response.Headers.CacheControl="no-store";c.Response.Headers["X-Report-Queried-At"]=file.QueriedAt.UtcDateTime.ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'",CultureInfo.InvariantCulture);c.Response.Headers["X-Report-From-Date"]=ReportQuery.Date(q.From);c.Response.Headers["X-Report-To-Date"]=ReportQuery.Date(q.To);c.Response.Headers["X-Report-Course-Id"]=q.Course?.ToString(CultureInfo.InvariantCulture)??"ALL";c.Response.Headers["X-Report-Overdue-Only"]=q.OverdueOnly?"true":"false";c.Response.Headers["X-CSV-Data-Row-Count"]=file.DataRows.ToString(CultureInfo.InvariantCulture);
    }
}
