using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Text;
using System.Text.Json;
using EnglishCenter.Api.Infrastructure.Readiness;
using Microsoft.IdentityModel.Tokens;

namespace EnglishCenter.Api.Infrastructure.Operator;

// Binds existing material; every resolution checks custody and file drift, never repairs.
public sealed class LocalRuntimeKeyStore : IDisposable
{
    private static readonly string[] Files = ["prepared.json", "key-manifest.json", "jwt.dpapi", "https.cer", "profile.json", "patch-manifest.json"];
    private readonly Dictionary<string, byte[]> snapshots;
    private readonly byte[] bytes;
    private readonly string sid;
    private bool disposed;
    public JwtReadinessMaterial Jwt { get; }
    public X509Certificate2 HttpsCertificate { get; }
    public string HttpsLeafSha256 { get; }

    private LocalRuntimeKeyStore(byte[] bytes, string sid, JwtReadinessMaterial jwt,
        X509Certificate2 certificate, string leaf, Dictionary<string, byte[]> snapshots)
    { this.bytes = bytes; this.sid = sid; Jwt = jwt; HttpsCertificate = certificate; HttpsLeafSha256 = leaf; this.snapshots = snapshots; }

    public static LocalRuntimeKeyStore Open()
    {
        if (!OperatingSystem.IsWindows()) throw new PlatformNotSupportedException();
        using var identity = WindowsIdentity.GetCurrent();
        if (identity.Name != Environment.MachineName + @"\EcM0Runtime" || identity.User is null)
            throw new InvalidOperationException("Invalid runtime custody identity");
        byte[]? keyBytes = null; X509Certificate2? certificate = null;
        try
        {
            if (!SafeAcl(identity.User.Value)) throw new InvalidOperationException("Invalid runtime custody");
            var snapshots = BoundFiles().ToDictionary(name => name, name => SHA256.HashData(Read(name)), StringComparer.Ordinal);
            using var prepared = JsonDocument.Parse(Read("prepared.json"));
            using var metadata = JsonDocument.Parse(Read("key-manifest.json"));
            var marker = prepared.RootElement; var record = metadata.RootElement;
            if (marker.GetProperty("runtimeSid").GetString() != identity.User.Value ||
                record.GetProperty("runtimeSid").GetString() != identity.User.Value ||
                marker.GetProperty("ownerId").GetString() != record.GetProperty("ownerId").GetString() ||
                record.GetProperty("phase").GetString() != "jwt_https_provisioned" ||
                record.GetProperty("jwtReference").GetString() != LocalRuntimeReferences.Jwt)
                throw new InvalidOperationException("Invalid runtime custody descriptor");
            keyBytes = ProtectedData.Unprotect(Read("jwt.dpapi"), Encoding.UTF8.GetBytes(LocalRuntimeReferences.JwtEntropy), DataProtectionScope.CurrentUser);
            if (keyBytes.Length != 32) throw new InvalidOperationException("Invalid runtime signing material");
            var key = new SymmetricSecurityKey(keyBytes) { CryptoProviderFactory = new CryptoProviderFactory { CacheSignatureProviders = false } };
            var material = new JwtReadinessMaterial(LocalRuntimeReferences.Jwt, new(key, "HS256"),
                LocalRuntimeReferences.Issuer, LocalRuntimeReferences.Audience,
                [new(LocalRuntimeReferences.Jwt, key, "HS256", record.GetProperty("jwtValidFrom").GetDateTimeOffset(),
                    record.GetProperty("jwtValidUntil").GetDateTimeOffset())]);
            var thumb = record.GetProperty("httpsThumbprint").GetString()!;
            var leaf = record.GetProperty("httpsLeafSha256").GetString()!;
            using var store = new X509Store(StoreName.My, StoreLocation.CurrentUser); store.Open(OpenFlags.ReadOnly);
            var found = store.Certificates.Find(X509FindType.FindByThumbprint, thumb, validOnly: false);
            if (found.Count != 1) throw new InvalidOperationException("Missing runtime HTTPS material");
            certificate = found[0];
            if (!certificate.HasPrivateKey || !certificate.MatchesHostname("localhost", allowWildcards: false, allowCommonName: false) ||
                !Convert.ToHexString(SHA256.HashData(certificate.RawData)).Equals(leaf, StringComparison.Ordinal) ||
                !CryptographicOperations.FixedTimeEquals(certificate.RawData, Read("https.cer")))
                throw new InvalidOperationException("Invalid runtime HTTPS binding");
            using var privateKey = certificate.GetRSAPrivateKey();
            if (privateKey is not RSACng cng || cng.Key.ExportPolicy != CngExportPolicies.None || cng.KeySize < 2048)
                throw new InvalidOperationException("Invalid runtime private key custody");
            var result = new LocalRuntimeKeyStore(keyBytes, identity.User.Value, material, certificate, leaf, snapshots);
            if (!result.IsCurrent()) throw new InvalidOperationException("Runtime custody changed");
            return result;
        }
        catch
        { if (keyBytes is not null) CryptographicOperations.ZeroMemory(keyBytes); certificate?.Dispose(); throw; }
    }

