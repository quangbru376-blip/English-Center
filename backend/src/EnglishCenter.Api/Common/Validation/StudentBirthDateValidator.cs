using EnglishCenter.Api.Common.Time;

namespace EnglishCenter.Api.Common.Validation;

public sealed class StudentBirthDateValidator
{
    private readonly BusinessClock _clock;

    public StudentBirthDateValidator(BusinessClock clock)
    {
        ArgumentNullException.ThrowIfNull(clock);
        _clock = clock;
    }

    public DateOnly Validate(DateOnly dateOfBirth)
    {
        DateOnly today = _clock.GetToday();
        if (dateOfBirth > today)
        {
            throw new ArgumentException("Date of birth cannot be in the future.", nameof(dateOfBirth));
        }

        return dateOfBirth;
    }
}
