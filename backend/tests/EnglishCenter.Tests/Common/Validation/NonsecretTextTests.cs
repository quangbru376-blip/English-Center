using System.Text;
using EnglishCenter.Api.Common.Validation;
using Xunit;

namespace EnglishCenter.Tests.Common.Validation;

public class NonsecretTextTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void NormalizeRequired_NonpositiveMax_ThrowsArgumentOutOfRangeException(int max)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => NonsecretText.NormalizeRequired("test", max));
        Assert.Throws<ArgumentOutOfRangeException>(() => NonsecretText.NormalizeOptional("test", max));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t\r\n")]
    public void NormalizeRequired_NullOrBlank_ThrowsArgumentException(string? input)
    {
        Assert.Throws<ArgumentException>(() => NonsecretText.NormalizeRequired(input, 100));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t\r\n")]
    public void NormalizeOptional_NullOrBlank_ReturnsNull(string? input)
    {
        Assert.Null(NonsecretText.NormalizeOptional(input, 100));
    }

    [Fact]
    public void Normalize_TrimsEdges_PreservesInternalWhitespacePunctuationAndCase()
    {
        string input = "  Hello,   World!  ";
        string expected = "Hello,   World!";

        Assert.Equal(expected, NonsecretText.NormalizeRequired(input, 100));
        Assert.Equal(expected, NonsecretText.NormalizeOptional(input, 100));
    }

    [Fact]
    public void Normalize_ComposedAndDecomposedNfc_ProducesEqualResult()
    {
        string composed = "Ti\u1EBFng Vi\u1EC7t";
        string decomposed = "Ti\u0065\u0302\u0301ng Vi\u0065\u0323\u0302t";

        string normalizedComposed = NonsecretText.NormalizeRequired(composed, 100);
        string normalizedDecomposed = NonsecretText.NormalizeRequired(decomposed, 100);

        Assert.Equal(normalizedComposed, normalizedDecomposed);
        Assert.True(normalizedDecomposed.IsNormalized(NormalizationForm.FormC));
        Assert.Equal("é", NonsecretText.NormalizeRequired("e\u0301", 1));
    }

    [Fact]
    public void Normalize_SupplementaryRune_CountsCodePointsNotUtf16Units()
    {
        string singleEmoji = "\U0001F600";
        string twoEmojis = "\U0001F600\U0001F601";

        string result = NonsecretText.NormalizeRequired(singleEmoji, 1);
        Assert.Equal(singleEmoji, result);

        Assert.Throws<ArgumentException>(() => NonsecretText.NormalizeRequired(twoEmojis, 1));
        Assert.Throws<ArgumentException>(() => NonsecretText.NormalizeOptional(twoEmojis, 1));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public void Normalize_UnpairedSurrogates_ThrowsArgumentException(int scenario)
    {
        string[] cases = ["\uD83D", "\uDE00", "\uD83D hello", "hello \uDE00", "\uD83D\uD83D", "\uDE00\uD83D"];
        string invalidUnicode = cases[scenario];
        Assert.Throws<ArgumentException>(() => NonsecretText.NormalizeRequired(invalidUnicode, 100));
        Assert.Throws<ArgumentException>(() => NonsecretText.NormalizeOptional(invalidUnicode, 100));
    }

    [Fact]
    public void Normalize_ConsumerLimits_ExactBoundaries()
    {
        string name100 = new('a', 100);
        string name101 = new('a', 101);
        Assert.Equal(100, NonsecretText.NormalizeRequired(name100, 100).Length);
        Assert.Throws<ArgumentException>(() => NonsecretText.NormalizeRequired(name101, 100));

        Assert.Equal("a", NonsecretText.NormalizeRequired(" a ", 1));

        string addr255 = new('b', 255);
        string addr256 = new('b', 256);
        Assert.Equal(255, NonsecretText.NormalizeOptional(addr255, 255)!.Length);
        Assert.Throws<ArgumentException>(() => NonsecretText.NormalizeOptional(addr256, 255));

        Assert.Equal(150, NonsecretText.NormalizeRequired(new string('c', 150), 150).Length);
        Assert.Throws<ArgumentException>(() => NonsecretText.NormalizeRequired(new string('c', 151), 150));

        Assert.Equal(1000, NonsecretText.NormalizeRequired(new string('d', 1000), 1000).Length);
        Assert.Throws<ArgumentException>(() => NonsecretText.NormalizeRequired(new string('d', 1001), 1000));
    }
}
