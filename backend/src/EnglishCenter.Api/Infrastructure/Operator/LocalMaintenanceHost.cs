using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using EnglishCenter.Api.Infrastructure.Maintenance;
using EnglishCenter.Api.Infrastructure.Readiness;

namespace EnglishCenter.Api.Infrastructure.Operator;

public static class LocalMaintenanceHost
{
    public static async Task<int> RunAsync(CancellationToken token)
    {
        if (!OperatingSystem.IsWindows()) return 1;
        try
        {
            using var identity = WindowsIdentity.GetCurrent();
            if (identity.Name != Environment.MachineName + @"\EcM0Maint" ||
                new WindowsPrincipal(identity).IsInRole(WindowsBuiltInRole.Administrator)) return 1;
            var profile = await LocalRuntimeReferences.LoadProfileAsync(LocalRuntimeReferences.MaintenanceRoot, token);
            if (profile is null) return 1;
            using var custody = MaintenanceProofCustody.Open(privateKey: true);
            if (custody.Descriptor.MaintenanceSid != identity.User?.Value) return 1;
            var builder = Host.CreateApplicationBuilder(); builder.Logging.ClearProviders();
            builder.Services.AddSingleton(new SqlMaintenanceService(profile.Maintenance!, profile.Execution!, 3600,
                TimeProvider.System, _ => ValueTask.FromResult<string?>(LocalRuntimeReferences.SqlConnection)));
            builder.Services.AddHostedService(services => services.GetRequiredService<SqlMaintenanceService>());
            builder.Services.AddSingleton(custody);
            builder.Services.AddHostedService<MaintenanceProofService>();
            using var host = builder.Build();
            await host.RunAsync(token);
            return 0;
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested) { return 0; }
        catch (Exception) { return 1; }
    }
}

internal sealed record MaintenanceProofDescriptor(string OwnerId, string MaintenanceSid, string RuntimeSid,
    string Phase, string Thumbprint, string LeafSha256, string PipeName);

