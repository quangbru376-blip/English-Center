namespace EnglishCenter.Tests.Api.Integration;
using System.Net;
using System.Text.Json.Nodes;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Common.Security;
using EnglishCenter.Api.Infrastructure.Classes;
using EnglishCenter.Api.Services.Classes;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Xunit;

[Collection("V01 HTTP SQL")]
public sealed class M3GradingSqlTests
{
    private const string OptIn="Assigned M3 batch2 owned scratch grading SQL/HTTP; exact ID/GUID cleanup.";
    private static CancellationToken Ct=>OwnedSqlFixture.Ct;
    internal static async Task<(VerifiedSessionToken Academic,VerifiedSessionToken Teacher,int Class,int Assignment,int Component,int Exam)> Seed(OwnedSqlFixture f,M1AdmissionsSqlTests.Clock time,bool complete=true,bool second=false)
    {
        var(a,t,id,lessons,exam)=await M3TeachingSqlTests.Seed(f);
        if(second){int source;await using(var db=f.Db())source=(await db.StudentClassAssignments.SingleAsync(Ct)).RegistrationId;var other=await M2ClassSqlTests.SecondPaid(f,source);var ex2=M2ClassSqlTests.Ex(f);var p2=await ex2.Preview(a,id,"placement",$"{{\"registrationId\":{other}}}",Ct);var b2=p2["manifest"]!.DeepClone().AsObject();b2["registrationId"]=other;await ex2.Command(a,108,id,Guid.NewGuid(),b2.ToJsonString(),65536,Ct);}
        time.Now=time.Now.AddMinutes(10);await M3TeachingSqlTests.Start(f,a,id);time.Now=time.Now.AddMinutes(50);
        a=f.Token(await f.Grant("AcademicAffairs"));t=f.Token(await f.Grant("Teacher"));
        var ex=M2ClassSqlTests.Ex(f);
        if(complete){foreach(var s in lessons.Append(exam))await M3TeachingSqlTests.Held(f,a,s);foreach(var s in lessons)await ex.AttendanceCommand(t,id,s,Guid.NewGuid(),(await M3TeachingSqlTests.Delta(f,t,id,s)).ToJsonString(),65536,Ct);var p=await ex.Preview(a,id,"transition","{\"action\":\"COMPLETE\"}",Ct);await ex.Command(a,101,id,Guid.NewGuid(),p["manifest"]!.ToJsonString(),65536,Ct);}
        await using var finalDb=f.Db();return(a,t,id,(await finalDb.StudentClassAssignments.OrderBy(x=>x.ClassAssignmentId).FirstAsync(Ct)).ClassAssignmentId,(await finalDb.ScoreComponentConfigs.SingleAsync(Ct)).ComponentId,exam);
    }
    internal static async Task<JsonObject> Delta(OwnedSqlFixture f,int id,int assignment,int component,int? schedule,string score="0",string? comment=null)
    {
        await using var db=f.Db();var g=await SqlGradeRepository.Read(db,id,Ct);var b=GradeWire.DraftManifest(g,assignment,[component],schedule is {} sid?[sid]:[]);
        b["scores"]=new JsonArray(ClassWire.Object(new{componentId=component,score,scheduleId=schedule}));if(comment is not null)b["teacherComment"]=comment;return b;
    }
    private static async Task<ApiErrorCode> Error(Func<Task> work)=>(await Assert.ThrowsAsync<ApiErrorException>(work)).Code;
    [Theory(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    [InlineData(141)][InlineData(142)][InlineData(146)]
    public async Task WholeWorkflowRejectsNewCohortMemberWithoutClassRvChange(int api)
    {
        var time=new M1AdmissionsSqlTests.Clock();await using var f=await OwnedSqlFixture.CreateAsync(time);var(a,t,id,assignment,component,exam)=await Seed(f,time);var ex=M2ClassSqlTests.Ex(f);await ex.GradeCommand(t,139,id,assignment,Guid.NewGuid(),(await Delta(f,id,assignment,component,exam,"75")).ToJsonString(),65536,Ct);
        var p=await ex.SubmissionPreview(t,id,"{\"action\":\"SUBMIT\"}",Ct);
        if(api!=141){await ex.GradeCommand(t,141,id,0,Guid.NewGuid(),p["manifest"]!.ToJsonString(),65536,Ct);p=await ex.ReviewPreview(a,id,"{\"action\":\"RETURN\"}",Ct);if(api==142){var r=p["manifest"]!.DeepClone().AsObject();r["reason"]="Trả để sửa";await ex.GradeCommand(a,146,id,0,Guid.NewGuid(),r.ToJsonString(),65536,Ct);p=await ex.SubmissionPreview(t,id,"{\"action\":\"RESUBMIT\"}",Ct);}}
        var body=p["manifest"]!.DeepClone().AsObject();if(api==146)body["reason"]="Trả cả lớp";int source;await using(var db=f.Db())source=(await db.StudentClassAssignments.SingleAsync(Ct)).RegistrationId;var second=await M2ClassSqlTests.SecondPaid(f,source);
        // Owned privileged fixture introduces a member under the protocol gates; no new placement API after teaching.
        await using(var db=f.Db()){await using var tx=await db.Database.BeginTransactionAsync(Ct);var program=(await db.Classes.SingleAsync(Ct)).ProgramId;foreach(var gate in new[]{"EC:CLASS:"+id,"EC:PROGRAM:"+program,"EC:REG:"+second}.Order(StringComparer.Ordinal))await db.Database.ExecuteSqlInterpolatedAsync($"EXEC sys.sp_getapplock @Resource={gate},@LockMode='Exclusive',@LockOwner='Transaction',@DbPrincipal='public';",Ct);var member=new EnglishCenter.Api.Domain.Training.StudentClassAssignment{ClassId=id,RegistrationId=second,AssignedBy=f.Accounts["AcademicAffairs"],AssignedAt=time.Now.UtcDateTime};db.StudentClassAssignments.Add(member);await db.SaveChangesAsync(Ct);db.GradeReports.Add(new(){ClassAssignmentId=member.ClassAssignmentId,Status="DRAFT"});await db.SaveChangesAsync(Ct);await tx.CommitAsync(Ct);}
        var key=Guid.NewGuid();Assert.Equal(ApiErrorCode.STATE_CONFLICT,await Error(()=>ex.GradeCommand(api==146?a:t,api,id,0,key,body.ToJsonString(),65536,Ct)));await using var check=f.Db();Assert.False(await check.ReplayOperations.AnyAsync(x=>x.RequestId==key,Ct));Assert.Equal(2,await check.GradeReports.CountAsync(Ct));
    }
    [Fact(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    public async Task ConcurrentSameKeySubmitHasOneHistory_ConcurrentFreshReturnHasOneWinner()
    {
        var time=new M1AdmissionsSqlTests.Clock();await using var f=await OwnedSqlFixture.CreateAsync(time);var(a,t,id,assignment,component,exam)=await Seed(f,time);var ex=M2ClassSqlTests.Ex(f);await ex.GradeCommand(t,139,id,assignment,Guid.NewGuid(),(await Delta(f,id,assignment,component,exam,"75")).ToJsonString(),65536,Ct);var key=Guid.NewGuid();var body=(await ex.SubmissionPreview(t,id,"{\"action\":\"SUBMIT\"}",Ct))["manifest"]!.ToJsonString();
        var submitted=await Task.WhenAll(ex.GradeCommand(t,141,id,0,key,body,65536,Ct),M2ClassSqlTests.Ex(f).GradeCommand(t,141,id,0,key,body,65536,Ct));Assert.Single(submitted,x=>x.Replayed);Assert.Equal(submitted[0].Result["operationId"]!.GetValue<Guid>(),submitted[1].Result["operationId"]!.GetValue<Guid>());
        var p=await ex.ReviewPreview(a,id,"{\"action\":\"RETURN\"}",Ct);var b=p["manifest"]!.DeepClone().AsObject();b["reason"]="Sửa cả lớp";
        async Task<bool> Return(){try{await M2ClassSqlTests.Ex(f).GradeCommand(a,146,id,0,Guid.NewGuid(),b.ToJsonString(),65536,Ct);return true;}catch(ApiErrorException e){Assert.Equal(ApiErrorCode.STATE_CONFLICT,e.Code);return false;}}
        Assert.Single(await Task.WhenAll(Return(),Return()),x=>x);await using var check=f.Db();Assert.Equal(2,await check.AuditLogs.CountAsync(x=>x.Action=="TEACHER.CLASS.SUBMIT_GRADES"||x.Action=="TEACHER.CLASS.SUBMIT_GRADES.EFFECT",Ct));Assert.Equal(2,await check.AuditLogs.CountAsync(x=>x.Action=="ACADEMIC.CLASS.RETURN_GRADES"||x.Action=="ACADEMIC.CLASS.RETURN_GRADES.EFFECT",Ct));
    }
    private sealed class FinalClock(M1AdmissionsSqlTests.Clock clock,DateTimeOffset cutoff):Microsoft.EntityFrameworkCore.Diagnostics.DbCommandInterceptor
    {
        private bool changed;public bool Fired{get;private set;}
        public override ValueTask<System.Data.Common.DbDataReader> ReaderExecutedAsync(System.Data.Common.DbCommand command,Microsoft.EntityFrameworkCore.Diagnostics.CommandExecutedEventData data,System.Data.Common.DbDataReader reader,CancellationToken ct=default)
        {
            if(command.CommandText.Contains("[Score]",StringComparison.Ordinal)&&command.CommandText.Contains("INSERT",StringComparison.Ordinal))changed=true;
            if(changed&&!Fired&&(cutoff<clock.Now||command.CommandText.Contains("ufn_GradeTotal",StringComparison.Ordinal))){clock.Now=cutoff;Fired=true;}
            return ValueTask.FromResult(reader);
        }
    }
    [Theory(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    [InlineData(0)][InlineData(1)][InlineData(2)]
    public async Task FinalProjectionClockCutoffRollsBackScoreCommentAuditAndT23(int cutoffMode)
    {
        var time=new M1AdmissionsSqlTests.Clock();var start=time.Now;await using var f=await OwnedSqlFixture.CreateAsync(time);var(_,_,id,assignment,component,exam)=await Seed(f,time,complete:false);
        time.Now=start.AddMinutes(40);var t=f.Token(await f.Grant("Teacher"));time.Now=start.AddMinutes(60);
        var b=await Delta(f,id,assignment,component,exam,"75","Rollback cutoff");var key=Guid.NewGuid();var probe=new FinalClock(time,cutoffMode==2?start.AddHours(4):cutoffMode==1?start.AddMinutes(71):start.AddMinutes(49));
        var ex=new SqlClassExecutor(()=>f.Db(probe),f.Clock,10,1000);
        Assert.Equal(cutoffMode>0?ApiErrorCode.AUTHENTICATION_REQUIRED:ApiErrorCode.STATE_CONFLICT,await Error(()=>ex.GradeCommand(t,139,id,assignment,key,b.ToJsonString(),65536,Ct)));Assert.True(probe.Fired);
        await using var db=f.Db();Assert.Empty(await db.Scores.ToArrayAsync(Ct));Assert.Null((await db.GradeReports.SingleAsync(Ct)).TeacherComment);Assert.False(await db.ReplayOperations.AnyAsync(x=>x.RequestId==key,Ct));Assert.False(await db.AuditLogs.AnyAsync(x=>x.Action.StartsWith("TEACHER.GRADE_DRAFT"),Ct));
    }
    [Fact(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    public async Task GeneralAndLinkedPartialDraft_Fn05HalfUpOnce_MixedEqualProvenance_CommentAtomic()
    {
        var time=new M1AdmissionsSqlTests.Clock();await using var f=await OwnedSqlFixture.CreateAsync(time);var(a,program,registration)=await M2ClassSqlTests.Seed(f);
        var programs=new EnglishCenter.Api.Infrastructure.Academics.SqlProgramExecutor(()=>f.Db(),f.Clock,10,1000);
        async Task Apply(int api,JsonObject b)=>await programs.CommandAsync(a,EnglishCenter.Api.Services.Academics.AcademicInput.Parse(api,program,Guid.NewGuid(),b.ToJsonString(),65536),Ct);
        await using(var db=f.Db()){var p=await EnglishCenter.Api.Infrastructure.Academics.SqlProgramRepository.ReadState(db,program,Ct);await Apply(88,ClassWire.Object(new{expectedVersions=new{program=ClassWire.Rv(p.Program.RowVersion)}}));}
        int general,examComponent;await using(var db=f.Db()){var p=await EnglishCenter.Api.Infrastructure.Academics.SqlProgramRepository.ReadState(db,program,Ct);var b=ClassWire.Object(new{courseId=p.Course.CourseId,bandId=p.Band.BandId,name=p.Program.ProgramName,expectedLessonCount=2,components=new object[]{new{id=(int?)p.Components[0].ComponentId,name="Final",weight="50.00",kind="FINAL_EXAM"},new{id=(int?)null,name="General",weight="50.00",kind="GENERAL"}},expectedVersions=new{program=ClassWire.Rv(p.Program.RowVersion),course=ClassWire.Rv(p.Course.RowVersion),band=ClassWire.Rv(p.Band.RowVersion),existingComponents=EnglishCenter.Api.Services.Academics.ProgramProjection.References(p)},expectedSet=new{existingComponentIds=p.Components.Select(x=>x.ComponentId)}});await Apply(84,b);}
        var activation=await programs.PreviewAsync(a,program,Ct);await Apply(87,System.Text.Json.JsonSerializer.SerializeToNode(activation.Manifest,new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web))!.AsObject());
        var id=await M2ClassSqlTests.Create(f,a,program);foreach(var(type,start,end) in new[]{("LESSON","10:10","10:20"),("LESSON","10:30","10:40"),("FINAL_EXAM","10:50","10:59")})await M2ClassSqlTests.Schedule(f,a,id,M2ClassSqlTests.Window(f,type,start,end,days:0));await M2ClassSqlTests.Open(f,a,id);var ex=M2ClassSqlTests.Ex(f);var place=await ex.Preview(a,id,"placement",$"{{\"registrationId\":{registration}}}",Ct);var pb=place["manifest"]!.DeepClone().AsObject();pb["registrationId"]=registration;await ex.Command(a,108,id,Guid.NewGuid(),pb.ToJsonString(),65536,Ct);time.Now=time.Now.AddMinutes(10);await M3TeachingSqlTests.Start(f,a,id);
        int assignment,exam;await using(var db=f.Db()){assignment=(await db.StudentClassAssignments.SingleAsync(Ct)).ClassAssignmentId;general=(await db.ScoreComponentConfigs.SingleAsync(x=>x.Kind=="GENERAL",Ct)).ComponentId;examComponent=(await db.ScoreComponentConfigs.SingleAsync(x=>x.Kind=="FINAL_EXAM",Ct)).ComponentId;exam=(await db.Schedules.SingleAsync(x=>x.SessionType=="FINAL_EXAM",Ct)).ScheduleId;}
        var t=f.Token(await f.Grant("Teacher"));await ex.GradeCommand(t,139,id,assignment,Guid.NewGuid(),(await Delta(f,id,assignment,general,null,"49.99")).ToJsonString(),65536,Ct);Assert.Null((await ex.GradeDraft(t,id,assignment,Ct))["computation"]!["total"]);
        Assert.False((await ex.GradeOptions(t,id,examComponent,Ct))["items"]![0]!["eligible"]!.GetValue<bool>());var early=(await Delta(f,id,assignment,examComponent,exam,"50")).ToJsonString();Assert.Equal(ApiErrorCode.STATE_CONFLICT,await Error(()=>ex.GradeCommand(t,139,id,assignment,Guid.NewGuid(),early,65536,Ct)));
        time.Now=time.Now.AddMinutes(40);t=f.Token(await f.Grant("Teacher"));await ex.GradeCommand(t,139,id,assignment,Guid.NewGuid(),(await Delta(f,id,assignment,examComponent,exam,"50.00")).ToJsonString(),65536,Ct);
        Assert.Equal("50.00",(await ex.GradeDraft(t,id,assignment,Ct))["computation"]!["total"]!.GetValue<string>());Assert.False((await ex.SubmissionPreview(t,id,"{\"action\":\"SUBMIT\"}",Ct))["eligible"]!.GetValue<bool>());
        var malformed=(await Delta(f,id,assignment,general,exam,"49.99")).ToJsonString();Assert.Equal(ApiErrorCode.VALIDATION_ERROR,await Error(()=>ex.GradeCommand(t,139,id,assignment,Guid.NewGuid(),malformed,65536,Ct)));
        byte[] oldRv;DateTime oldAt;await using(var db=f.Db()){var old=await db.Scores.SingleAsync(x=>x.ComponentId==general,Ct);oldRv=old.RowVersion;oldAt=old.EnteredAt;var state=await SqlGradeRepository.Read(db,id,Ct);var mixed=GradeWire.DraftManifest(state,assignment,[general,examComponent],[exam]);mixed["scores"]=new JsonArray(ClassWire.Object(new{componentId=general,score="49.99",scheduleId=(int?)null}),ClassWire.Object(new{componentId=examComponent,score="49.99",scheduleId=(int?)exam}));mixed["teacherComment"]="Mixed changed";await ex.GradeCommand(t,139,id,assignment,Guid.NewGuid(),mixed.ToJsonString(),65536,Ct);}
        await using var check=f.Db();var unchanged=await check.Scores.SingleAsync(x=>x.ComponentId==general,Ct);Assert.Equal(oldRv,unchanged.RowVersion);Assert.Equal(oldAt,unchanged.EnteredAt);Assert.Equal("49.99",(await ex.GradeDraft(t,id,assignment,Ct))["computation"]!["total"]!.GetValue<string>());Assert.Equal("ONGOING",(await check.Classes.SingleAsync(Ct)).Status);Assert.Null((await check.GradeReports.SingleAsync(Ct)).FinalScore);
    }
    [Fact(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    public async Task CommentOnlyIncomplete_NoRepair_SubmitRequiresFullCohort_StaleWorkflowRollback()
    {
        var time=new M1AdmissionsSqlTests.Clock();await using var f=await OwnedSqlFixture.CreateAsync(time);var(a,t,id,assignment,component,exam)=await Seed(f,time,second:true);var ex=M2ClassSqlTests.Ex(f);
        await using(var db=f.Db()){var g=await SqlGradeRepository.Read(db,id,Ct);var b=GradeWire.DraftManifest(g,assignment,[],[]);b["scores"]=new JsonArray();b["teacherComment"]="  Chưa đủ điểm  ";await ex.GradeCommand(t,139,id,assignment,Guid.NewGuid(),b.ToJsonString(),65536,Ct);}
        var blocked=await ex.SubmissionPreview(t,id,"{\"action\":\"SUBMIT\"}",Ct);Assert.False(blocked["eligible"]!.GetValue<bool>());Assert.Equal(2,blocked["issues"]!.AsArray().Count);
        int[] assignments;await using(var db=f.Db())assignments=await db.StudentClassAssignments.OrderBy(x=>x.ClassAssignmentId).Select(x=>x.ClassAssignmentId).ToArrayAsync(Ct);
        foreach(var target in assignments)await ex.GradeCommand(t,139,id,target,Guid.NewGuid(),(await Delta(f,id,target,component,exam,"50")).ToJsonString(),65536,Ct);
        var preview=await ex.SubmissionPreview(t,id,"{\"action\":\"SUBMIT\"}",Ct);Assert.Equal(2,preview["cohortSize"]!.GetValue<int>());
        await ex.GradeCommand(t,139,id,assignment,Guid.NewGuid(),(await Delta(f,id,assignment,component,exam,"51")).ToJsonString(),65536,Ct);
        Assert.Equal(ApiErrorCode.STATE_CONFLICT,await Error(()=>ex.GradeCommand(t,141,id,0,Guid.NewGuid(),preview["manifest"]!.ToJsonString(),65536,Ct)));
        preview=await ex.SubmissionPreview(t,id,"{\"action\":\"SUBMIT\"}",Ct);await ex.GradeCommand(t,141,id,0,Guid.NewGuid(),preview["manifest"]!.ToJsonString(),65536,Ct);
        var ret=await ex.ReviewPreview(a,id,"{\"action\":\"RETURN\"}",Ct);var b2=ret["manifest"]!.DeepClone().AsObject();b2["reason"]="Sửa toàn lớp";var key=Guid.NewGuid();await ex.GradeCommand(a,146,id,0,key,b2.ToJsonString(),65536,Ct);
        Assert.True((await ex.GradeCommand(a,146,id,0,key,b2.ToJsonString(),65536,Ct)).Replayed);
        await using var check=f.Db();Assert.All(await check.GradeReports.ToArrayAsync(Ct),x=>Assert.Equal("RETURNED",x.Status));var operation=(await check.GradeReports.FirstAsync(Ct)).ReviewOperationId;Assert.Equal(3,await check.AuditLogs.CountAsync(x=>x.OperationId==operation,Ct));
    }
    [Fact(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    public async Task MixedVisibleReviewNeverExposesDraftPeerValuesOrTokens_WholeReturnBlocked()
    {
        var time=new M1AdmissionsSqlTests.Clock();await using var f=await OwnedSqlFixture.CreateAsync(time);var(a,t,id,first,component,exam)=await Seed(f,time,second:true);var ex=M2ClassSqlTests.Ex(f);int[] assignments;await using(var db=f.Db())assignments=await db.StudentClassAssignments.Select(x=>x.ClassAssignmentId).ToArrayAsync(Ct);
        foreach(var assignment in assignments)await ex.GradeCommand(t,139,id,assignment,Guid.NewGuid(),(await Delta(f,id,assignment,component,exam,"75",assignment==first?"Visible":"PRIVATE DRAFT")).ToJsonString(),65536,Ct);
        var p=await ex.SubmissionPreview(t,id,"{\"action\":\"SUBMIT\"}",Ct);await ex.GradeCommand(t,141,id,0,Guid.NewGuid(),p["manifest"]!.ToJsonString(),65536,Ct);
        await using(var db=f.Db()){var hidden=await db.GradeReports.SingleAsync(x=>x.ClassAssignmentId!=first,Ct);hidden.Status="DRAFT";await db.SaveChangesAsync(Ct);}
        var detail=await ex.ReviewDetail(a,id,Ct);Assert.Single(detail["items"]!.AsArray());Assert.False(detail["hasCompleteVisibleCohort"]!.GetValue<bool>());Assert.Empty(detail["allowedActions"]!.AsArray());Assert.DoesNotContain("PRIVATE DRAFT",detail.ToJsonString());Assert.DoesNotContain("Synthetic Student2",detail.ToJsonString());var blocked=await ex.ReviewPreview(a,id,"{\"action\":\"RETURN\"}",Ct);Assert.Null(blocked["manifest"]);
    }
    [Theory(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    [InlineData(141)][InlineData(146)]
    public async Task WholeWorkflowAuditFailureRollsBackAllReportsAndMarker(int api)
    {
        var time=new M1AdmissionsSqlTests.Clock();await using var f=await OwnedSqlFixture.CreateAsync(time);var(a,t,id,_,component,exam)=await Seed(f,time,second:true);var ex=M2ClassSqlTests.Ex(f);int[] assignments;await using(var db=f.Db())assignments=await db.StudentClassAssignments.Select(x=>x.ClassAssignmentId).ToArrayAsync(Ct);foreach(var assignment in assignments)await ex.GradeCommand(t,139,id,assignment,Guid.NewGuid(),(await Delta(f,id,assignment,component,exam,"75")).ToJsonString(),65536,Ct);
        var p=await ex.SubmissionPreview(t,id,"{\"action\":\"SUBMIT\"}",Ct);if(api==146){await ex.GradeCommand(t,141,id,0,Guid.NewGuid(),p["manifest"]!.ToJsonString(),65536,Ct);p=await ex.ReviewPreview(a,id,"{\"action\":\"RETURN\"}",Ct);}var b=p["manifest"]!.DeepClone().AsObject();if(api==146)b["reason"]="Rollback all";var key=Guid.NewGuid();await using(var hold=await f.HoldAudit())await Assert.ThrowsAsync<EnglishCenter.Api.Infrastructure.Auditing.MandatoryAuditException>(()=>ex.GradeCommand(api==146?a:t,api,id,0,key,b.ToJsonString(),65536,Ct));
        await using var check=f.Db();Assert.All(await check.GradeReports.ToArrayAsync(Ct),r=>Assert.Equal(api==146?"SUBMITTED":"DRAFT",r.Status));Assert.False(await check.ReplayOperations.AnyAsync(x=>x.RequestId==key,Ct));
    }
    [Theory(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    [InlineData("LOCKED")][InlineData("REASSIGNED")][InlineData("EXPIRED")]
    public async Task CurrentTeacherAndSessionRefusedBeforeNewWriteAndReplay(string change)
    {
        var time=new M1AdmissionsSqlTests.Clock();await using var f=await OwnedSqlFixture.CreateAsync(time);var(_,t,id,assignment,component,exam)=await Seed(f,time);var ex=M2ClassSqlTests.Ex(f);var b=await Delta(f,id,assignment,component,exam,"75");var key=Guid.NewGuid();await ex.GradeCommand(t,139,id,assignment,key,b.ToJsonString(),65536,Ct);
        if(change=="EXPIRED")time.Now=time.Now.AddMinutes(31);else{await using var db=f.Db();var teacher=await db.Accounts.SingleAsync(x=>x.AccountId==f.Accounts["Teacher"],Ct);if(change=="LOCKED")teacher.Status="LOCKED";else{var other=new EnglishCenter.Api.Domain.Identity.Account{RoleId=teacher.RoleId,FullName="New Teacher",Email="other@example.invalid",PasswordHash=teacher.PasswordHash,Status="ACTIVE",SecurityVersion=1,CreatedAt=time.Now.UtcDateTime};db.Accounts.Add(other);await db.SaveChangesAsync(Ct);(await db.Classes.SingleAsync(Ct)).TeacherId=other.AccountId;}await db.SaveChangesAsync(Ct);}
        var expected=change=="REASSIGNED"?ApiErrorCode.NOT_FOUND:ApiErrorCode.AUTHENTICATION_REQUIRED;
        Assert.Equal(expected,await Error(()=>ex.GradeCommand(t,139,id,assignment,key,b.ToJsonString(),65536,Ct)));Assert.Equal(expected,await Error(()=>ex.TeachingLookup(t,"Teacher","TEACHER.GRADE_DRAFT.SAVE_DELTA",key,Ct)));
    }
    [Fact(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    public async Task DraftMissingNotZero_ParentTouch_Noop_ActualSqlTotal_Replay()
    {
        var time=new M1AdmissionsSqlTests.Clock();await using var f=await OwnedSqlFixture.CreateAsync(time);var(_,t,id,assignment,component,exam)=await Seed(f,time);var ex=M2ClassSqlTests.Ex(f);
        var before=await ex.GradeDraft(t,id,assignment,Ct);Assert.Null(before["computation"]!["total"]);var b=await Delta(f,id,assignment,component,exam,"0","Nhận xét");var key=Guid.NewGuid();
        var saved=await ex.GradeCommand(t,139,id,assignment,key,b.ToJsonString(),65536,Ct);var detail=saved.Result["detail"]!;
        Assert.Equal("0.00",detail["computation"]!["total"]!.GetValue<string>());Assert.NotEqual(before["report"]!["version"]!.GetValue<string>(),detail["report"]!["version"]!.GetValue<string>());Assert.Null(detail["report"]!["finalScore"]);
        Assert.True((await ex.GradeCommand(t,139,id,assignment,key,b.ToJsonString(),65536,Ct)).Replayed);
        var same=await Delta(f,id,assignment,component,exam,"0","Nhận xét");Assert.Equal(ApiErrorCode.NO_CHANGE,await Error(()=>ex.GradeCommand(t,139,id,assignment,Guid.NewGuid(),same.ToJsonString(),65536,Ct)));
        var lookup=await ex.TeachingLookup(t,"Teacher","TEACHER.GRADE_DRAFT.SAVE_DELTA",key,Ct);Assert.Equal(assignment,lookup.Target["classAssignmentId"]);
        await using var db=f.Db();Assert.Single(await db.Scores.ToArrayAsync(Ct));Assert.True((await db.Registrations.SingleAsync(Ct)).IsUnfinished);Assert.Equal(2,await db.AuditLogs.CountAsync(x=>x.OperationId==saved.Result["operationId"]!.GetValue<Guid>(),Ct));
    }
    [Fact(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    public async Task Submit_Return_Edit_Resubmit_SP03Privacy_WholeHistoryAndLockedAttendance()
    {
        var time=new M1AdmissionsSqlTests.Clock();await using var f=await OwnedSqlFixture.CreateAsync(time);var(a,t,id,assignment,component,exam)=await Seed(f,time);var ex=M2ClassSqlTests.Ex(f);
        Assert.Equal(ApiErrorCode.NOT_FOUND,await Error(()=>ex.ReviewDetail(a,id,Ct)));
        Assert.Empty((await ex.ReviewList(a,new QueryCollection(),Ct))["items"]!.AsArray());
        await ex.GradeCommand(t,139,id,assignment,Guid.NewGuid(),(await Delta(f,id,assignment,component,exam,"49.99","Giữ nhận xét")).ToJsonString(),65536,Ct);
        var preview=await ex.SubmissionPreview(t,id,"{\"action\":\"SUBMIT\"}",Ct);Assert.True(preview["eligible"]!.GetValue<bool>());var key=Guid.NewGuid();var body=preview["manifest"]!.ToJsonString();await ex.GradeCommand(t,141,id,0,key,body,65536,Ct);Assert.True((await ex.GradeCommand(t,141,id,0,key,body,65536,Ct)).Replayed);
        var detail=await ex.ReviewDetail(a,id,Ct);Assert.Equal("49.99",detail["items"]![0]!["computation"]!["total"]!.GetValue<string>());Assert.Null(detail["items"]![0]!["writeContext"]);Assert.Equal("SUBMITTED",detail["items"]![0]!["report"]!["status"]!.GetValue<string>());
        var returned=await ex.ReviewPreview(a,id,"{\"action\":\"RETURN\"}",Ct);var rb=returned["manifest"]!.DeepClone().AsObject();rb["reason"]="Bổ sung nhận xét";await ex.GradeCommand(a,146,id,0,Guid.NewGuid(),rb.ToJsonString(),65536,Ct);
        await ex.GradeCommand(t,139,id,assignment,Guid.NewGuid(),(await Delta(f,id,assignment,component,exam,"50.00")).ToJsonString(),65536,Ct);
        var resubmit=await ex.SubmissionPreview(t,id,"{\"action\":\"RESUBMIT\"}",Ct);await ex.GradeCommand(t,142,id,0,Guid.NewGuid(),resubmit["manifest"]!.ToJsonString(),65536,Ct);
        await using var db=f.Db();var r=await db.GradeReports.SingleAsync(Ct);Assert.Equal("SUBMITTED",r.Status);Assert.Equal("Bổ sung nhận xét",r.ReturnedReason);Assert.Equal("Giữ nhận xét",r.TeacherComment);Assert.Null(r.FinalScore);Assert.True((await db.Registrations.SingleAsync(Ct)).IsUnfinished);Assert.Equal(6,await db.AuditLogs.CountAsync(x=>x.Action.Contains("GRADES"),Ct));
    }
    [Fact(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    public async Task AuditFailureRollsBackScoreCommentReportAndCompletion()
    {
        var time=new M1AdmissionsSqlTests.Clock();await using var f=await OwnedSqlFixture.CreateAsync(time);var(_,t,id,assignment,component,exam)=await Seed(f,time);var ex=M2ClassSqlTests.Ex(f);var b=await Delta(f,id,assignment,component,exam,"75","Không lưu");var key=Guid.NewGuid();
        await using(var hold=await f.HoldAudit())await Assert.ThrowsAsync<EnglishCenter.Api.Infrastructure.Auditing.MandatoryAuditException>(()=>ex.GradeCommand(t,139,id,assignment,key,b.ToJsonString(),65536,Ct));
        await using var db=f.Db();Assert.Empty(await db.Scores.ToArrayAsync(Ct));Assert.Null((await db.GradeReports.SingleAsync(Ct)).TeacherComment);Assert.False(await db.ReplayOperations.AnyAsync(x=>x.RequestId==key,Ct));
    }
    [Fact(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    public async Task ConcurrentAbsentScoreOnlyOneWins_NoSilentMerge()
    {
        var time=new M1AdmissionsSqlTests.Clock();await using var f=await OwnedSqlFixture.CreateAsync(time);var(_,t,id,assignment,component,exam)=await Seed(f,time);var b=(await Delta(f,id,assignment,component,exam,"75")).ToJsonString();
        async Task<bool> Send(){try{await M2ClassSqlTests.Ex(f).GradeCommand(t,139,id,assignment,Guid.NewGuid(),b,65536,Ct);return true;}catch(ApiErrorException e){Assert.Equal(ApiErrorCode.STATE_CONFLICT,e.Code);return false;}}
        var results=await Task.WhenAll(Send(),Send());Assert.Single(results,x=>x);await using var db=f.Db();Assert.Single(await db.Scores.ToArrayAsync(Ct));
    }
    [Fact(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    public async Task SaveVersusSubmitOnlyOneWins_ParentRvProtectsWholeAggregate()
    {
        var time=new M1AdmissionsSqlTests.Clock();await using var f=await OwnedSqlFixture.CreateAsync(time);var(_,t,id,assignment,component,exam)=await Seed(f,time);var ex=M2ClassSqlTests.Ex(f);await ex.GradeCommand(t,139,id,assignment,Guid.NewGuid(),(await Delta(f,id,assignment,component,exam,"50")).ToJsonString(),65536,Ct);
        var write=(await Delta(f,id,assignment,component,exam,"75")).ToJsonString();var send=(await ex.SubmissionPreview(t,id,"{\"action\":\"SUBMIT\"}",Ct))["manifest"]!.ToJsonString();
        async Task<bool> Do(bool submit){try{await M2ClassSqlTests.Ex(f).GradeCommand(t,submit?141:139,id,submit?0:assignment,Guid.NewGuid(),submit?send:write,65536,Ct);return true;}catch(ApiErrorException e){Assert.Equal(ApiErrorCode.STATE_CONFLICT,e.Code);return false;}}
        var results=await Task.WhenAll(Do(true),Do(false));Assert.Single(results,x=>x);await using var db=f.Db();var r=await db.GradeReports.SingleAsync(Ct);Assert.Equal(r.Status=="SUBMITTED"?50m:75m,(await db.Scores.SingleAsync(Ct)).ScoreValue);
    }
    [Fact(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    public async Task HttpRolesCsrfShape_DraftPrecisionAndSafeLookup()
    {
        var time=new M1AdmissionsSqlTests.Clock();await using var f=await OwnedSqlFixture.CreateAsync(time);var(_,t,id,assignment,component,exam)=await Seed(f,time);var body=await Delta(f,id,assignment,component,exam,"49.99");await using var host=await M1HttpSqlTests.Host.Create(f);await host.Login("Teacher");var path=$"/api/v1/academics/teachers/me/classes/{id}/assignments/{assignment}/grade-draft";
        Assert.Equal(HttpStatusCode.Forbidden,(await host.Send(HttpMethod.Put,path,body,Guid.NewGuid(),false)).StatusCode);
        var invalid=body.DeepClone().AsObject();invalid["scores"]![0]!["score"]="49.999";Assert.Equal(HttpStatusCode.BadRequest,(await host.Send(HttpMethod.Put,path,invalid,Guid.NewGuid())).StatusCode);
        var key=Guid.NewGuid();Assert.Equal(HttpStatusCode.OK,(await host.Send(HttpMethod.Put,path,body,key)).StatusCode);
        Assert.Equal(HttpStatusCode.OK,(await host.Send(HttpMethod.Get,$"/api/v1/academics/command-results/{key}?action=TEACHER.GRADE_DRAFT.SAVE_DELTA")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden,(await host.Send(HttpMethod.Get,$"/api/v1/academics/grade-reviews/classes/{id}")).StatusCode);
        await host.Login("AcademicAffairs");Assert.Equal(HttpStatusCode.Forbidden,(await host.Send(HttpMethod.Get,path)).StatusCode);Assert.Equal(HttpStatusCode.NotFound,(await host.Send(HttpMethod.Get,$"/api/v1/academics/grade-reviews/classes/{id}")).StatusCode);
    }
}
