using System;
using System.Collections.Immutable;

namespace EnglishCenter.Api.Infrastructure.Readiness;

public sealed record RuntimeArtifactPin(string RelativePath, long Length, string Sha256)
{
    public override string ToString() => nameof(RuntimeArtifactPin);
}

public sealed record RuntimePatchManifest(
    Version DotNet,
    Version AspNet,
    Version Ef,
    Version Sql,
    DateOnly Reviewed,
    DateOnly NextReview,
    ImmutableArray<RuntimeArtifactPin> Artifacts)
{
    public override string ToString() => nameof(RuntimePatchManifest);
}

public static class RuntimePatchPolicy
{
    public static bool IsCurrent(RuntimePatchManifest? manifest, DateOnly today)
    {
        if (manifest is null ||
            manifest.Artifacts.IsDefault ||
            manifest.Artifacts.Length != 5 ||
            manifest.Reviewed >= manifest.NextReview ||
            today < manifest.Reviewed ||
            today >= manifest.NextReview)
        {
            return false;
        }

        if (!IsValidVersion(manifest.DotNet, 10) ||
            !IsValidVersion(manifest.AspNet, 10) ||
            !IsValidVersion(manifest.Ef, 10) ||
            !IsValidVersion(manifest.Sql, 17))
        {
            return false;
        }

        int seenMask = 0;
        foreach (var pin in manifest.Artifacts)
        {
            if (pin is null || pin.Length <= 0 || !IsHex64(pin.Sha256))
            {
                return false;
            }

            int index = GetExpectedIndex(pin.RelativePath);
            if (index < 0 || (seenMask & (1 << index)) != 0)
            {
                return false;
            }

            seenMask |= 1 << index;
        }

        return seenMask == 0b11111;
    }

    private static bool IsValidVersion(Version? version, int expectedMajor) =>
        version is not null && version.Major == expectedMajor && version.Minor >= 0 && version.Build >= 0;

    private static int GetExpectedIndex(string? path) => path switch
    {
        "backend/global.json" => 0,
        "backend/src/EnglishCenter.Api/EnglishCenter.Api.csproj" => 1,
        "backend/src/EnglishCenter.Api/packages.lock.json" => 2,
        "frontend/package.json" => 3,
        "frontend/package-lock.json" => 4,
        _ => -1,
    };

    private static bool IsHex64(string? sha)
    {
        if (sha is null || sha.Length != 64)
        {
            return false;
        }

        for (int i = 0; i < sha.Length; i++)
        {
            if (!char.IsAsciiHexDigit(sha[i]))
            {
                return false;
            }
        }

        return true;
    }
}
