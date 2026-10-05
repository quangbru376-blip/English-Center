namespace EnglishCenter.SqlTests;

using EnglishCenter.Api.Domain.Finance;
using EnglishCenter.Api.Domain.Training;
using Microsoft.EntityFrameworkCore;
using Xunit;

public class ViewTests
{
    private static async Task<SqlResult> Read(SqlFixture f, string view) => Assert.Single(await SqlRows.ReadAsync(f, "SELECT * FROM dbo." + view));

    [Fact]
    public async Task Admissions_OneRegistrationDespiteMultipleSchedulesAndComponents_NoPrivateProfile()
    {
        await using var f = await SqlFixture.CreateAsync();
        var r = await Read(f, "vw_AdmissionsRegistrationFacts");
        Assert.Single(r.Rows); Assert.Equal(f.Registration.RegistrationId, r.Value(0, "registrationId"));
        Assert.Null(r.Value(0, "bandId")); Assert.Null(r.Value(0, "invoiceId"));
        Assert.Equal(f.Class.ClassId, r.Value(0, "classId"));
        Assert.Equal(f.Registration.RowVersion, r.Value(0, "registrationVersion"));
        Assert.DoesNotContain(r.Columns, c => c.Contains("password", StringComparison.OrdinalIgnoreCase) || c.Contains("birth", StringComparison.OrdinalIgnoreCase) || c.Contains("address", StringComparison.OrdinalIgnoreCase));
        f.Teacher.FullName = "Current name"; await f.Context.SaveChangesAsync(TestContext.Current.CancellationToken);
        Assert.Equal("Current name", (await Read(f, "vw_AdmissionsRegistrationFacts")).Value(0, "fullName"));
    }

    [Fact]
    public async Task Roster_CompletedScoreOnlyItsOwnBooking_AbsenceIsNotZero()
    {
        await using var f = await SqlFixture.CreateAsync(); await f.CompletedAttemptAsync();
        var attempt = await f.Context.EntryTestAttempts.SingleAsync(TestContext.Current.CancellationToken);
        var session = await f.Context.EntryTestSessions.SingleAsync(TestContext.Current.CancellationToken);
        await f.ExecuteAsync($"INSERT dbo.EntryTestBooking(attempt_id,session_id,status,booked_at,absent_by,absent_at,absent_operation_id) VALUES({attempt.AttemptId},{session.SessionId},'ABSENT',{SqlFixture.Instant},{f.Teacher.AccountId},{SqlFixture.Instant},{Guid.NewGuid()});");
        var r = await Read(f, "vw_EntryTestBookingFacts"); Assert.Equal(2, r.Rows.Count);
        var completed = r.Rows.Single(x => Equals(x[Array.IndexOf(r.Columns, "bookingStatus")], "COMPLETED"));
        var absent = r.Rows.Single(x => Equals(x[Array.IndexOf(r.Columns, "bookingStatus")], "ABSENT"));
        Assert.Equal(50m, completed[Array.IndexOf(r.Columns, "targetScore")]);
        Assert.Null(absent[Array.IndexOf(r.Columns, "targetScore")]); Assert.Null(absent[Array.IndexOf(r.Columns, "targetGradedAt")]);
    }

    [Fact]
    public async Task CurrentSchedules_ExcludeHistoricalAndCancelledClass_KeepCurrentNotHeld()
    {
        await using var f = await SqlFixture.CreateAsync();
        await f.ExecuteAsync($"UPDATE dbo.Schedule SET is_current=0,occurrence_state='NOT_HELD' WHERE schedule_id={f.Schedule.ScheduleId}; INSERT dbo.Schedule(class_id,session_type,obligation_key,is_current,session_date,start_time,end_time,room,occurrence_state,replaces_schedule_id) SELECT class_id,session_type,obligation_key,1,session_date,start_time,end_time,room,'NOT_HELD',schedule_id FROM dbo.Schedule WHERE schedule_id={f.Schedule.ScheduleId}; UPDATE dbo.Class SET status='CANCELLED',cancelled_by={f.Teacher.AccountId},cancelled_at={SqlFixture.Instant},cancellation_reason='fixture',cancellation_operation_id={Guid.NewGuid()} WHERE class_id={f.OtherClass.ClassId};");
        var r = await Read(f, "vw_CurrentScheduleOccurrences"); Assert.Equal(2, r.Rows.Count);
        Assert.DoesNotContain(r.Rows, x => Equals(x[Array.IndexOf(r.Columns, "scheduleId")], f.Schedule.ScheduleId));
        Assert.All(r.Rows, x => Assert.Equal(f.Class.ClassId, x[Array.IndexOf(r.Columns, "classId")]));
        Assert.Contains(r.Rows, x => Equals(x[Array.IndexOf(r.Columns, "occurrenceState")], "NOT_HELD"));
    }

