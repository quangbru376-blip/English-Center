using System;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using EnglishCenter.Api.Domain.Administration;
using Xunit;

namespace EnglishCenter.Tests.Api.Integration;

internal static class OwnedAuditAssertions
{
    public static void Security(
        AuditLog row, string action, int? actor, bool success, DateTime at, string? ip = null)
    {
        Assert.True(row is not null, "AuditLog row must not be null.");
        Assert.True(row.AuditId > 0, "AuditId must be positive.");
        Assert.True(row.OperationId != Guid.Empty, "OperationId must not be empty.");
        Assert.True(row.ActorAccountId == actor, "ActorAccountId mismatch.");
        Assert.True(row.Action == action, "Action mismatch.");
        Assert.True(row.EntityType == "Account", "EntityType must be Account.");
        Assert.True(row.EntityId == (actor.HasValue ? actor.Value.ToString(CultureInfo.InvariantCulture) : null), "EntityId mismatch.");
        Assert.True(row.Result == (success ? "SUCCESS" : "FAILURE"), "Result mismatch.");
        Assert.True(row.AuditCategory == "SECURITY", "AuditCategory must be SECURITY.");
        Assert.True(row.Details is null, "Details must be null.");
        Assert.True(row.OccurredAt == at, "OccurredAt mismatch.");
        Assert.True(row.SecurityExpiresAt == at.AddDays(90), "SecurityExpiresAt mismatch.");
        Assert.True(row.IpAddress == ip, "IpAddress mismatch.");
        Assert.True(row.IpExpiresAt == (ip is null ? null : at.AddDays(90)), "IpExpiresAt mismatch.");
    }

    public static void PasswordChange(AuditLog row, int actor, long previous, int revoked, bool temp)
    {
        Assert.True(row is not null, "AuditLog row must not be null.");
        Assert.True(row.AuditId > 0, "AuditId must be positive.");
        Assert.True(row.OperationId != Guid.Empty, "OperationId must not be empty.");
        Assert.True(row.ActorAccountId == actor, "ActorAccountId mismatch.");
        Assert.True(row.EntityId == actor.ToString(CultureInfo.InvariantCulture), "EntityId mismatch.");
        Assert.True(row.Action == "AUTH.PASSWORD_CHANGE", "Action must be AUTH.PASSWORD_CHANGE.");
        Assert.True(row.EntityType == "Account", "EntityType must be Account.");
        Assert.True(row.Result == "SUCCESS", "Result must be SUCCESS.");
        Assert.True(row.AuditCategory == "MANDATORY", "AuditCategory must be MANDATORY.");
        Assert.True(row.SecurityExpiresAt is null, "SecurityExpiresAt must be null.");
        Assert.True(row.IpExpiresAt is null, "IpExpiresAt must be null.");
        Assert.True(row.IpAddress is null, "IpAddress must be null.");
        Assert.True(row.Details is not null, "Details must not be null.");

        JsonDocument doc;
        try
        {
            doc = JsonDocument.Parse(row.Details!);
        }
        catch (JsonException)
        {
            Assert.Fail("Details must be valid JSON.");
            return;
        }

        using (doc)
        {
            var root = doc.RootElement;
            Assert.True(root.ValueKind == JsonValueKind.Object && root.EnumerateObject().Count() == 2, "Root must contain exactly two properties.");
            Assert.True(root.TryGetProperty("old", out var oldEl) && oldEl.ValueKind == JsonValueKind.Object, "old object missing.");
            Assert.True(root.TryGetProperty("new", out var newEl) && newEl.ValueKind == JsonValueKind.Object, "new object missing.");

            Assert.True(oldEl.EnumerateObject().Count() == 2, "old must contain exactly two properties.");
            Assert.True(oldEl.TryGetProperty("mustChangePassword", out var oldMust) && oldMust.ValueKind == (temp ? JsonValueKind.True : JsonValueKind.False), "old.mustChangePassword mismatch.");
            Assert.True(oldEl.TryGetProperty("securityVersion", out var oldVer) && oldVer.ValueKind == JsonValueKind.Number && oldVer.TryGetInt64(out var oldValue) && oldValue == previous, "old.securityVersion mismatch.");

            Assert.True(newEl.EnumerateObject().Count() == 3, "new must contain exactly three properties.");
            Assert.True(newEl.TryGetProperty("mustChangePassword", out var newMust) && newMust.ValueKind == JsonValueKind.False, "new.mustChangePassword must be false.");
            Assert.True(newEl.TryGetProperty("securityVersion", out var newVer) && newVer.ValueKind == JsonValueKind.Number && newVer.TryGetInt64(out var newValue) && newValue == previous + 1, "new.securityVersion mismatch.");
            Assert.True(newEl.TryGetProperty("revokedSessionCount", out var newRev) && newRev.ValueKind == JsonValueKind.Number && newRev.TryGetInt32(out var revokedValue) && revokedValue == revoked, "new.revokedSessionCount mismatch.");
        }
    }
}
