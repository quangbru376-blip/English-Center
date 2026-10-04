using System.Globalization;
using System.Numerics;
using EnglishCenter.Api.Common.Serialization;
using Xunit;

namespace EnglishCenter.Tests.Common.Serialization;

public class ReportAmountCodecTests
{
    [Theory]
    [InlineData("0", "0")]
    [InlineData("1", "1")]
    [InlineData("79228162514264337593543950335", "79228162514264337593543950335")]
    [InlineData("79228162514264337593543950336", "79228162514264337593543950336")]
    [InlineData("10000000000000000000000000000000000000", "10000000000000000000000000000000000000")]
    [InlineData("99999999999999999999999999999999999999", "99999999999999999999999999999999999999")]
    public void Roundtrip_ValidAmounts_Exact(string input, string expected)
    {
        var parsed = ReportAmountCodec.Parse(input);
        var formatted = ReportAmountCodec.Format(parsed);
        Assert.Equal(expected, formatted);
    }

    [Fact]
    public void Parse_DecimalMaxValue_MatchesDecimalLiteral()
    {
        var expected = (BigInteger)decimal.MaxValue;
        var parsed = ReportAmountCodec.Parse(decimal.MaxValue.ToString(CultureInfo.InvariantCulture));
        Assert.Equal(expected, parsed);
    }

    [Fact]
    public void Parse_MaxAmount_MatchesConstant()
    {
        var parsed = ReportAmountCodec.Parse("99999999999999999999999999999999999999");
        Assert.Equal(ReportAmountCodec.MaxValue, parsed);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("00")]
    [InlineData("01")]
    [InlineData("00000000000000000000000000000000000000")]
    [InlineData("-0")]
    [InlineData("-1")]
    [InlineData("+1")]
    [InlineData("1.0")]
    [InlineData("1,000")]
    [InlineData(" 1")]
    [InlineData("1 ")]
    [InlineData("1e5")]
    [InlineData("1a")]
    [InlineData("100000000000000000000000000000000000000")]
    [InlineData("\uFF10")]
    [InlineData("\uFF11")]
    [InlineData("\u0660")]
    public void Parse_InvalidValues_ThrowsArgumentException(string? input)
    {
        Assert.ThrowsAny<ArgumentException>(() => ReportAmountCodec.Parse(input));
    }

    [Fact]
    public void Format_Negative_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => ReportAmountCodec.Format(BigInteger.MinusOne));
        Assert.Throws<ArgumentOutOfRangeException>(() => ReportAmountCodec.Format(new BigInteger(-100)));
    }

    [Fact]
    public void Format_ExceedsMaxValue_ThrowsArgumentOutOfRangeException()
    {
        var aboveMax = ReportAmountCodec.MaxValue + 1;
        Assert.Throws<ArgumentOutOfRangeException>(() => ReportAmountCodec.Format(aboveMax));
    }

    [Theory]
    [InlineData("vi-VN")]
    [InlineData("fr-FR")]
    [InlineData("ar-SA")]
    public void Parse_And_Format_AreCultureInvariant(string cultureName)
    {
        var prev = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo(cultureName);
            const string literal = "99999999999999999999999999999999999999";
            var parsed = ReportAmountCodec.Parse(literal);
            Assert.Equal(ReportAmountCodec.MaxValue, parsed);
            Assert.Equal(literal, ReportAmountCodec.Format(parsed));
        }
        finally
        {
            CultureInfo.CurrentCulture = prev;
        }
    }
}
