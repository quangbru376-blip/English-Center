namespace EnglishCenter.Tests.Api.Integration;

using System.Text.Json;
using System.Text.Json.Nodes;
using System.Data.Common;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Common.Security;
using EnglishCenter.Api.Domain.Admissions;
using EnglishCenter.Api.Domain.Finance;
using EnglishCenter.Api.Infrastructure.Admissions;
using EnglishCenter.Api.Infrastructure.Enrollment;
using EnglishCenter.Api.Infrastructure.StudentInvoices;
using EnglishCenter.Api.Services.Admissions;
using EnglishCenter.Api.Services.Enrollment;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Xunit;

[Collection("V01 SQL")]
public sealed class M1AdmissionsSqlTests
{
    private const string OptIn = "Owned M1 Admissions scratch SQL; ID/GUID paired cleanup, no operational data.";
    private static CancellationToken Ct => OwnedSqlFixture.Ct;
    private static readonly JsonSerializerOptions Wire = new(JsonSerializerDefaults.Web);
    internal sealed class Clock(DateTimeOffset? now = null) : TimeProvider
    { public DateTimeOffset Now { get; set; } = now ?? new(2026, 10, 10, 3, 0, 0, TimeSpan.Zero); public override DateTimeOffset GetUtcNow() => Now; }
    private static SqlAdmissionsExecutor Executor(OwnedSqlFixture f) => new(() => f.Db(), f.Clock, 10, 1000);
    private static async Task<RegistrationState> Read(OwnedSqlFixture f, int id)
    {
        await using var db = f.Db(); var student = await db.Registrations.Where(r => r.RegistrationId == id).Select(r => r.StudentId).SingleAsync(Ct);
        var set = await SqlEnrollmentRepository.ReadSet(db, student, [id], Ct); return new(set, set.Registrations.Single());
    }
    internal static async Task<int> Confirmed(OwnedSqlFixture f)
    {
        var seed = await M1EnrollmentFoundationSqlTests.Seed(f); var s = await Read(f, seed.Registration);
        var grade = JsonSerializer.SerializeToNode(TeacherTaskProjection.Manifest(s, s.Booking(seed.Booking), 70), Wire)!.AsObject(); grade["score"] = "0";
        var teacher = f.Token(await f.Grant("Teacher"));
        await new SqlTeacherEntryExecutor(() => f.Db(), f.Clock, 10, 1000).CommandAsync(teacher, 70, seed.Booking, Guid.NewGuid(), grade.ToJsonString(), 65536, Ct);
        s = await Read(f, seed.Registration); var student = f.Token(await f.Grant("Student"));
        await new SqlStudentEnrollmentExecutor(() => f.Db(), f.Clock, 10, 1000).CommandAsync(student, 36, seed.Registration, 0, Guid.NewGuid(), JsonSerializer.Serialize(EnrollmentProjection.ConfirmationManifest(s), Wire), 65536, Ct);
        return seed.Registration;
    }
    private static string Approval(RegistrationState s) => JsonSerializer.Serialize(AdmissionsProjection.ApprovalManifest(s), Wire);
    private static string Reject(RegistrationState s) => JsonSerializer.Serialize(new { reason = "  Không tiếp tục\n theo hồ sơ.  ", expectedVersions = new { registration = EnrollmentProjection.Rv(s.Registration.RowVersion) } });
    private static string Cancel(RegistrationState s, DateTimeOffset now)
    { var body = JsonSerializer.SerializeToNode(EnrollmentProjection.CancellationManifest(s, now), Wire)!.AsObject(); body["reason"] = "Hủy theo yêu cầu hợp lệ."; return body.ToJsonString(); }
    private sealed class AfterReplay(Clock time,DateTimeOffset target):DbCommandInterceptor
    {
        private bool fired;
        public override ValueTask<DbDataReader> ReaderExecutedAsync(DbCommand command,CommandExecutedEventData data,DbDataReader reader,CancellationToken ct=default)
        {if(!fired&&command.CommandText.Contains("INSERT",StringComparison.Ordinal)&&command.CommandText.Contains("[ReplayOperation]",StringComparison.Ordinal)){time.Now=target;fired=true;}return ValueTask.FromResult(reader);}
    }
    private static async Task<int> Future(OwnedSqlFixture f,int id)
    {
        await using var db=f.Db();var session=new EntryTestSession{StartAt=f.Clock.GetUtcNow().UtcDateTime.AddMinutes(1),EndAt=f.Clock.GetUtcNow().UtcDateTime.AddMinutes(2),Room="ADM_FUTURE",GraderId=f.Accounts["Teacher"],Capacity=20,Status="CLOSED"};var a=new EntryTestAttempt{RegistrationId=id,AttemptNumber=2};db.Add(session);db.Add(a);await db.SaveChangesAsync(Ct);var b=new EntryTestBooking{AttemptId=a.AttemptId,SessionId=session.SessionId,Status="SCHEDULED",BookedAt=f.Clock.GetUtcNow().UtcDateTime};db.Add(b);await db.SaveChangesAsync(Ct);return b.BookingId;
    }

