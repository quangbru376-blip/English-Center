namespace EnglishCenter.Tests.Api.Integration;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Domain.Administration;
using EnglishCenter.Api.Infrastructure.Administration;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using Xunit;

[Collection("V01 HTTP SQL")]
public sealed class M4AuditSqlTests
{
    private const string OptIn="Assigned M4 owned scratch Admin audit SQL; exact ID/GUID cleanup.";
    private static CancellationToken Ct=>OwnedSqlFixture.Ct;
    private static SqlAuditViewerExecutor Ex(OwnedSqlFixture f)=>new(()=>f.Db(),f.Clock,10,1000);
    private static QueryCollection Q(params(string Key,string Value)[] pairs)=>new(pairs.ToDictionary(x=>x.Key,x=>(StringValues)x.Value));
    private sealed class AfterSnapshot(OwnedSqlFixture fixture,M1AdmissionsSqlTests.Clock time,bool extend):Microsoft.EntityFrameworkCore.Diagnostics.DbCommandInterceptor
    {
        public bool Fired{get;private set;}
        public override async ValueTask<System.Data.Common.DbDataReader> ReaderExecutedAsync(System.Data.Common.DbCommand command,Microsoft.EntityFrameworkCore.Diagnostics.CommandExecutedEventData data,System.Data.Common.DbDataReader reader,CancellationToken ct=default)
        {
            if(!Fired&&command.Transaction?.IsolationLevel==System.Data.IsolationLevel.Snapshot&&command.CommandText.Contains("[AuditLog]",StringComparison.Ordinal))
            {
                Fired=true;await using var db=fixture.Db();
                if(extend){foreach(var session in await db.AuthSessions.Where(x=>x.AccountId==fixture.Accounts["Admin"]).ToArrayAsync(ct))session.LastActivityAt=time.Now.AddMinutes(20).UtcDateTime;await db.SaveChangesAsync(ct);time.Now=time.Now.AddMinutes(31);}
                else{var account=await db.Accounts.SingleAsync(x=>x.AccountId==fixture.Accounts["Admin"],ct);account.Status="LOCKED";await db.SaveChangesAsync(ct);}
            }
            return reader;
        }
    }
    private sealed class RetentionCutoff(M1AdmissionsSqlTests.Clock time):Microsoft.EntityFrameworkCore.Diagnostics.DbCommandInterceptor
    {
        public bool Fired{get;private set;}
        public override ValueTask<System.Data.Common.DbDataReader> ReaderExecutedAsync(System.Data.Common.DbCommand command,Microsoft.EntityFrameworkCore.Diagnostics.CommandExecutedEventData data,System.Data.Common.DbDataReader reader,CancellationToken ct=default)
        {if(!Fired&&command.Transaction?.IsolationLevel==System.Data.IsolationLevel.Snapshot&&command.CommandText.Contains("[AuditLog]",StringComparison.Ordinal)){Fired=true;time.Now=time.Now.AddSeconds(2);}return ValueTask.FromResult(reader);}
    }
    [Theory(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    [InlineData(true,false)][InlineData(true,true)][InlineData(false,true)]
    public async Task RetentionCrossingKeepsCoherentPageOrRedactsDetailIp(bool security,bool detail)
    {
        var time=new M1AdmissionsSqlTests.Clock();await using var f=await OwnedSqlFixture.CreateAsync(time);var token=f.Token(await f.Grant("Admin"));var operation=Guid.NewGuid();int id;
        await using(var db=f.Db())
        {
            var at=time.Now.AddDays(-90).AddSeconds(1).UtcDateTime;
            var row=new AuditLog{Action=security?"AUTH.LOGIN":"SYSTEM.BAND.SEED",EntityType=security?"Account":"Band",EntityId=security?null:"1",Result="SUCCESS",OccurredAt=at,OperationId=operation,AuditCategory=security?"SECURITY":"MANDATORY",SecurityExpiresAt=security?at.AddDays(90):null,IpAddress="127.0.0.1",IpExpiresAt=at.AddDays(90)};
            db.AuditLogs.Add(row);await db.SaveChangesAsync(Ct);id=row.AuditId;
        }
        var probe=new RetentionCutoff(time);var ex=new SqlAuditViewerExecutor(()=>f.Db(probe),f.Clock,10,1000);
        if(!security)Assert.Null((await ex.Detail(token,id,Ct)).IpAddress);
        else Assert.Equal(detail?ApiErrorCode.NOT_FOUND:ApiErrorCode.SERVICE_UNAVAILABLE,(await Assert.ThrowsAsync<ApiErrorException>(()=>detail?ex.Detail(token,id,Ct):(Task)ex.Operation(token,operation,Q(),Ct))).Code);
        Assert.True(probe.Fired);
    }
    [Theory(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    [InlineData(false)][InlineData(true)]
    public async Task SnapshotResponseRefusedAfterLockOrOriginalCapDespiteConcurrentActivity(bool extend)
    {
        var time=new M1AdmissionsSqlTests.Clock();await using var f=await OwnedSqlFixture.CreateAsync(time);var token=f.Token(await f.Grant("Admin"));var probe=new AfterSnapshot(f,time,extend);var ex=new SqlAuditViewerExecutor(()=>f.Db(probe),f.Clock,10,1000);
        Assert.Equal(ApiErrorCode.AUTHENTICATION_REQUIRED,(await Assert.ThrowsAsync<ApiErrorException>(()=>ex.List(token,Q(),Ct))).Code);Assert.True(probe.Fired);
    }
    [Fact(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    public async Task NewestEventPagingOperationGroupingAndDetailDoNotWriteOrJoinProfiles()
    {
        await using var f=await OwnedSqlFixture.CreateAsync();var admin=f.Token(await f.Grant("Admin"));var operation=Guid.NewGuid();var now=f.Clock.GetUtcNow().UtcDateTime;int first,last;long count;
        await using(var db=f.Db())
        {
            var one=new AuditLog{ActorAccountId=null,Action="SYSTEM.BAND.SEED",EntityType="Band",EntityId="1",Result="SUCCESS",OccurredAt=now,OperationId=operation,AuditCategory="MANDATORY",Details="{\"new\":{\"bandCode\":\"B0\",\"minScore\":0,\"token\":\"forbidden\"}}"};
            var two=new AuditLog{ActorAccountId=admin.AccountId,Action="AUTH.LOGIN",EntityType="Account",EntityId=admin.AccountId.ToString(),Result="SUCCESS",OccurredAt=now,OperationId=operation,AuditCategory="SECURITY",SecurityExpiresAt=now.AddDays(90),IpAddress="127.0.0.1",IpExpiresAt=now.AddDays(90)};
            db.AuditLogs.Add(one);await db.SaveChangesAsync(Ct);first=one.AuditId;db.AuditLogs.Add(two);await db.SaveChangesAsync(Ct);last=two.AuditId;count=await db.AuditLogs.LongCountAsync(Ct);
        }
        var page=await Ex(f).Operation(admin,operation,Q(("pageSize","1")),Ct);Assert.Equal(2,page.TotalCount);Assert.Equal(last,page.Items.Single().Id);Assert.Equal(first,(await Ex(f).Operation(admin,operation,Q(("page","2"),("pageSize","1")),Ct)).Items.Single().Id);
        var detail=await Ex(f).Detail(admin,first,Ct);Assert.Null(detail.ActorId);Assert.Equal(2,detail.Changes.Count);Assert.DoesNotContain("forbidden",System.Text.Json.JsonSerializer.Serialize(detail));
        Assert.Empty((await Ex(f).Operation(admin,Guid.NewGuid(),Q(),Ct)).Items);
        await using var final=f.Db();Assert.Equal(count,await final.AuditLogs.LongCountAsync(Ct));Assert.Equal(0,await final.ReplayOperations.CountAsync(Ct));
    }
    [Theory(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    [InlineData("Student")][InlineData("Admissions")][InlineData("AcademicAffairs")][InlineData("Teacher")][InlineData("Accountant")][InlineData("Director")]
    public async Task EveryOtherRoleDeniedBeforeData(string role)
    {await using var f=await OwnedSqlFixture.CreateAsync();var token=f.Token(await f.Grant(role));Assert.Equal(ApiErrorCode.FORBIDDEN,(await Assert.ThrowsAsync<ApiErrorException>(()=>Ex(f).List(token,Q(),Ct))).Code);}
    [Fact(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    public async Task SecurityExpiresWithoutPurgingMandatoryHistoryAndRevokedAdminIsDenied()
    {
        await using var f=await OwnedSqlFixture.CreateAsync();var token=f.Token(await f.Grant("Admin"));var now=f.Clock.GetUtcNow().UtcDateTime;int expired;
        await using(var db=f.Db())
        {var row=new AuditLog{Action="AUTH.LOGIN",EntityType="Account",Result="FAILURE",OccurredAt=now.AddDays(-90),OperationId=Guid.NewGuid(),AuditCategory="SECURITY",SecurityExpiresAt=now};db.AuditLogs.Add(row);await db.SaveChangesAsync(Ct);expired=row.AuditId;}
        Assert.Equal(ApiErrorCode.NOT_FOUND,(await Assert.ThrowsAsync<ApiErrorException>(()=>Ex(f).Detail(token,expired,Ct))).Code);
        await using(var db=f.Db()){var sessions=await db.AuthSessions.Where(x=>x.AccountId==token.AccountId).ToArrayAsync(Ct);foreach(var sid in sessions)sid.RevokedAt=now;await db.SaveChangesAsync(Ct);}
        Assert.Equal(ApiErrorCode.AUTHENTICATION_REQUIRED,(await Assert.ThrowsAsync<ApiErrorException>(()=>Ex(f).List(token,Q(),Ct))).Code);
    }
    [Fact(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    public async Task VietnamInclusiveBoundsAndMandatoryHistoryBeyondNinetyDays()
    {
        await using var f=await OwnedSqlFixture.CreateAsync();var token=f.Token(await f.Grant("Admin"));var operation=Guid.NewGuid();
        var start=new DateTime(2025,1,1,17,0,0,DateTimeKind.Utc);
        await using(var db=f.Db())
        {
            foreach(var at in new[]{start.AddMilliseconds(-1),start,start.AddDays(1).AddMilliseconds(-1),start.AddDays(1)})db.AuditLogs.Add(new AuditLog{Action="SYSTEM.BAND.SEED",EntityType="Band",EntityId="1",Result="SUCCESS",OccurredAt=at,OperationId=operation,AuditCategory="MANDATORY",Details="{\"new\":{\"bandCode\":\"B0\",\"minScore\":0}}"});
            await db.SaveChangesAsync(Ct);
        }
        var page=await Ex(f).List(token,Q(("fromDate","2025-01-02"),("toDate","2025-01-02"),("action","SYSTEM.BAND.SEED")),Ct);
        Assert.Equal(2,page.TotalCount);Assert.Equal("2025-01-02T16:59:59.999Z",page.Items[0].OccurredAt);Assert.Equal("2025-01-01T17:00:00.000Z",page.Items[1].OccurredAt);
        Assert.Equal(4,(await Ex(f).Operation(token,operation,Q(),Ct)).TotalCount);
    }
}
