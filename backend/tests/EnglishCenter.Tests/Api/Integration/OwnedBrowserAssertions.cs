using System.Globalization;
using System.Text.Json;
using EnglishCenter.Api.Domain.Administration;
using Xunit;

namespace EnglishCenter.Tests.Api.Integration;

internal static class OwnedBrowserAssertions
{
    public static void Lock(AuditLog audit, ReplayOperation marker, int actor, int target, int revoked, DateTime at)
    {
        Assert.NotNull(audit);
        Assert.NotNull(marker);
        Assert.True(audit.AuditId > 0, "AuditId must be positive.");
        Assert.True(marker.ReplayId > 0, "ReplayId must be positive.");
        Assert.NotEqual(Guid.Empty, audit.OperationId);
        Assert.NotEqual(Guid.Empty, marker.RequestId);
        Assert.NotEqual(Guid.Empty, marker.OperationId);
        Assert.True(audit.OperationId == marker.OperationId, "Persisted lock projection mismatch.");
        Assert.True(actor == audit.ActorAccountId, "Persisted lock projection mismatch.");
        Assert.True(actor == marker.ActorAccountId, "Persisted lock projection mismatch.");
        Assert.True("ADMIN.ACCOUNT.LOCK" == audit.Action, "Persisted lock projection mismatch.");
        Assert.True("ADMIN.ACCOUNT.LOCK" == marker.ActionCode, "Persisted lock projection mismatch.");
        Assert.True("Account" == audit.EntityType, "Persisted lock projection mismatch.");
        Assert.True(target.ToString(CultureInfo.InvariantCulture) == audit.EntityId, "Persisted lock projection mismatch.");
        Assert.True("SUCCESS" == audit.Result, "Persisted lock projection mismatch.");
        Assert.True("MANDATORY" == audit.AuditCategory, "Persisted lock projection mismatch.");
        Assert.True(at == audit.OccurredAt, "Persisted lock projection mismatch.");
        Assert.Null(audit.SecurityExpiresAt);
        Assert.Null(audit.IpExpiresAt);
        Assert.Null(audit.IpAddress);
        Assert.NotNull(marker.InputDigest);
        Assert.True(32 == marker.InputDigest.Length, "Persisted lock projection mismatch.");
        Assert.True(at == marker.CompletedAt, "Persisted lock projection mismatch.");
        Assert.True(at.AddHours(24) == marker.ExpiresAt, "Persisted lock projection mismatch.");

        if (marker.ResultReference is null) Assert.Fail("ResultReference must not be null.");
        try
        {
            using var refDoc = JsonDocument.Parse(marker.ResultReference);
            var root = refDoc.RootElement;
            if (root.ValueKind != JsonValueKind.Object || PropCount(root) != 2) Assert.Fail("Invalid replay root.");
            if (!root.TryGetProperty("outcome", out var outProp) || outProp.ValueKind != JsonValueKind.String || outProp.GetString() != "COMPLETED") Assert.Fail("Invalid outcome.");
            if (!root.TryGetProperty("target", out var tgt) || tgt.ValueKind != JsonValueKind.Object || PropCount(tgt) != 1) Assert.Fail("Invalid target.");
            if (!tgt.TryGetProperty("accountId", out var acc) || !MatchesInt(acc, target)) Assert.Fail("Invalid target accountId.");
        }
        catch (JsonException) { Assert.Fail("Malformed replay ResultReference JSON."); }

        if (audit.Details is null) Assert.Fail("Audit Details must not be null.");
        try
        {
            using var detDoc = JsonDocument.Parse(audit.Details);
            var root = detDoc.RootElement;
            if (root.ValueKind != JsonValueKind.Object || PropCount(root) != 2) Assert.Fail("Invalid details root.");
            if (!root.TryGetProperty("old", out var oldEl) || oldEl.ValueKind != JsonValueKind.Object || PropCount(oldEl) != 2) Assert.Fail("Invalid old details.");
            if (!oldEl.TryGetProperty("status", out var oldSt) || oldSt.ValueKind != JsonValueKind.String || oldSt.GetString() != "ACTIVE") Assert.Fail("Invalid old status.");
            if (!oldEl.TryGetProperty("securityVersion", out var oldSv) || !MatchesInt(oldSv, 1)) Assert.Fail("Invalid old version.");
            if (!root.TryGetProperty("new", out var newEl) || newEl.ValueKind != JsonValueKind.Object || PropCount(newEl) != 3) Assert.Fail("Invalid new details.");
            if (!newEl.TryGetProperty("status", out var newSt) || newSt.ValueKind != JsonValueKind.String || newSt.GetString() != "LOCKED") Assert.Fail("Invalid new status.");
            if (!newEl.TryGetProperty("securityVersion", out var newSv) || !MatchesInt(newSv, 2)) Assert.Fail("Invalid new version.");
            if (!newEl.TryGetProperty("revokedSessionCount", out var newRev) || !MatchesInt(newRev, revoked)) Assert.Fail("Invalid revoked count.");
        }
        catch (JsonException) { Assert.Fail("Malformed audit Details JSON."); }
    }

    private static int PropCount(JsonElement e) { int c = 0; foreach (var _ in e.EnumerateObject()) c++; return c; }
    private static bool MatchesInt(JsonElement e, int exp) => e.ValueKind == JsonValueKind.Number && ((e.TryGetInt32(out var i) && i == exp) || (e.TryGetInt64(out var l) && l == exp));
}
