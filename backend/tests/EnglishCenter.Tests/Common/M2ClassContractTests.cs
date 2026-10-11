namespace EnglishCenter.Tests.Common;

using System.Text.Json;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Services.Classes;
using Xunit;

public sealed class M2ClassContractTests
{
    [Theory]
    [InlineData("null")]
    [InlineData("{\"type\":\"LESSON\",\"date\":\"2026-10-12\",\"startTime\":\"09:00\",\"endTime\":\"10:00\",\"room\":null}")]
    [InlineData("{\"type\":\"LESSON\",\"date\":\"0001-01-01\",\"startTime\":\"00:00\",\"endTime\":\"01:00\",\"room\":\"A\"}")]
    [InlineData("{\"type\":\"LESSON\",\"date\":\"2026-02-30\",\"startTime\":\"09:00\",\"endTime\":\"10:00\",\"room\":\"A\"}")]
    [InlineData("{\"type\":\"LESSON\",\"date\":\"2026-10-12\",\"startTime\":\"09:00:01\",\"endTime\":\"10:00\",\"room\":\"A\"}")]
    [InlineData("{\"type\":\"LESSON\",\"date\":\"2026-10-12\",\"startTime\":\"10:00\",\"endTime\":\"09:00\",\"room\":\"A\"}")]
    [InlineData("{\"type\":\"LESSON\",\"date\":\"2026-10-12\",\"startTime\":\"09:00\",\"endTime\":\"10:00\",\"room\":\"A\",\"studentId\":1}")]
    public void InvalidScheduleShapeAndUnrepresentableUtcReturnValidation(string json)
    {
        using var doc = JsonDocument.Parse(json);
        Assert.Equal(ApiErrorCode.VALIDATION_ERROR,
            Assert.Throws<ApiErrorException>(() => ScheduleCandidate.Parse(doc.RootElement)).Code);
    }

    [Fact]
    public void CanonicalWindowHasVietnamUtcAndNormalizedRoom()
    {
        using var doc = JsonDocument.Parse("{\"type\":\"FINAL_EXAM\",\"date\":\"2026-10-12\",\"startTime\":\"09:00\",\"endTime\":\"10:00\",\"room\":\"  a-1  \"}");
        var window = ScheduleCandidate.Parse(doc.RootElement);
        Assert.Equal("A-1", window.Room);
        Assert.Equal(new DateTime(2026, 10, 12, 2, 0, 0, DateTimeKind.Utc), window.StartAt);
        Assert.Equal(TimeSpan.FromHours(1), window.EndAt - window.StartAt);
    }

    [Theory]
    [InlineData(100)]
    [InlineData(101)]
    [InlineData(120)]
    [InlineData(122)]
    [InlineData(124)]
    [InlineData(126)]
    public void DeferredCommandsCannotEnterClassService(int api)
        => Assert.Equal(ApiErrorCode.VALIDATION_ERROR,
            Assert.Throws<ApiErrorException>(() => new ClassInput(api, 1, Guid.NewGuid(), "{}", 65536)).Code);
}
