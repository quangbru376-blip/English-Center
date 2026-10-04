using System.Globalization;
using System.Text.Json;

namespace EnglishCenter.Api.Common.Serialization;

public static class StrictScalars
{
    public static string NormalizeUuid(string? value)
    {
        if (value is null) throw new ArgumentNullException(nameof(value), "UUID cannot be null.");
        if (value.Length != 36) throw new ArgumentException("UUID must be exactly 36 characters.", nameof(value));

        for (int i = 0; i < 36; i++)
        {
            char c = value[i];
            if (i is 8 or 13 or 18 or 23)
            {
                if (c != '-') throw new ArgumentException("UUID format is invalid.", nameof(value));
            }
            else if (!((c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F')))
            {
                throw new ArgumentException("UUID format is invalid.", nameof(value));
            }
        }

        if (!Guid.TryParseExact(value, "D", out var guid) || guid == Guid.Empty)
        {
            throw new ArgumentException("UUID is invalid or empty.", nameof(value));
        }

        return guid.ToString("D");
    }

    public static string NormalizeUuidHeader(IReadOnlyList<string>? values)
    {
        if (values is null) throw new ArgumentNullException(nameof(values), "Header values cannot be null.");
        if (values.Count != 1) throw new ArgumentException("Expected exactly one header value.", nameof(values));

        return NormalizeUuid(values[0]);
    }

    public static DateOnly ParseLocalDate(string? value)
    {
        if (value is null) throw new ArgumentNullException(nameof(value), "Date cannot be null.");
        if (value.Length != 10) throw new ArgumentException("Date must be exactly 10 characters.", nameof(value));

        for (int i = 0; i < 10; i++)
        {
            char c = value[i];
            if (i is 4 or 7)
            {
                if (c != '-') throw new ArgumentException("Date format is invalid.", nameof(value));
            }
            else if (c is < '0' or > '9')
            {
                throw new ArgumentException("Date format is invalid.", nameof(value));
            }
        }

        if (!DateOnly.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
        {
            throw new ArgumentException("Date value is not a valid calendar date.", nameof(value));
        }

        return date;
    }

    public static TimeOnly ParseMinuteTime(string? value)
    {
        if (value is null) throw new ArgumentNullException(nameof(value), "Time cannot be null.");
        if (value.Length != 5) throw new ArgumentException("Time must be exactly 5 characters.", nameof(value));

        for (int i = 0; i < 5; i++)
        {
            char c = value[i];
            if (i == 2)
            {
                if (c != ':') throw new ArgumentException("Time format is invalid.", nameof(value));
            }
            else if (c is < '0' or > '9')
            {
                throw new ArgumentException("Time format is invalid.", nameof(value));
            }
        }

        if (!TimeOnly.TryParseExact(value, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var time))
        {
            throw new ArgumentException("Time value is not a valid time.", nameof(value));
        }

        return time;
    }

    public static int ParseInt32(JsonElement value, int min, int max)
    {
        if (min > max) throw new ArgumentException("Minimum value cannot be greater than maximum value.", nameof(min));
        if (value.ValueKind != JsonValueKind.Number) throw new ArgumentException("Expected JSON number.", nameof(value));

        string raw = value.GetRawText();
        if (string.IsNullOrEmpty(raw)) throw new ArgumentException("JSON number text is empty.", nameof(value));

        int startIndex = 0;
        if (raw[0] == '-')
        {
            startIndex = 1;
            if (raw.Length == 1) throw new ArgumentException("Invalid JSON integer format.", nameof(value));
        }

        for (int i = startIndex; i < raw.Length; i++)
        {
            if (raw[i] is < '0' or > '9') throw new ArgumentException("Invalid JSON integer format.", nameof(value));
        }

        if (!value.TryGetInt32(out int result))
        {
            throw new ArgumentOutOfRangeException(nameof(value), "Integer value is outside the Int32 range.");
        }
        if (result < min || result > max)
        {
            throw new ArgumentOutOfRangeException(nameof(value), "Value is outside the allowed range.");
        }

        return result;
    }
}
