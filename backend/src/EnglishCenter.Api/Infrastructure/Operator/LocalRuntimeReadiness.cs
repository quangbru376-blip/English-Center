using EnglishCenter.Api.Api.Runtime;
using EnglishCenter.Api.Infrastructure.Maintenance;
using EnglishCenter.Api.Infrastructure.Readiness;

namespace EnglishCenter.Api.Infrastructure.Operator;

internal static class LocalRuntimeReadiness
{
    public static RuntimeReadinessBoundary Bind(LocalRuntimeKeyStore keys, Func<LocalTransportBinding?> transport,
        ExecutionBoundaryBinding execution, string corpusDirectory, Func<bool> uiIsCurrent)
    {
        var clock = TimeProvider.System;
        IRuntimeReadinessProbe[] probes =
        [
            new FreshManifestProbe(uiIsCurrent), new TimeZoneReadinessProbe(clock),
            new DatabaseReadinessProbe(LocalRuntimeReferences.RuntimeConnection, Connection),
            new DeferredTransportProbe(keys, transport),
            new JwtKeyReadinessProbe(clock, keys.ResolveJwt),
            new ProtectedCsrfReadinessProbe(LocalRuntimeReferences.Csrf, Path.Combine(LocalRuntimeReferences.RuntimeRoot, "csrf"), 16, 65536, clock),
            new OfflineBlocklistReadinessProbe(LocalRuntimeReferences.Blocklist, corpusDirectory, clock),
            new ExecutionLimitsReadinessProbe(execution), new MaintenanceProxyReadinessProbe()
        ];
        return new(new RuntimeReadinessGate(probes), token => LocalRuntimeReferences.LoadProfileAsync(LocalRuntimeReferences.RuntimeRoot, token),
            LocalRuntimeReferences.Execution.ReadinessMs);
    }
    private static ValueTask<string?> Connection(CancellationToken token)
    { token.ThrowIfCancellationRequested(); return ValueTask.FromResult<string?>(LocalRuntimeReferences.SqlConnection); }
    private sealed class DeferredTransportProbe(LocalRuntimeKeyStore keys, Func<LocalTransportBinding?> binding) : IRuntimeReadinessProbe
    {
        public ReadinessDependency Dependency => ReadinessDependency.Transport;
        public Task<ReadinessProbeStatus> CheckAsync(RuntimeProfile profile, CancellationToken token)
            => new TransportReadinessProbe(binding(), TimeProvider.System, LocalRuntimeReferences.RuntimeConnection,
                keys.ResolveTrust, Connection).CheckAsync(profile, token);
    }
    private sealed class FreshManifestProbe(Func<bool> uiIsCurrent) : IRuntimeReadinessProbe
    {
        public ReadinessDependency Dependency => ReadinessDependency.RuntimeManifest;
        public async Task<ReadinessProbeStatus> CheckAsync(RuntimeProfile profile, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            try
            {
                if (!uiIsCurrent()) return ReadinessProbeStatus.NotReady;
                var manifest = LocalRuntimeManifest.Load(LocalRuntimeReferences.RuntimeRoot);
                return await new RuntimeManifestReadinessProbe(LocalRuntimeReferences.Patch, LocalRuntimeReferences.Workspace,
                    manifest, TimeProvider.System, LocalRuntimeReferences.RuntimeConnection, LocalRuntimeManifest.ReadSqlVersionAsync).CheckAsync(profile, token);
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested) { throw; }
            catch (Exception) { token.ThrowIfCancellationRequested(); return ReadinessProbeStatus.NotReady; }
        }
    }
}
