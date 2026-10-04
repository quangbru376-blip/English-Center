using System.Text.Json;
using EnglishCenter.Api.Common.Serialization;
using Xunit;

namespace EnglishCenter.Tests.Common.Serialization;

public class StrictScalarsTests
{
    [Fact]
    public void NormalizeUuid_UppercaseAndMixedCase_ReturnsNormalizedLowercaseDString()
    {
        var upper = "D3B07384-D113-46D4-A78B-AC36E37C2F6E";
        var lower = "d3b07384-d113-46d4-a78b-ac36e37c2f6e";
        Assert.Equal(lower, StrictScalars.NormalizeUuid(upper));
        Assert.Equal(lower, StrictScalars.NormalizeUuid(lower));
    }

    [Fact]
    public void NormalizeUuid_GuidEmpty_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => StrictScalars.NormalizeUuid("00000000-0000-0000-0000-000000000000"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("d3b07384-d113-46d4-a78b-ac36e37c2f6")]
    [InlineData("d3b07384-d113-46d4-a78b-ac36e37c2f6ee")]
    [InlineData("{d3b07384-d113-46d4-a78b-ac36e37c2f6e}")]
    [InlineData("urn:uuid:d3b07384-d113-46d4-a78b-ac36e37c2f6e")]
    [InlineData("d3b07384-d113-46d4-a78b-ac36e37c2f6ｅ")]
    [InlineData("d3b07384d11346d4a78bac36e37c2f6e")]
    [InlineData("d3b0738-4d113-46d4-a78b-ac36e37c2f6e")]
    [InlineData("d3b07384-d113-46d4-a78b-ac36e37c2f6g")]
    [InlineData(" d3b07384-d113-46d4-a78b-ac36e37c2f6e")]
    [InlineData("d3b07384-d113-46d4-a78b-ac36e37c2f6e ")]
    public void NormalizeUuid_InvalidShapes_ThrowsArgumentException(string? input)
    {
        Assert.ThrowsAny<ArgumentException>(() => StrictScalars.NormalizeUuid(input));
    }

    [Fact]
    public void NormalizeUuidHeader_SingleValidValue_ReturnsNormalizedUuid()
    {
        var result = StrictScalars.NormalizeUuidHeader(new[] { "D3B07384-D113-46D4-A78B-AC36E37C2F6E" });
        Assert.Equal("d3b07384-d113-46d4-a78b-ac36e37c2f6e", result);
    }

    [Fact]
    public void NormalizeUuidHeader_InvalidInputs_ThrowsArgumentException()
    {
        Assert.ThrowsAny<ArgumentException>(() => StrictScalars.NormalizeUuidHeader(null));
        Assert.ThrowsAny<ArgumentException>(() => StrictScalars.NormalizeUuidHeader(Array.Empty<string>()));
        Assert.ThrowsAny<ArgumentException>(() => StrictScalars.NormalizeUuidHeader(new[]
        {
            "d3b07384-d113-46d4-a78b-ac36e37c2f6e",
            "a1b2c3d4-e5f6-7a8b-9c0d-1e2f3a4b5c6d"
        }));
        Assert.ThrowsAny<ArgumentException>(() => StrictScalars.NormalizeUuidHeader(new[] { "invalid-uuid" }));
    }

    [Theory]
    [InlineData("2024-02-29", 2024, 2, 29)]
    [InlineData("2000-02-29", 2000, 2, 29)]
    [InlineData("2026-10-04", 2026, 10, 4)]
    [InlineData("0001-01-01", 1, 1, 1)]
    [InlineData("9999-12-31", 9999, 12, 31)]
    public void ParseLocalDate_ValidCalendarDates_ReturnsDateOnly(string input, int year, int month, int day)
    {
        Assert.Equal(new DateOnly(year, month, day), StrictScalars.ParseLocalDate(input));
    }

