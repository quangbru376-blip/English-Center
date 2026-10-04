namespace EnglishCenter.Tests.Common.Serialization;

using System.Text.Json;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Common.Serialization;
using EnglishCenter.Api.Common.Validation;
using Xunit;

public class StrictJsonObjectTests
{
    private static StrictJsonObject Parse(string? json) => StrictJsonObject.Parse(json,new[]{"name","number","flag","nullable","payload"},new[]{"name"});
    [Theory]
    [InlineData(null),InlineData(""),InlineData("{"),InlineData("null"),InlineData("[]"),InlineData("1")]
    [InlineData("{}"),InlineData("{\"Name\":\"x\"}"),InlineData("{\"name\":\"x\",\"extra\":1}")]
    [InlineData("{\"name\":\"x\",\"name\":\"y\"}"),InlineData("{\"name\":\"x\",\"na\\u006de\":\"y\"}")]
    [InlineData("{\"name\":\"x\",\"payload\":{\"a\":1,\"a\":2}}")]
    [InlineData("{\"name\":\"x\",\"payload\":[{\"a\":1,\"a\":2}]}")]
    [InlineData("{\"name\":\"x\",}"),InlineData("{/*secret*/\"name\":\"x\"}")]
    public void MalformedShapeAndDuplicates_AreSafe400(string? json)
    {
        var error=Assert.Throws<ApiErrorException>(()=>Parse(json));
        Assert.Equal(ApiErrorCode.VALIDATION_ERROR,error.ErrorCode);
        Assert.All(error.FieldErrors.Keys,key=>Assert.Equal("name",key));
    }
    [Fact]
    public void PresenceNullAndRawStrings_AreDistinct()
    {
        var dto=Parse("{\"name\":\"  é  \",\"nullable\":null,\"payload\":{\"x\":1,\"X\":2}}");
        Assert.Equal("  é  ",dto.ReadString("name"));
        Assert.True(dto.Contains("nullable"));
        Assert.False(dto.Contains("number"));
        Assert.Null(dto.ReadNullableString("nullable"));
        Assert.Null(dto.ReadNullableString("number"));
        Assert.Equal("",Parse("{\"name\":\"\"}").ReadString("name"));
        var nullRequired=Parse("{\"name\":null}");
        Assert.True(nullRequired.Contains("name"));
        Assert.Throws<ApiErrorException>(()=>nullRequired.ReadString("name"));
    }
    [Theory]
    [InlineData("15"),InlineData("true"),InlineData("null"),InlineData("{}"),InlineData("[]"),InlineData("\"\\ud800\"")]
    public void RequiredString_RejectsTypesAndInvalidUtf16(string value)
    {
        var dto=Parse("{\"name\":"+value+"}");
        var error=Assert.Throws<ApiErrorException>(()=>dto.ReadString("name"));
        Assert.True(error.FieldErrors.ContainsKey("name"));
    }
    [Theory]
    [InlineData("1.0"),InlineData("1e0"),InlineData("\"1\""),InlineData("true"),InlineData("null"),InlineData("2147483648"),InlineData("-1")]
    public void Integers_AreExactAndInRange(string value)
    {
        var dto=Parse("{\"name\":\"x\",\"number\":"+value+"}");
        Assert.Throws<ApiErrorException>(()=>dto.ReadInt32("number",0,int.MaxValue));
    }
    [Fact]
    public void ScalarAdapters_PreserveStrictAndExactSemantics()
    {
        var zero=Parse("{\"name\":\"x\",\"number\":-0,\"flag\":true}");
        Assert.Equal(0,zero.ReadInt32("number",0,10));
        Assert.True(zero.ReadBoolean("flag"));
        Assert.False(Parse("{\"name\":\"x\",\"flag\":false}").ReadBoolean("flag"));
        Assert.Throws<ApiErrorException>(()=>Parse("{\"name\":\"x\",\"flag\":1}").ReadBoolean("flag"));
        Assert.Throws<ApiErrorException>(()=>Parse("{\"name\":\"x\",\"nullable\":1}").ReadNullableString("nullable"));
        Assert.Equal(999999999999999999m,Parse("{\"name\":\"999999999999999999\"}").ReadParsed("name",ExactNumbers.ParseMoney));
        Assert.Equal(38,ReportAmountCodec.Format(Parse("{\"name\":\"99999999999999999999999999999999999999\"}").ReadParsed("name",ReportAmountCodec.Parse)).Length);
        Assert.Equal(new DateOnly(2024,2,29),Parse("{\"name\":\"2024-02-29\"}").ReadParsed("name",StrictScalars.ParseLocalDate));
        Assert.Equal(new TimeOnly(23,59),Parse("{\"name\":\"23:59\"}").ReadParsed("name",StrictScalars.ParseMinuteTime));
        Assert.Equal(8,Parse("{\"name\":\"ASNFZ4mrze8=\"}").ReadParsed("name",RowVersionCodec.Decode).Length);
        Assert.Equal("é",Parse("{\"name\":\"é\"}").ReadString("name"));
        Assert.Equal("é",Parse("{\"name\":\"é\"}").ReadParsed("name",s=>NonsecretText.NormalizeRequired(s,100)));
        Assert.Equal("a@example.com",Parse("{\"name\":\" A@EXAMPLE.COM \"}").ReadParsed("name",ContactNormalizer.NormalizeEmail));
        var error=Assert.Throws<ApiErrorException>(()=>Parse("{\"name\":\"PRIVATE_INPUT\"}").ReadParsed("name",ExactNumbers.ParseMoney));
        Assert.True(error.FieldErrors.ContainsKey("name"));
        Assert.DoesNotContain("PRIVATE_INPUT",error.Message);
    }
    [Fact]
    public void UnknownNamesNeverEchoAndDepthIsBounded()
    {
        var error=Assert.Throws<ApiErrorException>(()=>Parse("{\"name\":\"x\",\"PRIVATE_INPUT\":1}"));
        Assert.Empty(error.FieldErrors);
        var deep="{\"name\":\"x\",\"payload\":"+new string('[',65)+"0"+new string(']',65)+"}";
        Assert.Throws<ApiErrorException>(()=>Parse(deep));
    }
    [Fact]
    public void BadCallerSchemaAndRange_AreProgrammerErrors()
    {
        Assert.Throws<ArgumentException>(()=>StrictJsonObject.Parse("{}",new[]{"name"},new[]{"missing"}));
        Assert.Throws<ArgumentException>(()=>StrictJsonObject.Parse("{}",new[]{"name","name"},Array.Empty<string>()));
        Assert.Throws<ArgumentException>(()=>StrictJsonObject.Parse("{}",new[]{"bad/name"},Array.Empty<string>()));
        Assert.Throws<ArgumentException>(()=>Parse("{\"name\":\"x\"}").ReadInt32("number",2,1));
        Assert.Throws<ArgumentNullException>(()=>Parse("{\"name\":\"x\"}").ReadParsed<int>("name",null!));
    }
}
