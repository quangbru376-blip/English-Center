namespace EnglishCenter.Tests.Infrastructure;

using EnglishCenter.Api.Infrastructure.Compatibility;
using Xunit;

public class CatalogCompatibilityTests
{
    [Theory]
    [InlineData("column/dbo/Account/status")] [InlineData("fk/dbo/Score/component")]
    [InlineData("parameter/dbo/usp_SearchAdmissions/1")] [InlineData("permission/ec_app_runtime/SELECT")]
    public void FullManifestComparisonRejectsMissingExtraAndChangedShape(string key)
    {
        var expected = new Dictionary<string, string> { [key] = "exact", ["another"] = "shape" };
        Assert.True(CatalogCompatibility.Matches(expected, new Dictionary<string, string> { ["another"] = "shape", [key] = "exact" }));
        Assert.False(CatalogCompatibility.Matches(expected, new Dictionary<string, string> { [key] = "exact" }));
        Assert.False(CatalogCompatibility.Matches(expected, new Dictionary<string, string> { [key] = "different", ["another"] = "shape" }));
        Assert.False(CatalogCompatibility.Matches(expected, new Dictionary<string, string> { [key] = "exact", ["unrelated"] = "shape" }));
    }

    [Fact]
    public void ConfiguredInsensitiveDictionaryNeverWeakensOrdinalContract()
    {
        var expected = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { ["Account"] = "varchar" };
        var actual = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { ["account"] = "varchar" };
        Assert.False(CatalogCompatibility.Matches(expected, actual));
        Assert.False(CatalogCompatibility.Matches(expected, new Dictionary<string, string> { ["Account"] = "VARCHAR" }));
    }

    [Theory]
    [InlineData("CREATE OR ALTER FUNCTION dbo.f() RETURNS int AS BEGIN RETURN 1; END", "CREATE   FUNCTION dbo.f() RETURNS int AS BEGIN RETURN 1; END")]
    [InlineData("ALTER VIEW dbo.v AS SELECT 1\r\n", "CREATE VIEW dbo.v AS SELECT 1\n")]
    public void EngineCreateHeaderAndNewlinesNormalizeOnlyHeader(string expected, string actual)
    {
        Assert.True(CatalogCompatibility.ModuleMatches(expected, actual));
        Assert.False(CatalogCompatibility.ModuleMatches(expected, actual.Replace("1", "2", StringComparison.Ordinal)));
        Assert.False(CatalogCompatibility.ModuleMatches(expected, actual.Replace("dbo", "DBO", StringComparison.Ordinal)));
    }

    [Theory]
    [InlineData("CREATE VIEW dbo.v AS SELECT 'ALTER  text';", "CREATE VIEW dbo.v AS SELECT 'ALTER text';")]
    [InlineData("CREATE VIEW dbo.v AS SELECT 1 -- keep", "CREATE VIEW dbo.v AS SELECT 1 -- changed")]
    [InlineData("CREATEX VIEW dbo.v AS SELECT 1", "CREATE VIEW dbo.v AS SELECT 1")]
    public void BodyLiteralCommentAndMalformedKeywordDriftIsNotNormalizedAway(string expected, string actual) =>
        Assert.False(CatalogCompatibility.ModuleMatches(expected, actual));
}
