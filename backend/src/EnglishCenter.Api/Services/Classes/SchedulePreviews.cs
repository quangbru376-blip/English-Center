namespace EnglishCenter.Api.Services.Classes;

using System.Text.Json.Nodes;
using EnglishCenter.Api.Domain.Training;
using EnglishCenter.Api.Infrastructure.Classes;

public static class SchedulePreviews
{
    public static void DetailActions(JsonObject detail,ClassState state,Schedule row,DateTime now,bool attendance,bool score)
    {
        var allowed=new JsonArray("VIEW_CLASS");var disabled=new JsonObject();
        var common=row.IsCurrent&&row.OccurrenceState=="PLANNED"&&!attendance&&!score&&state.Class.Status is not("COMPLETED" or "CANCELLED")&&ClassWire.TeacherValid(state);
        foreach(var action in new[]{"MOVE","REPLACE"})
        {if(common&&(action=="MOVE"?now<ClassWire.Window(row).StartAt:now>=ClassWire.Window(row).StartAt))allowed.Add(action);else disabled[action]="Thời điểm, trạng thái hoặc dữ liệu phụ thuộc chưa cho phép; cần xem trước lịch mới.";}
        detail["allowedActions"]=allowed;detail["disabledReasons"]=disabled;
    }
    public static JsonObject Weekly(ClassState state,WeeklyRule rule,ScheduleCandidate[] candidates,DateTime now,TimetableResult resources,TimetableResult cohort)
    {
        var conflicts=resources.Conflicts.Concat(cohort.Conflicts).ToArray();
        var exam=state.Schedules.SingleOrDefault(x=>x.IsCurrent&&x.SessionType=="FINAL_EXAM");
        var eligible=candidates.Length>0&&state.Class.Status is not("COMPLETED" or "CANCELLED")&&ClassWire.TeacherValid(state)&&candidates.All(x=>x.StartAt>now)&&
            state.Schedules.Count(x=>x.IsCurrent&&x.SessionType=="LESSON")+candidates.Length<=state.Program.Program.ExpectedLessonCount&&
            (exam is null||candidates.All(x=>x.EndAt<=ClassWire.Window(exam).StartAt))&&conflicts.Length==0;
        return ClassWire.Object(new{@class=ClassWire.Summary(state),rule=rule.Wire(),candidates=candidates.Select(ClassWire.Candidate),progress=ClassWire.Progress(state),candidateCount=candidates.Length,eligible,
            blockedReasons=eligible?Array.Empty<string>():new[]{"Toàn bộ lịch phải ở tương lai, đúng số nghĩa vụ/thứ tự thi và không trùng lịch."},
            conflicts=candidates.Select((candidate,index)=>new{candidateIndex=index,candidate=ClassWire.Candidate(candidate),conflicts=conflicts.Where(x=>x.StartAt==ClassWire.At(candidate.StartAt)&&x.EndAt==ClassWire.At(candidate.EndAt)).ToArray()}).Where(x=>x.conflicts.Length>0),manifest=eligible?Manifest(state,resources,cohort):null});
    }
    public static JsonObject Edit(ClassState state,Schedule old,ScheduleCandidate proposed,string? reason,DateTime now,bool attendance,bool score,JsonObject before,TimetableResult guard,TimetableResult resources,TimetableResult cohort)
    {
        var conflicts=resources.Conflicts.Concat(cohort.Conflicts).ToArray();
        var eligible=ScheduleRules.Editable(state,old,proposed,now,reason is not null,attendance,score)&&conflicts.Length==0;
        var manifest=Manifest(state,guard,cohort);
        manifest["expectedVersions"]!["schedule"]=ClassWire.Rv(old.RowVersion);
        manifest["expectedSet"]!["targetScheduleId"]=old.ScheduleId;
        manifest["expectedSet"]!["obligationKey"]=old.ObligationKey;
        manifest["expectedSet"]!["dependencies"]=ClassWire.Object(new{scheduleId=old.ScheduleId,hasAttendance=false,hasLinkedScore=false});
        var result=ClassWire.Object(new{before,proposed=ClassWire.Candidate(proposed),eligible,blockedReasons=eligible?Array.Empty<string>():new[]{"Cửa sổ thao tác, dữ liệu phụ thuộc, thứ tự nghĩa vụ hoặc lịch trùng chưa cho phép."},conflicts,manifest=eligible?manifest:null});
        if(reason is not null)result["reason"]=reason;
        return result;
    }
    private static JsonObject Manifest(ClassState state,TimetableResult resources,TimetableResult cohort)
    {var manifest=ClassWire.BaseManifest(state);manifest["expectedSet"]!["resources"]=resources.Guard.DeepClone();manifest["expectedSet"]!["cohortTimetables"]=cohort.Guard["cohortTimetables"]!.DeepClone();return manifest;}
}
