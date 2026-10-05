namespace EnglishCenter.SqlTests;

using System.Globalization;
using EnglishCenter.Api.Domain.Training;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Xunit;

public class FunctionTests
{
    public sealed class ProgressResult
    {
        public int ExpectedLessonCount { get; set; }
        public long CurrentLessonCount { get; set; }
        public long HeldLessonCount { get; set; }
        public int CurrentFinalExamCount { get; set; }
        public int HeldFinalExamCount { get; set; }
    }
    public sealed class GradeResult
    {
        public int RequiredComponentCount { get; set; }
        public int EnteredComponentCount { get; set; }
        public bool IsComplete { get; set; }
        public decimal? Total { get; set; }
    }
    public sealed class DebtResult
    {
        public bool IsOverdue { get; set; }
        public int DaysOverdue { get; set; }
    }
    public sealed class BandResult
    {
        public int BandId { get; set; }
        public decimal MinScore { get; set; }
    }
    private static decimal Number(string value) => decimal.Parse(value, CultureInfo.InvariantCulture);

    [Theory]
    [InlineData("OPEN",-1,"OPEN")]
    [InlineData("OPEN",0,"CLOSED")]
    [InlineData("OPEN",1,"CLOSED")]
    [InlineData("CLOSED",-1,"CLOSED")]
    [InlineData("CLOSED",0,"CLOSED")]
    [InlineData("CANCELLED",-1,"CANCELLED")]
    [InlineData("CANCELLED",0,"CANCELLED")]
    [InlineData("CANCELLED",1,"CANCELLED")]
    public async Task EffectiveState_UsesPassedInstantAndCancellationPrecedence(string status,int milliseconds,string expected)
    {
        await using var f=await SqlFixture.CreateAsync();
        var result=await f.Context.Database.SqlQuery<string>($"SELECT dbo.ufn_EffectiveEntryTestState({status},{SqlFixture.Instant},{SqlFixture.Instant.AddMilliseconds(milliseconds)}) AS Value").SingleAsync(TestContext.Current.CancellationToken);
        Assert.Equal(expected,result);
    }

    [Theory]
    [InlineData("0",0)]
    [InlineData("49.99",0)]
    [InlineData("50",50)]
    [InlineData("100",50)]
    [InlineData("-0.01",-1)]
    [InlineData("100.01",-1)]
    public async Task BandLookup_UsesGreatestActualThresholdWithRangeEdges(string score,int threshold)
    {
        await using var f=await SqlFixture.CreateAsync();
        var result=await f.Context.Database.SqlQuery<BandResult>($"SELECT bandId,minScore FROM dbo.ufn_BandForScore({Number(score)})").ToListAsync(TestContext.Current.CancellationToken);
        if(threshold<0){Assert.Empty(result);return;}
        var row=Assert.Single(result);Assert.Equal(threshold,row.MinScore);Assert.Equal(threshold==0?f.Band.BandId:f.OtherBand.BandId,row.BandId);
    }

