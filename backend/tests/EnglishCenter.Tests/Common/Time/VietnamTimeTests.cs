using EnglishCenter.Api.Common.Time;
using Xunit;

namespace EnglishCenter.Tests.Common.Time;

public sealed class VietnamTimeTests
{
    [Fact]
    public void ToDate_RejectsUpperOverflowInsteadOfClamping()
    {
        Assert.Throws<ArgumentOutOfRangeException>("instant", () => VietnamTime.ToDate(DateTimeOffset.MaxValue));
    }

    [Fact]
    public void FormatInstant_IsIndependentOfCurrentCulture()
    {
        var originalCulture = System.Globalization.CultureInfo.CurrentCulture;
        try
        {
            System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo("ar-SA");
            var instant = new DateTimeOffset(2026, 10, 4, 14, 30, 15, 123, TimeSpan.FromHours(7));
            Assert.Equal("2026-10-04T07:30:15.123Z", VietnamTime.FormatInstant(instant));
        }
        finally
        {
            System.Globalization.CultureInfo.CurrentCulture = originalCulture;
        }
    }
    [Fact]
    public void TimeZone_ModernBusinessDates_HasUtcPlusSevenOffset()
    {
        TimeZoneInfo zone = VietnamTime.TimeZone;
        var modernDate = new DateTime(2026, 10, 4, 12, 0, 0, DateTimeKind.Unspecified);

        TimeSpan offset = zone.GetUtcOffset(modernDate);

        Assert.Equal(TimeSpan.FromHours(7), offset);
    }

    [Fact]
    public void ToDate_ConvertsUtcInstantToVietnamDate()
    {
        var beforeMidnightUtc = new DateTimeOffset(2026, 10, 4, 16, 59, 59, 999, TimeSpan.Zero);
        var atMidnightUtc = new DateTimeOffset(2026, 10, 4, 17, 0, 0, 0, TimeSpan.Zero);

        Assert.Equal(new DateOnly(2026, 10, 4), VietnamTime.ToDate(beforeMidnightUtc));
        Assert.Equal(new DateOnly(2026, 10, 5), VietnamTime.ToDate(atMidnightUtc));
    }

    [Fact]
    public void ToDate_ConvertsNonZeroOffsetInstantBasedOnExactInstant()
    {
        var instantWithOffset = new DateTimeOffset(2026, 10, 4, 13, 0, 0, TimeSpan.FromHours(-4));

        DateOnly date = VietnamTime.ToDate(instantWithOffset);

        Assert.Equal(new DateOnly(2026, 10, 5), date);
    }

    [Fact]
    public void ToDate_Rollover_MonthYearLeapDay()
    {
        var monthEndUtc = new DateTimeOffset(2026, 1, 31, 17, 0, 0, TimeSpan.Zero);
        var yearEndUtc = new DateTimeOffset(2026, 12, 31, 17, 0, 0, TimeSpan.Zero);
        var leapDayUtc = new DateTimeOffset(2024, 2, 28, 17, 0, 0, TimeSpan.Zero);

        Assert.Equal(new DateOnly(2026, 2, 1), VietnamTime.ToDate(monthEndUtc));
        Assert.Equal(new DateOnly(2027, 1, 1), VietnamTime.ToDate(yearEndUtc));
        Assert.Equal(new DateOnly(2024, 2, 29), VietnamTime.ToDate(leapDayUtc));
    }

    [Fact]
    public void ToUtc_ValidMinutePrecision_ReturnsUtcInstant()
    {
        var date = new DateOnly(2026, 10, 4);
        var time = new TimeOnly(14, 30);

        DateTimeOffset utc = VietnamTime.ToUtc(date, time);

        Assert.Equal(TimeSpan.Zero, utc.Offset);
        Assert.Equal(new DateTimeOffset(2026, 10, 4, 7, 30, 0, TimeSpan.Zero), utc);
    }

    [Fact]
    public void ToUtc_MidnightVietnam_BecomesPreviousDayUtc()
    {
        var date = new DateOnly(2026, 10, 5);
        var time = new TimeOnly(0, 0);

        DateTimeOffset utc = VietnamTime.ToUtc(date, time);

        Assert.Equal(new DateTimeOffset(2026, 10, 4, 17, 0, 0, TimeSpan.Zero), utc);
    }

    [Fact]
    public void ToUtc_RejectsSecondsAndSubseconds()
    {
        var date = new DateOnly(2026, 10, 4);

        var timeWithSeconds = new TimeOnly(14, 30, 1);
        var timeWithMilliseconds = new TimeOnly(14, 30, 0, 1);
        var timeWithMicroseconds = new TimeOnly(14, 30, 0, 0, 1);

        Assert.Throws<ArgumentException>("time", () => VietnamTime.ToUtc(date, timeWithSeconds));
        Assert.Throws<ArgumentException>("time", () => VietnamTime.ToUtc(date, timeWithMilliseconds));
        Assert.Throws<ArgumentException>("time", () => VietnamTime.ToUtc(date, timeWithMicroseconds));
    }

