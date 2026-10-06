using System;
using System.Security.Cryptography;

namespace EnglishCenter.Api.Common.Security;

public static class PasswordChangeVersionGuard
{
    public static bool Matches(ReadOnlySpan<byte> expected, ReadOnlySpan<byte> current)
    {
        if (expected.Length != 8 || current.Length != 8)
        {
            return false;
        }

        return CryptographicOperations.FixedTimeEquals(expected, current);
    }

    public static long? Next(long current)
    {
        if (current >= 1 && current < long.MaxValue)
        {
            return current + 1;
        }

        return null;
    }
}
