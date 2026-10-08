using System;
using System.Text.Json;

namespace EnglishCenter.Api.Infrastructure.Readiness;

public static class BlocklistManifestPolicy
{
    public static bool IsCurrent(JsonElement manifest, DateOnly today)
    {
        if (today < new DateOnly(2026, 10, 4) || today >= new DateOnly(2027, 1, 4) || manifest.ValueKind != JsonValueKind.Object)
            return false;

        int rootPropCount = 0;
        bool hasLicense = false, hasRevision = false, hasSource = false, hasMergedCount = false;
        bool hasFiles = false, hasNextReview = false, hasComparison = false, hasReview = false;

        foreach (var prop in manifest.EnumerateObject())
        {
            rootPropCount++;
            switch (prop.Name)
            {
                case "license" when !hasLicense && prop.Value.ValueKind == JsonValueKind.String && prop.Value.GetString() == "MIT":
                    hasLicense = true; break;
                case "revision" when !hasRevision && prop.Value.ValueKind == JsonValueKind.String && prop.Value.GetString() == "56da8d991239cc52b0da594db53457504a757670":
                    hasRevision = true; break;
                case "source" when !hasSource && prop.Value.ValueKind == JsonValueKind.String && prop.Value.GetString() == "SecLists":
                    hasSource = true; break;
                case "comparison" when !hasComparison && prop.Value.ValueKind == JsonValueKind.String && prop.Value.GetString() == "Ordinal exact full string; no transforms":
                    hasComparison = true; break;
                case "reviewDate" when !hasReview && prop.Value.ValueKind == JsonValueKind.String && prop.Value.GetString() == "2026-10-04":
                    hasReview = true; break;
                case "nextReviewDate" when !hasNextReview && prop.Value.ValueKind == JsonValueKind.String && prop.Value.GetString() == "2027-01-04":
                    hasNextReview = true; break;
                case "mergedCount" when !hasMergedCount && prop.Value.ValueKind == JsonValueKind.Number && prop.Value.TryGetInt32(out var mc) && mc == 15310:
                    hasMergedCount = true; break;
                case "files" when !hasFiles && prop.Value.ValueKind == JsonValueKind.Array && ValidateFiles(prop.Value):
                    hasFiles = true; break;
                default:
                    return false;
            }
        }

        return rootPropCount == 8 && hasLicense && hasRevision && hasSource && hasComparison && hasReview && hasNextReview && hasMergedCount && hasFiles;
    }

    private static bool ValidateFiles(JsonElement files)
    {
        int count = 0;
        bool seen10k = false, seenProbable = false, seenLicense = false;

        foreach (var item in files.EnumerateArray())
        {
            count++;
            if (item.ValueKind != JsonValueKind.Object) return false;

            string? path = null, sha = null;
            int? records = null;
            int propCount = 0;
            bool hasPath = false, hasSha = false, hasRec = false;

            foreach (var p in item.EnumerateObject())
            {
                propCount++;
                if (p.Name == "path" && !hasPath && p.Value.ValueKind == JsonValueKind.String)
                {
                    hasPath = true; path = p.Value.GetString();
                }
                else if (p.Name == "sha256" && !hasSha && p.Value.ValueKind == JsonValueKind.String)
                {
                    hasSha = true; sha = p.Value.GetString();
                }
                else if (p.Name == "records" && !hasRec && p.Value.ValueKind == JsonValueKind.Number && p.Value.TryGetInt32(out var r))
                {
                    hasRec = true; records = r;
                }
                else return false;
            }

            if (!hasPath || !hasSha) return false;

            if (path == "10k-most-common.txt" && sha == "68782D6A4A19A4768D5F15DD66BD534E7A33055CC755411E33F16D18C50FDCCE" && records == 10001 && propCount == 3 && !seen10k)
                seen10k = true;
            else if (path == "probable-v2_top-12000.txt" && sha == "EA4C906EBB0B26790C549A047962573F72CCC26F42212B83D70165D9C03FB72B" && records == 12645 && propCount == 3 && !seenProbable)
                seenProbable = true;
            else if (path == "LICENSE" && sha == "3DBDC93D5F8829DE0941744841730A09C106D0732E5AE0E98CA1D77BE7DED66C" && !hasRec && propCount == 2 && !seenLicense)
                seenLicense = true;
            else
                return false;
        }

        return count == 3 && seen10k && seenProbable && seenLicense;
    }
}
