namespace EnglishCenter.Tests.Infrastructure;

using System.Reflection;
using System.Text.Json.Nodes;
using EnglishCenter.Api.Common.Auditing;
using Xunit;

public class AuditPrimitiveTests
{
    private static readonly DateTimeOffset At = new(2026,10,5,0,0,0,TimeSpan.Zero);
    private static AuditEntry Mandatory(string action="ADMIN.ACCOUNT.LOCK",AuditChanges? changes=null) => AuditEntry.Mandatory(action,1,"Account",2,Guid.NewGuid(),At,changes ?? new());

    [Fact]
    public void MandatoryDropsCredentialsUnknownKeysAndNestedUnknownPayload()
    {
        var entry=Mandatory(changes:new(New:new JsonObject { ["status"]="LOCKED",["password"]="SENTINEL_SECRET",["sid"]="SENTINEL_SECRET",["payload"]=new JsonObject { ["token"]="SENTINEL_SECRET" } }));
        var details=JsonNode.Parse(entry.Details!)!;
        Assert.Equal("LOCKED",details["new"]!["status"]!.GetValue<string>());
        Assert.DoesNotContain("SENTINEL",entry.Details);Assert.Null(entry.IpAddress);Assert.Null(entry.SecurityExpiresAt);
        Assert.Equal("MANDATORY",entry.Category);Assert.Equal("SUCCESS",entry.Result);
    }

    [Fact]
    public void ProjectionFreezesInputAndRejectsNestedKnownValues()
    {
        var source=new JsonObject { ["status"]="LOCKED" };var entry=Mandatory(changes:new(New:source));source["status"]="ACTIVE";
        Assert.Contains("LOCKED",entry.Details);
        Assert.Throws<ArgumentException>(()=>Mandatory(changes:new(New:new JsonObject { ["status"]=new JsonObject { ["secret"]="bad" } })));
    }

    [Theory]
    [InlineData("admin.account.lock")] [InlineData("ADMIN.ACCOUNT.LOCK.EFFECT.EFFECT")]
    [InlineData("TEACHER.ATTENDANCE.SAVE_DELTA")] [InlineData("unknown")]
    public void UnacceptedActionsAreRejectedWithoutReflectingInput(string action) =>
        Assert.Equal("Invalid audit projection.",Assert.Throws<ArgumentException>(()=>Mandatory(action)).Message);

    [Fact]
    public void RequiredReasonAndRestrictedCorrectionProjectionAreEnforced()
    {
        Assert.Throws<ArgumentException>(()=>AuditEntry.Mandatory("FINANCE.PAYMENT.UPDATE_METADATA",1,"Payment",2,Guid.NewGuid(),At,new()));
        var entry=AuditEntry.Mandatory("FINANCE.PAYMENT.UPDATE_METADATA",1,"Payment",2,Guid.NewGuid(),At,new(New:new JsonObject { ["method"]="CASH",["amountPaid"]=999m,["transactionReference"]=null },Reason:"correct metadata"));
        Assert.DoesNotContain("amountPaid",entry.Details);Assert.Contains("correct metadata",entry.Details);
        Assert.Throws<ArgumentException>(()=>Mandatory(changes:new(Reason:"not allowed")));
        Assert.Throws<ArgumentException>(()=>AuditEntry.Mandatory("ADMIN.STAFF.CREATE",1,"Account",2,Guid.NewGuid(),At,new(Old:new JsonObject { ["status"]="ACTIVE" })));
    }

    [Fact]
    public void EffectProjectionKeepsParentReferencesAndOperation()
    {
        var op=Guid.NewGuid();var entry=AuditEntry.Mandatory("ADMISSIONS.REGISTRATION.APPROVE.EFFECT",1,"Invoice",2,op,At,new(New:new JsonObject { ["amount"]=123456789012345678m,["status"]="UNPAID" },Parent:new JsonObject { ["registrationId"]=3,["courseId"]=4,["email"]="SECRET" }));
        Assert.Equal(op,entry.OperationId);Assert.Contains("123456789012345678",entry.Details);Assert.DoesNotContain("SECRET",entry.Details);
        Assert.Throws<ArgumentException>(()=>AuditEntry.Mandatory("ADMIN.ACCOUNT.LOCK.EFFECT",1,"Invoice",2,op,At,new()));
    }

