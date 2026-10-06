namespace EnglishCenter.Api.Api.Administration;

using System.Globalization;
using EnglishCenter.Api.Api.Auth;
using EnglishCenter.Api.Api.Identity;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Common.Security;
using EnglishCenter.Api.Common.Serialization;
using EnglishCenter.Api.Services.Administration;

public static class AdminAccountEndpoints
{
    public static void MapAdminAccountPilot(this IEndpointRouteBuilder endpoints,IAdminAccountExecutor executor,int maxBodyBytes)
    {
        ArgumentNullException.ThrowIfNull(executor);
        if(maxBodyBytes<=0)throw new ArgumentOutOfRangeException(nameof(maxBodyBytes));
        var policy=new CsrfRoutePolicy(CsrfAccess.Authenticated,true);
        endpoints.MapGet("/api/v1/admin/accounts/{id}",async (HttpContext context,string id)=>{
            var proof=RequireAdmin(context);RequireEmptyBody(context);RequireNoQuery(context);
            return Results.Ok(await executor.DetailAsync(proof,Id(id),context.RequestAborted));
        }).WithMetadata(policy).AllowAnonymous();
        endpoints.MapPost("/api/v1/admin/accounts/{id}/lock",async (HttpContext context,string id)=>{
            var proof=RequireAdmin(context);
            var request=HeaderKey(context);
            var input=AdminLockInput.Parse(Id(id),request,AuthJsonBoundary.Read(context),maxBodyBytes);
            return Results.Ok(await executor.LockAsync(proof,input,context.RequestAborted));
        }).WithMetadata(policy,new AuthBodyPolicy(maxBodyBytes)).AllowAnonymous();
        endpoints.MapGet("/api/v1/admin/command-results/{requestId}",async (HttpContext context,string requestId)=>{
            var proof=RequireAdmin(context);RequireEmptyBody(context);
            if(context.Request.Query.Count!=1||!context.Request.Query.TryGetValue("action",out var actions)||actions.Count!=1||string.IsNullOrEmpty(actions[0]))
                throw ApiErrorException.Validation("action");
            return Results.Ok(await executor.LookupAsync(proof,actions[0]!,Uuid(requestId),context.RequestAborted));
        }).WithMetadata(policy).AllowAnonymous();
    }
    private static VerifiedSessionToken RequireAdmin(HttpContext context)
    {
        var proof=CurrentSessionRequest.Require(context);
        if(!context.User.IsInRole("Admin"))throw new ApiErrorException(ApiErrorCode.FORBIDDEN);
        return proof;
    }
    private static void RequireEmptyBody(HttpContext context)
    {
        if((context.Features.Get<Microsoft.AspNetCore.Http.Features.IHttpRequestBodyDetectionFeature>()?.CanHaveBody ?? context.Request.ContentLength is > 0))throw ApiErrorException.Validation();
    }
    private static void RequireNoQuery(HttpContext context)
    { if(context.Request.Query.Count!=0)throw ApiErrorException.Validation(); }
    private static int Id(string text)
    {
        if(string.IsNullOrEmpty(text)||text[0]=='0'||text.Any(c=>c<'0'||c>'9')||
            !int.TryParse(text,NumberStyles.None,CultureInfo.InvariantCulture,out var id)||id<=0)throw ApiErrorException.Validation("accountId");
        return id;
    }
    private static Guid Uuid(string text)
    { try{return Guid.Parse(StrictScalars.NormalizeUuid(text));}catch(ArgumentException){throw ApiErrorException.Validation("requestId");} }
    private static Guid HeaderKey(HttpContext context)
    {
        try { return Guid.Parse(StrictScalars.NormalizeUuidHeader(context.Request.Headers["Idempotency-Key"].Select(v=>v!).ToArray())); }
        catch(ArgumentException){throw ApiErrorException.Validation("requestId");}
    }
}
