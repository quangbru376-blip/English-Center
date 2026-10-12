namespace EnglishCenter.Api.Services.Classes;

using System.Text.Json;
using System.Text.Json.Nodes;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Common.Replay;
using EnglishCenter.Api.Common.Serialization;
using EnglishCenter.Api.Domain.Training;
using EnglishCenter.Api.Services.Academics;

public sealed record GradeState(ClassState Class, GradeReport[] Reports, Score[] Scores, IReadOnlyDictionary<int,JsonObject> Assignments, IReadOnlyDictionary<int,JsonObject> Computations);

public sealed class GradeInput
{
    public int Api {get;} public int ClassId {get;} public int AssignmentId {get;} public Guid Request {get;} public CanonicalIntent Intent {get;} public JsonElement Body {get;}
    public GradeInput(int api,int classId,int assignmentId,Guid request,string json,int bytes)
    {
        if(api is not(139 or 141 or 142 or 146)||request==Guid.Empty)throw ApiErrorException.Validation();
        Api=api;ClassId=classId;AssignmentId=assignmentId;Request=request;
        Intent=CanonicalIntent.Bind(api,json,Route(),bytes);
        using var doc=JsonDocument.Parse(Intent.CopyBytes());Body=doc.RootElement.GetProperty("body").Clone();
        if(api==139&&Body.GetProperty("scores").GetArrayLength()==0&&!Body.TryGetProperty("teacherComment",out _))throw ApiErrorException.Validation("scores");
    }
    private Dictionary<string,int> Route()=>Api==139?new(){{"classId",ClassId},{"classAssignmentId",AssignmentId}}:new(){{"classId",ClassId}};
    public void RequireManifest(JsonObject manifest)
    {
        var b=JsonNode.Parse(Body.GetRawText())!.AsObject();
        b["expectedVersions"]=manifest["expectedVersions"]!.DeepClone();b["expectedSet"]=manifest["expectedSet"]!.DeepClone();
        CanonicalIntent fresh;
        try{fresh=CanonicalIntent.Bind(Api,b.ToJsonString(),Route(),int.MaxValue);}
        catch(ApiErrorException){throw AcademicGuard.Conflict();}
        if(!fresh.CopyBytes().AsSpan().SequenceEqual(Intent.CopyBytes()))throw AcademicGuard.Conflict();
    }
}