    [Theory]
    [InlineData(SecurityAuditKind.Login)] [InlineData(SecurityAuditKind.Logout)]
    [InlineData(SecurityAuditKind.AccessDenied)] [InlineData(SecurityAuditKind.Throttled)]
    public void SecurityEventsUseNullActorExactExpiryAndUtcMilliseconds(SecurityAuditKind kind)
    {
        var entry=AuditEntry.Security(kind,null,false,Guid.NewGuid(),At.AddTicks(12345).ToOffset(TimeSpan.FromHours(7)),"2001:db8::1");
        Assert.Null(entry.ActorAccountId);Assert.Null(entry.EntityId);Assert.Null(entry.Details);
        Assert.Equal(0,entry.OccurredAt.Ticks%TimeSpan.TicksPerMillisecond);
        Assert.Equal(entry.OccurredAt.AddDays(90),entry.SecurityExpiresAt);Assert.Equal(entry.SecurityExpiresAt,entry.IpExpiresAt);
    }

    [Theory]
    [InlineData(-1)] [InlineData(4)]
    public void InvalidCounterKindRejected(int value)
    {
        var counter=new AuditIncidentCounter();Assert.Throws<ArgumentOutOfRangeException>(()=>counter.RecordFailure((SecurityAuditKind)value));
        Assert.Throws<ArgumentOutOfRangeException>(()=>counter.GetFailureCount((SecurityAuditKind)value));
    }

    [Fact]
    public async Task ConcurrentCountersPreserveEveryIncidentAndSaturate()
    {
        var counter=new AuditIncidentCounter();
        await Task.WhenAll(Enumerable.Range(0,16).Select(_=>Task.Run(()=> { for(var i=0;i<1000;i++) counter.RecordFailure(SecurityAuditKind.Login); },TestContext.Current.CancellationToken)));
        Assert.Equal(16000,counter.GetFailureCount(SecurityAuditKind.Login));Assert.Equal(0,counter.GetFailureCount(SecurityAuditKind.Logout));
        var counters=(long[])typeof(AuditIncidentCounter).GetField("_counters",BindingFlags.NonPublic|BindingFlags.Instance)!.GetValue(counter)!;
        counters[1]=long.MaxValue-1;counter.RecordFailure(SecurityAuditKind.Logout);counter.RecordFailure(SecurityAuditKind.Logout);
        Assert.Equal(long.MaxValue,counter.GetFailureCount(SecurityAuditKind.Logout));
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public async Task FailedOrCanceledSecurityLoggingCountsIncidentWithoutRestoringRevocation(bool cancel)
    {
        var revoked=true;var counter=new AuditIncidentCounter();var store=new FailingStore(cancel);
        var result=await new SecurityAuditRecorder(store,counter).TryRecordAsync(SecurityAuditKind.Logout,1,true,Guid.NewGuid(),At,null,TestContext.Current.CancellationToken);
        Assert.False(result);Assert.True(revoked);Assert.Equal(1,counter.GetFailureCount(SecurityAuditKind.Logout));
    }

    [Fact]
    public void EffectsRequireProjectedPositiveParentAndKnownActionsCannotSmuggleRoleOrScore()
    {
        var op=Guid.NewGuid();
        Assert.Throws<ArgumentException>(()=>AuditEntry.Mandatory("ACADEMIC.PROGRAM.UPDATE_CONFIGURATION.EFFECT",1,"ScoreComponentConfig",2,op,At,new()));
        Assert.Throws<ArgumentException>(()=>AuditEntry.Mandatory("ACADEMIC.PROGRAM.UPDATE_CONFIGURATION.EFFECT",1,"ScoreComponentConfig",2,op,At,new(Parent:new JsonObject { ["token"]="SECRET" })));
        var account=Mandatory(changes:new(New:new JsonObject { ["roleId"]=3,["securityVersion"]=2,["revokedSessionCount"]=0 }));
        Assert.DoesNotContain("roleId",account.Details);Assert.Contains("securityVersion",account.Details);
        var absent=AuditEntry.Mandatory("TEACHER.ENTRY_BOOKING.ABSENCE",1,"EntryTestBooking",2,op,At,new(New:new JsonObject { ["status"]="ABSENT",["score"]=0m,["absentBy"]=1 }));
        Assert.DoesNotContain("score",absent.Details);Assert.Contains("absentBy",absent.Details);
    }

    private sealed class FailingStore(bool cancel) : ISecurityAuditStore
    {
        public Task AppendAsync(AuditEntry entry,CancellationToken cancellationToken) =>
            Task.FromException(cancel ? new OperationCanceledException() : new InvalidOperationException("SENTINEL_SECRET"));
    }
}
