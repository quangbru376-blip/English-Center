using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using EnglishCenter.Api.Infrastructure.Readiness;
using Xunit;

namespace EnglishCenter.Tests.Readiness;

public sealed class RuntimeProfileReaderTests
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;
    private static JsonSerializerOptions Options()
    {
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        options.Converters.Add(new JsonStringEnumConverter<RuntimeProfileKind>(allowIntegerValues: false));
        return options;
    }
    private static string Json(RuntimeProfile? profile = null) => JsonSerializer.Serialize(profile ?? ProfileFixture.Valid, Options());
    private static RuntimeProfileReader Reader(ProbeFiles files, string json, int maximumBytes = 100_000)
    {
        var path = Path.Combine(files.Root, "profile.json");
        File.WriteAllText(path, json, new UTF8Encoding(false));
        return new(path, maximumBytes, 1000);
    }
    [Fact]
    public async Task ExplicitReferenceOnlyFileLoadsTypedDescriptorsAndIsNeverReadyEvidence()
    {
        using var files = new ProbeFiles(); var json = Json();
        var result = await Reader(files, json).LoadAsync(Ct);
        Assert.True(result.IsValid); Assert.NotNull(result.Profile);
        Assert.Equal(Json(), Json(result.Profile));
        Assert.Equal("profile_valid", result.ToString());
        Assert.Equal(ProfileIssue.None, result.Issue);
    }
    [Theory]
    [InlineData("unknownRoot")] [InlineData("unknownNested")] [InlineData("numberString")]
    [InlineData("integerEnum")] [InlineData("caseEnum")] [InlineData("nullEnum")]
    [InlineData("duplicateRoot")] [InlineData("duplicateNested")] [InlineData("trailing")]
    [InlineData("comment")] [InlineData("arrayRoot")] [InlineData("nullRoot")]
    public async Task StrictShapeAndScalarViolationsAreMalformed(string change)
    {
        using var files = new ProbeFiles(); var node = JsonNode.Parse(Json())!.AsObject();
        if (change == "unknownRoot") node["password"] = "synthetic-invalid-field";
        if (change == "unknownNested") node["runtime"]!["unknown"] = true;
        if (change == "numberString") node["execution"]!["sqlSeconds"] = "1";
        if (change == "integerEnum") node["kind"] = 1;
        if (change == "caseEnum") node["kind"] = "local_dev";
        if (change == "nullEnum") node["kind"] = null;
        var text = node.ToJsonString();
        if (change == "duplicateRoot") text = text.Replace("\"kind\":\"LOCAL_DEV\"", "\"kind\":\"LOCAL_DEV\",\"kind\":\"LOCAL_DEV\"");
        if (change == "duplicateNested") text = text.Replace("\"sqlSeconds\":1", "\"sqlSeconds\":1,\"sqlSeconds\":1");
        if (change == "trailing") text = text[..^1] + ",}";
        if (change == "comment") text = "/* invalid */" + text;
        if (change == "arrayRoot") text = "[" + text + "]";
        if (change == "nullRoot") text = "null";
        var result = await Reader(files, text).LoadAsync(Ct);
        Assert.Equal(ProfileLoadFailure.Malformed, result.Failure); Assert.Null(result.Profile);
        Assert.Equal("profile_invalid", result.ToString());
    }
    [Theory]
    [InlineData("missing")] [InlineData("zeroBudget")] [InlineData("http")] [InlineData("noReference")]
    public async Task StructurallyTypedButIncompleteProfilesFailWithoutDefaults(string change)
    {
        using var files = new ProbeFiles(); var profile = ProfileFixture.Valid;
        profile = change switch
        {
            "missing" => profile with { Auth = null },
            "zeroBudget" => profile with { Execution = profile.Execution! with { LockMs = 0 } },
            "http" => profile with { Web = profile.Web! with { Origin = "http://localhost" } },
            _ => profile with { Database = profile.Database! with { ConnectionSecretReference = null } }
        };
        var result = await Reader(files, Json(profile)).LoadAsync(Ct);
        Assert.Equal(ProfileLoadFailure.InvalidProfile, result.Failure);
        Assert.NotEqual(ProfileIssue.None, result.Issue); Assert.Null(result.Profile);
    }
    [Fact]
    public async Task ChangedFileRechecksAndMissingFileHasNoCachedProfile()
    {
        using var files = new ProbeFiles(); var reader = Reader(files, Json());
        Assert.True((await reader.LoadAsync(Ct)).IsValid);
        File.WriteAllText(Path.Combine(files.Root, "profile.json"), "{}");
        Assert.False((await reader.LoadAsync(Ct)).IsValid);
        File.Delete(Path.Combine(files.Root, "profile.json"));
        Assert.Equal(ProfileLoadFailure.Unavailable, (await reader.LoadAsync(Ct)).Failure);
    }
    [Fact]
    public async Task ExplicitByteBoundAndUtf8AreEnforcedBeforeDeserialization()
    {
        using var files = new ProbeFiles();
        Assert.Equal(ProfileLoadFailure.Malformed, (await Reader(files, Json(), maximumBytes: 1).LoadAsync(Ct)).Failure);
        var reader = Reader(files, Json());
        File.WriteAllBytes(Path.Combine(files.Root, "profile.json"), [0xff, 0xff]);
        Assert.Equal(ProfileLoadFailure.Malformed, (await reader.LoadAsync(Ct)).Failure);
    }
    [Theory]
    [InlineData("relative", 1000, 1000)] [InlineData("", 1000, 1000)]
    [InlineData("relative", 0, 1000)] [InlineData("relative", 1000, 0)]
    public async Task NoAmbientPathOrInfiniteLimitIsSelected(string path, int bytes, int deadline)
    {
        var result = await new RuntimeProfileReader(path, bytes, deadline).LoadAsync(Ct);
        Assert.Equal(ProfileLoadFailure.InvalidSource, result.Failure);
    }
    [Fact]
    public async Task CallerCancellationPropagatesBeforeFileReads()
    {
        using var files = new ProbeFiles(); using var cancel = new CancellationTokenSource(); cancel.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => Reader(files, Json()).LoadAsync(cancel.Token));
    }
}