// Public files are maintenance-owned; the runtime receives only the pinned verifier.
internal sealed class MaintenanceProofCustody : IDisposable
{
    private static readonly string[] Files = ["prepared.json", "proof-manifest.json", "proof.cer"];
    private readonly byte[][] snapshots;
    private readonly X509Certificate2 certificate;
    public MaintenanceProofDescriptor Descriptor { get; }
    public RSA Key { get; }
    private MaintenanceProofCustody(MaintenanceProofDescriptor descriptor, X509Certificate2 certificate, RSA key, byte[][] snapshots)
    { Descriptor = descriptor; this.certificate = certificate; Key = key; this.snapshots = snapshots; }
    public static MaintenanceProofCustody Open(bool privateKey)
    {
        if (!OperatingSystem.IsWindows()) throw new PlatformNotSupportedException();
        var raw = Files.Select(Read).ToArray();
        using var document = JsonDocument.Parse(new UTF8Encoding(false, true).GetString(raw[1]), new JsonDocumentOptions { MaxDepth = 8 });
        if (!Unique(document.RootElement)) throw new InvalidOperationException("Invalid proof descriptor");
        var descriptor = document.RootElement.Deserialize<MaintenanceProofDescriptor>(new JsonSerializerOptions
        { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow })
            ?? throw new InvalidOperationException("Invalid proof descriptor");
        using var marker = JsonDocument.Parse(new UTF8Encoding(false, true).GetString(raw[0]));
        var prepared = marker.RootElement;
        if (!Unique(prepared) || prepared.GetProperty("ownerId").GetString() != descriptor.OwnerId ||
            prepared.GetProperty("maintenanceSid").GetString() != descriptor.MaintenanceSid ||
            prepared.GetProperty("runtimeSid").GetString() != descriptor.RuntimeSid ||
            prepared.GetProperty("phase").GetString() != "maintenance_root_prepared")
            throw new InvalidOperationException("Invalid proof ownership");
        if (!Guid.TryParseExact(descriptor.OwnerId, "N", out _) || descriptor.Phase != "proof_key_provisioned" ||
            descriptor.PipeName != "EnglishCenter.M0.Maint." + descriptor.OwnerId || !SafeAcl(descriptor))
            throw new InvalidOperationException("Invalid proof custody");
        var certificate = X509CertificateLoader.LoadCertificate(raw[2]);
        RSA? key = null;
        try
        {
            if (certificate.Subject != "CN=EnglishCenter M0 Maintenance Proof" || certificate.HasPrivateKey || certificate.Thumbprint != descriptor.Thumbprint ||
                Convert.ToHexString(SHA256.HashData(raw[2])) != descriptor.LeafSha256 ||
                certificate.NotBefore.ToUniversalTime() > DateTime.UtcNow || certificate.NotAfter.ToUniversalTime() <= DateTime.UtcNow)
                throw new InvalidOperationException("Invalid proof certificate");
            if (privateKey)
            {
                using var identity = WindowsIdentity.GetCurrent();
                if (identity.User?.Value != descriptor.MaintenanceSid) throw new InvalidOperationException("Invalid proof signer identity");
                using var store = new X509Store(StoreName.My, StoreLocation.CurrentUser); store.Open(OpenFlags.ReadOnly);
                var found = store.Certificates.Find(X509FindType.FindByThumbprint, descriptor.Thumbprint, validOnly: false);
                if (found.Count != 1 || !CryptographicOperations.FixedTimeEquals(found[0].RawData, raw[2]))
                    throw new InvalidOperationException("Missing proof signer");
                certificate.Dispose(); certificate = found[0]; key = certificate.GetRSAPrivateKey();
                if (key is not RSACng cng || cng.Key.ExportPolicy != CngExportPolicies.None)
                    throw new InvalidOperationException("Invalid proof signer custody");
            }
            else key = certificate.GetRSAPublicKey();
            if (key is null || key.KeySize != 2048) throw new InvalidOperationException("Invalid proof key");
            return new(descriptor, certificate, key, raw.Select(SHA256.HashData).ToArray());
        }
        catch { key?.Dispose(); certificate.Dispose(); throw; }
    }
    public bool IsCurrent()
    {
        try { return SafeAcl(Descriptor) && Files.Select(Read).Select(SHA256.HashData).Zip(snapshots)
            .All(pair => CryptographicOperations.FixedTimeEquals(pair.First, pair.Second)) &&
            certificate.NotBefore.ToUniversalTime() <= DateTime.UtcNow && certificate.NotAfter.ToUniversalTime() > DateTime.UtcNow; }
        catch (Exception) { return false; }
    }
    private static byte[] Read(string name)
    {
        var path = LocalReadinessFiles.Resolve(LocalRuntimeReferences.MaintenanceRoot, name) ?? throw new InvalidOperationException("Missing proof source");
        using var file = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        if (file.Length is < 1 or > 32768) throw new InvalidOperationException("Invalid proof source");
        var value = new byte[(int)file.Length]; file.ReadExactly(value);
        if (file.ReadByte() != -1) throw new InvalidOperationException("Proof source changed");
        return value;
    }
    private static bool SafeAcl(MaintenanceProofDescriptor descriptor)
    {
        if (!OperatingSystem.IsWindows()) return false;
        var acl = new DirectoryInfo(LocalRuntimeReferences.MaintenanceRoot).GetAccessControl();
        if (!acl.AreAccessRulesProtected || acl.GetOwner(typeof(SecurityIdentifier))?.Value != descriptor.MaintenanceSid || !Rules(acl, descriptor)) return false;
        foreach (var name in Files)
        {
            var path = LocalReadinessFiles.Resolve(LocalRuntimeReferences.MaintenanceRoot, name);
            if (path is null) return false;
            var fileAcl = new FileInfo(path).GetAccessControl();
            var owner = fileAcl.GetOwner(typeof(SecurityIdentifier))?.Value;
            if (owner != descriptor.MaintenanceSid && owner != "S-1-5-18" && owner != "S-1-5-32-544") return false;
            if (!Rules(fileAcl, descriptor)) return false;
        }
        return true;
    }
    private static bool Rules(FileSystemSecurity acl, MaintenanceProofDescriptor descriptor)
    {
        if (!OperatingSystem.IsWindows()) return false;
        var expected = new Dictionary<string, FileSystemRights>(StringComparer.Ordinal)
        {
            [descriptor.MaintenanceSid] = FileSystemRights.FullControl, ["S-1-5-18"] = FileSystemRights.FullControl,
            ["S-1-5-32-544"] = FileSystemRights.FullControl,
            [descriptor.RuntimeSid] = FileSystemRights.ReadAndExecute | FileSystemRights.Synchronize
        };
        var rules = acl.GetAccessRules(true, true, typeof(SecurityIdentifier));
        if (rules.Count != 4) return false;
        foreach (FileSystemAccessRule rule in rules)
            if (rule.AccessControlType != AccessControlType.Allow || !expected.Remove(rule.IdentityReference.Value, out var rights) || rule.FileSystemRights != rights) return false;
        return expected.Count == 0;
    }
    private static bool Unique(JsonElement value)
    {
        if (value.ValueKind != JsonValueKind.Object) return false;
        var names = new HashSet<string>(StringComparer.Ordinal);
        foreach (var item in value.EnumerateObject()) if (!names.Add(item.Name) || item.Value.ValueKind != JsonValueKind.String) return false;
        return true;
    }
    public void Dispose() { Key.Dispose(); certificate.Dispose(); }
}
