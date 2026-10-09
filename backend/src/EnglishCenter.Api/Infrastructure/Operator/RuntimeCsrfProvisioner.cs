using System.Security.Principal;
using System.Text.Json;
using EnglishCenter.Api.Infrastructure.Readiness;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.KeyManagement;

namespace EnglishCenter.Api.Infrastructure.Operator;

public static class RuntimeCsrfProvisioner
{
    private const string Root = @"C:\ProgramData\EnglishCenter\M0Runtime";
    public const string ApplicationName = "EnglishCenter.M0.Local";

    public static int Run(CancellationToken token)
    {
        if (!OperatingSystem.IsWindows()) return 1;
        using var identity = WindowsIdentity.GetCurrent();
        if (identity.Name != Environment.MachineName + @"\EcM0Runtime") return 1;
        try
        {
            token.ThrowIfCancellationRequested();
            var preparedPath = LocalReadinessFiles.Resolve(Root, "prepared.json");
            var manifestPath = LocalReadinessFiles.Resolve(Root, "key-manifest.json");
            if (preparedPath is null || manifestPath is null) return 1;
            using var prepared = ReadDescriptor(preparedPath);
            using var manifest = ReadDescriptor(manifestPath);
            var marker = prepared.RootElement;
            var keys = manifest.RootElement;
            if (marker.GetProperty("runtimeSid").GetString() != identity.User?.Value ||
                marker.GetProperty("phase").GetString() != "runtime_root_prepared" ||
                keys.GetProperty("runtimeSid").GetString() != identity.User?.Value ||
                keys.GetProperty("ownerId").GetString() != marker.GetProperty("ownerId").GetString() ||
                keys.GetProperty("phase").GetString() != "jwt_https_provisioned") return 1;
            var directory = Path.Combine(Root, "csrf");
            if (Directory.Exists(directory) || File.Exists(directory)) return 1;
            Directory.CreateDirectory(directory);
            var services = new ServiceCollection();
            services.AddDataProtection().SetApplicationName(ApplicationName)
                .PersistKeysToFileSystem(new DirectoryInfo(directory))
                .ProtectKeysWithDpapi(protectToLocalMachine: false).DisableAutomaticKeyGeneration();
            using var provider = services.BuildServiceProvider();
            var now = DateTimeOffset.UtcNow;
            provider.GetRequiredService<IKeyManager>().CreateNewKey(now.AddMinutes(-1), now.AddDays(90));
            token.ThrowIfCancellationRequested();
            return Directory.GetFiles(directory, "key-*.xml", SearchOption.TopDirectoryOnly).Length == 1 ? 0 : 1;
        }
        catch (Exception) { return 1; }
    }

    private static JsonDocument ReadDescriptor(string path)
    {
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        if (stream.Length is < 1 or > 65536) throw new InvalidOperationException("Invalid custody descriptor");
        return JsonDocument.Parse(stream, new JsonDocumentOptions { MaxDepth = 8 });
    }
}