    [Theory]
    [InlineData("2023-02-29")]
    [InlineData("1900-02-29")]
    [InlineData("0000-01-01")]
    [InlineData("2024-04-31")]
    [InlineData("2024-00-15")]
    [InlineData("2024-13-15")]
    [InlineData("2024-05-00")]
    [InlineData("2024-05-32")]
    public void ParseLocalDate_InvalidCalendarDates_ThrowsArgumentException(string input)
    {
        Assert.Throws<ArgumentException>(() => StrictScalars.ParseLocalDate(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" 2024-02-29")]
    [InlineData("2024-02-29 ")]
    [InlineData("2024/02/29")]
    [InlineData("2024-2-29")]
    [InlineData("2024-02-29T00:00:00")]
    [InlineData("2024-02-29Z")]
    [InlineData("۲۰۲۴-۰۲-۲۹")]
    public void ParseLocalDate_InvalidFormatWhitespaceAndUnicode_ThrowsArgumentException(string? input)
    {
        Assert.ThrowsAny<ArgumentException>(() => StrictScalars.ParseLocalDate(input));
    }

    [Theory]
    [InlineData("00:00", 0, 0)]
    [InlineData("23:59", 23, 59)]
    [InlineData("08:30", 8, 30)]
    public void ParseMinuteTime_ValidMinuteTimes_ReturnsTimeOnly(string input, int hour, int minute)
    {
        Assert.Equal(new TimeOnly(hour, minute), StrictScalars.ParseMinuteTime(input));
    }

    [Theory]
    [InlineData("24:00")]
    [InlineData("12:60")]
    [InlineData("25:00")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" 08:30")]
    [InlineData("08:30 ")]
    [InlineData("8:30")]
    [InlineData("08:30:00")]
    [InlineData("08:30Z")]
    [InlineData("۰۸:۳۰")]
    public void ParseMinuteTime_InvalidValuesAndFormats_ThrowsArgumentException(string? input)
    {
        Assert.ThrowsAny<ArgumentException>(() => StrictScalars.ParseMinuteTime(input));
    }

    [Theory]
    [InlineData("42", 0, 100, 42)]
    [InlineData("2147483647", int.MinValue, int.MaxValue, int.MaxValue)]
    [InlineData("-2147483648", int.MinValue, int.MaxValue, int.MinValue)]
    [InlineData("-10", -20, 0, -10)]
    [InlineData("-0", 0, 10, 0)]
    [InlineData("0", 0, 10, 0)]
    [InlineData("10", 0, 10, 10)]
    [InlineData("5", 5, 5, 5)]
    public void ParseInt32_ValidIntegersAndEndpoints_ReturnsExpected(string jsonNumber, int min, int max, int expected)
    {
        using var doc = JsonDocument.Parse(jsonNumber);
        Assert.Equal(expected, StrictScalars.ParseInt32(doc.RootElement, min, max));
    }

    [Theory]
    [InlineData("11", 0, 10)]
    [InlineData("-1", 0, 10)]
    public void ParseInt32_OutOfRange_ThrowsArgumentOutOfRangeException(string jsonNumber, int min, int max)
    {
        using var doc = JsonDocument.Parse(jsonNumber);
        Assert.Throws<ArgumentOutOfRangeException>(() => StrictScalars.ParseInt32(doc.RootElement, min, max));
    }

    [Theory]
    [InlineData("\"42\"")]
    [InlineData("true")]
    [InlineData("null")]
    [InlineData("{}")]
    [InlineData("[]")]
    public void ParseInt32_InvalidType_ThrowsArgumentException(string rawJson)
    {
        using var doc = JsonDocument.Parse(rawJson);
        Assert.Throws<ArgumentException>(() => StrictScalars.ParseInt32(doc.RootElement, 0, 100));
    }

    [Theory]
    [InlineData("12.3")]
    [InlineData("1e5")]
    [InlineData("12.0")]
    public void ParseInt32_InvalidLexeme_ThrowsArgumentException(string rawJson)
    {
        using var doc = JsonDocument.Parse(rawJson);
        Assert.Throws<ArgumentException>(() => StrictScalars.ParseInt32(doc.RootElement, 0, 100));
    }

    [Theory]
    [InlineData("2147483648")]
    [InlineData("-2147483649")]
    [InlineData("99999999999999999999")]
    public void ParseInt32_Overflow_ThrowsArgumentOutOfRangeException(string rawJson)
    {
        using var doc = JsonDocument.Parse(rawJson);
        Assert.Throws<ArgumentOutOfRangeException>(() => StrictScalars.ParseInt32(doc.RootElement, 0, 100));
    }

    [Fact]
    public void ParseInt32_InvalidConfig_ThrowsArgumentException()
    {
        using var doc = JsonDocument.Parse("5");
        Assert.Throws<ArgumentException>(() => StrictScalars.ParseInt32(doc.RootElement, 10, 5));
    }

    [Fact]
    public void StrictScalars_Exceptions_NeverEchoInput()
    {
        var secret = "S3CR3T_TOKEN";
        Assert.DoesNotContain(secret, Assert.ThrowsAny<ArgumentException>(() => StrictScalars.NormalizeUuid(secret)).Message);
        Assert.DoesNotContain(secret, Assert.ThrowsAny<ArgumentException>(() => StrictScalars.ParseLocalDate(secret)).Message);
        Assert.DoesNotContain(secret, Assert.ThrowsAny<ArgumentException>(() => StrictScalars.ParseMinuteTime(secret)).Message);
    }
}
