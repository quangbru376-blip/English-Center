namespace EnglishCenter.Api.Infrastructure.Classes;

using System.Text.Json.Nodes;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Domain.Training;
using EnglishCenter.Api.Services.Academics;
using EnglishCenter.Api.Services.Classes;
using Microsoft.EntityFrameworkCore;

public static class SqlAttendanceReads
{
    public static Schedule Lesson(ClassState s,int id)=>s.Schedules.SingleOrDefault(x=>x.ScheduleId==id&&x.SessionType=="LESSON")??throw new ApiErrorException(ApiErrorCode.NOT_FOUND);
    public static async Task<JsonObject> Sheet(EnglishCenterDbContext db,ClassState state,int id,bool teacher,DateTime now,CancellationToken ct)
    {
        var lesson=Lesson(state,id);
        var contacts=await (from a in db.StudentClassAssignments.AsNoTracking() join r in db.Registrations on a.RegistrationId equals r.RegistrationId
            join profile in db.StudentProfiles on r.StudentId equals profile.StudentId join account in db.Accounts on profile.AccountId equals account.AccountId
            where a.ClassId==state.Class.ClassId orderby a.ClassAssignmentId select new{a.ClassAssignmentId,r.StudentId,account.FullName,account.Email,account.Phone}).ToArrayAsync(ct);
        var rows=contacts.Select(x=>{var attendance=(state.AttendanceRows??[]).SingleOrDefault(a=>a.ScheduleId==id&&a.ClassAssignmentId==x.ClassAssignmentId);
            return ClassWire.Object(new{classAssignmentId=x.ClassAssignmentId,student=new{studentId=x.StudentId,fullName=x.FullName,email=x.Email,phone=x.Phone},attendance=attendance is null?null:AttendanceWire.Record(attendance,teacher)});});
        var result=ClassWire.Object(new{@class=AttendanceWire.ClassSummary(state,teacher),lesson=AttendanceWire.Lesson(state,lesson),rows,
            counts=AttendanceWire.Counts(state,lesson),countsTowardCompletion=AttendanceWire.CountsToward(state,lesson)});
        if(teacher){result["editing"]=AttendanceWire.Editing(state,lesson,now);result["writeContext"]=AttendanceWire.CanEdit(state,lesson,now)?AttendanceWire.Context(state,lesson):null;}
        return result;
    }
    public static JsonObject Summary(ClassState state,IQueryCollection query,bool teacher,DateTime now)
    {
        var q=new AcademicQuery(query,"view","sort","page","pageSize");var view=q.Choice("view","current","current","history");var sort=q.Choice("sort","START_ASC","START_ASC","START_DESC");
        var page=q.Number("page");var size=q.Number("pageSize",20,100);
        var all=state.Schedules.Where(x=>x.SessionType=="LESSON"&&(view=="history"||x.IsCurrent));
        var ordered=sort=="START_ASC"?all.OrderBy(x=>x.SessionDate).ThenBy(x=>x.StartTime).ThenBy(x=>x.ScheduleId):all.OrderByDescending(x=>x.SessionDate).ThenByDescending(x=>x.StartTime).ThenByDescending(x=>x.ScheduleId);
        var offset=(long)(page-1)*size;
        var items=(offset>int.MaxValue?Array.Empty<Schedule>():ordered.Skip((int)offset).Take(size)).Select(x=>{
            var item=ClassWire.Object(new{lesson=AttendanceWire.Lesson(state,x),counts=AttendanceWire.Counts(state,x),countsTowardCompletion=AttendanceWire.CountsToward(state,x)});
            if(teacher)item["editing"]=AttendanceWire.Editing(state,x,now);return item;});
        return ClassWire.Object(new{@class=AttendanceWire.ClassSummary(state,teacher),items,page,pageSize=size,totalCount=all.LongCount(),completionCoverage=AttendanceWire.Coverage(state)});
    }
}
