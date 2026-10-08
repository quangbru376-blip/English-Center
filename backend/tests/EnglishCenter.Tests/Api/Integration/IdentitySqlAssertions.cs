using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using Xunit;

namespace EnglishCenter.Tests.Api.Integration;

internal static class IdentitySqlAssertions
{
    private static readonly HashSet<string> ExpectedProperties = new()
    {
        "accountId",
        "fullName",
        "email",
        "role",
        "scope",
        "accountVersion",
        "serverTime",
        "absoluteExpiresAt",
        "effectiveExpiresAt"
    };

    internal static void Context(JsonElement body, int id, string role, string scope)
    {
        Assert.Equal(JsonValueKind.Object, body.ValueKind);

        var actualNames = new HashSet<string>();
        int count = 0;
        foreach (var prop in body.EnumerateObject())
        {
            count++;
            Assert.True(actualNames.Add(prop.Name));
        }

        Assert.Equal(9, count);
        Assert.True(actualNames.SetEquals(ExpectedProperties));

        Assert.Equal(id, body.GetProperty("accountId").GetInt32());
        Assert.Equal(role, body.GetProperty("role").GetString());
        Assert.Equal(scope, body.GetProperty("scope").GetString());

        var fullName = body.GetProperty("fullName").GetString();
        Assert.False(string.IsNullOrWhiteSpace(fullName));

        var email = body.GetProperty("email").GetString();
        Assert.False(string.IsNullOrWhiteSpace(email));

        var accountVersion = body.GetProperty("accountVersion").GetString();
        Assert.NotNull(accountVersion);
        var versionBytes = Convert.FromBase64String(accountVersion);
        Assert.Equal(8, versionBytes.Length);
        Assert.Equal(Convert.ToBase64String(versionBytes), accountVersion);

        var serverTime = ParseInstant(body, "serverTime");
        var effectiveExpiresAt = ParseInstant(body, "effectiveExpiresAt");
        var absoluteExpiresAt = ParseInstant(body, "absoluteExpiresAt");

        Assert.True(effectiveExpiresAt <= absoluteExpiresAt);
        Assert.True(serverTime < effectiveExpiresAt);
    }

    private static DateTimeOffset ParseInstant(JsonElement body, string propertyName)
    {
        var raw = body.GetProperty(propertyName).GetString();
        Assert.NotNull(raw);
        Assert.EndsWith("Z", raw);
        return DateTimeOffset.ParseExact(raw, "yyyy-MM-dd'T'HH:mm:ss.fff'Z'", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
    }
}
