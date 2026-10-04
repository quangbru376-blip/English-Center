using System.Globalization;
using System.Net.Mail;
using System.Text;

namespace EnglishCenter.Api.Common.Validation;

public static class ContactNormalizer
{
    public static string NormalizeEmail(string? value)
    {
        if (value is null)
        {
            throw new ArgumentException("Email is required.", nameof(value));
        }

        string trimmed = value.Trim();
        if (trimmed.Length == 0)
        {
            throw new ArgumentException("Email is required.", nameof(value));
        }

        if (trimmed.Length > 254)
        {
            throw new ArgumentException("Email exceeds maximum allowed length of 254 characters.", nameof(value));
        }

        for (int i = 0; i < trimmed.Length; i++)
        {
            char c = trimmed[i];
            if (c > 127 || char.IsControl(c) || char.IsWhiteSpace(c))
            {
                throw new ArgumentException("Email contains invalid characters.", nameof(value));
            }
        }

        int atIndex = trimmed.IndexOf('@');
        if (atIndex <= 0 || atIndex != trimmed.LastIndexOf('@') || atIndex == trimmed.Length - 1)
        {
            throw new ArgumentException("Email must have a single '@' with local and domain parts.", nameof(value));
        }

        if (!MailAddress.TryCreate(trimmed, out MailAddress? parsed) ||
            !string.Equals(parsed.Address, trimmed, StringComparison.Ordinal))
        {
            throw new ArgumentException("Email address format is invalid.", nameof(value));
        }

        string local = trimmed[..atIndex];
        string domain = trimmed[(atIndex + 1)..];
        bool badLocalDot = !local.StartsWith('"') && local.EndsWith('.');
        bool badDomainLabel = !domain.StartsWith('[') && domain.Split('.').Any(label =>
            label.Length == 0 || label.StartsWith('-') || label.EndsWith('-'));
        if (badLocalDot || badDomainLabel)
        {
            throw new ArgumentException("Email address format is invalid.", nameof(value));
        }

        return trimmed.ToLowerInvariant();
    }

    public static string NormalizePhone(string? value)
    {
        if (value is null)
        {
            throw new ArgumentException("Phone number is required.", nameof(value));
        }

        string trimmed = value.Trim();
        if (trimmed.Length == 0)
        {
            throw new ArgumentException("Phone number is required.", nameof(value));
        }

        return NormalizePhoneInternal(trimmed, nameof(value));
    }

    public static string? NormalizeOptionalPhone(string? value)
    {
        if (value is null)
        {
            return null;
        }

        string trimmed = value.Trim();
        if (trimmed.Length == 0)
        {
            return null;
        }

        return NormalizePhoneInternal(trimmed, nameof(value));
    }

    private static string NormalizePhoneInternal(string trimmed, string paramName)
    {
        var sb = new StringBuilder(trimmed.Length);
        for (int i = 0; i < trimmed.Length; i++)
        {
            char c = trimmed[i];
            if (char.GetUnicodeCategory(c) == UnicodeCategory.SpaceSeparator || c == '-')
            {
                continue;
            }

            if (c == '+' || (c >= '0' && c <= '9'))
            {
                sb.Append(c);
            }
            else
            {
                throw new ArgumentException("Phone number contains invalid characters.", paramName);
            }
        }

        if (sb.Length == 0)
        {
            throw new ArgumentException("Phone number cannot consist only of separators.", paramName);
        }

        string compact = sb.ToString();
        if (compact.Length == 10 && compact[0] == '0' && AreAllDigits(compact, 0))
        {
            return compact;
        }

        if (compact.Length == 12 && compact.StartsWith("+84", StringComparison.Ordinal) && AreAllDigits(compact, 3))
        {
            return "0" + compact.Substring(3);
        }

        throw new ArgumentException("Phone number must be 10 digits starting with 0 or +84 followed by 9 digits.", paramName);
    }

    private static bool AreAllDigits(string text, int startIndex)
    {
        for (int i = startIndex; i < text.Length; i++)
        {
            if (text[i] < '0' || text[i] > '9')
            {
                return false;
            }
        }

        return true;
    }
}
