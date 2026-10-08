using EnglishCenter.Api.Api.Auth;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Common.Security;
using EnglishCenter.Api.Infrastructure.Readiness;

namespace EnglishCenter.Api.Api.Runtime;

public sealed class LocalTransportBinding
{
    private LocalTransportBinding(WebProfile web, IServiceProvider services) { Web = web; Services = services; }
    public WebProfile Web { get; }
    internal IServiceProvider Services { get; }
    public override string ToString() => nameof(LocalTransportBinding);
    public static LocalTransportBinding Install(IApplicationBuilder app, RuntimeProfile profile, ICsrfIdentityResolver resolver)
    {
        ArgumentNullException.ThrowIfNull(app); ArgumentNullException.ThrowIfNull(resolver);
        if (RuntimeProfileValidator.Validate(profile) != ProfileIssue.None ||
            profile.Kind is not (RuntimeProfileKind.LOCAL_DEV or RuntimeProfileKind.LOCAL_DEMO) ||
            profile.Web!.ForwardingEnabled != false || !SameOrigin.IsCanonicalHttpsOrigin(profile.Web.Origin!) || new Uri(profile.Web.Origin!).Port != profile.Web.ListenPort)
            throw new ArgumentException("Invalid direct local transport profile");
        var web = profile.Web;
        app.Use(async (context, next) =>
        {
            if (!context.Request.IsHttps || "https://" + context.Request.Host.Value != web.Origin)
                throw new ApiErrorException(ApiErrorCode.SERVICE_UNAVAILABLE);
            await next(context);
        });
        app.UseCsrfBoundary(web.Origin!, resolver);
        return new(web, app.ApplicationServices);
    }
}
