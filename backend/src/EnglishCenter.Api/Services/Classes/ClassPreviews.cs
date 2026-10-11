namespace EnglishCenter.Api.Services.Classes;
using System.Text.Json.Nodes;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Common.Serialization;
using EnglishCenter.Api.Domain.Identity;
using EnglishCenter.Api.Infrastructure.Classes;

public static class ClassPreviews
{
    public static JsonObject Candidate(PlacementSource s,bool detail)
    {var d=ClassWire.Object(new{registrationId=s.Registration.RegistrationId,student=new{studentId=s.Registration.StudentId,fullName=s.Student.FullName,email=s.Student.Email,phone=s.Student.Phone},course=new{id=s.Course.CourseId,code=s.Course.CourseCode,name=s.Course.CourseName,status=s.Course.Status},band=new{id=s.Band.BandId,code=s.Band.BandCode,minScore=ExactNumbers.FormatScore(s.Band.MinScore)},entryScore=ExactNumbers.FormatScore(s.Best.Score!.Value),registrationStatus="ADMITTED",invoiceStatus="PAID",admittedAt=ClassWire.At(s.Registration.AdmissionApprovedAt!.Value),registrationVersion=ClassWire.Rv(s.Registration.RowVersion)});if(detail){d["bandAssignment"]=ClassWire.Object(new{id=s.BandAssignment.BandAssignmentId,version=ClassWire.Rv(s.BandAssignment.RowVersion)});d["invoice"]=ClassWire.Object(new{id=s.Invoice.InvoiceId,status="PAID",version=ClassWire.Rv(s.Invoice.RowVersion)});d["assignment"]=null;}return d;}
    public static JsonObject Transition(ClassState s,string action,DateTime now,TimetableResult resources)
    {
        var eligible=action=="OPEN"?ClassWire.CanOpen(s,now)&&resources.Conflicts.Count==0:action=="CANCEL"&&ClassWire.BeforeTeaching(s,now)&&s.Assignments.Length==0;
        var manifest=ClassWire.BaseManifest(s);if(action=="CANCEL"){manifest["expectedSet"]!["hasAttendance"]=false;manifest["expectedSet"]!["hasScores"]=false;}
        return new(){["class"]=ClassWire.Summary(s),["action"]=action,["prerequisites"]=ClassWire.Prerequisites(s),["eligible"]=eligible,["blockedReasons"]=ClassWire.Object(new{items=eligible?Array.Empty<string>():new[]{"Trạng thái, lịch tương lai, nguồn lực hoặc cửa sổ thao tác chưa hợp lệ."}})["items"]!.DeepClone(),["effects"]=ClassWire.Object(new{items=action=="CANCEL"?new[]{"Hủy lớp trống và giải phóng lịch hiệu lực; giữ lịch sử và khóa cấu hình chương trình."}:action=="OPEN"?new[]{"Mở lớp nhận học viên; chưa bắt đầu giảng dạy."}:new[]{"Thao tác thuộc giai đoạn giảng dạy sau."}})["items"]!.DeepClone(),["manifest"]=eligible?manifest:null};
    }
    public static JsonObject Teacher(ClassState s,Account a,DateTime now,TimetableResult resources)
    {
        var proposed=a.AccountId;var eligible=ClassWire.BeforeTeaching(s,now)&&a.Status=="ACTIVE"&&proposed!=s.Class.TeacherId&&resources.Conflicts.Count==0;
        var m=ClassWire.BaseManifest(s);var v=m["expectedVersions"]!.AsObject();v.Remove("teacher");v["currentTeacher"]=ClassWire.Rv(s.Teacher.RowVersion);v["proposedTeacher"]=ClassWire.Rv(a.RowVersion);m["expectedSet"]!["hasAttendance"]=false;m["expectedSet"]!["hasScores"]=false;
        return ClassWire.Object(new{@class=ClassWire.Summary(s),proposedTeacher=ClassWire.Teacher(a),eligible,blockedReasons=eligible?Array.Empty<string>():new[]{"Giáo viên, thời điểm, dữ liệu phụ thuộc hoặc lịch trùng chưa cho phép."},conflicts=resources.Conflicts,manifest=eligible?m:null});
    }
    public static JsonObject Placement(ClassState s,PlacementSource source,DateTime now,TimetableResult student,TimetableResult resources,bool alreadyAssigned)
    {
        var conflicts=student.Conflicts.Concat(resources.Conflicts).ToArray();
        var eligible=s.Class.Status=="OPEN"&&ClassWire.BeforeTeaching(s,now)&&ClassWire.TeacherValid(s)&&s.Assignments.Length<s.Class.MaxSize&&source.Registration.Status=="ADMITTED"&&source.Invoice.Status=="PAID"&&source.Student.Status=="ACTIVE"&&source.Registration.CourseId==s.Program.Program.CourseId&&source.BandAssignment.BandId==s.Program.Program.BandId&&!alreadyAssigned&&conflicts.Length==0;
        var m=ClassWire.BaseManifest(s);var v=m["expectedVersions"]!.AsObject();v["registration"]=ClassWire.Rv(source.Registration.RowVersion);v["bandAssignment"]=ClassWire.Rv(source.BandAssignment.RowVersion);v["invoice"]=ClassWire.Rv(source.Invoice.RowVersion);v["classSchedules"]=v["schedules"]!.DeepClone();v.Remove("schedules");var set=m["expectedSet"]!.AsObject();set["bandAssignmentId"]=source.BandAssignment.BandAssignmentId;set["invoiceId"]=source.Invoice.InvoiceId;set["registrationAssignmentId"]=null;set["classAssignmentIds"]=set["assignmentIds"]!.DeepClone();set.Remove("assignmentIds");set["classScheduleBindings"]=set["scheduleBindings"]!.DeepClone();set.Remove("scheduleBindings");set["studentTimetable"]=student.Guard;
        return ClassWire.Object(new{candidate=Candidate(source,true),@class=ClassWire.Summary(s),eligible,blockedReasons=eligible?Array.Empty<string>():new[]{"Hồ sơ/lớp không khớp, hết chỗ, quá thời điểm hoặc trùng lịch."},conflicts,manifest=eligible?m:null});
    }
    public static JsonObject Schedule(ClassState s,ScheduleCandidate candidate,DateTime now,TimetableResult resources,TimetableResult cohort)
    {
        var conflicts=resources.Conflicts.Concat(cohort.Conflicts).ToArray();var current=s.Schedules.Where(x=>x.IsCurrent).ToArray();var lessons=current.Where(x=>x.SessionType=="LESSON").ToArray();var exam=current.SingleOrDefault(x=>x.SessionType=="FINAL_EXAM");
        var order=candidate.Type=="FINAL_EXAM"?exam is null&&lessons.Length>0&&candidate.StartAt>=lessons.Max(x=>ClassWire.Window(x).EndAt):lessons.Length<s.Program.Program.ExpectedLessonCount&&(exam is null||candidate.EndAt<=ClassWire.Window(exam).StartAt);
        var eligible=s.Class.Status is not("COMPLETED" or "CANCELLED")&&ClassWire.TeacherValid(s)&&candidate.StartAt>now&&order&&conflicts.Length==0;var m=ClassWire.BaseManifest(s);m["expectedSet"]!["resources"]=resources.Guard;m["expectedSet"]!["cohortTimetables"]=cohort.Guard["cohortTimetables"]!.DeepClone();
        return ClassWire.Object(new{@class=ClassWire.Summary(s),candidate=ClassWire.Candidate(candidate),progress=ClassWire.Progress(s),eligible,blockedReasons=eligible?Array.Empty<string>():new[]{"Lịch phải ở tương lai, đúng số nghĩa vụ/thứ tự thi và không trùng nguồn lực/học viên."},conflicts,manifest=eligible?m:null});
    }
    public static JsonObject SchedulingContext(ClassState s){var m=ClassWire.BaseManifest(s);return ClassWire.Object(new{@class=ClassWire.Summary(s),progress=ClassWire.Progress(s),schedules=s.Schedules.Select(x=>ClassWire.Binding(x,true)),currentObligations=s.Schedules.Where(x=>x.IsCurrent).Select(x=>ClassWire.Binding(x,true)),assignments=s.Assignments.Select(x=>new{id=x.ClassAssignmentId,registrationId=x.RegistrationId}),expectedVersions=m["expectedVersions"]});}
}
