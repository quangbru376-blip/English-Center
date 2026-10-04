using EnglishCenter.Api.Common.Time;
using EnglishCenter.Api.Common.Validation;
using Xunit;

namespace EnglishCenter.Tests.Common.Validation;

public class StudentBirthDateValidatorTests
{
    private sealed class MutableTimeProvider : TimeProvider
    {
        private DateTimeOffset _utcNow;

        public MutableTimeProvider(DateTimeOffset utcNow)
        {
            _utcNow = utcNow;
        }

        public void SetUtcNow(DateTimeOffset utcNow)
        {
            _utcNow = utcNow;
        }

        public override DateTimeOffset GetUtcNow() => _utcNow;
    }

    [Fact]
    public void Constructor_NullClock_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new StudentBirthDateValidator(null!));
    }

    [Fact]
    public void Validate_YesterdayAndToday_ReturnsSameDate()
    {
        var timeProvider = new MutableTimeProvider(new DateTimeOffset(2026, 10, 4, 10, 0, 0, TimeSpan.Zero));
        var clock = new BusinessClock(timeProvider);
        var validator = new StudentBirthDateValidator(clock);

        DateOnly today = new(2026, 10, 4);
        DateOnly yesterday = new(2026, 10, 3);

        Assert.Equal(today, validator.Validate(today));
        Assert.Equal(yesterday, validator.Validate(yesterday));
    }

    [Fact]
    public void Validate_FutureDate_ThrowsArgumentException()
    {
        var timeProvider = new MutableTimeProvider(new DateTimeOffset(2026, 10, 4, 10, 0, 0, TimeSpan.Zero));
        var clock = new BusinessClock(timeProvider);
        var validator = new StudentBirthDateValidator(clock);

        DateOnly tomorrow = new(2026, 10, 5);

        var ex = Assert.Throws<ArgumentException>(() => validator.Validate(tomorrow));
        Assert.DoesNotContain(tomorrow.ToString(), ex.Message);
    }

    [Fact]
    public void Validate_NoMinimumOrMaximumAgeRestrictions_AcceptsOldDatesAndMinValue()
    {
        var timeProvider = new MutableTimeProvider(new DateTimeOffset(2026, 10, 4, 10, 0, 0, TimeSpan.Zero));
        var clock = new BusinessClock(timeProvider);
        var validator = new StudentBirthDateValidator(clock);

        DateOnly centuryAgo = new(1926, 10, 4);
        DateOnly minValue = DateOnly.MinValue;

        Assert.Equal(centuryAgo, validator.Validate(centuryAgo));
        Assert.Equal(minValue, validator.Validate(minValue));
    }

    [Fact]
    public void Validate_VietnamMidnightRollover_RespectsVietnamCalendarDay()
    {
        var timeProvider = new MutableTimeProvider(new DateTimeOffset(2026, 10, 4, 16, 59, 59, TimeSpan.Zero));
        var clock = new BusinessClock(timeProvider);
        var validator = new StudentBirthDateValidator(clock);

        DateOnly oct04 = new(2026, 10, 4);
        DateOnly oct05 = new(2026, 10, 5);

        Assert.Equal(oct04, validator.Validate(oct04));
        Assert.Throws<ArgumentException>(() => validator.Validate(oct05));

        timeProvider.SetUtcNow(new DateTimeOffset(2026, 10, 4, 17, 0, 0, TimeSpan.Zero));

        Assert.Equal(oct04, validator.Validate(oct04));
        Assert.Equal(oct05, validator.Validate(oct05));
    }

    [Fact]
    public void Validate_YearAndLeapDayRollover_RespectsVietnamDates()
    {
        var timeProvider = new MutableTimeProvider(new DateTimeOffset(2024, 2, 28, 17, 0, 0, TimeSpan.Zero));
        var clock = new BusinessClock(timeProvider);
        var validator = new StudentBirthDateValidator(clock);

        DateOnly leapDay = new(2024, 2, 29);
        DateOnly march01 = new(2024, 3, 1);

        Assert.Equal(leapDay, validator.Validate(leapDay));
        Assert.Throws<ArgumentException>(() => validator.Validate(march01));

        timeProvider.SetUtcNow(new DateTimeOffset(2024, 12, 31, 17, 0, 0, TimeSpan.Zero));

        DateOnly newYearDay = new(2025, 1, 1);
        DateOnly newYearTomorrow = new(2025, 1, 2);

        Assert.Equal(newYearDay, validator.Validate(newYearDay));
        Assert.Throws<ArgumentException>(() => validator.Validate(newYearTomorrow));
    }
}
