namespace EnglishCenter.Api.Infrastructure.Catalogue;

using System.Data;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Common.Replay;
using EnglishCenter.Api.Common.Security;
using EnglishCenter.Api.Common.Serialization;
using EnglishCenter.Api.Common.Time;
using EnglishCenter.Api.Infrastructure.Identity;
using EnglishCenter.Api.Infrastructure.Replay;
using EnglishCenter.Api.Services.Catalogue;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

public sealed class SqlCourseExecutor(Func<EnglishCenterDbContext> factory,BusinessClock clock,int sqlSeconds,int lockMs) : ICourseExecutor
{
    public Task<CourseResult> CommandAsync(VerifiedSessionToken token,CourseInput input,CancellationToken ct)=>Live((s,c)=>s.CommandAsync(token,input,c),ct);
    public Task<SafeCommandCompletion> LookupAsync(VerifiedSessionToken token,string action,Guid request,CancellationToken ct)=>Live((s,c)=>s.LookupAsync(token,action,request,c),ct);
    public Task<CourseView> DetailAsync(VerifiedSessionToken token,int id,CancellationToken ct)=>Read(token,async(db,c)=>{
        var row=await db.Courses.AsNoTracking().SingleOrDefaultAsync(x=>x.CourseId==id,c)??throw new ApiErrorException(ApiErrorCode.NOT_FOUND);return CourseView.Map(row);
    },ct);
    public Task<CoursePage> SearchAsync(VerifiedSessionToken token,CourseSearch search,CancellationToken ct)=>Read(token,async(db,c)=>{
        var rows=db.Courses.AsNoTracking();if(search.Code is not null)rows=rows.Where(x=>x.CourseCode==search.Code);
        if(search.Name is not null)rows=rows.Where(x=>x.CourseName.Contains(search.Name));if(search.Status is not null)rows=rows.Where(x=>x.Status==search.Status);
        var count=await rows.CountAsync(c);var ordered=search.Sort=="CODE_ASC"?rows.OrderBy(x=>x.CourseCode).ThenBy(x=>x.CourseId):rows.OrderByDescending(x=>x.CourseCode).ThenByDescending(x=>x.CourseId);
        var offset=(long)(search.Page-1)*search.PageSize;var data=offset>int.MaxValue?[]:await ordered.Skip((int)offset).Take(search.PageSize).ToListAsync(c);
        return new CoursePage(data.Select(CourseView.Map).ToArray(),search.Page,search.PageSize,count);
    },ct);
    public Task<BandSet> BandsAsync(VerifiedSessionToken token,CancellationToken ct)=>Read(token,async(db,c)=>{
        var bands=await db.Bands.AsNoTracking().OrderBy(x=>x.MinScore).ThenBy(x=>x.BandId).ToListAsync(c);
        var scores=new decimal[]{0,25,45,65,85};
        if(bands.Count!=5||bands.Where((b,i)=>b.BandCode!="Band"+(i+1)||b.MinScore!=scores[i]).Any())throw new ApiErrorException(ApiErrorCode.SERVICE_UNAVAILABLE);
        var frozen=await db.EntryTestAttempts.AnyAsync(x=>x.Score!=null&&x.CompletedBookingId!=null,c);
        return new BandSet(bands.Select(x=>new BandView(x.BandId,x.BandCode,ExactNumbers.FormatScore(x.MinScore),RowVersionCodec.Encode(x.RowVersion))).ToArray(),frozen);
    },ct);
    private async Task<T> Read<T>(VerifiedSessionToken token,Func<EnglishCenterDbContext,CancellationToken,Task<T>> read,CancellationToken ct)
    {
        await Live((s,c)=>s.AuthorizeAsync(token,c),ct);T result;
        await using(var db=factory())
        {
            if(db.Database.CurrentTransaction is not null)throw new ApiErrorException(ApiErrorCode.STATE_CONFLICT);
            db.Database.SetCommandTimeout(sqlSeconds);await db.Database.OpenConnectionAsync(ct);await using var tx=await db.Database.BeginTransactionAsync(IsolationLevel.Snapshot,ct);
            result=await read(db,ct);await tx.CommitAsync(ct);
        }
        await Live((s,c)=>s.AuthorizeAsync(token,c),ct);return result;
    }
    private async Task<T> Live<T>(Func<CourseService,CancellationToken,Task<T>> work,CancellationToken ct)
    {
        await using var db=factory();if(db.Database.CurrentTransaction is not null)throw new ApiErrorException(ApiErrorCode.STATE_CONFLICT);
        await db.Database.OpenConnectionAsync(ct);await using var tx=await db.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted,ct);
        await using var scope=await SqlAuthDeadlineScope.EnterAsync(db,sqlSeconds,lockMs,ct);
        try{var result=await work(new(new SqlCourseRepository(scope),new SqlReplayRepository(scope,lockMs),clock),ct);await tx.CommitAsync(ct);return result;}
        catch(DbUpdateConcurrencyException){throw new ApiErrorException(ApiErrorCode.CONFLICT);}
        catch(DbUpdateException e)when(e.InnerException is SqlException s&&s.Number is 2601 or 2627){throw new ApiErrorException(ApiErrorCode.CONFLICT);}
        catch(SqlException e)when(e.Number is 1205 or 1222){throw new ApiErrorException(ApiErrorCode.SERVICE_UNAVAILABLE);}
        catch(DbUpdateException e)when(e.InnerException is SqlException s&&s.Number is 1205 or 1222){throw new ApiErrorException(ApiErrorCode.SERVICE_UNAVAILABLE);}
    }
}
