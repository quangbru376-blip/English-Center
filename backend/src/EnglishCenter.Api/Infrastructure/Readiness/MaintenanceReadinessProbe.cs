using EnglishCenter.Api.Infrastructure.Maintenance;

namespace EnglishCenter.Api.Infrastructure.Readiness;

public sealed class MaintenanceReadinessProbe(SqlMaintenanceService? jobs) : IRuntimeReadinessProbe
{
    public ReadinessDependency Dependency => ReadinessDependency.MaintenanceConfiguration;
    public async Task<ReadinessProbeStatus> CheckAsync(RuntimeProfile profile, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (RuntimeProfileValidator.Validate(profile) != ProfileIssue.None || jobs is null || !jobs.HasFiniteSchedules ||
            jobs.Configuration != profile.Maintenance || jobs.Execution != profile.Execution ||
            profile.Maintenance!.MaintenancePrincipalReference == profile.Database!.ConnectionSecretReference)
            return ReadinessProbeStatus.NotReady;
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        deadline.CancelAfter(profile.Execution!.ReadinessMs);
        try
        {
            await using var connection = await jobs.OpenAsync(deadline.Token);
            if (connection is null) return ReadinessProbeStatus.NotReady;
            var safe = await jobs.HasSafePrincipalAsync(connection, deadline.Token);
            deadline.Token.ThrowIfCancellationRequested();
            return safe ? ReadinessProbeStatus.Pass : ReadinessProbeStatus.NotReady;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
        catch (Exception)
        { cancellationToken.ThrowIfCancellationRequested(); return ReadinessProbeStatus.NotReady; }
    }
}