    [Fact]
    public async Task BandLookup_MissingThresholdReturnsNoGuessedBand()
    {
        await using var f=await SqlFixture.CreateAsync();
        await f.ExecuteAsync($"UPDATE dbo.Band SET min_score=1 WHERE band_id={f.Band.BandId}");
        Assert.Empty(await f.Context.Database.SqlQuery<BandResult>($"SELECT bandId,minScore FROM dbo.ufn_BandForScore({0m})").ToListAsync(TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData("PENDING",true)]
    [InlineData("TESTING",true)]
    [InlineData("BAND_ASSIGNED",true)]
    [InlineData("CONFIRMED",true)]
    [InlineData("ADMITTED",true)]
    [InlineData("CANCELLED",false)]
    [InlineData("REJECTED",false)]
    public async Task Unfinished_IsDerivedFromStatusRatherThanStoredFlag(string status,bool expected)
    {
        await using var f=await SqlFixture.CreateAsync();
        if(status=="CANCELLED") await f.ExecuteAsync($"UPDATE dbo.Registration SET status='CANCELLED',cancelled_by={f.Teacher.AccountId},cancelled_at={SqlFixture.Instant},cancelled_from_status='PENDING',cancellation_operation_id={Guid.NewGuid()} WHERE registration_id={f.Registration.RegistrationId}");
        else if(status=="REJECTED") await f.ExecuteAsync($"UPDATE dbo.Registration SET status='REJECTED',rejected_by={f.Teacher.AccountId},rejected_at={SqlFixture.Instant},rejection_reason=N'fixture',rejected_from_status='CONFIRMED',rejection_operation_id={Guid.NewGuid()} WHERE registration_id={f.Registration.RegistrationId}");
        else await f.ExecuteAsync($"UPDATE dbo.Registration SET status={status},is_unfinished=0 WHERE registration_id={f.Registration.RegistrationId}");
        var result=await f.Context.Database.SqlQuery<bool>($"SELECT isUnfinished AS Value FROM dbo.ufn_RegistrationUnfinished({f.Registration.RegistrationId})").SingleAsync(TestContext.Current.CancellationToken);
        Assert.Equal(expected,result);
    }

    [Theory]
    [InlineData(false,true)]
    [InlineData(true,false)]
    public async Task Unfinished_RequiresBothCompletedClassAndOwnFinalizedReportRegardlessOfPassing(bool completed,bool expected)
    {
        await using var f=await SqlFixture.CreateAsync();
        await TriggerTests.LockReportAsync(f,true); // FinalScore49 is failing, still finished.
        if(completed) await f.ExecuteAsync($"UPDATE dbo.Class SET status='COMPLETED' WHERE class_id={f.Class.ClassId}");
        Assert.Equal(expected,await f.Context.Database.SqlQuery<bool>($"SELECT isUnfinished AS Value FROM dbo.ufn_RegistrationUnfinished({f.Registration.RegistrationId})").SingleAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Unfinished_MissingRegistrationIsZeroRowsAndCompletedDraftStaysUnfinished()
    {
        await using var f=await SqlFixture.CreateAsync();
        Assert.Empty(await f.Context.Database.SqlQuery<bool>($"SELECT isUnfinished AS Value FROM dbo.ufn_RegistrationUnfinished({-1})").ToListAsync(TestContext.Current.CancellationToken));
        await f.ExecuteAsync($"UPDATE dbo.Class SET status='COMPLETED' WHERE class_id={f.Class.ClassId}");
        Assert.True(await f.Context.Database.SqlQuery<bool>($"SELECT isUnfinished AS Value FROM dbo.ufn_RegistrationUnfinished({f.Registration.RegistrationId})").SingleAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Progress_MissingIsZeroRowsButRetainedEmptyClassHasZeroCounts()
    {
        await using var f=await SqlFixture.CreateAsync();
        Assert.Empty(await f.Context.Database.SqlQuery<ProgressResult>($"SELECT * FROM dbo.ufn_ClassObligationProgress({-1})").ToListAsync(TestContext.Current.CancellationToken));
        await f.ExecuteAsync($"DELETE dbo.Schedule WHERE class_id={f.OtherClass.ClassId}");
        var row=await ProgressAsync(f,f.OtherClass.ClassId);Assert.Equal(2,row.ExpectedLessonCount);Assert.Equal(0,row.CurrentLessonCount);Assert.Equal(0,row.HeldLessonCount);Assert.Equal(0,row.CurrentFinalExamCount);Assert.Equal(0,row.HeldFinalExamCount);
    }

    [Fact]
    public async Task Progress_CountsReplacementOnceAndKeepsCancelledHistory()
    {
        await using var f=await SqlFixture.CreateAsync();
        await f.ExecuteAsync($"UPDATE dbo.Schedule SET is_current=0,occurrence_state='NOT_HELD',not_held_by={f.Teacher.AccountId},not_held_at={SqlFixture.Instant},not_held_reason=N'fixture',replacement_operation_id={Guid.NewGuid()} WHERE schedule_id={f.Schedule.ScheduleId}");
        var replacement=new Schedule { ClassId=f.Class.ClassId,SessionType="LESSON",ObligationKey=f.Schedule.ObligationKey,ReplacesScheduleId=f.Schedule.ScheduleId,IsCurrent=true,SessionDate=new DateOnly(2026,10,8),StartTime=new TimeOnly(8,0),EndTime=new TimeOnly(9,0),Room="R1",OccurrenceState="HELD",HeldBy=f.Teacher.AccountId,HeldAt=SqlFixture.Instant };
        f.Context.Add(replacement);await f.Context.SaveChangesAsync(TestContext.Current.CancellationToken);
        await f.ExecuteAsync($"UPDATE dbo.Schedule SET occurrence_state='HELD',held_by={f.Teacher.AccountId},held_at={SqlFixture.Instant} WHERE schedule_id={f.Exam.ScheduleId}");
        await f.ExecuteAsync($"UPDATE dbo.Class SET status='CANCELLED',cancelled_by={f.Teacher.AccountId},cancelled_at={SqlFixture.Instant},cancellation_reason=N'fixture',cancellation_operation_id={Guid.NewGuid()} WHERE class_id={f.Class.ClassId}");
        var row=await ProgressAsync(f,f.Class.ClassId);Assert.Equal(2,row.ExpectedLessonCount);Assert.Equal(1,row.CurrentLessonCount);Assert.Equal(1,row.HeldLessonCount);Assert.Equal(1,row.CurrentFinalExamCount);Assert.Equal(1,row.HeldFinalExamCount);
    }

    [Theory]
    [InlineData("0","100","50","50","50.00")]
    [InlineData("49.99","50","50","50","50.00")]
    [InlineData("49.99","50","60","40","49.99")]
    [InlineData("100","100","33.33","66.67","100.00")]
    [InlineData("0","0","50","50","0.00")]
    [InlineData("1.01","99.99","25","75","75.25")]
    public async Task GradeTotal_RoundsOnceHalfUpWithIndependentDecimalParity(string first,string second,string firstWeight,string secondWeight,string expected)
    {
        await using var f=await SqlFixture.CreateAsync(Number(firstWeight),Number(secondWeight));
        await InsertScoresAsync(f,Number(first),Number(second));
        var row=await GradeAsync(f,f.Assignment.ClassAssignmentId);Assert.Equal(2,row.RequiredComponentCount);Assert.Equal(2,row.EnteredComponentCount);Assert.True(row.IsComplete);Assert.Equal(Number(expected),row.Total);
        Assert.Equal(decimal.Round((Number(first)*Number(firstWeight)+Number(second)*Number(secondWeight))/100m,2,MidpointRounding.AwayFromZero),row.Total);
        Assert.Null(await f.Context.GradeReports.AsNoTracking().Where(r=>r.ReportId==f.Report.ReportId).Select(r=>r.FinalScore).SingleAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task GradeTotal_MissingIncompleteAndEmptyAreDistinctAndOtherAssignmentDoesNotCount()
    {
        await using var f=await SqlFixture.CreateAsync();
        Assert.Empty(await f.Context.Database.SqlQuery<GradeResult>($"SELECT * FROM dbo.ufn_GradeTotal({-1})").ToListAsync(TestContext.Current.CancellationToken));
        var row=await GradeAsync(f,f.Assignment.ClassAssignmentId);Assert.Equal(2,row.RequiredComponentCount);Assert.Equal(0,row.EnteredComponentCount);Assert.False(row.IsComplete);Assert.Null(row.Total);
        var other=await f.OtherAssignmentAsync();
        await f.ExecuteAsync($"INSERT dbo.Score(class_assignment_id,component_id,score_value,entered_by,entered_at) VALUES ({other.ClassAssignmentId},{f.General.ComponentId},100,{f.Teacher.AccountId},{SqlFixture.Instant})");
        await TriggerTests.InsertScoreAsync(f);
        row=await GradeAsync(f,f.Assignment.ClassAssignmentId);Assert.Equal(1,row.EnteredComponentCount);Assert.False(row.IsComplete);Assert.Null(row.Total);
        await f.ExecuteAsync($"DELETE dbo.Score WHERE class_assignment_id={other.ClassAssignmentId}");
        await f.ExecuteAsync($"UPDATE dbo.Class SET program_id={f.UnusedProgram.ProgramId} WHERE class_id={f.OtherClass.ClassId}");
        row=await GradeAsync(f,other.ClassAssignmentId);Assert.Equal(0,row.RequiredComponentCount);Assert.Equal(0,row.EnteredComponentCount);Assert.False(row.IsComplete);Assert.Null(row.Total);
    }

    [Fact]
    public async Task GradeTotal_TwoComponentsSharingScheduleCountExactlyOnce()
    {
        await using var f=await SqlFixture.CreateAsync();
        await f.ExecuteAsync($"INSERT dbo.ScoreComponentConfig(program_id,component_name,weight,kind) VALUES ({f.UnusedProgram.ProgramId},N'A',50,'LESSON'),({f.UnusedProgram.ProgramId},N'B',50,'LESSON')");
        await f.ExecuteAsync($"UPDATE dbo.Class SET program_id={f.UnusedProgram.ProgramId} WHERE class_id={f.OtherClass.ClassId}");
        var other=await f.OtherAssignmentAsync();
        var ids=await f.Context.ScoreComponentConfigs.AsNoTracking().Where(c=>c.ProgramId==f.UnusedProgram.ProgramId).OrderBy(c=>c.ComponentId).Select(c=>c.ComponentId).ToArrayAsync(TestContext.Current.CancellationToken);
        await f.ExecuteAsync($"INSERT dbo.Score(class_assignment_id,component_id,schedule_id,score_value,entered_by,entered_at) VALUES ({other.ClassAssignmentId},{ids[0]},{f.OtherSchedule.ScheduleId},49.99,{f.Teacher.AccountId},{SqlFixture.Instant}),({other.ClassAssignmentId},{ids[1]},{f.OtherSchedule.ScheduleId},50,{f.Teacher.AccountId},{SqlFixture.Instant})");
        var row=await GradeAsync(f,other.ClassAssignmentId);Assert.Equal(2,row.RequiredComponentCount);Assert.Equal(2,row.EnteredComponentCount);Assert.True(row.IsComplete);Assert.Equal(50m,row.Total);
    }

    [Theory]
    [InlineData("UNPAID",-1,true,1)]
    [InlineData("UNPAID",0,false,0)]
    [InlineData("UNPAID",1,false,0)]
    [InlineData("PAID",-1,false,0)]
    [InlineData("CANCELLED",-1,false,0)]
    public async Task DebtFlags_OnlyUnpaidStrictlyBeforePassedVietnameseDate(string status,int dayOffset,bool overdue,int days)
    {
        await using var f=await SqlFixture.CreateAsync();var today=new DateOnly(2026,10,5);
        var row=await f.Context.Database.SqlQuery<DebtResult>($"SELECT * FROM dbo.ufn_InvoiceDebtFlags({status},{today.AddDays(dayOffset)},{today})").SingleAsync(TestContext.Current.CancellationToken);
        Assert.Equal(overdue,row.IsOverdue);Assert.Equal(days,row.DaysOverdue);
    }

    [Theory]
    [InlineData("SELECT * FROM dbo.ufn_BandForScore(50)","bandId,minScore")]
    [InlineData("SELECT * FROM dbo.ufn_RegistrationUnfinished(-1)","isUnfinished")]
    [InlineData("SELECT * FROM dbo.ufn_ClassObligationProgress(-1)","expectedLessonCount,currentLessonCount,heldLessonCount,currentFinalExamCount,heldFinalExamCount")]
    [InlineData("SELECT * FROM dbo.ufn_GradeTotal(-1)","requiredComponentCount,enteredComponentCount,isComplete,total")]
    [InlineData("SELECT * FROM dbo.ufn_InvoiceDebtFlags('UNPAID','2026-10-01','2026-10-05')","isOverdue,daysOverdue")]
    public async Task FunctionResultSchemasHaveExactAliasesAndTypes(string query,string names)
    {
        await using var f=await SqlFixture.CreateAsync();
        await using var command=f.Context.Database.GetDbConnection().CreateCommand();command.Transaction=f.Context.Database.CurrentTransaction!.GetDbTransaction();command.CommandText=query;
        await using var reader=await command.ExecuteReaderAsync(TestContext.Current.CancellationToken);
        var schema=await reader.GetColumnSchemaAsync(TestContext.Current.CancellationToken);
        Assert.Equal(names.Split(','),schema.Select(c=>c.ColumnName).ToArray());
        foreach(var column in schema)
        {
            if(column.ColumnName is "minScore" or "total") {Assert.Equal("decimal",column.DataTypeName);Assert.Equal(5,column.NumericPrecision);Assert.Equal(2,column.NumericScale);}
            else if(column.ColumnName is "isComplete" or "isUnfinished" or "isOverdue") Assert.Equal(typeof(bool),column.DataType);
            else if(column.ColumnName is "currentLessonCount" or "heldLessonCount") Assert.Equal(typeof(long),column.DataType);
            else Assert.Equal(typeof(int),column.DataType);
        }
    }

    private static async Task<ProgressResult> ProgressAsync(SqlFixture f,int id)=>await f.Context.Database.SqlQuery<ProgressResult>($"SELECT * FROM dbo.ufn_ClassObligationProgress({id})").SingleAsync(TestContext.Current.CancellationToken);
    private static async Task<GradeResult> GradeAsync(SqlFixture f,int id)=>await f.Context.Database.SqlQuery<GradeResult>($"SELECT * FROM dbo.ufn_GradeTotal({id})").SingleAsync(TestContext.Current.CancellationToken);
    private static async Task InsertScoresAsync(SqlFixture f,decimal general,decimal lesson)=>await f.ExecuteAsync($"INSERT dbo.Score(class_assignment_id,component_id,schedule_id,score_value,entered_by,entered_at) VALUES ({f.Assignment.ClassAssignmentId},{f.General.ComponentId},NULL,{general},{f.Teacher.AccountId},{SqlFixture.Instant}),({f.Assignment.ClassAssignmentId},{f.Lesson.ComponentId},{f.Schedule.ScheduleId},{lesson},{f.Teacher.AccountId},{SqlFixture.Instant})");
}