    [Fact(Skip = OptIn, SkipUnless = nameof(OwnedSqlGate.Enabled), SkipType = typeof(OwnedSqlGate))]
    public async Task Approval_SP01_SP07_OwnInvoiceFN06_AndReplayAreCoherent()
    {
        await using var f = await OwnedSqlFixture.CreateAsync(new Clock()); var id = await Confirmed(f);
        var token = f.Token(await f.Grant("Admissions")); var ex = Executor(f); var s = await Read(f, id);
        await using (var db = f.Db()) { var course = await db.Courses.SingleAsync(Ct); course.Status = "INACTIVE"; await db.SaveChangesAsync(Ct); }
        s = await Read(f, id); var request = Guid.NewGuid(); var body = Approval(s); var preview = await ex.ApprovalAsync(token, id, Ct); Assert.True(preview.Eligible);
        var result = await ex.CommandAsync(token, 41, id, request, body, 65536, Ct); Assert.Equal("ADMITTED", result.Registration.Status); Assert.Equal("1000", result.Registration.Invoice!.Amount);
        var replay = await ex.CommandAsync(token, 41, id, request, body, 65536, Ct); Assert.Equal(result.OperationId, replay.OperationId);
        var found = await ex.SearchAsync(token, AdmissionsSearchFilter.Parse("{\"status\":\"ADMITTED\",\"pageSize\":1}"), Ct); Assert.Equal(1, found.TotalCount); Assert.Single(found.Items); Assert.Equal(result.Registration.Invoice, found.Items[0].Invoice);
        var completion = await ex.LookupAsync(token, "ADMISSIONS.REGISTRATION.APPROVE", request, Ct); Assert.Equal(id, completion.Target["registrationId"]); Assert.Equal(result.Registration.Invoice.Id, completion.Target["invoiceId"]);
        var student = f.Token(await f.Grant("Student")); var invoices = new SqlStudentInvoiceReader(() => f.Db(), f.Clock, 10, 1000);
        var list = await invoices.SearchAsync(student, new QueryCollection(), Ct); Assert.Single(list.Items); Assert.False(list.Items[0].IsOverdue); Assert.Equal(0, list.Items[0].OverdueDays);
        var invoice = await invoices.DetailAsync(student, result.Registration.Invoice.Id, Ct); Assert.Equal("2026-10-17", invoice.DueDate); Assert.Equal("HD-2026-00000001", invoice.Number);
        var wire = JsonSerializer.SerializeToNode(invoice, Wire)!.AsObject(); Assert.Equal(12, wire.Count); Assert.DoesNotContain("payment", wire.Select(x => x.Key)); Assert.DoesNotContain("version", wire.Select(x => x.Key));
        await using (var db = f.Db()) { Assert.Single(await db.Invoices.ToListAsync(Ct)); Assert.Equal(1, (await db.InvoiceNumberCounters.SingleAsync(Ct)).LastValue); Assert.Equal(2, await db.AuditLogs.CountAsync(x => x.OperationId == result.OperationId, Ct)); }
    }

