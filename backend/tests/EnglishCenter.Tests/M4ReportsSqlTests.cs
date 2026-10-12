namespace EnglishCenter.Tests;
using System.Data;
using System.Data.Common;
using System.Net;
using System.Text;
using System.Text.Json.Nodes;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Domain.Academics;
using EnglishCenter.Api.Domain.Admissions;
using EnglishCenter.Api.Domain.Finance;
using EnglishCenter.Api.Infrastructure.Reports;
using EnglishCenter.Api.Services.Classes;
using EnglishCenter.Api.Services.Reports;
using EnglishCenter.Tests.Api.Integration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Xunit;

[Collection("V01 HTTP SQL")]
public sealed class M4ReportsSqlTests
{
    private const string OptIn="Assigned M4 owned scratch SP04–06/HTTP only; exact ID/GUID cleanup.";
    private static CancellationToken Ct=>OwnedSqlFixture.Ct;
    private static QueryCollection Query(string raw="")=>new(QueryHelpers.ParseQuery(raw));
    private static SqlReportsExecutor Ex(OwnedSqlFixture f)=>new(()=>f.Db(),f.Clock,10,1000);
    private static async Task<int> Facts(OwnedSqlFixture f)
    {
        var(_,_,source)=await M2ClassSqlTests.Seed(f);await using var db=f.Db();var original=await db.Registrations.SingleAsync(x=>x.RegistrationId==source,Ct);var old=await db.Invoices.SingleAsync(Ct);old.IssuedAt=f.Clock.GetUtcNow().UtcDateTime.AddMonths(-2);
        for(var i=2;i<=3;i++){
            var course=new Course{CourseCode="REPORT_"+i,CourseName=i==2?" =SUM(1,2),\"Tiếng Việt\"\nKhóa":"Cancelled facts",Status="ACTIVE",BasePrice=999999999999999999m};db.Courses.Add(course);await db.SaveChangesAsync(Ct);
            var reg=new Registration{StudentId=original.StudentId,CourseId=course.CourseId,Status="PENDING",CreatedAt=f.Clock.GetUtcNow().UtcDateTime};db.Registrations.Add(reg);await db.SaveChangesAsync(Ct);
            var invoice=new Invoice{RegistrationId=reg.RegistrationId,InvoiceNumber="HD-2026-0000010"+i,Amount=999999999999999999m,IssuedAt=f.Clock.GetUtcNow().UtcDateTime.AddMonths(-2),DueDate=f.Clock.GetToday(),Status=i==2?"UNPAID":"CANCELLED"};if(i==3){invoice.CancelledBy=f.Accounts["Admissions"];invoice.CancelledAt=f.Clock.GetUtcNow().UtcDateTime;invoice.CancelledFromStatus="UNPAID";invoice.CancellationOperationId=Guid.NewGuid();}db.Invoices.Add(invoice);await db.SaveChangesAsync(Ct);
        }return original.CourseId;
    }
    [Fact(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    public async Task FinanceRealSpUnionPeriodIndependentDebtCurrentDueEqualityAndFreshCsv()
    {
        await using var f=await OwnedSqlFixture.CreateAsync(new M1AdmissionsSqlTests.Clock());await Facts(f);var token=f.Token(await f.Grant("Accountant"));var ex=Ex(f);var report=await ex.Report(token,150,Query("?pageSize=1"),Ct);
        Assert.Equal("1000",report["totals"]!["revenueAmount"]!.GetValue<string>());Assert.Equal("0",report["totals"]!["billedAmount"]!.GetValue<string>());Assert.Equal("999999999999999999",report["totals"]!["currentDebtAmount"]!.GetValue<string>());Assert.Equal("0",report["totals"]!["overdueDebtAmount"]!.GetValue<string>());Assert.Equal(2,report["groups"]!["totalCount"]!.GetValue<int>());Assert.Single(report["groups"]!["items"]!.AsArray());Assert.Equal(3,report["options"]!["courses"]!.AsArray().Count);
        var overdue=await ex.Report(token,150,Query("?overdueOnly=true&page=999999"),Ct);Assert.Equal("1000",overdue["totals"]!["revenueAmount"]!.GetValue<string>());Assert.Equal("0",overdue["totals"]!["currentDebtAmount"]!.GetValue<string>());Assert.Empty(overdue["groups"]!["items"]!.AsArray());
        var csv=await ex.Csv(token,Query(),Ct);Assert.Equal(3,csv.DataRows);var text=Encoding.UTF8.GetString(csv.Bytes);Assert.Contains("\"' =SUM(1,2),\"\"Tiếng Việt\"\"\nKhóa\"",text);Assert.Contains("999999999999999999",text);await using(var db=f.Db()){var unpaid=await db.Invoices.SingleAsync(x=>x.Status=="UNPAID",Ct);unpaid.DueDate=f.Clock.GetToday().AddDays(-1);await db.SaveChangesAsync(Ct);}
        var fresh=await ex.Csv(token,Query("?overdueOnly=true"),Ct);Assert.NotEqual(csv.Bytes,fresh.Bytes);Assert.Contains("999999999999999999",Encoding.UTF8.GetString(fresh.Bytes));
    }
    [Fact(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    public async Task AdmissionsRealSpOverallDistinctNotGroupSum_NoPersonIdentifiersOrOperationalValues()
    {
        await using var f=await OwnedSqlFixture.CreateAsync(new M1AdmissionsSqlTests.Clock());await Facts(f);var director=f.Token(await f.Grant("Director"));var report=await Ex(f).Report(director,152,Query(),Ct);Assert.Equal(3,report["totals"]!["registrationCount"]!.GetValue<int>());Assert.Equal(1,report["totals"]!["distinctStudentCount"]!.GetValue<int>());Assert.Equal(3,report["groups"]!["items"]!.AsArray().Sum(x=>x!["metrics"]!["distinctStudentCount"]!.GetValue<int>()));
        foreach(var field in new[]{"studentId","registrationId","invoiceId","accountId","email","phone","finalScore","basePrice","version"})Assert.DoesNotContain("\""+field+"\"",report.ToJsonString());
        var empty=await Ex(f).Report(director,152,Query("?status=REJECTED"),Ct);Assert.Equal(0,empty["totals"]!["registrationCount"]!.GetValue<int>());Assert.Empty(empty["groups"]!["items"]!.AsArray());Assert.Equal(ApiErrorCode.NOT_FOUND,(await Assert.ThrowsAsync<ApiErrorException>(()=>Ex(f).Report(director,152,Query("?courseId=999999"),Ct))).Code);
    }
    [Fact(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    public async Task TrainingRealSpInventoryDateFreeMatchedDualPagesAndStoredRoundedResults()
    {
        var time=new M1AdmissionsSqlTests.Clock();await using var f=await OwnedSqlFixture.CreateAsync(time);var(a,_,id,_)=await M3PublicationSqlTests.Submitted(f,time,true,"50");await M2ClassSqlTests.Ex(f).GradeCommand(a,147,id,0,Guid.NewGuid(),(await M3PublicationSqlTests.Manifest(f,a,id)).ToJsonString(),65536,Ct);var director=f.Token(await f.Grant("Director"));var report=await Ex(f).Report(director,153,Query(),Ct);
        Assert.Equal(1,report["inventory"]!["totals"]!["classCount"]!.GetValue<int>());Assert.Equal(new[]{"PLANNED","OPEN","ONGOING","COMPLETED","CANCELLED"},report["inventory"]!["totals"]!["classCountsByState"]!.AsObject().Select(x=>x.Key));Assert.Equal(1,report["inventory"]!["totals"]!["classCountsByState"]!["COMPLETED"]!.GetValue<int>());Assert.Equal(2,report["results"]!["totals"]!["finalizedResultCount"]!.GetValue<int>());Assert.Equal("100.00",report["results"]!["totals"]!["passRatePercent"]!.GetValue<string>());Assert.Equal("100.00",report["inventory"]!["groups"]!["items"]![0]!["lessons"]!["progressPercent"]!.GetValue<string>());
        var other=await Ex(f).Report(director,153,Query("?finalizedFromDate=2026-11-01&finalizedToDate=2026-11-30&page=2"),Ct);Assert.Equal(1,other["inventory"]!["totals"]!["classCount"]!.GetValue<int>());Assert.Equal(0,other["results"]!["totals"]!["finalizedResultCount"]!.GetValue<int>());Assert.Null(other["results"]!["totals"]!["passRatePercent"]);Assert.Empty(other["inventory"]!["groups"]!["items"]!.AsArray());Assert.Empty(other["results"]!["groups"]!["items"]!.AsArray());Assert.Single(other["options"]!["teachers"]!.AsArray());
        foreach(var field in new[]{"studentId","classAssignmentId","reportId","teacherComment","enteredBy","finalScore","version"})Assert.DoesNotContain("\""+field+"\"",report.ToJsonString());
    }
    [Fact(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    public async Task SevenRoleHttpDirectFiltersOptionsCsvPrivateAndExactHeaders()
    {
        await using var f=await OwnedSqlFixture.CreateAsync(new M1AdmissionsSqlTests.Clock());await Facts(f);await using var host=await M1HttpSqlTests.Host.Create(f);
        foreach(var role in new[]{"Student","Admissions","AcademicAffairs","Teacher","Accountant","Director","Admin"}){
            await host.Login(role);foreach(var(api,path) in new[]{(150,"accountant/finance"),(151,"director/finance"),(152,"director/admissions"),(153,"director/training"),(154,"accountant/finance/csv")}){
                using var response=await host.Send(HttpMethod.Get,"/api/v1/reports/"+path);var allowed=api is 150 or 154?role=="Accountant":role=="Director";Assert.Equal(allowed?HttpStatusCode.OK:HttpStatusCode.Forbidden,response.StatusCode);Assert.Contains("no-store",response.Headers.CacheControl!.ToString());
                if(api==154&&allowed){Assert.Equal("text/csv",response.Content.Headers.ContentType!.MediaType);Assert.Equal("utf-8",response.Content.Headers.ContentType.CharSet);Assert.Contains("attachment; filename=\"finance-",response.Content.Headers.ContentDisposition!.ToString());Assert.Equal("3",response.Headers.GetValues("X-CSV-Data-Row-Count").Single());Assert.Equal(new byte[]{0xef,0xbb,0xbf},(await response.Content.ReadAsByteArrayAsync(Ct))[..3]);}
            }
        }
        await host.Login("Accountant");Assert.Equal(HttpStatusCode.BadRequest,(await host.Send(HttpMethod.Get,"/api/v1/reports/accountant/finance/csv?page=1")).StatusCode);
    }
    private sealed class AfterSnapshot(M1AdmissionsSqlTests.Clock clock,OwnedSqlFixture fixture,bool expire):DbTransactionInterceptor
    {
        public bool Fired{get;private set;}
        public override async ValueTask<InterceptionResult> TransactionCommittingAsync(DbTransaction transaction,TransactionEventData data,InterceptionResult result,CancellationToken ct=default)
        {
            if(!Fired&&transaction.IsolationLevel==IsolationLevel.Snapshot){Fired=true;if(expire)clock.Now=clock.Now.AddMinutes(31);else{await using var db=fixture.Db();var unpaid=await db.Invoices.SingleAsync(x=>x.Status=="UNPAID",ct);unpaid.Amount=2000;await db.SaveChangesAsync(ct);}}return result;
        }
    }
    [Theory(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    [InlineData(false)][InlineData(true)]
    public async Task FullSnapshotResponseRemainsCoherentAcrossConcurrentChangeOrFinalCutoff(bool expire)
    {
        var time=new M1AdmissionsSqlTests.Clock();await using var f=await OwnedSqlFixture.CreateAsync(time);await Facts(f);var token=f.Token(await f.Grant("Accountant"));var probe=new AfterSnapshot(time,f,expire);var ex=new SqlReportsExecutor(()=>f.Db(probe),f.Clock,10,1000);
        if(expire)Assert.Equal(ApiErrorCode.AUTHENTICATION_REQUIRED,(await Assert.ThrowsAsync<ApiErrorException>(()=>ex.Report(token,150,Query(),Ct))).Code);
        else{var report=await ex.Report(token,150,Query(),Ct);Assert.Equal("999999999999999999",report["totals"]!["currentDebtAmount"]!.GetValue<string>());Assert.Equal("999999999999999999",report["groups"]!["items"]!.AsArray().Single(x=>x!["course"]!["code"]!.GetValue<string>()=="REPORT_2")!["metrics"]!["currentDebtAmount"]!.GetValue<string>());Assert.Equal("2000",(await Ex(f).Report(token,150,Query(),Ct))["totals"]!["currentDebtAmount"]!.GetValue<string>());}Assert.True(probe.Fired);
    }
    [Fact(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    public async Task RealDecimal38AggregateExceedsIndividualDecimal18WithoutPrecisionLoss()
    {
        await using var f=await OwnedSqlFixture.CreateAsync(new M1AdmissionsSqlTests.Clock());await Facts(f);
        await using(var db=f.Db()){
            var student=(await db.Registrations.OrderBy(x=>x.RegistrationId).FirstAsync(Ct)).StudentId;var course=new Course{CourseCode="REPORT_LARGE",CourseName="Large exact aggregate",Status="ACTIVE",BasePrice=999999999999999999m};db.Courses.Add(course);await db.SaveChangesAsync(Ct);var reg=new Registration{StudentId=student,CourseId=course.CourseId,Status="PENDING",CreatedAt=f.Clock.GetUtcNow().UtcDateTime};db.Registrations.Add(reg);await db.SaveChangesAsync(Ct);db.Invoices.Add(new(){RegistrationId=reg.RegistrationId,InvoiceNumber="HD-2026-00000104",Amount=999999999999999999m,IssuedAt=f.Clock.GetUtcNow().UtcDateTime,DueDate=f.Clock.GetToday().AddDays(-1),Status="UNPAID"});await db.SaveChangesAsync(Ct);
        }
        var token=f.Token(await f.Grant("Accountant"));var report=await Ex(f).Report(token,150,Query(),Ct);Assert.Equal("1999999999999999998",report["totals"]!["currentDebtAmount"]!.GetValue<string>());Assert.Equal("999999999999999999",report["totals"]!["overdueDebtAmount"]!.GetValue<string>());var csv=await Ex(f).Csv(token,Query(),Ct);Assert.Contains("\"1999999999999999998\"",Encoding.UTF8.GetString(csv.Bytes));
    }

}
