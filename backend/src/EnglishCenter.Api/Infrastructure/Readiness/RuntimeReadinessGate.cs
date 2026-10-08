namespace EnglishCenter.Api.Infrastructure.Readiness;

public enum ReadinessDependency { RuntimeManifest = 1, TimeZone, DatabaseAuthorityAndCatalog,
    Transport, JwtKeys, ProtectedCsrfKeys, OfflineBlocklist, ExecutionLimits, MaintenanceConfiguration }
public enum ReadinessFailure { None, InvalidProfile, ProbeConfiguration, DependencyUnavailable, Deadline }
public enum ReadinessProbeStatus { NotReady, Pass }

// Concrete probes must be read-only, cancellation-aware, and prove their whole category.
// Database evidence must use the runtime principal (never diagnostic/owner mode).
public interface IRuntimeReadinessProbe
{
    ReadinessDependency Dependency { get; }
    Task<ReadinessProbeStatus> CheckAsync(RuntimeProfile profile, CancellationToken cancellationToken);
}

public sealed class RuntimeReadinessResult
{
    internal RuntimeReadinessResult(ReadinessFailure failure, ProfileIssue profileIssue = ProfileIssue.None)
    { Failure = failure; ProfileIssue = profileIssue; }
    public ReadinessFailure Failure { get; }
    public ProfileIssue ProfileIssue { get; }
    public bool IsReady => Failure == ReadinessFailure.None;
    public int StatusCode => IsReady ? 200 : 503;
    public string Body => IsReady ? "ready" : "not_ready";
    public override string ToString() => Body;
}

// Unmounted orchestration seam; no probe implementations, registration, or health route.
public sealed class RuntimeReadinessGate
{
    private readonly IRuntimeReadinessProbe[] probes;

    public RuntimeReadinessGate(IEnumerable<IRuntimeReadinessProbe>? probes)
    {
        this.probes = probes?.ToArray() ?? [];
    }

    private bool HasRequiredProbes()
    {
        try
        {
            var required = Enum.GetValues<ReadinessDependency>();
            return probes.Length == required.Length &&
                probes.All(probe => probe is not null && Enum.IsDefined(probe.Dependency)) &&
                probes.Select(probe => probe.Dependency).Distinct().Count() == required.Length;
        }
        catch (Exception) { return false; }
    }

    public async Task<RuntimeReadinessResult> EvaluateAsync(RuntimeProfile? profile,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var issue = RuntimeProfileValidator.Validate(profile);
        if (issue != ProfileIssue.None) return new(ReadinessFailure.InvalidProfile, issue);
        var validProbes = HasRequiredProbes();
        cancellationToken.ThrowIfCancellationRequested();
        if (!validProbes) return new(ReadinessFailure.ProbeConfiguration);
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        deadline.CancelAfter(profile!.Execution!.ReadinessMs);
        foreach (var probe in probes)
        {
            try
            {
                deadline.Token.ThrowIfCancellationRequested();
                var pending = probe.CheckAsync(profile, deadline.Token);
                // A timed-out asynchronous implementation must not leave an unobserved late fault.
                _ = pending.ContinueWith(static task => { _ = task.Exception; },
                    CancellationToken.None, TaskContinuationOptions.OnlyOnFaulted |
                    TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
                var status = await pending.WaitAsync(deadline.Token);
                deadline.Token.ThrowIfCancellationRequested();
                if (status != ReadinessProbeStatus.Pass) return new(ReadinessFailure.DependencyUnavailable);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            { throw; }
            catch (OperationCanceledException) when (deadline.IsCancellationRequested)
            { return new(ReadinessFailure.Deadline); }
            catch (Exception)
            { cancellationToken.ThrowIfCancellationRequested(); return new(ReadinessFailure.DependencyUnavailable); }
        }
        cancellationToken.ThrowIfCancellationRequested();
        if (deadline.IsCancellationRequested) return new(ReadinessFailure.Deadline);
        return new(ReadinessFailure.None);
    }
}
