using System.Globalization;
using EnglishCenter.Api.Common.Serialization;
using Xunit;

namespace EnglishCenter.Tests.Common.Serialization;

public class ExactNumbersTests
{
    [Fact]
    public void MoneyBoundsAndLeadingZerosRemainExact()
    {
        Assert.Equal(1m, ExactNumbers.ParseMoney("1"));
        Assert.Equal(100_000_000_000_000_000m, ExactNumbers.ParseMoney("000100000000000000000"));
        Assert.Equal(999_999_999_999_999_999m, ExactNumbers.ParseMoney("999999999999999999"));
        Assert.Equal("1", ExactNumbers.FormatMoney(ExactNumbers.ParseMoney(new string('0', 100) + "1")));
        Assert.Equal("999999999999999999", ExactNumbers.FormatMoney(999_999_999_999_999_999m));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("0")]
    [InlineData("00")]
    [InlineData("-1")]
    [InlineData("+1")]
    [InlineData("1.0")]
    [InlineData("1,000")]
    [InlineData("1e2")]
    [InlineData(" 1")]
    [InlineData("1 ")]
    [InlineData("１００")]
    [InlineData("1000000000000000000")]
    public void MoneyRejectsInvalidGrammarAndRange(string? input) =>
        Assert.ThrowsAny<ArgumentException>(() => ExactNumbers.ParseMoney(input));

    [Theory]
    [InlineData("0", "0.00")]
    [InlineData("0.01", "0.01")]
    [InlineData("80", "80.00")]
    [InlineData("80.5", "80.50")]
    [InlineData("80.50", "80.50")]
    [InlineData("100.00", "100.00")]
    [InlineData("00080.50", "80.50")]
    public void ScoreParsesExactlyAndFormatsCanonical(string input, string expected)
    {
        Assert.Equal(decimal.Parse(expected, CultureInfo.InvariantCulture), ExactNumbers.ParseScore(input));
        Assert.Equal(expected, ExactNumbers.FormatScore(ExactNumbers.ParseScore(input)));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(".5")]
    [InlineData("1.")]
    [InlineData("1.000")]
    [InlineData("80.555")]
    [InlineData("-1")]
    [InlineData("-0")]
    [InlineData("+1")]
    [InlineData("1e2")]
    [InlineData("1,2")]
    [InlineData(" 1")]
    [InlineData("1 ")]
    [InlineData("１")]
    [InlineData("1.2.3")]
    [InlineData("100.01")]
    [InlineData("101")]
    public void ScoreAndWeightRejectMalformedOrOutOfRangeInput(string? input)
    {
        Assert.ThrowsAny<ArgumentException>(() => ExactNumbers.ParseScore(input));
        Assert.ThrowsAny<ArgumentException>(() => ExactNumbers.ParseWeight(input));
    }

    [Fact]
    public void WeightIsPositiveAndExactAtBounds()
    {
        Assert.Equal(0.01m, ExactNumbers.ParseWeight("0.01"));
        Assert.Equal(100m, ExactNumbers.ParseWeight("100.00"));
        Assert.Equal("80.50", ExactNumbers.FormatWeight(ExactNumbers.ParseWeight("00080.5")));
        Assert.ThrowsAny<ArgumentException>(() => ExactNumbers.ParseWeight("0.00"));
    }

    [Fact]
    public void FormattersRejectRoundingAndRangeViolations()
    {
        Assert.Equal("1.00", ExactNumbers.FormatScore(1.000m));
        Assert.Equal("1.00", ExactNumbers.FormatWeight(1.000m));
        foreach (decimal invalidMoney in new[] { 0m, -1m, 1.01m, 1_000_000_000_000_000_000m })
            Assert.ThrowsAny<ArgumentException>(() => ExactNumbers.FormatMoney(invalidMoney));
        foreach (decimal invalidGrade in new[] { -0.01m, 100.01m, 80.125m, 0.001m })
        {
            Assert.ThrowsAny<ArgumentException>(() => ExactNumbers.FormatScore(invalidGrade));
            Assert.ThrowsAny<ArgumentException>(() => ExactNumbers.FormatWeight(invalidGrade));
        }
        Assert.ThrowsAny<ArgumentException>(() => ExactNumbers.FormatWeight(0m));
    }

    [Fact]
    public void ParsingAndFormattingAreInvariantAndErrorsDoNotEchoInput()
    {
        CultureInfo previous = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR");
            Assert.Equal("80.50", ExactNumbers.FormatScore(ExactNumbers.ParseScore("80.5")));
            Assert.Equal("123", ExactNumbers.FormatMoney(ExactNumbers.ParseMoney("00123")));
            const string invalid = "PRIVATE_VALUE";
            Assert.DoesNotContain(invalid, Assert.ThrowsAny<ArgumentException>(() => ExactNumbers.ParseMoney(invalid)).Message);
            Assert.DoesNotContain(invalid, Assert.ThrowsAny<ArgumentException>(() => ExactNumbers.ParseWeight(invalid)).Message);
        }
        finally { CultureInfo.CurrentCulture = previous; }
    }
}
