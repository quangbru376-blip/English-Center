using System;
using System.Collections.Generic;

namespace EnglishCenter.Api.Common.Security;

/// <summary>
/// Pure helper for verifying canonical HTTPS origins against a configured trusted origin.
/// Supplements framework antiforgery / CSRF defenses without fallback to ambient headers.
/// </summary>
public static class SameOrigin
{
    /// <summary>
    /// Validates that an origin string is an exact canonical HTTPS origin:
    /// ASCII, no whitespace/control characters, absolute HTTPS scheme, no userinfo,
    /// no path beyond the implicit slash, no query, no fragment, canonical host casing,
    /// and omission of the default 443 port.
    /// </summary>
    public static bool IsCanonicalHttpsOrigin(string? origin)
    {
        if (string.IsNullOrEmpty(origin))
        {
            return false;
        }

        for (int i = 0; i < origin.Length; i++)
        {
            char c = origin[i];
            if (c > 127 || char.IsControl(c) || char.IsWhiteSpace(c))
            {
                return false;
            }
        }

        if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri))
        {
            return false;
        }

        if (!string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.Ordinal))
        {
            return false;
        }

        if (string.IsNullOrEmpty(uri.Host))
        {
            return false;
        }

        if (!string.IsNullOrEmpty(uri.UserInfo))
        {
            return false;
        }

        if (uri.AbsolutePath != "/")
        {
            return false;
        }

        if (!string.IsNullOrEmpty(uri.Query) || !string.IsNullOrEmpty(uri.Fragment))
        {
            return false;
        }

        string authority = uri.GetLeftPart(UriPartial.Authority);
        return string.Equals(origin, authority, StringComparison.Ordinal);
    }

    /// <summary>
    /// Compares a configured trusted origin with request Origin header values.
    /// Requires exactly one origin header and exact ordinal match against the canonical HTTPS origin.
    /// </summary>
    public static bool Matches(string configuredOrigin, IReadOnlyList<string> origins)
    {
        if (configuredOrigin is null)
        {
            throw new ArgumentNullException(nameof(configuredOrigin));
        }

        if (origins is null)
        {
            throw new ArgumentNullException(nameof(origins));
        }

        if (!IsCanonicalHttpsOrigin(configuredOrigin))
        {
            return false;
        }

        if (origins.Count != 1)
        {
            return false;
        }

        string? originHeader = origins[0];
        if (!IsCanonicalHttpsOrigin(originHeader))
        {
            return false;
        }

        return string.Equals(configuredOrigin, originHeader, StringComparison.Ordinal);
    }
}
