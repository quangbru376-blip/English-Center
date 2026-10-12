namespace EnglishCenter.Tests.Api.Integration;
using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Common.Security;
using EnglishCenter.Api.Common.Serialization;
using EnglishCenter.Api.Domain.Academics;
using EnglishCenter.Api.Infrastructure.Academics;
using EnglishCenter.Api.Infrastructure.Classes;
using EnglishCenter.Api.Infrastructure.Finance;
using EnglishCenter.Api.Services.Academics;
using EnglishCenter.Api.Services.Classes;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Xunit;
using System.Net.Http.Json;
using System.Data.Common;
using EnglishCenter.Api.Domain.Identity;
using Microsoft.EntityFrameworkCore.Diagnostics;
using EnglishCenter.Api.Domain.Admissions;
using EnglishCenter.Api.Domain.Finance;
[Collection("V01 HTTP SQL")]
public sealed class M2ClassSqlTests
{
    private const string OptIn="Assigned M2 batch2 owned scratch SQL with identity-paired cleanup.";
    private static CancellationToken Ct=>OwnedSqlFixture.Ct;
    private static readonly JsonSerializerOptions Wire=new(JsonSerializerDefaults.Web);
    internal static SqlClassExecutor Ex(OwnedSqlFixture f)=>new(()=>f.Db(),f.Clock,10,1000);
    internal static async Task<(VerifiedSessionToken Token,int Program,int Registration)> Seed(OwnedSqlFixture f,bool paid=true)
    {
        var invoice=await M2FinanceSqlTests.Seed(f);var accountant=f.Token(await f.Grant("Accountant"));var finance=new SqlFinanceExecutor(()=>f.Db(),f.Clock,10,1000);
        if(paid){var collection=await finance.PreviewAsync(accountant,invoice,Ct);var body=JsonSerializer.SerializeToNode(collection.Manifest,Wire)!.AsObject();body["amount"]=collection.Amount;body["method"]="CASH";await finance.CommandAsync(accountant,51,invoice,Guid.NewGuid(),body.ToJsonString(),65536,Ct);}
        int registration,courseId,bandId;string courseRv,bandRv;await using(var db=f.Db()){var inv=await db.Invoices.SingleAsync(x=>x.InvoiceId==invoice,Ct);registration=inv.RegistrationId;var reg=await db.Registrations.SingleAsync(x=>x.RegistrationId==registration,Ct);var band=await db.BandAssignments.SingleAsync(x=>x.RegistrationId==registration,Ct);courseId=reg.CourseId;bandId=band.BandId;courseRv=ClassWire.Rv((await db.Courses.SingleAsync(x=>x.CourseId==courseId,Ct)).RowVersion);bandRv=ClassWire.Rv((await db.Bands.SingleAsync(x=>x.BandId==bandId,Ct)).RowVersion);}
        var token=f.Token(await f.Grant("AcademicAffairs"));var programs=new SqlProgramExecutor(()=>f.Db(),f.Clock,10,1000);var create=new{courseId,bandId,name="Synthetic batch2",expectedLessonCount=2,components=new[]{new{name="Final",weight="100.00",kind="FINAL_EXAM"}},expectedVersions=new{course=courseRv,band=bandRv}};var draft=await programs.CommandAsync(token,AcademicInput.Parse(82,0,Guid.NewGuid(),JsonSerializer.Serialize(create,Wire),65536),Ct);var id=draft.Program.Id;var activation=await programs.PreviewAsync(token,id,Ct);await programs.CommandAsync(token,AcademicInput.Parse(87,id,Guid.NewGuid(),JsonSerializer.Serialize(activation.Manifest,Wire),65536),Ct);return(token,id,registration);
    }
    internal static async Task<JsonObject> CreateBody(OwnedSqlFixture f,int program,string code="CLASS_A",int size=15)
    {await using var db=f.Db();var p=await SqlProgramRepository.ReadState(db,program,Ct);var a=await db.Accounts.SingleAsync(x=>x.AccountId==f.Accounts["Teacher"],Ct);return ClassWire.Object(new{classCode=code,programId=program,teacherId=a.AccountId,maxSize=size,expectedVersions=new{program=ClassWire.Rv(p.Program.RowVersion),teacher=ClassWire.Rv(a.RowVersion),components=ProgramProjection.References(p)},expectedSet=new{componentIds=p.Components.Select(x=>x.ComponentId)}});}
    internal static async Task<int> Create(OwnedSqlFixture f,VerifiedSessionToken token,int program,string code="CLASS_A",int size=15)=> (await Ex(f).Command(token,92,0,Guid.NewGuid(),(await CreateBody(f,program,code,size)).ToJsonString(),65536,Ct)).Result["class"]!["id"]!.GetValue<int>();
    internal static JsonObject Window(OwnedSqlFixture f,string type="LESSON",string start="09:00",string end="10:00",string room="CLASS_ROOM",int days=1)=>ClassWire.Object(new{type,date=f.Clock.GetToday().AddDays(days).ToString("yyyy-MM-dd"),startTime=start,endTime=end,room});
    internal static async Task<JsonObject> ScheduleBody(OwnedSqlFixture f,VerifiedSessionToken t,int id,JsonObject window)
    {var p=await Ex(f).Preview(t,id,"schedule",new JsonObject{["candidate"]=window.DeepClone()}.ToJsonString(),Ct);Assert.True(p["eligible"]!.GetValue<bool>(),p.ToJsonString());var body=p["manifest"]!.DeepClone().AsObject();body["candidate"]=window.DeepClone();return body;}
    internal static async Task Schedule(OwnedSqlFixture f,VerifiedSessionToken t,int id,JsonObject window)=>await Ex(f).Command(t,118,id,Guid.NewGuid(),(await ScheduleBody(f,t,id,window)).ToJsonString(),65536,Ct);
    internal static async Task Open(OwnedSqlFixture f,VerifiedSessionToken t,int id)
    {var p=await Ex(f).Preview(t,id,"transition","{\"action\":\"OPEN\"}",Ct);Assert.True(p["eligible"]!.GetValue<bool>(),p.ToJsonString());await Ex(f).Command(t,99,id,Guid.NewGuid(),p["manifest"]!.ToJsonString(),65536,Ct);}
    private static async Task<ApiErrorCode> Error(Func<Task> work)=>(await Assert.ThrowsAsync<ApiErrorException>(work)).Code;
    private static async Task<int> SecondTeacher(OwnedSqlFixture f)
    {await using var db=f.Db();var role=await db.Roles.SingleAsync(x=>x.RoleCode=="Teacher",Ct);var a=new Account{RoleId=role.RoleId,FullName="Synthetic Teacher2",Email="teacher2@example.invalid",PasswordHash=f.Hasher.Hash(f.Password),Status="ACTIVE",SecurityVersion=1,CreatedAt=f.Clock.GetUtcNow().UtcDateTime};db.Accounts.Add(a);await db.SaveChangesAsync(Ct);f.Accounts["Teacher2"]=a.AccountId;return a.AccountId;}
    private sealed class Cutoff(M1AdmissionsSqlTests.Clock time,DateTimeOffset target):DbCommandInterceptor
    {
        public bool Fired{get;private set;}
        public override ValueTask<DbDataReader> ReaderExecutedAsync(DbCommand cmd,CommandExecutedEventData data,DbDataReader reader,CancellationToken ct=default)
        {if(!Fired&&cmd.CommandText.Contains("[Schedule]",StringComparison.Ordinal)&&cmd.CommandText.Contains("INSERT",StringComparison.Ordinal)){time.Now=target;Fired=true;}return ValueTask.FromResult(reader);}
    }
    internal static async Task<int> SecondPaid(OwnedSqlFixture f,int source)
    {
        await using var db=f.Db();var original=await db.Registrations.SingleAsync(x=>x.RegistrationId==source,Ct);var role=await db.Roles.SingleAsync(x=>x.RoleCode=="Student",Ct);var now=f.Clock.GetUtcNow().UtcDateTime;
        var account=new Account{RoleId=role.RoleId,FullName="Synthetic Student2",Email="student2@example.invalid",Phone="0987654321",PasswordHash=f.Hasher.Hash(f.Password),Status="ACTIVE",SecurityVersion=1,CreatedAt=now};db.Accounts.Add(account);await db.SaveChangesAsync(Ct);f.Accounts["Student2"]=account.AccountId;var profile=new StudentProfile{AccountId=account.AccountId,DateOfBirth=new(2000,1,1)};db.StudentProfiles.Add(profile);await db.SaveChangesAsync(Ct);
        var reg=new Registration{CourseId=original.CourseId,StudentId=profile.StudentId,Status="ADMITTED",CreatedAt=now,AdmissionConfirmedAt=now,AdmissionApprovedAt=now,AdmissionApprovedBy=f.Accounts["Admissions"]};db.Registrations.Add(reg);await db.SaveChangesAsync(Ct);
        var attempt=new EntryTestAttempt{RegistrationId=reg.RegistrationId,AttemptNumber=1};db.EntryTestAttempts.Add(attempt);await db.SaveChangesAsync(Ct);var existing=await db.EntryTestSessions.OrderBy(x=>x.SessionId).FirstAsync(Ct);var booking=new EntryTestBooking{AttemptId=attempt.AttemptId,SessionId=existing.SessionId,Status="COMPLETED",BookedAt=now.AddDays(-1)};db.EntryTestBookings.Add(booking);await db.SaveChangesAsync(Ct);attempt.Score=0;attempt.GradedBy=f.Accounts["Teacher"];attempt.GradedAt=now;attempt.CompletedBookingId=booking.BookingId;
        var band=(await db.BandAssignments.SingleAsync(x=>x.RegistrationId==source,Ct)).BandId;db.BandAssignments.Add(new(){RegistrationId=reg.RegistrationId,BestAttemptId=attempt.AttemptId,BandId=band,AssignedAt=now});var invoice=new Invoice{RegistrationId=reg.RegistrationId,InvoiceNumber="HD-2026-99999999",Amount=1000,IssuedAt=now,DueDate=f.Clock.GetToday().AddDays(7),Status="UNPAID"};db.Invoices.Add(invoice);await db.SaveChangesAsync(Ct);
        var accountant=f.Token(await f.Grant("Accountant"));var finance=new SqlFinanceExecutor(()=>f.Db(),f.Clock,10,1000);var preview=await finance.PreviewAsync(accountant,invoice.InvoiceId,Ct);var b=JsonSerializer.SerializeToNode(preview.Manifest,Wire)!.AsObject();b["amount"]=preview.Amount;b["method"]="CASH";await finance.CommandAsync(accountant,51,invoice.InvoiceId,Guid.NewGuid(),b.ToJsonString(),65536,Ct);return reg.RegistrationId;
    }
    [Fact(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    public async Task TwoPaidCandidatesRaceForLastSlotOneAssignmentDraftAuditAndCompletion()
    {
        await using var f=await OwnedSqlFixture.CreateAsync(new M1AdmissionsSqlTests.Clock());var(t,p,r)=await Seed(f);var second=await SecondPaid(f,r);var id=await Create(f,t,p,size:1);await Schedule(f,t,id,Window(f));await Open(f,t,id);var ex=Ex(f);
        // Both previews must be prepared before either command can take the slot.
        var bodies=new List<JsonObject>();foreach(var registration in new[]{r,second}){var preview=await ex.Preview(t,id,"placement",$"{{\"registrationId\":{registration}}}",Ct);Assert.True(preview["eligible"]!.GetValue<bool>());var b=preview["manifest"]!.DeepClone().AsObject();b["registrationId"]=registration;bodies.Add(b);}
        async Task<bool> Command(JsonObject b){try{await ex.Command(t,108,id,Guid.NewGuid(),b.ToJsonString(),65536,Ct);return true;}catch(ApiErrorException e){Assert.Equal(ApiErrorCode.STATE_CONFLICT,e.Code);return false;}}
        var winners=await Task.WhenAll(bodies.Select(Command));Assert.Single(winners,x=>x);await using var db=f.Db();Assert.Single(await db.StudentClassAssignments.ToArrayAsync(Ct));Assert.Single(await db.GradeReports.ToArrayAsync(Ct));Assert.Equal(1,await db.ReplayOperations.CountAsync(x=>x.ActionCode=="ACADEMIC.CLASS.PLACE_REGISTRATION",Ct));Assert.Equal(2,await db.AuditLogs.CountAsync(x=>x.Action.StartsWith("ACADEMIC.CLASS.PLACE_REGISTRATION"),Ct));
    }
    [Fact(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    public async Task NewStudentBookingInvalidatesPlacementFullTimetableAndBlocksNewCohortSchedule()
    {
        await using var f=await OwnedSqlFixture.CreateAsync(new M1AdmissionsSqlTests.Clock());var(t,p,r)=await Seed(f);var id=await Create(f,t,p);await Schedule(f,t,id,Window(f));await Open(f,t,id);var ex=Ex(f);var preview=await ex.Preview(t,id,"placement",$"{{\"registrationId\":{r}}}",Ct);var stale=preview["manifest"]!.DeepClone().AsObject();stale["registrationId"]=r;var otherTeacher=await SecondTeacher(f);
        await using(var db=f.Db()){var student=(await db.Registrations.SingleAsync(x=>x.RegistrationId==r,Ct)).StudentId;var course=new Course{CourseCode="ANOTHER",CourseName="Synthetic other course",BasePrice=1000,Status="ACTIVE"};db.Courses.Add(course);await db.SaveChangesAsync(Ct);var reg=new Registration{CourseId=course.CourseId,StudentId=student,Status="TESTING",CreatedAt=f.Clock.GetUtcNow().UtcDateTime};db.Registrations.Add(reg);await db.SaveChangesAsync(Ct);var attempt=new EntryTestAttempt{RegistrationId=reg.RegistrationId,AttemptNumber=1};db.EntryTestAttempts.Add(attempt);var start=EnglishCenter.Api.Common.Time.VietnamTime.ToUtc(f.Clock.GetToday().AddDays(1),new TimeOnly(10,0)).UtcDateTime;var session=new EntryTestSession{StartAt=start,EndAt=start.AddHours(1),Room="OTHER_ROOM",GraderId=otherTeacher,Status="OPEN",Capacity=20};db.EntryTestSessions.Add(session);await db.SaveChangesAsync(Ct);db.EntryTestBookings.Add(new(){AttemptId=attempt.AttemptId,SessionId=session.SessionId,Status="SCHEDULED",BookedAt=f.Clock.GetUtcNow().UtcDateTime});await db.SaveChangesAsync(Ct);}
        Assert.Equal(ApiErrorCode.STATE_CONFLICT,await Error(()=>ex.Command(t,108,id,Guid.NewGuid(),stale.ToJsonString(),65536,Ct)));var fresh=await ex.Preview(t,id,"placement",$"{{\"registrationId\":{r}}}",Ct);Assert.True(fresh["eligible"]!.GetValue<bool>());var body=fresh["manifest"]!.DeepClone().AsObject();body["registrationId"]=r;await ex.Command(t,108,id,Guid.NewGuid(),body.ToJsonString(),65536,Ct);var blocked=await ex.Preview(t,id,"schedule",new JsonObject{["candidate"]=Window(f,start:"10:00",end:"11:00")}.ToJsonString(),Ct);Assert.False(blocked["eligible"]!.GetValue<bool>());Assert.Contains(blocked["conflicts"]!.AsArray(),x=>x!["kind"]!.GetValue<string>()=="STUDENT");Assert.Null(blocked["manifest"]);
    }
    [Fact(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    public async Task TeacherChangeRequiresFullContextAndOldTeacherImmediatelyLosesAllClassScope()
    {
        await using var f=await OwnedSqlFixture.CreateAsync(new M1AdmissionsSqlTests.Clock());var(t,p,_)=await Seed(f);var id=await Create(f,t,p);await Schedule(f,t,id,Window(f));var ex=Ex(f);var teacher=f.Token(await f.Grant("Teacher"));Assert.Single((await ex.Search(teacher,QueryCollection.Empty,"Teacher",Ct))["items"]!.AsArray());var next=await SecondTeacher(f);var preview=await ex.Preview(t,id,"teacher",$"{{\"teacherId\":{next}}}",Ct);Assert.True(preview["eligible"]!.GetValue<bool>());var b=preview["manifest"]!.DeepClone().AsObject();b["teacherId"]=next;var stale=b.DeepClone().AsObject();stale["expectedVersions"]!["schedules"]=new JsonArray();stale["expectedSet"]!["scheduleBindings"]=new JsonArray();Assert.Equal(ApiErrorCode.STATE_CONFLICT,await Error(()=>ex.Command(t,97,id,Guid.NewGuid(),stale.ToJsonString(),65536,Ct)));var changed=await ex.Command(t,97,id,Guid.NewGuid(),b.ToJsonString(),65536,Ct);Assert.Equal(next,changed.Result["class"]!["teacher"]!["accountId"]!.GetValue<int>());Assert.Empty((await ex.Search(teacher,QueryCollection.Empty,"Teacher",Ct))["items"]!.AsArray());Assert.Equal(ApiErrorCode.NOT_FOUND,await Error(()=>ex.Detail(teacher,id,"Teacher",Ct)));Assert.Equal(ApiErrorCode.NOT_FOUND,await Error(()=>ex.Roster(teacher,id,ClassContactFilter.Parse("{}",false),"Teacher",Ct)));Assert.Equal(id,(await ex.Detail(f.Token(await f.Grant("Teacher2")),id,"Teacher",Ct))["id"]!.GetValue<int>());
    }
    [Fact(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    public async Task CapacityNoopAndStaleMembershipHaveNoMutationMarker()
    {
        await using var f=await OwnedSqlFixture.CreateAsync(new M1AdmissionsSqlTests.Clock());var(t,p,_)=await Seed(f);var id=await Create(f,t,p);var ex=Ex(f);var d=await ex.Detail(t,id,"AcademicAffairs",Ct);var b=ClassWire.Object(new{maxSize=14,expectedVersions=new{@class=d["version"]!.GetValue<string>()},expectedSet=new{assignmentIds=Array.Empty<int>()}});var key=Guid.NewGuid();var changed=await ex.Command(t,94,id,key,b.ToJsonString(),65536,Ct);Assert.Equal(14,changed.Result["class"]!["maxSize"]!.GetValue<int>());Assert.True((await ex.Command(t,94,id,key,b.ToJsonString(),65536,Ct)).Replayed);Assert.Equal(ApiErrorCode.STATE_CONFLICT,await Error(()=>ex.Command(t,94,id,Guid.NewGuid(),b.ToJsonString(),65536,Ct)));b["expectedVersions"]!["class"]=changed.Result["class"]!["version"]!.DeepClone();Assert.Equal(ApiErrorCode.VALIDATION_ERROR,await Error(()=>ex.Command(t,94,id,Guid.NewGuid(),b.ToJsonString(),65536,Ct)));b["maxSize"]=13;b["expectedSet"]!["assignmentIds"]=new JsonArray(2147483647);Assert.Equal(ApiErrorCode.STATE_CONFLICT,await Error(()=>ex.Command(t,94,id,Guid.NewGuid(),b.ToJsonString(),65536,Ct)));await using var db=f.Db();Assert.Equal(1,await db.ReplayOperations.CountAsync(x=>x.ActionCode=="ACADEMIC.CLASS.UPDATE_CAPACITY",Ct));
    }
    [Fact(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    public async Task CandidateUnpaidIsHiddenAndPlacedClassCannotCancel()
    {
        await using var f=await OwnedSqlFixture.CreateAsync(new M1AdmissionsSqlTests.Clock());var(t,p,r)=await Seed(f,false);var id=await Create(f,t,p,size:1);var ex=Ex(f);Assert.Empty((await ex.CandidateSearch(t,ClassContactFilter.Parse("{}",true),Ct))["items"]!.AsArray());Assert.Equal(ApiErrorCode.NOT_FOUND,await Error(()=>ex.Candidate(t,r,Ct)));await using(var db=f.Db()){var inv=await db.Invoices.SingleAsync(Ct);var finance=new SqlFinanceExecutor(()=>f.Db(),f.Clock,10,1000);var accountant=f.Token(await f.Grant("Accountant"));var preview=await finance.PreviewAsync(accountant,inv.InvoiceId,Ct);var body=JsonSerializer.SerializeToNode(preview.Manifest,Wire)!.AsObject();body["amount"]=preview.Amount;body["method"]="CASH";await finance.CommandAsync(accountant,51,inv.InvoiceId,Guid.NewGuid(),body.ToJsonString(),65536,Ct);}
        await Schedule(f,t,id,Window(f));await Open(f,t,id);var placement=await ex.Preview(t,id,"placement",$"{{\"registrationId\":{r}}}",Ct);var b=placement["manifest"]!.DeepClone().AsObject();b["registrationId"]=r;await ex.Command(t,108,id,Guid.NewGuid(),b.ToJsonString(),65536,Ct);var cancel=await ex.Preview(t,id,"transition","{\"action\":\"CANCEL\"}",Ct);Assert.False(cancel["eligible"]!.GetValue<bool>());Assert.Null(cancel["manifest"]);Assert.Equal(ApiErrorCode.NOT_FOUND,await Error(()=>ex.Options(t,r,QueryCollection.Empty,Ct)));
    }
    [Fact(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    public async Task CrossingStartDuringScheduleSaveRollsBackRootAuditAndCompletion()
    {
        var time=new M1AdmissionsSqlTests.Clock();await using var f=await OwnedSqlFixture.CreateAsync(time);var(t,p,_)=await Seed(f);var id=await Create(f,t,p);var window=Window(f,days:0,start:"10:01",end:"11:01");var body=await ScheduleBody(f,t,id,window);var cutoff=new Cutoff(time,time.Now.AddMinutes(1));var ex=new SqlClassExecutor(()=>f.Db(cutoff),f.Clock,10,1000);var key=Guid.NewGuid();Assert.Equal(ApiErrorCode.STATE_CONFLICT,await Error(()=>ex.Command(t,118,id,key,body.ToJsonString(),65536,Ct)));Assert.True(cutoff.Fired);await using var db=f.Db();Assert.Empty(await db.Schedules.ToArrayAsync(Ct));Assert.False(await db.ReplayOperations.AnyAsync(x=>x.RequestId==key,Ct));Assert.Empty(await db.AuditLogs.Where(x=>x.Action.StartsWith("ACADEMIC.SCHEDULE.")).ToArrayAsync(Ct));
    }
    [Fact(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    public async Task RealHttpCookieCsrfTypedCommandsRoleRefusalsAndSharedLookup()
    {
        await using var f=await OwnedSqlFixture.CreateAsync(new M1AdmissionsSqlTests.Clock());var(t,p,_)=await Seed(f);await using var host=await M1HttpSqlTests.Host.Create(f);await host.Login("AcademicAffairs");var body=await CreateBody(f,p);var key=Guid.NewGuid();using(var denied=await host.Send(HttpMethod.Post,"/api/v1/academics/classes",body,key,false))Assert.Equal(HttpStatusCode.Forbidden,denied.StatusCode);int id;
        using(var created=await host.Send(HttpMethod.Post,"/api/v1/academics/classes",body,key)){Assert.Equal(HttpStatusCode.Created,created.StatusCode);Assert.Contains("no-store",created.Headers.CacheControl!.ToString());id=(await created.Content.ReadFromJsonAsync<JsonElement>(Ct)).GetProperty("class").GetProperty("id").GetInt32();}
        using(var replay=await host.Send(HttpMethod.Post,"/api/v1/academics/classes",body,key))Assert.Equal(HttpStatusCode.OK,replay.StatusCode);
        using(var marker=await host.Send(HttpMethod.Get,$"/api/v1/admissions/command-results/{key:D}?action=ACADEMIC.CLASS.CREATE"))Assert.Equal(HttpStatusCode.OK,marker.StatusCode);
        using(var deferred=await host.Send(HttpMethod.Get,$"/api/v1/admissions/command-results/{key:D}?action=ACADEMIC.CLASS.START"))Assert.Equal(HttpStatusCode.Forbidden,deferred.StatusCode);
        using(var wrongQuery=await host.Send(HttpMethod.Get,"/api/v1/academics/classes?name=secret"))Assert.Equal(HttpStatusCode.BadRequest,wrongQuery.StatusCode);
        await host.Login("Teacher");using(var academic=await host.Send(HttpMethod.Get,$"/api/v1/academics/classes/{id}"))Assert.Equal(HttpStatusCode.Forbidden,academic.StatusCode);using(var own=await host.Send(HttpMethod.Get,$"/api/v1/academics/teachers/me/classes/{id}")){Assert.Equal(HttpStatusCode.OK,own.StatusCode);var json=await own.Content.ReadAsStringAsync(Ct);Assert.DoesNotContain("guardContext",json);Assert.DoesNotContain("version",json);}
        await host.Login("Student");using(var notOwn=await host.Send(HttpMethod.Get,$"/api/v1/students/me/classes/{id}"))Assert.Equal(HttpStatusCode.NotFound,notOwn.StatusCode);
    }
    [Fact(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    public async Task FullLifecycleCommitsOnePlacementDraftAuditReplayAndPrivateRoleReads()
    {
        await using var f=await OwnedSqlFixture.CreateAsync(new M1AdmissionsSqlTests.Clock());var (t,p,r)=await Seed(f);var id=await Create(f,t,p);var ex=Ex(f);await Schedule(f,t,id,Window(f));await Open(f,t,id);
        Assert.Single((await ex.CandidateSearch(t,ClassContactFilter.Parse("{}",true),Ct))["items"]!.AsArray());var preview=await ex.Preview(t,id,"placement",$"{{\"registrationId\":{r}}}",Ct);Assert.True(preview["eligible"]!.GetValue<bool>());
        var b=preview["manifest"]!.DeepClone().AsObject();b["registrationId"]=r;var key=Guid.NewGuid();var result=await ex.Command(t,108,id,key,b.ToJsonString(),65536,Ct);Assert.False(result.Replayed);var replay=await ex.Command(t,108,id,key,b.ToJsonString(),65536,Ct);Assert.True(replay.Replayed);Assert.Equal(result.Result["operationId"]!.ToJsonString(),replay.Result["operationId"]!.ToJsonString());
        var marker=await ex.Lookup(t,"ACADEMIC.CLASS.PLACE_REGISTRATION",key,Ct);Assert.Equal(id,marker.Target["classId"]);
        await using(var db=f.Db()){Assert.Equal("ADMITTED",(await db.Registrations.SingleAsync(x=>x.RegistrationId==r,Ct)).Status);Assert.Single(await db.StudentClassAssignments.ToArrayAsync(Ct));Assert.Equal("DRAFT",(await db.GradeReports.SingleAsync(Ct)).Status);Assert.Empty(await db.Scores.ToArrayAsync(Ct));Assert.Empty(await db.Attendances.ToArrayAsync(Ct));Assert.Equal(2,await db.AuditLogs.CountAsync(x=>x.OperationId==marker.OperationId,Ct));}
        var student=f.Token(await f.Grant("Student"));var own=await ex.Detail(student,id,"Student",Ct);Assert.False(own.ContainsKey("version"));Assert.False(own.ContainsKey("guardContext"));Assert.False(own.ContainsKey("maxSize"));Assert.Equal(r,own["assignment"]!["registrationId"]!.GetValue<int>());
        var teacher=f.Token(await f.Grant("Teacher"));var roster=await ex.Roster(teacher,id,ClassContactFilter.Parse("{}",false),"Teacher",Ct);Assert.Single(roster["items"]!.AsArray());Assert.DoesNotContain("invoice",roster.ToJsonString());Assert.DoesNotContain("entryScore",roster.ToJsonString());Assert.Equal(ApiErrorCode.NOT_FOUND,await Error(()=>ex.Candidate(t,r,Ct)));
        var context=await ex.Scheduling(t,id,Ct);Assert.Single(context["assignments"]!.AsArray());Assert.Single(context["schedules"]!.AsArray());var calendar=await ex.Calendar(t,new QueryCollection(new Dictionary<string,Microsoft.Extensions.Primitives.StringValues>{{"fromDate",f.Clock.GetToday().ToString("yyyy-MM-dd")}}),Ct);Assert.Single(calendar["items"]!.AsArray());
    }
    [Fact(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    public async Task CreateUniqueReplayFreezeCancellationKeepsHistoryAndCannotReleaseProgram()
    {
        await using var f=await OwnedSqlFixture.CreateAsync(new M1AdmissionsSqlTests.Clock());var(t,p,_)=await Seed(f);var ex=Ex(f);var body=await CreateBody(f,p);var key=Guid.NewGuid();var create=await ex.Command(t,92,0,key,body.ToJsonString(),65536,Ct);var id=create.Result["class"]!["id"]!.GetValue<int>();Assert.True((await ex.Command(t,92,0,key,body.ToJsonString(),65536,Ct)).Replayed);Assert.Equal(ApiErrorCode.CONFLICT,await Error(()=>ex.Command(t,92,0,Guid.NewGuid(),body.ToJsonString(),65536,Ct)));
        var before=await new SqlProgramExecutor(()=>f.Db(),f.Clock,10,1000).DetailAsync(t,p,Ct);Assert.True(before.IsConfigurationFrozen);
        var preview=await ex.Preview(t,id,"transition","{\"action\":\"CANCEL\"}",Ct);var b=preview["manifest"]!.DeepClone().AsObject();b["reason"]="Synthetic cancellation";var canceled=await ex.Command(t,102,id,Guid.NewGuid(),b.ToJsonString(),65536,Ct);Assert.Equal("CANCELLED",canceled.Result["class"]!["status"]!.GetValue<string>());Assert.True((await new SqlProgramExecutor(()=>f.Db(),f.Clock,10,1000).DetailAsync(t,p,Ct)).IsConfigurationFrozen);Assert.Equal(ApiErrorCode.CONFLICT,await Error(()=>ex.Command(t,92,0,Guid.NewGuid(),body.ToJsonString(),65536,Ct)));
    }
    [Fact(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    public async Task InsertedScheduleInvalidatesManifestWithoutChangingParentVersionAndAdjacentSlotWorks()
    {
        await using var f=await OwnedSqlFixture.CreateAsync(new M1AdmissionsSqlTests.Clock());var(t,p,_)=await Seed(f);var id=await Create(f,t,p);var ex=Ex(f);var stale=await ScheduleBody(f,t,id,Window(f,start:"10:00",end:"11:00"));await Schedule(f,t,id,Window(f));Assert.Equal(ApiErrorCode.STATE_CONFLICT,await Error(()=>ex.Command(t,118,id,Guid.NewGuid(),stale.ToJsonString(),65536,Ct)));await Schedule(f,t,id,Window(f,start:"10:00",end:"11:00"));var blocked=await ex.Preview(t,id,"schedule",new JsonObject{["candidate"]=Window(f,start:"11:00",end:"12:00")}.ToJsonString(),Ct);Assert.False(blocked["eligible"]!.GetValue<bool>());await Schedule(f,t,id,Window(f,"FINAL_EXAM","11:00","12:00"));var context=await ex.Scheduling(t,id,Ct);Assert.Equal(0,context["progress"]!["remainingLessonObligationCount"]!.GetValue<int>());Assert.Equal(3,context["schedules"]!.AsArray().Count);
    }
    [Fact(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    public async Task ClosedEntrySessionReservesRoomAndTeacherAndNoLessonExamIsBlocked()
    {
        await using var f=await OwnedSqlFixture.CreateAsync(new M1AdmissionsSqlTests.Clock());var(t,p,_)=await Seed(f);var id=await Create(f,t,p);var ex=Ex(f);var exam=await ex.Preview(t,id,"schedule",new JsonObject{["candidate"]=Window(f,"FINAL_EXAM")}.ToJsonString(),Ct);Assert.False(exam["eligible"]!.GetValue<bool>());var w=Window(f);var session=new{date=w["date"]!.GetValue<string>(),startTime="09:00",endTime="10:00",room="CLASS_ROOM",graderId=f.Accounts["Teacher"],capacity=20};await new SqlEntrySessionExecutor(()=>f.Db(),f.Clock,10,1000).CommandAsync(t,AcademicInput.Parse(55,0,Guid.NewGuid(),JsonSerializer.Serialize(session,Wire),65536),Ct);var blocked=await ex.Preview(t,id,"schedule",new JsonObject{["candidate"]=w}.ToJsonString(),Ct);Assert.False(blocked["eligible"]!.GetValue<bool>());Assert.Equal(2,blocked["conflicts"]!.AsArray().Count);Assert.Null(blocked["manifest"]);
    }
    [Fact(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    public async Task ConcurrentIdenticalScheduleKeyHasOneEffectAndCurrentAuthorityRequiredForReplay()
    {
        await using var f=await OwnedSqlFixture.CreateAsync(new M1AdmissionsSqlTests.Clock());var(t,p,_)=await Seed(f);var id=await Create(f,t,p);var ex=Ex(f);var b=await ScheduleBody(f,t,id,Window(f));var key=Guid.NewGuid();var pair=await Task.WhenAll(ex.Command(t,118,id,key,b.ToJsonString(),65536,Ct),ex.Command(t,118,id,key,b.ToJsonString(),65536,Ct));Assert.Single(pair,x=>x.Replayed);Assert.Single(pair,x=>!x.Replayed);await using(var db=f.Db()){Assert.Single(await db.Schedules.ToArrayAsync(Ct));var a=await db.Accounts.SingleAsync(x=>x.AccountId==t.AccountId,Ct);a.Status="LOCKED";a.SecurityVersion++;await db.SaveChangesAsync(Ct);}Assert.Equal(ApiErrorCode.AUTHENTICATION_REQUIRED,await Error(()=>ex.Command(t,118,id,key,b.ToJsonString(),65536,Ct)));
    }
    [Fact(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    public async Task MandatoryAuditFailureRollsBackClassAndScheduleAndPlacementEffects()
    {
        await using var f=await OwnedSqlFixture.CreateAsync(new M1AdmissionsSqlTests.Clock());var(t,p,r)=await Seed(f);var b=await CreateBody(f,p);await using(var hold=await f.HoldAudit())await Assert.ThrowsAsync<EnglishCenter.Api.Infrastructure.Auditing.MandatoryAuditException>(()=>Ex(f).Command(t,92,0,Guid.NewGuid(),b.ToJsonString(),65536,Ct));var id=await Create(f,t,p);b=await ScheduleBody(f,t,id,Window(f));await using(var hold=await f.HoldAudit())await Assert.ThrowsAsync<EnglishCenter.Api.Infrastructure.Auditing.MandatoryAuditException>(()=>Ex(f).Command(t,118,id,Guid.NewGuid(),b.ToJsonString(),65536,Ct));await Schedule(f,t,id,Window(f));await Open(f,t,id);var preview=await Ex(f).Preview(t,id,"placement",$"{{\"registrationId\":{r}}}",Ct);b=preview["manifest"]!.DeepClone().AsObject();b["registrationId"]=r;await using(var hold=await f.HoldAudit())await Assert.ThrowsAsync<EnglishCenter.Api.Infrastructure.Auditing.MandatoryAuditException>(()=>Ex(f).Command(t,108,id,Guid.NewGuid(),b.ToJsonString(),65536,Ct));await using var db=f.Db();Assert.Single(await db.Classes.ToArrayAsync(Ct));Assert.Single(await db.Schedules.ToArrayAsync(Ct));Assert.Empty(await db.StudentClassAssignments.ToArrayAsync(Ct));Assert.Empty(await db.GradeReports.ToArrayAsync(Ct));
    }
    [Fact(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    public async Task SessionExpiresAfterInsertRollsBackScheduleAuditAndCompletion()
    {
        var time=new M1AdmissionsSqlTests.Clock();await using var f=await OwnedSqlFixture.CreateAsync(time);var(t,p,_)=await Seed(f);var id=await Create(f,t,p);var body=await ScheduleBody(f,t,id,Window(f));var cutoff=new Cutoff(time,time.Now.AddHours(24));var ex=new SqlClassExecutor(()=>f.Db(cutoff),f.Clock,10,1000);var key=Guid.NewGuid();Assert.Equal(ApiErrorCode.AUTHENTICATION_REQUIRED,await Error(()=>ex.Command(t,118,id,key,body.ToJsonString(),65536,Ct)));Assert.True(cutoff.Fired);await using var db=f.Db();Assert.Empty(await db.Schedules.ToArrayAsync(Ct));Assert.False(await db.ReplayOperations.AnyAsync(x=>x.RequestId==key,Ct));Assert.Empty(await db.AuditLogs.Where(x=>x.Action.StartsWith("ACADEMIC.SCHEDULE.")).ToArrayAsync(Ct));
    }
    [Fact(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    public async Task ConcurrentNewClassIntentsWithSameCodeCommitExactlyOne()
    {
        await using var f=await OwnedSqlFixture.CreateAsync(new M1AdmissionsSqlTests.Clock());var(t,p,_)=await Seed(f);var body=await CreateBody(f,p);var ex=Ex(f);
        async Task<bool> Send(){try{await ex.Command(t,92,0,Guid.NewGuid(),body.ToJsonString(),65536,Ct);return true;}catch(ApiErrorException e){Assert.Equal(ApiErrorCode.CONFLICT,e.Code);return false;}}
        var outcomes=await Task.WhenAll(Send(),Send());Assert.Single(outcomes,x=>x);Assert.Single(outcomes,x=>!x);await using var db=f.Db();Assert.Single(await db.Classes.ToArrayAsync(Ct));Assert.Single(await db.ReplayOperations.Where(x=>x.ActionCode=="ACADEMIC.CLASS.CREATE").ToArrayAsync(Ct));Assert.Single(await db.AuditLogs.Where(x=>x.Action=="ACADEMIC.CLASS.CREATE").ToArrayAsync(Ct));
    }
    [Fact(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    public async Task CancelAndPlacementRaceCannotCancelAnOccupiedClass()
    {
        await using var f=await OwnedSqlFixture.CreateAsync(new M1AdmissionsSqlTests.Clock());var(t,p,r)=await Seed(f);var id=await Create(f,t,p);await Schedule(f,t,id,Window(f));await Open(f,t,id);var ex=Ex(f);
        var cancel=(await ex.Preview(t,id,"transition","{\"action\":\"CANCEL\"}",Ct))["manifest"]!.DeepClone().AsObject();cancel["reason"]="Synthetic cancellation race";
        var placement=(await ex.Preview(t,id,"placement",$"{{\"registrationId\":{r}}}",Ct))["manifest"]!.DeepClone().AsObject();placement["registrationId"]=r;
        async Task<bool> Send(int api,JsonObject body){try{await ex.Command(t,api,id,Guid.NewGuid(),body.ToJsonString(),65536,Ct);return true;}catch(ApiErrorException e){Assert.Equal(ApiErrorCode.STATE_CONFLICT,e.Code);return false;}}
        var outcomes=await Task.WhenAll(Send(102,cancel),Send(108,placement));Assert.Single(outcomes,x=>x);Assert.Single(outcomes,x=>!x);await using var db=f.Db();var state=await db.Classes.SingleAsync(Ct);var count=await db.StudentClassAssignments.CountAsync(Ct);Assert.Equal(state.Status=="CANCELLED"?0:1,count);Assert.Equal(count,await db.GradeReports.CountAsync(Ct));Assert.Equal(1,await db.ReplayOperations.CountAsync(x=>x.ActionCode=="ACADEMIC.CLASS.CANCEL"||x.ActionCode=="ACADEMIC.CLASS.PLACE_REGISTRATION",Ct));
    }
}
