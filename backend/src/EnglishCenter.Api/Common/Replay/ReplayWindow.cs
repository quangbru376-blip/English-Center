namespace EnglishCenter.Api.Common.Replay;

public static class ReplayWindow
{
    public static long? ExpiresAt(long completedTicks)
    {
        if (completedTicks < DateTime.MinValue.Ticks || completedTicks > DateTime.MaxValue.Ticks)
        {
            return null;
        }

        if (DateTime.MaxValue.Ticks - completedTicks < TimeSpan.TicksPerDay)
        {
            return null;
        }

        return completedTicks + TimeSpan.TicksPerDay;
    }

    public static bool IsActive(long completedTicks, long expiresTicks, long nowTicks)
    {
        if (completedTicks < DateTime.MinValue.Ticks || completedTicks > DateTime.MaxValue.Ticks)
        {
            return false;
        }

        if (expiresTicks < DateTime.MinValue.Ticks || expiresTicks > DateTime.MaxValue.Ticks)
        {
            return false;
        }

        if (nowTicks < DateTime.MinValue.Ticks || nowTicks > DateTime.MaxValue.Ticks)
        {
            return false;
        }

        long? expectedExpires = ExpiresAt(completedTicks);
        if (!expectedExpires.HasValue || expiresTicks != expectedExpires.Value)
        {
            return false;
        }

        return completedTicks <= nowTicks && nowTicks < expiresTicks;
    }
}
