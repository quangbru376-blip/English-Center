using EnglishCenter.Api.Common.Serialization;
using Xunit;

namespace EnglishCenter.Tests.Common.Serialization;

public class RowVersionCodecTests
{
    [Fact]
    public void Roundtrip_AllZeros_EncodesAndDecodesCorrectly()
    {
        var zeros = new byte[8];
        var encoded = RowVersionCodec.Encode(zeros);
        Assert.Equal("AAAAAAAAAAA=", encoded);
        Assert.Equal(zeros, RowVersionCodec.Decode(encoded));
    }

    [Fact]
    public void Roundtrip_AllFF_EncodesAndDecodesCorrectly()
    {
        var allFf = new byte[] { 255, 255, 255, 255, 255, 255, 255, 255 };
        var encoded = RowVersionCodec.Encode(allFf);
        Assert.Equal("//////////8=", encoded);
        Assert.Equal(allFf, RowVersionCodec.Decode(encoded));
    }

    [Fact]
    public void Roundtrip_KnownArbitraryVector_PreservesByteOrder()
    {
        var original = new byte[] { 0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF };
        var encoded = RowVersionCodec.Encode(original);
        Assert.Equal("ASNFZ4mrze8=", encoded);
        Assert.Equal(original, RowVersionCodec.Decode(encoded));
    }

    [Fact]
    public void Decode_UnusedBitsSet_ThrowsArgumentException()
    {
        Assert.Equal(new byte[8], RowVersionCodec.Decode("AAAAAAAAAAA="));
        Assert.Throws<ArgumentException>(() => RowVersionCodec.Decode("AAAAAAAAAAB="));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("AAAAAAAAAAA")]     // 11 chars, unpadded
    [InlineData("AAAAAAAAAAAA=")]    // 13 chars
    [InlineData("AAAAAAAAAA==")]    // 12 chars, 2 padding chars (7 bytes)
    [InlineData("AAAAAAAAAAAA")]    // 12 chars, 0 padding chars (9 bytes)
    public void Decode_InvalidLengthOrPadding_ThrowsArgumentException(string? input)
    {
        Assert.ThrowsAny<ArgumentException>(() => RowVersionCodec.Decode(input));
    }

    [Theory]
    [InlineData("-_________8=")]
    [InlineData("AAAAAAAAAA-_")]    // URL-safe characters
    [InlineData("AAAAAAAAAA!@")]    // Non-Base64 symbols
    [InlineData(" AAAAAAAAAA=")]    // Leading whitespace
    [InlineData("AAAAAAAAAA= ")]    // Trailing whitespace
    [InlineData("AAAA AAAAAA=")]    // Internal whitespace
    public void Decode_InvalidCharacters_ThrowsArgumentException(string input)
    {
        Assert.Throws<ArgumentException>(() => RowVersionCodec.Decode(input));
    }

    [Fact]
    public void Encode_InvalidLength_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => RowVersionCodec.Encode(new byte[7]));
        Assert.Throws<ArgumentException>(() => RowVersionCodec.Encode(new byte[9]));
        Assert.Throws<ArgumentException>(() => RowVersionCodec.Encode(Array.Empty<byte>()));
        Assert.Throws<ArgumentException>(() => RowVersionCodec.Encode(default(ReadOnlySpan<byte>)));
    }

    [Fact]
    public void RowVersionCodec_Exceptions_NeverEchoInput()
    {
        var sensitive = "SECRET_TOKEN";
        var ex = Assert.ThrowsAny<ArgumentException>(() => RowVersionCodec.Decode(sensitive));
        Assert.DoesNotContain(sensitive, ex.Message);
    }
}
