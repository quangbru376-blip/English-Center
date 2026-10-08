namespace EnglishCenter.Api.Infrastructure.Readiness;

public sealed class TimeZoneReadinessProbe : IRuntimeReadinessProbe
{
    private const string VietnamId = "Asia/Ho_Chi_Minh";
    private const string WindowsId = "SE Asia Standard Time";
    private static readonly TimeSpan VietnamOffset = TimeSpan.FromHours(7);
    private readonly TimeProvider clock;
    private readonly Func<string, TimeZoneInfo> resolve;

    public TimeZoneReadinessProbe(TimeProvider clock, Func<string, TimeZoneInfo>? resolve = null)
    {
        this.clock = clock ?? throw new ArgumentNullException(nameof(clock));
        this.resolve = resolve ?? TimeZoneInfo.FindSystemTimeZoneById;
    }
    public ReadinessDependency Dependency => ReadinessDependency.TimeZone;

    public Task<ReadinessProbeStatus> CheckAsync(RuntimeProfile profile, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (profile.BusinessTimeZone != VietnamId) return Task.FromResult(ReadinessProbeStatus.NotReady);
        return Task.Run(() => Inspect(cancellationToken), cancellationToken);
    }

    private ReadinessProbeStatus Inspect(CancellationToken cancellationToken)
    {
        try
        {
            TimeZoneInfo zone;
            try { zone = resolve(VietnamId); }
            catch (TimeZoneNotFoundException) when (OperatingSystem.IsWindows()) { zone = resolve(WindowsId); }
            cancellationToken.ThrowIfCancellationRequested();
            var utcNow = clock.GetUtcNow();
            var localToday = utcNow.ToOffset(VietnamOffset).Date;
            if (zone.Id is not (VietnamId or WindowsId) || zone.BaseUtcOffset != VietnamOffset ||
                zone.GetUtcOffset(utcNow) != VietnamOffset || zone.GetAdjustmentRules().Any(rule =>
                    rule.DateEnd.Date >= localToday &&
                    (rule.DaylightDelta != TimeSpan.Zero || rule.BaseUtcOffsetDelta != TimeSpan.Zero)))
                return ReadinessProbeStatus.NotReady;
            cancellationToken.ThrowIfCancellationRequested();
            return ReadinessProbeStatus.Pass;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
        catch (Exception)
        { cancellationToken.ThrowIfCancellationRequested(); return ReadinessProbeStatus.NotReady; }
    }
}
