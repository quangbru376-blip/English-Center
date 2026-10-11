namespace EnglishCenter.Api.Infrastructure.Classes;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Domain.Admissions;
using EnglishCenter.Api.Domain.Finance;
using EnglishCenter.Api.Domain.Identity;
using EnglishCenter.Api.Domain.Training;
using EnglishCenter.Api.Infrastructure.Academics;
using EnglishCenter.Api.Infrastructure.Identity;
using EnglishCenter.Api.Services.Academics;
using EnglishCenter.Api.Services.Classes;
using Microsoft.EntityFrameworkCore;
public sealed class PlacementSource
{
    public Registration Registration {get;init;}=null!;
    public BandAssignment BandAssignment {get;init;}=null!;
    public Invoice Invoice {get;init;}=null!;
    public Account Student {get;init;}=null!;
    public EnglishCenter.Api.Domain.Academics.Course Course {get;init;}=null!;
    public EnglishCenter.Api.Domain.Academics.Band Band {get;init;}=null!;
    public EntryTestAttempt Best {get;init;}=null!;
    public PlacementSource(){}
    public PlacementSource(Registration r,BandAssignment b,Invoice i,Account a,EnglishCenter.Api.Domain.Academics.Course c,EnglishCenter.Api.Domain.Academics.Band band,EntryTestAttempt best){Registration=r;BandAssignment=b;Invoice=i;Student=a;Course=c;Band=band;Best=best;}
}
public sealed class SqlClassRepository:SqlAcademicRepository
{
    private readonly SqlAuthDeadlineScope scope;
    public SqlClassRepository(SqlAuthDeadlineScope scope):base(scope){this.scope=scope;}
    internal SqlAuthDeadlineScope Scope=>scope;

