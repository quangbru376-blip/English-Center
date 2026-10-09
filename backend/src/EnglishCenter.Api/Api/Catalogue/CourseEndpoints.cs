namespace EnglishCenter.Api.Api.Catalogue;

using EnglishCenter.Api.Api.Administration;
using EnglishCenter.Api.Api.Auth;
using EnglishCenter.Api.Api.Identity;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Common.Security;
using EnglishCenter.Api.Services.Catalogue;

public static class CourseEndpoints
{
    public static void MapCourseCatalogue(this IEndpointRouteBuilder routes,ICourseExecutor executor,int maxBytes)
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
            var token=RequireAcademic(context);AdminAccountEndpoints.RequireEmptyBody(context);
            if(context.Request.Query.Count!=1||!context.Request.Query.TryGetValue("action",out var action)||action.Count!=1||string.IsNullOrEmpty(action[0]))throw ApiErrorException.Validation("action");
            return Results.Ok(await executor.LookupAsync(token,action[0]!,AdminAccountEndpoints.Uuid(requestId),context.RequestAborted));
        }).WithMetadata(policy).AllowAnonymous();
    }
    private static VerifiedSessionToken RequireAcademic(HttpContext context)
    {
        var token=CurrentSessionRequest.Require(context);if(!context.User.IsInRole("AcademicAffairs"))throw new ApiErrorException(ApiErrorCode.FORBIDDEN);return token;
    }
}
