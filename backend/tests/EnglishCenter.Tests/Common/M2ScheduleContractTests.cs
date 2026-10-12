namespace EnglishCenter.Tests.Common;

using System.Text.Json;
using System.Text.Json.Nodes;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Services.Classes;
using Xunit;

public sealed class M2ScheduleContractTests
{
    private static JsonObject Rule()=>new(){["fromDate"]="2026-10-12",["toDate"]="2026-10-18",["weekdays"]=new JsonArray(1,7),["startTime"]="09:00",["endTime"]="10:00",["room"]="  a-1  "};
    private static WeeklyRule Parse(JsonObject rule){using var doc=JsonDocument.Parse(rule.ToJsonString());return WeeklyRule.Parse(doc.RootElement);}
    [Fact]
    public void InclusiveIsoMondaySundayProducesFullOrderedLessonPlanAndVietnamTime()
    {
        var plan=Parse(Rule()).Expand(new(7));Assert.Equal(2,plan.Length);Assert.Equal(new DateOnly(2026,10,12),plan[0].Date);Assert.Equal(new DateOnly(2026,10,18),plan[1].Date);Assert.All(plan,x=>{Assert.Equal("LESSON",x.Type);Assert.Equal("A-1",x.Room);Assert.Equal(2,x.StartAt.Hour);});
    }
    [Theory]
    [InlineData("duplicate")][InlineData("empty")][InlineData("zero")][InlineData("eight")][InlineData("seconds")][InlineData("date")][InlineData("end")][InlineData("extra")]
    public void InvalidRulesFail400(string invalid)
    {
        var r=Rule();switch(invalid){case "duplicate":r["weekdays"]=new JsonArray(1,1);break;case "empty":r["weekdays"]=new JsonArray();break;case "zero":r["weekdays"]=new JsonArray(0);break;case "eight":r["weekdays"]=new JsonArray(8);break;case "seconds":r["startTime"]="09:00:00";break;case "date":r["fromDate"]="2026-02-30";break;case "end":r["endTime"]="08:59";break;case "extra":r["skipDates"]=new JsonArray();break;}
        Assert.Equal(ApiErrorCode.VALIDATION_ERROR,Assert.Throws<ApiErrorException>(()=>Parse(r)).Code);
    }
    [Fact]
    public void BudgetExhaustionFails503WithoutTruncationAndMaximumLocalDateDoesNotOverflow()
    {
        Assert.Equal(ApiErrorCode.SERVICE_UNAVAILABLE,Assert.Throws<ApiErrorException>(()=>Parse(Rule()).Expand(new(6))).Code);
        var r=Rule();r["fromDate"]="9999-12-31";r["toDate"]="9999-12-31";r["weekdays"]=new JsonArray(1,2,3,4,5,6,7);Assert.Single(Parse(r).Expand(new(1)));
    }
    [Theory]
    [InlineData(100)][InlineData(101)][InlineData(126)]
    public void TeachingAndHeldCannotEnterAssignedScheduleCommands(int api)=>Assert.Equal(ApiErrorCode.VALIDATION_ERROR,Assert.Throws<ApiErrorException>(()=>new ScheduleInput(api,1,Guid.NewGuid(),"{}",65536)).Code);
}
