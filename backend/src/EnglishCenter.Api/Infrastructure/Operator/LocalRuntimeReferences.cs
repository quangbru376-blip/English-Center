using EnglishCenter.Api.Infrastructure.Readiness;

namespace EnglishCenter.Api.Infrastructure.Operator;

public static class LocalRuntimeReferences
{
    public const string Workspace = @"C:\Study\SOEN\main_workplace";
    public const string RuntimeRoot = @"C:\ProgramData\EnglishCenter\M0Runtime";
    public const string MaintenanceRoot = @"C:\ProgramData\EnglishCenter\M0Maintenance";
    public const string RuntimeConnection = "m0-local-runtime";
    public const string MaintenanceConnection = "m0-local-maintenance";
    public const string Patch = "m0-patch-reviewed-v1";
    public const string Https = "m0-runtime-https";
    public const string HttpsTrust = "m0-runtime-https-leaf";
    public const string Jwt = "m0-jwt-v1";
    public const string Csrf = "m0-csrf-current-user";
    public const string Blocklist = "m0-password-corpus";
    public const string SqlConnection = "Server=lpc:.\\SQLEXPRESS;Database=EnglishCenter_M0_LOCALDEV;Integrated Security=True;Encrypt=True;TrustServerCertificate=False;HostNameInCertificate=localhost;Pooling=False;Enlist=False;ConnectRetryCount=0;Connect Timeout=5";
    public static ExecutionProfile Execution => new(3000, 10, 30000, 25000, 5000, 262144, 5000000, 10000);
    public static MaintenanceProfile Maintenance => new("m0-replay-cleanup", 60, "m0-audit-cleanup", MaintenanceConnection);

    public static bool IsBound(RuntimeProfile? profile) => profile is not null &&
        profile.Kind == RuntimeProfileKind.LOCAL_DEV &&
        profile.Runtime == new RuntimeTarget(10, 10, 17, "Express", Patch) &&
        profile.Database == new DatabaseProfile(RuntimeConnection, DatabaseReadinessProbe.SchemaManifestReference, 160, 160, true) &&
        profile.Execution == Execution && profile.Maintenance == Maintenance &&
        profile.BusinessTimeZone == "Asia/Ho_Chi_Minh" && profile.BlocklistManifestReference == Blocklist &&
        profile.Web is { Origin: "https://localhost:8443", ListenAddress: "127.0.0.1", ListenPort: 8443,
            ForwardingEnabled: false, CertificateSecretReference: Https, ClientTrustReference: HttpsTrust } web &&
        !web.KnownProxies.IsDefault && web.KnownProxies.IsEmpty && !web.KnownNetworks.IsDefault && web.KnownNetworks.IsEmpty &&
        profile.Auth is { Issuer: "english-center-m0-local", Audience: "english-center-m0-local-ui",
            ProtectedPersistedCsrfStoreReference: Csrf } auth &&
        !auth.JwtKeySecretReferences.IsDefault && auth.JwtKeySecretReferences.Length == 1 && auth.JwtKeySecretReferences[0] == Jwt;

    public static async Task<RuntimeProfile?> LoadProfileAsync(string root, CancellationToken token)
    {
        var result = await new RuntimeProfileReader(Path.Combine(root, "profile.json"), 65536, Execution.ReadinessMs).LoadAsync(token);
        return result.IsValid && IsBound(result.Profile) ? result.Profile : null;
    }
}
