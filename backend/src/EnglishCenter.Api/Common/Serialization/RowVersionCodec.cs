namespace EnglishCenter.Api.Common.Serialization;

public static class RowVersionCodec
{
    public static byte[] Decode(string? value)
    {
        if (value is null) throw new ArgumentNullException(nameof(value), "RowVersion cannot be null.");
        if (value.Length != 12) throw new ArgumentException("RowVersion must be exactly 12 characters.", nameof(value));
        if (value[11] != '=' || value[10] == '=')
        {
            throw new ArgumentException("RowVersion must have exactly one trailing padding character.", nameof(value));
        }

        for (int i = 0; i < 11; i++)
        {
            char c = value[i];
            if (!((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || (c >= '0' && c <= '9') || c is '+' or '/'))
            {
                throw new ArgumentException("RowVersion contains invalid characters.", nameof(value));
            }
        }

        byte[] bytes;
        try
        {
            bytes = Convert.FromBase64String(value);
        }
        catch (FormatException)
        {
            throw new ArgumentException("RowVersion is not a valid Base64 string.", nameof(value));
        }

        if (bytes.Length != 8) throw new ArgumentException("Decoded RowVersion must be exactly 8 bytes.", nameof(value));

        string reEncoded = Convert.ToBase64String(bytes);
        if (!string.Equals(reEncoded, value, StringComparison.Ordinal))
        {
            throw new ArgumentException("RowVersion is not in canonical Base64 form.", nameof(value));
        }

        return bytes;
    }

    public static string Encode(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length != 8) throw new ArgumentException("RowVersion byte sequence must be exactly 8 bytes.", nameof(bytes));

        return Convert.ToBase64String(bytes);
    }
}
