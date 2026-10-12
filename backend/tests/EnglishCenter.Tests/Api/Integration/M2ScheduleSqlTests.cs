namespace EnglishCenter.Tests.Api.Integration;

using System.Text.Json.Nodes;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Common.Security;
using EnglishCenter.Api.Common.Time;
using EnglishCenter.Api.Infrastructure.Classes;
using EnglishCenter.Api.Services.Classes;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Xunit;

[Collection("V01 HTTP SQL")]
public sealed class M2ScheduleSqlTests
{
    private const string OptIn="Assigned final M2 batch: owned scratch COMMIT, identity-paired cleanup.";
    private static CancellationToken Ct=>OwnedSqlFixture.Ct;
    internal static JsonObject Rule(OwnedSqlFixture f,int days=2)=>ClassWire.Object(new{fromDate=f.Clock.GetToday().AddDays(1).ToString("yyyy-MM-dd"),toDate=f.Clock.GetToday().AddDays(days).ToString("yyyy-MM-dd"),weekdays=new[]{1,2,3,4,5,6,7},startTime="09:00",endTime="10:00",room="WEEK_ROOM"});
    internal static async Task<JsonObject> WeeklyBody(OwnedSqlFixture f,VerifiedSessionToken t,int id,JsonObject? rule=null)
    {var p=await M2ClassSqlTests.Ex(f).WeeklyPreview(t,id,new JsonObject{["rule"]=(rule??Rule(f)).DeepClone()}.ToJsonString(),Ct);Assert.True(p["eligible"]!.GetValue<bool>(),p.ToJsonString());var b=p["manifest"]!.DeepClone().AsObject();b["rule"]=p["rule"]!.DeepClone();b["candidates"]=p["candidates"]!.DeepClone();return b;}
    private static JsonObject Draft(OwnedSqlFixture f,int days=3,bool replace=false)=>ClassWire.Object(new{newDate=f.Clock.GetToday().AddDays(days).ToString("yyyy-MM-dd"),startTime="09:30",endTime="10:30",room="NEXT_ROOM"}).WithReason(replace);
    internal static async Task<JsonObject> EditBody(OwnedSqlFixture f,VerifiedSessionToken t,int schedule,JsonObject draft,bool replace=false)
    {var p=await M2ClassSqlTests.Ex(f).EditPreview(t,schedule,draft.ToJsonString(),replace,Ct);Assert.True(p["eligible"]!.GetValue<bool>(),p.ToJsonString());var b=p["manifest"]!.DeepClone().AsObject();foreach(var(k,v)in draft)b[k]=v?.DeepClone();return b;}
    private static async Task<(VerifiedSessionToken Token,int Class)> Setup(OwnedSqlFixture f)
    {var(t,p,_)=await M2ClassSqlTests.Seed(f);return(t,await M2ClassSqlTests.Create(f,t,p));}
    private static async Task<ApiErrorCode> Error(Func<Task> work)=>(await Assert.ThrowsAsync<ApiErrorException>(work)).Code;
    [Fact(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    public async Task TwoReplacementIntentsHaveOneSuccessorAndFullThreeLinkChainRetainsDirectReferences()
    {
        var time=new M1AdmissionsSqlTests.Clock();await using var f=await OwnedSqlFixture.CreateAsync(time);var(t,id)=await Setup(f);await M2ClassSqlTests.Schedule(f,t,id,M2ClassSqlTests.Window(f));int oldId;Guid obligation;await using(var db=f.Db()){var old=await db.Schedules.SingleAsync(Ct);oldId=old.ScheduleId;obligation=old.ObligationKey;time.Now=new DateTimeOffset(DateTime.SpecifyKind(ClassWire.Window(old).StartAt,DateTimeKind.Utc));}
        t=f.Token(await f.Grant("AcademicAffairs"));var ex=M2ClassSqlTests.Ex(f);var body=await EditBody(f,t,oldId,Draft(f,1,true),true);
        async Task<bool> Send(){try{await ex.ScheduleCommand(t,124,oldId,Guid.NewGuid(),body.ToJsonString(),65536,Ct);return true;}catch(ApiErrorException e){Assert.Equal(ApiErrorCode.STATE_CONFLICT,e.Code);return false;}}
        Assert.Single(await Task.WhenAll(Send(),Send()),x=>x);int middle;await using(var db=f.Db()){var row=await db.Schedules.SingleAsync(x=>x.IsCurrent,Ct);middle=row.ScheduleId;time.Now=new DateTimeOffset(DateTime.SpecifyKind(ClassWire.Window(row).StartAt,DateTimeKind.Utc));}
        t=f.Token(await f.Grant("AcademicAffairs"));body=await EditBody(f,t,middle,Draft(f,1,true),true);var last=(await ex.ScheduleCommand(t,124,middle,Guid.NewGuid(),body.ToJsonString(),65536,Ct)).Result["newScheduleId"]!.GetValue<int>();var detail=await ex.ScheduleDetail(t,oldId,Ct);Assert.Equal(middle,detail["replacementScheduleId"]!.GetValue<int>());Assert.Equal(last,detail["currentScheduleId"]!.GetValue<int>());Assert.Equal(3,detail["replacementChain"]!.AsArray().Count);await using(var db=f.Db()){Assert.Equal(3,await db.Schedules.CountAsync(x=>x.ObligationKey==obligation,Ct));Assert.Single(await db.Schedules.Where(x=>x.IsCurrent).ToArrayAsync(Ct));Assert.Equal(2,await db.ReplayOperations.CountAsync(x=>x.ActionCode=="ACADEMIC.SCHEDULE.REPLACE",Ct));Assert.Equal(4,await db.AuditLogs.CountAsync(x=>x.Action.StartsWith("ACADEMIC.SCHEDULE.REPLACE"),Ct));}
        var teacher=f.Token(await f.Grant("Teacher"));var scoped=await ex.OwnScheduleDetail(teacher,oldId,"Teacher",Ct);Assert.Equal(3,scoped["replacementChain"]!.AsArray().Count);Assert.False(scoped.ContainsKey("notHeld"));
    }
    [Fact(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    public async Task MoveDoesNotIgnoreOtherOwnObligationsOrPostPlanExamOrdering()
    {
        await using var f=await OwnedSqlFixture.CreateAsync(new M1AdmissionsSqlTests.Clock());var(t,id)=await Setup(f);await M2ClassSqlTests.Schedule(f,t,id,M2ClassSqlTests.Window(f));await M2ClassSqlTests.Schedule(f,t,id,M2ClassSqlTests.Window(f,start:"10:00",end:"11:00"));await M2ClassSqlTests.Schedule(f,t,id,M2ClassSqlTests.Window(f,"FINAL_EXAM",start:"11:00",end:"12:00"));int first,exam;await using(var db=f.Db()){first=await db.Schedules.Where(x=>x.SessionType=="LESSON").OrderBy(x=>x.ScheduleId).Select(x=>x.ScheduleId).FirstAsync(Ct);exam=(await db.Schedules.SingleAsync(x=>x.SessionType=="FINAL_EXAM",Ct)).ScheduleId;}
        var ex=M2ClassSqlTests.Ex(f);var overlap=ClassWire.Object(new{newDate=f.Clock.GetToday().AddDays(1).ToString("yyyy-MM-dd"),startTime="09:30",endTime="10:30",room="OTHER"});var preview=await ex.EditPreview(t,first,overlap.ToJsonString(),false,Ct);Assert.False(preview["eligible"]!.GetValue<bool>());Assert.Contains(preview["conflicts"]!.AsArray(),x=>x!["kind"]!.GetValue<string>()=="CLASS");overlap["newDate"]=f.Clock.GetToday().AddDays(2).ToString("yyyy-MM-dd");Assert.False((await ex.EditPreview(t,first,overlap.ToJsonString(),false,Ct))["eligible"]!.GetValue<bool>());overlap["newDate"]=f.Clock.GetToday().AddDays(1).ToString("yyyy-MM-dd");overlap["startTime"]="08:00";overlap["endTime"]="09:00";Assert.False((await ex.EditPreview(t,exam,overlap.ToJsonString(),false,Ct))["eligible"]!.GetValue<bool>());
    }
    [Fact(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    public async Task CookieCsrfHttpWeeklyMoveLookupRoleAndStrictQueryBoundaries()
    {
        await using var f=await OwnedSqlFixture.CreateAsync(new M1AdmissionsSqlTests.Clock());var(t,id)=await Setup(f);var body=await WeeklyBody(f,t,id);await using var host=await M1HttpSqlTests.Host.Create(f);await host.Login("AcademicAffairs");var key=Guid.NewGuid();
        using(var denied=await host.Send(System.Net.Http.HttpMethod.Post,$"/api/v1/academics/classes/{id}/weekly-schedules",body,key,csrf:false))Assert.Equal(System.Net.HttpStatusCode.Forbidden,denied.StatusCode);
        using(var first=await host.Send(System.Net.Http.HttpMethod.Post,$"/api/v1/academics/classes/{id}/weekly-schedules",body,key))Assert.Equal(System.Net.HttpStatusCode.Created,first.StatusCode);
        using(var again=await host.Send(System.Net.Http.HttpMethod.Post,$"/api/v1/academics/classes/{id}/weekly-schedules",body,key))Assert.Equal(System.Net.HttpStatusCode.OK,again.StatusCode);
        using(var lookup=await host.Send(System.Net.Http.HttpMethod.Get,$"/api/v1/admissions/command-results/{key:D}?action=ACADEMIC.SCHEDULE.CREATE_WEEKLY"))Assert.Equal(System.Net.HttpStatusCode.OK,lookup.StatusCode);
        int sid;await using(var db=f.Db())sid=await db.Schedules.OrderBy(x=>x.ScheduleId).Select(x=>x.ScheduleId).FirstAsync(Ct);var edit=await EditBody(f,t,sid,Draft(f));
        using(var move=await host.Send(System.Net.Http.HttpMethod.Put,$"/api/v1/academics/schedules/{sid}/time-room",edit,Guid.NewGuid()))Assert.Equal(System.Net.HttpStatusCode.OK,move.StatusCode);
        await host.Login("Teacher");using(var role=await host.Send(System.Net.Http.HttpMethod.Post,$"/api/v1/academics/schedules/{sid}/replace",new{},Guid.NewGuid()))Assert.Equal(System.Net.HttpStatusCode.Forbidden,role.StatusCode);
        using(var own=await host.Send(System.Net.Http.HttpMethod.Get,$"/api/v1/academics/teachers/me/schedules/{sid}"))Assert.Equal(System.Net.HttpStatusCode.OK,own.StatusCode);
        using(var malformed=await host.Send(System.Net.Http.HttpMethod.Get,"/api/v1/academics/teachers/me/schedules?teacherId=1"))Assert.Equal(System.Net.HttpStatusCode.BadRequest,malformed.StatusCode);
        using(var held=await host.Send(System.Net.Http.HttpMethod.Post,$"/api/v1/academics/schedules/{sid}/confirm-held",new{},Guid.NewGuid()))Assert.Equal(System.Net.HttpStatusCode.Forbidden,held.StatusCode);
    }
    private static async Task Place(OwnedSqlFixture f,VerifiedSessionToken t,int id,int registration)
    {var p=await M2ClassSqlTests.Ex(f).Preview(t,id,"placement",$"{{\"registrationId\":{registration}}}",Ct);var b=p["manifest"]!.DeepClone().AsObject();b["registrationId"]=registration;await M2ClassSqlTests.Ex(f).Command(t,108,id,Guid.NewGuid(),b.ToJsonString(),65536,Ct);}
    private sealed class Advance(M1AdmissionsSqlTests.Clock clock,DateTimeOffset time,string verb):Microsoft.EntityFrameworkCore.Diagnostics.DbCommandInterceptor
    {
        public bool Fired {get;private set;}
        public override ValueTask<System.Data.Common.DbDataReader> ReaderExecutedAsync(System.Data.Common.DbCommand command,Microsoft.EntityFrameworkCore.Diagnostics.CommandExecutedEventData data,System.Data.Common.DbDataReader reader,CancellationToken ct=default)
        {if(!Fired&&command.CommandText.Contains("[Schedule]",StringComparison.Ordinal)&&command.CommandText.Contains(verb,StringComparison.Ordinal)){Fired=true;clock.Now=time;}return ValueTask.FromResult(reader);}
    }
    [Theory(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    [InlineData(false)][InlineData(true)]
    public async Task FinalProjectionClockCutoffRollsBackAllScheduleAuditAndMarker(bool move)
    {
        var time=new M1AdmissionsSqlTests.Clock();await using var f=await OwnedSqlFixture.CreateAsync(time);var(t,id)=await Setup(f);var rule=Rule(f);rule["fromDate"]=f.Clock.GetToday().ToString("yyyy-MM-dd");rule["toDate"]=f.Clock.GetToday().AddDays(1).ToString("yyyy-MM-dd");rule["startTime"]="10:10";rule["endTime"]="11:10";
        JsonObject b;int target=id;var api=120;
        if(move){await M2ClassSqlTests.Schedule(f,t,id,M2ClassSqlTests.Window(f,start:"10:10",end:"11:10",days:0));await using var db=f.Db();target=(await db.Schedules.SingleAsync(Ct)).ScheduleId;api=122;b=await EditBody(f,t,target,Draft(f));}else b=await WeeklyBody(f,t,id,rule);
        var interception=new Advance(time,time.Now.AddMinutes(10),move?"UPDATE":"INSERT");var ex=new SqlClassExecutor(()=>f.Db(interception),f.Clock,10,1000);var key=Guid.NewGuid();Assert.Equal(ApiErrorCode.STATE_CONFLICT,await Error(()=>ex.ScheduleCommand(t,api,target,key,b.ToJsonString(),65536,Ct)));Assert.True(interception.Fired);
        await using(var db=f.Db()){Assert.Equal(move?1:0,await db.Schedules.CountAsync(Ct));if(move)Assert.Equal(new TimeOnly(10,10),(await db.Schedules.SingleAsync(Ct)).StartTime);Assert.False(await db.ReplayOperations.AnyAsync(x=>x.RequestId==key,Ct));Assert.Empty(await db.AuditLogs.Where(x=>x.Action=="ACADEMIC.SCHEDULE.CREATE_WEEKLY"||x.Action=="ACADEMIC.SCHEDULE.MOVE").ToArrayAsync(Ct));}
    }
    [Fact(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    public async Task NewEntryResourceWithoutClassRvChangeInvalidatesWholeWeek()
    {
        await using var f=await OwnedSqlFixture.CreateAsync(new M1AdmissionsSqlTests.Clock());var(t,id)=await Setup(f);var body=await WeeklyBody(f,t,id);var key=Guid.NewGuid();var session=new EnglishCenter.Api.Infrastructure.Academics.SqlEntrySessionExecutor(()=>f.Db(),f.Clock,10,1000);
        var dto=ClassWire.Object(new{date=f.Clock.GetToday().AddDays(2).ToString("yyyy-MM-dd"),startTime="09:30",endTime="10:30",room="WEEK_ROOM",graderId=f.Accounts["Teacher"],capacity=20});await session.CommandAsync(t,EnglishCenter.Api.Services.Academics.AcademicInput.Parse(55,0,Guid.NewGuid(),dto.ToJsonString(),65536),Ct);
        Assert.Equal(ApiErrorCode.STATE_CONFLICT,await Error(()=>M2ClassSqlTests.Ex(f).ScheduleCommand(t,120,id,key,body.ToJsonString(),65536,Ct)));await using var db=f.Db();Assert.Empty(await db.Schedules.ToArrayAsync(Ct));Assert.Equal(body["expectedVersions"]!["class"]!.GetValue<string>(),ClassWire.Rv((await db.Classes.SingleAsync(Ct)).RowVersion));Assert.False(await db.ReplayOperations.AnyAsync(x=>x.RequestId==key,Ct));
    }
    [Fact(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    public async Task NewAssignmentWithoutClassRvChangeInvalidatesWeeklyCohortManifest()
    {
        await using var f=await OwnedSqlFixture.CreateAsync(new M1AdmissionsSqlTests.Clock());var(t,p,r)=await M2ClassSqlTests.Seed(f);var id=await M2ClassSqlTests.Create(f,t,p);await M2ClassSqlTests.Schedule(f,t,id,M2ClassSqlTests.Window(f,days:4));await M2ClassSqlTests.Open(f,t,id);var body=await WeeklyBody(f,t,id,Rule(f,1));await Place(f,t,id,r);var key=Guid.NewGuid();Assert.Equal(ApiErrorCode.STATE_CONFLICT,await Error(()=>M2ClassSqlTests.Ex(f).ScheduleCommand(t,120,id,key,body.ToJsonString(),65536,Ct)));await using var db=f.Db();Assert.Single(await db.Schedules.ToArrayAsync(Ct));Assert.Equal(body["expectedVersions"]!["class"]!.GetValue<string>(),ClassWire.Rv((await db.Classes.SingleAsync(Ct)).RowVersion));Assert.False(await db.ReplayOperations.AnyAsync(x=>x.RequestId==key,Ct));
    }
    [Theory(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    [InlineData(false,false)][InlineData(false,true)][InlineData(true,false)][InlineData(true,true)]
    public async Task NewAttendanceOrLinkedScoreBlocksMoveAndReplacementWithoutParentTokenChange(bool score,bool replace)
    {
        var time=new M1AdmissionsSqlTests.Clock();await using var f=await OwnedSqlFixture.CreateAsync(time);var(t,p,r)=await M2ClassSqlTests.Seed(f);var id=await M2ClassSqlTests.Create(f,t,p);await M2ClassSqlTests.Schedule(f,t,id,M2ClassSqlTests.Window(f));if(score)await M2ClassSqlTests.Schedule(f,t,id,M2ClassSqlTests.Window(f,"FINAL_EXAM",start:"11:00",end:"12:00"));await M2ClassSqlTests.Open(f,t,id);await Place(f,t,id,r);
        int schedule;await using(var db=f.Db()){var old=await db.Schedules.SingleAsync(x=>x.SessionType==(score?"FINAL_EXAM":"LESSON"),Ct);schedule=old.ScheduleId;if(replace)time.Now=new DateTimeOffset(DateTime.SpecifyKind(ClassWire.Window(old).StartAt,DateTimeKind.Utc));}
        if(replace)t=f.Token(await f.Grant("AcademicAffairs"));var ex=M2ClassSqlTests.Ex(f);var body=await EditBody(f,t,schedule,Draft(f,3,replace),replace);
        await using(var db=f.Db()){await using var tx=await db.Database.BeginTransactionAsync(Ct);foreach(var gate in new[]{"EC:CLASS:"+id,"EC:PROGRAM:"+p})await db.Database.ExecuteSqlInterpolatedAsync($"EXEC sys.sp_getapplock @Resource={gate},@LockMode=N'Exclusive',@LockOwner=N'Transaction',@DbPrincipal=N'public';",Ct);var assignment=await db.StudentClassAssignments.SingleAsync(Ct);if(score)db.Scores.Add(new(){ClassAssignmentId=assignment.ClassAssignmentId,ComponentId=(await db.ScoreComponentConfigs.SingleAsync(x=>x.ProgramId==p,Ct)).ComponentId,ScheduleId=schedule,ScoreValue=0,EnteredBy=f.Accounts["Teacher"],EnteredAt=time.Now.UtcDateTime});else db.Attendances.Add(new(){ClassAssignmentId=assignment.ClassAssignmentId,ScheduleId=schedule,Present=false,EnteredBy=f.Accounts["Teacher"],EnteredAt=time.Now.UtcDateTime});await db.SaveChangesAsync(Ct);await tx.CommitAsync(Ct);}
        var key=Guid.NewGuid();Assert.Equal(ApiErrorCode.STATE_CONFLICT,await Error(()=>ex.ScheduleCommand(t,replace?124:122,schedule,key,body.ToJsonString(),65536,Ct)));await using(var db=f.Db()){var old=await db.Schedules.SingleAsync(x=>x.ScheduleId==schedule,Ct);Assert.True(old.IsCurrent);Assert.Equal(body["expectedVersions"]!["schedule"]!.GetValue<string>(),ClassWire.Rv(old.RowVersion));Assert.False(await db.ReplayOperations.AnyAsync(x=>x.RequestId==key,Ct));}
    }
    [Fact(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    public async Task ScopedCalendarAndDetailHaveExactPrivacyForeign404AndCurrentTeacherOwnership()
    {
        await using var f=await OwnedSqlFixture.CreateAsync(new M1AdmissionsSqlTests.Clock());var(t,p,r)=await M2ClassSqlTests.Seed(f);var id=await M2ClassSqlTests.Create(f,t,p);await M2ClassSqlTests.Schedule(f,t,id,M2ClassSqlTests.Window(f));await M2ClassSqlTests.Open(f,t,id);await Place(f,t,id,r);var ex=M2ClassSqlTests.Ex(f);var foreign=await M2ClassSqlTests.Create(f,t,p,"FOREIGN");int sid;await using(var db=f.Db())sid=(await db.Schedules.SingleAsync(Ct)).ScheduleId;
        var query=new QueryCollection(new Dictionary<string,Microsoft.Extensions.Primitives.StringValues>{{"fromDate",f.Clock.GetToday().ToString("yyyy-MM-dd")}});
        foreach(var role in new[]{"Teacher","Student"}){var caller=f.Token(await f.Grant(role));var page=await ex.OwnCalendar(caller,query,role,Ct);Assert.Single(page["items"]!.AsArray());var d=await ex.OwnScheduleDetail(caller,sid,role,Ct);Assert.Equal(16,d.Count);foreach(var key in new[]{"version","held","notHeld","dependencies","allowedActions","disabledReasons"})Assert.False(d.ContainsKey(key));if(role=="Student"){Assert.Single(d["class"]!["teacher"]!.AsObject());Assert.Equal(ApiErrorCode.NOT_FOUND,await Error(()=>ex.OwnCalendar(caller,new QueryCollection(new Dictionary<string,Microsoft.Extensions.Primitives.StringValues>{{"classId",foreign.ToString()}}),role,Ct)));}Assert.Equal(ApiErrorCode.VALIDATION_ERROR,await Error(()=>ex.OwnCalendar(caller,new QueryCollection(new Dictionary<string,Microsoft.Extensions.Primitives.StringValues>{{"teacherId","1"}}),role,Ct)));}
        await using(var db=f.Db()){var other=await db.Accounts.SingleAsync(x=>x.AccountId==f.Accounts["Director"],Ct);other.RoleId=(await db.Roles.SingleAsync(x=>x.RoleCode=="Teacher",Ct)).RoleId;var changed=await db.Classes.SingleAsync(x=>x.ClassId==id,Ct);changed.TeacherId=other.AccountId;await db.SaveChangesAsync(Ct);}
        Assert.Empty((await ex.OwnCalendar(f.Token(await f.Grant("Teacher")),query,"Teacher",Ct))["items"]!.AsArray());Assert.Equal(ApiErrorCode.NOT_FOUND,await Error(async()=>await ex.OwnScheduleDetail(f.Token(await f.Grant("Teacher")),sid,"Teacher",Ct)));
    }
    [Fact(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    public async Task WholeWeekRootsAuditsOneMarkerAndMatchedReplay()
    {
        await using var f=await OwnedSqlFixture.CreateAsync(new M1AdmissionsSqlTests.Clock());var(t,id)=await Setup(f);var b=await WeeklyBody(f,t,id);var key=Guid.NewGuid();var ex=M2ClassSqlTests.Ex(f);
        var first=await ex.ScheduleCommand(t,120,id,key,b.ToJsonString(),65536,Ct);Assert.Equal(2,first.Result["created"]!.AsArray().Count);Assert.False(first.Replayed);
        var again=await ex.ScheduleCommand(t,120,id,key,b.ToJsonString(),65536,Ct);Assert.True(again.Replayed);Assert.Equal(first.Result.ToJsonString(),again.Result.ToJsonString());
        Assert.Equal(id,(await ex.Lookup(t,"ACADEMIC.SCHEDULE.CREATE_WEEKLY",key,Ct)).Target["classId"]);
        await using var db=f.Db();var rows=await db.Schedules.OrderBy(x=>x.SessionDate).ToArrayAsync(Ct);Assert.Equal(2,rows.Length);Assert.Equal(2,rows.Select(x=>x.ObligationKey).Distinct().Count());Assert.All(rows,x=>{Assert.True(x.IsCurrent);Assert.Null(x.ReplacesScheduleId);});Assert.Equal(2,await db.AuditLogs.CountAsync(x=>x.Action=="ACADEMIC.SCHEDULE.CREATE_WEEKLY",Ct));Assert.Single(await db.ReplayOperations.Where(x=>x.RequestId==key).ToArrayAsync(Ct));
    }
    [Theory(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    [InlineData("omit")][InlineData("type")][InlineData("room")][InlineData("reverse")]
    public async Task RulePlanMismatchIs400WithoutPartialEffects(string change)
    {
        await using var f=await OwnedSqlFixture.CreateAsync(new M1AdmissionsSqlTests.Clock());var(t,id)=await Setup(f);var b=await WeeklyBody(f,t,id);var a=b["candidates"]!.AsArray();if(change=="omit")a.RemoveAt(1);else if(change=="type")a[0]!["type"]="FINAL_EXAM";else if(change=="room")a[0]!["room"]="CHANGED";else{var first=a[0]!.DeepClone();a[0]=a[1]!.DeepClone();a[1]=first;}
        var key=Guid.NewGuid();Assert.Equal(ApiErrorCode.VALIDATION_ERROR,await Error(()=>M2ClassSqlTests.Ex(f).ScheduleCommand(t,120,id,key,b.ToJsonString(),65536,Ct)));await using var db=f.Db();Assert.Empty(await db.Schedules.ToArrayAsync(Ct));Assert.False(await db.ReplayOperations.AnyAsync(x=>x.RequestId==key,Ct));
    }
    [Fact(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    public async Task EmptyOverCountAndOneConflictBlockEntirePreview()
    {
        await using var f=await OwnedSqlFixture.CreateAsync(new M1AdmissionsSqlTests.Clock());var(t,id)=await Setup(f);var ex=M2ClassSqlTests.Ex(f);var rule=Rule(f,1);rule["weekdays"]=new JsonArray(((((int)f.Clock.GetToday().AddDays(1).DayOfWeek+6)%7+1)%7)+1);
        var empty=await ex.WeeklyPreview(t,id,new JsonObject{["rule"]=rule}.ToJsonString(),Ct);Assert.Equal(0,empty["candidateCount"]!.GetValue<int>());Assert.False(empty["eligible"]!.GetValue<bool>());Assert.Null(empty["manifest"]);
        var over=await ex.WeeklyPreview(t,id,new JsonObject{["rule"]=Rule(f,3)}.ToJsonString(),Ct);Assert.Equal(3,over["candidateCount"]!.GetValue<int>());Assert.Null(over["manifest"]);
        await M2ClassSqlTests.Schedule(f,t,id,M2ClassSqlTests.Window(f,room:"WEEK_ROOM"));var blocked=await ex.WeeklyPreview(t,id,new JsonObject{["rule"]=Rule(f)}.ToJsonString(),Ct);Assert.Equal(2,blocked["candidateCount"]!.GetValue<int>());Assert.Null(blocked["manifest"]);Assert.Single(blocked["conflicts"]!.AsArray());
    }
    [Fact(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    public async Task AuditFailureAndBudgetExhaustionRollbackWholeWeek()
    {
        await using var f=await OwnedSqlFixture.CreateAsync(new M1AdmissionsSqlTests.Clock());var(t,id)=await Setup(f);var b=await WeeklyBody(f,t,id);var key=Guid.NewGuid();await using(var hold=await f.HoldAudit())await Assert.ThrowsAsync<EnglishCenter.Api.Infrastructure.Auditing.MandatoryAuditException>(()=>M2ClassSqlTests.Ex(f).ScheduleCommand(t,120,id,key,b.ToJsonString(),65536,Ct));
        var limited=new SqlClassExecutor(()=>f.Db(),f.Clock,10,1000,1);Assert.Equal(ApiErrorCode.SERVICE_UNAVAILABLE,await Error(()=>limited.WeeklyPreview(t,id,new JsonObject{["rule"]=Rule(f)}.ToJsonString(),Ct)));Assert.Equal(ApiErrorCode.SERVICE_UNAVAILABLE,await Error(()=>limited.ScheduleCommand(t,120,id,Guid.NewGuid(),b.ToJsonString(),65536,Ct)));await using var db=f.Db();Assert.Empty(await db.Schedules.ToArrayAsync(Ct));Assert.False(await db.ReplayOperations.AnyAsync(x=>x.RequestId==key,Ct));
    }
    [Fact(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    public async Task WeeklyRaceAndNewResourceInsertCannotPartiallyWin()
    {
        await using var f=await OwnedSqlFixture.CreateAsync(new M1AdmissionsSqlTests.Clock());var(t,id)=await Setup(f);var b=await WeeklyBody(f,t,id);var ex=M2ClassSqlTests.Ex(f);
        async Task<bool> Send(){try{await ex.ScheduleCommand(t,120,id,Guid.NewGuid(),b.ToJsonString(),65536,Ct);return true;}catch(ApiErrorException e){Assert.Equal(ApiErrorCode.STATE_CONFLICT,e.Code);return false;}}
        Assert.Single(await Task.WhenAll(Send(),Send()),x=>x);await using var db=f.Db();Assert.Equal(2,await db.Schedules.CountAsync(Ct));Assert.Single(await db.ReplayOperations.Where(x=>x.ActionCode=="ACADEMIC.SCHEDULE.CREATE_WEEKLY").ToArrayAsync(Ct));
    }
    [Theory(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    [InlineData("LESSON")][InlineData("FINAL_EXAM")]
    public async Task SameRowMoveExcludesOnlySelfKeepsIdentityAndNoopHasNoMarker(string type)
    {
        await using var f=await OwnedSqlFixture.CreateAsync(new M1AdmissionsSqlTests.Clock());var(t,id)=await Setup(f);await M2ClassSqlTests.Schedule(f,t,id,M2ClassSqlTests.Window(f));if(type=="FINAL_EXAM")await M2ClassSqlTests.Schedule(f,t,id,M2ClassSqlTests.Window(f,type,start:"11:00",end:"12:00"));
        await using var db=f.Db();var old=await db.Schedules.AsNoTracking().SingleAsync(x=>x.SessionType==type,Ct);var draft=ClassWire.Object(new{newDate=old.SessionDate.ToString("yyyy-MM-dd"),startTime=type=="LESSON"?"09:30":"11:30",endTime=type=="LESSON"?"10:30":"12:30",room=old.Room});var b=await EditBody(f,t,old.ScheduleId,draft);var ex=M2ClassSqlTests.Ex(f);var key=Guid.NewGuid();var result=await ex.ScheduleCommand(t,122,old.ScheduleId,key,b.ToJsonString(),65536,Ct);Assert.Equal(old.ScheduleId,result.Result["schedule"]!["id"]!.GetValue<int>());Assert.Equal(old.ObligationKey.ToString(),result.Result["schedule"]!["obligationKey"]!.GetValue<string>());Assert.True((await ex.ScheduleCommand(t,122,old.ScheduleId,key,b.ToJsonString(),65536,Ct)).Replayed);
        var noop=await EditBody(f,t,old.ScheduleId,draft);var failed=Guid.NewGuid();Assert.Equal(ApiErrorCode.VALIDATION_ERROR,await Error(()=>ex.ScheduleCommand(t,122,old.ScheduleId,failed,noop.ToJsonString(),65536,Ct)));Assert.False(await db.ReplayOperations.AnyAsync(x=>x.RequestId==failed,Ct));Assert.Equal(type=="LESSON"?1:2,await db.Schedules.CountAsync(Ct));
    }
    [Theory(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    [InlineData("LESSON")][InlineData("FINAL_EXAM")]
    public async Task ReplacementAtBoundarySameKeyFullChainReplayAndAuditRollback(string type)
    {
        var time=new M1AdmissionsSqlTests.Clock();await using var f=await OwnedSqlFixture.CreateAsync(time);var(t,id)=await Setup(f);await M2ClassSqlTests.Schedule(f,t,id,M2ClassSqlTests.Window(f));if(type=="FINAL_EXAM")await M2ClassSqlTests.Schedule(f,t,id,M2ClassSqlTests.Window(f,type,start:"11:00",end:"12:00"));
        int oldId;Guid key;DateTime start;await using(var db=f.Db()){var old=await db.Schedules.SingleAsync(x=>x.SessionType==type,Ct);oldId=old.ScheduleId;key=old.ObligationKey;start=ClassWire.Window(old).StartAt;}
        time.Now=new DateTimeOffset(DateTime.SpecifyKind(start,DateTimeKind.Utc));t=f.Token(await f.Grant("AcademicAffairs"));var ex=M2ClassSqlTests.Ex(f);var draft=Draft(f,1,true);var b=await EditBody(f,t,oldId,draft,true);var request=Guid.NewGuid();
        await using(var hold=await f.HoldAudit())await Assert.ThrowsAsync<EnglishCenter.Api.Infrastructure.Auditing.MandatoryAuditException>(()=>ex.ScheduleCommand(t,124,oldId,request,b.ToJsonString(),65536,Ct));await using(var db=f.Db()){var old=await db.Schedules.SingleAsync(x=>x.ScheduleId==oldId,Ct);Assert.True(old.IsCurrent);Assert.Equal("PLANNED",old.OccurrenceState);Assert.Null(old.NotHeldBy);}
        var result=await ex.ScheduleCommand(t,124,oldId,request,b.ToJsonString(),65536,Ct);var next=result.Result["newScheduleId"]!.GetValue<int>();Assert.True((await ex.ScheduleCommand(t,124,oldId,request,b.ToJsonString(),65536,Ct)).Replayed);Assert.Equal(next,(await ex.Lookup(t,"ACADEMIC.SCHEDULE.REPLACE",request,Ct)).Target["newScheduleId"]);
        var detail=await ex.ScheduleDetail(t,oldId,Ct);Assert.Equal(next,detail["currentScheduleId"]!.GetValue<int>());Assert.Equal(2,detail["replacementChain"]!.AsArray().Count);
        await using(var db=f.Db()){var rows=await db.Schedules.Where(x=>x.ObligationKey==key).ToArrayAsync(Ct);Assert.Equal(2,rows.Length);Assert.Single(rows,x=>x.IsCurrent);Assert.Equal(oldId,rows.Single(x=>x.IsCurrent).ReplacesScheduleId);Assert.Equal("NOT_HELD",rows.Single(x=>!x.IsCurrent).OccurrenceState);Assert.Equal(2,await db.AuditLogs.CountAsync(x=>x.OperationId==Guid.Parse(result.Result["operationId"]!.GetValue<string>()),Ct));}
        Assert.False((await ex.EditPreview(t,oldId,Draft(f,2,true).ToJsonString(),true,Ct))["eligible"]!.GetValue<bool>());
    }
}
internal static class ScheduleTestDraft
{
    internal static JsonObject WithReason(this JsonObject draft,bool replace){if(replace)draft["reason"]="Cả lớp không tổ chức buổi này.";return draft;}
}
