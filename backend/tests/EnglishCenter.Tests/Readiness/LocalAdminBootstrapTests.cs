using System.Text.Json;
using EnglishCenter.Api.Infrastructure.Operator;
using Xunit;

namespace EnglishCenter.Tests.Readiness;

public sealed class LocalAdminBootstrapTests
{
    private static string Envelope() => JsonSerializer.Serialize(new { databaseId = 12, databaseGuid = Guid.NewGuid().ToString("D"), verifyOnly = true, password = new string('x', 44) });
    [Fact]
    public async Task OperatorCannotBootstrapOrConsumeInput()
    {
        using var input = new RefusingReader();
        Assert.Equal(LocalAdminBootstrap.Outcome.Refused, await LocalAdminBootstrap.RunAsync(input, TestContext.Current.CancellationToken));
    }
    [Fact]
    public async Task CallerCancellationPrecedesIdentityAndInput()
    {
        using var source = new CancellationTokenSource(); source.Cancel();
        using var input = new RefusingReader();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => LocalAdminBootstrap.RunAsync(input, source.Token));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => LocalAdminBootstrap.ReadEnvelopeAsync(new StringReader(Envelope()), source.Token));
    }
    [Fact]
    public async Task ValidEnvelopeNeverFormatsSecret()
    {
        var result = await LocalAdminBootstrap.ReadEnvelopeAsync(new StringReader(Envelope()), TestContext.Current.CancellationToken);
        Assert.NotNull(result); Assert.True(result.VerifyOnly); Assert.Equal("Protected bootstrap envelope", result.ToString());
    }
    [Fact]
    public async Task RejectsDuplicateUnknownTrailingAndOversizedInput()
    {
        var valid = Envelope();
        foreach (var invalid in new[] { valid.Replace("\"verifyOnly\":true", "\"verifyOnly\":true,\"verifyOnly\":false"),
            valid[..^1] + ",\"extra\":1}", valid + "{}", new string(' ', 4097), "[]", "null" })
            Assert.Null(await LocalAdminBootstrap.ReadEnvelopeAsync(new StringReader(invalid), TestContext.Current.CancellationToken));
    }
    [Theory]
    [InlineData("databaseId", "0")][InlineData("databaseGuid", "\"not-guid\"")]
    [InlineData("verifyOnly", "1")][InlineData("password", "\"short\"")]
    public async Task RejectsInvalidCatalogAndSecretTypes(string field, string value)
    {
        using var json = JsonDocument.Parse(Envelope());
        var invalid = "{" + string.Join(",", json.RootElement.EnumerateObject().Select(item => JsonSerializer.Serialize(item.Name) + ":" + (item.Name == field ? value : item.Value.GetRawText()))) + "}";
        Assert.Null(await LocalAdminBootstrap.ReadEnvelopeAsync(new StringReader(invalid), TestContext.Current.CancellationToken));
    }
    private sealed class RefusingReader : TextReader
    {
        public override ValueTask<int> ReadAsync(Memory<char> buffer, CancellationToken cancellationToken = default) => throw new InvalidOperationException("Input must remain untouched");
    }
}