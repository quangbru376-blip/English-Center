namespace EnglishCenter.Api.Infrastructure.Classes;

using System.Text.Json;
using System.Text.Json.Nodes;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Common.Security;
using EnglishCenter.Api.Services.Academics;
using EnglishCenter.Api.Services.Classes;
using Microsoft.EntityFrameworkCore;

public sealed partial class SqlClassExecutor
{
    public Task<JsonObject> Gradebook(VerifiedSessionToken t,int id,CancellationToken ct)=>ScopedRead(t,id,"Teacher",async(db,c)=>GradeWire.Gradebook(await SqlGradeRepository.Read(db,id,c)),ct);
    public Task<JsonObject> GradeDraft(VerifiedSessionToken t,int id,int assignment,CancellationToken ct)=>ScopedRead(t,id,"Teacher",async(db,c)=>GradeWire.Detail(await SqlGradeRepository.Read(db,id,c),assignment),ct);
    public Task<JsonObject> GradeOptions(VerifiedSessionToken t,int id,int component,CancellationToken ct)=>ScopedRead(t,id,"Teacher",async(db,c)=>
    {
        var s=await SqlClassRepository.ReadState(db,id,c);var config=s.Program.Components.SingleOrDefault(x=>x.ComponentId==component)??throw new ApiErrorException(ApiErrorCode.NOT_FOUND);
        if(config.Kind=="GENERAL")throw ApiErrorException.Validation("componentId");
        return ClassWire.Object(new{classId=id,componentId=component,kind=config.Kind,items=s.Schedules.Where(x=>x.IsCurrent&&x.OccurrenceState!="NOT_HELD"&&x.SessionType==config.Kind).OrderBy(x=>x.SessionDate).ThenBy(x=>x.StartTime).ThenBy(x=>x.ScheduleId).Select(x=>{var eligible=s.Class.Status is "ONGOING" or "COMPLETED"&&Now>=ClassWire.Window(x).StartAt;return new{schedule=GradeWire.Schedule(x),eligible,blockedReasons=eligible?Array.Empty<string>():new[]{"Chưa đến giờ hoặc lớp chưa cho phép nhập điểm."}};}),context=GradeWire.Configuration(s)});
    },ct);
    public Task<JsonObject> SubmissionPreview(VerifiedSessionToken t,int id,string json,CancellationToken ct)
    {
        using var doc=JsonDocument.Parse(json);var action=Single(doc.RootElement,"action");if(action.ValueKind!=JsonValueKind.String||action.GetString() is not("SUBMIT" or "RESUBMIT"))throw ApiErrorException.Validation("action");var selected=action.GetString()!;
        return ScopedRead(t,id,"Teacher",async(db,c)=>GradeWire.SubmissionPreview(await SqlGradeRepository.Read(db,id,c),selected,Now),ct);
    }
    public Task<JsonObject> ReviewDetail(VerifiedSessionToken t,int id,CancellationToken ct)=>ScopedRead(t,id,"AcademicAffairs",async(db,c)=>GradeWire.ReviewDetail(await SqlGradeReviewReads.Read(db,id,t.AccountId,sqlSeconds,c),Now),ct);
    public Task<JsonObject> ReviewPreview(VerifiedSessionToken t,int id,string json,CancellationToken ct)
    {
        using var doc=JsonDocument.Parse(json);var action=Single(doc.RootElement,"action");if(action.ValueKind!=JsonValueKind.String||action.GetString()!="RETURN")throw ApiErrorException.Validation("action");
        return ScopedRead(t,id,"AcademicAffairs",async(db,c)=>{var g=await SqlGradeReviewReads.Read(db,id,t.AccountId,sqlSeconds,c);var eligible=GradeWire.WorkflowEligible(g,true,Now);return ClassWire.Object(new{detail=GradeWire.ReviewDetail(g,Now),action="RETURN",eligible,blockedReasons=eligible?Array.Empty<string>():new[]{"Cần cả lớp đã gửi, đủ điểm và báo cáo hợp lệ."},effects=new[]{"Trả lại cả lớp để giáo viên sửa; giữ nguyên điểm và nhận xét."},manifest=eligible?GradeWire.WorkflowManifest(g):null});},ct);
    }
    public Task<JsonObject> ReviewList(VerifiedSessionToken t,IQueryCollection query,CancellationToken ct)
    {
        var q=new AcademicQuery(query,"reviewStatus","courseId","classId","teacherId","sort","page","pageSize");var status=q.Choice("reviewStatus","SUBMITTED","SUBMITTED","RETURNED","FINALIZED");var id=q.Id("classId");var course=q.Id("courseId");var teacher=q.Id("teacherId");var sort=q.Choice("sort","CLASS_CODE_ASC","CLASS_CODE_ASC","CLASS_CODE_DESC");var page=q.Number("page");var size=q.Number("pageSize",20,100);
        return tx.Read(t,"AcademicAffairs",async(db,c)=>
        {
            var rows=db.Classes.AsNoTracking().Where(x=>x.Status=="COMPLETED"&&db.GradeReports.Any(r=>r.ClassAssignment!.ClassId==x.ClassId&&r.Status==status));if(id is {} cid)rows=rows.Where(x=>x.ClassId==cid);if(course is {} courseId)rows=rows.Where(x=>x.Program!.CourseId==courseId);if(teacher is {} teacherId)rows=rows.Where(x=>x.TeacherId==teacherId);
            var count=await rows.LongCountAsync(c);var ordered=sort=="CLASS_CODE_ASC"?rows.OrderBy(x=>x.ClassCode).ThenBy(x=>x.ClassId):rows.OrderByDescending(x=>x.ClassCode).ThenByDescending(x=>x.ClassId);var offset=(long)(page-1)*size;var ids=offset>int.MaxValue?[]:await ordered.Skip((int)offset).Take(size).Select(x=>x.ClassId).ToArrayAsync(c);
            var states=await SqlClassStateReads.Read(db,ids,c);var visible=await db.GradeReports.AsNoTracking().Where(x=>ids.Contains(x.ClassAssignment!.ClassId)&&x.Status!="DRAFT").Select(x=>new{classId=x.ClassAssignment!.ClassId,x.Status,x.SubmittedAt,x.ReviewedAt}).ToArrayAsync(c);
            var items=ids.Select(key=>{var reports=visible.Where(x=>x.classId==key).ToArray();var s=states[key];return ClassWire.Object(new{@class=ClassWire.Summary(s),cohortSize=s.Assignments.Length,visibleReportCount=reports.Length,states=new{submitted=reports.Count(x=>x.Status=="SUBMITTED"),returned=reports.Count(x=>x.Status=="RETURNED"),finalized=reports.Count(x=>x.Status=="FINALIZED")},hasCompleteVisibleCohort=reports.Length==s.Assignments.Length,latestSubmittedAt=reports.Max(x=>x.SubmittedAt) is {} a?ClassWire.At(a):null,latestReviewedAt=reports.Max(x=>x.ReviewedAt) is {} b?ClassWire.At(b):null});});return SqlClassReads.Page(items,page,size,count);
        },ct);
    }
    public async Task<ClassCommandOutcome> GradeCommand(VerifiedSessionToken t,int api,int id,int assignment,Guid request,string json,int bytes,CancellationToken ct)
    {
        var role=api==146?"AcademicAffairs":"Teacher";
        await ScopedRead(t,id,role,async(db,c)=>{if(api==139&&!await db.StudentClassAssignments.AnyAsync(x=>x.ClassId==id&&x.ClassAssignmentId==assignment,c))throw new ApiErrorException(ApiErrorCode.NOT_FOUND);return true;},ct);
        var input=new GradeInput(api,id,assignment,request,json,bytes);
        return await tx.Write((s,c)=>{var classes=new SqlClassRepository(s);return new GradeService(classes,new(classes),new Infrastructure.Replay.SqlReplayRepository(s,lockMs),clock).Command(t,input,c);},ct);
    }
}
