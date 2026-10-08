using System.Collections.Immutable;

namespace EnglishCenter.Api.Infrastructure.Readiness;

public enum RuntimeProfileKind { LOCAL_DEV = 1, LOCAL_DEMO, LAN_DEMO, INTERNET_DEMO }

// Descriptors contain references, never loaded key material or connection strings.
public sealed record RuntimeProfile(RuntimeProfileKind Kind, RuntimeTarget? Runtime,
    DatabaseProfile? Database, WebProfile? Web, AuthKeyProfile? Auth,
    ExecutionProfile? Execution, MaintenanceProfile? Maintenance,
    string? BusinessTimeZone, string? BlocklistManifestReference)
{
    public override string ToString() => nameof(RuntimeProfile);
}

public sealed record RuntimeTarget(int DotNetMajor, int EfMajor, int SqlMajor,
    string? SqlEdition, string? PatchManifestReference)
{
    public override string ToString() => nameof(RuntimeTarget);
}

public sealed record DatabaseProfile(string? ConnectionSecretReference,
    string? SchemaManifestReference, int DatabaseCompatibility, int EfCompatibility,
    bool? SnapshotRequired)
{
    public override string ToString() => nameof(DatabaseProfile);
}

public sealed record WebProfile(string? Origin, string? ListenAddress, int ListenPort,
    bool? ForwardingEnabled, ImmutableArray<string> KnownProxies,
    ImmutableArray<string> KnownNetworks, string? CertificateSecretReference,
    string? ClientTrustReference)
{
    public override string ToString() => nameof(WebProfile);
}

public sealed record AuthKeyProfile(ImmutableArray<string> JwtKeySecretReferences,
    string? Issuer, string? Audience, string? ProtectedPersistedCsrfStoreReference)
{
    public override string ToString() => nameof(AuthKeyProfile);
}

public sealed record ExecutionProfile(int LockMs, int SqlSeconds, int RequestMs,
    int CsvMs, int ReadinessMs, int RequestBodyBytes, int ResponseBufferBytes,
    int WeeklyPreviewBudget)
{
    public override string ToString() => nameof(ExecutionProfile);
}

public sealed record MaintenanceProfile(string? ReplayCleanupJobReference,
    int ReplayCleanupIntervalSeconds, string? SecurityAuditCleanupJobReference,
    string? MaintenancePrincipalReference)
{
    public override string ToString() => nameof(MaintenanceProfile);
}
