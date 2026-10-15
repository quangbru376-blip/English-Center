using EnglishCenter.Api.Infrastructure.Readiness;

namespace EnglishCenter.Api.Infrastructure.Operator;

public static class LocalRuntimeReferences
{
    private static int selected;
    // Select once before any binding is read. No environment/header/request override.
    public static void SelectDemo()
    { if (Interlocked.CompareExchange(ref selected, 2, 0) != 0) throw new InvalidOperationException("Runtime binding already selected"); }
    private static bool Demo { get { Interlocked.CompareExchange(ref selected, 1, 0); return Volatile.Read(ref selected) == 2; } }
    public static string Workspace => Demo ? @"C:\Study\SOEN\main_workplace\.local-demo\release" : @"C:\Study\SOEN\main_workplace";
    public static string RuntimeRoot => Demo ? @"C:\ProgramData\EnglishCenter\DemoRuntime" : @"C:\ProgramData\EnglishCenter\M0Runtime";
    public static string MaintenanceRoot => Demo ? @"C:\ProgramData\EnglishCenter\DemoMaintenance" : @"C:\ProgramData\EnglishCenter\M0Maintenance";
    public static string Issuer => Demo ? "english-center-demo-local" : "english-center-m0-local";
    public static string Audience => Demo ? "english-center-demo-local-ui" : "english-center-m0-local-ui";
    public static string Origin => Demo ? "https://localhost:8444" : "https://localhost:8443";
    public static int Port => Demo ? 8444 : 8443;
    public static string CsrfApplication => Demo ? "EnglishCenter.Demo.Local" : "EnglishCenter.M0.Local";
    public static string JwtEntropy => Demo ? "EnglishCenter.Demo.Jwt.v1" : "EnglishCenter.M0.Jwt.v1";
    public const string RuntimeConnection = "m0-local-runtime";
    public const string MaintenanceConnection = "m0-local-maintenance";
    public const string Patch = "m0-patch-reviewed-v1";
    public const string Https = "m0-runtime-https";
    public const string HttpsTrust = "m0-runtime-https-leaf";
    public const string Jwt = "m0-jwt-v1";
    public const string Csrf = "m0-csrf-current-user";
    public const string Blocklist = "m0-password-corpus";
    public static string SqlConnection => "Server=lpc:.\\SQLEXPRESS;Database=" + (Demo ? "EnglishCenter_DEMO_LOCAL" : "EnglishCenter_M0_LOCALDEV") + ";Integrated Security=True;Encrypt=True;TrustServerCertificate=False;HostNameInCertificate=localhost;Pooling=False;Enlist=False;ConnectRetryCount=0;Connect Timeout=5";
    public static ExecutionProfile Execution => new(3000, 10, 30000, 25000, 5000, 262144, 5000000, 10000);
    public static MaintenanceProfile Maintenance => new("m0-replay-cleanup", 60, "m0-audit-cleanup", MaintenanceConnection);

    public static bool IsBound(RuntimeProfile? profile) => profile is not null &&
        profile.Kind == (Demo ? RuntimeProfileKind.LOCAL_DEMO : RuntimeProfileKind.LOCAL_DEV) &&
        profile.Runtime == new RuntimeTarget(10, 10, 17, "Express", Patch) &&
        profile.Database == new DatabaseProfile(RuntimeConnection, DatabaseReadinessProbe.SchemaManifestReference, 160, 160, true) &&
        profile.Execution == Execution && profile.Maintenance == Maintenance &&
        profile.BusinessTimeZone == "Asia/Ho_Chi_Minh" && profile.BlocklistManifestReference == Blocklist &&
        profile.Web is { ListenAddress: "127.0.0.1",
            ForwardingEnabled: false, CertificateSecretReference: Https, ClientTrustReference: HttpsTrust } web &&
        web.Origin == Origin && web.ListenPort == Port &&
        !web.KnownProxies.IsDefault && web.KnownProxies.IsEmpty && !web.KnownNetworks.IsDefault && web.KnownNetworks.IsEmpty &&
        profile.Auth is { ProtectedPersistedCsrfStoreReference: Csrf } auth && auth.Issuer == Issuer && auth.Audience == Audience &&
        !auth.JwtKeySecretReferences.IsDefault && auth.JwtKeySecretReferences.Length == 1 && auth.JwtKeySecretReferences[0] == Jwt;

    public static async Task<RuntimeProfile?> LoadProfileAsync(string root, CancellationToken token)
    {
        var result = await new RuntimeProfileReader(Path.Combine(root, "profile.json"), 65536, Execution.ReadinessMs).LoadAsync(token);
        return result.IsValid && IsBound(result.Profile) ? result.Profile : null;
    }
}
