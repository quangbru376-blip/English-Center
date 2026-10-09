namespace EnglishCenter.Tests.Common;
using System.Text.Json;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Services.Academics;
using EnglishCenter.Api.Infrastructure.Academics;
using Xunit;
public sealed class M1AcademicInputTests
{
    [Theory]
    [InlineData("0")][InlineData("100.001")][InlineData("1e2")][InlineData("-1")][InlineData("100.01")]
    public void ComponentWeightRejectsRangeAndPrecision(string weight)
    {var json=JsonSerializer.Serialize(new{courseId=1,bandId=2,name="Program",expectedLessonCount=1,components=new[]{new{name="Exam",weight,kind="FINAL_EXAM"}},expectedVersions=new{course="AAAAAAAAAAE=",band="AAAAAAAAAAE="}});Assert.Throws<ApiErrorException>(()=>AcademicInput.Parse(82,0,Guid.NewGuid(),json,4096));}
    [Theory]
    [InlineData("{\"page\":\"1\"}")][InlineData("{\"pageSize\":101}")][InlineData("{\"actorId\":1}")][InlineData("{\"page\":1e0}")][InlineData("{\"view\":\"all\"}")][InlineData("{\"phone\":\"123\"}")]
    public void RosterMalformedInputsAre400(string json)=>Assert.Throws<ApiErrorException>(()=>SessionRosterFilter.Parse(json));
    [Fact]
    public void RosterBlankFiltersLiteralNameAndCanonicalPhone()
    {var filter=SessionRosterFilter.Parse("{\"name\":\" %_ \",\"email\":\" \",\"phone\":\"+84912345678\",\"page\":2147483647}");Assert.Equal("%_",filter.Name);Assert.Null(filter.Email);Assert.Equal("0912345678",filter.Phone);Assert.Equal(int.MaxValue,filter.Page);}
    [Theory]
    [InlineData("09:00","09:00")][InlineData("10:00","09:00")][InlineData("09:00:00","10:00")]
    public void SessionMinuteWindowRejectsMalformedOrNonpositive(string start,string end)=>Assert.Throws<ApiErrorException>(()=>SessionWindow.Parse(JsonSerializer.SerializeToElement(new{date="2026-10-10",startTime=start,endTime=end,room="TEST"})));
}
