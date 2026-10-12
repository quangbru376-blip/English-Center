namespace EnglishCenter.Api.Infrastructure.Classes;

using System.Text.Json;
using System.Text.Json.Nodes;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Common.Security;
using EnglishCenter.Api.Infrastructure.Replay;
using EnglishCenter.Api.Services.Classes;
using Microsoft.EntityFrameworkCore;

public sealed partial class SqlClassExecutor
{
    public Task<JsonObject> WeeklyPreview(VerifiedSessionToken token,int id,string json,CancellationToken ct)=>tx.Read(token,"AcademicAffairs",async(db,c)=>
    {
        using var doc=JsonDocument.Parse(json);
        var rule=WeeklyRule.Parse(Single(doc.RootElement,"rule"));
        await SqlSchedulePreviews.CheckStateSize(db,id,weeklyBudget,c);
        return await SqlSchedulePreviews.Weekly(db,await SqlClassRepository.ReadState(db,id,c),rule,Now,weeklyBudget,c);
    },ct);
    public Task<JsonObject> EditPreview(VerifiedSessionToken token,int id,string json,bool replacement,CancellationToken ct)=>tx.Read(token,"AcademicAffairs",async(db,c)=>
    {
        var old=await db.Schedules.AsNoTracking().SingleOrDefaultAsync(x=>x.ScheduleId==id,c)??throw new ApiErrorException(ApiErrorCode.NOT_FOUND);
        using var doc=JsonDocument.Parse(json);var candidate=ScheduleRules.Destination(doc.RootElement,old.SessionType,replacement);
        var reason=replacement?ScheduleRules.Reason(doc.RootElement):null;
        return await SqlSchedulePreviews.Edit(db,await SqlClassRepository.ReadState(db,old.ClassId,c),old,candidate,reason,Now,c);
    },ct);
    public async Task<ClassCommandOutcome> ScheduleCommand(VerifiedSessionToken token,int api,int id,Guid request,string json,int bytes,CancellationToken ct)
    {
        await tx.Read(token,"AcademicAffairs",async(db,c)=>
        {
            if(api==120)await Scope(db,id,"AcademicAffairs",token.AccountId,c);
            else if(!await db.Schedules.AnyAsync(x=>x.ScheduleId==id,c))throw new ApiErrorException(ApiErrorCode.NOT_FOUND);
            return true;
        },ct);
        var input=new ScheduleInput(api,id,request,json,bytes,weeklyBudget);
        return await tx.Write((s,c)=>new ScheduleService(new(s),new SqlReplayRepository(s,lockMs),clock,weeklyBudget).Command(token,input,c),ct);
    }
    public async Task<JsonObject> OwnScheduleDetail(VerifiedSessionToken token,int id,string role,CancellationToken ct)
    {
        var classId=await tx.Read(token,role,async(db,c)=>await db.Schedules.Where(x=>x.ScheduleId==id).Select(x=>x.ClassId).SingleOrDefaultAsync(c),ct);
        if(classId==0)throw new ApiErrorException(ApiErrorCode.NOT_FOUND);
        return await ScopedRead(token,classId,role,async(db,c)=>
        {
            var detail=await SqlClassReads.ScheduleDetail(db,id,Now,c);
            foreach(var key in new[]{"version","held","notHeld","dependencies","allowedActions","disabledReasons"})detail.Remove(key);
            OwnShape(detail,role);return detail;
        },ct);
    }
    private static void OwnShape(JsonObject row,string role)
    {
        row.Remove("version");
        if(role=="Student")row["class"]!["teacher"]=new JsonObject{["displayName"]=row["class"]!["teacher"]!["displayName"]!.DeepClone()};
    }
    public async Task<JsonObject> OwnCalendar(VerifiedSessionToken token,IQueryCollection query,string role,CancellationToken ct)
    {
        var q=new Services.Academics.AcademicQuery(query,"fromDate","toDate","classId","view","sort","page","pageSize");
        var from=q.Date("fromDate");var to=q.Date("toDate");if(from>to)throw ApiErrorException.Validation("toDate");
        if(from is null&&to is null){var today=clock.GetToday();from=today.AddDays(-((int)today.DayOfWeek+6)%7);to=from.Value.AddDays(6);}
        var classId=q.Id("classId");var view=q.Choice("view","current","current","history");var sort=q.Choice("sort","START_ASC","START_ASC","START_DESC");var page=q.Number("page");var size=q.Number("pageSize",20,100);
        var result=await tx.Read(token,role,async(db,c)=>
        {
            if(classId is {} selected)await Scope(db,selected,role,token.AccountId,c);
            var classes=db.Classes.Where(x=>(role=="Teacher"&&x.TeacherId==token.AccountId)||(role=="Student"&&db.StudentClassAssignments.Any(a=>a.ClassId==x.ClassId&&a.Registration!.Student!.AccountId==token.AccountId))).Select(x=>x.ClassId);
            var rows=db.Schedules.AsNoTracking().Where(x=>classes.Contains(x.ClassId));
            if(classId is {} selectedId)rows=rows.Where(x=>x.ClassId==selectedId);
            if(from is {} first)rows=rows.Where(x=>x.SessionDate>=first);if(to is {} last)rows=rows.Where(x=>x.SessionDate<=last);
            if(view=="current"){var current=db.Database.SqlQuery<int>($"SELECT scheduleId AS Value FROM dbo.vw_CurrentScheduleOccurrences");rows=rows.Where(x=>current.Contains(x.ScheduleId)&&(x.Class!.Status=="PLANNED"||x.Class.Status=="OPEN"||x.Class.Status=="ONGOING"));}
            var count=await rows.LongCountAsync(c);var sorted=sort=="START_ASC"?rows.OrderBy(x=>x.SessionDate).ThenBy(x=>x.StartTime).ThenBy(x=>x.ScheduleId):rows.OrderByDescending(x=>x.SessionDate).ThenByDescending(x=>x.StartTime).ThenByDescending(x=>x.ScheduleId);
            var offset=(long)(page-1)*size;var data=offset>int.MaxValue?[]:await sorted.Skip((int)offset).Take(size).ToArrayAsync(c);
            var states=await SqlClassStateReads.Read(db,data.Select(x=>x.ClassId).Distinct().ToArray(),c);
            var items=data.Select(x=>{var row=SqlClassReads.ScheduleView(states[x.ClassId],x);OwnShape(row,role);return row;}).ToArray();
            var output=SqlClassReads.Page(items,page,size,count);output["dateWindow"]=ClassWire.Object(new{fromDate=from?.ToString("yyyy-MM-dd"),toDate=to?.ToString("yyyy-MM-dd")});return output;
        },ct);
        // Recheck current assignment under live account/session locks after the SNAPSHOT projection.
        await tx.Write(async(s,c)=>
        {
            var live=await Services.Enrollment.EnrollmentAuthority.RequireAsync(new Enrollment.SqlEnrollmentRepository(s),token,clock,role,c);
            var ids=result["items"]!.AsArray().Select(x=>x!["class"]!["id"]!.GetValue<int>()).Concat(classId is {} explicitId?[explicitId]:Array.Empty<int>()).ToArray();
            await ScopeMany(s.Context,ids,role,token.AccountId,c);
            if(!clock.IsBefore(live.EffectiveDeadline))throw new ApiErrorException(ApiErrorCode.AUTHENTICATION_REQUIRED);
            return true;
        },ct);
        return result;
    }
}
