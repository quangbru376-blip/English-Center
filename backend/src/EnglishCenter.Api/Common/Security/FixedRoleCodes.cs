namespace EnglishCenter.Api.Common.Security;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

/// <summary>
/// Canonical fixed role codes and bootstrap validation helper.
/// </summary>
public static class FixedRoleCodes
{
    public const string Student = "Student";
    public const string Admissions = "Admissions";
    public const string AcademicAffairs = "AcademicAffairs";
    public const string Teacher = "Teacher";
    public const string Accountant = "Accountant";
    public const string Director = "Director";
    public const string Admin = "Admin";

    /// <summary>
    /// Gets the immutable list of the seven fixed roles in exact canonical order:
    /// Student, Admissions, AcademicAffairs, Teacher, Accountant, Director, Admin.
    /// </summary>
    public static IReadOnlyList<string> All { get; } = Array.AsReadOnly(
        new[]
        {
            Student,
            Admissions,
            AcademicAffairs,
            Teacher,
            Accountant,
            Director,
            Admin
        });

    private static readonly HashSet<string> ExpectedRoleSet = new(All, StringComparer.Ordinal);

    /// <summary>
    /// Verifies that the provided sequence contains exactly the seven fixed roles:
    /// no duplicates, no missing roles, no extra roles, no null values, and exact ordinal casing.
    /// Order of elements in the enumeration is ignored. The sequence is enumerated exactly once.
    /// </summary>
    /// <param name="codes">Sequence of role codes to validate.</param>
    /// <returns>True if the sequence contains exactly the seven fixed roles; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="codes"/> is null.</exception>
    public static bool MatchesExact(IEnumerable<string> codes)
    {
        if (codes is null)
        {
            throw new ArgumentNullException(nameof(codes));
        }

        var seen = new HashSet<string>(All.Count, StringComparer.Ordinal);

        foreach (var code in codes)
        {
            if (code is null || !ExpectedRoleSet.Contains(code) || !seen.Add(code))
            {
                return false;
            }
        }

        return seen.Count == All.Count;
    }
}
