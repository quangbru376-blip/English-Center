namespace EnglishCenter.Api.Services.Classes;
using System.Text.Json;
using System.Text.Json.Nodes;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Common.Replay;
using EnglishCenter.Api.Common.Serialization;
using EnglishCenter.Api.Common.Time;
using EnglishCenter.Api.Domain.Identity;
using EnglishCenter.Api.Domain.Training;
using EnglishCenter.Api.Services.Academics;
public sealed record ClassState(Class Class, ProgramState Program, Account Teacher, bool IsTeacher, StudentClassAssignment[] Assignments, Schedule[] Schedules, int AttendanceCount, bool HasScores, bool HasAttendance=false, Attendance[]? AttendanceRows=null);
public sealed record ScheduleCandidate(string Type, DateOnly Date, TimeOnly StartTime, TimeOnly EndTime, string Room)
{
    public DateTime StartAt => VietnamTime.ToUtc(Date,StartTime).UtcDateTime;
    public DateTime EndAt => VietnamTime.ToUtc(Date,EndTime).UtcDateTime;
    public static ScheduleCandidate Parse(JsonElement e)
    {
        if(e.ValueKind!=JsonValueKind.Object||e.EnumerateObject().Count()!=5||e.EnumerateObject().Select(x=>x.Name).Distinct().Count()!=5||e.EnumerateObject().Any(x=>!new[]{"type","date","startTime","endTime","room"}.Contains(x.Name)))throw ApiErrorException.Validation();
        try {var t=e.GetProperty("type").GetString();if(t is not("LESSON" or "FINAL_EXAM"))throw new ArgumentException();var d=StrictScalars.ParseLocalDate(e.GetProperty("date").GetString());var start=StrictScalars.ParseMinuteTime(e.GetProperty("startTime").GetString());var end=StrictScalars.ParseMinuteTime(e.GetProperty("endTime").GetString());if(start>=end)throw new ArgumentException();var candidate=new ScheduleCandidate(t,d,start,end,ClassWire.Code(e.GetProperty("room").GetString()!));_=candidate.StartAt;_=candidate.EndAt;return candidate;}
        catch(Exception x) when(x is ArgumentException or InvalidOperationException or KeyNotFoundException){throw ApiErrorException.Validation();}
    }
}
public sealed class ClassInput
{
    public int Api {get;} public int Id {get;} public Guid Request {get;} public CanonicalIntent Intent {get;} public JsonElement Body {get;}
    public ClassInput(int api,int id,Guid request,string json,int bytes)
    {if(api is not(92 or 94 or 97 or 99 or 100 or 101 or 102 or 108 or 118)||request==Guid.Empty)throw ApiErrorException.Validation();Api=api;Id=id;Request=request;Intent=CanonicalIntent.Bind(api,json,api==92?new Dictionary<string,int>():new(){{"classId",id}},bytes);using var doc=JsonDocument.Parse(Intent.CopyBytes());Body=doc.RootElement.GetProperty("body").Clone();}
}
public sealed record ClassCommandOutcome(JsonObject Result,bool Replayed);
public static class ClassWire
{
    public static string Code(string? raw){if(raw is null)throw ApiErrorException.Validation();var c=raw.Trim().ToUpperInvariant();if(c.Length is <1 or >30||c.Any(x=>!char.IsAsciiLetterUpper(x)&&!char.IsAsciiDigit(x)&&x!='_'&&x!='-'))throw ApiErrorException.Validation();return c;}
    private static readonly JsonSerializerOptions Options=new(JsonSerializerDefaults.Web);
    public static JsonObject Object(object value)=>JsonSerializer.SerializeToNode(value,Options)!.AsObject();
    public static JsonArray Array(IEnumerable<JsonNode> nodes)=>new(nodes.Select(x=>(JsonNode?)x.DeepClone()).ToArray());
    public static string Rv(byte[] v)=>RowVersionCodec.Encode(v);
    public static string At(DateTime v)=>AcademicGuard.Instant(v);
    public static JsonObject Program(ProgramState s)=>Object(new{id=s.Program.ProgramId,name=s.Program.ProgramName,course=new{id=s.Course.CourseId,code=s.Course.CourseCode,name=s.Course.CourseName,status=s.Course.Status},band=new{id=s.Band.BandId,code=s.Band.BandCode,minScore=ExactNumbers.FormatScore(s.Band.MinScore)},status=s.Program.Status,expectedLessonCount=s.Program.ExpectedLessonCount});
    public static JsonObject Teacher(Account a)=>Object(new{accountId=a.AccountId,displayName=a.FullName,status=a.Status});
    public static JsonObject Summary(ClassState s)=>new(){["id"]=s.Class.ClassId,["code"]=s.Class.ClassCode,["program"]=Program(s.Program),["teacher"]=Teacher(s.Teacher),["status"]=s.Class.Status,["maxSize"]=s.Class.MaxSize,["placedCount"]=s.Assignments.Length,["createdAt"]=At(s.Class.CreatedAt),["version"]=Rv(s.Class.RowVersion)};
    public static JsonObject Binding(Schedule x,bool version=false){var b=Object(new{id=x.ScheduleId,classId=x.ClassId,obligationKey=x.ObligationKey,sessionType=x.SessionType,isCurrent=x.IsCurrent,replacesScheduleId=x.ReplacesScheduleId});if(version)b["version"]=Rv(x.RowVersion);return b;}
    public static ScheduleCandidate Window(Schedule s)=>new(s.SessionType,s.SessionDate,s.StartTime,s.EndTime,s.Room);
    public static JsonObject Prerequisites(ClassState s)
    {var current=s.Schedules.Where(x=>x.IsCurrent).ToArray();var lessons=current.Where(x=>x.SessionType=="LESSON").ToArray();return Object(new{expectedLessonCount=s.Program.Program.ExpectedLessonCount,currentLessonCount=lessons.Length,heldLessonCount=lessons.Count(x=>x.OccurrenceState=="HELD"),currentFinalExamCount=current.Count(x=>x.SessionType=="FINAL_EXAM"),heldFinalExamCount=current.Count(x=>x.SessionType=="FINAL_EXAM"&&x.OccurrenceState=="HELD"),markedLessonAttendanceCount=s.AttendanceCount,requiredLessonAttendanceCount=(long)lessons.Length*s.Assignments.Length,firstLessonStartAt=lessons.Length==0?null:At(lessons.Min(x=>Window(x).StartAt))});}
    public static JsonObject Progress(ClassState s)
    {var lessons=s.Schedules.Where(x=>x.IsCurrent&&x.SessionType=="LESSON").ToArray();var exam=s.Schedules.SingleOrDefault(x=>x.IsCurrent&&x.SessionType=="FINAL_EXAM");if(lessons.Length>s.Program.Program.ExpectedLessonCount)throw new ApiErrorException(ApiErrorCode.INTERNAL_ERROR);return Object(new{expectedLessonCount=s.Program.Program.ExpectedLessonCount,lessonObligationCount=lessons.Length,heldLessonObligationCount=lessons.Count(x=>x.OccurrenceState=="HELD"),remainingLessonObligationCount=s.Program.Program.ExpectedLessonCount-lessons.Length,finalExamScheduleId=exam?.ScheduleId,heldFinalExamCount=exam?.OccurrenceState=="HELD"?1:0});}
    public static JsonObject BaseManifest(ClassState s)=>new(){["expectedVersions"]=Object(new{@class=Rv(s.Class.RowVersion),program=Rv(s.Program.Program.RowVersion),teacher=Rv(s.Teacher.RowVersion),schedules=s.Schedules.Select(x=>new{id=x.ScheduleId,version=Rv(x.RowVersion)})}),["expectedSet"]=new JsonObject{["assignmentIds"]=JsonSerializer.SerializeToNode(s.Assignments.Select(x=>x.ClassAssignmentId)),["scheduleBindings"]=Array(s.Schedules.Select(x=>Binding(x)))}};
    public static bool BeforeTeaching(ClassState s,DateTime now)=>s.Class.Status is "PLANNED" or "OPEN"&&!s.HasAttendance&&!s.HasScores&&!s.Schedules.Any(x=>x.IsCurrent&&(x.OccurrenceState=="HELD"||x.SessionType=="LESSON"&&Window(x).StartAt<=now));
    public static bool CanStart(ClassState s,DateTime now)=>s.Class.Status=="OPEN"&&s.Assignments.Length>0&&s.Schedules.Count(x=>x.IsCurrent&&x.SessionType=="LESSON")==s.Program.Program.ExpectedLessonCount&&s.Schedules.Any(x=>x.IsCurrent&&x.SessionType=="LESSON"&&Window(x).StartAt<=now);
    public static bool CanComplete(ClassState s)=>s.Class.Status=="ONGOING"&&s.Assignments.Length>0&&s.Schedules.Count(x=>x.IsCurrent&&x.SessionType=="LESSON")==s.Program.Program.ExpectedLessonCount&&s.Schedules.Count(x=>x.IsCurrent&&x.SessionType=="FINAL_EXAM")==1&&s.Schedules.Where(x=>x.IsCurrent).All(x=>x.OccurrenceState=="HELD")&&s.Schedules.Where(x=>x.IsCurrent&&x.SessionType=="LESSON").All(x=>s.Assignments.All(a=>(s.AttendanceRows??[]).Any(t=>t.ScheduleId==x.ScheduleId&&t.ClassAssignmentId==a.ClassAssignmentId)));
    public static JsonObject CompleteManifest(ClassState s){var m=BaseManifest(s);var ids=s.Schedules.Where(x=>x.IsCurrent&&x.SessionType=="LESSON").Select(x=>x.ScheduleId).ToArray();var rows=(s.AttendanceRows??[]).Where(x=>ids.Contains(x.ScheduleId)&&s.Assignments.Any(a=>a.ClassAssignmentId==x.ClassAssignmentId)).ToArray();m["expectedVersions"]!["attendances"]=System.Text.Json.JsonSerializer.SerializeToNode(rows.Select(x=>new{id=x.AttendanceId,version=Rv(x.RowVersion)}));m["expectedSet"]!["attendanceBindings"]=System.Text.Json.JsonSerializer.SerializeToNode(rows.Select(x=>new{attendanceId=x.AttendanceId,scheduleId=x.ScheduleId,classAssignmentId=x.ClassAssignmentId}));return m;}
    public static bool TeacherValid(ClassState s)=>s.IsTeacher&&s.Teacher.Status=="ACTIVE";
    public static bool CanOpen(ClassState s,DateTime now)=>s.Class.Status=="PLANNED"&&s.Program.Program.Status=="ACTIVE"&&ProgramProjection.Validate(s.Program).MeetsActivationRules&&TeacherValid(s)&&BeforeTeaching(s,now)&&s.Schedules.Any(x=>x.IsCurrent&&x.SessionType=="LESSON"&&x.OccurrenceState=="PLANNED"&&Window(x).StartAt>now);
    public static JsonObject Cancellation(ClassState s)=>Object(new{at=At(s.Class.CancelledAt!.Value),reason=s.Class.CancellationReason});
    public static JsonObject Detail(ClassState s,DateTime now)
    {var d=Summary(s);d["prerequisites"]=Prerequisites(s);d["cancellation"]=s.Class.CancelledAt is null?null:Cancellation(s);var flags=new Dictionary<string,bool>{{"CREATE_SCHEDULE",s.Class.Status is not("COMPLETED" or "CANCELLED")&&TeacherValid(s)},{"EDIT_CAPACITY",BeforeTeaching(s,now)},{"CHANGE_TEACHER",BeforeTeaching(s,now)},{"PREVIEW_TRANSITION",true},{"OPEN",CanOpen(s,now)},{"START",CanStart(s,now)},{"COMPLETE",CanComplete(s)},{"CANCEL",BeforeTeaching(s,now)&&s.Assignments.Length==0},{"VIEW_ROSTER",true},{"PLACE_REGISTRATION",s.Class.Status=="OPEN"&&BeforeTeaching(s,now)&&TeacherValid(s)&&s.Assignments.Length<s.Class.MaxSize}};d["allowedActions"]=JsonSerializer.SerializeToNode(flags.Where(x=>x.Value).Select(x=>x.Key));d["disabledReasons"]=JsonSerializer.SerializeToNode(flags.Where(x=>!x.Value).ToDictionary(x=>x.Key,_=>"Trạng thái, nguồn lực hoặc thời điểm chưa cho phép."));d["guardContext"]=Object(new{program=Rv(s.Program.Program.RowVersion),teacher=Rv(s.Teacher.RowVersion),assignments=s.Assignments.Select(x=>new{id=x.ClassAssignmentId,registrationId=x.RegistrationId}),schedules=s.Schedules.Select(x=>Binding(x,true))});return d;}
    public static JsonObject Candidate(ScheduleCandidate c)=>Object(new{type=c.Type,date=c.Date.ToString("yyyy-MM-dd"),startTime=c.StartTime.ToString("HH:mm"),endTime=c.EndTime.ToString("HH:mm"),room=c.Room});
}
