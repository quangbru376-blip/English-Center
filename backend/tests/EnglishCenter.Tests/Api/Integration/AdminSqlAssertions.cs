using System.Text.Json;
using Xunit;

namespace EnglishCenter.Tests.Api.Integration;

internal static class AdminSqlAssertions
{
    private static readonly string[] ExpectedActions =
        ["DISABLE", "LOCK", "RESET_PASSWORD", "RESTORE", "UNLOCK"];

    private static readonly string[] ExpectedDetailProps =
    [
        "allowedActions", "createdAt", "disabledReasons", "email",
        "fullName", "id", "mustChangePassword", "phone",
        "role", "status", "temporaryPasswordExpiresAt", "version"
    ];

    private static readonly string[] ExpectedCompletionProps =
        ["operationId", "outcome", "resultMode", "target"];

    internal static void Detail(JsonElement body, int id, string role, string status)
    {
        Assert.Equal(JsonValueKind.Object, body.ValueKind);
        var props = body.EnumerateObject().Select(p => p.Name).OrderBy(x => x).ToList();
        Assert.Equal(ExpectedDetailProps, props);

        Assert.Equal(id, body.GetProperty("id").GetInt32());
        Assert.Equal(role, body.GetProperty("role").GetString());
        Assert.Equal(status, body.GetProperty("status").GetString());

        Assert.False(string.IsNullOrWhiteSpace(body.GetProperty("fullName").GetString()));
        Assert.False(string.IsNullOrWhiteSpace(body.GetProperty("email").GetString()));

        var phone = body.GetProperty("phone");
        Assert.True(phone.ValueKind is JsonValueKind.Null or JsonValueKind.String);

        var mustChange = body.GetProperty("mustChangePassword");
        Assert.True(mustChange.ValueKind is JsonValueKind.True or JsonValueKind.False);

        var createdAt = body.GetProperty("createdAt");
        Assert.True(Utc(createdAt));

        var tempExpiry = body.GetProperty("temporaryPasswordExpiresAt");
        Assert.True(tempExpiry.ValueKind == JsonValueKind.Null ||
            Utc(tempExpiry));

        var versionStr = body.GetProperty("version").GetString();
        Assert.NotNull(versionStr);
        var versionBytes = Convert.FromBase64String(versionStr);
        Assert.Equal(8, versionBytes.Length);
        Assert.Equal(Convert.ToBase64String(versionBytes), versionStr);

        var allowedProp = body.GetProperty("allowedActions");
        Assert.Equal(JsonValueKind.Array, allowedProp.ValueKind);
        Assert.All(allowedProp.EnumerateArray(), x => Assert.Equal(JsonValueKind.String, x.ValueKind));
        var allowed = allowedProp.EnumerateArray().Select(x => x.GetString()!).ToList();
        Assert.Equal(allowed.Count, new HashSet<string>(allowed).Count);

        var disabledProp = body.GetProperty("disabledReasons");
        Assert.Equal(JsonValueKind.Object, disabledProp.ValueKind);
        var disabled = disabledProp.EnumerateObject().ToList();
        Assert.All(disabled, p =>
        {
            Assert.Equal(JsonValueKind.String, p.Value.ValueKind);
            Assert.False(string.IsNullOrWhiteSpace(p.Value.GetString()));
        });

        var combined = allowed.Concat(disabled.Select(p => p.Name)).OrderBy(x => x).ToList();
        Assert.Equal(ExpectedActions, combined);
    }

    internal static void Completion(JsonElement body, int target, Guid operationId)
    {
        Assert.Equal(JsonValueKind.Object, body.ValueKind);
        var props = body.EnumerateObject().Select(p => p.Name).OrderBy(x => x).ToList();
        Assert.Equal(ExpectedCompletionProps, props);

        Assert.Equal("COMPLETED", body.GetProperty("outcome").GetString());
        Assert.Equal("SAFE_REFERENCE", body.GetProperty("resultMode").GetString());
        Assert.Equal(operationId, body.GetProperty("operationId").GetGuid());
        var reference = body.GetProperty("target");
        Assert.Equal(JsonValueKind.Object, reference.ValueKind);
        Assert.Equal(new[] { "accountId" }, reference.EnumerateObject().Select(p => p.Name).ToArray());
        Assert.Equal(target, reference.GetProperty("accountId").GetInt32());
    }
    private static bool Utc(JsonElement value) => value.ValueKind == JsonValueKind.String && DateTimeOffset.TryParseExact(value.GetString(), "yyyy-MM-dd'T'HH:mm:ss.fff'Z'", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AssumeUniversal, out _);
}
