namespace EnglishCenter.Api.Common.Replay;

using System;
using System.IO;
using System.Text;

/// <summary>
/// Encodes a string as a strict C1 JSON string literal in UTF-8 bytes.
/// </summary>
public static class C1StringEncoder
{
    private static readonly UTF8Encoding StrictUtf8 = new(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

    private static readonly byte[] HexDigits =
    [
        (byte)'0', (byte)'1', (byte)'2', (byte)'3', (byte)'4', (byte)'5', (byte)'6', (byte)'7',
        (byte)'8', (byte)'9', (byte)'a', (byte)'b', (byte)'c', (byte)'d', (byte)'e', (byte)'f'
    ];

    public static byte[] Encode(string value)
    {
        if (value is null)
        {
            throw new ArgumentNullException(nameof(value));
        }

        using var ms = new MemoryStream(value.Length + 16);
        ms.WriteByte((byte)'"');

        Span<byte> utf8Bytes = stackalloc byte[4];
        for (var i = 0; i < value.Length; i++)
        {
            var c = value[i];

            if (c == '"')
            {
                ms.WriteByte((byte)'\\');
                ms.WriteByte((byte)'"');
            }
            else if (c == '\\')
            {
                ms.WriteByte((byte)'\\');
                ms.WriteByte((byte)'\\');
            }
            else if (c <= 0x1F)
            {
                ms.WriteByte((byte)'\\');
                ms.WriteByte((byte)'u');
                ms.WriteByte((byte)'0');
                ms.WriteByte((byte)'0');
                ms.WriteByte(HexDigits[(c >> 4) & 0x0F]);
                ms.WriteByte(HexDigits[c & 0x0F]);
            }
            else if (c <= 0x7F)
            {
                ms.WriteByte((byte)c);
            }
            else if (char.IsSurrogate(c))
            {
                if (char.IsHighSurrogate(c) && i + 1 < value.Length && char.IsLowSurrogate(value[i + 1]))
                {
                    var written = StrictUtf8.GetBytes(value.AsSpan(i, 2), utf8Bytes);
                    ms.Write(utf8Bytes.Slice(0, written));
                    i++;
                }
                else
                {
                    throw new ArgumentException("String contains an unpaired surrogate.", nameof(value));
                }
            }
            else
            {
                var written = StrictUtf8.GetBytes(value.AsSpan(i, 1), utf8Bytes);
                ms.Write(utf8Bytes.Slice(0, written));
            }
        }

        ms.WriteByte((byte)'"');
        return ms.ToArray();
    }
}
