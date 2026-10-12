namespace EnglishCenter.Tests;
using System.Text.Json.Nodes;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Domain.Administration;
using EnglishCenter.Api.Services.Administration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Xunit;

public sealed class M4AuditPolicyTests
{
    private static QueryCollection Query(params(string Key,string Value)[] pairs)=>new(pairs.ToDictionary(x=>x.Key,x=>(StringValues)x.Value));
    private static AuditLog Row(string action="ACADEMIC.CLASS.RETURN_GRADES",string entity="Class",string details="{}")=>new(){AuditId=1,OccurredAt=new(2026,10,12,0,0,0,DateTimeKind.Utc),ActorAccountId=7,Action=action,EntityType=entity,EntityId="19",OperationId=Guid.NewGuid(),Result="SUCCESS",Details=details,AuditCategory="MANDATORY"};
    [Theory][InlineData("action","UNKNOWN")][InlineData("action","AUTH.LOGIN.EFFECT")][InlineData("entityType","AuthSession")][InlineData("entityId","alice@example.com")][InlineData("entityId","012")][InlineData("entityId","ffffffff-ffff-ffff-ffff-ffffffffffff")][InlineData("result","ACTIVE")][InlineData("pageSize","101")][InlineData("sort","ASC")]
    public void ArbitraryFiltersNeverReachSql(string key,string value)=>Assert.Throws<ApiErrorException>(()=>AuditViewerQuery.Parse(Query((key,value))));
    [Fact] public void StructuredFiltersKeepHistoryUnboundedAndRejectInvertedDates()
    {var q=AuditViewerQuery.Parse(Query(("action","AUTH.PASSWORD_CHANGE"),("entityType","Account"),("entityId","19")));Assert.Null(q.From);Assert.Null(q.To);Assert.Equal(20,q.Size);Assert.Throws<ApiErrorException>(()=>AuditViewerQuery.Parse(Query(("fromDate","2026-10-13"),("toDate","2026-10-12"))));Assert.Throws<ApiErrorException>(()=>AuditViewerQuery.Parse(Query(("action","AUTH.LOGIN"),("entityType","Invoice"))));}
    [Fact] public void DetailOnlyProjectsTypedAcceptedFieldsAndStoredReason()
    {
        var row=Row(details:"{\"old\":{\"status\":\"SUBMITTED\",\"password\":\"forbidden\",\"teacherId\":{\"nested\":1}},\"new\":{\"status\":\"RETURNED\",\"token\":\"forbidden\"},\"reason\":\"Cần kiểm tra\",\"request\":{\"sid\":\"forbidden\"}}");
        var detail=AuditViewerPolicy.Detail(row,row.OccurredAt);Assert.Equal("Cần kiểm tra",detail.Reason);Assert.Single(detail.Changes);Assert.Equal("status",detail.Changes[0].Field);Assert.Equal("SUBMITTED",detail.Changes[0].OldValue!.GetValue<string>());Assert.Equal("RETURNED",detail.Changes[0].NewValue!.GetValue<string>());
    }
    [Fact] public void SpecialEventsNeverExposeProfileOrPasswordPayloads()
    {
        var profile=AuditViewerPolicy.Detail(Row("STUDENT.PROFILE.UPDATE","StudentProfile","{\"changedFields\":[\"phone\"],\"new\":{\"phone\":\"private\"},\"reason\":\"unregistered\"}"),DateTime.UtcNow);Assert.Empty(profile.Changes);Assert.Null(profile.Reason);
        var pw=AuditViewerPolicy.Detail(Row("AUTH.PASSWORD_CHANGE","Account","{\"new\":{\"securityVersion\":3,\"revokedSessionCount\":2,\"passwordHash\":\"forbidden\",\"mustChangePassword\":false}}"),DateTime.UtcNow);Assert.Equal(3,pw.Changes.Count);
    }
    [Fact] public void WireMoneyAndScoresAreExactStringsAndMissingBeforeIsNull()
    {
        var amount=AuditViewerPolicy.Detail(Row("FINANCE.INVOICE.COLLECT.EFFECT","Payment","{\"new\":{\"amountPaid\":999999999999999999,\"method\":\"CASH\"}}"),DateTime.UtcNow).Changes.First(x=>x.Field=="amountPaid");Assert.Null(amount.OldValue);Assert.Equal("999999999999999999",amount.NewValue!.GetValue<string>());
        var score=AuditViewerPolicy.Detail(Row("ACADEMIC.CLASS.FINALIZE_GRADES.EFFECT","GradeReport","{\"new\":{\"finalScore\":49.99}}"),DateTime.UtcNow).Changes.Single();Assert.Equal("49.99",score.NewValue!.GetValue<string>());
    }
    [Fact] public void IpRetentionCutoffAndUnsafeReferencesFailClosed()
    {var row=Row();row.IpAddress="127.0.0.1";row.IpExpiresAt=row.OccurredAt.AddDays(90);Assert.Equal(row.IpAddress,AuditViewerPolicy.Detail(row,row.IpExpiresAt.Value.AddMilliseconds(-1)).IpAddress);Assert.Null(AuditViewerPolicy.Detail(row,row.IpExpiresAt.Value).IpAddress);row.EntityId="raw sid";Assert.Null(AuditViewerPolicy.Summary(row).EntityId);}
    [Theory][InlineData("{bad")][InlineData("{\"new\":{\"status\":[]}}")][InlineData("{\"new\":{\"status\":\"TOKEN\"}}")]
    public void MalformedDetailsDoNotLeakRawJson(string details)=>Assert.Empty(AuditViewerPolicy.Detail(Row(details:details),DateTime.UtcNow).Changes);
    [Fact] public void OperationAcceptsPagingOnly()=>Assert.Throws<ApiErrorException>(()=>AuditViewerQuery.Parse(Query(("action","AUTH.LOGIN")),true));
}
