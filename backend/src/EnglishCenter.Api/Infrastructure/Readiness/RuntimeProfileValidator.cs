using System.Collections.Immutable;
using System.Net;

namespace EnglishCenter.Api.Infrastructure.Readiness;

public enum ProfileIssue { None, MissingProfile, ProfileKind, RuntimeTarget,
    Database, TimeZone, Web, AuthKeys, Blocklist, Deadlines, Limits, Maintenance }

public static class RuntimeProfileValidator
{
    public static ProfileIssue Validate(RuntimeProfile? profile)
    {
        if (profile is null) return ProfileIssue.MissingProfile;
        if (!Enum.IsDefined(profile.Kind)) return ProfileIssue.ProfileKind;
        var runtime = profile.Runtime;
        if (runtime is null || runtime.DotNetMajor != 10 || runtime.EfMajor != 10 ||
            runtime.SqlMajor != 17 || runtime.SqlEdition != "Express" ||
            !Reference(runtime.PatchManifestReference)) return ProfileIssue.RuntimeTarget;
        var database = profile.Database;
        if (database is null || !Reference(database.ConnectionSecretReference) ||
            !Reference(database.SchemaManifestReference) || database.DatabaseCompatibility != 160 ||
            database.EfCompatibility != 160 || database.SnapshotRequired != true)
            return ProfileIssue.Database;
        if (profile.BusinessTimeZone != "Asia/Ho_Chi_Minh") return ProfileIssue.TimeZone;
        if (!ValidWeb(profile.Web, profile.Kind)) return ProfileIssue.Web;
        var auth = profile.Auth;
        if (auth is null || !References(auth.JwtKeySecretReferences) ||
            !Reference(auth.Issuer) || !Reference(auth.Audience) ||
            !Reference(auth.ProtectedPersistedCsrfStoreReference)) return ProfileIssue.AuthKeys;
        if (!Reference(profile.BlocklistManifestReference)) return ProfileIssue.Blocklist;
        var execution = profile.Execution;
        if (execution is null || !DeadlineBudgets.IsValid(execution.LockMs, execution.SqlSeconds,
            execution.RequestMs, execution.CsvMs, execution.ReadinessMs)) return ProfileIssue.Deadlines;
        if (execution.RequestBodyBytes <= 0 || execution.ResponseBufferBytes < 5_000_000 ||
            execution.WeeklyPreviewBudget <= 0) return ProfileIssue.Limits;
        var maintenance = profile.Maintenance;
        if (maintenance is null || !Reference(maintenance.ReplayCleanupJobReference) ||
            maintenance.ReplayCleanupIntervalSeconds <= 0 ||
            !Reference(maintenance.SecurityAuditCleanupJobReference) ||
            !Reference(maintenance.MaintenancePrincipalReference)) return ProfileIssue.Maintenance;
        return ProfileIssue.None;
    }

    private static bool ValidWeb(WebProfile? web, RuntimeProfileKind kind)
    {
        if (web is null || !Reference(web.Origin) || !Uri.TryCreate(web.Origin, UriKind.Absolute, out var origin) ||
            origin.Scheme != Uri.UriSchemeHttps || origin.UserInfo.Length != 0 ||
            origin.AbsolutePath != "/" || origin.Query.Length != 0 || origin.Fragment.Length != 0 ||
            origin.HostNameType == UriHostNameType.Unknown || !Address(web.ListenAddress, out var listen) ||
            web.ListenPort is < 1 or > 65535 || web.ForwardingEnabled is null ||
            web.KnownProxies.IsDefault || web.KnownNetworks.IsDefault ||
            !Reference(web.CertificateSecretReference) || !Reference(web.ClientTrustReference)) return false;
        if (kind is RuntimeProfileKind.LOCAL_DEV or RuntimeProfileKind.LOCAL_DEMO)
        {
            if (!IPAddress.IsLoopback(listen)) return false;
            if (origin.Host != "localhost" &&
                (!IPAddress.TryParse(origin.IdnHost, out var host) || !IPAddress.IsLoopback(host))) return false;
        }
        if (web.ForwardingEnabled == false)
            return web.KnownProxies.IsEmpty && web.KnownNetworks.IsEmpty;
        return (!web.KnownProxies.IsEmpty || !web.KnownNetworks.IsEmpty) &&
            web.KnownProxies.All(value => Address(value, out _)) && web.KnownNetworks.All(Network);
    }

    private static bool Reference(string? value) => !string.IsNullOrWhiteSpace(value) &&
        value == value.Trim() && !value.Any(char.IsControl);

    private static bool References(ImmutableArray<string> values) => !values.IsDefaultOrEmpty &&
        values.All(Reference) && values.Distinct(StringComparer.Ordinal).Count() == values.Length;

    private static bool Address(string? value, out IPAddress address)
    {
        address = IPAddress.None;
        if (!Reference(value) || !IPAddress.TryParse(value, out var parsed) ||
            parsed.Equals(IPAddress.Any) || parsed.Equals(IPAddress.IPv6Any) ||
            parsed.Equals(IPAddress.Broadcast) || parsed.IsIPv4MappedToIPv6) return false;
        address = parsed;
        return true;
    }

    private static bool Network(string? value)
    {
        if (!Reference(value)) return false;
        var parts = value!.Split('/');
        if (parts.Length != 2 || !Address(parts[0], out var address) ||
            !int.TryParse(parts[1], System.Globalization.NumberStyles.None,
                System.Globalization.CultureInfo.InvariantCulture, out var prefix)) return false;
        var bytes = address.GetAddressBytes();
        if (prefix <= 0 || prefix > bytes.Length * 8) return false;
        // Require canonical network bits; reject trust-all /0, not an implicit trust repair.
        for (var bit = prefix; bit < bytes.Length * 8; bit++)
            if ((bytes[bit / 8] & (1 << (7 - bit % 8))) != 0) return false;
        return true;
    }
}
