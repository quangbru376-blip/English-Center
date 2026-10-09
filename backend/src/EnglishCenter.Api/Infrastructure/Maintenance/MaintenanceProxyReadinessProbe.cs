using System.Security.Cryptography;
using System.Security.Principal;
using EnglishCenter.Api.Infrastructure.Operator;
using EnglishCenter.Api.Infrastructure.Readiness;

namespace EnglishCenter.Api.Infrastructure.Maintenance;

public sealed class MaintenanceProxyReadinessProbe : IRuntimeReadinessProbe
{
    public ReadinessDependency Dependency => ReadinessDependency.MaintenanceConfiguration;
    public async Task<ReadinessProbeStatus> CheckAsync(RuntimeProfile profile, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!OperatingSystem.IsWindows() || !LocalRuntimeReferences.IsBound(profile)) return ReadinessProbeStatus.NotReady;
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        deadline.CancelAfter(profile.Execution!.ReadinessMs);
        try
        {
            using var identity = WindowsIdentity.GetCurrent();
            if (identity.Name != Environment.MachineName + @"\EcM0Runtime") return ReadinessProbeStatus.NotReady;
            using var custody = MaintenanceProofCustody.Open(privateKey: false);
            if (identity.User?.Value != custody.Descriptor.RuntimeSid) return ReadinessProbeStatus.NotReady;
            using var pipe = await LocalMaintenancePipe.ConnectAsync(custody.Descriptor.PipeName, deadline.Token);
            var request = MaintenanceProofCodec.CreateRequest(RandomNumberGenerator.GetBytes(32), MaintenanceProofService.ProfileHash(profile));
            await pipe.WriteAsync(request, deadline.Token); await pipe.FlushAsync(deadline.Token);
            var response = new byte[325]; await pipe.ReadExactlyAsync(response, deadline.Token);
            if (!custody.IsCurrent() || !MaintenanceProofCodec.VerifyResponse(request, response, custody.Key, out var ready)) return ReadinessProbeStatus.NotReady;
            await pipe.WriteAsync(new byte[] { 0xAC }, deadline.Token);
            if (await pipe.ReadAsync(new byte[1], deadline.Token) != 0 || !custody.IsCurrent()) return ReadinessProbeStatus.NotReady;
            return ready ? ReadinessProbeStatus.Pass : ReadinessProbeStatus.NotReady;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
        catch (Exception) { cancellationToken.ThrowIfCancellationRequested(); return ReadinessProbeStatus.NotReady; }
    }
}
