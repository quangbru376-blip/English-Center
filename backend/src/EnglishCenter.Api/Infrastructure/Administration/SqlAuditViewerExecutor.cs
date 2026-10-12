namespace EnglishCenter.Api.Infrastructure.Administration;
using System.Data;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Common.Security;
using EnglishCenter.Api.Common.Time;
using EnglishCenter.Api.Domain.Administration;
using EnglishCenter.Api.Infrastructure.Academics;
using EnglishCenter.Api.Infrastructure.Enrollment;
using EnglishCenter.Api.Services.Administration;
using EnglishCenter.Api.Services.Enrollment;
using Microsoft.EntityFrameworkCore;

public sealed class SqlAuditViewerExecutor(Func<EnglishCenterDbContext> factory,BusinessClock clock,int sqlSeconds,int lockMs):IAuditViewerExecutor
{
    private readonly SqlAcademicTransactions authority=new(factory,clock,sqlSeconds,lockMs);
    private Task<DateTimeOffset> Require(VerifiedSessionToken token,CancellationToken ct)=>authority.Write(async(s,c)=>
    {var live=await EnrollmentAuthority.RequireAsync(new SqlEnrollmentRepository(s),token,clock,"Admin",c);if(!clock.IsBefore(live.EffectiveDeadline))throw new ApiErrorException(ApiErrorCode.AUTHENTICATION_REQUIRED);return live.EffectiveDeadline;},ct);
    private async Task<T> Read<T>(VerifiedSessionToken token,Func<EnglishCenterDbContext,DateTime, CancellationToken,Task<T>> read,CancellationToken ct,Func<T,DateTime,T>? finalize=null)
    {
        var cap=await Require(token,ct);T result;
        await using(var db=factory())
        {
            if(db.Database.CurrentTransaction is not null)throw new ApiErrorException(ApiErrorCode.STATE_CONFLICT);
            db.Database.SetCommandTimeout(sqlSeconds);await using var snapshot=await db.Database.BeginTransactionAsync(IsolationLevel.Snapshot,ct);
            result=await read(db,clock.GetUtcNow().UtcDateTime,ct);await snapshot.CommitAsync(ct);
        }
        await Require(token,ct);if(!clock.IsBefore(cap))throw new ApiErrorException(ApiErrorCode.AUTHENTICATION_REQUIRED);
        if(finalize is not null)result=finalize(result,clock.GetUtcNow().UtcDateTime);
        if(!clock.IsBefore(cap))throw new ApiErrorException(ApiErrorCode.AUTHENTICATION_REQUIRED);return result;
    }
    public Task<AuditEventPage> List(VerifiedSessionToken token,IQueryCollection query,CancellationToken ct)=>Page(token,AuditViewerQuery.Parse(query),null,ct);
    public Task<AuditEventPage> Operation(VerifiedSessionToken token,Guid operation,IQueryCollection query,CancellationToken ct)
    {if(operation==Guid.Empty)throw ApiErrorException.Validation("operationId");return Page(token,AuditViewerQuery.Parse(query,true),operation,ct);}
    private Task<AuditEventPage> Page(VerifiedSessionToken token,AuditViewerQuery f,Guid? operation,CancellationToken ct)
    {
      DateTime? firstExpiry=null;
      return Read(token,async(db,now,c)=>
      {
        var rows=db.AuditLogs.AsNoTracking().Where(x=>x.AuditCategory=="MANDATORY"||x.AuditCategory=="SECURITY"&&x.SecurityExpiresAt>now);
        if(operation is {} op)rows=rows.Where(x=>x.OperationId==op);
        if(f.From is {} from&&from!=DateOnly.MinValue){var at=from.ToDateTime(TimeOnly.MinValue).AddHours(-7);rows=rows.Where(x=>x.OccurredAt>=at);}
        if(f.To is {} to&&to!=DateOnly.MaxValue){var at=to.AddDays(1).ToDateTime(TimeOnly.MinValue).AddHours(-7);rows=rows.Where(x=>x.OccurredAt<at);}
        if(f.Actor is {} actor)rows=rows.Where(x=>x.ActorAccountId==actor);if(f.Action is {} action)rows=rows.Where(x=>x.Action==action);if(f.Entity is {} entity)rows=rows.Where(x=>x.EntityType==entity);if(f.Reference is {} reference)rows=rows.Where(x=>x.EntityId==reference);if(f.Result is {} result)rows=rows.Where(x=>x.Result==result);
        var count=await rows.LongCountAsync(c);var offset=(long)(f.Page-1)*f.Size;
        var data=offset>int.MaxValue?[]:await rows.OrderByDescending(x=>x.OccurredAt).ThenByDescending(x=>x.AuditId).Skip((int)offset).Take(f.Size).ToArrayAsync(c);
        if(data.Any(x=>!AuditViewerPolicy.Accepts(x.Action,x.EntityType)))throw new ApiErrorException(ApiErrorCode.INTERNAL_ERROR);
        firstExpiry=data.Where(x=>x.AuditCategory=="SECURITY").Select(x=>x.SecurityExpiresAt).Min();
        return new AuditEventPage(data.Select(AuditViewerPolicy.Summary).ToArray(),f.Page,f.Size,count);
      },ct,(result,now)=>firstExpiry is {} expires&&expires<=now?throw new ApiErrorException(ApiErrorCode.SERVICE_UNAVAILABLE):result);
    }
    public Task<AuditEventDetail> Detail(VerifiedSessionToken token,int id,CancellationToken ct)
    {
        if(id<=0)throw ApiErrorException.Validation("id");AuditLog? buffered=null;return Read(token,async(db,now,c)=>
        {
            var row=await db.AuditLogs.AsNoTracking().SingleOrDefaultAsync(x=>x.AuditId==id&&(x.AuditCategory=="MANDATORY"||x.AuditCategory=="SECURITY"&&x.SecurityExpiresAt>now),c)??throw new ApiErrorException(ApiErrorCode.NOT_FOUND);
            if(!AuditViewerPolicy.Accepts(row.Action,row.EntityType))throw new ApiErrorException(ApiErrorCode.INTERNAL_ERROR);
            buffered=row;
            return AuditViewerPolicy.Detail(row,now);
        },ct,(_,now)=>buffered!.AuditCategory=="SECURITY"&&buffered.SecurityExpiresAt<=now?throw new ApiErrorException(ApiErrorCode.NOT_FOUND):AuditViewerPolicy.Detail(buffered,now));
    }
}
