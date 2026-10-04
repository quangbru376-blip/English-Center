using System.Globalization;
using System.Numerics;

namespace EnglishCenter.Api.Common.Serialization;

public static class ReportAmountCodec
{
    public const int MaxDigits = 38;
    public static readonly BigInteger MaxValue = BigInteger.Pow(10, MaxDigits) - 1;

    public static BigInteger Parse(string? value)
    {
        if (value is null)
        {
            throw new ArgumentNullException(nameof(value), "Report amount cannot be null.");
        }

        if (value.Length == 0 || value.Length > MaxDigits)
        {
            throw new ArgumentException("Report amount must be between 1 and 38 digits.", nameof(value));
        }

        if (value.Length > 1 && value[0] == '0')
        {
            throw new ArgumentException("Report amount cannot have leading zeros.", nameof(value));
        }

        for (int i = 0; i < value.Length; i++)
        {
            char c = value[i];
            if (c is < '0' or > '9')
            {
                throw new ArgumentException("Report amount must contain only ASCII digits.", nameof(value));
            }
        }

        return BigInteger.Parse(value, NumberStyles.None, CultureInfo.InvariantCulture);
    }

    public static string Format(BigInteger value)
    {
        if (value < BigInteger.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(value), "Report amount cannot be negative.");
        }

        if (value > MaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(value), "Report amount exceeds maximum allowed value.");
        }

        return value.ToString(CultureInfo.InvariantCulture);
    }
}
