using System.Globalization;

namespace EnglishCenter.Api.Common.Time;

public static class VietnamTime
{
    private static readonly TimeZoneInfo VietnamZone = ResolveVietnamZone();

    public static TimeZoneInfo TimeZone => VietnamZone;

    public static DateOnly ToDate(DateTimeOffset instant)
    {
        long localTicks = instant.UtcTicks + VietnamZone.GetUtcOffset(instant).Ticks;
        if (localTicks < DateTime.MinValue.Ticks || localTicks > DateTime.MaxValue.Ticks)
        {
            throw new ArgumentOutOfRangeException(nameof(instant), "The Vietnam date is outside representable bounds.");
        }

        DateTime vietnamTime = new(localTicks, DateTimeKind.Unspecified);
        return DateOnly.FromDateTime(vietnamTime);
    }

    public static DateTimeOffset ToUtc(DateOnly date, TimeOnly time)
    {
        if (time.Ticks % TimeSpan.TicksPerMinute != 0)
        {
            throw new ArgumentException("Time must have minute precision only (seconds and subseconds must be zero).", nameof(time));
        }

        DateTime localDateTime = date.ToDateTime(time, DateTimeKind.Unspecified);
        if (VietnamZone.IsInvalidTime(localDateTime) || VietnamZone.IsAmbiguousTime(localDateTime))
        {
            throw new ArgumentException("The local time does not identify one Vietnam instant.", nameof(time));
        }

        TimeSpan offset = VietnamZone.GetUtcOffset(localDateTime);
        long utcTicks = localDateTime.Ticks - offset.Ticks;

        if (utcTicks < DateTime.MinValue.Ticks || utcTicks > DateTime.MaxValue.Ticks)
        {
            throw new ArgumentOutOfRangeException(nameof(date), "The resulting UTC instant is outside DateTimeOffset representable bounds.");
        }

        return new DateTimeOffset(utcTicks, TimeSpan.Zero);
    }

    public static (DateTimeOffset StartInclusive, DateTimeOffset EndExclusive) InclusiveDateRange(DateOnly from, DateOnly to)
    {
        if (from > to)
        {
            throw new ArgumentException("'from' date cannot be after 'to' date.", nameof(from));
        }

        if (to == DateOnly.MaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(to), "The 'to' date cannot be DateOnly.MaxValue because the exclusive end boundary would overflow.");
        }

        DateTimeOffset start = ToUtc(from, TimeOnly.MinValue);
        DateTimeOffset end = ToUtc(to.AddDays(1), TimeOnly.MinValue);

        return (start, end);
    }

    public static string FormatInstant(DateTimeOffset instant)
    {
        DateTimeOffset utc = instant.ToUniversalTime();
        long truncatedTicks = utc.Ticks - (utc.Ticks % TimeSpan.TicksPerMillisecond);
        var truncatedUtc = new DateTimeOffset(truncatedTicks, TimeSpan.Zero);
        return truncatedUtc.ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'", CultureInfo.InvariantCulture);
    }

    private static TimeZoneInfo ResolveVietnamZone()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh");
        }
        catch (TimeZoneNotFoundException)
        {
            if (TimeZoneInfo.TryConvertIanaIdToWindowsId("Asia/Ho_Chi_Minh", out string? windowsId))
            {
                return TimeZoneInfo.FindSystemTimeZoneById(windowsId);
            }

            return TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        }
    }
}