    public ValueTask<JwtReadinessMaterial?> ResolveJwt(CancellationToken token)
    { token.ThrowIfCancellationRequested(); return ValueTask.FromResult(IsCurrent() ? Jwt : null); }
    public ValueTask<LocalTlsTrust?> ResolveTrust(CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        return ValueTask.FromResult<LocalTlsTrust?>(IsCurrent()
            ? new(LocalRuntimeReferences.Https, LocalRuntimeReferences.HttpsTrust, HttpsLeafSha256) : null);
    }
    private bool IsCurrent()
    {
        try
        {
            return !disposed && SafeAcl(sid) && BoundFiles().Order(StringComparer.Ordinal).SequenceEqual(snapshots.Keys.Order(StringComparer.Ordinal)) &&
                snapshots.Keys.All(name => CryptographicOperations.FixedTimeEquals(snapshots[name], SHA256.HashData(Read(name))));
        }
        catch (Exception) { return false; }
    }
    private static string[] BoundFiles()
    {
        var xml = Directory.GetFiles(Path.Combine(LocalRuntimeReferences.RuntimeRoot, "csrf"), "*.xml");
        if (xml.Length is < 1 or > 16) throw new InvalidOperationException("Invalid CSRF custody set");
        return Files.Concat(xml.Select(path => Path.GetRelativePath(LocalRuntimeReferences.RuntimeRoot, path))).ToArray();
    }
    private static byte[] Read(string name)
    {
        var path = LocalReadinessFiles.Resolve(LocalRuntimeReferences.RuntimeRoot, name) ?? throw new InvalidOperationException("Missing custody source");
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        if (stream.Length is < 1 or > 65536) throw new InvalidOperationException("Invalid custody source");
        var value = new byte[(int)stream.Length]; stream.ReadExactly(value);
        if (stream.ReadByte() != -1) throw new InvalidOperationException("Custody source changed");
        return value;
    }
    private static bool SafeAcl(string sid)
    {
        if (!OperatingSystem.IsWindows()) return false;
        var allowed = new HashSet<string>([sid, "S-1-5-18", "S-1-5-32-544"], StringComparer.Ordinal);
        var directoryAcl = new DirectoryInfo(LocalRuntimeReferences.RuntimeRoot).GetAccessControl();
        if (!directoryAcl.AreAccessRulesProtected || directoryAcl.GetOwner(typeof(SecurityIdentifier))?.Value != sid ||
            !RulesMatch(directoryAcl, allowed)) return false;
        var csrf = new DirectoryInfo(Path.Combine(LocalRuntimeReferences.RuntimeRoot, "csrf")).GetAccessControl();
        if (!allowed.Contains(csrf.GetOwner(typeof(SecurityIdentifier))?.Value ?? "") || !RulesMatch(csrf, allowed)) return false;
        foreach (var name in BoundFiles())
        {
            var path = LocalReadinessFiles.Resolve(LocalRuntimeReferences.RuntimeRoot, name);
            if (path is null) return false;
            var fileAcl = new FileInfo(path).GetAccessControl();
            if (!allowed.Contains(fileAcl.GetOwner(typeof(SecurityIdentifier))?.Value ?? "") ||
                !RulesMatch(fileAcl, allowed)) return false;
        }
        return true;
    }
    private static bool RulesMatch(FileSystemSecurity acl, HashSet<string> allowed)
    {
        if (!OperatingSystem.IsWindows()) return false;
        var rules = acl.GetAccessRules(true, true, typeof(SecurityIdentifier));
        if (rules.Count != allowed.Count) return false;
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (FileSystemAccessRule rule in rules)
            if (rule.AccessControlType != AccessControlType.Allow || !allowed.Contains(rule.IdentityReference.Value) ||
                !seen.Add(rule.IdentityReference.Value) || rule.FileSystemRights != FileSystemRights.FullControl) return false;
        return true;
    }
    public void Dispose() { disposed = true; CryptographicOperations.ZeroMemory(bytes); HttpsCertificate.Dispose(); }
    public override string ToString() => nameof(LocalRuntimeKeyStore);
}
