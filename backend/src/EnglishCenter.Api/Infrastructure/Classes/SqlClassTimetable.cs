namespace EnglishCenter.Api.Infrastructure.Classes;
using System.Text.Json.Nodes;
using EnglishCenter.Api.Common.Time;
using EnglishCenter.Api.Services.Classes;
using Microsoft.EntityFrameworkCore;
public sealed record ClassConflict(string Kind,string StartAt,string EndAt,string Message);
public sealed record TimetableResult(JsonObject Guard,IReadOnlyList<ClassConflict> Conflicts);
public sealed record StudentLink(int StudentId,int ClassAssignmentId,int ClassId,byte[] RowVersion);
public sealed record StudentBooking(int StudentId,int BookingId,int AttemptId,int SessionId,byte[] BookingVersion,byte[] SessionVersion,DateTime StartAt,DateTime EndAt);
public sealed record StudentTimetableData(StudentLink[] Links,EnglishCenter.Api.Domain.Training.Schedule[] Schedules,StudentBooking[] Bookings);
public sealed record ResourceSchedule(EnglishCenter.Api.Domain.Training.Schedule Schedule,int TeacherId,byte[] ClassVersion);
public sealed record ResourceData(ResourceSchedule[] Schedules,EnglishCenter.Api.Domain.Admissions.EntryTestSession[] Sessions);
public static class SqlClassTimetable
{
    private static bool Overlap(DateTime a,DateTime b,DateTime c,DateTime d)=>a<d&&b>c;
    public static async Task<TimetableResult> Student(EnglishCenterDbContext db,int student,ScheduleCandidate[] windows,DateTime now,CancellationToken ct)
        =>Student(await Students(db,[student],now,ct),student,windows);
    public static async Task<StudentTimetableData> Students(EnglishCenterDbContext db,int[] students,DateTime now,CancellationToken ct,ScheduleWorkBudget? budget=null)
    {
        var links=await (from a in db.StudentClassAssignments.AsNoTracking() join r in db.Registrations on a.RegistrationId equals r.RegistrationId join c in db.Classes on a.ClassId equals c.ClassId where students.Contains(r.StudentId)&&c.Status!="CANCELLED" select new StudentLink(r.StudentId,a.ClassAssignmentId,c.ClassId,c.RowVersion)).Take(budget is null?int.MaxValue:budget.Remaining+1).ToArrayAsync(ct);budget?.Consume(links.Length);
        var classIds=links.Select(x=>x.ClassId).Distinct().ToArray();
        var schedules=await db.Schedules.AsNoTracking().Where(x=>classIds.Contains(x.ClassId)&&x.IsCurrent&&(x.OccurrenceState=="PLANNED"||x.OccurrenceState=="HELD")).OrderBy(x=>x.ScheduleId).Take(budget is null?int.MaxValue:budget.Remaining+1).ToArrayAsync(ct);budget?.Consume(schedules.Length);
        schedules=schedules.Where(x=>ClassWire.Window(x).EndAt>now).ToArray();
        var bookings=await (from b in db.EntryTestBookings.AsNoTracking() join a in db.EntryTestAttempts on b.AttemptId equals a.AttemptId join r in db.Registrations on a.RegistrationId equals r.RegistrationId join session in db.EntryTestSessions on b.SessionId equals session.SessionId where students.Contains(r.StudentId)&&r.Status!="CANCELLED"&&r.Status!="REJECTED"&&b.Status=="SCHEDULED"&&session.Status!="CANCELLED"&&session.EndAt>now orderby b.BookingId select new StudentBooking(r.StudentId,b.BookingId,b.AttemptId,b.SessionId,b.RowVersion,session.RowVersion,session.StartAt,session.EndAt)).Take(budget is null?int.MaxValue:budget.Remaining+1).ToArrayAsync(ct);budget?.Consume(bookings.Length);
        return new(links,schedules,bookings);
    }
    public static TimetableResult Student(StudentTimetableData data,int student,ScheduleCandidate[] windows,int? excludeSchedule=null,ScheduleWorkBudget? budget=null)
    {
        var links=data.Links.Where(x=>x.StudentId==student).OrderBy(x=>x.ClassAssignmentId).ToArray();
        var ids=links.Select(x=>x.ClassId).ToHashSet();var schedules=data.Schedules.Where(x=>ids.Contains(x.ClassId)).ToArray();
        var bookings=data.Bookings.Where(x=>x.StudentId==student).ToArray();
        var conflicts=new List<ClassConflict>();foreach(var w in windows){budget?.Consume(schedules.Length+bookings.Length+1);if(schedules.Any(x=>x.ScheduleId!=excludeSchedule&&Overlap(ClassWire.Window(x).StartAt,ClassWire.Window(x).EndAt,w.StartAt,w.EndAt))||bookings.Any(x=>Overlap(x.StartAt,x.EndAt,w.StartAt,w.EndAt)))conflicts.Add(new("STUDENT",ClassWire.At(w.StartAt),ClassWire.At(w.EndAt),"Học viên có lịch lớp hoặc ca thi trùng giờ."));}
        var guard=ClassWire.Object(new{bookings=bookings.Select(x=>new{bookingId=x.BookingId,attemptId=x.AttemptId,sessionId=x.SessionId,bookingVersion=ClassWire.Rv(x.BookingVersion),sessionVersion=ClassWire.Rv(x.SessionVersion)}),assignments=links.Select(x=>new{classAssignmentId=x.ClassAssignmentId,classId=x.ClassId,classVersion=ClassWire.Rv(x.RowVersion)}),schedules=schedules.Select(x=>new{scheduleId=x.ScheduleId,classId=x.ClassId,version=ClassWire.Rv(x.RowVersion)})});return new(guard,conflicts);
    }
    public static async Task<TimetableResult> Resources(EnglishCenterDbContext db,ClassState state,ScheduleCandidate[] windows,int teacher,bool ignoreOwn,CancellationToken ct)
        =>Resources(await ResourceSnapshot(db,[state.Class.ClassId],windows,[teacher],ct),state,windows,teacher,ignoreOwn);
    public static async Task<ResourceData> ResourceSnapshot(EnglishCenterDbContext db,int[] classes,ScheduleCandidate[] windows,int[] teachers,CancellationToken ct,ScheduleWorkBudget? budget=null)
    {
        if(windows.Length==0)return new([],[]);
        var first=windows.Min(x=>x.Date);var last=windows.Max(x=>x.Date);var rooms=windows.Select(x=>x.Room).Distinct().ToArray();var start=windows.Min(x=>x.StartAt);var end=windows.Max(x=>x.EndAt);
        var rows=await (from schedule in db.Schedules.AsNoTracking() join cl in db.Classes on schedule.ClassId equals cl.ClassId where schedule.IsCurrent&&cl.Status!="CANCELLED"&&(schedule.OccurrenceState=="PLANNED"||schedule.OccurrenceState=="HELD")&&schedule.SessionDate>=first&&schedule.SessionDate<=last&&(classes.Contains(schedule.ClassId)||rooms.Contains(schedule.Room)||teachers.Contains(cl.TeacherId)) select new ResourceSchedule(schedule,cl.TeacherId,cl.RowVersion)).Take(budget is null?int.MaxValue:budget.Remaining+1).ToArrayAsync(ct);budget?.Consume(rows.Length);
        var sessions=await db.EntryTestSessions.AsNoTracking().Where(x=>x.Status!="CANCELLED"&&x.StartAt<end&&x.EndAt>start&&(rooms.Contains(x.Room)||teachers.Contains(x.GraderId))).Take(budget is null?int.MaxValue:budget.Remaining+1).ToArrayAsync(ct);budget?.Consume(sessions.Length);
        return new(rows,sessions);
    }
    public static TimetableResult Resources(ResourceData data,ClassState state,ScheduleCandidate[] windows,int teacher,bool ignoreOwn,int? excludeSchedule=null,ScheduleWorkBudget? budget=null)
    {
        var scheduleRows=new Dictionary<int,(EnglishCenter.Api.Domain.Training.Schedule Schedule,int Teacher,byte[] ClassVersion)>();var sessionRows=new Dictionary<int,EnglishCenter.Api.Domain.Admissions.EntryTestSession>();var conflicts=new List<ClassConflict>();
        var allRows=data.Schedules;var allSessions=data.Sessions;
        foreach(var w in windows)
        {
            budget?.Consume(allRows.Length+allSessions.Length+1);
            var rows=allRows.Where(x=>x.Schedule.SessionDate==w.Date&&x.Schedule.StartTime<w.EndTime&&x.Schedule.EndTime>w.StartTime&&(x.Schedule.ClassId==state.Class.ClassId||x.Schedule.Room==w.Room||x.TeacherId==teacher)).ToArray();
            var sessions=allSessions.Where(x=>x.StartAt<w.EndAt&&x.EndAt>w.StartAt&&(x.Room==w.Room||x.GraderId==teacher)).ToArray();            foreach(var r in rows)scheduleRows[r.Schedule.ScheduleId]=(r.Schedule,r.TeacherId,r.ClassVersion);foreach(var r in sessions)sessionRows[r.SessionId]=r;
            var active=rows.Where(x=>x.Schedule.ScheduleId!=excludeSchedule&&(!ignoreOwn||x.Schedule.ClassId!=state.Class.ClassId)).ToArray();
            void Add(string kind,string message){conflicts.Add(new(kind,ClassWire.At(w.StartAt),ClassWire.At(w.EndAt),message));}
            if(active.Any(x=>x.Schedule.ClassId==state.Class.ClassId))Add("CLASS","Lớp đã có lịch trùng giờ.");
            if(active.Any(x=>x.Schedule.Room==w.Room)||sessions.Any(x=>x.Room==w.Room))Add("ROOM","Phòng đã được sử dụng trong khoảng giờ này.");
            if(active.Any(x=>x.TeacherId==teacher)||sessions.Any(x=>x.GraderId==teacher))Add("TEACHER","Giáo viên có lịch lớp hoặc ca thi trùng giờ.");
        }
        return new(ClassWire.Object(new{classSchedules=scheduleRows.Values.OrderBy(x=>x.Schedule.ScheduleId).Select(x=>new{scheduleId=x.Schedule.ScheduleId,classId=x.Schedule.ClassId,scheduleVersion=ClassWire.Rv(x.Schedule.RowVersion),classVersion=ClassWire.Rv(x.ClassVersion)}),entrySessions=sessionRows.Values.OrderBy(x=>x.SessionId).Select(x=>new{sessionId=x.SessionId,version=ClassWire.Rv(x.RowVersion)})}),conflicts);
    }
    public static async Task<TimetableResult> Cohort(EnglishCenterDbContext db,ClassState state,ScheduleCandidate[] windows,DateTime now,CancellationToken ct,int? excludeSchedule=null,ScheduleWorkBudget? budget=null)
    {
        budget?.Consume(state.Assignments.Length);var ids=state.Assignments.Select(x=>x.RegistrationId).ToArray();var students=await db.Registrations.Where(x=>ids.Contains(x.RegistrationId)).ToDictionaryAsync(x=>x.RegistrationId,x=>x.StudentId,ct);var all=new List<JsonNode>();var conflicts=new List<ClassConflict>();
        var data=await Students(db,students.Values.Distinct().ToArray(),now,ct,budget);
        foreach(var a in state.Assignments){var info=Student(data,students[a.RegistrationId],windows,excludeSchedule,budget);all.Add(new JsonObject{["classAssignmentId"]=a.ClassAssignmentId,["timetable"]=info.Guard});conflicts.AddRange(info.Conflicts);}return new(new(){["cohortTimetables"]=ClassWire.Array(all)},conflicts);
    }
}
