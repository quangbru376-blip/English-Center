using EnglishCenter.Api.Api.Runtime;

namespace EnglishCenter.Api.Infrastructure.Readiness;

public sealed class ExecutionLimitsReadinessProbe(ExecutionBoundaryBinding? binding) : IRuntimeReadinessProbe
{
    public ReadinessDependency Dependency => ReadinessDependency.ExecutionLimits;
    public Task<ReadinessProbeStatus> CheckAsync(RuntimeProfile profile, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(RuntimeProfileValidator.Validate(profile) == ProfileIssue.None &&
            binding is not null && binding.Limits == profile.Execution &&
            binding.Limits.ResponseBufferBytes >= 5_000_000 && binding.Limits.WeeklyPreviewBudget > 0
            ? ReadinessProbeStatus.Pass : ReadinessProbeStatus.NotReady);
    }
}
