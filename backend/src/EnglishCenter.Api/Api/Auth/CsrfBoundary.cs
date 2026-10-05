namespace EnglishCenter.Api.Api.Auth;

using System.Security.Claims;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Common.Security;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.DataProtection;

public sealed class VerifiedCsrfIdentity(ClaimsPrincipal principal,bool restricted)
{
    public ClaimsPrincipal Principal {get;}=principal ?? throw new ArgumentNullException(nameof(principal));
    public bool Restricted {get;}=restricted;
}

public interface ICsrfIdentityResolver
{
    // Future A06 integration verifies session/account/RV/deadlines without renewing idle.
    Task<VerifiedCsrfIdentity?> ResolveAsync(HttpContext context,CancellationToken cancellationToken);
}

public enum CsrfAccess { AnonymousOrCurrent, AnonymousOnly, Authenticated }
public sealed record CsrfRoutePolicy(CsrfAccess Access,bool RequiresNormalScope=false);

public static class CsrfBoundary
{
    private static readonly object VerifiedBoundary = new();
    public static IServiceCollection AddCsrfBoundary(this IServiceCollection services,IDataProtectionProvider keys)
    {
        ArgumentNullException.ThrowIfNull(keys);services.AddSingleton(keys);
        services.AddAntiforgery(options=>
        {
            options.HeaderName="X-CSRF-TOKEN";options.Cookie.Name=SafeCookiePolicy.CsrfCookieName;
            options.Cookie.HttpOnly=true;options.Cookie.SecurePolicy=CookieSecurePolicy.Always;
            options.Cookie.SameSite=SameSiteMode.Strict;options.Cookie.Path="/";options.Cookie.Domain=null;options.Cookie.IsEssential=true;
        });
        return services;
    }

    // Deliberately not wired into Program: requires operator profile/keys and real A06 resolver.
    public static IApplicationBuilder UseCsrfBoundary(this IApplicationBuilder app,string origin,ICsrfIdentityResolver resolver)
    {
        ArgumentNullException.ThrowIfNull(resolver);
        if (!SameOrigin.IsCanonicalHttpsOrigin(origin)) throw new ArgumentException("Invalid HTTPS origin profile.");
        return app.Use(async(context,next)=>
        {
            var policy=context.GetEndpoint()?.Metadata.GetMetadata<CsrfRoutePolicy>();
            if (policy is null)
            {
                if(context.GetEndpoint() is not null && context.Request.Path.StartsWithSegments("/api/v1")) throw new ApiErrorException(ApiErrorCode.SERVICE_UNAVAILABLE);
                await next(context);return;
            }
            context.Response.OnStarting(()=> { context.Response.Headers.CacheControl="no-store";context.Response.Headers.Pragma="no-cache";return Task.CompletedTask; });
            if (!context.Request.IsHttps || !string.Equals("https://"+context.Request.Host.Value,origin,StringComparison.Ordinal)) throw new ApiErrorException(ApiErrorCode.SERVICE_UNAVAILABLE);
            var identity=await resolver.ResolveAsync(context,context.RequestAborted);
            if(identity is not null && identity.Principal.Identity?.IsAuthenticated!=true) throw new ApiErrorException(ApiErrorCode.AUTHENTICATION_REQUIRED);
            if (identity is null && (context.Request.Cookies.ContainsKey(SafeCookiePolicy.AuthCookieName) || context.User.Identity?.IsAuthenticated==true)) throw new ApiErrorException(ApiErrorCode.AUTHENTICATION_REQUIRED);
            context.User=identity?.Principal ?? new ClaimsPrincipal(new ClaimsIdentity());
            if (policy.Access==CsrfAccess.Authenticated && identity is null) throw new ApiErrorException(ApiErrorCode.AUTHENTICATION_REQUIRED);
            var unsafeMethod=!HttpMethods.IsGet(context.Request.Method) && !HttpMethods.IsHead(context.Request.Method) && !HttpMethods.IsOptions(context.Request.Method) && !HttpMethods.IsTrace(context.Request.Method);
            var origins=context.Request.Headers.Origin.ToArray();
            if (unsafeMethod || origins.Length>0)
            {
                if (!SameOrigin.Matches(origin,origins!)) throw new ApiErrorException(ApiErrorCode.CSRF_INVALID);
            }
            if (unsafeMethod)
            {
                if (context.Request.Headers["X-CSRF-TOKEN"].Count!=1) throw new ApiErrorException(ApiErrorCode.CSRF_INVALID);
                try { await context.RequestServices.GetRequiredService<IAntiforgery>().ValidateRequestAsync(context); }
                catch(AntiforgeryValidationException) { throw new ApiErrorException(ApiErrorCode.CSRF_INVALID); }
            }
            if ((policy.Access==CsrfAccess.AnonymousOnly && identity is not null) || (policy.RequiresNormalScope && identity?.Restricted==true)) throw new ApiErrorException(ApiErrorCode.FORBIDDEN);
            context.Items[VerifiedBoundary]=true;
            await next(context);
        });
    }

    public static IEndpointConventionBuilder MapCsrfContext(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapGet("/api/v1/auth/csrf",(HttpContext context,IAntiforgery antiforgery)=>
        {
            if (!context.Items.ContainsKey(VerifiedBoundary)) throw new ApiErrorException(ApiErrorCode.SERVICE_UNAVAILABLE);
            if (context.Request.Query.Count!=0 || (context.Features.Get<Microsoft.AspNetCore.Http.Features.IHttpRequestBodyDetectionFeature>()?.CanHaveBody ?? context.Request.ContentLength is >0)) throw ApiErrorException.Validation();
            var tokens=antiforgery.GetAndStoreTokens(context);
            if (tokens.RequestToken is null) throw new ApiErrorException(ApiErrorCode.SERVICE_UNAVAILABLE);
            return Results.Json(new Dictionary<string,string> { ["requestToken"]=tokens.RequestToken });
        }).WithName("GetCsrfContext").Produces<Dictionary<string,string>>(StatusCodes.Status200OK).WithMetadata(new CsrfRoutePolicy(CsrfAccess.AnonymousOrCurrent)).AllowAnonymous();
}
