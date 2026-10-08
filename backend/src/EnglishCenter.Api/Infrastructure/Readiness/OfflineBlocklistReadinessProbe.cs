using System.Text.Json;
using EnglishCenter.Api.Common.Security;

namespace EnglishCenter.Api.Infrastructure.Readiness;

public sealed class OfflineBlocklistReadinessProbe : IRuntimeReadinessProbe
{
    private readonly string manifestReference;
    private readonly string directory;
    private readonly TimeProvider clock;
    private readonly bool localDirectory;

    public OfflineBlocklistReadinessProbe(string manifestReference, string directory, TimeProvider clock)
    {
        this.manifestReference = manifestReference;
        this.directory = directory;
        this.clock = clock ?? throw new ArgumentNullException(nameof(clock));
        localDirectory = IsLocalDirectory(directory);
    }
    public ReadinessDependency Dependency => ReadinessDependency.OfflineBlocklist;

    public async Task<ReadinessProbeStatus> CheckAsync(RuntimeProfile profile, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!localDirectory || string.IsNullOrWhiteSpace(manifestReference) ||
            !string.Equals(profile.BlocklistManifestReference, manifestReference, StringComparison.Ordinal))
            return ReadinessProbeStatus.NotReady;
        try
        {
            using var manifest = await ReadManifestAsync(cancellationToken);
            var today = DateOnly.FromDateTime(clock.GetUtcNow().ToOffset(TimeSpan.FromHours(7)).DateTime);
            if (!BlocklistManifestPolicy.IsCurrent(manifest.RootElement, today)) return ReadinessProbeStatus.NotReady;
            var corpus = await OfflinePasswordBlocklist.LoadAsync(directory, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            return corpus.Count == OfflinePasswordBlocklist.MergedRecordCount
                ? ReadinessProbeStatus.Pass : ReadinessProbeStatus.NotReady;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
        catch (Exception)
        { cancellationToken.ThrowIfCancellationRequested(); return ReadinessProbeStatus.NotReady; }
    }

    private async Task<JsonDocument> ReadManifestAsync(CancellationToken cancellationToken)
    {
        await using var stream = new FileStream(Path.Combine(directory, "manifest.json"), FileMode.Open,
            FileAccess.Read, FileShare.Read, 4096, FileOptions.Asynchronous | FileOptions.SequentialScan);
        if (stream.Length is < 1 or > 4096) throw new InvalidOperationException("Invalid blocklist manifest");
        var bytes = new byte[(int)stream.Length];
        await stream.ReadExactlyAsync(bytes, cancellationToken);
        if (await stream.ReadAsync(new byte[1], cancellationToken) != 0)
            throw new InvalidOperationException("Invalid blocklist manifest");
        return JsonDocument.Parse(bytes);
    }

    private static bool IsLocalDirectory(string? value)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(value) || !Path.IsPathFullyQualified(value) ||
                value.StartsWith("\\\\", StringComparison.Ordinal) || value.StartsWith("//", StringComparison.Ordinal)) return false;
            var root = Path.GetPathRoot(value);
            return root is not null && new DriveInfo(root).DriveType is DriveType.Fixed or DriveType.Removable or DriveType.Ram;
        }
        catch (Exception) { return false; }
    }
}