    [Fact(Skip = OptIn, SkipUnless = nameof(OwnedSqlGate.Enabled), SkipType = typeof(OwnedSqlGate))]
    public async Task ApprovalAuditFailureRollsBackInvoiceCounterParentAndMarker()
    {
        await using var f = await OwnedSqlFixture.CreateAsync(new Clock()); var id = await Confirmed(f); var s = await Read(f, id); var token = f.Token(await f.Grant("Admissions")); var request = Guid.NewGuid();
        await using (var held = await f.HoldAudit()) await Assert.ThrowsAsync<EnglishCenter.Api.Infrastructure.Auditing.MandatoryAuditException>(() => Executor(f).CommandAsync(token, 41, id, request, Approval(s), 65536, Ct));
        await using var db = f.Db(); Assert.Equal("CONFIRMED", (await db.Registrations.SingleAsync(Ct)).Status); Assert.Empty(await db.Invoices.ToListAsync(Ct)); Assert.Empty(await db.InvoiceNumberCounters.ToListAsync(Ct)); Assert.False(await db.ReplayOperations.AnyAsync(x => x.RequestId == request, Ct));
    }

    [Fact(Skip = OptIn, SkipUnless = nameof(OwnedSqlGate.Enabled), SkipType = typeof(OwnedSqlGate))]
    public async Task PriceVersionScopeAndRoleGuardsPrecedeEffects()
    {
        await using var f = await OwnedSqlFixture.CreateAsync(new Clock()); var id = await Confirmed(f); var s = await Read(f, id); var body = Approval(s); var token = f.Token(await f.Grant("Admissions"));
        await using (var db = f.Db()) { var c = await db.Courses.SingleAsync(Ct); c.BasePrice = 2000; await db.SaveChangesAsync(Ct); }
        var stale = await Assert.ThrowsAsync<ApiErrorException>(() => Executor(f).CommandAsync(token, 41, id, Guid.NewGuid(), body, 65536, Ct)); Assert.Equal(ApiErrorCode.STATE_CONFLICT, stale.Code);
        var admin = f.Token(await f.Grant("Admin")); var denied = await Assert.ThrowsAsync<ApiErrorException>(() => Executor(f).CommandAsync(admin, 41, id, Guid.NewGuid(), "{}", 65536, Ct)); Assert.Equal(ApiErrorCode.FORBIDDEN, denied.Code);
        var missing = await Assert.ThrowsAsync<ApiErrorException>(() => Executor(f).CommandAsync(token, 41, int.MaxValue, Guid.NewGuid(), "{}", 65536, Ct)); Assert.Equal(ApiErrorCode.NOT_FOUND, missing.Code);
        await using var check = f.Db(); Assert.Empty(await check.Invoices.ToListAsync(Ct)); Assert.Empty(await check.InvoiceNumberCounters.ToListAsync(Ct));
    }