public static class GradeWire
{
    public static bool Editable(ClassState s,GradeReport r)=>s.Class.Status is "ONGOING" or "COMPLETED"&&r.Status is "DRAFT" or "RETURNED";
    public static bool ScheduleValid(ClassState s,Score score,DateTime now)
    {
        var component=s.Program.Components.SingleOrDefault(x=>x.ComponentId==score.ComponentId);
        if(component is null)return false;
        if(component.Kind=="GENERAL")return score.ScheduleId is null;
        var schedule=s.Schedules.SingleOrDefault(x=>x.ScheduleId==score.ScheduleId);
        return schedule is not null&&schedule.ClassId==s.Class.ClassId&&schedule.IsCurrent&&schedule.OccurrenceState!="NOT_HELD"&&schedule.SessionType==component.Kind&&now>=ClassWire.Window(schedule).StartAt;
    }
    public static JsonObject Report(GradeReport r)=>ClassWire.Object(new{id=r.ReportId,status=r.Status,teacherComment=r.TeacherComment,submittedAt=r.SubmittedAt is {} a?ClassWire.At(a):null,reviewedAt=r.ReviewedAt is {} b?ClassWire.At(b):null,finalizedAt=r.FinalizedAt is {} f?ClassWire.At(f):null,latestReturnReason=r.ReturnedReason,finalScore=r.FinalScore is {} total?ExactNumbers.FormatScore(total):null,version=ClassWire.Rv(r.RowVersion)});
    public static JsonObject Schedule(Schedule s)=>ClassWire.Object(new{id=s.ScheduleId,classId=s.ClassId,type=s.SessionType,obligationKey=s.ObligationKey,date=s.SessionDate.ToString("yyyy-MM-dd"),startTime=s.StartTime.ToString("HH:mm"),endTime=s.EndTime.ToString("HH:mm"),startAt=ClassWire.At(ClassWire.Window(s).StartAt),endAt=ClassWire.At(ClassWire.Window(s).EndAt),room=s.Room,occurrenceState=s.OccurrenceState,isCurrent=s.IsCurrent,replacesScheduleId=s.ReplacesScheduleId,version=ClassWire.Rv(s.RowVersion)});
    public static JsonObject Score(GradeState g,Score s)=>ClassWire.Object(new{id=s.ScoreId,componentId=s.ComponentId,score=ExactNumbers.FormatScore(s.ScoreValue),schedule=s.ScheduleId is {} id?Schedule(g.Class.Schedules.Single(x=>x.ScheduleId==id)):null,enteredByAccountId=s.EnteredBy,enteredAt=ClassWire.At(s.EnteredAt),version=ClassWire.Rv(s.RowVersion)});
    public static JsonArray Components(ClassState s)=>ClassWire.Array(s.Program.Components.OrderBy(x=>x.ComponentId).Select(x=>ClassWire.Object(new{id=x.ComponentId,name=x.ComponentName,kind=x.Kind,weight=ExactNumbers.FormatWeight(x.Weight),version=ClassWire.Rv(x.RowVersion)})));
    public static JsonObject Configuration(ClassState s)=>ClassWire.Object(new{expectedVersions=new{@class=ClassWire.Rv(s.Class.RowVersion),program=ClassWire.Rv(s.Program.Program.RowVersion),teacher=ClassWire.Rv(s.Teacher.RowVersion),components=ProgramProjection.References(s.Program)},expectedSet=new{componentIds=s.Program.Components.Select(x=>x.ComponentId)}});
    public static JsonObject DraftContext(GradeState g,int assignment)
    {
        var m=Configuration(g.Class);var report=g.Reports.Single(x=>x.ClassAssignmentId==assignment);
        m["expectedVersions"]!["report"]=ClassWire.Rv(report.RowVersion);m["expectedSet"]!["classAssignmentId"]=assignment;m["expectedSet"]!["reportId"]=report.ReportId;return m;
    }
    public static JsonObject DraftManifest(GradeState g,int assignment,int[] changed,int[] selected)
    {
        var m=DraftContext(g,assignment);var scores=g.Scores.Where(x=>x.ClassAssignmentId==assignment).ToArray();var delta=scores.Where(x=>changed.Contains(x.ComponentId)).ToArray();
        var schedules=g.Class.Schedules.Where(x=>scores.Any(v=>v.ScheduleId==x.ScheduleId)||selected.Contains(x.ScheduleId)).ToArray();
        m["expectedVersions"]!["scores"]=JsonSerializer.SerializeToNode(delta.Select(x=>new{id=x.ScoreId,version=ClassWire.Rv(x.RowVersion)}));
        m["expectedVersions"]!["schedules"]=JsonSerializer.SerializeToNode(schedules.Select(x=>new{id=x.ScheduleId,version=ClassWire.Rv(x.RowVersion)}));
        m["expectedSet"]!["scoreBindings"]=JsonSerializer.SerializeToNode(delta.Select(x=>new{componentId=x.ComponentId,scoreId=x.ScoreId,scheduleId=x.ScheduleId}));
        m["expectedSet"]!["unrecordedComponentIds"]=JsonSerializer.SerializeToNode(changed.Where(x=>!delta.Any(v=>v.ComponentId==x)));
        m["expectedSet"]!["scheduleBindings"]=ClassWire.Array(schedules.Select(x=>ClassWire.Binding(x)));return m;
    }
    public static JsonObject WorkflowManifest(GradeState g)
    {
        var m=Configuration(g.Class);var schedules=g.Class.Schedules.Where(x=>g.Scores.Any(v=>v.ScheduleId==x.ScheduleId)).ToArray();
        m["expectedVersions"]!["reports"]=JsonSerializer.SerializeToNode(g.Reports.Select(x=>new{id=x.ReportId,version=ClassWire.Rv(x.RowVersion)}));
        m["expectedVersions"]!["scores"]=JsonSerializer.SerializeToNode(g.Scores.Select(x=>new{id=x.ScoreId,version=ClassWire.Rv(x.RowVersion)}));
        m["expectedVersions"]!["schedules"]=JsonSerializer.SerializeToNode(schedules.Select(x=>new{id=x.ScheduleId,version=ClassWire.Rv(x.RowVersion)}));
        m["expectedSet"]!["assignmentIds"]=JsonSerializer.SerializeToNode(g.Class.Assignments.Select(x=>x.ClassAssignmentId));
        m["expectedSet"]!["reportBindings"]=JsonSerializer.SerializeToNode(g.Reports.Select(x=>new{classAssignmentId=x.ClassAssignmentId,reportId=x.ReportId,status=x.Status}));
        m["expectedSet"]!["scoreBindings"]=JsonSerializer.SerializeToNode(g.Scores.Select(x=>new{classAssignmentId=x.ClassAssignmentId,componentId=x.ComponentId,scoreId=x.ScoreId,scheduleId=x.ScheduleId}));
        m["expectedSet"]!["scheduleBindings"]=ClassWire.Array(schedules.Select(x=>ClassWire.Binding(x)));return m;
    }
    public static bool WorkflowEligible(GradeState g,bool review,DateTime now)=>g.Class.Class.Status=="COMPLETED"&&g.Class.Assignments.Length>0&&g.Reports.Length==g.Class.Assignments.Length&&ProgramProjection.Validate(g.Class.Program).MeetsActivationRules&&g.Reports.All(x=>review?x.Status=="SUBMITTED":x.Status is "DRAFT" or "RETURNED")&&g.Computations.Values.All(x=>x["isComplete"]!.GetValue<bool>())&&g.Scores.All(x=>ScheduleValid(g.Class,x,now));
    public static JsonObject Student(GradeState g,GradeReport r,bool teacher)
    {
        var row=ClassWire.Object(new{assignment=g.Assignments[r.ClassAssignmentId],report=Report(r),scores=g.Scores.Where(x=>x.ClassAssignmentId==r.ClassAssignmentId).OrderBy(x=>x.ComponentId).Select(x=>Score(g,x)),computation=g.Computations[r.ClassAssignmentId]});
        if(teacher){var edit=Editable(g.Class,r);row["editing"]=ClassWire.Object(new{canEdit=edit,blockedReasons=edit?Array.Empty<string>():new[]{"Chỉ sửa điểm nháp hoặc điểm trả lại khi lớp đang dạy/đã hoàn thành."}});row["writeContext"]=edit?DraftContext(g,r.ClassAssignmentId):null;}return row;
    }
    public static JsonObject TeacherSummary(ClassState s){var c=ClassWire.Summary(s);c.Remove("version");return c;}
    public static JsonObject Gradebook(GradeState g)=>ClassWire.Object(new{@class=TeacherSummary(g.Class),components=Components(g.Class),items=g.Reports.OrderBy(x=>x.ClassAssignmentId).Select(x=>Student(g,x,true)),assignmentIds=g.Class.Assignments.Select(x=>x.ClassAssignmentId),context=Configuration(g.Class)});
    public static JsonObject Detail(GradeState g,int assignment){var r=g.Reports.SingleOrDefault(x=>x.ClassAssignmentId==assignment)??throw new ApiErrorException(ApiErrorCode.NOT_FOUND);var d=Student(g,r,true);d["class"]=TeacherSummary(g.Class);d["components"]=Components(g.Class);return d;}
    public static JsonObject SubmissionPreview(GradeState g,string action,DateTime now)
    {
        var eligible=WorkflowEligible(g,false,now);
        var items=g.Reports.OrderBy(x=>x.ClassAssignmentId).Select(x=>ClassWire.Object(new{assignment=g.Assignments[x.ClassAssignmentId],report=Report(x),computation=g.Computations[x.ClassAssignmentId]}));
        var issues=g.Computations.SelectMany(x=>x.Value["missingComponentIds"]!.AsArray().Select(v=>new{classAssignmentId=(int?)x.Key,componentId=(int?)v!.GetValue<int>(),message="Chưa nhập điểm thành phần."}));
        return ClassWire.Object(new{@class=TeacherSummary(g.Class),action,cohortSize=g.Class.Assignments.Length,states=new{draft=g.Reports.Count(x=>x.Status=="DRAFT"),submitted=g.Reports.Count(x=>x.Status=="SUBMITTED"),returned=g.Reports.Count(x=>x.Status=="RETURNED"),finalized=g.Reports.Count(x=>x.Status=="FINALIZED")},items,eligible,blockedReasons=eligible?Array.Empty<string>():new[]{"Lớp phải hoàn thành, cả lớp ở trạng thái nháp/trả lại và đủ điểm hợp lệ."},issues,effects=new[]{"Gửi bảng điểm cả lớp; khóa sửa điểm, chưa công bố cho học viên."},manifest=eligible?WorkflowManifest(g):null});
    }
    public static JsonObject ReviewDetail(GradeState g,DateTime now)
    {
        var visible=g.Reports.Where(x=>x.Status!="DRAFT").ToArray();if(visible.Length==0)throw new ApiErrorException(ApiErrorCode.NOT_FOUND);
        var eligible=WorkflowEligible(g,true,now);
        return ClassWire.Object(new{@class=ClassWire.Summary(g.Class),components=Components(g.Class),cohortSize=g.Class.Assignments.Length,visibleReportCount=visible.Length,hasCompleteVisibleCohort=visible.Length==g.Class.Assignments.Length,items=visible.OrderBy(x=>x.ClassAssignmentId).Select(x=>Student(g,x,false)),allowedActions=eligible?new[]{"RETURN"}:Array.Empty<string>(),disabledReasons=eligible?new Dictionary<string,string>():new(){{"RETURN","Chỉ trả lại toàn lớp đã gửi và đủ điểm hợp lệ."}}});
    }
    public static JsonObject WorkflowResult(GradeState g,Guid operation)=>ClassWire.Object(new{classId=g.Class.Class.ClassId,reportIds=g.Reports.Select(x=>x.ReportId).Order(),reportCount=g.Reports.Length,status=g.Reports.Select(x=>x.Status).Distinct().Single(),operationId=operation});
}
