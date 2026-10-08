using System.Reflection;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;

namespace EnglishCenter.Api.Infrastructure.Readiness;

public sealed class RuntimeManifestReadinessProbe : IRuntimeReadinessProbe
{
    private readonly string reference;
    private readonly string root;
    private readonly string connectionReference;
    private readonly RuntimePatchManifest manifest;
    private readonly TimeProvider clock;
    private readonly Func<CancellationToken, ValueTask<Version?>> sqlVersion;
    public RuntimeManifestReadinessProbe(string reference, string root, RuntimePatchManifest manifest,
        TimeProvider clock, string connectionReference, Func<CancellationToken, ValueTask<Version?>> sqlVersion)
    {
        this.reference = reference; this.root = root; this.manifest = manifest; this.connectionReference = connectionReference;
        this.clock = clock ?? throw new ArgumentNullException(nameof(clock));
        this.sqlVersion = sqlVersion ?? throw new ArgumentNullException(nameof(sqlVersion));
    }
    public ReadinessDependency Dependency => ReadinessDependency.RuntimeManifest;
    public async Task<ReadinessProbeStatus> CheckAsync(RuntimeProfile profile, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (RuntimeProfileValidator.Validate(profile) != ProfileIssue.None || string.IsNullOrWhiteSpace(reference) ||
            profile.Runtime!.PatchManifestReference != reference || string.IsNullOrWhiteSpace(connectionReference) ||
            profile.Database!.ConnectionSecretReference != connectionReference) return ReadinessProbeStatus.NotReady;
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        deadline.CancelAfter(profile.Execution!.ReadinessMs);
        try
        {
            var today = DateOnly.FromDateTime(clock.GetUtcNow().ToOffset(TimeSpan.FromHours(7)).DateTime);
            if (!RuntimePatchPolicy.IsCurrent(manifest, today) || Environment.Version != manifest.DotNet ||
                InformationalVersion(typeof(WebApplication).Assembly) != manifest.AspNet ||
                InformationalVersion(typeof(DbContext).Assembly) != manifest.Ef) return ReadinessProbeStatus.NotReady;
            foreach (var pin in manifest.Artifacts)
            {
                var path = LocalReadinessFiles.Resolve(root, pin.RelativePath);
                if (path is null) return ReadinessProbeStatus.NotReady;
                await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read,
                    81920, FileOptions.Asynchronous | FileOptions.SequentialScan);
                if (stream.Length != pin.Length) return ReadinessProbeStatus.NotReady;
                var digest = await SHA256.HashDataAsync(stream, deadline.Token);
                if (stream.Position != pin.Length || !Convert.ToHexString(digest).Equals(pin.Sha256,
                    StringComparison.OrdinalIgnoreCase)) return ReadinessProbeStatus.NotReady;
            }
            var pending = sqlVersion(deadline.Token).AsTask();
            ProbeWait.Observe(pending);
            var actual = await pending.WaitAsync(deadline.Token);
            deadline.Token.ThrowIfCancellationRequested();
            return actual == manifest.Sql ? ReadinessProbeStatus.Pass : ReadinessProbeStatus.NotReady;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
        catch (Exception)
        { cancellationToken.ThrowIfCancellationRequested(); return ReadinessProbeStatus.NotReady; }
    }
    private static Version? InformationalVersion(Assembly assembly)
    {
        var text = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion.Split('+')[0];
        return Version.TryParse(text, out var version) ? version : null;
    }
}

internal static class ProbeWait
{
    internal static void Observe(Task task) => _ = task.ContinueWith(static late => { _ = late.Exception; },
        CancellationToken.None, TaskContinuationOptions.OnlyOnFaulted |
        TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
}

internal static class LocalReadinessFiles
{
    internal static string? Resolve(string root, string relative)
    {
        if (!Path.IsPathFullyQualified(root) || root.StartsWith(@"\\", StringComparison.Ordinal) ||
            root.StartsWith("//", StringComparison.Ordinal) || Path.IsPathRooted(relative)) return null;
        var drive = Path.GetPathRoot(root);
        if (drive is null || new DriveInfo(drive).DriveType is not (DriveType.Fixed or DriveType.Removable or DriveType.Ram)) return null;
        var absoluteRoot = Path.TrimEndingDirectorySeparator(Path.GetFullPath(root));
        var full = Path.GetFullPath(Path.Combine(absoluteRoot, relative));
        var relation = Path.GetRelativePath(absoluteRoot, full);
        if (relation == ".." || relation.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal) ||
            Path.IsPathRooted(relation)) return null;
        for (string? part = full; part is not null; part = Path.GetDirectoryName(part))
        {
            if ((File.GetAttributes(part) & FileAttributes.ReparsePoint) != 0) return null;

        }
        return full;
    }
}