    public Task<Class?> Discover(int id,CancellationToken ct)=>Db.Classes.AsNoTracking().SingleOrDefaultAsync(x=>x.ClassId==id,ct);
    public Task<int?> InvoiceId(int registration,CancellationToken ct)=>Db.Invoices.Where(x=>x.RegistrationId==registration).Select(x=>(int?)x.InvoiceId).SingleOrDefaultAsync(ct);
    public Task<ProgramState> Program(int id,CancellationToken ct)=>new SqlProgramRepository(scope).StateAsync(id,ct);
    public async Task<Account> Teacher(int id,CancellationToken ct)
    {var a=await Db.Accounts.SingleOrDefaultAsync(x=>x.AccountId==id,ct)??throw new ApiErrorException(ApiErrorCode.NOT_FOUND);if(!await Db.Roles.AnyAsync(x=>x.RoleId==a.RoleId&&x.RoleCode=="Teacher",ct))throw new ApiErrorException(ApiErrorCode.NOT_FOUND);return a;}
    public Task<ClassState> State(int id,CancellationToken ct)=>ReadState(Db,id,ct);
    public Task<StudentClassAssignment> Assignment(int id,CancellationToken ct)=>Db.StudentClassAssignments.AsNoTracking().SingleAsync(x=>x.ClassAssignmentId==id,ct);
    public Task<System.Text.Json.Nodes.JsonObject> ScheduleDetail(int id,DateTime now,CancellationToken ct)=>SqlClassReads.ScheduleDetail(Db,id,now,ct);
    public Task<PlacementSource> PlacementCandidate(int id,CancellationToken ct)=>Candidate(Db,id,false,ct);
    public Task<System.Text.Json.Nodes.JsonObject> TeacherPreview(ClassState s,int proposed,DateTime now,CancellationToken ct)=>SqlClassPreviewQueries.Teacher(Db,s,proposed,now,ct);
    public Task<System.Text.Json.Nodes.JsonObject> TransitionPreview(ClassState s,string action,DateTime now,CancellationToken ct)=>SqlClassPreviewQueries.Transition(Db,s,action,now,ct);
    public Task<System.Text.Json.Nodes.JsonObject> PlacementPreview(ClassState s,PlacementSource source,DateTime now,CancellationToken ct)=>SqlClassPreviewQueries.Placement(Db,s,source,now,ct);
    public Task<System.Text.Json.Nodes.JsonObject> SchedulePreview(ClassState s,ScheduleCandidate candidate,DateTime now,CancellationToken ct)=>SqlClassPreviewQueries.Schedule(Db,s,candidate,now,ct);
    public Task<bool> Matches(IReadOnlyDictionary<string,int> target,CancellationToken ct)=>Matches(Db,target,ct);
    public static async Task<bool> Matches(EnglishCenterDbContext db,IReadOnlyDictionary<string,int> target,CancellationToken ct)
    {if(!await db.Classes.AnyAsync(x=>x.ClassId==target["classId"],ct))return false;if(target.TryGetValue("scheduleId",out var sid)&&!await db.Schedules.AnyAsync(x=>x.ScheduleId==sid&&x.ClassId==target["classId"],ct))return false;if(target.TryGetValue("classAssignmentId",out var aid)&&!await db.StudentClassAssignments.AnyAsync(x=>x.ClassAssignmentId==aid&&x.ClassId==target["classId"]&&x.RegistrationId==target["registrationId"],ct))return false;return true;}
    public async Task<int[]> Registrations(int id,int? candidate,CancellationToken ct)=>await Db.StudentClassAssignments.Where(x=>x.ClassId==id).Select(x=>x.RegistrationId).Concat(candidate is null?Db.Registrations.Where(x=>false).Select(x=>x.RegistrationId):Db.Registrations.Where(x=>x.RegistrationId==candidate).Select(x=>x.RegistrationId)).Distinct().Order().ToArrayAsync(ct);
    public async Task<int[]> Accounts(int[] registrations,int teacher,int? proposed,CancellationToken ct){var students=await Db.StudentProfiles.Where(x=>Db.Registrations.Where(r=>registrations.Contains(r.RegistrationId)).Select(r=>r.StudentId).Contains(x.StudentId)).Select(x=>x.AccountId).ToArrayAsync(ct);return students.Concat(new[]{teacher,proposed??teacher}).Distinct().ToArray();}
    public async Task<ClassState> LockState(int id,int[] registrations,int? candidate,CancellationToken ct)
    {
        var reference=await Db.Classes.AsNoTracking().SingleOrDefaultAsync(x=>x.ClassId==id,ct)??throw new ApiErrorException(ApiErrorCode.NOT_FOUND);
        var program=await new SqlProgramRepository(scope).StateAsync(reference.ProgramId,ct);
        foreach(var r in registrations)await Db.Registrations.FromSqlInterpolated($"SELECT * FROM dbo.Registration WITH(UPDLOCK,HOLDLOCK) WHERE registration_id={r}").SingleAsync(ct);
        if(candidate is { } registration)await Db.BandAssignments.FromSqlInterpolated($"SELECT * FROM dbo.BandAssignment WITH(UPDLOCK,HOLDLOCK) WHERE registration_id={registration}").SingleAsync(ct);
        var source=await Db.Classes.FromSqlInterpolated($"SELECT * FROM dbo.Class WITH(UPDLOCK,HOLDLOCK) WHERE class_id={id}").SingleAsync(ct);
        var assignments=await Db.StudentClassAssignments.FromSqlInterpolated($"SELECT * FROM dbo.StudentClassAssignment WITH(UPDLOCK,HOLDLOCK) WHERE class_id={id}").OrderBy(x=>x.ClassAssignmentId).ToArrayAsync(ct);
        if(assignments.Any(x=>!registrations.Contains(x.RegistrationId))||source.ProgramId!=reference.ProgramId||source.TeacherId!=reference.TeacherId)throw AcademicGuard.Conflict();
        if(candidate is { } placement)await Db.StudentClassAssignments.FromSqlInterpolated($"SELECT * FROM dbo.StudentClassAssignment WITH(UPDLOCK,HOLDLOCK) WHERE registration_id={placement}").ToArrayAsync(ct);
        var schedules=await Db.Schedules.FromSqlInterpolated($"SELECT * FROM dbo.Schedule WITH(UPDLOCK,HOLDLOCK) WHERE class_id={id}").OrderBy(x=>x.ScheduleId).ToArrayAsync(ct);
        if(candidate is { } invoice)await Db.Invoices.FromSqlInterpolated($"SELECT * FROM dbo.Invoice WITH(UPDLOCK,HOLDLOCK) WHERE registration_id={invoice}").SingleAsync(ct);
        return await Finish(Db,source,program,assignments,schedules,ct);
    }
    public static async Task<ClassState> ReadState(EnglishCenterDbContext db,int id,CancellationToken ct)
    {var states=await SqlClassStateReads.Read(db,[id],ct);return states.TryGetValue(id,out var state)?state:throw new ApiErrorException(ApiErrorCode.NOT_FOUND);}
    private sealed class ProgressRow{public int ExpectedLessonCount{get;set;} public long CurrentLessonCount{get;set;} public long HeldLessonCount{get;set;} public int CurrentFinalExamCount{get;set;} public int HeldFinalExamCount{get;set;}}
    private static async Task<ClassState> Finish(EnglishCenterDbContext db,Class c,ProgramState p,StudentClassAssignment[] a,Schedule[] schedules,CancellationToken ct)
    {
        var teacher=await db.Accounts.AsNoTracking().SingleAsync(x=>x.AccountId==c.TeacherId,ct);var role=await db.Roles.AnyAsync(x=>x.RoleId==teacher.RoleId&&x.RoleCode=="Teacher",ct);var ids=a.Select(x=>x.ClassAssignmentId).ToArray();var currentLessons=schedules.Where(x=>x.IsCurrent&&x.SessionType=="LESSON").Select(x=>x.ScheduleId).ToArray();
        var count=await db.Attendances.CountAsync(x=>ids.Contains(x.ClassAssignmentId)&&currentLessons.Contains(x.ScheduleId),ct);var hasAnyAttendance=await db.Attendances.AnyAsync(x=>ids.Contains(x.ClassAssignmentId),ct);var scores=await db.Scores.AnyAsync(x=>ids.Contains(x.ClassAssignmentId),ct);
        var progress=await db.Database.SqlQuery<ProgressRow>($"SELECT expectedLessonCount AS ExpectedLessonCount,currentLessonCount AS CurrentLessonCount,heldLessonCount AS HeldLessonCount,currentFinalExamCount AS CurrentFinalExamCount,heldFinalExamCount AS HeldFinalExamCount FROM dbo.ufn_ClassObligationProgress({c.ClassId})").SingleAsync(ct);
        if(progress.ExpectedLessonCount!=p.Program.ExpectedLessonCount||progress.CurrentLessonCount!=currentLessons.Length||progress.CurrentFinalExamCount!=schedules.Count(x=>x.IsCurrent&&x.SessionType=="FINAL_EXAM"))throw new ApiErrorException(ApiErrorCode.INTERNAL_ERROR);
        return new(c,p,teacher,role,a,schedules,count,scores,hasAnyAttendance);
    }
    public static async Task<PlacementSource> Candidate(EnglishCenterDbContext db,int id,bool requireEligible,CancellationToken ct)
    {
        var r=await db.Registrations.AsNoTracking().SingleOrDefaultAsync(x=>x.RegistrationId==id,ct)??throw new ApiErrorException(ApiErrorCode.NOT_FOUND);var b=await db.BandAssignments.AsNoTracking().SingleOrDefaultAsync(x=>x.RegistrationId==id,ct);var i=await db.Invoices.AsNoTracking().SingleOrDefaultAsync(x=>x.RegistrationId==id,ct);
        if(b is null||i is null||requireEligible&&(r.Status!="ADMITTED"||i.Status!="PAID"||await db.StudentClassAssignments.AnyAsync(x=>x.RegistrationId==id,ct)))throw new ApiErrorException(ApiErrorCode.NOT_FOUND);
        var profile=await db.StudentProfiles.AsNoTracking().SingleAsync(x=>x.StudentId==r.StudentId,ct);var account=await db.Accounts.AsNoTracking().SingleAsync(x=>x.AccountId==profile.AccountId,ct);
        var best=await db.EntryTestAttempts.AsNoTracking().SingleAsync(x=>x.AttemptId==b.BestAttemptId,ct);if(best.RegistrationId!=r.RegistrationId||best.Score is null)throw new ApiErrorException(ApiErrorCode.INTERNAL_ERROR);
        return new(r,b,i,account,await db.Courses.AsNoTracking().SingleAsync(x=>x.CourseId==r.CourseId,ct),await db.Bands.AsNoTracking().SingleAsync(x=>x.BandId==b.BandId,ct),best);
    }
    public void Add(Class c)=>Db.Classes.Add(c);public void Add(Schedule s)=>Db.Schedules.Add(s);public void Add(StudentClassAssignment a)=>Db.StudentClassAssignments.Add(a);public void Add(GradeReport r)=>Db.GradeReports.Add(r);
}
