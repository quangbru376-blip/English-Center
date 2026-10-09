namespace EnglishCenter.Tests.Api.Integration;

using System.Text.Json;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Common.Serialization;
using EnglishCenter.Api.Domain.Academics;
using EnglishCenter.Api.Domain.Admissions;
using EnglishCenter.Api.Infrastructure.Academics;
using EnglishCenter.Api.Services.Academics;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Xunit;

[Collection("V01 HTTP SQL")]
public sealed class M1ProgramSessionSqlTests
{
    private const string OptIn="Owned M1 scratch SQL, existing migrations and identity-checked cleanup.";
    private static CancellationToken Ct=>OwnedSqlFixture.Ct;
    private static AcademicInput Input(int api,int id,object body,Guid? key=null)=>AcademicInput.Parse(api,id,key??Guid.NewGuid(),JsonSerializer.Serialize(body,new JsonSerializerOptions(JsonSerializerDefaults.Web)),65536);
    private static SqlProgramExecutor Programs(OwnedSqlFixture f)=>new(()=>f.Db(),f.Clock,10,1000);
    private static SqlEntrySessionExecutor Sessions(OwnedSqlFixture f)=>new(()=>f.Db(),f.Clock,10,1000);
    private static async Task<(Course Course,Band Band)> References(OwnedSqlFixture f)
    {await using var db=f.Db();await EnglishCenter.Api.Infrastructure.Operator.LocalBandReferenceSeed.SeedAsync(db,Ct);var course=new Course{CourseCode="M1",CourseName="Synthetic",BasePrice=1000,Status="INACTIVE"};db.Courses.Add(course);await db.SaveChangesAsync(Ct);return(course,await db.Bands.OrderBy(x=>x.MinScore).FirstAsync(Ct));}
    private static object Window(OwnedSqlFixture f,int grader,int days=1,string room="M1_ROOM",string start="09:00",string end="10:00")=>new{date=f.Clock.GetUtcNow().AddHours(7).AddDays(days).ToString("yyyy-MM-dd"),startTime=start,endTime=end,room,graderId=grader,capacity=20};
    [Fact(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    public async Task Program_InactiveCourseDraftWholeReplacementSwapAndSavedActivationReplay()
    {
        await using var f=await OwnedSqlFixture.CreateAsync();var token=f.Token(await f.Grant("AcademicAffairs"));var (course,band)=await References(f);var executor=Programs(f);
        var create=Input(82,0,new{courseId=course.CourseId,bandId=band.BandId,name="Draft",expectedLessonCount=4,components=Array.Empty<object>(),expectedVersions=new{course=RowVersionCodec.Encode(course.RowVersion),band=RowVersionCodec.Encode(band.RowVersion)}});
        var first=await executor.CommandAsync(token,create,Ct);Assert.Equal("DRAFT",first.Program.Status);Assert.Empty(first.Program.Components);Assert.False(first.Program.Validation.MeetsActivationRules);
        Assert.Equal(first.OperationId,(await executor.CommandAsync(token,create,Ct)).OperationId);
        JsonElement manifest=JsonSerializer.SerializeToElement(first.Program.ConfigurationManifest,new JsonSerializerOptions(JsonSerializerDefaults.Web));var edit=Input(84,first.Program.Id,new{courseId=course.CourseId,bandId=band.BandId,name="Complete",expectedLessonCount=4,components=new[]{new{id=(int?)null,name="A",weight="40.00",kind="GENERAL"},new{id=(int?)null,name="B",weight="60.00",kind="FINAL_EXAM"}},expectedVersions=manifest.GetProperty("expectedVersions"),expectedSet=manifest.GetProperty("expectedSet")});
        var full=await executor.CommandAsync(token,edit,Ct);Assert.True(full.Program.Validation.MeetsActivationRules);manifest=JsonSerializer.SerializeToElement(full.Program.ConfigurationManifest,new JsonSerializerOptions(JsonSerializerDefaults.Web));
        var swap=Input(84,full.Program.Id,new{courseId=course.CourseId,bandId=band.BandId,name="Complete",expectedLessonCount=4,components=full.Program.Components.Select(x=>new{id=(int?)x.Id,name=x.Name=="A"?"B":"A",weight=x.Weight,kind=x.Kind=="GENERAL"?"FINAL_EXAM":"GENERAL"}).ToArray(),expectedVersions=manifest.GetProperty("expectedVersions"),expectedSet=manifest.GetProperty("expectedSet")});
        var swapped=await executor.CommandAsync(token,swap,Ct);Assert.Equal(full.Program.Components.Select(x=>x.Id).Order(),swapped.Program.Components.Select(x=>x.Id).Order());
        Assert.Equal(full.OperationId,(await executor.CommandAsync(token,edit,Ct)).OperationId);
        await Assert.ThrowsAsync<ApiErrorException>(()=>executor.CommandAsync(token,AcademicInput.Parse(84,full.Program.Id,Guid.NewGuid(),JsonDocument.Parse(edit.Intent.CopyBytes()).RootElement.GetProperty("body").GetRawText(),65536),Ct));
        var preview=await executor.PreviewAsync(token,first.Program.Id,Ct);Assert.True(preview.Eligible);var activate=Input(87,first.Program.Id,preview.Manifest!);var active=await executor.CommandAsync(token,activate,Ct);Assert.Equal("ACTIVE",active.Program.Status);Assert.Equal(active.OperationId,(await executor.CommandAsync(token,activate,Ct)).OperationId);
        var inactive=await executor.CommandAsync(token,Input(89,active.Program.Id,new{expectedVersions=new{program=active.Program.Version}}),Ct);Assert.Equal("INACTIVE",inactive.Program.Status);
        var restore=await executor.PreviewAsync(token,inactive.Program.Id,Ct);Assert.Equal("ACTIVE",(await executor.CommandAsync(token,Input(87,inactive.Program.Id,restore.Manifest!),Ct)).Program.Status);
        await using var db=f.Db();Assert.Equal(1,await db.TrainingPrograms.CountAsync(Ct));Assert.Equal(2,await db.ScoreComponentConfigs.CountAsync(Ct));Assert.Equal(6,await db.ReplayOperations.CountAsync(Ct));
    }
    [Fact(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    public async Task DraftRetargetRequiresTargetVersionsAndPreservesFullSourceGuard()
    {
        await using var f=await OwnedSqlFixture.CreateAsync();var token=f.Token(await f.Grant("AcademicAffairs"));var (course,band)=await References(f);var executor=Programs(f);
        var draft=await executor.CommandAsync(token,Input(82,0,new{courseId=course.CourseId,bandId=band.BandId,name="Retarget",expectedLessonCount=1,components=Array.Empty<object>(),expectedVersions=new{course=RowVersionCodec.Encode(course.RowVersion),band=RowVersionCodec.Encode(band.RowVersion)}}),Ct);
        Course target;Band next;await using(var db=f.Db()){target=new Course{CourseCode="TARGET",CourseName="Synthetic target",BasePrice=1,Status="INACTIVE"};db.Courses.Add(target);await db.SaveChangesAsync(Ct);next=await db.Bands.OrderBy(x=>x.MinScore).Skip(1).FirstAsync(Ct);}
        var manifest=JsonSerializer.SerializeToElement(draft.Program.ConfigurationManifest,new JsonSerializerOptions(JsonSerializerDefaults.Web));
        object Body(string version)=>new{courseId=target.CourseId,bandId=next.BandId,name="Retarget",expectedLessonCount=1,components=Array.Empty<object>(),expectedSet=manifest.GetProperty("expectedSet"),expectedVersions=new{program=draft.Program.Version,course=version,band=RowVersionCodec.Encode(next.RowVersion),existingComponents=manifest.GetProperty("expectedVersions").GetProperty("existingComponents")}};
        Assert.Equal(ApiErrorCode.STATE_CONFLICT,(await Assert.ThrowsAsync<ApiErrorException>(()=>executor.CommandAsync(token,Input(84,draft.Program.Id,Body("AAAAAAAAAAA=")),Ct))).Code);
        var moved=await executor.CommandAsync(token,Input(84,draft.Program.Id,Body(RowVersionCodec.Encode(target.RowVersion))),Ct);Assert.Equal(target.CourseId,moved.Program.Course.Id);Assert.Equal(next.BandId,moved.Program.Band.Id);Assert.Empty(moved.Program.Components);
    }
    [Fact(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    public async Task Session_CreateOverlapAdjacencyOpenCloseReadonlyCutoffAndSafeReplay()
    {
        await using var f=await OwnedSqlFixture.CreateAsync();var token=f.Token(await f.Grant("AcademicAffairs"));var executor=Sessions(f);var grader=f.Accounts["Teacher"];var input=Input(55,0,Window(f,grader));var first=await executor.CommandAsync(token,input,Ct);Assert.Equal("CLOSED",first.Session.Status);Assert.Equal(first.OperationId,(await executor.CommandAsync(token,input,Ct)).OperationId);
        await Assert.ThrowsAsync<ApiErrorException>(()=>executor.CommandAsync(token,Input(55,0,Window(f,grader,room:"OTHER")),Ct));
        var adjacent=await executor.CommandAsync(token,Input(55,0,Window(f,grader,start:"10:00",end:"11:00")),Ct);Assert.Equal("CLOSED",adjacent.Session.Status);
        var open=await executor.CommandAsync(token,Input(58,first.Session.Id,new{expectedVersions=new{session=first.Session.Version}}),Ct);Assert.Equal("OPEN",open.Session.Status);
        var closed=await executor.CommandAsync(token,Input(59,first.Session.Id,new{expectedVersions=new{session=open.Session.Version}}),Ct);Assert.Equal("CLOSED",closed.Session.Status);
        await Assert.ThrowsAsync<ApiErrorException>(()=>executor.CommandAsync(token,Input(58,first.Session.Id,new{expectedVersions=new{session=first.Session.Version}}),Ct));
        var page=await executor.SearchAsync(token,QueryCollection.Empty,Ct);Assert.Equal(2,page.TotalCount);await using var db=f.Db();Assert.Equal(4,await db.ReplayOperations.CountAsync(Ct));
        var options=await executor.OptionsAsync(token,new QueryCollection(new Dictionary<string,Microsoft.Extensions.Primitives.StringValues>{{"date",first.Session.Date},{"startTime",first.Session.StartTime},{"endTime",first.Session.EndTime},{"sessionId",first.Session.Id.ToString()}}),Ct);Assert.Contains(options.Items,x=>x.AccountId==grader);Assert.NotNull(options.CurrentGrader);
    }
    [Fact(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    public async Task Session_CancellationFullMembershipSp02AndAtomicProvenance()
    {
        await using var f=await OwnedSqlFixture.CreateAsync();var token=f.Token(await f.Grant("AcademicAffairs"));var executor=Sessions(f);var (course,_)=await References(f);var created=await executor.CommandAsync(token,Input(55,0,Window(f,f.Accounts["Teacher"])),Ct);
        int registrationId,attemptId,bookingId;await using(var db=f.Db()){var student=await db.StudentProfiles.SingleAsync(Ct);var contact=await db.Accounts.SingleAsync(x=>x.AccountId==student.AccountId,Ct);contact.Phone="0912345678";var reg=new Registration{StudentId=student.StudentId,CourseId=course.CourseId,Status="PENDING",CreatedAt=f.Clock.GetUtcNow().UtcDateTime};db.Registrations.Add(reg);await db.SaveChangesAsync(Ct);var attempt=new EntryTestAttempt{RegistrationId=reg.RegistrationId,AttemptNumber=1};db.EntryTestAttempts.Add(attempt);await db.SaveChangesAsync(Ct);var booking=new EntryTestBooking{AttemptId=attempt.AttemptId,SessionId=created.Session.Id,Status="SCHEDULED",BookedAt=f.Clock.GetUtcNow().UtcDateTime};db.EntryTestBookings.Add(booking);await db.SaveChangesAsync(Ct);registrationId=reg.RegistrationId;attemptId=attempt.AttemptId;bookingId=booking.BookingId;}
        var roster=await executor.RosterAsync(token,created.Session.Id,SessionRosterFilter.Parse("{}"),Ct);Assert.Single(roster.Items);Assert.Null(roster.Items[0].InvoiceStatus);Assert.Null(roster.Items[0].TargetScore);
        var literal=await executor.RosterAsync(token,created.Session.Id,SessionRosterFilter.Parse("{\"name\":\"%\"}"),Ct);Assert.Empty(literal.Items);
        var preview=await executor.CancellationAsync(token,created.Session.Id,Ct);Assert.True(preview.Eligible);Assert.Single(preview.Effects);var m=JsonSerializer.SerializeToElement(preview.Manifest,new JsonSerializerOptions(JsonSerializerDefaults.Web));
        var cancel=Input(61,created.Session.Id,new{reason="Synthetic cancellation",expectedVersions=m.GetProperty("expectedVersions"),expectedSet=m.GetProperty("expectedSet")});var done=await executor.CommandAsync(token,cancel,Ct);Assert.Equal("CANCELLED",done.Session.Status);Assert.Equal(done.OperationId,(await executor.CommandAsync(token,cancel,Ct)).OperationId);
        await using var check=f.Db();Assert.Equal("PENDING",(await check.Registrations.FindAsync([registrationId],Ct))!.Status);Assert.Null((await check.EntryTestAttempts.FindAsync([attemptId],Ct))!.Score);var cancelled=(await check.EntryTestBookings.FindAsync([bookingId],Ct))!;Assert.Equal("CANCELLED",cancelled.Status);Assert.Equal(done.OperationId,cancelled.CancellationOperationId);Assert.Equal("SCHEDULED",cancelled.CancelledFromStatus);Assert.Equal(2,await check.AuditLogs.CountAsync(x=>x.OperationId==done.OperationId,Ct));
        Assert.Empty((await executor.RosterAsync(token,created.Session.Id,SessionRosterFilter.Parse("{}"),Ct)).Items);Assert.Single((await executor.RosterAsync(token,created.Session.Id,SessionRosterFilter.Parse("{\"view\":\"history\"}"),Ct)).Items);
    }
    [Fact(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    public async Task MandatoryFailure_RollsBackBothNewFamilies()
    {
        await using var f=await OwnedSqlFixture.CreateAsync();var token=f.Token(await f.Grant("AcademicAffairs"));var (course,band)=await References(f);
        await using(var gate=await f.HoldAudit()){await Assert.ThrowsAsync<EnglishCenter.Api.Infrastructure.Auditing.MandatoryAuditException>(()=>Sessions(f).CommandAsync(token,Input(55,0,Window(f,f.Accounts["Teacher"])),Ct));await Assert.ThrowsAsync<EnglishCenter.Api.Infrastructure.Auditing.MandatoryAuditException>(()=>Programs(f).CommandAsync(token,Input(82,0,new{courseId=course.CourseId,bandId=band.BandId,name="Draft",expectedLessonCount=1,components=Array.Empty<object>(),expectedVersions=new{course=RowVersionCodec.Encode(course.RowVersion),band=RowVersionCodec.Encode(band.RowVersion)}}),Ct));}
        await using var db=f.Db();Assert.Empty(await db.EntryTestSessions.ToListAsync(Ct));Assert.Empty(await db.TrainingPrograms.ToListAsync(Ct));Assert.Empty(await db.ReplayOperations.ToListAsync(Ct));
    }
    [Theory(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    [InlineData("Admin")][InlineData("Admissions")][InlineData("Teacher")][InlineData("Student")][InlineData("Accountant")][InlineData("Director")]
    public async Task CurrentRoleCannotReadAcademicFamilies(string role)
    {await using var f=await OwnedSqlFixture.CreateAsync();var token=f.Token(await f.Grant(role));Assert.Equal(ApiErrorCode.FORBIDDEN,(await Assert.ThrowsAsync<ApiErrorException>(()=>Programs(f).SearchAsync(token,QueryCollection.Empty,Ct))).Code);Assert.Equal(ApiErrorCode.FORBIDDEN,(await Assert.ThrowsAsync<ApiErrorException>(()=>Sessions(f).SearchAsync(token,QueryCollection.Empty,Ct))).Code);}
    [Fact(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    public async Task UsedCancelledClassFreezesCore_StillRenameDeactivateAndRestore()
    {
        await using var f=await OwnedSqlFixture.CreateAsync();var token=f.Token(await f.Grant("AcademicAffairs"));var (course,band)=await References(f);var executor=Programs(f);
        var first=await executor.CommandAsync(token,Input(82,0,new{courseId=course.CourseId,bandId=band.BandId,name="Ready",expectedLessonCount=1,components=new[]{new{name="Exam",weight="100",kind="FINAL_EXAM"}},expectedVersions=new{course=RowVersionCodec.Encode(course.RowVersion),band=RowVersionCodec.Encode(band.RowVersion)}}),Ct);
        var active=await executor.CommandAsync(token,Input(87,first.Program.Id,(await executor.PreviewAsync(token,first.Program.Id,Ct)).Manifest!),Ct);
        await using(var db=f.Db()){db.Classes.Add(new EnglishCenter.Api.Domain.Training.Class{ProgramId=active.Program.Id,TeacherId=f.Accounts["Teacher"],ClassCode="SYNTHETIC",Status="CANCELLED",MaxSize=10,CreatedAt=f.Clock.GetUtcNow().UtcDateTime,CancelledBy=token.AccountId,CancelledAt=f.Clock.GetUtcNow().UtcDateTime,CancellationReason="Synthetic",CancellationOperationId=Guid.NewGuid()});await db.SaveChangesAsync(Ct);}
        var frozen=await executor.DetailAsync(token,active.Program.Id,Ct);Assert.True(frozen.IsConfigurationFrozen);Assert.Null(frozen.ConfigurationManifest);Assert.DoesNotContain("RETURN_TO_DRAFT",frozen.AllowedActions);
        await Assert.ThrowsAsync<ApiErrorException>(()=>executor.CommandAsync(token,Input(88,frozen.Id,new{expectedVersions=new{program=frozen.Version}}),Ct));
        var renamed=await executor.CommandAsync(token,Input(85,frozen.Id,new{name="Renamed used",expectedVersions=new{program=frozen.Version}}),Ct);Assert.True(renamed.Program.IsConfigurationFrozen);
        var inactive=await executor.CommandAsync(token,Input(89,frozen.Id,new{expectedVersions=new{program=renamed.Program.Version}}),Ct);var restored=await executor.CommandAsync(token,Input(87,frozen.Id,(await executor.PreviewAsync(token,frozen.Id,Ct)).Manifest!),Ct);Assert.Equal("ACTIVE",restored.Program.Status);Assert.True(restored.Program.IsConfigurationFrozen);
    }
    [Fact(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    public async Task GraderChange_CurrentCandidateVersionGuardAndSameGraderNoop()
    {
        await using var f=await OwnedSqlFixture.CreateAsync();var token=f.Token(await f.Grant("AcademicAffairs"));var executor=Sessions(f);var first=await executor.CommandAsync(token,Input(55,0,Window(f,f.Accounts["Teacher"])),Ct);
        var same=await executor.GraderPreviewAsync(token,first.Session.Id,f.Accounts["Teacher"],Ct);Assert.False(same.Eligible);Assert.Null(same.Manifest);
        int candidate;await using(var db=f.Db()){var teacher=await db.Accounts.SingleAsync(x=>x.AccountId==f.Accounts["Teacher"],Ct);var other=new EnglishCenter.Api.Domain.Identity.Account{RoleId=teacher.RoleId,FullName="Synthetic second",Email="second@example.invalid",PasswordHash=teacher.PasswordHash,Status="ACTIVE",SecurityVersion=1,CreatedAt=f.Clock.GetUtcNow().UtcDateTime};db.Accounts.Add(other);await db.SaveChangesAsync(Ct);candidate=other.AccountId;}
        var preview=await executor.GraderPreviewAsync(token,first.Session.Id,candidate,Ct);Assert.True(preview.Eligible);var m=JsonSerializer.SerializeToElement(preview.Manifest,new JsonSerializerOptions(JsonSerializerDefaults.Web));var body=new{graderId=candidate,reason="Synthetic transfer",expectedVersions=m.GetProperty("expectedVersions"),expectedSet=m.GetProperty("expectedSet")};
        await using(var db=f.Db()){var other=await db.Accounts.SingleAsync(x=>x.AccountId==candidate,Ct);other.FullName="Changed since preview";await db.SaveChangesAsync(Ct);}
        Assert.Equal(ApiErrorCode.STATE_CONFLICT,(await Assert.ThrowsAsync<ApiErrorException>(()=>executor.CommandAsync(token,Input(64,first.Session.Id,body),Ct))).Code);
        preview=await executor.GraderPreviewAsync(token,first.Session.Id,candidate,Ct);m=JsonSerializer.SerializeToElement(preview.Manifest,new JsonSerializerOptions(JsonSerializerDefaults.Web));var input=Input(64,first.Session.Id,new{graderId=candidate,reason="Synthetic transfer",expectedVersions=m.GetProperty("expectedVersions"),expectedSet=m.GetProperty("expectedSet")});var done=await executor.CommandAsync(token,input,Ct);Assert.Equal(candidate,done.Session.Grader.AccountId);Assert.Equal(done.OperationId,(await executor.CommandAsync(token,input,Ct)).OperationId);
        // A fresh same-grader intent with valid full context is 400, without effects.
        await using var check=f.Db();var state=await SqlEntrySessionRepository.ReadState(check,first.Session.Id,Ct);var account=await check.Accounts.SingleAsync(x=>x.AccountId==candidate,Ct);m=JsonSerializer.SerializeToElement(SessionProjection.Manifest(state,account),new JsonSerializerOptions(JsonSerializerDefaults.Web));Assert.Equal(ApiErrorCode.VALIDATION_ERROR,(await Assert.ThrowsAsync<ApiErrorException>(()=>executor.CommandAsync(token,Input(64,first.Session.Id,new{graderId=candidate,reason="Noop",expectedVersions=m.GetProperty("expectedVersions"),expectedSet=m.GetProperty("expectedSet")}),Ct))).Code);
    }
    [Fact(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    public async Task ClockAtStart_ReadOnlyEffectiveClosureAndWriteRefusal()
    {
        var time=new MovingClock();await using var f=await OwnedSqlFixture.CreateAsync(time);var token=f.Token(await f.Grant("AcademicAffairs"));var executor=Sessions(f);var next=time.Now.AddMinutes(5).AddHours(7);var end=next.AddMinutes(10);var session=await executor.CommandAsync(token,Input(55,0,new{date=next.ToString("yyyy-MM-dd"),startTime=next.ToString("HH:mm"),endTime=end.ToString("HH:mm"),room="CLOCK",graderId=f.Accounts["Teacher"],capacity=1}),Ct);var opened=await executor.CommandAsync(token,Input(58,session.Session.Id,new{expectedVersions=new{session=session.Session.Version}}),Ct);
        time.Now=DateTimeOffset.Parse(opened.Session.StartAt);var detail=await executor.DetailAsync(token,session.Session.Id,Ct);Assert.Equal("OPEN",detail.Status);Assert.Equal("CLOSED",detail.EffectiveStatus);Assert.Equal(opened.Session.Version,detail.Version);
        Assert.False((await executor.CancellationAsync(token,detail.Id,Ct)).Eligible);Assert.Equal(ApiErrorCode.STATE_CONFLICT,(await Assert.ThrowsAsync<ApiErrorException>(()=>executor.CommandAsync(token,Input(59,detail.Id,new{expectedVersions=new{session=detail.Version}}),Ct))).Code);
        await using var db=f.Db();Assert.Equal("OPEN",(await db.EntryTestSessions.FindAsync([detail.Id],Ct))!.Status);Assert.Equal(2,await db.ReplayOperations.CountAsync(Ct));
    }
    private sealed class MovingClock:TimeProvider{public DateTimeOffset Now=new(2026,10,9,1,0,0,TimeSpan.Zero);public override DateTimeOffset GetUtcNow()=>Now;}
    [Fact(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    public async Task CutoffDuringFinalProjectionRollsBackSessionAuditAndCompletion()
    {
        var time=new MovingClock();await using var f=await OwnedSqlFixture.CreateAsync(time);var token=f.Token(await f.Grant("AcademicAffairs"));var cutoff=time.Now.AddMinutes(5);var interceptor=new FinalProjectionClock(time,cutoff);var executor=new SqlEntrySessionExecutor(()=>f.Db(interceptor),f.Clock,10,1000);
        var local=cutoff.AddHours(7);var body=new{date=local.ToString("yyyy-MM-dd"),startTime=local.ToString("HH:mm"),endTime=local.AddMinutes(10).ToString("HH:mm"),room="CUTOFF",graderId=f.Accounts["Teacher"],capacity=1};
        Assert.Equal(ApiErrorCode.STATE_CONFLICT,(await Assert.ThrowsAsync<ApiErrorException>(()=>executor.CommandAsync(token,Input(55,0,body),Ct))).Code);Assert.True(interceptor.Fired);
        await using var db=f.Db();Assert.Empty(await db.EntryTestSessions.ToListAsync(Ct));Assert.Empty(await db.ReplayOperations.ToListAsync(Ct));Assert.Equal(0,await db.AuditLogs.CountAsync(x=>x.AuditCategory=="MANDATORY",Ct));
    }
    private sealed class FinalProjectionClock(MovingClock time,DateTimeOffset cutoff):Microsoft.EntityFrameworkCore.Diagnostics.DbCommandInterceptor
    {
        public bool Fired{get;private set;}
        public override ValueTask<System.Data.Common.DbDataReader> ReaderExecutedAsync(System.Data.Common.DbCommand command,Microsoft.EntityFrameworkCore.Diagnostics.CommandExecutedEventData eventData,System.Data.Common.DbDataReader result,CancellationToken ct=default)
        {
            if(!Fired&&command.CommandText.Contains("[Schedule]",StringComparison.Ordinal)&&eventData.Context is EnglishCenter.Api.Infrastructure.EnglishCenterDbContext db&&db.ReplayOperations.Local.Count>0){Fired=true;time.Now=cutoff;}
            return ValueTask.FromResult(result);
        }
    }
    [Fact(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    public async Task MembershipDriftWithoutParentRvRejectsStalePreview_AnyBookingHistoryBlocksSchedule()
    {
        await using var f=await OwnedSqlFixture.CreateAsync();var token=f.Token(await f.Grant("AcademicAffairs"));var executor=Sessions(f);var (course,_)=await References(f);var created=await executor.CommandAsync(token,Input(55,0,Window(f,f.Accounts["Teacher"])),Ct);var preview=await executor.CancellationAsync(token,created.Session.Id,Ct);var manifest=JsonSerializer.SerializeToElement(preview.Manifest,new JsonSerializerOptions(JsonSerializerDefaults.Web));
        int bookingId;await using(var db=f.Db()){var student=await db.StudentProfiles.SingleAsync(Ct);var reg=new Registration{StudentId=student.StudentId,CourseId=course.CourseId,Status="PENDING",CreatedAt=f.Clock.GetUtcNow().UtcDateTime};db.Registrations.Add(reg);await db.SaveChangesAsync(Ct);var attempt=new EntryTestAttempt{RegistrationId=reg.RegistrationId,AttemptNumber=1};db.EntryTestAttempts.Add(attempt);await db.SaveChangesAsync(Ct);var booking=new EntryTestBooking{SessionId=created.Session.Id,AttemptId=attempt.AttemptId,Status="SCHEDULED",BookedAt=f.Clock.GetUtcNow().UtcDateTime};db.EntryTestBookings.Add(booking);await db.SaveChangesAsync(Ct);bookingId=booking.BookingId;}
        Assert.Equal(created.Session.Version,(await executor.DetailAsync(token,created.Session.Id,Ct)).Version);
        var stale=Input(61,created.Session.Id,new{reason="Stale membership",expectedVersions=manifest.GetProperty("expectedVersions"),expectedSet=manifest.GetProperty("expectedSet")});Assert.Equal(ApiErrorCode.STATE_CONFLICT,(await Assert.ThrowsAsync<ApiErrorException>(()=>executor.CommandAsync(token,stale,Ct))).Code);
        await using(var db=f.Db()){var booking=await db.EntryTestBookings.SingleAsync(x=>x.BookingId==bookingId,Ct);booking.Status="CANCELLED";booking.CancelledBy=token.AccountId;booking.CancelledAt=f.Clock.GetUtcNow().UtcDateTime;booking.CancelledFromStatus="SCHEDULED";booking.CancellationReason="Synthetic history";booking.CancellationOperationId=Guid.NewGuid();await db.SaveChangesAsync(Ct);}
        var detail=await executor.DetailAsync(token,created.Session.Id,Ct);Assert.Equal(0,detail.ScheduledBookingCount);Assert.True(detail.HasBookingHistory);Assert.DoesNotContain("EDIT_SCHEDULE",detail.AllowedActions);
        var edit=Input(57,detail.Id,new{date=detail.Date,startTime=detail.StartTime,endTime=detail.EndTime,room="NEW",expectedVersions=new{session=detail.Version}});Assert.Equal(ApiErrorCode.STATE_CONFLICT,(await Assert.ThrowsAsync<ApiErrorException>(()=>executor.CommandAsync(token,edit,Ct))).Code);
        await using var check=f.Db();Assert.Equal(1,await check.ReplayOperations.CountAsync(Ct));Assert.Equal("CLOSED",(await check.EntryTestSessions.FindAsync([detail.Id],Ct))!.Status);
    }
    [Fact(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    public async Task CurrentClassScheduleBlocksRoomAndGraderButAllowsAdjacency()
    {
        await using var f=await OwnedSqlFixture.CreateAsync();var token=f.Token(await f.Grant("AcademicAffairs"));var (course,band)=await References(f);var programs=Programs(f);
        var draft=await programs.CommandAsync(token,Input(82,0,new{courseId=course.CourseId,bandId=band.BandId,name="Schedule fixture",expectedLessonCount=1,components=new[]{new{name="Exam",weight="100",kind="FINAL_EXAM"}},expectedVersions=new{course=RowVersionCodec.Encode(course.RowVersion),band=RowVersionCodec.Encode(band.RowVersion)}}),Ct);
        var active=await programs.CommandAsync(token,Input(87,draft.Program.Id,(await programs.PreviewAsync(token,draft.Program.Id,Ct)).Manifest!),Ct);
        var window=SessionWindow.Parse(JsonSerializer.SerializeToElement(Window(f,f.Accounts["Teacher"]),new JsonSerializerOptions(JsonSerializerDefaults.Web)));
        await using(var db=f.Db()){
            var cls=new EnglishCenter.Api.Domain.Training.Class{ProgramId=active.Program.Id,TeacherId=f.Accounts["Teacher"],ClassCode="OVERLAP",Status="PLANNED",MaxSize=10,CreatedAt=f.Clock.GetUtcNow().UtcDateTime};db.Classes.Add(cls);await db.SaveChangesAsync(Ct);
            db.Schedules.Add(new EnglishCenter.Api.Domain.Training.Schedule{ClassId=cls.ClassId,SessionType="LESSON",ObligationKey=Guid.NewGuid(),IsCurrent=true,SessionDate=window.Date,StartTime=window.StartTime,EndTime=window.EndTime,Room=window.Room,OccurrenceState="PLANNED"});await db.SaveChangesAsync(Ct);
        }
        var executor=Sessions(f);Assert.Equal(ApiErrorCode.STATE_CONFLICT,(await Assert.ThrowsAsync<ApiErrorException>(()=>executor.CommandAsync(token,Input(55,0,Window(f,f.Accounts["Teacher"],room:"OTHER")),Ct))).Code);
        await using(var db=f.Db()){var conflicts=await SqlEntrySessionRepository.FindConflicts(db,window,f.Accounts["Teacher"],0,Ct);Assert.Equal(new[]{"ROOM","GRADER"},conflicts.Select(x=>x.Resource));}
        var adjacent=await executor.CommandAsync(token,Input(55,0,Window(f,f.Accounts["Teacher"],start:"10:00",end:"11:00")),Ct);Assert.Equal("CLOSED",adjacent.Session.Status);
    }
    [Theory(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    [InlineData(true)][InlineData(false)]
    public async Task TwoClientsUniqueProgramPairOrTimetableCommitOnce(bool program)
    {
        await using var f=await OwnedSqlFixture.CreateAsync();var token=f.Token(await f.Grant("AcademicAffairs"));var (course,band)=await References(f);
        async Task<bool> Attempt(){try{if(program)await Programs(f).CommandAsync(token,Input(82,0,new{courseId=course.CourseId,bandId=band.BandId,name="Concurrent",expectedLessonCount=1,components=Array.Empty<object>(),expectedVersions=new{course=RowVersionCodec.Encode(course.RowVersion),band=RowVersionCodec.Encode(band.RowVersion)}}),Ct);else await Sessions(f).CommandAsync(token,Input(55,0,Window(f,f.Accounts["Teacher"])),Ct);return true;}catch(ApiErrorException e){Assert.Contains(e.Code,new[]{ApiErrorCode.CONFLICT,ApiErrorCode.STATE_CONFLICT,ApiErrorCode.SERVICE_UNAVAILABLE});return false;}}
        var results=await Task.WhenAll(Attempt(),Attempt());Assert.Single(results,x=>x);await using var db=f.Db();Assert.Equal(1,await db.ReplayOperations.CountAsync(Ct));Assert.Equal(program?1:0,await db.TrainingPrograms.CountAsync(Ct));Assert.Equal(program?0:1,await db.EntryTestSessions.CountAsync(Ct));
    }
    [Fact(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    public async Task FirstClassAndReturnToDraftSerializeOnProgramGate()
    {
        await using var f=await OwnedSqlFixture.CreateAsync();var token=f.Token(await f.Grant("AcademicAffairs"));var (course,band)=await References(f);var executor=Programs(f);var draft=await executor.CommandAsync(token,Input(82,0,new{courseId=course.CourseId,bandId=band.BandId,name="First use",expectedLessonCount=1,components=new[]{new{name="Exam",weight="100",kind="FINAL_EXAM"}},expectedVersions=new{course=RowVersionCodec.Encode(course.RowVersion),band=RowVersionCodec.Encode(band.RowVersion)}}),Ct);var active=await executor.CommandAsync(token,Input(87,draft.Program.Id,(await executor.PreviewAsync(token,draft.Program.Id,Ct)).Manifest!),Ct);
        async Task<bool> FirstUse(){await using var db=f.Db();await using var tx=await db.Database.BeginTransactionAsync(Ct);await db.Database.ExecuteSqlInterpolatedAsync($"DECLARE @r int;EXEC @r=sys.sp_getapplock @Resource={"EC:PROGRAM:"+active.Program.Id},@LockMode='Exclusive',@LockOwner='Transaction',@DbPrincipal='public',@LockTimeout=1000;IF @r<0 THROW 51000,'Fixture gate failed',1;",Ct);var p=await db.TrainingPrograms.FromSqlInterpolated($"SELECT * FROM dbo.TrainingProgram WITH(UPDLOCK,HOLDLOCK) WHERE program_id={active.Program.Id}").SingleAsync(Ct);if(p.Status!="ACTIVE")return false;db.Classes.Add(new EnglishCenter.Api.Domain.Training.Class{ProgramId=p.ProgramId,TeacherId=f.Accounts["Teacher"],ClassCode="FIRST",Status="PLANNED",MaxSize=10,CreatedAt=f.Clock.GetUtcNow().UtcDateTime});await db.SaveChangesAsync(Ct);await tx.CommitAsync(Ct);return true;}
        async Task<bool> Return(){try{await executor.CommandAsync(token,Input(88,active.Program.Id,new{expectedVersions=new{program=active.Program.Version}}),Ct);return true;}catch(ApiErrorException e){Assert.Equal(ApiErrorCode.STATE_CONFLICT,e.Code);return false;}}
        var results=await Task.WhenAll(FirstUse(),Return());Assert.Single(results,x=>x);var detail=await executor.DetailAsync(token,active.Program.Id,Ct);Assert.Equal(results[0]?"ACTIVE":"DRAFT",detail.Status);Assert.Equal(results[0],detail.IsConfigurationFrozen);
    }
}
