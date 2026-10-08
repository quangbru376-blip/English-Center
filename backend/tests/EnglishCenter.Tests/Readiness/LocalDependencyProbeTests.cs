using System.Text.Json;
using System.Text.Json.Nodes;
using EnglishCenter.Api.Common.Security;
using EnglishCenter.Api.Infrastructure.Readiness;
using Xunit;

namespace EnglishCenter.Tests.Readiness;

public sealed class LocalDependencyProbeTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 8, 0, 0, 0, TimeSpan.Zero);
    private sealed class Clock(DateTimeOffset now, Action? read = null) : TimeProvider
    { public override DateTimeOffset GetUtcNow() { read?.Invoke(); return now; } }
    private static string CorpusDirectory => Path.Combine(AppContext.BaseDirectory, "Resources", "PasswordBlocklist");

    [Fact]
    public async Task ActualBclVietnamTimezonePassesWithoutHostFallback()
    {
        var probe = new TimeZoneReadinessProbe(new Clock(Now));
        Assert.Equal(ReadinessDependency.TimeZone, probe.Dependency);
        Assert.Equal(ReadinessProbeStatus.Pass, await probe.CheckAsync(ProfileFixture.Valid, TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData("different-id")] [InlineData("wrong-offset")] [InlineData("current-dst")]
    [InlineData("future-dst")] [InlineData("future-base")]
    public async Task WrongIdentityOffsetOrCurrentFutureRulesFailClosed(string variant)
    {
        var today = new DateTime(2026, 10, 8);
        var start = TimeZoneInfo.TransitionTime.CreateFixedDateRule(new DateTime(1, 1, 1), 1, 1);
        var end = TimeZoneInfo.TransitionTime.CreateFixedDateRule(new DateTime(1, 1, 1), 12, 31);
        var rules = variant.Contains("dst", StringComparison.Ordinal) || variant == "future-base"
            ? new[] { TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule(
                variant.StartsWith("future", StringComparison.Ordinal) ? new DateTime(2027, 1, 1) : today,
                new DateTime(2027, 12, 31), variant == "future-base" ? TimeSpan.Zero : TimeSpan.FromHours(1),
                start, end, variant == "future-base" ? TimeSpan.FromHours(1) : TimeSpan.Zero) }
            : [];
        var zone = TimeZoneInfo.CreateCustomTimeZone(variant == "different-id" ? "Other UTC7" : "Asia/Ho_Chi_Minh",
            TimeSpan.FromHours(variant == "wrong-offset" ? 8 : 7), "Synthetic zone", "Synthetic standard", "Synthetic summer", rules);
        var probe = new TimeZoneReadinessProbe(new Clock(Now), _ => zone);
        Assert.Equal(ReadinessProbeStatus.NotReady, await probe.CheckAsync(ProfileFixture.Valid, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task HistoricalChangesDoNotRejectModernVietnamZone()
    {
        var start = TimeZoneInfo.TransitionTime.CreateFixedDateRule(new DateTime(1, 1, 1), 1, 1);
        var end = TimeZoneInfo.TransitionTime.CreateFixedDateRule(new DateTime(1, 1, 1), 12, 31);
        var history = TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule(new DateTime(1940, 1, 1),
            new DateTime(1945, 12, 31), TimeSpan.FromHours(1), start, end);
        var zone = TimeZoneInfo.CreateCustomTimeZone("Asia/Ho_Chi_Minh", TimeSpan.FromHours(7),
            "Synthetic zone", "Synthetic standard", "Synthetic summer", [history]);
        Assert.Equal(ReadinessProbeStatus.Pass, await new TimeZoneReadinessProbe(new Clock(Now), _ => zone)
            .CheckAsync(ProfileFixture.Valid, TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData("wrong-profile")] [InlineData("missing-mapping")] [InlineData("invalid-mapping")]
    public async Task MissingOrInvalidTimezoneNeverUsesLocalZone(string variant)
    {
        var ids = new List<string>();
        var probe = new TimeZoneReadinessProbe(new Clock(Now), id =>
        { ids.Add(id); if (variant == "invalid-mapping") throw new InvalidTimeZoneException("private-path");
            throw new TimeZoneNotFoundException("private-path"); });
        var profile = variant == "wrong-profile" ? ProfileFixture.Valid with { BusinessTimeZone = "UTC" } : ProfileFixture.Valid;
        Assert.Equal(ReadinessProbeStatus.NotReady, await probe.CheckAsync(profile, TestContext.Current.CancellationToken));
        Assert.All(ids, id => Assert.True(id is "Asia/Ho_Chi_Minh" or "SE Asia Standard Time"));
        if (variant == "wrong-profile") Assert.Empty(ids);
    }

    [Fact]
    public async Task ActualShippedManifestLicenseAndCorpusPassAsync()
    {
        var probe = new OfflineBlocklistReadinessProbe("synthetic-blocklist", CorpusDirectory, new Clock(Now));
        Assert.Equal(ReadinessDependency.OfflineBlocklist, probe.Dependency);
        Assert.Equal(ReadinessProbeStatus.Pass, await probe.CheckAsync(ProfileFixture.Valid, TestContext.Current.CancellationToken));
        var corpus = await OfflinePasswordBlocklist.LoadAsync(CorpusDirectory, TestContext.Current.CancellationToken);
        Assert.Equal(15310, corpus.Count);
        Assert.Equal(OfflinePasswordBlocklist.Load(CorpusDirectory).Count, corpus.Count);
    }

    [Theory]
    [InlineData("relative-path")] [InlineData("\\\\invalid.invalid\\share")]
    public async Task NonlocalOrRelativeDirectoryIsRejectedBeforeIo(string directory)
        => Assert.Equal(ReadinessProbeStatus.NotReady, await new OfflineBlocklistReadinessProbe("synthetic-blocklist", directory,
            new Clock(Now)).CheckAsync(ProfileFixture.Valid, TestContext.Current.CancellationToken));

    [Fact]
    public async Task ReferenceMismatchRejectsWithoutFallbackDirectory()
        => Assert.Equal(ReadinessProbeStatus.NotReady, await new OfflineBlocklistReadinessProbe("different-reference", CorpusDirectory,
            new Clock(Now)).CheckAsync(ProfileFixture.Valid, TestContext.Current.CancellationToken));

    [Theory]
    [InlineData("2026-10-03", false)] [InlineData("2026-10-04", true)]
    [InlineData("2027-01-03", true)] [InlineData("2027-01-04", false)]
    public void ApprovedManifestReviewWindowHasNoStaleGracePeriod(string date, bool expected)
    {
        using var manifest = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(CorpusDirectory, "manifest.json")));
        Assert.Equal(expected, BlocklistManifestPolicy.IsCurrent(manifest.RootElement, DateOnly.ParseExact(date, "yyyy-MM-dd")));
    }

    [Theory]
    [InlineData("revision")] [InlineData("license")] [InlineData("source")] [InlineData("reviewDate")]
    [InlineData("nextReviewDate")] [InlineData("comparison")] [InlineData("mergedCount")]
    [InlineData("missing")] [InlineData("extra")] [InlineData("duplicate")]
    [InlineData("file-hash")] [InlineData("file-count")] [InlineData("file-duplicate")]
    [InlineData("file-extra")] [InlineData("file-path")]
    public void TamperedOrAmbiguousManifestMetadataRejects(string change)
    {
        var original = File.ReadAllText(Path.Combine(CorpusDirectory, "manifest.json"));
        var value = JsonNode.Parse(original)!.AsObject();
        switch (change)
        {
            case "missing": value.Remove("source"); break;
            case "extra": value["extra"] = "unapproved"; break;
            case "file-hash": value["files"]![0]!["sha256"] = "invalid"; break;
            case "file-count": value["files"]![0]!["records"] = "10001"; break;
            case "file-path": value["files"]![0]!["path"] = "../corpus"; break;
            case "file-extra": value["files"]![0]!["extra"] = true; break;
            case "file-duplicate": value["files"]![1] = value["files"]![0]!.DeepClone(); break;
            case "duplicate": break;
            default: value[change] = "unapproved"; break;
        }
        var json = change == "duplicate" ? "{\"source\":\"SecLists\"," + original.TrimStart()[1..] : value.ToJsonString();
        using var manifest = JsonDocument.Parse(json);
        Assert.False(BlocklistManifestPolicy.IsCurrent(manifest.RootElement, new DateOnly(2026, 10, 8)));
    }

    [Theory]
    [InlineData("null")] [InlineData("[]")] [InlineData("0")] [InlineData("{}")]
    public void NonManifestJsonShapesRejectWithoutException(string json)
    {
        using var manifest = JsonDocument.Parse(json);
        Assert.False(BlocklistManifestPolicy.IsCurrent(manifest.RootElement, new DateOnly(2026, 10, 8)));
    }

    [Theory]
    [InlineData("10k-most-common.txt")] [InlineData("probable-v2_top-12000.txt")]
    [InlineData("LICENSE")] [InlineData("manifest.json")] [InlineData("manifest-oversized")]
    public async Task FreshProbeDetectsDeletedTamperedOrOversizedFiles(string file)
    {
        var directory = CreateCopy();
        try
        {
            var probe = new OfflineBlocklistReadinessProbe("synthetic-blocklist", directory, new Clock(Now));
            Assert.Equal(ReadinessProbeStatus.Pass, await probe.CheckAsync(ProfileFixture.Valid, TestContext.Current.CancellationToken));
            if (file == "manifest-oversized") File.WriteAllBytes(Path.Combine(directory, "manifest.json"), new byte[4097]);
            else File.Delete(Path.Combine(directory, file));
            Assert.Equal(ReadinessProbeStatus.NotReady, await probe.CheckAsync(ProfileFixture.Valid, TestContext.Current.CancellationToken));
            if (file is not ("manifest.json" or "manifest-oversized"))
            {
                File.Copy(Path.Combine(CorpusDirectory, file), Path.Combine(directory, file));
                var bytes = File.ReadAllBytes(Path.Combine(directory, file));
                bytes[0] ^= 0xff;
                File.WriteAllBytes(Path.Combine(directory, file), bytes);
                var error = await Assert.ThrowsAsync<InvalidOperationException>(() => OfflinePasswordBlocklist.LoadAsync(directory,
                    TestContext.Current.CancellationToken));
                Assert.Equal("Password blocklist corpus is missing, invalid, or tampered.", error.Message);
                Assert.Null(error.InnerException);
                Assert.Equal(ReadinessProbeStatus.NotReady, await probe.CheckAsync(ProfileFixture.Valid, TestContext.Current.CancellationToken));
            }
        }
        finally { DeleteCopy(directory); }
    }

    [Fact]
    public async Task OverdueManifestStopsBeforeSuccessfulReadiness()
    {
        var probe = new OfflineBlocklistReadinessProbe("synthetic-blocklist", CorpusDirectory,
            new Clock(new DateTimeOffset(2027, 1, 4, 0, 0, 0, TimeSpan.Zero)));
        Assert.Equal(ReadinessProbeStatus.NotReady, await probe.CheckAsync(ProfileFixture.Valid, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task CancellationPropagatesBeforeAnyDependencyRead()
    {
        using var cancelled = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        cancelled.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => new TimeZoneReadinessProbe(new Clock(Now))
            .CheckAsync(ProfileFixture.Valid, cancelled.Token));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => new OfflineBlocklistReadinessProbe("synthetic-blocklist",
            CorpusDirectory, new Clock(Now)).CheckAsync(ProfileFixture.Valid, cancelled.Token));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => OfflinePasswordBlocklist.LoadAsync(CorpusDirectory, cancelled.Token));
    }

    [Fact]
    public async Task CancellationBetweenManifestAndCorpusPropagates()
    {
        using var cancelled = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        var probe = new OfflineBlocklistReadinessProbe("synthetic-blocklist", CorpusDirectory,
            new Clock(Now, cancelled.Cancel));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => probe.CheckAsync(ProfileFixture.Valid, cancelled.Token));
    }

    private static string CreateCopy()
    {
        var root = Path.Combine(Path.GetTempPath(), "ec-v01-h");
        var directory = Path.Combine(root, Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        foreach (var name in new[] { "manifest.json", "LICENSE", "10k-most-common.txt", "probable-v2_top-12000.txt" })
            File.Copy(Path.Combine(CorpusDirectory, name), Path.Combine(directory, name));
        return directory;
    }
    private static void DeleteCopy(string directory)
    {
        var root = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "ec-v01-h")) + Path.DirectorySeparatorChar;
        var absolute = Path.GetFullPath(directory);
        if (!absolute.StartsWith(root, StringComparison.OrdinalIgnoreCase) ||
            !Guid.TryParseExact(Path.GetFileName(absolute), "N", out _)) throw new InvalidOperationException("Invalid fixture cleanup scope");
        Directory.Delete(absolute, true);
    }
}
