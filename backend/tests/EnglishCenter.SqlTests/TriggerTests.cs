namespace EnglishCenter.SqlTests;

using EnglishCenter.Api.Domain.Training;
using Microsoft.EntityFrameworkCore;
using Xunit;

public class TriggerTests
{
    [Fact]
    public async Task UsedProgram_AllowsNameStatusAndNoOpConfiguration()
    {
        await using var f = await SqlFixture.CreateAsync();
        await f.ExecuteAsync($"UPDATE dbo.TrainingProgram SET program_name=N'Renamed',status='ACTIVE',expected_lesson_count=expected_lesson_count WHERE program_id={f.Program.ProgramId}");
        Assert.Equal("Renamed", await f.Context.TrainingPrograms.AsNoTracking().Where(p => p.ProgramId == f.Program.ProgramId).Select(p => p.ProgramName).SingleAsync(TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData("lessons")]
    [InlineData("band")]
    [InlineData("cancelled-class")]
    [InlineData("mixed")]
    public async Task UsedProgram_RejectsCoreChangesIncludingCancelledAndMixedBatch(string change)
    {
        await using var f = await SqlFixture.CreateAsync();
        if (change == "cancelled-class")
            await f.ExecuteAsync($"UPDATE dbo.Class SET status='CANCELLED',cancelled_by={f.Teacher.AccountId},cancelled_at={SqlFixture.Instant},cancellation_reason=N'fixture',cancellation_operation_id={Guid.NewGuid()} WHERE program_id={f.Program.ProgramId}");
        FormattableString statement = change switch
        {
            "band" => $"UPDATE dbo.TrainingProgram SET band_id={f.OtherBand.BandId} WHERE program_id={f.Program.ProgramId}",
            "mixed" => $"UPDATE dbo.TrainingProgram SET expected_lesson_count=expected_lesson_count+1 WHERE program_id IN ({f.Program.ProgramId},{f.UnusedProgram.ProgramId})",
            _ => $"UPDATE dbo.TrainingProgram SET expected_lesson_count=3 WHERE program_id={f.Program.ProgramId}"
        };
        // A different course/Band pair avoids colliding with the unused pair's unique key.
        if (change == "band")
            await f.ExecuteAsync($"DELETE dbo.TrainingProgram WHERE program_id={f.UnusedProgram.ProgramId}");
        await f.ExpectViolationAsync(51002, statement);
    }

    [Theory]
    [InlineData("insert")]
    [InlineData("delete")]
    [InlineData("weight")]
    [InlineData("name-case")]
    [InlineData("kind")]
    [InlineData("move-away")]
    [InlineData("move-in")]
    [InlineData("mixed")]
    public async Task UsedComponents_RejectListAndValueChangesOnBothSides(string change)
    {
        await using var f = await SqlFixture.CreateAsync();
        await f.ExecuteAsync($"INSERT dbo.ScoreComponentConfig(program_id,component_name,weight,kind) VALUES ({f.UnusedProgram.ProgramId},N'Unused',100,'GENERAL')");
        FormattableString statement = change switch
        {
            "insert" => $"INSERT dbo.ScoreComponentConfig(program_id,component_name,weight,kind) VALUES ({f.Program.ProgramId},N'Added',10,'GENERAL')",
            "delete" => $"DELETE dbo.ScoreComponentConfig WHERE component_id={f.General.ComponentId}",
            "name-case" => $"UPDATE dbo.ScoreComponentConfig SET component_name=N'general' WHERE component_id={f.General.ComponentId}",
            "kind" => $"UPDATE dbo.ScoreComponentConfig SET kind='LESSON' WHERE component_id={f.General.ComponentId}",
            "move-away" => $"UPDATE dbo.ScoreComponentConfig SET program_id={f.UnusedProgram.ProgramId} WHERE component_id={f.General.ComponentId}",
            "move-in" => $"UPDATE dbo.ScoreComponentConfig SET program_id={f.Program.ProgramId} WHERE program_id={f.UnusedProgram.ProgramId}",
            "mixed" => $"UPDATE dbo.ScoreComponentConfig SET weight=25 WHERE program_id IN ({f.Program.ProgramId},{f.UnusedProgram.ProgramId})",
            _ => $"UPDATE dbo.ScoreComponentConfig SET weight=25 WHERE component_id={f.General.ComponentId}"
        };
        await f.ExpectViolationAsync(51012, statement);
    }

    [Fact]
    public async Task UnusedConfiguration_AllowsScratchRenameKindSwapAndRemoval()
    {
        await using var f = await SqlFixture.CreateAsync();
        await f.ExecuteAsync($"INSERT dbo.ScoreComponentConfig(program_id,component_name,weight,kind) VALUES ({f.UnusedProgram.ProgramId},N'Exam',100,'FINAL_EXAM')");
        await f.ExecuteAsync($"UPDATE dbo.ScoreComponentConfig SET component_name=N'Scratch',kind='GENERAL' WHERE program_id={f.UnusedProgram.ProgramId}");
        await f.ExecuteAsync($"DELETE dbo.ScoreComponentConfig WHERE program_id={f.UnusedProgram.ProgramId}");
        await f.ExecuteAsync($"UPDATE dbo.ScoreComponentConfig SET weight=weight,component_name=component_name WHERE program_id={f.Program.ProgramId}");
    }

    [Theory]
    [InlineData("insert")]
    [InlineData("threshold")]
    [InlineData("code-case")]
    [InlineData("delete")]
    [InlineData("mixed")]
    public async Task CompletedResult_FreezesEntireBandSet(string change)
    {
        await using var f = await SqlFixture.CreateAsync();
        // A third unused Band can be deleted after completion without an FK masking the trigger.
        await f.ExecuteAsync($"INSERT dbo.Band(band_code,min_score) VALUES ('B2',75)");
        await f.CompletedAttemptAsync();
        FormattableString statement = change switch
        {
            "insert" => $"INSERT dbo.Band(band_code,min_score) VALUES ('B3',90)",
            "code-case" => $"UPDATE dbo.Band SET band_code='b0' WHERE band_id={f.Band.BandId}",
            "delete" => $"DELETE dbo.Band WHERE band_code='B2'",
            "mixed" => $"UPDATE dbo.Band SET min_score=CASE WHEN band_id={f.OtherBand.BandId} THEN 51 ELSE min_score END",
            _ => $"UPDATE dbo.Band SET min_score=1 WHERE band_id={f.Band.BandId}"
        };
        await f.ExpectViolationAsync(51022, statement);
    }

    [Fact]
    public async Task BandNoOpAfterCompletionAndUnusedSetEditAreAllowed()
    {
        await using var f = await SqlFixture.CreateAsync();
        await f.ExecuteAsync($"UPDATE dbo.Band SET min_score=51 WHERE band_id={f.OtherBand.BandId}");
        await f.CompletedAttemptAsync();
        await f.ExecuteAsync($"UPDATE dbo.Band SET min_score=min_score,band_code=band_code");
    }

    [Theory]
    [InlineData("wrong-class")]
    [InlineData("exam")]
    [InlineData("not-held")]
    [InlineData("completed")]
    [InlineData("cancelled")]
    [InlineData("mixed")]
    public async Task AttendanceRejectsInvalidScheduleOrLockedClass(string change)
    {
        await using var f = await SqlFixture.CreateAsync();
        if (change == "not-held")
            await f.ExecuteAsync($"UPDATE dbo.Schedule SET is_current=0,occurrence_state='NOT_HELD',not_held_by={f.Teacher.AccountId},not_held_at={SqlFixture.Instant},not_held_reason=N'fixture',replacement_operation_id={Guid.NewGuid()} WHERE schedule_id={f.Schedule.ScheduleId}");
        if (change == "completed") await f.ExecuteAsync($"UPDATE dbo.Class SET status='COMPLETED' WHERE class_id={f.Class.ClassId}");
        if (change == "cancelled") await f.ExecuteAsync($"UPDATE dbo.Class SET status='CANCELLED',cancelled_by={f.Teacher.AccountId},cancelled_at={SqlFixture.Instant},cancellation_reason=N'fixture',cancellation_operation_id={Guid.NewGuid()} WHERE class_id={f.Class.ClassId}");
        var scheduleId = change switch { "wrong-class" => f.OtherSchedule.ScheduleId, "exam" => f.Exam.ScheduleId, _ => f.Schedule.ScheduleId };
        var statement = change == "mixed"
            ? $"INSERT dbo.Attendance(class_assignment_id,schedule_id,present,entered_by,entered_at) VALUES ({f.Assignment.ClassAssignmentId},{f.Schedule.ScheduleId},1,{f.Teacher.AccountId},{SqlFixture.Instant}),({f.Assignment.ClassAssignmentId},{f.Exam.ScheduleId},1,{f.Teacher.AccountId},{SqlFixture.Instant})"
            : (FormattableString)$"INSERT dbo.Attendance(class_assignment_id,schedule_id,present,entered_by,entered_at) VALUES ({f.Assignment.ClassAssignmentId},{scheduleId},1,{f.Teacher.AccountId},{SqlFixture.Instant})";
        await f.ExpectViolationAsync(51032, statement);
    }

    [Theory]
    [InlineData("delete")]
    [InlineData("update")]
    [InlineData("move")]
    public async Task AttendanceOldCompletedClassCannotBeEscaped(string change)
    {
        await using var f = await SqlFixture.CreateAsync();
        await InsertAttendanceAsync(f);
        var other = await f.OtherAssignmentAsync();
        await f.ExecuteAsync($"UPDATE dbo.Class SET status='COMPLETED' WHERE class_id={f.Class.ClassId}");
        await f.ExpectViolationAsync(51032, change == "delete" ? $"DELETE dbo.Attendance" : change == "move" ? $"UPDATE dbo.Attendance SET class_assignment_id={other.ClassAssignmentId},schedule_id={f.OtherSchedule.ScheduleId}" : (FormattableString)$"UPDATE dbo.Attendance SET present=0");
    }

    [Theory]
    [InlineData("general-linked")]
    [InlineData("lesson-null")]
    [InlineData("wrong-class")]
    [InlineData("wrong-type")]
    [InlineData("not-held")]
    [InlineData("wrong-program")]
    [InlineData("submitted")]
    [InlineData("finalized")]
    public async Task ScoreRejectsWrongRelationsOrLockedReport(string change)
    {
        await using var f = await SqlFixture.CreateAsync();
        int component = f.Lesson.ComponentId; int? schedule = f.Schedule.ScheduleId;
        if (change == "general-linked") component = f.General.ComponentId;
        if (change == "lesson-null") schedule = null;
        if (change == "wrong-class") schedule = f.OtherSchedule.ScheduleId;
        if (change == "wrong-type") schedule = f.Exam.ScheduleId;
        if (change == "not-held") await f.ExecuteAsync($"UPDATE dbo.Schedule SET is_current=0,occurrence_state='NOT_HELD',not_held_by={f.Teacher.AccountId},not_held_at={SqlFixture.Instant},not_held_reason=N'fixture',replacement_operation_id={Guid.NewGuid()} WHERE schedule_id={f.Schedule.ScheduleId}");
        if (change == "wrong-program")
        {
            await f.ExecuteAsync($"INSERT dbo.ScoreComponentConfig(program_id,component_name,weight,kind) VALUES ({f.UnusedProgram.ProgramId},N'Other',100,'GENERAL')");
            component = await f.Context.ScoreComponentConfigs.AsNoTracking().Where(c => c.ProgramId == f.UnusedProgram.ProgramId).Select(c => c.ComponentId).SingleAsync(TestContext.Current.CancellationToken); schedule = null;
        }
        if (change is "submitted" or "finalized") await LockReportAsync(f, change == "finalized");
        await f.ExpectViolationAsync(51042, $"INSERT dbo.Score(class_assignment_id,component_id,schedule_id,score_value,entered_by,entered_at) VALUES ({f.Assignment.ClassAssignmentId},{component},{schedule},50,{f.Teacher.AccountId},{SqlFixture.Instant})");
    }

    [Theory]
    [InlineData("delete")]
    [InlineData("update")]
    [InlineData("move")]
    public async Task SubmittedOldReportBlocksScoreDeleteAndUpdate(string change)
    {
        await using var f = await SqlFixture.CreateAsync();
        await InsertScoreAsync(f); var other = await f.OtherAssignmentAsync(); await LockReportAsync(f, false);
        await f.ExpectViolationAsync(51042, change == "delete" ? $"DELETE dbo.Score" : change == "move" ? $"UPDATE dbo.Score SET class_assignment_id={other.ClassAssignmentId}" : (FormattableString)$"UPDATE dbo.Score SET score_value=51");
    }

    [Theory]
    [InlineData("program",51001)]
    [InlineData("component",51011)]
    [InlineData("band",51021)]
    [InlineData("attendance",51031)]
    [InlineData("score",51041)]
    public async Task MissingPreheldGateFailsClosed(string kind, int error)
    {
        await using var f = await SqlFixture.CreateAsync();
        await f.ReleaseAsync(kind == "band" ? "EC:BANDSET" : kind is "attendance" or "score" ? $"EC:CLASS:{f.Class.ClassId}" : $"EC:PROGRAM:{f.Program.ProgramId}");
        FormattableString sql = kind switch
        {
            "program" => $"UPDATE dbo.TrainingProgram SET program_name=N'Changed' WHERE program_id={f.Program.ProgramId}",
            "component" => $"UPDATE dbo.ScoreComponentConfig SET weight=weight WHERE component_id={f.General.ComponentId}",
            "band" => $"UPDATE dbo.Band SET min_score=min_score WHERE band_id={f.Band.BandId}",
            "attendance" => $"INSERT dbo.Attendance(class_assignment_id,schedule_id,present,entered_by,entered_at) VALUES ({f.Assignment.ClassAssignmentId},{f.Schedule.ScheduleId},1,{f.Teacher.AccountId},{SqlFixture.Instant})",
            _ => $"INSERT dbo.Score(class_assignment_id,component_id,schedule_id,score_value,entered_by,entered_at) VALUES ({f.Assignment.ClassAssignmentId},{f.General.ComponentId},NULL,50,{f.Teacher.AccountId},{SqlFixture.Instant})"
        };
        await f.ExpectViolationAsync(error, sql);
    }

    [Fact]
    public async Task ValidEfWritesReturnIdsAndVersionsAndCompletedDraftScoresStayEditable()
    {
        await using var f = await SqlFixture.CreateAsync();
        Assert.True(f.Band.BandId > 0); Assert.Equal(8,f.Band.RowVersion.Length);
        Assert.True(f.Program.ProgramId > 0); Assert.Equal(8,f.Program.RowVersion.Length);
        Assert.True(f.General.ComponentId > 0); Assert.Equal(8,f.General.RowVersion.Length);
        var attendance = new Attendance { ClassAssignmentId=f.Assignment.ClassAssignmentId,ScheduleId=f.Schedule.ScheduleId,Present=true,EnteredBy=f.Teacher.AccountId,EnteredAt=SqlFixture.Instant };
        var score = new Score { ClassAssignmentId=f.Assignment.ClassAssignmentId,ComponentId=f.General.ComponentId,ScoreValue=50,EnteredBy=f.Teacher.AccountId,EnteredAt=SqlFixture.Instant };
        f.Context.AddRange(attendance,score); await f.Context.SaveChangesAsync(TestContext.Current.CancellationToken);
        Assert.True(attendance.AttendanceId > 0); Assert.Equal(8,attendance.RowVersion.Length);
        Assert.True(score.ScoreId > 0); Assert.Equal(8,score.RowVersion.Length);
        var before=score.RowVersion.ToArray();
        await f.ExecuteAsync($"UPDATE dbo.Class SET status='COMPLETED' WHERE class_id={f.Class.ClassId}");
        score.ScoreValue=51; await f.Context.SaveChangesAsync(TestContext.Current.CancellationToken);
        Assert.False(before.SequenceEqual(score.RowVersion));
    }

    [Fact]
    public async Task ScoreMixedBatchRollsBackValidGeneralAndInvalidLinkedRow()
    {
        await using var f=await SqlFixture.CreateAsync();
        await f.ExpectViolationAsync(51042,$"INSERT dbo.Score(class_assignment_id,component_id,schedule_id,score_value,entered_by,entered_at) VALUES ({f.Assignment.ClassAssignmentId},{f.General.ComponentId},NULL,50,{f.Teacher.AccountId},{SqlFixture.Instant}),({f.Assignment.ClassAssignmentId},{f.Lesson.ComponentId},NULL,50,{f.Teacher.AccountId},{SqlFixture.Instant})");
    }

    [Theory]
    [InlineData("band")]
    [InlineData("program")]
    [InlineData("class")]
    public async Task SharedExclusiveGateSerializesTwoConnections(string kind)
    {
        await using var f=await SqlFixture.CreateAsync();
        var resource=kind switch {"band"=>"EC:BANDSET","program"=>$"EC:PROGRAM:{f.Program.ProgramId}",_=>$"EC:CLASS:{f.Class.ClassId}"};
        await using var connection=new Microsoft.Data.SqlClient.SqlConnection(f.Context.Database.GetConnectionString());
        await connection.OpenAsync(TestContext.Current.CancellationToken);
        await using var transaction=(Microsoft.Data.SqlClient.SqlTransaction)await connection.BeginTransactionAsync(TestContext.Current.CancellationToken);
        await using var command=connection.CreateCommand(); command.Transaction=transaction;
        command.CommandText="DECLARE @result int; EXEC @result=sys.sp_getapplock @Resource=@resource,@LockMode='Exclusive',@LockOwner='Transaction',@DbPrincipal='public',@LockTimeout=100; SELECT @result;";
        command.Parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@resource",System.Data.SqlDbType.NVarChar,255){Value=resource});
        Assert.Equal(-1,Convert.ToInt32(await command.ExecuteScalarAsync(TestContext.Current.CancellationToken)));
        await f.ReleaseAsync(resource);
        Assert.True(Convert.ToInt32(await command.ExecuteScalarAsync(TestContext.Current.CancellationToken))>=0);
    }

    internal static async Task InsertScoreAsync(SqlFixture f) => await f.ExecuteAsync($"INSERT dbo.Score(class_assignment_id,component_id,schedule_id,score_value,entered_by,entered_at) VALUES ({f.Assignment.ClassAssignmentId},{f.General.ComponentId},NULL,50,{f.Teacher.AccountId},{SqlFixture.Instant})");
    private static async Task InsertAttendanceAsync(SqlFixture f) => await f.ExecuteAsync($"INSERT dbo.Attendance(class_assignment_id,schedule_id,present,entered_by,entered_at) VALUES ({f.Assignment.ClassAssignmentId},{f.Schedule.ScheduleId},1,{f.Teacher.AccountId},{SqlFixture.Instant})");
    internal static async Task LockReportAsync(SqlFixture f, bool finalized)
    {
        await f.ExecuteAsync($"UPDATE dbo.GradeReport SET status='SUBMITTED',submitted_by={f.Teacher.AccountId},submitted_at={SqlFixture.Instant},submitted_operation_id={Guid.NewGuid()} WHERE report_id={f.Report.ReportId}");
        if (finalized) await f.ExecuteAsync($"UPDATE dbo.GradeReport SET status='FINALIZED',final_score=49,reviewed_by={f.Teacher.AccountId},reviewed_at={SqlFixture.Instant},review_operation_id={Guid.NewGuid()},finalized_at={SqlFixture.Instant} WHERE report_id={f.Report.ReportId}");
    }
}
