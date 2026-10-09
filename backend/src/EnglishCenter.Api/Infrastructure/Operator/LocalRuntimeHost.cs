using System.Net;
using System.Security.Principal;
using EnglishCenter.Api.Api.Identity;
using EnglishCenter.Api.Api.Runtime;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Infrastructure.Readiness;

namespace EnglishCenter.Api.Infrastructure.Operator;

public static class LocalRuntimeHost
{
    public sealed class Session : IAsyncDisposable
    {
        private readonly LocalRuntimeKeyStore keys;
        private readonly LocalRuntimeServices services;
        private readonly LocalUiFiles files;
        public WebApplication Application { get; }
        public RuntimeReadinessBoundary Readiness { get; }
        public StartupReadinessService Startup { get; }
        internal Session(WebApplication application, RuntimeReadinessBoundary readiness, StartupReadinessService startup,
            LocalRuntimeKeyStore keys, LocalRuntimeServices services, LocalUiFiles files)
        { Application = application; Readiness = readiness; Startup = startup; this.keys = keys; this.services = services; this.files = files; }
        public async ValueTask DisposeAsync()
        { try { await Application.DisposeAsync(); } finally { services.Dispose(); keys.Dispose();  } }
    }
    public static async Task<Session?> CreateAsync(CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        if (!OperatingSystem.IsWindows()) return null;
        using var identity = WindowsIdentity.GetCurrent();
        if (identity.Name != Environment.MachineName + @"\EcM0Runtime" || new WindowsPrincipal(identity).IsInRole(WindowsBuiltInRole.Administrator)) return null;
        var profile = await LocalRuntimeReferences.LoadProfileAsync(LocalRuntimeReferences.RuntimeRoot, token);
        if (profile is null) return null;
        LocalRuntimeKeyStore? keys = null; LocalRuntimeServices? services = null; LocalUiFiles? files = null; WebApplication? app = null;
        try
        {
            keys = LocalRuntimeKeyStore.Open();
            var corpus = Path.Combine(AppContext.BaseDirectory, "Resources", "PasswordBlocklist");
            services = new(profile, keys, corpus);
            var entry = LocalReadinessFiles.Resolve(LocalRuntimeReferences.Workspace, "frontend/dist/index.html");
            if (entry is null) throw new InvalidOperationException("Unavailable UI artifact");
            files = new LocalUiFiles(Path.GetDirectoryName(entry)!);
            var builder = WebApplication.CreateBuilder(new WebApplicationOptions { ContentRootPath = AppContext.BaseDirectory, EnvironmentName = Environments.Production });
            builder.Logging.ClearProviders();
            builder.WebHost.ConfigureKestrel(options =>
            {
                options.Limits.MaxRequestBodySize = profile.Execution!.RequestBodyBytes;
                options.Listen(IPAddress.Loopback, profile.Web!.ListenPort, listener => listener.UseHttps(keys.HttpsCertificate));
            });
            LocalRuntimeServices.RegisterCsrf(builder.Services);
            RuntimeReadinessBoundary? boundary = null;
            StartupReadinessService? startup = null;
            builder.Services.AddHostedService(provider => startup = new StartupReadinessService(boundary ?? throw new InvalidOperationException("Missing readiness binding"), provider.GetRequiredService<IHostApplicationLifetime>()));
            app = builder.Build();
            app.UseApiErrorMiddleware(); app.UseRouting();
            var execution = ExecutionBoundaryBinding.Install(app, profile);
            LocalTransportBinding? transport = null;
            boundary = LocalRuntimeReadiness.Bind(keys, () => transport, execution, corpus, files.IsCurrent);
            boundary.Install(app, "/health/live", "/health/ready");
            app.UseAuthJsonBoundary();
            transport = LocalTransportBinding.Install(app, profile, services.Identity);
            services.Map(app, profile.Execution!.RequestBodyBytes);
            var staticOptions = new StaticFileOptions { FileProvider = files, OnPrepareResponse = response => response.Context.Response.Headers.CacheControl = "no-store" };
            app.UseStaticFiles(staticOptions);
            app.MapFallbackToFile("index.html", staticOptions);
            // Hosted services are activated on StartAsync, after the complete pipeline exists.
            startup = app.Services.GetServices<IHostedService>().OfType<StartupReadinessService>().Single();
            return new(app, boundary, startup, keys, services, files);
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        { if (app is not null) await app.DisposeAsync(); services?.Dispose(); keys?.Dispose();  throw; }
        catch (Exception)
        { if (app is not null) await app.DisposeAsync(); services?.Dispose(); keys?.Dispose();  return null; }
    }
    public static async Task<int> RunAsync(CancellationToken token)
    {
        try
        {
            await using var session = await CreateAsync(token);
            if (session is null) return 1;
            await session.Application.RunAsync(token);
            return session.Startup.HasRefusedStartup ? 1 : 0;
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested) { return 0; }
        catch (Exception) { return 1; }
    }
}
