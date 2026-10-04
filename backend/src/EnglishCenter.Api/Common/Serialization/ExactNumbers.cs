using System.Globalization;

namespace EnglishCenter.Api.Common.Serialization;

public static class ExactNumbers
{
    private const decimal MaxMoney = 999_999_999_999_999_999m;

    public static decimal ParseMoney(string? value)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (value.Length == 0 || value.Any(c => c is < '0' or > '9'))
            throw new ArgumentException("Money must contain ASCII digits only.", nameof(value));

        string significant = value.TrimStart('0');
        if (significant.Length is 0 or > 18)
            throw new ArgumentException("Money must be positive and within the permitted range.", nameof(value));

        return decimal.Parse(significant, NumberStyles.None, CultureInfo.InvariantCulture);
    }

    public static string FormatMoney(decimal value)
    {
        if (value <= 0 || value > MaxMoney || decimal.Truncate(value) != value)
            throw new ArgumentException("Money must be a positive whole amount within the permitted range.", nameof(value));

        return value.ToString("0", CultureInfo.InvariantCulture);
    }

    public static decimal ParseScore(string? value) => ParseGrade(value, allowZero: true);
    public static decimal ParseWeight(string? value) => ParseGrade(value, allowZero: false);
    public static string FormatScore(decimal value) => FormatGrade(value, allowZero: true);
    public static string FormatWeight(decimal value) => FormatGrade(value, allowZero: false);

    private static decimal ParseGrade(string? value, bool allowZero)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (value.Length == 0)
            throw new ArgumentException("A decimal string is required.", nameof(value));

        int dot = value.IndexOf('.');
        if (dot >= 0 && (dot == 0 || dot != value.LastIndexOf('.') || value.Length - dot - 1 is < 1 or > 2))
            throw new ArgumentException("Decimal input must have at most two fractional digits.", nameof(value));

        for (int i = 0; i < value.Length; i++)
        {
            if (i != dot && value[i] is < '0' or > '9')
                throw new ArgumentException("Decimal input must contain ASCII digits and an optional dot.", nameof(value));
        }

        if (!decimal.TryParse(value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out decimal result))
            throw new ArgumentException("Decimal input is outside the permitted range.", nameof(value));

        ValidateGradeRange(result, allowZero);
        return result;
    }

    private static string FormatGrade(decimal value, bool allowZero)
    {
        ValidateGradeRange(value, allowZero);
        if (decimal.Truncate(value * 100) != value * 100)
            throw new ArgumentException("Decimal value exceeds two fractional digits.", nameof(value));

        return value.ToString("0.00", CultureInfo.InvariantCulture);
    }

    private static void ValidateGradeRange(decimal value, bool allowZero)
    {
        if (value < 0 || value > 100 || (!allowZero && value == 0))
            throw new ArgumentException("Score or weight is outside the permitted range.", nameof(value));
    }
}
