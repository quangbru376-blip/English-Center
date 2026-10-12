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
public sealed class M3PublicationSqlTests
{
    private const string OptIn="Assigned M3 batch3 owned scratch publication SQL/HTTP; exact ID/GUID cleanup.";
    private static CancellationToken Ct=>OwnedSqlFixture.Ct;
    internal static async Task<(VerifiedSessionToken Academic,VerifiedSessionToken Teacher,int Class,int Report)> Submitted(OwnedSqlFixture f,M1AdmissionsSqlTests.Clock time,bool second=false,string score="49.99")
    {
        var(a,t,id,assignment,component,exam)=await M3GradingSqlTests.Seed(f,time,true,second);var ex=M2ClassSqlTests.Ex(f);
        int[] ids;await using(var db=f.Db())ids=await db.StudentClassAssignments.OrderBy(x=>x.ClassAssignmentId).Select(x=>x.ClassAssignmentId).ToArrayAsync(Ct);
        foreach(var aid in ids)await ex.GradeCommand(t,139,id,aid,Guid.NewGuid(),(await M3GradingSqlTests.Delta(f,id,aid,component,exam,score,"Nhận xét đã công bố")).ToJsonString(),65536,Ct);
        var p=await ex.SubmissionPreview(t,id,"{\"action\":\"SUBMIT\"}",Ct);await ex.GradeCommand(t,141,id,0,Guid.NewGuid(),p["manifest"]!.ToJsonString(),65536,Ct);
        await using var final=f.Db();return(a,t,id,(await final.GradeReports.OrderBy(x=>x.ReportId).FirstAsync(Ct)).ReportId);
    }
    internal static async Task<JsonObject> Manifest(OwnedSqlFixture f,VerifiedSessionToken a,int id)=>(await M2ClassSqlTests.Ex(f).ReviewPreview(a,id,"{\"action\":\"FINALIZE\"}",Ct))["manifest"]!.DeepClone().AsObject();
    private static async Task<ApiErrorCode> Error(Func<Task> work)=>(await Assert.ThrowsAsync<ApiErrorException>(work)).Code;
    [Theory(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    [InlineData("49.99",false)][InlineData("50",true)][InlineData("0",false)]
    public async Task WholeCohortAtomicPublicationClearsUnfinishedRegardlessPass_ImmutableReplay(string score,bool passed)
    {
        var time=new M1AdmissionsSqlTests.Clock();await using var f=await OwnedSqlFixture.CreateAsync(time);var(a,t,id,report)=await Submitted(f,time,true,score);var ex=M2ClassSqlTests.Ex(f);var student=f.Token(await f.Grant("Student"));
        Assert.Empty((await ex.StudentGrades(student,new QueryCollection(),Ct))["items"]!.AsArray());Assert.Equal(ApiErrorCode.NOT_FOUND,await Error(()=>ex.StudentGrade(student,report,Ct)));
        var body=await Manifest(f,a,id);Assert.Equal(2,body["expectedVersions"]!["registrations"]!.AsArray().Count);var key=Guid.NewGuid();var finalized=await ex.GradeCommand(a,147,id,0,key,body.ToJsonString(),65536,Ct);Assert.Equal("FINALIZED",finalized.Result["status"]!.GetValue<string>());
        var replay=await ex.GradeCommand(a,147,id,0,key,body.ToJsonString(),65536,Ct);Assert.True(replay.Replayed);Assert.Equal(finalized.Result["operationId"]!.GetValue<Guid>(),replay.Result["operationId"]!.GetValue<Guid>());
        Assert.Equal(ApiErrorCode.STATE_CONFLICT,await Error(()=>ex.GradeCommand(a,147,id,0,Guid.NewGuid(),body.ToJsonString(),65536,Ct)));
        var detail=await ex.StudentGrade(student,report,Ct);Assert.Equal(passed,detail["passed"]!.GetValue<bool>());Assert.Equal(decimal.Parse(score),decimal.Parse(detail["finalScore"]!.GetValue<string>()));Assert.Single(detail["components"]!.AsArray());
        var forbidden=new[]{"version","enteredBy","enteredAt","latestReturnReason","computation","visibleReportCount","writeContext"};foreach(var field in forbidden)Assert.DoesNotContain("\""+field+"\"",detail.ToJsonString());
        await using var db=f.Db();var reports=await db.GradeReports.ToArrayAsync(Ct);Assert.All(reports,x=>{Assert.Equal("FINALIZED",x.Status);Assert.Equal(x.ReviewedAt,x.FinalizedAt);Assert.Equal(finalized.Result["operationId"]!.GetValue<Guid>(),x.ReviewOperationId);});Assert.Single(reports.Select(x=>x.FinalizedAt).Distinct());Assert.All(await db.Registrations.ToArrayAsync(Ct),x=>{Assert.Equal("ADMITTED",x.Status);Assert.False(x.IsUnfinished);});Assert.Equal(5,await db.AuditLogs.CountAsync(x=>x.Action.StartsWith("ACADEMIC.CLASS.FINALIZE_GRADES"),Ct));Assert.Single(await db.ReplayOperations.Where(x=>x.RequestId==key).ToArrayAsync(Ct));
        var completion=await ex.TeachingLookup(a,"AcademicAffairs","ACADEMIC.CLASS.FINALIZE_GRADES",key,Ct);Assert.NotNull(completion);
        var teacherDraft=await ex.GradeDraft(t,id,detail["class"]!["assignment"]!["classAssignmentId"]!.GetValue<int>(),Ct);Assert.False(teacherDraft["editing"]!["canEdit"]!.GetValue<bool>());
    }
    [Fact(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    public async Task AuditFailureRollsBackAllPublishedTotalsProjectionHistoryAndMarker()
    {
        var time=new M1AdmissionsSqlTests.Clock();await using var f=await OwnedSqlFixture.CreateAsync(time);var(a,_,id,_)=await Submitted(f,time,true);var b=await Manifest(f,a,id);var key=Guid.NewGuid();await using(var hold=await f.HoldAudit())await Assert.ThrowsAsync<EnglishCenter.Api.Infrastructure.Auditing.MandatoryAuditException>(()=>M2ClassSqlTests.Ex(f).GradeCommand(a,147,id,0,key,b.ToJsonString(),65536,Ct));
        await using var db=f.Db();Assert.All(await db.GradeReports.ToArrayAsync(Ct),x=>{Assert.Equal("SUBMITTED",x.Status);Assert.Null(x.FinalScore);Assert.Null(x.FinalizedAt);});Assert.All(await db.Registrations.ToArrayAsync(Ct),x=>Assert.True(x.IsUnfinished));Assert.False(await db.ReplayOperations.AnyAsync(x=>x.RequestId==key,Ct));Assert.False(await db.AuditLogs.AnyAsync(x=>x.Action.StartsWith("ACADEMIC.CLASS.FINALIZE_GRADES"),Ct));
    }
    [Theory(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    [InlineData(false)][InlineData(true)]
    public async Task RegistrationRvAndStoredPredicateCannotBeBypassed(bool falseProjection)
    {
        var time=new M1AdmissionsSqlTests.Clock();await using var f=await OwnedSqlFixture.CreateAsync(time);var(a,_,id,_)=await Submitted(f,time);var b=await Manifest(f,a,id);
        await using(var db=f.Db()){var r=await db.Registrations.SingleAsync(Ct);if(falseProjection)r.IsUnfinished=false;else r.CreatedAt=r.CreatedAt.AddMilliseconds(1);await db.SaveChangesAsync(Ct);}
        if(falseProjection){Assert.False((await M2ClassSqlTests.Ex(f).ReviewPreview(a,id,"{\"action\":\"FINALIZE\"}",Ct))["eligible"]!.GetValue<bool>());await using var db=f.Db();b=await SqlGradeRepository.FinalizeManifest(db,await SqlGradeRepository.Read(db,id,Ct),Ct);}Assert.Equal(ApiErrorCode.STATE_CONFLICT,await Error(()=>M2ClassSqlTests.Ex(f).GradeCommand(a,147,id,0,Guid.NewGuid(),b.ToJsonString(),65536,Ct)));await using var check=f.Db();Assert.Equal("SUBMITTED",(await check.GradeReports.SingleAsync(Ct)).Status);
    }
    [Theory(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    [InlineData(false)][InlineData(true)]
    public async Task SameKeyAndFinalizeVersusReturnHaveSingleAtomicWinner(bool returning)
    {
        var time=new M1AdmissionsSqlTests.Clock();await using var f=await OwnedSqlFixture.CreateAsync(time);var(a,_,id,_)=await Submitted(f,time,true);var b=await Manifest(f,a,id);var r=(await M2ClassSqlTests.Ex(f).ReviewPreview(a,id,"{\"action\":\"RETURN\"}",Ct))["manifest"]!.DeepClone().AsObject();r["reason"]="Sửa trước công bố";var key=Guid.NewGuid();
        async Task<bool> Run(bool second){try{await M2ClassSqlTests.Ex(f).GradeCommand(a,returning&&second?146:147,id,0,returning&&second?Guid.NewGuid():key,(returning&&second?r:b).ToJsonString(),65536,Ct);return true;}catch(ApiErrorException e){Assert.Equal(ApiErrorCode.STATE_CONFLICT,e.Code);return false;}}
        var outcomes=await Task.WhenAll(Run(false),Run(true));Assert.Equal(returning?1:2,outcomes.Count(x=>x));await using var db=f.Db();var reports=await db.GradeReports.ToArrayAsync(Ct);Assert.Single(reports.Select(x=>x.Status).Distinct());Assert.All(await db.Registrations.ToArrayAsync(Ct),x=>Assert.Equal(reports[0].Status!="FINALIZED",x.IsUnfinished));
    }
    [Fact(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    public async Task OwnFiltersVietnameseDatesPrivacyAndCookieCsrfRoleBoundary()
    {
        var time=new M1AdmissionsSqlTests.Clock();await using var f=await OwnedSqlFixture.CreateAsync(time);var(a,_,id,report)=await Submitted(f,time);var ex=M2ClassSqlTests.Ex(f);await ex.GradeCommand(a,147,id,0,Guid.NewGuid(),(await Manifest(f,a,id)).ToJsonString(),65536,Ct);var s=f.Token(await f.Grant("Student"));
        QueryCollection Query(string value)=>new(Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(value));
        var vn=DateOnly.FromDateTime(time.Now.UtcDateTime.AddHours(7)).ToString("yyyy-MM-dd");Assert.Single((await ex.StudentGrades(s,Query("?fromDate="+vn+"&toDate="+vn),Ct))["items"]!.AsArray());Assert.Empty((await ex.StudentGrades(s,Query("?courseId=999999"),Ct))["items"]!.AsArray());Assert.Equal(ApiErrorCode.NOT_FOUND,await Error(()=>ex.StudentGrades(s,Query("?classId=999999"),Ct)));Assert.Equal(ApiErrorCode.VALIDATION_ERROR,await Error(()=>ex.StudentGrades(s,Query("?fromDate=2026-10-13&toDate=2026-10-12"),Ct)));Assert.Equal(ApiErrorCode.NOT_FOUND,await Error(()=>ex.StudentGrade(s,report+99999,Ct)));
        await using var host=await M1HttpSqlTests.Host.Create(f);await host.Login("Student");var response=await host.Send(HttpMethod.Get,"/api/v1/students/me/grade-reports/"+report);Assert.Equal(HttpStatusCode.OK,response.StatusCode);Assert.Contains("no-store",response.Headers.CacheControl!.ToString());Assert.Equal(HttpStatusCode.Forbidden,(await host.Send(HttpMethod.Post,$"/api/v1/academics/grade-reviews/classes/{id}/finalize",new JsonObject(),Guid.NewGuid())).StatusCode);
        await host.Login("AcademicAffairs");Assert.Equal(HttpStatusCode.Forbidden,(await host.Send(HttpMethod.Get,"/api/v1/students/me/grade-reports")).StatusCode);
    }
    [Fact(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    public async Task CookieFinalizeRequiresCsrfThenPublishes_NoUnpublishedOrForeignDetail()
    {
        var time=new M1AdmissionsSqlTests.Clock();await using var f=await OwnedSqlFixture.CreateAsync(time);var(a,_,id,report)=await Submitted(f,time,true);var b=await Manifest(f,a,id);await using var host=await M1HttpSqlTests.Host.Create(f);await host.Login("AcademicAffairs");
        var key=Guid.NewGuid();var path=$"/api/v1/academics/grade-reviews/classes/{id}/finalize";
        Assert.Equal(HttpStatusCode.Forbidden,(await host.Send(HttpMethod.Post,path,b,key,false)).StatusCode);Assert.Equal(HttpStatusCode.OK,(await host.Send(HttpMethod.Post,path,b,key)).StatusCode);
        await host.Login("Student");Assert.Equal(HttpStatusCode.OK,(await host.Send(HttpMethod.Get,"/api/v1/students/me/grade-reports/"+report)).StatusCode);
        int foreign;await using(var db=f.Db())foreign=await db.GradeReports.OrderByDescending(x=>x.ReportId).Select(x=>x.ReportId).FirstAsync(Ct);Assert.Equal(HttpStatusCode.NotFound,(await host.Send(HttpMethod.Get,"/api/v1/students/me/grade-reports/"+foreign)).StatusCode);
    }
    [Fact(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    public async Task CorruptPublishedCompletenessFailsClosedWithoutRepair()
    {
        var time=new M1AdmissionsSqlTests.Clock();await using var f=await OwnedSqlFixture.CreateAsync(time);var(a,_,id,report)=await Submitted(f,time);var ex=M2ClassSqlTests.Ex(f);await ex.GradeCommand(a,147,id,0,Guid.NewGuid(),(await Manifest(f,a,id)).ToJsonString(),65536,Ct);
        await using(var db=f.Db()){
            await using var tx=await db.Database.BeginTransactionAsync(Ct);var program=(await db.Classes.SingleAsync(Ct)).ProgramId;foreach(var gate in new[]{"EC:CLASS:"+id,"EC:PROGRAM:"+program}.Order(StringComparer.Ordinal))await db.Database.ExecuteSqlInterpolatedAsync($"EXEC sys.sp_getapplock @Resource={gate},@LockMode='Exclusive',@LockOwner='Transaction',@DbPrincipal='public';",Ct);
            var r=await db.GradeReports.SingleAsync(Ct);var total=r.FinalScore;var at=r.FinalizedAt;r.Status="DRAFT";r.FinalScore=null;r.FinalizedAt=null;await db.SaveChangesAsync(Ct);db.Scores.Remove(await db.Scores.SingleAsync(Ct));await db.SaveChangesAsync(Ct);r.Status="FINALIZED";r.FinalScore=total;r.FinalizedAt=at;await db.SaveChangesAsync(Ct);await tx.CommitAsync(Ct);
        }
        var s=f.Token(await f.Grant("Student"));Assert.Equal(ApiErrorCode.INTERNAL_ERROR,await Error(()=>ex.StudentGrade(s,report,Ct)));Assert.Equal(ApiErrorCode.INTERNAL_ERROR,await Error(()=>ex.StudentGrades(s,new QueryCollection(),Ct)));await using var check=f.Db();Assert.Empty(await check.Scores.ToArrayAsync(Ct));Assert.Equal("FINALIZED",(await check.GradeReports.SingleAsync(Ct)).Status);
    }

    private sealed class PublicationCutoff(M1AdmissionsSqlTests.Clock clock,DateTimeOffset cutoff,bool afterTouch):Microsoft.EntityFrameworkCore.Diagnostics.DbCommandInterceptor
    {
        private bool touched;public bool Fired{get;private set;}
        public override ValueTask<System.Data.Common.DbDataReader> ReaderExecutedAsync(System.Data.Common.DbCommand command,Microsoft.EntityFrameworkCore.Diagnostics.CommandExecutedEventData data,System.Data.Common.DbDataReader reader,CancellationToken ct=default)
        {
            if(command.CommandText.Contains("[AuthSession]",StringComparison.Ordinal)&&command.CommandText.Contains("UPDATE",StringComparison.Ordinal))touched=true;
            // Advance only at the post-touch result projection: renewed live authority still permits it,
            // but the originally captured request deadline must stop commit.
            if(!Fired&&(afterTouch?touched&&command.CommandText.Contains("ufn_GradeTotal",StringComparison.Ordinal):command.CommandText.Contains("[ReplayOperation]",StringComparison.Ordinal)&&command.CommandText.Contains("INSERT",StringComparison.Ordinal))){clock.Now=cutoff;Fired=true;}return ValueTask.FromResult(reader);
        }
    }
    [Theory(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    [InlineData(false)][InlineData(true)]
    public async Task FinalPublicationOriginalDeadlineOrLowerClockCutoffRollsBack(bool lower)
    {
        var time=new M1AdmissionsSqlTests.Clock();await using var f=await OwnedSqlFixture.CreateAsync(time);var(a,_,id,_)=await Submitted(f,time,true);{var current=time.Now;time.Now=current.AddMinutes(-20);a=f.Token(await f.Grant("AcademicAffairs"));time.Now=current;}var b=await Manifest(f,a,id);var key=Guid.NewGuid();var probe=new PublicationCutoff(time,lower?time.Now.AddMinutes(-20):time.Now.AddMinutes(11),!lower);var ex=new SqlClassExecutor(()=>f.Db(probe),f.Clock,10,1000);
        Assert.Equal(lower?ApiErrorCode.STATE_CONFLICT:ApiErrorCode.AUTHENTICATION_REQUIRED,await Error(()=>ex.GradeCommand(a,147,id,0,key,b.ToJsonString(),65536,Ct)));Assert.True(probe.Fired);
        await using var db=f.Db();Assert.All(await db.GradeReports.ToArrayAsync(Ct),x=>{Assert.Equal("SUBMITTED",x.Status);Assert.Null(x.FinalScore);});Assert.All(await db.Registrations.ToArrayAsync(Ct),x=>Assert.True(x.IsUnfinished));Assert.False(await db.ReplayOperations.AnyAsync(x=>x.RequestId==key,Ct));Assert.False(await db.AuditLogs.AnyAsync(x=>x.Action.StartsWith("ACADEMIC.CLASS.FINALIZE_GRADES"),Ct));
    }
    [Theory(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    [InlineData(false)][InlineData(true)]
    public async Task CompletedFinalizeReplayAndLookupRequireFreshRoleAndAccount(bool roleChanged)
    {
        var time=new M1AdmissionsSqlTests.Clock();await using var f=await OwnedSqlFixture.CreateAsync(time);var(a,_,id,_)=await Submitted(f,time);var body=await Manifest(f,a,id);var key=Guid.NewGuid();var ex=M2ClassSqlTests.Ex(f);await ex.GradeCommand(a,147,id,0,key,body.ToJsonString(),65536,Ct);
        await using(var db=f.Db()){var account=await db.Accounts.SingleAsync(x=>x.AccountId==a.AccountId,Ct);if(roleChanged)account.RoleId=(await db.Roles.SingleAsync(x=>x.RoleCode=="Teacher",Ct)).RoleId;else account.Status="LOCKED";await db.SaveChangesAsync(Ct);}
        await Assert.ThrowsAsync<ApiErrorException>(()=>ex.GradeCommand(a,147,id,0,key,body.ToJsonString(),65536,Ct));await Assert.ThrowsAsync<ApiErrorException>(()=>ex.TeachingLookup(a,"AcademicAffairs","ACADEMIC.CLASS.FINALIZE_GRADES",key,Ct));await using var check=f.Db();Assert.Single(await check.ReplayOperations.Where(x=>x.RequestId==key).ToArrayAsync(Ct));Assert.Equal(3,await check.AuditLogs.CountAsync(x=>x.Action.StartsWith("ACADEMIC.CLASS.FINALIZE_GRADES"),Ct));
    }
    [Theory(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    [InlineData("schedule")][InlineData("component")][InlineData("report")]
    public async Task FinalizeFullSavedChildVersionsRejectStaleMutation(string kind)
    {
        var time=new M1AdmissionsSqlTests.Clock();await using var f=await OwnedSqlFixture.CreateAsync(time);var(a,_,id,_)=await Submitted(f,time);var body=await Manifest(f,a,id);
        await using(var db=f.Db()){
            await using var tx=await db.Database.BeginTransactionAsync(Ct);var program=(await db.Classes.SingleAsync(Ct)).ProgramId;foreach(var gate in new[]{"EC:CLASS:"+id,"EC:PROGRAM:"+program}.Order(StringComparer.Ordinal))await db.Database.ExecuteSqlInterpolatedAsync($"EXEC sys.sp_getapplock @Resource={gate},@LockMode='Exclusive',@LockOwner='Transaction',@DbPrincipal='public';",Ct);
            if(kind=="schedule")(await db.Schedules.SingleAsync(x=>x.SessionType=="FINAL_EXAM",Ct)).Room="CHANGED";
            else if(kind=="component"){var component=await db.ScoreComponentConfigs.SingleAsync(Ct);db.Entry(component).Property(x=>x.ComponentName).IsModified=true;}
            else (await db.GradeReports.SingleAsync(Ct)).TeacherComment="Privileged fixture stale comment";
            await db.SaveChangesAsync(Ct);await tx.CommitAsync(Ct);
        }
        var key=Guid.NewGuid();Assert.Equal(ApiErrorCode.STATE_CONFLICT,await Error(()=>M2ClassSqlTests.Ex(f).GradeCommand(a,147,id,0,key,body.ToJsonString(),65536,Ct)));await using var check=f.Db();Assert.Equal("SUBMITTED",(await check.GradeReports.SingleAsync(Ct)).Status);Assert.False(await check.ReplayOperations.AnyAsync(x=>x.RequestId==key,Ct));
    }
    [Fact(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    public async Task FinalizeSerializesRegistrationCreateThenPermitsIndependentRetake()
    {
        var time=new M1AdmissionsSqlTests.Clock();await using var f=await OwnedSqlFixture.CreateAsync(time);var(a,_,id,report)=await Submitted(f,time);var body=await Manifest(f,a,id);var student=f.Token(await f.Grant("Student"));int course;await using(var db=f.Db())course=(await db.Registrations.SingleAsync(Ct)).CourseId;
        var enroll=new EnglishCenter.Api.Infrastructure.Enrollment.SqlStudentEnrollmentExecutor(()=>f.Db(),f.Clock,10,1000);var create=ClassWire.Object(new{courseId=course}).ToJsonString();
        await Assert.ThrowsAsync<ApiErrorException>(()=>enroll.CommandAsync(student,25,0,0,Guid.NewGuid(),create,65536,Ct));
        async Task<bool> Register(){try{await enroll.CommandAsync(student,25,0,0,Guid.NewGuid(),create,65536,Ct);return true;}catch(ApiErrorException){return false;}}
        var finalize=M2ClassSqlTests.Ex(f).GradeCommand(a,147,id,0,Guid.NewGuid(),body.ToJsonString(),65536,Ct);var registration=Register();await finalize;if(!await registration)Assert.True(await Register());
        await using var check=f.Db();Assert.Equal(2,await check.Registrations.CountAsync(Ct));Assert.Single(await check.Registrations.Where(x=>x.IsUnfinished).ToArrayAsync(Ct));Assert.Equal("FINALIZED",(await check.GradeReports.SingleAsync(Ct)).Status);Assert.Single((await M2ClassSqlTests.Ex(f).StudentGrades(student,new QueryCollection(),Ct))["items"]!.AsArray());Assert.Equal(report,(await M2ClassSqlTests.Ex(f).StudentGrade(student,report,Ct))["id"]!.GetValue<int>());
    }

    [Theory(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    [InlineData(true,"50.00",true)][InlineData(false,"49.99",false)]
    public async Task ActualFn05WeightedIntermediateFinalScoreRoundsOnlyOnceAtThreshold(bool halfUp,string expected,bool passed)
    {
        var time=new M1AdmissionsSqlTests.Clock();await using var f=await OwnedSqlFixture.CreateAsync(time);var(a,program,registration)=await M2ClassSqlTests.Seed(f);
        var programs=new EnglishCenter.Api.Infrastructure.Academics.SqlProgramExecutor(()=>f.Db(),f.Clock,10,1000);
        async Task Apply(int api,JsonObject b)=>await programs.CommandAsync(a,EnglishCenter.Api.Services.Academics.AcademicInput.Parse(api,program,Guid.NewGuid(),b.ToJsonString(),65536),Ct);
        await using(var db=f.Db()){var p=await EnglishCenter.Api.Infrastructure.Academics.SqlProgramRepository.ReadState(db,program,Ct);await Apply(88,ClassWire.Object(new{expectedVersions=new{program=ClassWire.Rv(p.Program.RowVersion)}}));}
        int general,examComponent;await using(var db=f.Db()){var p=await EnglishCenter.Api.Infrastructure.Academics.SqlProgramRepository.ReadState(db,program,Ct);var b=ClassWire.Object(new{courseId=p.Course.CourseId,bandId=p.Band.BandId,name=p.Program.ProgramName,expectedLessonCount=2,components=new object[]{new{id=(int?)p.Components[0].ComponentId,name="Final",weight=halfUp?"50.00":"40.00",kind="FINAL_EXAM"},new{id=(int?)null,name="General",weight=halfUp?"50.00":"60.00",kind="GENERAL"}},expectedVersions=new{program=ClassWire.Rv(p.Program.RowVersion),course=ClassWire.Rv(p.Course.RowVersion),band=ClassWire.Rv(p.Band.RowVersion),existingComponents=EnglishCenter.Api.Services.Academics.ProgramProjection.References(p)},expectedSet=new{existingComponentIds=p.Components.Select(x=>x.ComponentId)}});await Apply(84,b);}
        var activation=await programs.PreviewAsync(a,program,Ct);await Apply(87,System.Text.Json.JsonSerializer.SerializeToNode(activation.Manifest,new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web))!.AsObject());
        var id=await M2ClassSqlTests.Create(f,a,program);foreach(var(type,start,end) in new[]{("LESSON","10:10","10:20"),("LESSON","10:30","10:40"),("FINAL_EXAM","10:50","10:59")})await M2ClassSqlTests.Schedule(f,a,id,M2ClassSqlTests.Window(f,type,start,end,days:0));await M2ClassSqlTests.Open(f,a,id);var ex=M2ClassSqlTests.Ex(f);var place=await ex.Preview(a,id,"placement",$"{{\"registrationId\":{registration}}}",Ct);var pb=place["manifest"]!.DeepClone().AsObject();pb["registrationId"]=registration;await ex.Command(a,108,id,Guid.NewGuid(),pb.ToJsonString(),65536,Ct);time.Now=time.Now.AddMinutes(10);await M3TeachingSqlTests.Start(f,a,id);
        int assignment,exam;await using(var db=f.Db()){assignment=(await db.StudentClassAssignments.SingleAsync(Ct)).ClassAssignmentId;general=(await db.ScoreComponentConfigs.SingleAsync(x=>x.Kind=="GENERAL",Ct)).ComponentId;examComponent=(await db.ScoreComponentConfigs.SingleAsync(x=>x.Kind=="FINAL_EXAM",Ct)).ComponentId;exam=(await db.Schedules.SingleAsync(x=>x.SessionType=="FINAL_EXAM",Ct)).ScheduleId;}
        var t=f.Token(await f.Grant("Teacher"));await ex.GradeCommand(t,139,id,assignment,Guid.NewGuid(),(await M3GradingSqlTests.Delta(f,id,assignment,general,null,"49.99")).ToJsonString(),65536,Ct);Assert.Null((await ex.GradeDraft(t,id,assignment,Ct))["computation"]!["total"]);
        Assert.False((await ex.GradeOptions(t,id,examComponent,Ct))["items"]![0]!["eligible"]!.GetValue<bool>());var early=(await M3GradingSqlTests.Delta(f,id,assignment,examComponent,exam,"50")).ToJsonString();Assert.Equal(ApiErrorCode.STATE_CONFLICT,await Error(()=>ex.GradeCommand(t,139,id,assignment,Guid.NewGuid(),early,65536,Ct)));
        time.Now=time.Now.AddMinutes(40);t=f.Token(await f.Grant("Teacher"));await ex.GradeCommand(t,139,id,assignment,Guid.NewGuid(),(await M3GradingSqlTests.Delta(f,id,assignment,examComponent,exam,"50.00")).ToJsonString(),65536,Ct);

        time.Now=time.Now.AddMinutes(10);a=f.Token(await f.Grant("AcademicAffairs"));t=f.Token(await f.Grant("Teacher"));
        int[] lessons;await using(var db=f.Db())lessons=await db.Schedules.Where(x=>x.SessionType=="LESSON").Select(x=>x.ScheduleId).ToArrayAsync(Ct);
        foreach(var occurrence in lessons.Append(exam))await M3TeachingSqlTests.Held(f,a,occurrence);foreach(var lesson in lessons)await ex.AttendanceCommand(t,id,lesson,Guid.NewGuid(),(await M3TeachingSqlTests.Delta(f,t,id,lesson)).ToJsonString(),65536,Ct);
        var complete=await ex.Preview(a,id,"transition","{\"action\":\"COMPLETE\"}",Ct);await ex.Command(a,101,id,Guid.NewGuid(),complete["manifest"]!.ToJsonString(),65536,Ct);
        Assert.Equal(expected,(await ex.GradeDraft(t,id,assignment,Ct))["computation"]!["total"]!.GetValue<string>());
        var submit=await ex.SubmissionPreview(t,id,"{\"action\":\"SUBMIT\"}",Ct);await ex.GradeCommand(t,141,id,0,Guid.NewGuid(),submit["manifest"]!.ToJsonString(),65536,Ct);
        await ex.GradeCommand(a,147,id,0,Guid.NewGuid(),(await Manifest(f,a,id)).ToJsonString(),65536,Ct);int report;await using(var db=f.Db())report=(await db.GradeReports.SingleAsync(Ct)).ReportId;
        var student=f.Token(await f.Grant("Student"));var result=await ex.StudentGrade(student,report,Ct);Assert.Equal(expected,result["finalScore"]!.GetValue<string>());Assert.Equal(passed,result["passed"]!.GetValue<bool>());Assert.Equal(2,result["components"]!.AsArray().Count);
    }

    private sealed class AfterPublishedSnapshot(OwnedSqlFixture fixture,bool ownership):Microsoft.EntityFrameworkCore.Diagnostics.DbCommandInterceptor
    {
        public bool Fired{get;private set;}
        public override async ValueTask<System.Data.Common.DbDataReader> ReaderExecutedAsync(System.Data.Common.DbCommand command,Microsoft.EntityFrameworkCore.Diagnostics.CommandExecutedEventData data,System.Data.Common.DbDataReader reader,CancellationToken ct=default)
        {
            if(!Fired&&command.Transaction?.IsolationLevel==System.Data.IsolationLevel.Snapshot&&command.CommandText.Contains("[GradeReport]",StringComparison.Ordinal)){
                Fired=true;await using var db=fixture.Db();
                if(ownership){var profile=await db.StudentProfiles.SingleAsync(x=>x.AccountId==fixture.Accounts["Student"],ct);profile.AccountId=fixture.Accounts["Teacher"];}
                else{var account=await db.Accounts.SingleAsync(x=>x.AccountId==fixture.Accounts["Student"],ct);account.Status="LOCKED";}
                await db.SaveChangesAsync(ct);
            }
            return reader;
        }
    }
    [Theory(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    [InlineData(false,false)][InlineData(false,true)][InlineData(true,false)][InlineData(true,true)]
    public async Task StudentSnapshotCannotReturnAfterAccountLockOrOwnershipChange(bool ownership,bool list)
    {
        var time=new M1AdmissionsSqlTests.Clock();await using var f=await OwnedSqlFixture.CreateAsync(time);var(a,_,id,report)=await Submitted(f,time);await M2ClassSqlTests.Ex(f).GradeCommand(a,147,id,0,Guid.NewGuid(),(await Manifest(f,a,id)).ToJsonString(),65536,Ct);var student=f.Token(await f.Grant("Student"));var probe=new AfterPublishedSnapshot(f,ownership);var ex=new SqlClassExecutor(()=>f.Db(probe),f.Clock,10,1000);
        var error=await Error(()=>list?ex.StudentGrades(student,new QueryCollection(),Ct):ex.StudentGrade(student,report,Ct));Assert.Equal(ownership?ApiErrorCode.NOT_FOUND:ApiErrorCode.AUTHENTICATION_REQUIRED,error);Assert.True(probe.Fired);
        await using var db=f.Db();Assert.Equal("FINALIZED",(await db.GradeReports.SingleAsync(Ct)).Status);Assert.False((await db.Registrations.SingleAsync(Ct)).IsUnfinished);
    }
    [Fact(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    public async Task FinalizeRejectsNewCohortMemberWithoutClassRvChange()
    {
        var time=new M1AdmissionsSqlTests.Clock();await using var f=await OwnedSqlFixture.CreateAsync(time);var(a,t,id,assignment,component,exam)=await M3GradingSqlTests.Seed(f,time);var ex=M2ClassSqlTests.Ex(f);await ex.GradeCommand(t,139,id,assignment,Guid.NewGuid(),(await M3GradingSqlTests.Delta(f,id,assignment,component,exam,"75")).ToJsonString(),65536,Ct);
        var p=await ex.SubmissionPreview(t,id,"{\"action\":\"SUBMIT\"}",Ct);
        await ex.GradeCommand(t,141,id,0,Guid.NewGuid(),p["manifest"]!.ToJsonString(),65536,Ct);p=await ex.ReviewPreview(a,id,"{\"action\":\"FINALIZE\"}",Ct);
        var body=p["manifest"]!.DeepClone().AsObject();int source;await using(var db=f.Db())source=(await db.StudentClassAssignments.SingleAsync(Ct)).RegistrationId;var second=await M2ClassSqlTests.SecondPaid(f,source);
        // Owned privileged fixture introduces a member under the protocol gates; no new placement API after teaching.
        await using(var db=f.Db()){await using var tx=await db.Database.BeginTransactionAsync(Ct);var program=(await db.Classes.SingleAsync(Ct)).ProgramId;foreach(var gate in new[]{"EC:CLASS:"+id,"EC:PROGRAM:"+program,"EC:REG:"+second}.Order(StringComparer.Ordinal))await db.Database.ExecuteSqlInterpolatedAsync($"EXEC sys.sp_getapplock @Resource={gate},@LockMode='Exclusive',@LockOwner='Transaction',@DbPrincipal='public';",Ct);var member=new EnglishCenter.Api.Domain.Training.StudentClassAssignment{ClassId=id,RegistrationId=second,AssignedBy=f.Accounts["AcademicAffairs"],AssignedAt=time.Now.UtcDateTime};db.StudentClassAssignments.Add(member);await db.SaveChangesAsync(Ct);db.GradeReports.Add(new(){ClassAssignmentId=member.ClassAssignmentId,Status="DRAFT"});await db.SaveChangesAsync(Ct);await tx.CommitAsync(Ct);}
        var key=Guid.NewGuid();Assert.Equal(ApiErrorCode.STATE_CONFLICT,await Error(()=>ex.GradeCommand(a,147,id,0,key,body.ToJsonString(),65536,Ct)));await using var check=f.Db();Assert.False(await check.ReplayOperations.AnyAsync(x=>x.RequestId==key,Ct));Assert.Equal(2,await check.GradeReports.CountAsync(Ct));
    }

    [Fact(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    public async Task OwnHistoricalRetakeResultsStablePagingVietnamMidnightAndInactiveReferences()
    {
        var time=new M1AdmissionsSqlTests.Clock();await using var f=await OwnedSqlFixture.CreateAsync(time);var(a,_,id,first)=await Submitted(f,time);var ex=M2ClassSqlTests.Ex(f);await ex.GradeCommand(a,147,id,0,Guid.NewGuid(),(await Manifest(f,a,id)).ToJsonString(),65536,Ct);var student=f.Token(await f.Grant("Student"));int course,program;await using(var db=f.Db()){course=(await db.Registrations.SingleAsync(Ct)).CourseId;program=(await db.Classes.SingleAsync(Ct)).ProgramId;}
        var enrollment=new EnglishCenter.Api.Infrastructure.Enrollment.SqlStudentEnrollmentExecutor(()=>f.Db(),f.Clock,10,1000);var retake=(EnglishCenter.Api.Services.Enrollment.OwnRegistrationResult)await enrollment.CommandAsync(student,25,0,0,Guid.NewGuid(),ClassWire.Object(new{courseId=course}).ToJsonString(),65536,Ct);var reg=retake.Registration.Id;
        var secondClass=await M2ClassSqlTests.Create(f,a,program,"RETAKE_HISTORY");int second;
        // Synthetic historical projection fixture: existing APIs allocate the independent Registration/Class;
        // gated scratch-only rows provide the second published own report without claiming another runtime workflow.
        var midnight=new DateTime(2026,10,11,17,0,0,DateTimeKind.Utc);
        await using(var db=f.Db()){
            await using var tx=await db.Database.BeginTransactionAsync(Ct);foreach(var gate in new[]{"EC:CLASS:"+id,"EC:CLASS:"+secondClass,"EC:PROGRAM:"+program,"EC:REG:"+reg}.Order(StringComparer.Ordinal))await db.Database.ExecuteSqlInterpolatedAsync($"EXEC sys.sp_getapplock @Resource={gate},@LockMode='Exclusive',@LockOwner='Transaction',@DbPrincipal='public';",Ct);
            var r=await db.Registrations.SingleAsync(x=>x.RegistrationId==reg,Ct);r.Status="ADMITTED";r.AdmissionConfirmedAt=midnight;r.AdmissionApprovedAt=midnight;r.AdmissionApprovedBy=f.Accounts["Admissions"];r.IsUnfinished=false;(await db.Classes.SingleAsync(x=>x.ClassId==secondClass,Ct)).Status="COMPLETED";
            var member=new EnglishCenter.Api.Domain.Training.StudentClassAssignment{ClassId=secondClass,RegistrationId=reg,AssignedBy=f.Accounts["AcademicAffairs"],AssignedAt=midnight.AddDays(-1)};db.StudentClassAssignments.Add(member);await db.SaveChangesAsync(Ct);
            var report=new EnglishCenter.Api.Domain.Training.GradeReport{ClassAssignmentId=member.ClassAssignmentId,Status="DRAFT"};db.GradeReports.Add(report);
            var old=await db.Schedules.SingleAsync(x=>x.ClassId==id&&x.SessionType=="FINAL_EXAM",Ct);var schedule=new EnglishCenter.Api.Domain.Training.Schedule{ClassId=secondClass,SessionType="FINAL_EXAM",SessionDate=old.SessionDate,StartTime=old.StartTime,EndTime=old.EndTime,Room="HISTORY",ObligationKey=Guid.NewGuid(),IsCurrent=true,OccurrenceState="HELD",HeldBy=f.Accounts["AcademicAffairs"],HeldAt=midnight};db.Schedules.Add(schedule);await db.SaveChangesAsync(Ct);
            var component=await db.ScoreComponentConfigs.SingleAsync(Ct);db.Scores.Add(new(){ClassAssignmentId=member.ClassAssignmentId,ComponentId=component.ComponentId,ScheduleId=schedule.ScheduleId,ScoreValue=75,EnteredBy=f.Accounts["Teacher"],EnteredAt=midnight});await db.SaveChangesAsync(Ct);
            report.Status="FINALIZED";report.FinalScore=75;report.FinalizedAt=midnight;report.ReviewedAt=midnight;report.ReviewedBy=f.Accounts["AcademicAffairs"];report.ReviewOperationId=Guid.NewGuid();report.SubmittedAt=midnight;report.SubmittedBy=f.Accounts["Teacher"];report.SubmittedOperationId=Guid.NewGuid();report.TeacherComment="Lượt học lại độc lập";
            var original=await db.GradeReports.SingleAsync(x=>x.ReportId==first,Ct);original.FinalizedAt=midnight;original.ReviewedAt=midnight;
            (await db.TrainingPrograms.SingleAsync(x=>x.ProgramId==program,Ct)).Status="INACTIVE";(await db.Courses.SingleAsync(x=>x.CourseId==course,Ct)).Status="INACTIVE";await db.SaveChangesAsync(Ct);second=report.ReportId;await tx.CommitAsync(Ct);
        }
        QueryCollection Query(string value)=>new(Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(value));
        var page1=await ex.StudentGrades(student,Query("?pageSize=1&sort=FINALIZED_ASC"),Ct);var page2=await ex.StudentGrades(student,Query("?pageSize=1&page=2&sort=FINALIZED_ASC"),Ct);Assert.Equal(2,page1["totalCount"]!.GetValue<long>());Assert.Equal(first,page1["items"]![0]!["id"]!.GetValue<int>());Assert.Equal(second,page2["items"]![0]!["id"]!.GetValue<int>());Assert.Equal(second,(await ex.StudentGrades(student,Query("?pageSize=1&sort=FINALIZED_DESC"),Ct))["items"]![0]!["id"]!.GetValue<int>());
        Assert.Equal(2,(await ex.StudentGrades(student,Query("?fromDate=2026-10-12&toDate=2026-10-12"),Ct))["items"]!.AsArray().Count);Assert.Empty((await ex.StudentGrades(student,Query("?toDate=2026-10-11"),Ct))["items"]!.AsArray());Assert.Equal(2,(await ex.StudentGrades(student,Query("?fromDate=0001-01-01&toDate=9999-12-31"),Ct))["items"]!.AsArray().Count);Assert.Empty((await ex.StudentGrades(student,Query("?fromDate=2026-10-13"),Ct))["items"]!.AsArray());Assert.Equal(ApiErrorCode.VALIDATION_ERROR,await Error(()=>ex.StudentGrades(student,Query("?studentId=1"),Ct)));
        var oldResult=await ex.StudentGrade(student,first,Ct);var newResult=await ex.StudentGrade(student,second,Ct);Assert.Equal("49.99",oldResult["finalScore"]!.GetValue<string>());Assert.Equal("75.00",newResult["finalScore"]!.GetValue<string>());Assert.NotEqual(oldResult["class"]!["assignment"]!["registrationId"]!.GetValue<int>(),newResult["class"]!["assignment"]!["registrationId"]!.GetValue<int>());Assert.Equal("INACTIVE",oldResult["class"]!["program"]!["status"]!.GetValue<string>());Assert.Equal("INACTIVE",newResult["class"]!["program"]!["course"]!["status"]!.GetValue<string>());
    }
    [Fact(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    public async Task FinalizedTeacherDeltaAndActualSqlScoreTriggerRejectWithoutEffects()
    {
        var time=new M1AdmissionsSqlTests.Clock();await using var f=await OwnedSqlFixture.CreateAsync(time);var(a,t,id,report)=await Submitted(f,time);var ex=M2ClassSqlTests.Ex(f);await ex.GradeCommand(a,147,id,0,Guid.NewGuid(),(await Manifest(f,a,id)).ToJsonString(),65536,Ct);int assignment,component,exam;await using(var db=f.Db()){var score=await db.Scores.SingleAsync(Ct);assignment=score.ClassAssignmentId;component=score.ComponentId;exam=score.ScheduleId!.Value;}
        var key=Guid.NewGuid();var delta=await M3GradingSqlTests.Delta(f,id,assignment,component,exam,"80");Assert.Equal(ApiErrorCode.STATE_CONFLICT,await Error(()=>ex.GradeCommand(t,139,id,assignment,key,delta.ToJsonString(),65536,Ct)));
        await using(var db=f.Db()){await using var tx=await db.Database.BeginTransactionAsync(Ct);var program=(await db.Classes.SingleAsync(Ct)).ProgramId;foreach(var gate in new[]{"EC:CLASS:"+id,"EC:PROGRAM:"+program}.Order(StringComparer.Ordinal))await db.Database.ExecuteSqlInterpolatedAsync($"EXEC sys.sp_getapplock @Resource={gate},@LockMode='Exclusive',@LockOwner='Transaction',@DbPrincipal='public';",Ct);var score=await db.Scores.SingleAsync(Ct);score.ScoreValue=80;var failure=await Assert.ThrowsAsync<DbUpdateException>(()=>db.SaveChangesAsync(Ct));Assert.Equal(51042,Assert.IsType<Microsoft.Data.SqlClient.SqlException>(failure.InnerException).Number);await tx.RollbackAsync(Ct);}
        await using var check=f.Db();Assert.Equal(49.99m,(await check.Scores.SingleAsync(Ct)).ScoreValue);Assert.Equal(49.99m,(await check.GradeReports.SingleAsync(Ct)).FinalScore);Assert.False(await check.ReplayOperations.AnyAsync(x=>x.RequestId==key,Ct));Assert.Equal(report,(await check.GradeReports.SingleAsync(Ct)).ReportId);
    }

}
