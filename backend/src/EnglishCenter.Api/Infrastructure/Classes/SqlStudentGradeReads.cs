namespace EnglishCenter.Api.Infrastructure.Classes;
using System.Text.Json.Nodes;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Common.Security;
using EnglishCenter.Api.Common.Serialization;
using EnglishCenter.Api.Domain.Training;
using EnglishCenter.Api.Infrastructure.Enrollment;
using EnglishCenter.Api.Services.Academics;
using EnglishCenter.Api.Services.Classes;
using EnglishCenter.Api.Services.Enrollment;
using Microsoft.EntityFrameworkCore;

public sealed partial class SqlClassExecutor
{
    private async Task StudentGradeScope(VerifiedSessionToken t,int? report,int? classId,JsonObject[]? reports,CancellationToken ct)
    {
        await tx.Write(async(s,c)=>{
            var db=s.Context;
            // Discover only resource IDs; authority is checked after sorted gates, before returning any data.
            var gates=new List<string>();
            if(classId is {} selected)gates.Add("EC:CLASS:"+selected);
            if(report is {} requested){var target=await db.GradeReports.AsNoTracking().Where(x=>x.ReportId==requested).Select(x=>new{x.ClassAssignment!.ClassId,x.ClassAssignment.RegistrationId}).SingleOrDefaultAsync(c);if(target is not null){gates.Add("EC:CLASS:"+target.ClassId);gates.Add("EC:REG:"+target.RegistrationId);}}
            if(reports is not null)foreach(var row in reports){gates.Add("EC:CLASS:"+row["class"]!["id"]!.GetValue<int>());gates.Add("EC:REG:"+row["class"]!["assignment"]!["registrationId"]!.GetValue<int>());}
            var locks=new EnglishCenter.Api.Infrastructure.Replay.SqlReplayRepository(s,lockMs);
            foreach(var gate in gates.Distinct().Order(StringComparer.Ordinal))await locks.AcquireAsync(gate,c);
            var live=await EnrollmentAuthority.RequireAsync(new SqlEnrollmentRepository(s),t,clock,"Student",c);
            if(classId is {} cid&&!await db.StudentClassAssignments.AnyAsync(x=>x.ClassId==cid&&x.Registration!.Student!.AccountId==t.AccountId,c))throw new ApiErrorException(ApiErrorCode.NOT_FOUND);
            var rows=SqlStudentGradeReads.Own(db,t.AccountId);
            if(report is {} id&&!await rows.AnyAsync(x=>x.ReportId==id,c))throw new ApiErrorException(ApiErrorCode.NOT_FOUND);
            if(reports is not null){var ids=reports.Select(x=>x["id"]!.GetValue<int>()).ToArray();var current=await rows.Where(x=>ids.Contains(x.ReportId)).Select(x=>new{x.ReportId,x.ClassAssignmentId,x.ClassAssignment!.RegistrationId,x.ClassAssignment.ClassId}).ToDictionaryAsync(x=>x.ReportId,c);foreach(var row in reports){var rid=row["id"]!.GetValue<int>();if(!current.TryGetValue(rid,out var binding)||binding.ClassAssignmentId!=row["class"]!["assignment"]!["classAssignmentId"]!.GetValue<int>()||binding.RegistrationId!=row["class"]!["assignment"]!["registrationId"]!.GetValue<int>()||binding.ClassId!=row["class"]!["id"]!.GetValue<int>())throw new ApiErrorException(ApiErrorCode.NOT_FOUND);}}
            if(!clock.IsBefore(live.EffectiveDeadline))throw new ApiErrorException(ApiErrorCode.AUTHENTICATION_REQUIRED);
            return true;
        },ct);
    }
    public async Task<JsonObject> StudentGrades(VerifiedSessionToken t,IQueryCollection query,CancellationToken ct)
    {
        var q=new AcademicQuery(query,"courseId","classId","fromDate","toDate","sort","page","pageSize");
        var course=q.Id("courseId");var id=q.Id("classId");var from=q.Date("fromDate");var to=q.Date("toDate");if(from>to)throw ApiErrorException.Validation("toDate");
        var sort=q.Choice("sort","FINALIZED_DESC","FINALIZED_ASC","FINALIZED_DESC");var page=q.Number("page");var size=q.Number("pageSize",20,100);
        await StudentGradeScope(t,null,id,null,ct);
        var result=await tx.Read(t,"Student",async(db,c)=>{
            var rows=SqlStudentGradeReads.Own(db,t.AccountId);if(course is {} co)rows=rows.Where(x=>x.ClassAssignment!.Registration!.CourseId==co);if(id is {} cid)rows=rows.Where(x=>x.ClassAssignment!.ClassId==cid);
            if(from is {} start&&start!=DateOnly.MinValue){var utc=start.ToDateTime(TimeOnly.MinValue).AddHours(-7);rows=rows.Where(x=>x.FinalizedAt>=utc);}
            if(to is {} end&&end!=DateOnly.MaxValue){var utc=end.AddDays(1).ToDateTime(TimeOnly.MinValue).AddHours(-7);rows=rows.Where(x=>x.FinalizedAt<utc);}
            var count=await rows.LongCountAsync(c);var ordered=sort=="FINALIZED_ASC"?rows.OrderBy(x=>x.FinalizedAt).ThenBy(x=>x.ReportId):rows.OrderByDescending(x=>x.FinalizedAt).ThenByDescending(x=>x.ReportId);
            var offset=(long)(page-1)*size;var ids=offset>int.MaxValue?[]:await ordered.Skip((int)offset).Take(size).Select(x=>x.ReportId).ToArrayAsync(c);
            return SqlClassReads.Page(await SqlStudentGradeReads.Read(db,t.AccountId,ids,false,c),page,size,count);
        },ct);
        await StudentGradeScope(t,null,id,result["items"]!.AsArray().Select(x=>x!.AsObject()).ToArray(),ct);return result;
    }
    public async Task<JsonObject> StudentGrade(VerifiedSessionToken t,int id,CancellationToken ct)
    {
        await StudentGradeScope(t,id,null,null,ct);
        var result=await tx.Read(t,"Student",async(db,c)=>(await SqlStudentGradeReads.Read(db,t.AccountId,[id],true,c)).Single(),ct);
        await StudentGradeScope(t,id,null,[result],ct);return result;
    }
}
internal static class SqlStudentGradeReads
{
    internal static IQueryable<GradeReport> Own(EnglishCenterDbContext db,int account)=>db.GradeReports.AsNoTracking().Where(x=>x.Status=="FINALIZED"&&x.ClassAssignment!.Class!.Status=="COMPLETED"&&x.ClassAssignment.Registration!.Student!.AccountId==account);
    internal static async Task<JsonObject[]> Read(EnglishCenterDbContext db,int account,int[] ids,bool detail,CancellationToken ct)
    {
        var reports=await Own(db,account).Where(x=>ids.Contains(x.ReportId)).ToArrayAsync(ct);if(reports.Length!=ids.Length)throw new ApiErrorException(ApiErrorCode.NOT_FOUND);
        var assignmentIds=reports.Select(x=>x.ClassAssignmentId).ToArray();
        var assignments=await db.StudentClassAssignments.AsNoTracking().Where(x=>assignmentIds.Contains(x.ClassAssignmentId)).Include(x=>x.Class!).ThenInclude(x=>x.Teacher).Include(x=>x.Class!).ThenInclude(x=>x.Program!).ThenInclude(x=>x.Course).Include(x=>x.Class!).ThenInclude(x=>x.Program!).ThenInclude(x=>x.Band).ToDictionaryAsync(x=>x.ClassAssignmentId,ct);
        var programs=assignments.Values.Select(x=>x.Class!.ProgramId).Distinct().ToArray();var classIds=assignments.Values.Select(x=>x.ClassId).Distinct().ToArray();
        var components=await db.ScoreComponentConfigs.AsNoTracking().Where(x=>programs.Contains(x.ProgramId)).OrderBy(x=>x.ComponentId).ToArrayAsync(ct);
        var scores=await db.Scores.AsNoTracking().Where(x=>assignmentIds.Contains(x.ClassAssignmentId)).ToArrayAsync(ct);
        var scheduleIds=scores.Where(x=>x.ScheduleId is not null).Select(x=>x.ScheduleId!.Value).Distinct().ToArray();
        var schedules=await db.Schedules.AsNoTracking().Where(x=>classIds.Contains(x.ClassId)&&scheduleIds.Contains(x.ScheduleId)).ToDictionaryAsync(x=>x.ScheduleId,ct);
        return ids.Select(id=>{
            var report=reports.Single(x=>x.ReportId==id);var a=assignments[report.ClassAssignmentId];var cl=a.Class!;var p=cl.Program!;var config=components.Where(x=>x.ProgramId==p.ProgramId).ToArray();var own=scores.Where(x=>x.ClassAssignmentId==a.ClassAssignmentId).ToArray();
            if(report.FinalScore is not {} total||total<0||total>100||report.FinalizedAt is not {} at||config.Length==0||config.Sum(x=>x.Weight)!=100||own.Length!=config.Length||config.Any(x=>own.Count(y=>y.ComponentId==x.ComponentId)!=1)||own.Any(x=>x.ScoreValue<0||x.ScoreValue>100))throw new ApiErrorException(ApiErrorCode.INTERNAL_ERROR);
            var program=ClassWire.Object(new{id=p.ProgramId,name=p.ProgramName,course=new{id=p.Course!.CourseId,code=p.Course.CourseCode,name=p.Course.CourseName,status=p.Course.Status},band=new{id=p.Band!.BandId,code=p.Band.BandCode,minScore=ExactNumbers.FormatScore(p.Band.MinScore)},status=p.Status,expectedLessonCount=p.ExpectedLessonCount});
            var result=ClassWire.Object(new{id=report.ReportId,@class=new{id=cl.ClassId,code=cl.ClassCode,program,teacher=new{displayName=cl.Teacher!.FullName},status=cl.Status,assignment=new{classAssignmentId=a.ClassAssignmentId,registrationId=a.RegistrationId,assignedAt=ClassWire.At(a.AssignedAt)}},finalizedAt=ClassWire.At(at),finalScore=ExactNumbers.FormatScore(total),passed=total>=50});
            var published=ClassWire.Array(config.Select(component=>{
                var score=own.Single(x=>x.ComponentId==component.ComponentId);JsonObject? schedule=null;
                if(component.Kind=="GENERAL"){if(score.ScheduleId is not null)throw new ApiErrorException(ApiErrorCode.INTERNAL_ERROR);}
                else{if(score.ScheduleId is not {} sid||!schedules.TryGetValue(sid,out var s)||s.ClassId!=cl.ClassId||!s.IsCurrent||s.OccurrenceState=="NOT_HELD"||s.SessionType!=component.Kind)throw new ApiErrorException(ApiErrorCode.INTERNAL_ERROR);schedule=ClassWire.Object(new{id=s.ScheduleId,type=s.SessionType,date=s.SessionDate.ToString("yyyy-MM-dd"),startTime=s.StartTime.ToString("HH:mm"),endTime=s.EndTime.ToString("HH:mm"),room=s.Room});}
                return ClassWire.Object(new{componentId=component.ComponentId,name=component.ComponentName,kind=component.Kind,weight=ExactNumbers.FormatWeight(component.Weight),score=ExactNumbers.FormatScore(score.ScoreValue),schedule});
            }));
            if(detail){result["components"]=published;result["teacherComment"]=report.TeacherComment;}return result;
        }).ToArray();
    }
}
