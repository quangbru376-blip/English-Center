using EnglishCenter.Api.Common.Time;
using Xunit;

namespace EnglishCenter.Tests.Common.Time;

public sealed class BusinessClockTests
{
    private sealed class MutableTimeProvider : TimeProvider
    {
        private DateTimeOffset _utcNow;
        public int GetUtcNowCallCount { get; private set; }

        public MutableTimeProvider(DateTimeOffset initialUtcNow)
        {
            _utcNow = initialUtcNow;
        }

        public override DateTimeOffset GetUtcNow()
        {
            GetUtcNowCallCount++;
            return _utcNow;
        }

        public void SetUtcNow(DateTimeOffset utcNow)
        {
            _utcNow = utcNow;
        }

        public void Advance(TimeSpan delta)
        {
            _utcNow = _utcNow.Add(delta);
        }
    }

    [Fact]
    public void Constructor_NullProvider_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>("timeProvider", () => new BusinessClock(null!));
    }

    [Fact]
    public void GetUtcNow_NormalizesNonZeroOffsetToUtc()
    {
        var rawInstant = new DateTimeOffset(2026, 10, 4, 14, 30, 0, TimeSpan.FromHours(7));
        var provider = new MutableTimeProvider(rawInstant);
        var clock = new BusinessClock(provider);

        DateTimeOffset utcNow = clock.GetUtcNow();

        Assert.Equal(TimeSpan.Zero, utcNow.Offset);
        Assert.Equal(new DateTimeOffset(2026, 10, 4, 7, 30, 0, TimeSpan.Zero), utcNow);
    }

    [Fact]
    public void GetUtcNow_TruncatesSubmillisecondTicks_NeverRoundsIntoFuture()
    {
        DateTimeOffset baseInstant = new DateTimeOffset(2026, 10, 4, 7, 30, 0, TimeSpan.Zero).AddMilliseconds(123);
        DateTimeOffset instantWithSubmilliseconds = baseInstant.AddTicks(9999);
        var provider = new MutableTimeProvider(instantWithSubmilliseconds);
        var clock = new BusinessClock(provider);

        DateTimeOffset utcNow = clock.GetUtcNow();

        Assert.Equal(baseInstant, utcNow);
        Assert.Equal(0, utcNow.Ticks % TimeSpan.TicksPerMillisecond);
    }

    [Fact]
    public void GetUtcNow_TracksMovingTime()
    {
        var initial = new DateTimeOffset(2026, 10, 4, 8, 0, 0, TimeSpan.Zero);
        var provider = new MutableTimeProvider(initial);
        var clock = new BusinessClock(provider);

        Assert.Equal(initial, clock.GetUtcNow());

        provider.Advance(TimeSpan.FromMinutes(45));

        Assert.Equal(initial.AddMinutes(45), clock.GetUtcNow());
    }

    [Fact]
    public void GetToday_CapturesSingleClockSample()
    {
        var provider = new MutableTimeProvider(new DateTimeOffset(2026, 10, 4, 8, 0, 0, TimeSpan.Zero));
        var clock = new BusinessClock(provider);

        DateOnly today = clock.GetToday();

        Assert.Equal(1, provider.GetUtcNowCallCount);
        Assert.Equal(new DateOnly(2026, 10, 4), today);
    }

    [Theory]
    [InlineData(2026, 10, 4, 16, 59, 59, 999, 2026, 10, 4)]
    [InlineData(2026, 10, 4, 17, 0, 0, 0, 2026, 10, 5)]
    public void GetToday_VietnamMidnightRollover(
        int utcYear, int utcMonth, int utcDay, int utcHour, int utcMinute, int utcSecond, int utcMs,
        int expectedYear, int expectedMonth, int expectedDay)
    {
        var instant = new DateTimeOffset(utcYear, utcMonth, utcDay, utcHour, utcMinute, utcSecond, utcMs, TimeSpan.Zero);
        var provider = new MutableTimeProvider(instant);
        var clock = new BusinessClock(provider);

        DateOnly today = clock.GetToday();

        Assert.Equal(new DateOnly(expectedYear, expectedMonth, expectedDay), today);
    }

    [Fact]
    public void GetToday_VietnamYearRollover()
    {
        var beforeMidnightUtc = new DateTimeOffset(2026, 12, 31, 16, 59, 59, 999, TimeSpan.Zero);
        var atMidnightUtc = new DateTimeOffset(2026, 12, 31, 17, 0, 0, 0, TimeSpan.Zero);
        var provider = new MutableTimeProvider(beforeMidnightUtc);
        var clock = new BusinessClock(provider);

        Assert.Equal(new DateOnly(2026, 12, 31), clock.GetToday());

        provider.SetUtcNow(atMidnightUtc);
        Assert.Equal(new DateOnly(2027, 1, 1), clock.GetToday());
    }

    [Fact]
    public void GetToday_VietnamLeapDayRollover()
    {
        var feb28Utc = new DateTimeOffset(2024, 2, 28, 16, 59, 59, TimeSpan.Zero);
        var leapDayUtc = new DateTimeOffset(2024, 2, 28, 17, 0, 0, TimeSpan.Zero);
        var march1Utc = new DateTimeOffset(2024, 2, 29, 17, 0, 0, TimeSpan.Zero);
        var provider = new MutableTimeProvider(feb28Utc);
        var clock = new BusinessClock(provider);

        Assert.Equal(new DateOnly(2024, 2, 28), clock.GetToday());

        provider.SetUtcNow(leapDayUtc);
        Assert.Equal(new DateOnly(2024, 2, 29), clock.GetToday());

        provider.SetUtcNow(march1Utc);
        Assert.Equal(new DateOnly(2024, 3, 1), clock.GetToday());
    }

    [Fact]
    public void IsBefore_StrictInstantComparison_EqualityIsExpired()
    {
        var cutoff = new DateTimeOffset(2026, 10, 4, 12, 0, 0, TimeSpan.Zero);
        var provider = new MutableTimeProvider(cutoff.AddMilliseconds(-1));
        var clock = new BusinessClock(provider);

        Assert.True(clock.IsBefore(cutoff));

        provider.SetUtcNow(cutoff);
        Assert.False(clock.IsBefore(cutoff));

        provider.SetUtcNow(cutoff.AddMilliseconds(1));
        Assert.False(clock.IsBefore(cutoff));
    }

    [Fact]
    public void IsBefore_PreservesSubmillisecondPrecision_DoesNotTruncateGuard()
    {
        var cutoff = new DateTimeOffset(2026, 10, 4, 12, 0, 0, TimeSpan.Zero).AddTicks(5000);
        var provider = new MutableTimeProvider(cutoff.AddTicks(-1));
        var clock = new BusinessClock(provider);

        Assert.True(clock.IsBefore(cutoff));

        provider.SetUtcNow(cutoff.AddTicks(1));
        Assert.False(clock.IsBefore(cutoff));
    }

    [Fact]
    public void IsBefore_ComparesAcrossDifferentOffsets()
    {
        var cutoffUtc = new DateTimeOffset(2026, 10, 4, 12, 0, 0, TimeSpan.Zero);
        var cutoffVietnam = new DateTimeOffset(2026, 10, 4, 19, 0, 0, TimeSpan.FromHours(7));
        var provider = new MutableTimeProvider(new DateTimeOffset(2026, 10, 4, 11, 59, 59, TimeSpan.Zero));
        var clock = new BusinessClock(provider);

        Assert.True(clock.IsBefore(cutoffUtc));
        Assert.True(clock.IsBefore(cutoffVietnam));

        provider.SetUtcNow(cutoffUtc);
        Assert.False(clock.IsBefore(cutoffVietnam));
    }
}
