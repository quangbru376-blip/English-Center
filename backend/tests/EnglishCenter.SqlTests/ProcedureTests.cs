namespace EnglishCenter.SqlTests;

using System.Data;
using EnglishCenter.Api.Domain.Finance;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Xunit;

public class ProcedureTests
{
    private static Task<SqlFixture> Fixture() => SqlFixture.CreateAsync(isolation: IsolationLevel.Snapshot);

    private static Dictionary<string, object?> Arguments(int sp, SqlFixture f) => sp switch
    {
        1 => new() { ["actorId"]=f.Teacher.AccountId,["courseId"]=null,["status"]=null,["createdFromUtc"]=null,["createdToExclusiveUtc"]=null,["name"]=null,["email"]=null,["phone"]=null,["sort"]="CREATED_ASC",["page"]=1,["pageSize"]=20 },
        2 => new() { ["sessionId"]=0,["actorId"]=f.Teacher.AccountId,["asOfUtc"]=SqlFixture.Instant,["name"]=null,["email"]=null,["phone"]=null,["bookingStatus"]=null,["view"]="history",["sort"]="BOOKED_ASC",["page"]=1,["pageSize"]=20 },
        3 => new() { ["classId"]=f.Class.ClassId,["actorId"]=f.Teacher.AccountId },
        4 => new() { ["fromUtc"]=SqlFixture.Instant,["toExclusiveUtc"]=SqlFixture.Instant.AddDays(1),["courseId"]=null,["overdueOnly"]=false,["todayVn"]=new DateOnly(2026,10,5),["page"]=1,["pageSize"]=20 },
        5 => new() { ["fromUtc"]=SqlFixture.Instant,["toExclusiveUtc"]=SqlFixture.Instant.AddDays(1),["courseId"]=null,["status"]=null,["page"]=1,["pageSize"]=20 },
        6 => new() { ["courseId"]=null,["bandId"]=null,["classId"]=null,["teacherId"]=null,["status"]=null,["finalizedFromUtc"]=SqlFixture.Instant,["finalizedToExclusiveUtc"]=SqlFixture.Instant.AddDays(1),["page"]=1,["pageSize"]=20 },
        7 => new() { ["issueYear"]=(short)2026 },
        _ => throw new ArgumentOutOfRangeException(nameof(sp))
    };

    private static readonly string[] Names = ["", "usp_SearchAdmissions", "usp_GetEntryTestRoster", "usp_GetClassGradeReview", "usp_GetFinancialReport", "usp_GetAdmissionsReport", "usp_GetTrainingReport", "usp_AllocateInvoiceNumber"];
    private static Task<List<SqlResult>> Call(SqlFixture f, int sp, Dictionary<string,object?>? args = null)
    {
        var values = (args ?? Arguments(sp,f)).Select((x,i) => (x.Key, Parameter:"@p"+i, x.Value)).ToArray();
        return SqlRows.ReadAsync(f,"EXEC dbo."+Names[sp]+" "+string.Join(",",values.Select(x=>"@"+x.Key+"="+x.Parameter)),values.Select(x=>(x.Parameter,x.Value)).ToArray());
    }

    private static async Task Role(SqlFixture f, string role)
    {
        await f.ExecuteAsync($"UPDATE dbo.Role SET role_code={role} WHERE role_id={f.Teacher.RoleId}; UPDATE dbo.Account SET phone='0900000000' WHERE account_id={f.Teacher.AccountId};");
    }

