namespace EnglishCenter.Api.Api.Administration;

using EnglishCenter.Api.Api.Auth;
using EnglishCenter.Api.Api.Identity;
using EnglishCenter.Api.Common.Security;
using EnglishCenter.Api.Services.Administration;

public static class StaffAccountEndpoints
{
    public static void MapStaffAccounts(this IEndpointRouteBuilder routes,IStaffAccountExecutor executor,int maxBytes)
    {
        var policy=new CsrfRoutePolicy(CsrfAccess.Authenticated,true);
        routes.MapPost("/api/v1/admin/accounts/search",async(HttpContext context)=>{
            var token=AdminAccountEndpoints.RequireAdmin(context);AdminAccountEndpoints.RequireNoQuery(context);
            return Results.Ok(await executor.SearchAsync(token,AccountSearch.Parse(AuthJsonBoundary.Read(context)),context.RequestAborted));
        }).WithMetadata(policy,new AuthBodyPolicy(maxBytes)).AllowAnonymous();
        routes.MapPost("/api/v1/admin/staff-accounts",async(HttpContext context)=>{
            var token=AdminAccountEndpoints.RequireAdmin(context);AdminAccountEndpoints.RequireNoQuery(context);
            var input=StaffAccountInput.Parse(8,0,AdminAccountEndpoints.HeaderKey(context),AuthJsonBoundary.Read(context),maxBytes);
            var result=await executor.ExecuteAsync(token,input,context.RequestAborted);
            return Results.Json(result,statusCode:result is CredentialIssueResult?201:200);
        }).WithMetadata(policy,new AuthBodyPolicy(maxBytes)).AllowAnonymous();
        foreach(var (api,path) in new[]{(9,"reset-password"),(11,"unlock"),(12,"disable"),(13,"restore")})
            routes.MapPost("/api/v1/admin/accounts/{id}/"+path,async(HttpContext context,string id)=>{
                var token=AdminAccountEndpoints.RequireAdmin(context);AdminAccountEndpoints.RequireNoQuery(context);
                var input=StaffAccountInput.Parse(api,AdminAccountEndpoints.Id(id),AdminAccountEndpoints.HeaderKey(context),AuthJsonBoundary.Read(context),maxBytes);
                return Results.Ok(await executor.ExecuteAsync(token,input,context.RequestAborted));
            }).WithMetadata(policy,new AuthBodyPolicy(maxBytes)).AllowAnonymous();
    }
}