    [Fact]
    public void ToUtc_RejectsUnrepresentableLowerBoundWithoutSilentSaturation()
    {
        var minDate = DateOnly.MinValue;
        var minTime = TimeOnly.MinValue;

        Assert.Throws<ArgumentOutOfRangeException>("date", () => VietnamTime.ToUtc(minDate, minTime));
    }

    [Fact]
    public void ToUtc_RepresentableMinimumDateRoundTrips()
    {
        var minDate = DateOnly.MinValue;
        var lowestValidTime = new TimeOnly(12, 0);

        DateTimeOffset utc = VietnamTime.ToUtc(minDate, lowestValidTime);

        Assert.Equal(minDate, VietnamTime.ToDate(utc));
    }

    [Fact]
    public void InclusiveDateRange_ValidSingleDayRange()
    {
        var date = new DateOnly(2026, 10, 4);

        var range = VietnamTime.InclusiveDateRange(date, date);

        var expectedStart = new DateTimeOffset(2026, 10, 3, 17, 0, 0, TimeSpan.Zero);
        var expectedEnd = new DateTimeOffset(2026, 10, 4, 17, 0, 0, TimeSpan.Zero);

        Assert.Equal(expectedStart, range.StartInclusive);
        Assert.Equal(expectedEnd, range.EndExclusive);
        Assert.Equal(TimeSpan.FromHours(24), range.EndExclusive - range.StartInclusive);
    }

    [Fact]
    public void InclusiveDateRange_ValidMultiDayAndCrossings()
    {
        var monthRange = VietnamTime.InclusiveDateRange(new DateOnly(2026, 1, 31), new DateOnly(2026, 2, 1));
        Assert.Equal(new DateTimeOffset(2026, 1, 30, 17, 0, 0, TimeSpan.Zero), monthRange.StartInclusive);
        Assert.Equal(new DateTimeOffset(2026, 2, 1, 17, 0, 0, TimeSpan.Zero), monthRange.EndExclusive);

        var yearRange = VietnamTime.InclusiveDateRange(new DateOnly(2026, 12, 31), new DateOnly(2026, 12, 31));
        Assert.Equal(new DateTimeOffset(2026, 12, 30, 17, 0, 0, TimeSpan.Zero), yearRange.StartInclusive);
        Assert.Equal(new DateTimeOffset(2026, 12, 31, 17, 0, 0, TimeSpan.Zero), yearRange.EndExclusive);

        var leapRange = VietnamTime.InclusiveDateRange(new DateOnly(2024, 2, 28), new DateOnly(2024, 2, 29));
        Assert.Equal(new DateTimeOffset(2024, 2, 27, 17, 0, 0, TimeSpan.Zero), leapRange.StartInclusive);
        Assert.Equal(new DateTimeOffset(2024, 2, 29, 17, 0, 0, TimeSpan.Zero), leapRange.EndExclusive);
    }

    [Fact]
    public void InclusiveDateRange_ReversedDates_ThrowsArgumentException()
    {
        var from = new DateOnly(2026, 10, 5);
        var to = new DateOnly(2026, 10, 4);

        Assert.Throws<ArgumentException>("from", () => VietnamTime.InclusiveDateRange(from, to));
    }

    [Fact]
    public void InclusiveDateRange_DateOnlyMaxValueEnd_ThrowsArgumentOutOfRangeException()
    {
        var from = new DateOnly(2026, 10, 4);
        var to = DateOnly.MaxValue;

        Assert.Throws<ArgumentOutOfRangeException>("to", () => VietnamTime.InclusiveDateRange(from, to));
    }

    [Fact]
    public void InclusiveDateRange_UnrepresentableLowerBoundary_ThrowsArgumentOutOfRangeException()
    {
        var from = DateOnly.MinValue;
        var to = new DateOnly(2026, 10, 4);

        Assert.Throws<ArgumentOutOfRangeException>("date", () => VietnamTime.InclusiveDateRange(from, to));
    }

    [Fact]
    public void FormatInstant_FormatsExactInvariantUtcString()
    {
        var instant = new DateTimeOffset(2026, 10, 4, 14, 30, 15, 123, TimeSpan.Zero);

        string formatted = VietnamTime.FormatInstant(instant);

        Assert.Equal("2026-10-04T14:30:15.123Z", formatted);
    }

    [Fact]
    public void FormatInstant_TruncatesSubmilliseconds_NeverRoundsIntoFuture()
    {
        var baseInstant = new DateTimeOffset(2026, 10, 4, 14, 30, 15, 123, TimeSpan.Zero);
        var instantWithTicks = baseInstant.AddTicks(9999);

        string formatted = VietnamTime.FormatInstant(instantWithTicks);

        Assert.Equal("2026-10-04T14:30:15.123Z", formatted);
    }

    [Fact]
    public void FormatInstant_NormalizesNonZeroOffsetToUtc()
    {
        var instantVietnam = new DateTimeOffset(2026, 10, 4, 14, 30, 15, 123, TimeSpan.FromHours(7));

        string formatted = VietnamTime.FormatInstant(instantVietnam);

        Assert.Equal("2026-10-04T07:30:15.123Z", formatted);
    }
}
