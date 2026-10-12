namespace EnglishCenter.Api.Infrastructure.Reports;
using System.Data;
using System.Text.Json.Nodes;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Common.Security;
using EnglishCenter.Api.Common.Time;
using EnglishCenter.Api.Infrastructure.Academics;
using EnglishCenter.Api.Infrastructure.Enrollment;
using EnglishCenter.Api.Services.Enrollment;
using EnglishCenter.Api.Services.Reports;
using Microsoft.EntityFrameworkCore;

public sealed class SqlReportsExecutor(Func<EnglishCenterDbContext> factory,BusinessClock clock,int sqlSeconds,int lockMs)
{
    private readonly SqlAcademicTransactions transactions=new(factory,clock,sqlSeconds,lockMs);
    private Task<DateTimeOffset> Authority(VerifiedSessionToken token,string role,CancellationToken ct)=>transactions.Write(async(scope,c)=>{
        var repository=new SqlEnrollmentRepository(scope);await repository.PrepareAccountsAsync(token,[],c);var live=await EnrollmentAuthority.RequireAsync(repository,token,clock,role,c);if(!clock.IsBefore(live.EffectiveDeadline))throw new ApiErrorException(ApiErrorCode.AUTHENTICATION_REQUIRED);return live.EffectiveDeadline;
    },ct);
    private async Task<(JsonObject Report,ReportQuery Query,DateTimeOffset At,DateTimeOffset Deadline)> Read(VerifiedSessionToken token,int api,IQueryCollection input,CancellationToken ct)
    {
        if(api is <150 or >154)throw new ApiErrorException(ApiErrorCode.FORBIDDEN);var role=api is 150 or 154?"Accountant":"Director";var deadline=await Authority(token,role,ct);var at=clock.GetUtcNow();var query=ReportQuery.Parse(api,input,at);JsonObject report;
        await using(var db=factory()){
            db.Database.SetCommandTimeout(sqlSeconds);await db.Database.OpenConnectionAsync(ct);await using var snapshot=await db.Database.BeginTransactionAsync(IsolationLevel.Snapshot,ct);
            var rows=await new SqlReportRepository(db,sqlSeconds).Read(query,DateOnly.FromDateTime(at.UtcDateTime.AddHours(7)),ct);report=ReportProjection.Render(query,at,rows);await snapshot.CommitAsync(ct);
        }
        await Authority(token,role,ct);if(!clock.IsBefore(deadline))throw new ApiErrorException(ApiErrorCode.AUTHENTICATION_REQUIRED);return(report,query,at,deadline);
    }
    public async Task<JsonObject> Report(VerifiedSessionToken token,int api,IQueryCollection input,CancellationToken ct)
    {
        if(api==154)throw new ApiErrorException(ApiErrorCode.FORBIDDEN);var result=await Read(token,api,input,ct);return result.Report;
    }
    public async Task<FinanceCsvFile> Csv(VerifiedSessionToken token,IQueryCollection input,CancellationToken ct)
    {
        var result=await Read(token,154,input,ct);ct.ThrowIfCancellationRequested();var file=FinanceCsv.Create(result.Report,result.Query,result.At);ct.ThrowIfCancellationRequested();await Authority(token,"Accountant",ct);if(!clock.IsBefore(result.Deadline))throw new ApiErrorException(ApiErrorCode.AUTHENTICATION_REQUIRED);return file;
    }
}