    [Fact]
    public async Task Training_GrainAndPreAggregation_AvoidScheduleTimesPlacementMultiplication()
    {
        await using var f = await SqlFixture.CreateAsync(); await f.OtherAssignmentAsync();
        await f.ExecuteAsync($"UPDATE dbo.Schedule SET occurrence_state='HELD' WHERE schedule_id={f.Schedule.ScheduleId};");
        var r = await Read(f, "vw_TrainingClassFacts"); Assert.Equal(2, r.Rows.Count);
        var index = r.Rows.FindIndex(x => Equals(x[Array.IndexOf(r.Columns, "classId")], f.Class.ClassId));
        Assert.Equal(1, r.Value(index, "placedCount")); Assert.Equal(2, r.Value(index, "expectedCount"));
        Assert.Equal(1L, r.Value(index, "currentCount")); Assert.Equal(1L, r.Value(index, "heldCount"));
        Assert.Equal(1, r.Value(index, "examCurrentCount")); Assert.Equal(0, r.Value(index, "examHeldCount"));
    }

    [Theory]
    [InlineData("DRAFT", false)]
    [InlineData("SUBMITTED", false)]
    [InlineData("RETURNED", false)]
    [InlineData("FINALIZED", true)]
    public async Task FinalizedFacts_OnlyStoredFinalizedResults(string status, bool visible)
    {
        await using var f = await SqlFixture.CreateAsync();
        await f.ExecuteAsync($"UPDATE dbo.GradeReport SET status={status},submitted_by=CASE WHEN {status}<>'DRAFT' THEN {f.Teacher.AccountId} END,submitted_at=CASE WHEN {status}<>'DRAFT' THEN {SqlFixture.Instant} END,submitted_operation_id=CASE WHEN {status}<>'DRAFT' THEN {Guid.NewGuid()} END,reviewed_by=CASE WHEN {status} IN ('FINALIZED','RETURNED') THEN {f.Teacher.AccountId} END,reviewed_at=CASE WHEN {status} IN ('FINALIZED','RETURNED') THEN {SqlFixture.Instant} END,review_operation_id=CASE WHEN {status} IN ('FINALIZED','RETURNED') THEN {Guid.NewGuid()} END,returned_reason=CASE WHEN {status}='RETURNED' THEN 'fixture' END,final_score=CASE WHEN {status}='FINALIZED' THEN 50 ELSE NULL END,finalized_at=CASE WHEN {status}='FINALIZED' THEN {SqlFixture.Instant} ELSE NULL END WHERE report_id={f.Report.ReportId};");
        var r = await Read(f, "vw_FinalizedGradeFacts");
        if (!visible) Assert.Empty(r.Rows);
        else { Assert.Single(r.Rows); Assert.Equal(50m, r.Value(0, "finalScore")); Assert.Equal(true, r.Value(0, "isPassing")); }
    }

    [Fact]
    public async Task Finance_OneInvoiceWithPayment_UsesReceiptAmountAndTimestamp()
    {
        await using var f = await SqlFixture.CreateAsync();
        var invoice = new Invoice { InvoiceNumber = "HD-2026-00000001", RegistrationId = f.Registration.RegistrationId, Amount = 100m, Status = "PAID", IssuedAt = SqlFixture.Instant.AddMonths(-1), DueDate = new(2026,10,4) };
        f.Context.Add(invoice); await f.Context.SaveChangesAsync(TestContext.Current.CancellationToken);
        f.Context.Add(new Payment { InvoiceId = invoice.InvoiceId, AmountPaid = 90m, Method = "CASH", ConfirmedBy = f.Teacher.AccountId, PaidAt = SqlFixture.Instant });
        await f.Context.SaveChangesAsync(TestContext.Current.CancellationToken);
        var r = await Read(f, "vw_FinanceInvoiceFacts"); Assert.Single(r.Rows);
        Assert.Equal(100m, r.Value(0,"amount")); Assert.Equal(90m,r.Value(0,"paidAmount")); Assert.Equal(SqlFixture.Instant,r.Value(0,"paidAt"));
        Assert.DoesNotContain(r.Columns, c => c.Contains("method",StringComparison.OrdinalIgnoreCase) || c.Contains("email",StringComparison.OrdinalIgnoreCase));
    }
}