    [Fact(Skip = OptIn, SkipUnless = nameof(OwnedSqlGate.Enabled), SkipType = typeof(OwnedSqlGate))]
    public async Task ApproveVersusRejectHasExactlyOneAtomicWinner()
    {
        await using var f = await OwnedSqlFixture.CreateAsync(new Clock()); var id = await Confirmed(f); var s = await Read(f, id); var one = f.Token(await f.Grant("Admissions")); var two = f.Token(await f.Grant("Admissions"));
        async Task<string> Run(VerifiedSessionToken token, int api, string body)
        { try { return (await Executor(f).CommandAsync(token, api, id, Guid.NewGuid(), body, 65536, Ct)).Registration.Status; } catch (ApiErrorException e) { return e.Code.ToString(); } }
        var outcomes = await Task.WhenAll(Run(one, 41, Approval(s)), Run(two, 42, Reject(s))); Assert.Single(outcomes, x => x is "ADMITTED" or "REJECTED"); Assert.Single(outcomes, x => x == "STATE_CONFLICT");
        await using var db = f.Db(); var parent = await db.Registrations.SingleAsync(Ct); var invoices = await db.Invoices.ToListAsync(Ct); Assert.Equal(parent.Status == "ADMITTED" ? 1 : 0, invoices.Count);
        Assert.Equal(parent.Status == "ADMITTED", parent.IsUnfinished); if (parent.Status == "REJECTED") Assert.Equal("Không tiếp tục\n theo hồ sơ.", parent.RejectionReason);
    }

    [Fact(Skip = OptIn, SkipUnless = nameof(OwnedSqlGate.Enabled), SkipType = typeof(OwnedSqlGate))]
    public async Task CancelFullFutureClosedBookingAndInvoicePreservesCompletedHistory()
    {
        await using var f = await OwnedSqlFixture.CreateAsync(new Clock()); var id = await Confirmed(f); var token = f.Token(await f.Grant("Admissions")); var ex = Executor(f);
        await ex.CommandAsync(token, 41, id, Guid.NewGuid(), Approval(await Read(f, id)), 65536, Ct);
        int futureBooking;
        await using (var db = f.Db())
        {
            var session = new EntryTestSession { StartAt = f.Clock.GetUtcNow().UtcDateTime.AddHours(1), EndAt = f.Clock.GetUtcNow().UtcDateTime.AddHours(2), Room = "ADM_CANCEL", GraderId = f.Accounts["Teacher"], Capacity = 20, Status = "CLOSED" }; db.EntryTestSessions.Add(session);
            var attempt = new EntryTestAttempt { RegistrationId = id, AttemptNumber = 2 }; db.EntryTestAttempts.Add(attempt); await db.SaveChangesAsync(Ct);
            var b = new EntryTestBooking { AttemptId = attempt.AttemptId, SessionId = session.SessionId, Status = "SCHEDULED", BookedAt = f.Clock.GetUtcNow().UtcDateTime }; db.EntryTestBookings.Add(b); await db.SaveChangesAsync(Ct); futureBooking = b.BookingId;
        }
        var state = await Read(f, id); var preview = await ex.CancellationAsync(token, id, Ct); Assert.True(preview.Eligible); var request = Guid.NewGuid();
        var result = await ex.CommandAsync(token, 44, id, request, Cancel(state, f.Clock.GetUtcNow()), 65536, Ct); Assert.Equal("CANCELLED", result.Registration.Status); Assert.Equal("CANCELLED", result.Registration.Invoice!.Status);
        Assert.Equal(result.OperationId, (await ex.CommandAsync(token, 44, id, request, Cancel(state, f.Clock.GetUtcNow()), 65536, Ct)).OperationId);
        await using var db2 = f.Db(); Assert.Equal("CANCELLED", (await db2.EntryTestBookings.SingleAsync(x => x.BookingId == futureBooking, Ct)).Status); Assert.Single(await db2.EntryTestBookings.Where(x => x.Status == "COMPLETED").ToListAsync(Ct)); Assert.Single(await db2.BandAssignments.ToListAsync(Ct)); Assert.Equal(3, await db2.AuditLogs.CountAsync(x => x.OperationId == result.OperationId, Ct));
    }

