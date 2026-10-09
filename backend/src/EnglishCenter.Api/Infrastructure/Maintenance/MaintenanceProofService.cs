using System.Security.Cryptography;
using System.Text.Json;
using EnglishCenter.Api.Infrastructure.Operator;
using EnglishCenter.Api.Infrastructure.Readiness;

namespace EnglishCenter.Api.Infrastructure.Maintenance;

internal sealed class MaintenanceProofService(SqlMaintenanceService jobs, MaintenanceProofCustody custody,
    IHostApplicationLifetime lifetime) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!OperatingSystem.IsWindows()) throw new PlatformNotSupportedException();
        using var pipe = LocalMaintenancePipe.CreateServer(custody.Descriptor.PipeName, custody.Descriptor.MaintenanceSid, custody.Descriptor.RuntimeSid);
        while (!stoppingToken.IsCancellationRequested)
        {
            if (!custody.IsCurrent()) throw new InvalidOperationException("Maintenance custody unavailable");
            await pipe.WaitForConnectionAsync(stoppingToken);
            using var deadline = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
            deadline.CancelAfter(LocalRuntimeReferences.Execution.ReadinessMs);
            try
            {
                var request = new byte[68]; await pipe.ReadExactlyAsync(request, deadline.Token);
                if (LocalMaintenancePipe.ReadClientSid(pipe) != custody.Descriptor.RuntimeSid) continue;
                if (!MaintenanceProofCodec.TryReadRequest(request, out _, out var hash)) continue;
                var profile = await LocalRuntimeReferences.LoadProfileAsync(LocalRuntimeReferences.MaintenanceRoot, deadline.Token);
                var ready = profile is not null && CryptographicOperations.FixedTimeEquals(hash, ProfileHash(profile)) && IsLive() && custody.IsCurrent();
                if (ready) ready = await new MaintenanceReadinessProbe(jobs).CheckAsync(profile!, deadline.Token) == ReadinessProbeStatus.Pass;
                ready = ready && IsLive() && custody.IsCurrent();
                var response = MaintenanceProofCodec.CreateResponse(request, ready, custody.Key);
                await pipe.WriteAsync(response, deadline.Token); await pipe.FlushAsync(deadline.Token);
                var acknowledgment = new byte[1]; await pipe.ReadExactlyAsync(acknowledgment, deadline.Token);
                if (acknowledgment[0] != 0xAC) throw new IOException("Invalid maintenance acknowledgment");
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { throw; }
            catch (IOException) { }
            catch (OperationCanceledException) { }
            finally { if (pipe.IsConnected) pipe.Disconnect(); }
        }
    }
    private bool IsLive() => lifetime.ApplicationStarted.IsCancellationRequested && !lifetime.ApplicationStopping.IsCancellationRequested &&
        jobs.HasFiniteSchedules && jobs.ExecuteTask is { IsCompleted: false };
    internal static byte[] ProfileHash(RuntimeProfile profile) => SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(profile));
}
