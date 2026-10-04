using System.Text;

namespace EnglishCenter.Api.Common.Validation;

public static class NonsecretText
{
    public static string NormalizeRequired(string? value, int maxCodePoints)
    {
        if (maxCodePoints <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxCodePoints), "Maximum code points must be greater than zero.");
        }

        if (value is null)
        {
            throw new ArgumentException("Value is required.", nameof(value));
        }

        ValidateUtf16Surrogates(value);

        string trimmed = value.Trim();
        if (trimmed.Length == 0)
        {
            throw new ArgumentException("Value cannot be empty or whitespace.", nameof(value));
        }

        return NormalizeAndValidateLength(trimmed, maxCodePoints, nameof(value));
    }

    public static string? NormalizeOptional(string? value, int maxCodePoints)
    {
        if (maxCodePoints <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxCodePoints), "Maximum code points must be greater than zero.");
        }

        if (value is null)
        {
            return null;
        }

        ValidateUtf16Surrogates(value);

        string trimmed = value.Trim();
        if (trimmed.Length == 0)
        {
            return null;
        }

        return NormalizeAndValidateLength(trimmed, maxCodePoints, nameof(value));
    }

    private static string NormalizeAndValidateLength(string trimmed, int maxCodePoints, string paramName)
    {
        string normalized = trimmed.Normalize(NormalizationForm.FormC);
        int codePointCount = CountCodePoints(normalized);
        if (codePointCount > maxCodePoints)
        {
            throw new ArgumentException("Value exceeds maximum allowed length.", paramName);
        }

        return normalized;
    }

    private static void ValidateUtf16Surrogates(string value)
    {
        for (int i = 0; i < value.Length; i++)
        {
            char c = value[i];
            if (char.IsSurrogate(c))
            {
                if (char.IsHighSurrogate(c) && i + 1 < value.Length && char.IsLowSurrogate(value[i + 1]))
                {
                    i++;
                    continue;
                }

                throw new ArgumentException("Value contains unpaired UTF-16 surrogate.", nameof(value));
            }
        }
    }

    private static int CountCodePoints(string s)
    {
        int count = 0;
        for (int i = 0; i < s.Length; i++)
        {
            count++;
            if (char.IsHighSurrogate(s[i]) && i + 1 < s.Length && char.IsLowSurrogate(s[i + 1]))
            {
                i++;
            }
        }

        return count;
    }
}
