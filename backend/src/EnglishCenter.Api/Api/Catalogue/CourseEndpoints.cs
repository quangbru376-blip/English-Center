namespace EnglishCenter.Api.Api.Catalogue;

using EnglishCenter.Api.Api.Administration;
using EnglishCenter.Api.Api.Auth;
using EnglishCenter.Api.Api.Identity;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Common.Security;
using EnglishCenter.Api.Services.Catalogue;

public static class CourseEndpoints
{
    public static void MapCourseCatalogue(this IEndpointRouteBuilder routes,ICourseExecutor executor,int maxBytes,Infrastructure.Classes.SqlClassExecutor? classes=null)
    {
        var policy=new CsrfRoutePolicy(CsrfAccess.Authenticated,true);
        routes.MapGet("/api/v1/academics/courses",async(HttpContext context)=>{
            var token=RequireAcademic(context);AdminAccountEndpoints.RequireEmptyBody(context);
            return Results.Ok(await executor.SearchAsync(token,CourseSearch.Parse(context.Request.Query),context.RequestAborted));
        }).WithMetadata(policy).AllowAnonymous();
        routes.MapGet("/api/v1/academics/courses/{id}",async(HttpContext context,string id)=>{
            var token=RequireAcademic(context);AdminAccountEndpoints.RequireEmptyBody(context);AdminAccountEndpoints.RequireNoQuery(context);
            return Results.Ok(await executor.DetailAsync(token,AdminAccountEndpoints.Id(id),context.RequestAborted));
        }).WithMetadata(policy).AllowAnonymous();
        routes.MapGet("/api/v1/academics/bands",async(HttpContext context)=>{
            var token=RequireAcademic(context);AdminAccountEndpoints.RequireEmptyBody(context);AdminAccountEndpoints.RequireNoQuery(context);
            return Results.Ok(await executor.BandsAsync(token,context.RequestAborted));
        }).WithMetadata(policy).AllowAnonymous();
        routes.MapPost("/api/v1/academics/courses",async(HttpContext context)=>{
            var token=RequireAcademic(context);AdminAccountEndpoints.RequireNoQuery(context);
            var input=CourseInput.Parse(75,0,AdminAccountEndpoints.HeaderKey(context),AuthJsonBoundary.Read(context),maxBytes);
            var result=await executor.CommandAsync(token,input,context.RequestAborted);
            // Completed replay is 200; the first successful create alone is 201.
            return Results.Json(result,statusCode:result.Replayed?200:201);
        }).WithMetadata(policy,new AuthBodyPolicy(maxBytes)).AllowAnonymous();
        foreach(var (api,verb,path) in new[]{(77,"PUT","details"),(78,"POST","deactivate"),(79,"POST","activate")})
            routes.MapMethods("/api/v1/academics/courses/{id}/"+path,[verb],async(HttpContext context,string id)=>{
                var token=RequireAcademic(context);AdminAccountEndpoints.RequireNoQuery(context);
                var input=CourseInput.Parse(api,AdminAccountEndpoints.Id(id),AdminAccountEndpoints.HeaderKey(context),AuthJsonBoundary.Read(context),maxBytes);
                return Results.Ok(await executor.CommandAsync(token,input,context.RequestAborted));
            }).WithMetadata(policy,new AuthBodyPolicy(maxBytes)).AllowAnonymous();
        routes.MapGet("/api/v1/academics/command-results/{requestId}",async(HttpContext context,string requestId)=>{
            var role=context.User.IsInRole("Teacher")?"Teacher":"AcademicAffairs";var token=role=="Teacher"?Enrollment.EnrollmentEndpoints.RequireRole(context,role):RequireAcademic(context);AdminAccountEndpoints.RequireEmptyBody(context);
            if(context.Request.Query.Count!=1||!context.Request.Query.TryGetValue("action",out var action)||action.Count!=1||string.IsNullOrEmpty(action[0]))throw ApiErrorException.Validation("action");
            if(!Common.Replay.CanonicalActionRegistry.All.Any(x=>x.Code==action[0]))throw ApiErrorException.Validation("action");
            var request=AdminAccountEndpoints.Uuid(requestId);
            if(classes is not null&&Common.Replay.CanonicalActionRegistry.All.Any(x=>x.Code==action[0]&&x.ApiId is 100 or 101 or 126 or 133 or 139 or 141 or 142 or 146 or 147))return Results.Ok(await classes.TeachingLookup(token,role,action[0]!,request,context.RequestAborted));
            if(role=="Teacher")throw new ApiErrorException(ApiErrorCode.FORBIDDEN);
            if(classes is not null&&Common.Replay.CanonicalActionRegistry.All.Any(x=>x.Code==action[0]&&x.ApiId is 92 or 94 or 97 or 99 or 102 or 108 or 118 or 120 or 122 or 124))return Results.Ok(await classes.Lookup(token,action[0]!,request,context.RequestAborted));
            return Results.Ok(await executor.LookupAsync(token,action[0]!,request,context.RequestAborted));
        }).WithMetadata(policy).AllowAnonymous();
    }
    private static VerifiedSessionToken RequireAcademic(HttpContext context)
    {
        var token=CurrentSessionRequest.Require(context);if(!context.User.IsInRole("AcademicAffairs"))throw new ApiErrorException(ApiErrorCode.FORBIDDEN);return token;
    }
}