    private static async Task Publish(SqlFixture f, string status, decimal score=50m)
    {
        await f.ExecuteAsync($"UPDATE dbo.GradeReport SET status={status},submitted_by={f.Teacher.AccountId},submitted_at={SqlFixture.Instant},submitted_operation_id={Guid.NewGuid()},reviewed_by=CASE WHEN {status} IN ('FINALIZED','RETURNED') THEN {f.Teacher.AccountId} END,reviewed_at=CASE WHEN {status} IN ('FINALIZED','RETURNED') THEN {SqlFixture.Instant} END,review_operation_id=CASE WHEN {status} IN ('FINALIZED','RETURNED') THEN {Guid.NewGuid()} END,returned_reason=CASE WHEN {status}='RETURNED' THEN 'fixture return' END,final_score=CASE WHEN {status}='FINALIZED' THEN {score} END,finalized_at=CASE WHEN {status}='FINALIZED' THEN {SqlFixture.Instant} END WHERE report_id={f.Report.ReportId};");
    }

    [Theory]
    [InlineData(1)] [InlineData(2)] [InlineData(3)] [InlineData(4)] [InlineData(5)] [InlineData(6)]
    public async Task ReadProcedure_RejectsReadCommittedCallerWithoutChangingTransaction(int sp)
    {
        await using var f=await SqlFixture.CreateAsync();
        var exception=await Assert.ThrowsAsync<SqlException>(()=>Call(f,sp)); Assert.Equal(51104,exception.Number);
        Assert.Equal(1,await f.Context.Database.SqlQueryRaw<int>("SELECT @@TRANCOUNT AS Value").SingleAsync(TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData(1)] [InlineData(3)]
    public async Task OperationalReads_RejectWrongRole(int sp)
    {
        await using var f=await Fixture();
        var error=await Assert.ThrowsAsync<SqlException>(()=>Call(f,sp)); Assert.Equal(51101,error.Number);
    }

    [Theory]
    [InlineData(1,"Admissions")] [InlineData(2,"Teacher")] [InlineData(3,"AcademicAffairs")]
    public async Task OperationalReads_RejectRestrictedActiveActor(int sp,string role)
    {
        await using var f=await Fixture(); await Role(f,role);
        await f.ExecuteAsync($"UPDATE dbo.Account SET must_change_password=1,temp_issued_at={SqlFixture.Instant},temp_expires_at={SqlFixture.Instant.AddDays(1)} WHERE account_id={f.Teacher.AccountId};");
        var error=await Assert.ThrowsAsync<SqlException>(()=>Call(f,sp)); Assert.Equal(51101,error.Number);
    }

    [Theory]
    [InlineData("%",false)] [InlineData("_",false)] [InlineData("[",false)] [InlineData("~",false)] [InlineData("SQL",true)]
    public async Task Admissions_SearchTreatsLikeMetacharactersLiterally(string search,bool matches)
    {
        await using var f=await Fixture(); await Role(f,"Admissions");
        var args=Arguments(1,f);args["name"]=search;
        var sets=await Call(f,1,args);Assert.Equal(2,sets.Count);Assert.Equal(matches?1L:0L,sets[0].Value(0,"totalCount"));Assert.Equal(matches?1:0,sets[1].Rows.Count);
        Assert.Equal(["registrationId","studentId","fullName","email","phone","courseId","courseCode","courseName","courseStatus","registrationStatus","createdAt","admissionConfirmedAt","admissionApprovedAt","bandId","bandCode","bandMinScore","invoiceId","invoiceNumber","invoiceAmount","invoiceStatus","classId","assignmentId","classCode","classStatus","registrationVersion"],sets[1].Columns);
        if(matches) Assert.Equal("binary",sets[1].Types[^1]);
    }

    [Fact]
    public async Task Admissions_TiedDatesStableBothDirections_OutsidePageKeepsCount()
    {
        await using var f=await Fixture();await Role(f,"Admissions");
        await f.ExecuteAsync($"UPDATE dbo.Registration SET is_unfinished=0,created_at={SqlFixture.Instant} WHERE registration_id={f.Registration.RegistrationId}; INSERT dbo.Registration(student_id,course_id,status,created_at,is_unfinished) SELECT student_id,course_id,'PENDING',{SqlFixture.Instant},1 FROM dbo.Registration WHERE registration_id={f.Registration.RegistrationId};");
        var args=Arguments(1,f);args["pageSize"]=1;
        var ascending=await Call(f,1,args);Assert.Equal(2L,ascending[0].Value(0,"totalCount"));Assert.Equal(f.Registration.RegistrationId,ascending[1].Value(0,"registrationId"));
        args["sort"]="CREATED_DESC";var descending=await Call(f,1,args);Assert.NotEqual(f.Registration.RegistrationId,descending[1].Value(0,"registrationId"));
        args["page"]=int.MaxValue;Assert.Empty((await Call(f,1,args))[1].Rows);
    }

    [Theory]
    [InlineData("Teacher",false)] [InlineData("AcademicAffairs",true)]
    public async Task Roster_RoleSpecificShapeAndCurrentVersusHistory(string role,bool invoiceColumn)
    {
        await using var f=await Fixture();await Role(f,role);await f.CompletedAttemptAsync();
        var session=await f.Context.EntryTestSessions.SingleAsync(TestContext.Current.CancellationToken);
        var args=Arguments(2,f);args["sessionId"]=session.SessionId;
        var sets=await Call(f,2,args);Assert.Equal(2,sets.Count);Assert.Single(sets[1].Rows);
        Assert.Equal(invoiceColumn,sets[1].Columns.Contains("invoiceStatus"));Assert.Equal(50m,sets[1].Value(0,"targetScore"));
        args["view"]="current";sets=await Call(f,2,args);Assert.Equal(0L,sets[0].Value(0,"totalCount"));Assert.Empty(sets[1].Rows);
        args["sessionId"]=int.MaxValue;var miss=await Assert.ThrowsAsync<SqlException>(()=>Call(f,2,args));Assert.Equal(51102,miss.Number);
    }

    [Fact]
    public async Task Roster_CurrentTeacherAssignmentEnforced()
    {
        await using var f=await Fixture();await f.CompletedAttemptAsync();
        var other=await f.OtherAssignmentAsync();var otherAccount=await f.Context.Accounts.SingleAsync(x=>x.AccountId!=f.Teacher.AccountId,TestContext.Current.CancellationToken);
        var session=await f.Context.EntryTestSessions.SingleAsync(TestContext.Current.CancellationToken);await f.ExecuteAsync($"UPDATE dbo.EntryTestSession SET grader_id={otherAccount.AccountId} WHERE session_id={session.SessionId};");
        var args=Arguments(2,f);args["sessionId"]=session.SessionId;
        var error=await Assert.ThrowsAsync<SqlException>(()=>Call(f,2,args));Assert.Equal(51102,error.Number);Assert.True(other.ClassAssignmentId>0);
    }

    [Theory]
    [InlineData("SUBMITTED")] [InlineData("RETURNED")] [InlineData("FINALIZED")]
    public async Task GradeReview_AllSetsVisibleOnly_NoDraftValuesOrScoreTokens(string state)
    {
        await using var f=await Fixture();await Role(f,"AcademicAffairs");
        await f.ExecuteAsync($"INSERT dbo.Score(class_assignment_id,component_id,score_value,entered_by,entered_at) VALUES({f.Assignment.ClassAssignmentId},{f.General.ComponentId},80,{f.Teacher.AccountId},{SqlFixture.Instant}); INSERT dbo.Score(class_assignment_id,component_id,schedule_id,score_value,entered_by,entered_at) VALUES({f.Assignment.ClassAssignmentId},{f.Lesson.ComponentId},{f.Schedule.ScheduleId},60,{f.Teacher.AccountId},{SqlFixture.Instant});");
        await Publish(f,state);
        // Same class has a draft report with a secret comment and saved score.
        await f.ExecuteAsync($"UPDATE dbo.Registration SET is_unfinished=0 WHERE registration_id={f.Registration.RegistrationId}; DECLARE @r int; INSERT dbo.Registration(student_id,course_id,status,is_unfinished) SELECT student_id,course_id,'PENDING',1 FROM dbo.Registration WHERE registration_id={f.Registration.RegistrationId}; SET @r=SCOPE_IDENTITY(); INSERT dbo.StudentClassAssignment(registration_id,class_id,assigned_by,assigned_at) VALUES(@r,{f.Class.ClassId},{f.Teacher.AccountId},{SqlFixture.Instant}); DECLARE @a int=SCOPE_IDENTITY(); INSERT dbo.GradeReport(class_assignment_id,status,teacher_comment) VALUES(@a,'DRAFT','PRIVATE DRAFT'); INSERT dbo.Score(class_assignment_id,component_id,score_value,entered_by,entered_at) VALUES(@a,{f.General.ComponentId},12,{f.Teacher.AccountId},{SqlFixture.Instant});");
        var sets=await Call(f,3);Assert.Equal(5,sets.Count);
        Assert.Equal(2,sets[0].Value(0,"cohortSize"));Assert.Equal(1,sets[0].Value(0,"visibleReportCount"));Assert.Equal(2,sets[1].Rows.Count);
        Assert.Single(sets[2].Rows);Assert.Equal(state,sets[2].Value(0,"status"));Assert.Equal(70m,sets[2].Value(0,"total"));
        Assert.Equal(2,sets[3].Rows.Count);Assert.All(sets[3].Rows,r=>Assert.Equal(f.Assignment.ClassAssignmentId,r[0]));
        Assert.Single(sets[4].Rows);Assert.Equal(f.Schedule.ScheduleId,sets[4].Value(0,"scheduleId"));
        Assert.DoesNotContain(sets.SelectMany(x=>x.Rows).SelectMany(x=>x),value=>Equals(value,"PRIVATE DRAFT"));
    }

    [Fact]
    public async Task GradeReview_OnlyDraftIsTaggedMissNotEmptySuccess()
    {
        await using var f=await Fixture();await Role(f,"AcademicAffairs");
        var error=await Assert.ThrowsAsync<SqlException>(()=>Call(f,3));Assert.Equal(51102,error.Number);
    }

    [Theory]
    [InlineData(false,"100","100","30","10")] [InlineData(true,"100","100","10","10")]
    public async Task Finance_IndependentEventPeriodsAndDebtOnlyOverdueFilter(bool overdue,string revenue,string billed,string debt,string overdueDebt)
    {
        await using var f=await Fixture();
        await Invoice(f,100,"PAID",SqlFixture.Instant.AddMonths(-1),new(2026,10,4),100,SqlFixture.Instant);
        await Invoice(f,100,"UNPAID",SqlFixture.Instant,new(2026,10,5));
        await Invoice(f,10,"UNPAID",SqlFixture.Instant.AddMonths(-1),new(2026,10,4));
        await Invoice(f,20,"UNPAID",SqlFixture.Instant.AddMonths(-1),new(2026,10,6));
        var args=Arguments(4,f);args["overdueOnly"]=overdue;
        // Current in-period UNPAID100 is also current debt: expected debt130/overdue10.
        var sets=await Call(f,4,args);Assert.Equal(3,sets.Count);
        Assert.Equal(revenue,sets[0].Value(0,"revenueAmount"));Assert.Equal(billed,sets[0].Value(0,"billedAmount"));
        Assert.Equal(overdue?debt:"130",sets[0].Value(0,"currentDebtAmount"));Assert.Equal(overdueDebt,sets[0].Value(0,"overdueDebtAmount"));
        Assert.Single(sets[1].Rows);Assert.All(sets[0].Types.Skip(1),type=>Assert.Equal("varchar",type));
        args["page"]=int.MaxValue;Assert.Empty((await Call(f,4,args))[1].Rows);
        args["page"]=null;args["pageSize"]=null;Assert.Single((await Call(f,4,args))[1].Rows);
        args["page"]=1;var bad=await Assert.ThrowsAsync<SqlException>(()=>Call(f,4,args));Assert.Equal(51103,bad.Number);
    }

    private static async Task Invoice(SqlFixture f,decimal amount,string status,DateTime issue,DateOnly due,decimal? paid=null,DateTime? receipt=null)
    {
        await f.ExecuteAsync($"UPDATE dbo.Registration SET is_unfinished=0 WHERE student_id={f.Registration.StudentId}; INSERT dbo.Registration(student_id,course_id,status,is_unfinished) VALUES({f.Registration.StudentId},{f.Course.CourseId},'PENDING',1);");
        var id=await f.Context.Database.SqlQueryRaw<int>("SELECT MAX(registration_id) AS Value FROM dbo.Registration").SingleAsync(TestContext.Current.CancellationToken);
        var invoice=new Invoice {RegistrationId=id,InvoiceNumber="HD-2026-"+id.ToString("D8",System.Globalization.CultureInfo.InvariantCulture),Amount=amount,Status=status,IssuedAt=issue,DueDate=due};
        f.Context.Add(invoice);await f.Context.SaveChangesAsync(TestContext.Current.CancellationToken);
        if(paid is not null){f.Context.Add(new Payment {InvoiceId=invoice.InvoiceId,AmountPaid=paid.Value,PaidAt=receipt!.Value,ConfirmedBy=f.Teacher.AccountId,Method="CASH"});await f.Context.SaveChangesAsync(TestContext.Current.CancellationToken);}
    }

    [Fact]
    public async Task Finance_SumBeyondIndividualMoneyRangeRemainsExactDigitString()
    {
        await using var f=await Fixture();
        await Invoice(f,999999999999999999m,"UNPAID",SqlFixture.Instant,new(2026,10,4));
        await Invoice(f,999999999999999999m,"UNPAID",SqlFixture.Instant,new(2026,10,4));
        var sets=await Call(f,4);Assert.Equal("1999999999999999998",sets[0].Value(0,"billedAmount"));Assert.Equal("1999999999999999998",sets[0].Value(0,"currentDebtAmount"));
    }

    [Fact]
    public async Task AdmissionsReport_DistinctWholeStudentNotSumGroups()
    {
        await using var f=await Fixture();
        await f.ExecuteAsync($"UPDATE dbo.Registration SET created_at={SqlFixture.Instant} WHERE registration_id={f.Registration.RegistrationId}; INSERT dbo.Course(course_code,course_name,base_price,status) VALUES('ANOTHER','Other',1,'INACTIVE'); DECLARE @c int=SCOPE_IDENTITY(); INSERT dbo.Registration(student_id,course_id,status,created_at) VALUES({f.Registration.StudentId},@c,'PENDING',{SqlFixture.Instant});");
        var sets=await Call(f,5);Assert.Equal(3,sets.Count);Assert.Equal(2L,sets[0].Value(0,"totalCount"));Assert.Equal(2L,sets[0].Value(0,"registrationCount"));Assert.Equal(1L,sets[0].Value(0,"distinctStudentCount"));Assert.Equal(2,sets[1].Rows.Count);Assert.All(sets[1].Rows,r=>Assert.Equal(1L,r[^1]));
        Assert.Equal(2,sets[2].Rows.Count);
    }

    [Fact]
    public async Task Training_DateChangesOnlyResults_CancelledProgressNullAndStoredRoundedPassing()
    {
        await using var f=await Fixture();await Publish(f,"FINALIZED",50m);
        await f.ExecuteAsync($"UPDATE dbo.Schedule SET occurrence_state='HELD' WHERE schedule_id={f.Schedule.ScheduleId}; UPDATE dbo.Class SET status='CANCELLED',cancelled_by={f.Teacher.AccountId},cancelled_at={SqlFixture.Instant},cancellation_reason='fixture',cancellation_operation_id={Guid.NewGuid()} WHERE class_id={f.Class.ClassId};");
        var sets=await Call(f,6);Assert.Equal(6,sets.Count);Assert.Equal(2L,sets[0].Value(0,"classCount"));Assert.Equal(1L,sets[0].Value(0,"cancelledCount"));Assert.Equal(1L,sets[0].Value(0,"passingResultCount"));Assert.Equal(100m,sets[0].Value(0,"passRatePercent"));
        var index=sets[1].Rows.FindIndex(r=>Equals(r[0],f.Class.ClassId));
        foreach(var column in new[]{"heldCount","progressPercent","examCurrentCount","examHeldCount"}) Assert.Null(sets[1].Value(index,column));
        Assert.Equal(1L,sets[1].Value(index,"finalizedResultCount"));
        var args=Arguments(6,f);args["finalizedFromUtc"]=SqlFixture.Instant.AddDays(1);args["finalizedToExclusiveUtc"]=SqlFixture.Instant.AddDays(2);
        sets=await Call(f,6,args);Assert.Equal(2L,sets[0].Value(0,"classCount"));Assert.Equal(0L,sets[0].Value(0,"finalizedResultCount"));Assert.Null(sets[0].Value(0,"passRatePercent"));
        Assert.DoesNotContain(sets.SelectMany(s=>s.Columns),c=>c is "studentId" or "reportId" or "registrationId" or "scoreId");
    }

    [Theory]
    [InlineData(4)] [InlineData(5)] [InlineData(6)]
    public async Task Reports_EmptyGroupsStillReturnAllOptionsAndCorrectNoDataValues(int sp)
    {
        await using var f=await Fixture();var args=Arguments(sp,f);args["courseId"]=int.MaxValue;
        var sets=await Call(f,sp,args);Assert.Equal(0L,sets[0].Value(0,"totalCount"));Assert.Empty(sets[1].Rows);Assert.NotEmpty(sets[2].Rows);
        if(sp==4) Assert.All(sets[0].Rows[0].Skip(1),x=>Assert.Equal("0",x));
        if(sp==6) Assert.Null(sets[0].Value(0,"passRatePercent"));
    }

    [Fact]
    public async Task Allocator_CallerOwnedIncrementAndRollback_NoIndependentCommit()
    {
        await using(var f=await SqlFixture.CreateAsync())
        {
            var first=await Call(f,7);Assert.Equal("HD-2026-00000001",first[0].Value(0,"invoiceNumber"));Assert.Equal("varchar",first[0].Types[0]);
            var second=await Call(f,7);Assert.Equal("HD-2026-00000002",second[0].Value(0,"invoiceNumber"));
            Assert.Equal(1,await f.Context.Database.SqlQueryRaw<int>("SELECT @@TRANCOUNT AS Value").SingleAsync(TestContext.Current.CancellationToken));
        }
        await using(var f=await SqlFixture.CreateAsync()) Assert.Equal("HD-2026-00000001",(await Call(f,7))[0].Value(0,"invoiceNumber"));
    }

    [Theory]
    [InlineData(0)] [InlineData(-1)]
    public async Task Allocator_RejectsYearWithoutCounterMutation(short year)
    {
        await using var f=await SqlFixture.CreateAsync();var args=Arguments(7,f);args["issueYear"]=year;
        var error=await Assert.ThrowsAsync<SqlException>(()=>Call(f,7,args));Assert.Equal(51108,error.Number);
    }

    [Fact]
    public async Task Allocator_OverflowRejectedBeforeIncrement()
    {
        await using var f=await SqlFixture.CreateAsync();await f.ExecuteAsync($"INSERT dbo.InvoiceNumberCounter(issue_year,last_value) VALUES(2026,99999999);");
        var error=await Assert.ThrowsAsync<SqlException>(()=>Call(f,7));Assert.Equal(51108,error.Number);
    }
}
