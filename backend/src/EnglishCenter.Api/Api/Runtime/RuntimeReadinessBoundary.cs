using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Infrastructure.Readiness;

namespace EnglishCenter.Api.Api.Runtime;

public sealed class RuntimeReadinessBoundary
{
    private readonly RuntimeReadinessGate gate;
    private readonly Func<CancellationToken, Task<RuntimeProfile?>> profiles;
    private readonly int profileLoadDeadlineMs;

    public RuntimeReadinessBoundary(RuntimeReadinessGate gate,
        Func<CancellationToken, Task<RuntimeProfile?>> profiles, int profileLoadDeadlineMs)
    {
        this.gate = gate ?? throw new ArgumentNullException(nameof(gate));
        this.profiles = profiles ?? throw new ArgumentNullException(nameof(profiles));
        if (profileLoadDeadlineMs <= 0) throw new ArgumentOutOfRangeException(nameof(profileLoadDeadlineMs));
        this.profileLoadDeadlineMs = profileLoadDeadlineMs;
    }

    public async Task<RuntimeReadinessResult> EvaluateAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        RuntimeProfile? profile;
        using (var loading = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
        {
            loading.CancelAfter(profileLoadDeadlineMs);
            try
            {
                var pending = profiles(loading.Token);
                ProbeWait.Observe(pending);
                profile = await pending.WaitAsync(loading.Token);
                loading.Token.ThrowIfCancellationRequested();
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
            catch (OperationCanceledException) when (loading.IsCancellationRequested)
            { return new(ReadinessFailure.Deadline); }
            catch (Exception)
            { cancellationToken.ThrowIfCancellationRequested(); return new(ReadinessFailure.DependencyUnavailable); }
        }
        return await gate.EvaluateAsync(profile, cancellationToken);
    }

    public async Task RequireReadyAsync(CancellationToken cancellationToken)
    {
        if (!(await EvaluateAsync(cancellationToken)).IsReady)
            throw new ApiErrorException(ApiErrorCode.SERVICE_UNAVAILABLE);
    }

    public void Install(IApplicationBuilder app, string livenessPath, string readinessPath)
    {
        ArgumentNullException.ThrowIfNull(app);
        if (!ValidHealthPath(livenessPath) || !ValidHealthPath(readinessPath) ||
            string.Equals(livenessPath, readinessPath, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Invalid explicit health paths");

        app.Use(async (context, next) =>
        {
            var live = context.Request.Path.Value == livenessPath;
            var ready = context.Request.Path.Value == readinessPath;
            if (live || ready)
            {
                context.Response.Headers.CacheControl = "no-store";
                context.Response.Headers.Pragma = "no-cache";
                context.Response.ContentType = "text/plain; charset=utf-8";
                if (!HttpMethods.IsGet(context.Request.Method) && !HttpMethods.IsHead(context.Request.Method))
                { context.Response.StatusCode = 405; context.Response.Headers.Allow = "GET, HEAD"; return; }
                var result = ready ? await EvaluateAsync(context.RequestAborted) : null;
                context.Response.StatusCode = result?.StatusCode ?? 200;
                if (!HttpMethods.IsHead(context.Request.Method))
                    await context.Response.WriteAsync(result?.Body ?? "alive", context.RequestAborted);
                return;
            }
            if (context.Request.Path.StartsWithSegments("/api/v1"))
                await RequireReadyAsync(context.RequestAborted);
            await next(context);
        });
    }

    private static bool ValidHealthPath(string? value)
    {
        return !string.IsNullOrEmpty(value) && value.Length <= 128 && value.StartsWith('/') &&
            !new PathString(value).StartsWithSegments("/api/v1") && value.Split('/').Skip(1).All(segment => segment.Length > 0) &&
            value.All(c => char.IsAsciiLetterOrDigit(c) || c is '/' or '-' or '_');
    }

    public override string ToString() => nameof(RuntimeReadinessBoundary);
}
