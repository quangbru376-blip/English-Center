using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace EnglishCenter.Api.Infrastructure.Compatibility;

/// <summary>
/// Pure helper for ordinal catalog comparison and module definition normalization.
/// </summary>
public static class CatalogCompatibility
{
    private static readonly Regex ModuleHeaderRegex = new(
        @"^(?:CREATE\s+OR\s+ALTER|CREATE|ALTER)(?:\s+|$)",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>
    /// Compares two dictionaries for an exact match of key/value pairs using ordinal comparison,
    /// independent of either dictionary's configured comparer.
    /// </summary>
    public static bool Matches(
        IReadOnlyDictionary<string, string> expected,
        IReadOnlyDictionary<string, string> actual)
    {
        ArgumentNullException.ThrowIfNull(expected);
        ArgumentNullException.ThrowIfNull(actual);

        if (expected.Count != actual.Count)
        {
            return false;
        }

        var expectedMap = new Dictionary<string, string>(expected.Count, StringComparer.Ordinal);
        foreach (var pair in expected)
        {
            if (pair.Key is null || !expectedMap.TryAdd(pair.Key, pair.Value))
            {
                return false;
            }
        }

        var actualMap = new Dictionary<string, string>(actual.Count, StringComparer.Ordinal);
        foreach (var pair in actual)
        {
            if (pair.Key is null || !actualMap.TryAdd(pair.Key, pair.Value))
            {
                return false;
            }
        }

        if (expectedMap.Count != actualMap.Count)
        {
            return false;
        }

        foreach (var pair in expectedMap)
        {
            if (!actualMap.TryGetValue(pair.Key, out var actualValue))
            {
                return false;
            }

            if (!string.Equals(pair.Value, actualValue, StringComparison.Ordinal))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Normalizes CRLF and lone CR to LF, trims outer whitespace, and normalizes
    /// leading CREATE, ALTER, or CREATE OR ALTER headers to a canonical CREATE header.
    /// </summary>
    public static string NormalizeModuleDefinition(string definition)
    {
        ArgumentNullException.ThrowIfNull(definition);

        string normalized = definition
            .Replace("\r\n", "\n")
            .Replace('\r', '\n')
            .Trim();

        var match = ModuleHeaderRegex.Match(normalized);
        if (!match.Success)
        {
            return normalized;
        }

        if (match.Length == normalized.Length)
        {
            return "CREATE";
        }

        return "CREATE " + normalized[match.Length..];
    }

    /// <summary>
    /// Compares two module definitions using ordinal comparison after header normalization.
    /// </summary>
    public static bool ModuleMatches(string expected, string actual)
    {
        ArgumentNullException.ThrowIfNull(expected);
        ArgumentNullException.ThrowIfNull(actual);

        return string.Equals(
            NormalizeModuleDefinition(expected),
            NormalizeModuleDefinition(actual),
            StringComparison.Ordinal);
    }
}