    [Fact(Skip = OptIn, SkipUnless = nameof(OwnedSqlGate.Enabled), SkipType = typeof(OwnedSqlGate))]
    public async Task AllocatorOverflowRollsBackAndVnYearUsesIssuedTime()
    {
        var time = new Clock(new(2026, 12, 31, 15, 0, 0, TimeSpan.Zero));
        await using var f = await OwnedSqlFixture.CreateAsync(time); var id = await Confirmed(f);
        time.Now = new(2026, 12, 31, 17, 0, 0, TimeSpan.Zero); var token = f.Token(await f.Grant("Admissions"));
        await using (var db = f.Db()) { db.InvoiceNumberCounters.Add(new() { IssueYear = 2027, LastValue = 99999999 }); await db.SaveChangesAsync(Ct); }
        var body = Approval(await Read(f, id));
        await Assert.ThrowsAsync<ApiErrorException>(() => Executor(f).CommandAsync(token, 41, id, Guid.NewGuid(), body, 65536, Ct));
        await using (var db = f.Db()) { Assert.Empty(await db.Invoices.ToListAsync(Ct)); var counter = await db.InvoiceNumberCounters.SingleAsync(Ct); Assert.Equal(99999999, counter.LastValue); db.InvoiceNumberCounters.Remove(counter); await db.SaveChangesAsync(Ct); }
        var result = await Executor(f).CommandAsync(token, 41, id, Guid.NewGuid(), Approval(await Read(f, id)), 65536, Ct);
        Assert.Equal("HD-2027-00000001", result.Registration.Invoice!.Number);
        var student = f.Token(await f.Grant("Student")); var detail = await new SqlStudentInvoiceReader(() => f.Db(), f.Clock, 10, 1000).DetailAsync(student, result.Registration.Invoice.Id, Ct); Assert.Equal("2027-01-08", detail.DueDate);
    }

