using System;

namespace EnglishCenter.Api.Common.Security;

/// <summary>
/// Pure helper for computing effective session deadlines.
/// </summary>
public static class SessionDeadlineMath
{
    public static DateTimeOffset? Normal(
        DateTimeOffset issuedAt,
        DateTimeOffset expiresAt,
        DateTimeOffset lastActivityAt,
        DateTimeOffset jwtExpiresAt)
    {
        var issuedUtc = issuedAt.ToUniversalTime();
        var expiresUtc = expiresAt.ToUniversalTime();
        var lastActivityUtc = lastActivityAt.ToUniversalTime();
        var jwtExpiresUtc = jwtExpiresAt.ToUniversalTime();

        if (expiresUtc <= issuedUtc || jwtExpiresUtc <= issuedUtc)
        {
            return null;
        }

        if (lastActivityUtc < issuedUtc || lastActivityUtc > expiresUtc)
        {
            return null;
        }

        DateTimeOffset maxAbsolute;
        DateTimeOffset idleDeadline;

        try
        {
            maxAbsolute = issuedUtc.AddHours(2);
            idleDeadline = lastActivityUtc.AddMinutes(30);
        }
        catch (ArgumentOutOfRangeException)
        {
            return null;
        }

        var effectiveDeadline = expiresUtc;

        if (maxAbsolute < effectiveDeadline)
        {
            effectiveDeadline = maxAbsolute;
        }

        if (idleDeadline < effectiveDeadline)
        {
            effectiveDeadline = idleDeadline;
        }

        if (jwtExpiresUtc < effectiveDeadline)
        {
            effectiveDeadline = jwtExpiresUtc;
        }

        return effectiveDeadline;
    }

    public static DateTimeOffset? Restricted(
        DateTimeOffset issuedAt,
        DateTimeOffset expiresAt,
        DateTimeOffset jwtExpiresAt,
        DateTimeOffset temporaryExpiresAt)
    {
        var issuedUtc = issuedAt.ToUniversalTime();
        var expiresUtc = expiresAt.ToUniversalTime();
        var jwtExpiresUtc = jwtExpiresAt.ToUniversalTime();
        var temporaryExpiresUtc = temporaryExpiresAt.ToUniversalTime();

        if (expiresUtc <= issuedUtc || jwtExpiresUtc <= issuedUtc)
        {
            return null;
        }

        if (temporaryExpiresUtc <= issuedUtc)
        {
            return null;
        }

        DateTimeOffset maxAbsolute;

        try
        {
            maxAbsolute = issuedUtc.AddMinutes(10);
        }
        catch (ArgumentOutOfRangeException)
        {
            return null;
        }

        var effectiveDeadline = expiresUtc;

        if (maxAbsolute < effectiveDeadline)
        {
            effectiveDeadline = maxAbsolute;
        }

        if (temporaryExpiresUtc < effectiveDeadline)
        {
            effectiveDeadline = temporaryExpiresUtc;
        }

        if (jwtExpiresUtc < effectiveDeadline)
        {
            effectiveDeadline = jwtExpiresUtc;
        }

        return effectiveDeadline;
    }
}
