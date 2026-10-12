namespace EnglishCenter.Tests.Common;
using EnglishCenter.Api.Domain.Identity;
using EnglishCenter.Api.Domain.Academics;
using EnglishCenter.Api.Domain.Training;
using EnglishCenter.Api.Services.Academics;
using EnglishCenter.Api.Services.Classes;
using Xunit;

public sealed class M3TeachingContractTests
{
    private static readonly DateTime Start=new(2026,10,12,3,0,0,DateTimeKind.Utc);
    private static ClassState State(string status="OPEN")
    {
        var program=new ProgramState(new TrainingProgram{ProgramId=2,ExpectedLessonCount=2,Status="INACTIVE",RowVersion=new byte[8]},new Course(),new Band(),[],true);
        var schedules=Enumerable.Range(0,2).Select(i=>new Schedule{ScheduleId=8+i,ClassId=5,SessionType="LESSON",SessionDate=new(2026,10,12),StartTime=new(10+i,0),EndTime=new(11+i,0),IsCurrent=true,OccurrenceState="PLANNED",ObligationKey=Guid.NewGuid(),RowVersion=new byte[8]}).ToArray();
        return new(new Class{ClassId=5,ProgramId=2,TeacherId=4,Status=status,RowVersion=new byte[8]},program,new Account{AccountId=4,Status="ACTIVE",RowVersion=new byte[8]},true,[new StudentClassAssignment{ClassAssignmentId=6,ClassId=5}],schedules,0,false);
    }
    [Fact]
    public void StartRequiresWholeLessonSetCohortAndStartBoundary_WithoutExamOrProgramActiveGuard()
    {
        var s=State();Assert.False(ClassWire.CanStart(s,Start.AddTicks(-1)));Assert.True(ClassWire.CanStart(s,Start));
        Assert.False(ClassWire.CanStart(s with{Assignments=[]},Start));Assert.False(ClassWire.CanStart(s with{Schedules=[s.Schedules[0]]},Start));s.Class.Status="PLANNED";Assert.False(ClassWire.CanStart(s,Start));
    }
    [Fact]
    public void CompleteUsesFullCurrentLessonCoverage_AbsentValidAndExamNeverNeedsAttendance()
    {
        var s=State("ONGOING");foreach(var lesson in s.Schedules)lesson.OccurrenceState="HELD";
        var exam=new Schedule{ScheduleId=10,ClassId=5,SessionType="FINAL_EXAM",SessionDate=new(2026,10,12),StartTime=new(12,0),EndTime=new(13,0),IsCurrent=true,OccurrenceState="HELD",ObligationKey=Guid.NewGuid(),RowVersion=new byte[8]};
        var rows=s.Schedules.Select((x,i)=>new Attendance{AttendanceId=i+1,ClassAssignmentId=6,ScheduleId=x.ScheduleId,Present=false,RowVersion=new byte[8]}).ToArray();s=s with{Schedules=[..s.Schedules,exam],AttendanceRows=rows};
        Assert.True(ClassWire.CanComplete(s));Assert.False(ClassWire.CanComplete(s with{AttendanceRows=[rows[0]]}));Assert.False(ClassWire.CanComplete(s with{Schedules=s.Schedules.Where(x=>x.SessionType=="LESSON").ToArray()}));
        Assert.Equal(2,ClassWire.CompleteManifest(s)["expectedVersions"]!["attendances"]!.AsArray().Count);exam.OccurrenceState="PLANNED";Assert.False(ClassWire.CanComplete(s));
    }
    [Fact]
    public void AttendanceStartsBeforeClassStartTransition_AndHistoricalOrEndedRowsAreReadOnly()
    {
        var s=State();var lesson=s.Schedules[0];Assert.True(AttendanceWire.CanEdit(s,lesson,Start));Assert.False(AttendanceWire.CanEdit(s,lesson,Start.AddTicks(-1)));
        lesson.IsCurrent=false;Assert.False(AttendanceWire.CanEdit(s,lesson,Start));lesson.IsCurrent=true;lesson.OccurrenceState="NOT_HELD";Assert.False(AttendanceWire.CanEdit(s,lesson,Start));lesson.OccurrenceState="HELD";Assert.True(AttendanceWire.CanEdit(s,lesson,Start.AddDays(5)));s.Class.Status="COMPLETED";Assert.False(AttendanceWire.CanEdit(s,lesson,Start.AddDays(5)));
    }
    [Fact]
    public void HeldIsExplicitEndBoundaryAndCoverageIsPageIndependent_CancelledCoverageVacuous()
    {
        var s=State("ONGOING");Assert.False(TeachingHeld.CanConfirm(s,s.Schedules[0],Start.AddHours(1).AddTicks(-1)));Assert.True(TeachingHeld.CanConfirm(s,s.Schedules[0],Start.AddHours(1)));
        var count=AttendanceWire.Coverage(s);Assert.Equal(2,count["requiredRecordCount"]!.GetValue<long>());Assert.False(count["isComplete"]!.GetValue<bool>());s.Class.Status="CANCELLED";count=AttendanceWire.Coverage(s);Assert.Equal(0,count["requiredRecordCount"]!.GetValue<long>());Assert.True(count["isComplete"]!.GetValue<bool>());Assert.False(TeachingHeld.CanConfirm(s,s.Schedules[0],Start.AddDays(5)));
    }
}
