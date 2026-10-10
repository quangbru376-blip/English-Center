namespace EnglishCenter.Tests.Common;

using System.Text.Json;
using EnglishCenter.Api.Domain.Training;
using EnglishCenter.Api.Services.Enrollment;
using Xunit;

public sealed partial class M1EnrollmentContractTests
{
    [Fact]
    public void HomeTiesChooseClassBeforeEntryAndKeepKindSpecificScopedIds()
    {
        var state=State();state.Session(5).StartAt=Now.UtcDateTime.AddMinutes(1);state.Session(5).EndAt=Now.UtcDateTime.AddHours(1);state.Session(5).Status="OPEN";
        var placement=new StudentClassAssignment{ClassAssignmentId=20,RegistrationId=3,ClassId=21};var cls=new Class{ClassId=21,ClassCode="C1",Status="ONGOING"};
        var schedule=new Schedule{ScheduleId=22,ClassId=21,SessionDate=new(2026,10,9),StartTime=new(10,1),EndTime=new(11,0),Room="CLASS",IsCurrent=true,OccurrenceState="PLANNED"};
        state=new(state.Set with{Placements=[placement],Classes=[cls],Schedules=[schedule]},state.Registration);
        var home=StudentHomeProjection.Home(state,Now);var first=JsonSerializer.SerializeToElement(home.NearestEvent,Wire);
        Assert.Equal("CLASS_SESSION",first.GetProperty("kind").GetString());Assert.Equal(21,first.GetProperty("classId").GetInt32());Assert.Equal(22,first.GetProperty("scheduleId").GetInt32());Assert.False(first.TryGetProperty("bookingId",out _));
        schedule.OccurrenceState="HELD";var second=JsonSerializer.SerializeToElement(StudentHomeProjection.Home(state,Now).NearestEvent,Wire);
        Assert.Equal("ENTRY_TEST",second.GetProperty("kind").GetString());Assert.Equal(8,second.GetProperty("bookingId").GetInt32());Assert.False(second.TryGetProperty("classId",out _));
        state.Registration.Status="CANCELLED";Assert.Null(StudentHomeProjection.Home(state,Now).NearestEvent);
    }
    [Fact]
    public void HomeFinalizedReportContainsOnlyOwnReferencesAndDraftHasNoCapability()
    {
        var state=State(true);var placement=new StudentClassAssignment{ClassAssignmentId=20,RegistrationId=3,ClassId=21};var cls=new Class{ClassId=21,ClassCode="C1",Status="COMPLETED"};
        var report=new GradeReport{ReportId=22,ClassAssignmentId=20,Status="DRAFT",FinalScore=99.99m,TeacherComment="Unpublished"};
        state=new(state.Set with{Placements=[placement],Classes=[cls],Reports=[report]},state.Registration);
        Assert.Null(StudentHomeProjection.Home(state,Now).FinalizedReport);Assert.DoesNotContain("VIEW_FINALIZED_REPORT",EnrollmentProjection.Detail(state,Now).AllowedActions);
        report.Status="FINALIZED";var published=JsonSerializer.SerializeToElement(StudentHomeProjection.Home(state,Now).FinalizedReport,Wire);
        Assert.Equal(3,published.EnumerateObject().Count());Assert.Equal(22,published.GetProperty("gradeReportId").GetInt32());Assert.Equal(3,published.GetProperty("registrationId").GetInt32());Assert.Equal(21,published.GetProperty("classId").GetInt32());Assert.DoesNotContain("Unpublished",published.GetRawText());
        Assert.Contains("VIEW_FINALIZED_REPORT",EnrollmentProjection.Detail(state,Now).AllowedActions);
    }
    [Fact]
    public void BookingWindowAndFutureCancellationMembershipUseStrictStartBoundaryIncludingClosedSources()
    {
        var state=State();state.Session(5).StartAt=Now.UtcDateTime.AddMinutes(1);state.Session(5).EndAt=Now.UtcDateTime.AddHours(1);
        Assert.Equal(8,Assert.Single(EnrollmentProjection.Future(state,Now)).BookingId);Assert.Equal("CLOSED",state.Session(5).Status);
        Assert.Empty(EnrollmentProjection.Future(state,Now.AddMinutes(1)));
    }
    [Fact]
    public void ConfirmationCapabilityRejectsWrongBestAndHigherBandThreshold()
    {
        var state=State(true);Assert.True(EnrollmentProjection.CanConfirm(state));state.Band!.BestAttemptId=99;Assert.False(EnrollmentProjection.CanConfirm(state));state.Band.BestAttemptId=7;
        state.Attempt(7).Score=80m;state=new(state.Set with{Bands=[state.Set.Bands[0],new(){BandId=12,BandCode="B2",MinScore=65m}]},state.Registration);Assert.False(EnrollmentProjection.CanConfirm(state));
    }
}