    [Fact(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    public async Task SearchLiteralFiltersInclusiveVnDayAndHistoryUseCurrentContact()
    {
        await using var f=await OwnedSqlFixture.CreateAsync(new Clock());var id=await Confirmed(f);var token=f.Token(await f.Grant("Admissions"));
        await using(var db=f.Db()){var a=await db.Accounts.SingleAsync(x=>x.AccountId==f.Accounts["Student"],Ct);a.FullName="Nguyễn 100%_[x]";var r=await db.Registrations.SingleAsync(Ct);r.CreatedAt=new DateTime(2026,10,9,17,0,0,DateTimeKind.Utc);await db.SaveChangesAsync(Ct);}
        var ex=Executor(f);var filter=AdmissionsSearchFilter.Parse("{\"name\":\"%_[x]\",\"email\":\"student@\",\"phone\":\"+84 912-345-678\",\"createdFromDate\":\"2026-10-10\",\"createdToDate\":\"2026-10-10\",\"sort\":\"CREATED_ASC\",\"pageSize\":1}");var page=await ex.SearchAsync(token,filter,Ct);Assert.Equal(1,page.TotalCount);Assert.Equal(id,page.Items.Single().Id);Assert.Equal("Nguyễn 100%_[x]",page.Items[0].Student.FullName);
        Assert.Empty((await ex.SearchAsync(token,AdmissionsSearchFilter.Parse("{\"name\":\"%not-a-wildcard\"}"),Ct)).Items);
        Assert.Empty((await ex.SearchAsync(token,AdmissionsSearchFilter.Parse("{\"createdToDate\":\"2026-10-09\"}"),Ct)).Items);
        Assert.Empty((await ex.SearchAsync(token,AdmissionsSearchFilter.Parse("{\"page\":2147483647}"),Ct)).Items);
        var body=Reject(await Read(f,id));await ex.CommandAsync(token,42,id,Guid.NewGuid(),body,65536,Ct);Assert.Equal("REJECTED",(await ex.SearchAsync(token,AdmissionsSearchFilter.Parse("{}"),Ct)).Items.Single().Status);
    }
    [Fact(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    public async Task StudentDebtBoundaryCurrentNamesAndSnapshotAmountAreIndependent()
    {
        var time=new Clock();await using var f=await OwnedSqlFixture.CreateAsync(time);var id=await Confirmed(f);var admission=f.Token(await f.Grant("Admissions"));var approved=await Executor(f).CommandAsync(admission,41,id,Guid.NewGuid(),Approval(await Read(f,id)),65536,Ct);var invoice=approved.Registration.Invoice!.Id;
        await using(var db=f.Db()){var a=await db.Accounts.SingleAsync(x=>x.AccountId==f.Accounts["Student"],Ct);a.FullName="Current Student";var course=await db.Courses.SingleAsync(Ct);course.CourseName="Current Course";course.BasePrice=9999;course.Status="INACTIVE";await db.SaveChangesAsync(Ct);}
        time.Now=new(2026,10,17,16,59,59,TimeSpan.Zero);var token=f.Token(await f.Grant("Student"));var reader=new SqlStudentInvoiceReader(()=>f.Db(),f.Clock,10,1000);var before=await reader.DetailAsync(token,invoice,Ct);Assert.False(before.IsOverdue);Assert.Equal("1000",before.Amount);Assert.Equal("Current Student",before.CurrentStudentName);Assert.Equal("Current Course",before.Course.Name);Assert.Equal("INACTIVE",before.Course.Status);
        time.Now=time.Now.AddSeconds(1);var after=await reader.DetailAsync(token,invoice,Ct);Assert.True(after.IsOverdue);Assert.Equal(1,after.OverdueDays);
        await using(var db=f.Db()){var i=await db.Invoices.SingleAsync(Ct);i.Status="PAID";await db.SaveChangesAsync(Ct);}var paid=await reader.DetailAsync(token,invoice,Ct);Assert.False(paid.IsOverdue);Assert.Equal(0,paid.OverdueDays);
    }
    [Fact(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    public async Task CancelRejectsPaidAndAnyPaymentAndAuditFailureHasNoPartialEffects()
    {
        await using var f=await OwnedSqlFixture.CreateAsync(new Clock());var id=await Confirmed(f);var token=f.Token(await f.Grant("Admissions"));var ex=Executor(f);await ex.CommandAsync(token,41,id,Guid.NewGuid(),Approval(await Read(f,id)),65536,Ct);
        await using(var db=f.Db()){var i=await db.Invoices.SingleAsync(Ct);i.Status="PAID";await db.SaveChangesAsync(Ct);}
        Assert.False((await ex.CancellationAsync(token,id,Ct)).Eligible);var paidBody=Cancel(await Read(f,id),f.Clock.GetUtcNow());await Assert.ThrowsAsync<ApiErrorException>(()=>ex.CommandAsync(token,44,id,Guid.NewGuid(),paidBody,65536,Ct));
        await using(var db=f.Db()){var i=await db.Invoices.SingleAsync(Ct);i.Status="UNPAID";db.Payments.Add(new(){InvoiceId=i.InvoiceId,AmountPaid=i.Amount,Method="CASH",ConfirmedBy=f.Accounts["Accountant"],PaidAt=f.Clock.GetUtcNow().UtcDateTime});await db.SaveChangesAsync(Ct);}
        Assert.False((await ex.CancellationAsync(token,id,Ct)).Eligible);var body=Cancel(await Read(f,id),f.Clock.GetUtcNow());await Assert.ThrowsAsync<ApiErrorException>(()=>ex.CommandAsync(token,44,id,Guid.NewGuid(),body,65536,Ct));
        await using(var db=f.Db()){db.Payments.RemoveRange(await db.Payments.ToArrayAsync(Ct));await db.SaveChangesAsync(Ct);}var request=Guid.NewGuid();body=Cancel(await Read(f,id),f.Clock.GetUtcNow());
        await using(var held=await f.HoldAudit())await Assert.ThrowsAsync<EnglishCenter.Api.Infrastructure.Auditing.MandatoryAuditException>(()=>ex.CommandAsync(token,44,id,request,body,65536,Ct));
        await using var check=f.Db();Assert.Equal("ADMITTED",(await check.Registrations.SingleAsync(Ct)).Status);Assert.Equal("UNPAID",(await check.Invoices.SingleAsync(Ct)).Status);Assert.False(await check.ReplayOperations.AnyAsync(x=>x.RequestId==request,Ct));
    }
    [Fact(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    public async Task CancellationFullSetAndFinalClockCutoffFailEvenWithoutVersionChange()
    {
        var time=new Clock();await using var f=await OwnedSqlFixture.CreateAsync(time);var id=await Confirmed(f);var old=Cancel(await Read(f,id),time.Now);var booking=await Future(f,id);var token=f.Token(await f.Grant("Admissions"));var ex=Executor(f);
        var changed=await Assert.ThrowsAsync<ApiErrorException>(()=>ex.CommandAsync(token,44,id,Guid.NewGuid(),old,65536,Ct));Assert.Equal(ApiErrorCode.STATE_CONFLICT,changed.Code);
        var body=Cancel(await Read(f,id),time.Now);var request=Guid.NewGuid();var interceptor=new AfterReplay(time,time.Now.AddMinutes(1));var atCutoff=new SqlAdmissionsExecutor(()=>f.Db(interceptor),f.Clock,10,1000);
        var cutoff=await Assert.ThrowsAsync<ApiErrorException>(()=>atCutoff.CommandAsync(token,44,id,request,body,65536,Ct));Assert.Equal(ApiErrorCode.STATE_CONFLICT,cutoff.Code);
        await using var check=f.Db();Assert.Equal("CONFIRMED",(await check.Registrations.SingleAsync(Ct)).Status);Assert.Equal("SCHEDULED",(await check.EntryTestBookings.SingleAsync(x=>x.BookingId==booking,Ct)).Status);Assert.False(await check.ReplayOperations.AnyAsync(x=>x.RequestId==request,Ct));
    }
    [Fact(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    public async Task ActualAllocatorTwoOwnedClientsSerializeFirstUseOfSameYear()
    {
        await using var f=await OwnedSqlFixture.CreateAsync(new Clock());var tx=new SqlEnrollmentTransactions(()=>f.Db(),f.Clock,10,1000);
        Task<string> Allocate()=>tx.Write((scope,ct)=>new SqlAdmissionsRepository(scope).AllocateAsync(2026,ct),Ct);
        var numbers=await Task.WhenAll(Allocate(),Allocate());Assert.Equal(2,numbers.Distinct().Count());Assert.Contains("HD-2026-00000001",numbers);Assert.Contains("HD-2026-00000002",numbers);
        await using var db=f.Db();Assert.Equal(2,(await db.InvoiceNumberCounters.SingleAsync(Ct)).LastValue);
    }
    [Fact(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    public async Task CancellationVersusSyntheticProtocolPaymentHasOneCoherentWinner()
    {
        await using var f=await OwnedSqlFixture.CreateAsync(new Clock());var id=await Confirmed(f);var token=f.Token(await f.Grant("Admissions"));var ex=Executor(f);var approved=await ex.CommandAsync(token,41,id,Guid.NewGuid(),Approval(await Read(f,id)),65536,Ct);var invoice=approved.Registration.Invoice!.Id;var body=Cancel(await Read(f,id),f.Clock.GetUtcNow());
        async Task<bool> CancelNow(){try{await ex.CommandAsync(token,44,id,Guid.NewGuid(),body,65536,Ct);return true;}catch(ApiErrorException e)when(e.Code==ApiErrorCode.STATE_CONFLICT){return false;}}
        async Task<bool> Pay()=>await new SqlEnrollmentTransactions(()=>f.Db(),f.Clock,10,1000).Write(async(scope,ct)=>
        {
            var gates=new EnglishCenter.Api.Infrastructure.Replay.SqlReplayRepository(scope,1000);
            foreach(var gate in new[]{"EC:REG:"+id,"EC:INVOICE:"+invoice}.Order(StringComparer.Ordinal))await gates.AcquireAsync(gate,ct);
            var r=await scope.Context.Registrations.SingleAsync(x=>x.RegistrationId==id,ct);var i=await scope.Context.Invoices.SingleAsync(x=>x.InvoiceId==invoice,ct);
            if(r.Status!="ADMITTED"||i.Status!="UNPAID")return false;
            i.Status="PAID";scope.Context.Payments.Add(new(){InvoiceId=invoice,AmountPaid=i.Amount,Method="CASH",ConfirmedBy=f.Accounts["Accountant"],PaidAt=f.Clock.GetUtcNow().UtcDateTime});await scope.Context.SaveChangesAsync(ct);return true;
        },Ct);
        var outcomes=await Task.WhenAll(CancelNow(),Pay());Assert.Single(outcomes,x=>x);await using var check=f.Db();var r=await check.Registrations.SingleAsync(Ct);var i=await check.Invoices.SingleAsync(Ct);var payments=await check.Payments.CountAsync(Ct);
        Assert.True(r.Status=="CANCELLED"&&i.Status=="CANCELLED"&&payments==0||r.Status=="ADMITTED"&&i.Status=="PAID"&&payments==1);
    }
    [Fact(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    public async Task ExistingPlacementBlocksCancelWithoutLosingItsScopedReadReference()
    {
        await using var f=await OwnedSqlFixture.CreateAsync(new Clock());var id=await Confirmed(f);var token=f.Token(await f.Grant("Admissions"));var ex=Executor(f);await ex.CommandAsync(token,41,id,Guid.NewGuid(),Approval(await Read(f,id)),65536,Ct);
        await using(var db=f.Db())
        {
            var r=await db.Registrations.SingleAsync(Ct);var band=await db.BandAssignments.SingleAsync(Ct);var program=new EnglishCenter.Api.Domain.Academics.TrainingProgram{CourseId=r.CourseId,BandId=band.BandId,ProgramName="Synthetic placement guard",Status="DRAFT",ExpectedLessonCount=1};db.Add(program);await db.SaveChangesAsync(Ct);
            var cls=new EnglishCenter.Api.Domain.Training.Class{ProgramId=program.ProgramId,TeacherId=f.Accounts["Teacher"],ClassCode="ADM_GUARD",Status="PLANNED",MaxSize=15,CreatedAt=f.Clock.GetUtcNow().UtcDateTime};db.Add(cls);await db.SaveChangesAsync(Ct);db.Add(new EnglishCenter.Api.Domain.Training.StudentClassAssignment{ClassId=cls.ClassId,RegistrationId=id,AssignedBy=f.Accounts["AcademicAffairs"],AssignedAt=f.Clock.GetUtcNow().UtcDateTime});await db.SaveChangesAsync(Ct);
        }
        var detail=await ex.DetailAsync(token,id,Ct);Assert.Equal("ADM_GUARD",detail.Class!.Code);Assert.DoesNotContain("CANCEL",detail.AllowedActions);Assert.False((await ex.CancellationAsync(token,id,Ct)).Eligible);
        var state=await Read(f,id);var request=Guid.NewGuid();var error=await Assert.ThrowsAsync<ApiErrorException>(()=>ex.CommandAsync(token,44,id,request,Cancel(state,f.Clock.GetUtcNow()),65536,Ct));Assert.Equal(ApiErrorCode.STATE_CONFLICT,error.Code);
        await using var check=f.Db();Assert.Single(await check.StudentClassAssignments.ToListAsync(Ct));Assert.Equal("ADMITTED",(await check.Registrations.SingleAsync(Ct)).Status);Assert.Equal("UNPAID",(await check.Invoices.SingleAsync(Ct)).Status);Assert.False(await check.ReplayOperations.AnyAsync(x=>x.RequestId==request,Ct));
    }
}
