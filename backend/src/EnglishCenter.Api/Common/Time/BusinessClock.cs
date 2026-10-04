namespace EnglishCenter.Api.Common.Time;

public sealed class BusinessClock
{
    private readonly TimeProvider _timeProvider;

    public BusinessClock(TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(timeProvider);
        _timeProvider = timeProvider;
    }

    public DateTimeOffset GetUtcNow()
    {
        DateTimeOffset raw = _timeProvider.GetUtcNow();
        DateTimeOffset utc = raw.ToUniversalTime();
        long truncatedTicks = utc.Ticks - (utc.Ticks % TimeSpan.TicksPerMillisecond);
        return new DateTimeOffset(truncatedTicks, TimeSpan.Zero);
    }

    public DateOnly GetToday()
    {
        DateTimeOffset sample = _timeProvider.GetUtcNow();
        return VietnamTime.ToDate(sample);
    }

    public bool IsBefore(DateTimeOffset cutoff)
    {
        return _timeProvider.GetUtcNow() < cutoff;
    }
}
