namespace EnglishCenter.Api.Api.Classes;
using EnglishCenter.Api.Api.Administration;
using EnglishCenter.Api.Api.Auth;
using EnglishCenter.Api.Api.Enrollment;
using EnglishCenter.Api.Api.Identity;
using EnglishCenter.Api.Infrastructure.Classes;

public static class GradeEndpoints
{
    public static void MapGrades(this IEndpointRouteBuilder routes,SqlClassExecutor executor,int bytes)
    {
        routes.MapStudentGrades(executor);
        const string teacher="/api/v1/academics/teachers/me/classes/{id}";
        const string review="/api/v1/academics/grade-reviews/classes/{id}";
        var policy=new CsrfRoutePolicy(CsrfAccess.Authenticated,true);
        routes.MapGet(teacher+"/gradebook",async(HttpContext c,string id)=>{var t=EnrollmentEndpoints.RequireRole(c,"Teacher");Read(c);return Results.Ok(await executor.Gradebook(t,AdminAccountEndpoints.Id(id),c.RequestAborted));}).WithMetadata(policy).AllowAnonymous();
        routes.MapGet(teacher+"/assignments/{assignment}/grade-draft",async(HttpContext c,string id,string assignment)=>{var t=EnrollmentEndpoints.RequireRole(c,"Teacher");Read(c);return Results.Ok(await executor.GradeDraft(t,AdminAccountEndpoints.Id(id),AdminAccountEndpoints.Id(assignment),c.RequestAborted));}).WithMetadata(policy).AllowAnonymous();
        routes.MapGet(teacher+"/components/{component}/schedule-options",async(HttpContext c,string id,string component)=>{var t=EnrollmentEndpoints.RequireRole(c,"Teacher");Read(c);return Results.Ok(await executor.GradeOptions(t,AdminAccountEndpoints.Id(id),AdminAccountEndpoints.Id(component),c.RequestAborted));}).WithMetadata(policy).AllowAnonymous();
        routes.MapPut(teacher+"/assignments/{assignment}/grade-draft",async(HttpContext c,string id,string assignment)=>{var t=EnrollmentEndpoints.RequireRole(c,"Teacher");AdminAccountEndpoints.RequireNoQuery(c);var r=await executor.GradeCommand(t,139,AdminAccountEndpoints.Id(id),AdminAccountEndpoints.Id(assignment),AdminAccountEndpoints.HeaderKey(c),AuthJsonBoundary.Read(c),bytes,c.RequestAborted);return Results.Ok(r.Result);}).WithMetadata(policy,new AuthBodyPolicy(bytes)).AllowAnonymous();
        routes.MapPost(teacher+"/grade-submission-preview",async(HttpContext c,string id)=>{var t=EnrollmentEndpoints.RequireRole(c,"Teacher");AdminAccountEndpoints.RequireNoQuery(c);return Results.Ok(await executor.SubmissionPreview(t,AdminAccountEndpoints.Id(id),AuthJsonBoundary.Read(c),c.RequestAborted));}).WithMetadata(policy,new AuthBodyPolicy(bytes)).AllowAnonymous();
        foreach(var(api,path) in new[]{(141,"submit-grades"),(142,"resubmit-grades")})
            routes.MapPost(teacher+"/"+path,async(HttpContext c,string id)=>{var t=EnrollmentEndpoints.RequireRole(c,"Teacher");AdminAccountEndpoints.RequireNoQuery(c);var r=await executor.GradeCommand(t,api,AdminAccountEndpoints.Id(id),0,AdminAccountEndpoints.HeaderKey(c),AuthJsonBoundary.Read(c),bytes,c.RequestAborted);return Results.Ok(r.Result);}).WithMetadata(policy,new AuthBodyPolicy(bytes)).AllowAnonymous();
        routes.MapGet("/api/v1/academics/grade-reviews",async(HttpContext c)=>{var t=EnrollmentEndpoints.RequireRole(c,"AcademicAffairs");AdminAccountEndpoints.RequireEmptyBody(c);return Results.Ok(await executor.ReviewList(t,c.Request.Query,c.RequestAborted));}).WithMetadata(policy).AllowAnonymous();
        routes.MapGet(review,async(HttpContext c,string id)=>{var t=EnrollmentEndpoints.RequireRole(c,"AcademicAffairs");Read(c);return Results.Ok(await executor.ReviewDetail(t,AdminAccountEndpoints.Id(id),c.RequestAborted));}).WithMetadata(policy).AllowAnonymous();
        routes.MapPost(review+"/review-preview",async(HttpContext c,string id)=>{var t=EnrollmentEndpoints.RequireRole(c,"AcademicAffairs");AdminAccountEndpoints.RequireNoQuery(c);return Results.Ok(await executor.ReviewPreview(t,AdminAccountEndpoints.Id(id),AuthJsonBoundary.Read(c),c.RequestAborted));}).WithMetadata(policy,new AuthBodyPolicy(bytes)).AllowAnonymous();
        routes.MapPost(review+"/return",async(HttpContext c,string id)=>{var t=EnrollmentEndpoints.RequireRole(c,"AcademicAffairs");AdminAccountEndpoints.RequireNoQuery(c);var r=await executor.GradeCommand(t,146,AdminAccountEndpoints.Id(id),0,AdminAccountEndpoints.HeaderKey(c),AuthJsonBoundary.Read(c),bytes,c.RequestAborted);return Results.Ok(r.Result);}).WithMetadata(policy,new AuthBodyPolicy(bytes)).AllowAnonymous();
        routes.MapPost(review+"/finalize",async(HttpContext c,string id)=>{var t=EnrollmentEndpoints.RequireRole(c,"AcademicAffairs");AdminAccountEndpoints.RequireNoQuery(c);var r=await executor.GradeCommand(t,147,AdminAccountEndpoints.Id(id),0,AdminAccountEndpoints.HeaderKey(c),AuthJsonBoundary.Read(c),bytes,c.RequestAborted);return Results.Ok(r.Result);}).WithMetadata(policy,new AuthBodyPolicy(bytes)).AllowAnonymous();
    }
    public static void MapStudentGrades(this IEndpointRouteBuilder routes,SqlClassExecutor executor)
    {
        var policy=new CsrfRoutePolicy(CsrfAccess.Authenticated,true);
        routes.MapGet("/api/v1/students/me/grade-reports",async(HttpContext c)=>{var t=EnrollmentEndpoints.RequireRole(c,"Student");AdminAccountEndpoints.RequireEmptyBody(c);return Results.Ok(await executor.StudentGrades(t,c.Request.Query,c.RequestAborted));}).WithMetadata(policy).AllowAnonymous();
        routes.MapGet("/api/v1/students/me/grade-reports/{id}",async(HttpContext c,string id)=>{var t=EnrollmentEndpoints.RequireRole(c,"Student");Read(c);return Results.Ok(await executor.StudentGrade(t,AdminAccountEndpoints.Id(id),c.RequestAborted));}).WithMetadata(policy).AllowAnonymous();
    }
    private static void Read(HttpContext c){AdminAccountEndpoints.RequireEmptyBody(c);AdminAccountEndpoints.RequireNoQuery(c);}
}
