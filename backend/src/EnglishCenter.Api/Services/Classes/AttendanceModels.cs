namespace EnglishCenter.Api.Services.Classes;

using System.Text.Json;
using System.Text.Json.Nodes;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Common.Replay;
using EnglishCenter.Api.Domain.Training;

public sealed class AttendanceInput
{
    public int ClassId { get; }
    public int ScheduleId { get; }
    public Guid Request { get; }
    public CanonicalIntent Intent { get; }
    public JsonElement Body { get; }
    public AttendanceInput(int classId, int scheduleId, Guid request, string json, int bytes)
    {
        if (request == Guid.Empty) throw ApiErrorException.Validation();
        ClassId = classId; ScheduleId = scheduleId; Request = request;
        Intent = CanonicalIntent.Bind(133, json, new Dictionary<string,int>{{"classId",classId},{"scheduleId",scheduleId}}, bytes);
        using var doc = JsonDocument.Parse(Intent.CopyBytes()); Body = doc.RootElement.GetProperty("body").Clone();
    }
    public void RequireManifest(JsonObject manifest)
    {
        var body = JsonNode.Parse(Body.GetRawText())!.AsObject();
        body["expectedVersions"] = manifest["expectedVersions"]!.DeepClone();
        body["expectedSet"] = manifest["expectedSet"]!.DeepClone();
        var fresh = CanonicalIntent.Bind(133, body.ToJsonString(), new Dictionary<string,int>{{"classId",ClassId},{"scheduleId",ScheduleId}}, int.MaxValue);
        if (!fresh.CopyBytes().AsSpan().SequenceEqual(Intent.CopyBytes())) throw Academics.AcademicGuard.Conflict();
    }
}

public static class AttendanceWire
{
    public static bool CanEdit(ClassState state, Schedule lesson, DateTime now) =>
        state.Class.Status is not ("COMPLETED" or "CANCELLED") && lesson.SessionType == "LESSON" &&
        lesson.IsCurrent && lesson.OccurrenceState != "NOT_HELD" && now >= ClassWire.Window(lesson).StartAt;
    public static bool CountsToward(ClassState s, Schedule lesson) =>
        s.Class.Status != "CANCELLED" && lesson.IsCurrent && lesson.OccurrenceState != "NOT_HELD";
    public static JsonObject Counts(ClassState state, Schedule lesson)
    {
        var records = (state.AttendanceRows??[]).Where(x=>x.ScheduleId==lesson.ScheduleId&&state.Assignments.Any(a=>a.ClassAssignmentId==x.ClassAssignmentId)).ToArray();
        return ClassWire.Object(new{rosterCount=state.Assignments.Length,recordedCount=records.Length,unrecordedCount=state.Assignments.Length-records.Length,
            presentCount=records.Count(x=>x.Present),absentCount=records.Count(x=>!x.Present),isComplete=records.Length==state.Assignments.Length});
    }
    public static JsonObject Coverage(ClassState state)
    {
        var lessons=state.Schedules.Where(x=>x.SessionType=="LESSON"&&CountsToward(state,x)).ToArray();
        var required=(long)lessons.Length*state.Assignments.Length;
        var recorded=(state.AttendanceRows??[]).LongCount(x=>lessons.Any(s=>s.ScheduleId==x.ScheduleId)&&state.Assignments.Any(a=>a.ClassAssignmentId==x.ClassAssignmentId));
        return ClassWire.Object(new{currentLessonCount=lessons.Length,requiredRecordCount=required,recordedRecordCount=recorded,missingRecordCount=required-recorded,isComplete=recorded==required});
    }
    public static JsonObject Editing(ClassState s, Schedule lesson, DateTime now) =>
        ClassWire.Object(new{canEdit=CanEdit(s,lesson,now),opensAt=ClassWire.At(ClassWire.Window(lesson).StartAt),
            blockedReasons=CanEdit(s,lesson,now)?Array.Empty<string>():new[]{"Chưa đến giờ học, lớp đã kết thúc hoặc buổi thuộc lịch sử/không tổ chức."}});
    public static JsonObject ClassSummary(ClassState s,bool teacher)
    {var summary=ClassWire.Summary(s);if(teacher)summary.Remove("version");return summary;}
    public static JsonObject Lesson(ClassState s,Schedule lesson)
    {
        var row=Infrastructure.Classes.SqlClassReads.ScheduleView(s,lesson);row.Remove("class");row.Remove("version");row["classId"]=s.Class.ClassId;return row;
    }
    public static JsonObject Record(Attendance row,bool teacher)
    {
        var wire=ClassWire.Object(new{id=row.AttendanceId,present=row.Present?1:0,enteredByAccountId=row.EnteredBy,enteredAt=ClassWire.At(row.EnteredAt)});
        if(teacher)wire["version"]=ClassWire.Rv(row.RowVersion);return wire;
    }
    public static JsonObject Context(ClassState s,Schedule lesson)=>ClassWire.Object(new{
        expectedVersions=new{@class=ClassWire.Rv(s.Class.RowVersion),teacher=ClassWire.Rv(s.Teacher.RowVersion),schedule=ClassWire.Rv(lesson.RowVersion)},
        expectedSet=new{assignmentIds=s.Assignments.Select(x=>x.ClassAssignmentId)}});
    public static JsonObject SaveManifest(ClassState s,Schedule lesson,int[] changed)
    {
        var m=Context(s,lesson);
        var existing=(s.AttendanceRows??[]).Where(x=>x.ScheduleId==lesson.ScheduleId&&changed.Contains(x.ClassAssignmentId)).ToArray();
        m["expectedVersions"]!["attendance"]=JsonSerializer.SerializeToNode(existing.Select(x=>new{id=x.AttendanceId,version=ClassWire.Rv(x.RowVersion)}));
        m["expectedSet"]!["attendanceBindings"]=JsonSerializer.SerializeToNode(existing.Select(x=>new{classAssignmentId=x.ClassAssignmentId,attendanceId=x.AttendanceId}));
        m["expectedSet"]!["unrecordedAssignmentIds"]=JsonSerializer.SerializeToNode(changed.Where(x=>!existing.Any(a=>a.ClassAssignmentId==x)));
        return m;
    }
}
