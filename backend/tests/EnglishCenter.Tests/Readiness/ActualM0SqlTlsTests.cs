using EnglishCenter.Api.Infrastructure.Maintenance;
using EnglishCenter.Api.Infrastructure.Readiness;
using System.Security.Principal;
using Xunit;

namespace EnglishCenter.Tests.Readiness;

public static class M0SqlTlsGate
{
    public static bool Runtime => IsIdentity("EcM0Runtime");
    public static bool Maintenance => IsIdentity("EcM0Maint");
    public static bool Schema => IsIdentity("EcM0Schema");
    private static bool IsIdentity(string name) => OperatingSystem.IsWindows() &&
        Environment.GetEnvironmentVariable("EC_M0_SQL_TLS") == "1" &&
        WindowsIdentity.GetCurrent().Name == Environment.MachineName + "\\" + name;
}

// Only database/maintenance probes: other seven dependencies and host remain unverified.
public sealed class ActualM0SqlTlsTests
{
    private const string Connection = "Server=lpc:.\\SQLEXPRESS;Database=EnglishCenter_M0_LOCALDEV;Integrated Security=True;Encrypt=True;TrustServerCertificate=False;HostNameInCertificate=localhost;Pooling=False";
    private static RuntimeProfile Profile => new(RuntimeProfileKind.LOCAL_DEV,
        new(10, 10, 17, "Express", "proof-only:patch-unverified"),
        new("m0-local-runtime", DatabaseReadinessProbe.SchemaManifestReference, 160, 160, true),
        new("https://localhost:8443", "127.0.0.1", 8443, false, [], [], "proof-only:web-unverified", "proof-only:trust-unverified"),
        new(["proof-only:jwt-unverified"], "english-center-m0-local", "english-center-m0-local-ui", "proof-only:csrf-unverified"),
        new(3000, 10, 30000, 25000, 5000, 262144, 5000000, 10000),
        new("m0-replay-cleanup", 60, "m0-audit-cleanup", "m0-local-maintenance"), "Asia/Ho_Chi_Minh", "proof-only:blocklist-unverified");

    [Fact(Skip = "M0 actual runtime identity/TLS opt-in", SkipUnless = nameof(M0SqlTlsGate.Runtime), SkipType = typeof(M0SqlTlsGate))]
    public async Task RuntimeActualTlsAnd880CatalogPassOnFreshConnections()
    {
        var probe = new DatabaseReadinessProbe("m0-local-runtime", _ => ValueTask.FromResult<string?>(Connection));
        Assert.Equal(ReadinessProbeStatus.Pass, await probe.CheckAsync(Profile, TestContext.Current.CancellationToken));
        Assert.Equal(ReadinessProbeStatus.Pass, await probe.CheckAsync(Profile, TestContext.Current.CancellationToken));
        Assert.Equal(2, probe.CompletedPrincipalChecks);
    }

    [Fact(Skip = "M0 actual runtime identity/TLS opt-in", SkipUnless = nameof(M0SqlTlsGate.Runtime), SkipType = typeof(M0SqlTlsGate))]
    public async Task WrongCertificateHostnameFailsBeforePrincipalProof()
    {
        var probe = new DatabaseReadinessProbe("m0-local-runtime", _ => ValueTask.FromResult<string?>(
            Connection.Replace("HostNameInCertificate=localhost", "HostNameInCertificate=wrong.example.invalid", StringComparison.Ordinal)));
        Assert.Equal(ReadinessProbeStatus.NotReady, await probe.CheckAsync(Profile, TestContext.Current.CancellationToken));
        Assert.Equal(0, probe.CompletedPrincipalChecks);
    }

    [Fact(Skip = "M0 actual maintenance identity/TLS opt-in", SkipUnless = nameof(M0SqlTlsGate.Maintenance), SkipType = typeof(M0SqlTlsGate))]
    public async Task MaintenanceActualTlsAndLeastPrivilegeProbePassWithoutCleanup()
    {
        using var jobs = new SqlMaintenanceService(Profile.Maintenance!, Profile.Execution!, 3600,
            TimeProvider.System, _ => ValueTask.FromResult<string?>(Connection));
        var probe = new MaintenanceReadinessProbe(jobs);
        Assert.Equal(ReadinessProbeStatus.Pass, await probe.CheckAsync(Profile, TestContext.Current.CancellationToken));
        Assert.Equal(ReadinessProbeStatus.Pass, await probe.CheckAsync(Profile, TestContext.Current.CancellationToken));
        Assert.Equal(0, jobs.FailedRuns);
    }
}
