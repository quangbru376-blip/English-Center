namespace EnglishCenter.Tests.Api.Integration;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Common.Security;
using EnglishCenter.Api.Infrastructure.Classes;
using EnglishCenter.Api.Services.Classes;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Xunit;

[Collection("V01 HTTP SQL")]
public sealed class M3TeachingSqlTests
{
    private const string OptIn="Assigned M3 batch1 real owned scratch SQL/HTTP, ID/GUID cleanup.";
    private static CancellationToken Ct=>OwnedSqlFixture.Ct;
    internal static async Task<(VerifiedSessionToken Academic,VerifiedSessionToken Teacher,int Class,int[] Lessons,int Exam)> Seed(OwnedSqlFixture f,bool exam=true)
    {
        var(t,p,r)=await M2ClassSqlTests.Seed(f);var id=await M2ClassSqlTests.Create(f,t,p);
        var local=f.Clock.GetUtcNow().UtcDateTime.AddHours(7);var hour=new DateTime(local.Year,local.Month,local.Day,local.Hour,0,0);
        JsonObject Window(string type,int start,int end)=>ClassWire.Object(new{type,date=hour.ToString("yyyy-MM-dd"),startTime=hour.AddMinutes(start).ToString("HH:mm"),endTime=hour.AddMinutes(end).ToString("HH:mm"),room="CLASS_ROOM"});
        foreach(var(start,end) in new[]{(10,20),(30,40)})await M2ClassSqlTests.Schedule(f,t,id,Window("LESSON",start,end));
        if(exam)await M2ClassSqlTests.Schedule(f,t,id,Window("FINAL_EXAM",50,59));
        await M2ClassSqlTests.Open(f,t,id);var ex=M2ClassSqlTests.Ex(f);var preview=await ex.Preview(t,id,"placement",$"{{\"registrationId\":{r}}}",Ct);
        var b=preview["manifest"]!.DeepClone().AsObject();b["registrationId"]=r;await ex.Command(t,108,id,Guid.NewGuid(),b.ToJsonString(),65536,Ct);
        var teacher=f.Token(await f.Grant("Teacher"));await using var db=f.Db();var rows=await db.Schedules.Where(x=>x.ClassId==id).OrderBy(x=>x.ScheduleId).ToArrayAsync(Ct);
        return(t,teacher,id,rows.Where(x=>x.SessionType=="LESSON").Select(x=>x.ScheduleId).ToArray(),rows.SingleOrDefault(x=>x.SessionType=="FINAL_EXAM")?.ScheduleId??0);
    }
    internal static async Task<JsonObject> Delta(OwnedSqlFixture f,VerifiedSessionToken t,int id,int schedule,int value=0)
    {
        var sheet=await M2ClassSqlTests.Ex(f).AttendanceSheet(t,id,schedule,"Teacher",Ct);var b=sheet["writeContext"]!.DeepClone().AsObject();
        var rows=sheet["rows"]!.AsArray();b["changes"]=new JsonArray(rows.Select(x=>(JsonNode?)new JsonObject{["classAssignmentId"]=x!["classAssignmentId"]!.DeepClone(),["present"]=value}).ToArray());
        b["expectedVersions"]!["attendance"]=new JsonArray(rows.Where(x=>x!["attendance"] is not null).Select(x=>(JsonNode?)new JsonObject{["id"]=x!["attendance"]!["id"]!.DeepClone(),["version"]=x["attendance"]!["version"]!.DeepClone()}).ToArray());
        b["expectedSet"]!["attendanceBindings"]=new JsonArray(rows.Where(x=>x!["attendance"] is not null).Select(x=>(JsonNode?)new JsonObject{["classAssignmentId"]=x!["classAssignmentId"]!.DeepClone(),["attendanceId"]=x["attendance"]!["id"]!.DeepClone()}).ToArray());
        b["expectedSet"]!["unrecordedAssignmentIds"]=new JsonArray(rows.Where(x=>x!["attendance"] is null).Select(x=>x!["classAssignmentId"]!.DeepClone()).ToArray());return b;
    }
    internal static async Task Start(OwnedSqlFixture f,VerifiedSessionToken t,int id)
    {var ex=M2ClassSqlTests.Ex(f);var p=await ex.Preview(t,id,"transition","{\"action\":\"START\"}",Ct);Assert.True(p["eligible"]!.GetValue<bool>(),p.ToJsonString());await ex.Command(t,100,id,Guid.NewGuid(),p["manifest"]!.ToJsonString(),65536,Ct);}
    internal static async Task Held(OwnedSqlFixture f,VerifiedSessionToken t,int schedule)
    {var ex=M2ClassSqlTests.Ex(f);var p=await ex.HeldPreview(t,schedule,Ct);Assert.True(p["eligible"]!.GetValue<bool>(),p.ToJsonString());await ex.ScheduleCommand(t,126,schedule,Guid.NewGuid(),p["manifest"]!.ToJsonString(),65536,Ct);}
    private static async Task<ApiErrorCode> Error(Func<Task> task)=>(await Assert.ThrowsAsync<ApiErrorException>(task)).Code;
    [Fact(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    public async Task StartNeedsFullLessonsAndCohortButNoExam_ClockDoesNotStartAutomatically()
    {
        var clock=new M1AdmissionsSqlTests.Clock();await using var f=await OwnedSqlFixture.CreateAsync(clock);var(t,_,id,_,_)=await Seed(f,false);var ex=M2ClassSqlTests.Ex(f);
        Assert.False((await ex.Preview(t,id,"transition","{\"action\":\"START\"}",Ct))["eligible"]!.GetValue<bool>());
        clock.Now=clock.Now.AddMinutes(10);t=f.Token(await f.Grant("AcademicAffairs"));Assert.Equal("OPEN",(await ex.Detail(t,id,"AcademicAffairs",Ct))["status"]!.GetValue<string>());await Start(f,t,id);
        Assert.Equal("ONGOING",(await ex.Detail(t,id,"AcademicAffairs",Ct))["status"]!.GetValue<string>());
        Assert.False((await ex.Preview(t,id,"transition","{\"action\":\"COMPLETE\"}",Ct))["eligible"]!.GetValue<bool>());
        await using var db=f.Db();Assert.Empty(await db.Attendances.ToArrayAsync(Ct));Assert.True((await db.Registrations.SingleAsync(Ct)).IsUnfinished);
    }
    [Fact(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    public async Task HeldAtEndIndependentOfAttendanceAndRepeatedCommitHasOneAudit()
    {
        var time=new M1AdmissionsSqlTests.Clock();await using var f=await OwnedSqlFixture.CreateAsync(time);var(t,_,id,lessons,_)=await Seed(f);time.Now=time.Now.AddMinutes(10);await Start(f,t,id);var ex=M2ClassSqlTests.Ex(f);
        Assert.False((await ex.HeldPreview(t,lessons[0],Ct))["eligible"]!.GetValue<bool>());time.Now=time.Now.AddMinutes(10);t=f.Token(await f.Grant("AcademicAffairs"));
        var p=await ex.HeldPreview(t,lessons[0],Ct);var b=p["manifest"]!.ToJsonString();var key=Guid.NewGuid();var result=await ex.ScheduleCommand(t,126,lessons[0],key,b,65536,Ct);Assert.True((await ex.ScheduleCommand(t,126,lessons[0],key,b,65536,Ct)).Replayed);
        var lookup=await ex.TeachingLookup(t,"AcademicAffairs","ACADEMIC.SCHEDULE.CONFIRM_HELD",key,Ct);Assert.Equal(result.Result["operationId"]!.GetValue<Guid>(),lookup.OperationId);
        await using var db=f.Db();Assert.Empty(await db.Attendances.ToArrayAsync(Ct));Assert.Equal(1,await db.AuditLogs.CountAsync(x=>x.Action=="ACADEMIC.SCHEDULE.CONFIRM_HELD",Ct));Assert.Equal("ONGOING",(await db.Classes.SingleAsync(Ct)).Status);
    }
    [Fact(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    public async Task OpenClassAttendanceFromStart_MissingNotAbsent_NoNewAttendanceAuditOrGradeTouch()
    {
        var time=new M1AdmissionsSqlTests.Clock();await using var f=await OwnedSqlFixture.CreateAsync(time);var(_,t,id,lessons,_)=await Seed(f);var ex=M2ClassSqlTests.Ex(f);
        var sheet=await ex.AttendanceSheet(t,id,lessons[0],"Teacher",Ct);Assert.Null(sheet["rows"]![0]!["attendance"]);Assert.Null(sheet["writeContext"]);
        time.Now=time.Now.AddMinutes(10);var b=await Delta(f,t,id,lessons[0]);byte[] reportRv;await using(var db=f.Db())reportRv=(await db.GradeReports.SingleAsync(Ct)).RowVersion;
        var key=Guid.NewGuid();var result=await ex.AttendanceCommand(t,id,lessons[0],key,b.ToJsonString(),65536,Ct);Assert.Equal(0,result.Result["sheet"]!["rows"]![0]!["attendance"]!["present"]!.GetValue<int>());
        Assert.True((await ex.AttendanceCommand(t,id,lessons[0],key,b.ToJsonString(),65536,Ct)).Replayed);
        await using var check=f.Db();Assert.Equal("OPEN",(await check.Classes.SingleAsync(Ct)).Status);Assert.Equal(reportRv,(await check.GradeReports.SingleAsync(Ct)).RowVersion);Assert.False(await check.AuditLogs.AnyAsync(x=>x.EntityType=="Attendance",Ct));Assert.Single(await check.Attendances.ToArrayAsync(Ct));
    }
    [Fact(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    public async Task AllEqualNoMarker_StaleEqualAndAbsentConcurrentInsertRefuse_NoProvenanceOverwrite()
    {
        var time=new M1AdmissionsSqlTests.Clock();await using var f=await OwnedSqlFixture.CreateAsync(time);var(_,t,id,lessons,_)=await Seed(f);time.Now=time.Now.AddMinutes(10);var ex=M2ClassSqlTests.Ex(f);
        var absent=await Delta(f,t,id,lessons[0]);await ex.AttendanceCommand(t,id,lessons[0],Guid.NewGuid(),absent.ToJsonString(),65536,Ct);
        Assert.Equal(ApiErrorCode.STATE_CONFLICT,await Error(()=>ex.AttendanceCommand(t,id,lessons[0],Guid.NewGuid(),absent.ToJsonString(),65536,Ct)));
        var equal=await Delta(f,t,id,lessons[0]);var key=Guid.NewGuid();Assert.Equal(ApiErrorCode.VALIDATION_ERROR,await Error(()=>ex.AttendanceCommand(t,id,lessons[0],key,equal.ToJsonString(),65536,Ct)));
        var edit=await Delta(f,t,id,lessons[0],1);await ex.AttendanceCommand(t,id,lessons[0],Guid.NewGuid(),edit.ToJsonString(),65536,Ct);
        Assert.Equal(ApiErrorCode.STATE_CONFLICT,await Error(()=>ex.AttendanceCommand(t,id,lessons[0],Guid.NewGuid(),equal.ToJsonString(),65536,Ct)));
        await using var db=f.Db();Assert.True((await db.Attendances.SingleAsync(Ct)).Present);Assert.False(await db.ReplayOperations.AnyAsync(x=>x.RequestId==key,Ct));
    }
    [Fact(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    public async Task CompleteRequiresAllHeldAndLessonCoverage_RecordedAbsentValid_ExamHasNoAttendance()
    {
        var time=new M1AdmissionsSqlTests.Clock();await using var f=await OwnedSqlFixture.CreateAsync(time);var(t,teacher,id,lessons,exam)=await Seed(f);time.Now=time.Now.AddMinutes(10);await Start(f,t,id);
        time.Now=time.Now.AddMinutes(50);t=f.Token(await f.Grant("AcademicAffairs"));teacher=f.Token(await f.Grant("Teacher"));foreach(var sid in lessons.Append(exam))await Held(f,t,sid);
        var ex=M2ClassSqlTests.Ex(f);Assert.Null((await ex.Preview(t,id,"transition","{\"action\":\"COMPLETE\"}",Ct))["manifest"]);
        foreach(var sid in lessons)await ex.AttendanceCommand(teacher,id,sid,Guid.NewGuid(),(await Delta(f,teacher,id,sid)).ToJsonString(),65536,Ct);
        var p=await ex.Preview(t,id,"transition","{\"action\":\"COMPLETE\"}",Ct);Assert.True(p["eligible"]!.GetValue<bool>());var key=Guid.NewGuid();await ex.Command(t,101,id,key,p["manifest"]!.ToJsonString(),65536,Ct);Assert.True((await ex.Command(t,101,id,key,p["manifest"]!.ToJsonString(),65536,Ct)).Replayed);
        var sheet=await ex.AttendanceSheet(teacher,id,lessons[0],"Teacher",Ct);Assert.Null(sheet["writeContext"]);Assert.False(sheet["editing"]!["canEdit"]!.GetValue<bool>());
        await using var db=f.Db();Assert.Equal("COMPLETED",(await db.Classes.SingleAsync(Ct)).Status);Assert.Equal(2,await db.Attendances.CountAsync(Ct));Assert.False(await db.Attendances.AnyAsync(x=>x.ScheduleId==exam,Ct));Assert.Equal("DRAFT",(await db.GradeReports.SingleAsync(Ct)).Status);Assert.True((await db.Registrations.SingleAsync(Ct)).IsUnfinished);
    }
    [Fact(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    public async Task MandatoryHeldAuditFailureRollsBackOccurrenceAndT23()
    {
        var time=new M1AdmissionsSqlTests.Clock();await using var f=await OwnedSqlFixture.CreateAsync(time);var(t,_,id,lessons,_)=await Seed(f);time.Now=time.Now.AddMinutes(10);await Start(f,t,id);time.Now=time.Now.AddMinutes(10);var ex=M2ClassSqlTests.Ex(f);var p=await ex.HeldPreview(t,lessons[0],Ct);var key=Guid.NewGuid();
        await using(var hold=await f.HoldAudit())await Assert.ThrowsAsync<EnglishCenter.Api.Infrastructure.Auditing.MandatoryAuditException>(()=>ex.ScheduleCommand(t,126,lessons[0],key,p["manifest"]!.ToJsonString(),65536,Ct));
        await using var db=f.Db();Assert.Equal("PLANNED",(await db.Schedules.SingleAsync(x=>x.ScheduleId==lessons[0],Ct)).OccurrenceState);Assert.False(await db.ReplayOperations.AnyAsync(x=>x.RequestId==key,Ct));
    }
    [Fact(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    public async Task ConcurrentAbsentInsertOnlyOneWins_NoSilentUpsert()
    {
        var time=new M1AdmissionsSqlTests.Clock();await using var f=await OwnedSqlFixture.CreateAsync(time);var(_,t,id,lessons,_)=await Seed(f);time.Now=time.Now.AddMinutes(10);var b=await Delta(f,t,id,lessons[0]);
        async Task<bool> Save(){try{await M2ClassSqlTests.Ex(f).AttendanceCommand(t,id,lessons[0],Guid.NewGuid(),b.ToJsonString(),65536,Ct);return true;}catch(ApiErrorException e){Assert.Equal(ApiErrorCode.STATE_CONFLICT,e.Code);return false;}}
        var outcomes=await Task.WhenAll(Save(),Save());Assert.Single(outcomes,x=>x);await using var db=f.Db();Assert.Single(await db.Attendances.ToArrayAsync(Ct));Assert.Equal(1,await db.ReplayOperations.CountAsync(x=>x.ActionCode=="TEACHER.ATTENDANCE.SAVE_DELTA",Ct));
    }
    [Fact(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    public async Task AttendanceCorrectionVersusCompleteOnlyOneWins_WithFullChildVersions()
    {
        var time=new M1AdmissionsSqlTests.Clock();await using var f=await OwnedSqlFixture.CreateAsync(time);var(t,teacher,id,lessons,exam)=await Seed(f);time.Now=time.Now.AddMinutes(10);await Start(f,t,id);time.Now=time.Now.AddMinutes(50);t=f.Token(await f.Grant("AcademicAffairs"));teacher=f.Token(await f.Grant("Teacher"));var ex=M2ClassSqlTests.Ex(f);foreach(var sid in lessons.Append(exam))await Held(f,t,sid);foreach(var sid in lessons)await ex.AttendanceCommand(teacher,id,sid,Guid.NewGuid(),(await Delta(f,teacher,id,sid)).ToJsonString(),65536,Ct);
        var complete=(await ex.Preview(t,id,"transition","{\"action\":\"COMPLETE\"}",Ct))["manifest"]!.ToJsonString();var delta=(await Delta(f,teacher,id,lessons[0],1)).ToJsonString();
        async Task<bool> Send(bool finish){try{if(finish)await M2ClassSqlTests.Ex(f).Command(t,101,id,Guid.NewGuid(),complete,65536,Ct);else await M2ClassSqlTests.Ex(f).AttendanceCommand(teacher,id,lessons[0],Guid.NewGuid(),delta,65536,Ct);return true;}catch(ApiErrorException e){Assert.Equal(ApiErrorCode.STATE_CONFLICT,e.Code);return false;}}
        var results=await Task.WhenAll(Send(true),Send(false));Assert.Single(results,x=>x);await using var db=f.Db();var cl=await db.Classes.SingleAsync(Ct);var attendance=await db.Attendances.SingleAsync(x=>x.ScheduleId==lessons[0],Ct);Assert.Equal(cl.Status=="ONGOING",attendance.Present);
    }
    [Fact(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    public async Task HttpTeacherAndAcademicExactPrivacy_OriginCsrfStrictScalarAndManualLookup()
    {
        var time=new M1AdmissionsSqlTests.Clock();await using var f=await OwnedSqlFixture.CreateAsync(time);var(_,teacher,id,lessons,exam)=await Seed(f);time.Now=time.Now.AddMinutes(10);var b=await Delta(f,teacher,id,lessons[0]);
        await using var host=await M1HttpSqlTests.Host.Create(f);await host.Login("Teacher");var path=$"/api/v1/academics/teachers/me/classes/{id}/lessons/{lessons[0]}/attendance";
        using(var noCsrf=await host.Send(HttpMethod.Put,path,b,Guid.NewGuid(),false))Assert.Equal(HttpStatusCode.Forbidden,noCsrf.StatusCode);
        var invalid=b.DeepClone();invalid["changes"]![0]!["present"]=true;using(var bad=await host.Send(HttpMethod.Put,path,invalid,Guid.NewGuid()))Assert.Equal(HttpStatusCode.BadRequest,bad.StatusCode);
        var key=Guid.NewGuid();using(var saved=await host.Send(HttpMethod.Put,path,b,key))Assert.Equal(HttpStatusCode.OK,saved.StatusCode);
        using(var lookup=await host.Send(HttpMethod.Get,$"/api/v1/academics/command-results/{key:D}?action=TEACHER.ATTENDANCE.SAVE_DELTA"))Assert.Equal(HttpStatusCode.OK,lookup.StatusCode);
        using(var foreign=await host.Send(HttpMethod.Get,$"/api/v1/academics/teachers/me/classes/{id}/lessons/{exam}/attendance"))Assert.Equal(HttpStatusCode.NotFound,foreign.StatusCode);
        await host.Login("AcademicAffairs");using(var read=await host.Send(HttpMethod.Get,$"/api/v1/academics/classes/{id}/lessons/{lessons[0]}/attendance")){Assert.Equal(HttpStatusCode.OK,read.StatusCode);var result=await read.Content.ReadFromJsonAsync<JsonObject>(Ct);Assert.False(result!.ContainsKey("editing"));Assert.False(result.ContainsKey("writeContext"));Assert.False(result["rows"]![0]!["attendance"]!.AsObject().ContainsKey("version"));}
        using(var wrong=await host.Send(HttpMethod.Put,path,b,Guid.NewGuid()))Assert.Equal(HttpStatusCode.Forbidden,wrong.StatusCode);
    }
    [Fact(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    public async Task MixedBatchValidatesEqualStaleRowsAndPreservesEqualProvenance_NoPartialSave()
    {
        var time=new M1AdmissionsSqlTests.Clock();await using var f=await OwnedSqlFixture.CreateAsync(time);var(t,teacher,id,lessons,_)=await Seed(f);
        int first;await using(var db=f.Db())first=(await db.StudentClassAssignments.SingleAsync(Ct)).RegistrationId;
        var second=await M2ClassSqlTests.SecondPaid(f,first);var ex=M2ClassSqlTests.Ex(f);var preview=await ex.Preview(t,id,"placement",$"{{\"registrationId\":{second}}}",Ct);var placement=preview["manifest"]!.DeepClone().AsObject();placement["registrationId"]=second;await ex.Command(t,108,id,Guid.NewGuid(),placement.ToJsonString(),65536,Ct);
        time.Now=time.Now.AddMinutes(10);var all=await Delta(f,teacher,id,lessons[0],0);var one=all.DeepClone().AsObject();one["changes"]!.AsArray().RemoveAt(1);one["expectedSet"]!["unrecordedAssignmentIds"]!.AsArray().RemoveAt(1);
        await ex.AttendanceCommand(teacher,id,lessons[0],Guid.NewGuid(),one.ToJsonString(),65536,Ct);
        var mixed=await Delta(f,teacher,id,lessons[0],0);byte[] rv;DateTime at;await using(var db=f.Db()){var row=await db.Attendances.SingleAsync(Ct);rv=row.RowVersion;at=row.EnteredAt;}
        var saved=await ex.AttendanceCommand(teacher,id,lessons[0],Guid.NewGuid(),mixed.ToJsonString(),65536,Ct);Assert.Single(saved.Result["saved"]!.AsArray());
        var stale=await Delta(f,teacher,id,lessons[0],0);var fresh=await Delta(f,teacher,id,lessons[0],1);fresh["changes"]!.AsArray().RemoveAt(1);fresh["expectedVersions"]!["attendance"]!.AsArray().RemoveAt(1);fresh["expectedSet"]!["attendanceBindings"]!.AsArray().RemoveAt(1);await ex.AttendanceCommand(teacher,id,lessons[0],Guid.NewGuid(),fresh.ToJsonString(),65536,Ct);
        stale["changes"]![1]!["present"]=1;Assert.Equal(ApiErrorCode.STATE_CONFLICT,await Error(()=>ex.AttendanceCommand(teacher,id,lessons[0],Guid.NewGuid(),stale.ToJsonString(),65536,Ct)));
        await using var check=f.Db();var rows=await check.Attendances.OrderBy(x=>x.ClassAssignmentId).ToArrayAsync(Ct);Assert.True(rows[0].Present);Assert.False(rows[1].Present);Assert.Equal(at,rows[0].EnteredAt);Assert.NotEqual(rv,rows[0].RowVersion);
    }
    [Fact(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    public async Task HeldVersusReplacementSerializesOneOccurrence_NoForkOrDoubleConfirmation()
    {
        var time=new M1AdmissionsSqlTests.Clock();await using var f=await OwnedSqlFixture.CreateAsync(time);var(t,_,id,_,exam)=await Seed(f);time.Now=time.Now.AddMinutes(10);await Start(f,t,id);time.Now=time.Now.AddMinutes(50);t=f.Token(await f.Grant("AcademicAffairs"));var ex=M2ClassSqlTests.Ex(f);
        var held=(await ex.HeldPreview(t,exam,Ct))["manifest"]!.ToJsonString();var draft=ClassWire.Object(new{newDate=f.Clock.GetToday().ToString("yyyy-MM-dd"),startTime="11:10",endTime="11:20",room="NEW_ROOM",reason="Synthetic not held"});
        var p=await ex.EditPreview(t,exam,draft.ToJsonString(),true,Ct);Assert.True(p["eligible"]!.GetValue<bool>(),p.ToJsonString());var replacement=p["manifest"]!.DeepClone().AsObject();foreach(var pair in draft)replacement[pair.Key]=pair.Value?.DeepClone();
        async Task<bool> Send(bool confirm){try{await M2ClassSqlTests.Ex(f).ScheduleCommand(t,confirm?126:124,exam,Guid.NewGuid(),confirm?held:replacement.ToJsonString(),65536,Ct);return true;}catch(ApiErrorException e){Assert.Equal(ApiErrorCode.STATE_CONFLICT,e.Code);return false;}}
        var results=await Task.WhenAll(Send(true),Send(false));Assert.Single(results,x=>x);await using var db=f.Db();var old=await db.Schedules.SingleAsync(x=>x.ScheduleId==exam,Ct);Assert.Contains(old.OccurrenceState,new[]{"HELD","NOT_HELD"});Assert.Equal(1,await db.Schedules.CountAsync(x=>x.ObligationKey==old.ObligationKey&&x.IsCurrent,Ct));
    }
    [Fact(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    public async Task CompleteAuditFailureRollsBackStatusAndMarker_WithRecordedAbsentCoverage()
    {
        var time=new M1AdmissionsSqlTests.Clock();await using var f=await OwnedSqlFixture.CreateAsync(time);var(t,teacher,id,lessons,exam)=await Seed(f);time.Now=time.Now.AddMinutes(10);await Start(f,t,id);time.Now=time.Now.AddMinutes(50);t=f.Token(await f.Grant("AcademicAffairs"));teacher=f.Token(await f.Grant("Teacher"));var ex=M2ClassSqlTests.Ex(f);foreach(var sid in lessons.Append(exam))await Held(f,t,sid);foreach(var sid in lessons)await ex.AttendanceCommand(teacher,id,sid,Guid.NewGuid(),(await Delta(f,teacher,id,sid)).ToJsonString(),65536,Ct);
        var body=(await ex.Preview(t,id,"transition","{\"action\":\"COMPLETE\"}",Ct))["manifest"]!.ToJsonString();var key=Guid.NewGuid();await using(var hold=await f.HoldAudit())await Assert.ThrowsAsync<EnglishCenter.Api.Infrastructure.Auditing.MandatoryAuditException>(()=>ex.Command(t,101,id,key,body,65536,Ct));
        await using var db=f.Db();Assert.Equal("ONGOING",(await db.Classes.SingleAsync(Ct)).Status);Assert.False(await db.ReplayOperations.AnyAsync(x=>x.RequestId==key,Ct));Assert.Equal(2,await db.Attendances.CountAsync(Ct));
    }
    [Fact(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    public async Task CurrentTeacherReassignmentDeniesSheetSaveAndPriorMarkerLookup()
    {
        var time=new M1AdmissionsSqlTests.Clock();await using var f=await OwnedSqlFixture.CreateAsync(time);var(_,t,id,lessons,_)=await Seed(f);time.Now=time.Now.AddMinutes(10);var ex=M2ClassSqlTests.Ex(f);var b=await Delta(f,t,id,lessons[0]);var key=Guid.NewGuid();await ex.AttendanceCommand(t,id,lessons[0],key,b.ToJsonString(),65536,Ct);
        // A privileged fixture changes assignment under the same CLASS gate to prove current task rights, without exposing a post-start reassignment API.
        await using(var db=f.Db()){await using var tx=await db.Database.BeginTransactionAsync(Ct);await db.Database.ExecuteSqlInterpolatedAsync($"EXEC sys.sp_getapplock @Resource={"EC:CLASS:"+id},@LockMode='Exclusive',@LockOwner='Transaction',@DbPrincipal='public';",Ct);var role=await db.Roles.SingleAsync(x=>x.RoleCode=="Teacher",Ct);var account=new EnglishCenter.Api.Domain.Identity.Account{RoleId=role.RoleId,FullName="Synthetic replacement",Email="replacement@example.invalid",PasswordHash=f.Hasher.Hash(f.Password),Status="ACTIVE",SecurityVersion=1,CreatedAt=f.Clock.GetUtcNow().UtcDateTime};db.Accounts.Add(account);await db.SaveChangesAsync(Ct);(await db.Classes.SingleAsync(Ct)).TeacherId=account.AccountId;await db.SaveChangesAsync(Ct);await tx.CommitAsync(Ct);}
        Assert.Equal(ApiErrorCode.NOT_FOUND,await Error(()=>ex.AttendanceSheet(t,id,lessons[0],"Teacher",Ct)));Assert.Equal(ApiErrorCode.NOT_FOUND,await Error(()=>ex.AttendanceCommand(t,id,lessons[0],Guid.NewGuid(),b.ToJsonString(),65536,Ct)));await Assert.ThrowsAsync<ApiErrorException>(()=>ex.TeachingLookup(t,"Teacher","TEACHER.ATTENDANCE.SAVE_DELTA",key,Ct));
    }
    [Fact(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    public async Task AttendanceInsertFinalDeadlineFailureRollsBackDataAndT23()
    {
        var time=new M1AdmissionsSqlTests.Clock();await using var f=await OwnedSqlFixture.CreateAsync(time);var(_,t,id,lessons,_)=await Seed(f);time.Now=time.Now.AddMinutes(10);var b=await Delta(f,t,id,lessons[0]);var advance=new ExpireOnInsert(time);var ex=new SqlClassExecutor(()=>f.Db(advance),f.Clock,10,1000);var key=Guid.NewGuid();
        Assert.Equal(ApiErrorCode.AUTHENTICATION_REQUIRED,await Error(()=>ex.AttendanceCommand(t,id,lessons[0],key,b.ToJsonString(),65536,Ct)));Assert.True(advance.Fired);await using var db=f.Db();Assert.Empty(await db.Attendances.ToArrayAsync(Ct));Assert.False(await db.ReplayOperations.AnyAsync(x=>x.RequestId==key,Ct));
    }
    private sealed class ExpireOnInsert(M1AdmissionsSqlTests.Clock clock):Microsoft.EntityFrameworkCore.Diagnostics.DbCommandInterceptor
    {
        public bool Fired{get;private set;}
        public override ValueTask<System.Data.Common.DbDataReader> ReaderExecutedAsync(System.Data.Common.DbCommand command,Microsoft.EntityFrameworkCore.Diagnostics.CommandExecutedEventData data,System.Data.Common.DbDataReader reader,CancellationToken ct=default)
        {if(!Fired&&command.CommandText.Contains("[Attendance]",StringComparison.Ordinal)&&command.CommandText.Contains("INSERT",StringComparison.Ordinal)){Fired=true;clock.Now=clock.Now.AddHours(24);}return ValueTask.FromResult(reader);}
    }
    [Fact(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    public async Task TeacherAccountLockVersusAttendanceHasOrderedCommitOrRevocationRefusal()
    {
        var time=new M1AdmissionsSqlTests.Clock();await using var f=await OwnedSqlFixture.CreateAsync(time);var(_,t,id,lessons,_)=await Seed(f);time.Now=time.Now.AddMinutes(10);var ex=M2ClassSqlTests.Ex(f);var b=await Delta(f,t,id,lessons[0]);var admin=f.Token(await f.Grant("Admin"));var detail=await f.Admin.DetailAsync(admin,f.Accounts["Teacher"],Ct);
        var input=OwnedSqlFixture.LockInput(f.Accounts["Teacher"],detail.Version,Guid.NewGuid());
        async Task<bool> Save(){try{await ex.AttendanceCommand(t,id,lessons[0],Guid.NewGuid(),b.ToJsonString(),65536,Ct);return true;}catch(ApiErrorException e){Assert.Equal(ApiErrorCode.AUTHENTICATION_REQUIRED,e.Code);return false;}}
        var save=Save();var locked=f.Admin.LockAsync(admin,input,Ct);var persisted=await save;await locked;
        Assert.Equal(ApiErrorCode.AUTHENTICATION_REQUIRED,await Error(()=>ex.AttendanceSheet(t,id,lessons[0],"Teacher",Ct)));
        await using var db=f.Db();Assert.Equal("LOCKED",(await db.Accounts.SingleAsync(x=>x.AccountId==f.Accounts["Teacher"],Ct)).Status);Assert.Equal(persisted?1:0,await db.Attendances.CountAsync(Ct));Assert.Equal(persisted?1:0,await db.ReplayOperations.CountAsync(x=>x.ActionCode=="TEACHER.ATTENDANCE.SAVE_DELTA",Ct));Assert.False(await db.AuthSessions.AnyAsync(x=>x.AccountId==f.Accounts["Teacher"]&&x.RevokedAt==null,Ct));
    }
    [Theory(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    [InlineData(false)][InlineData(true)]
    public async Task FinalLowerClockBoundaryRollsBackStartOrHeldEvenWithValidActorDeadline(bool held)
    {
        var time=new M1AdmissionsSqlTests.Clock();await using var f=await OwnedSqlFixture.CreateAsync(time);var(t,_,id,lessons,_)=await Seed(f);time.Now=time.Now.AddMinutes(10);var ex=M2ClassSqlTests.Ex(f);
        if(held){await Start(f,t,id);time.Now=time.Now.AddMinutes(10);}
        var preview=held?await ex.HeldPreview(t,lessons[0],Ct):await ex.Preview(t,id,"transition","{\"action\":\"START\"}",Ct);
        var rewind=new RewindAfterUpdate(time,time.Now.AddMinutes(-1),held?"[Schedule]":"[Class]");var bounded=new SqlClassExecutor(()=>f.Db(rewind),f.Clock,10,1000);var key=Guid.NewGuid();
        Assert.Equal(ApiErrorCode.STATE_CONFLICT,await Error(()=>held?bounded.ScheduleCommand(t,126,lessons[0],key,preview["manifest"]!.ToJsonString(),65536,Ct):bounded.Command(t,100,id,key,preview["manifest"]!.ToJsonString(),65536,Ct)));
        Assert.True(rewind.Fired);await using var db=f.Db();Assert.Equal(held?"ONGOING":"OPEN",(await db.Classes.SingleAsync(Ct)).Status);Assert.Equal("PLANNED",(await db.Schedules.SingleAsync(x=>x.ScheduleId==lessons[0],Ct)).OccurrenceState);Assert.False(await db.ReplayOperations.AnyAsync(x=>x.RequestId==key,Ct));Assert.False(await db.AuditLogs.AnyAsync(x=>x.Action==(held?"ACADEMIC.SCHEDULE.CONFIRM_HELD":"ACADEMIC.CLASS.START"),Ct));
    }
    private sealed class RewindAfterUpdate(M1AdmissionsSqlTests.Clock clock,DateTimeOffset target,string table):Microsoft.EntityFrameworkCore.Diagnostics.DbCommandInterceptor
    {
        public bool Fired{get;private set;}
        public override ValueTask<System.Data.Common.DbDataReader> ReaderExecutedAsync(System.Data.Common.DbCommand command,Microsoft.EntityFrameworkCore.Diagnostics.CommandExecutedEventData data,System.Data.Common.DbDataReader reader,CancellationToken ct=default)
        {if(!Fired&&command.CommandText.Contains(table,StringComparison.Ordinal)&&command.CommandText.Contains("UPDATE",StringComparison.Ordinal)){Fired=true;clock.Now=target;}return ValueTask.FromResult(reader);}
    }
}
