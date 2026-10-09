namespace EnglishCenter.Api.Services.Catalogue;

using System.Text.Json.Nodes;
using EnglishCenter.Api.Common.Auditing;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Common.Replay;
using EnglishCenter.Api.Common.Security;
using EnglishCenter.Api.Common.Serialization;
using EnglishCenter.Api.Common.Time;
using EnglishCenter.Api.Domain.Academics;
using EnglishCenter.Api.Services.Auth;
using EnglishCenter.Api.Services.Replay;

public interface ICourseRepository : ICurrentSessionRepository
{
    Task<Course?> LockCourseAsync(int id,CancellationToken ct);
    Task<bool> CodeExistsAsync(string code,CancellationToken ct);
    Task InsertAsync(Course course,CancellationToken ct);
    Task SaveAsync(AuditEntry audit,CancellationToken ct);
}
public interface ICourseExecutor
{
    Task<CoursePage> SearchAsync(VerifiedSessionToken token,CourseSearch search,CancellationToken ct);
    Task<CourseView> DetailAsync(VerifiedSessionToken token,int id,CancellationToken ct);
    Task<BandSet> BandsAsync(VerifiedSessionToken token,CancellationToken ct);
    Task<CourseResult> CommandAsync(VerifiedSessionToken token,CourseInput input,CancellationToken ct);
    Task<SafeCommandCompletion> LookupAsync(VerifiedSessionToken token,string action,Guid request,CancellationToken ct);
}
public sealed class CourseService(ICourseRepository repository,IReplayRepository markers,BusinessClock clock)
{
    public async Task<CurrentSessionSnapshot> AuthorizeAsync(VerifiedSessionToken token,CancellationToken ct)
    {
        var current=await new CurrentSessionService(repository,clock).ReadAsync(token,true,ct);
        if(current.Context.Role!="AcademicAffairs")throw new ApiErrorException(ApiErrorCode.FORBIDDEN);
        return current;
    }
    public async Task<CourseResult> CommandAsync(VerifiedSessionToken token,CourseInput input,CancellationToken ct)
    {
        var action=input.Intent.Action;
        await markers.AcquireAsync(ReplayService.RequestResource(token.AccountId,action,input.RequestId),ct);
        var current=await AuthorizeAsync(token,ct);var replay=new ReplayService(markers,clock);Course? existing=null;
        var done=await replay.BeginAsync(token.AccountId,input.RequestId,input.Intent,async(stored,cancel)=>{
            await AuthorizeAsync(token,cancel);existing=await repository.LockCourseAsync(stored.Target["courseId"],cancel);return existing is not null;
        },ct);
        if(done is not null)return new(CourseView.Map(existing!),done.OperationId){Replayed=true};
        Course course;JsonObject? old=null;
        if(action.ApiId==75)
        {
            var code=input.Body.GetProperty("code").GetString()!;
            if(await repository.CodeExistsAsync(code,ct))throw new ApiErrorException(ApiErrorCode.CONFLICT);
            course=new(){CourseCode=code,CourseName=input.Body.GetProperty("name").GetString()!,BasePrice=ExactNumbers.ParseMoney(input.Body.GetProperty("basePrice").GetString()),Status=input.Body.GetProperty("status").GetString()!};
            await repository.InsertAsync(course,ct);
        }
        else
        {
            course=await repository.LockCourseAsync(input.TargetId,ct)??throw new ApiErrorException(ApiErrorCode.NOT_FOUND);
            if(!PasswordChangeVersionGuard.Matches(RowVersionCodec.Decode(input.Body.GetProperty("expectedVersions").GetProperty("course").GetString()),course.RowVersion))throw new ApiErrorException(ApiErrorCode.CONFLICT);
            old=State(course);
            if(action.ApiId==77)
            {
                var name=input.Body.GetProperty("name").GetString()!;var price=ExactNumbers.ParseMoney(input.Body.GetProperty("basePrice").GetString());
                if(name==course.CourseName&&price==course.BasePrice)throw ApiErrorException.Validation();
                course.CourseName=name;course.BasePrice=price;
            }
            else
            {
                var source=action.ApiId==78?"ACTIVE":"INACTIVE";
                if(course.Status!=source)throw new ApiErrorException(ApiErrorCode.STATE_CONFLICT);
                course.Status=source=="ACTIVE"?"INACTIVE":"ACTIVE";
            }
        }
        var operation=Guid.NewGuid();var now=clock.GetUtcNow();
        await repository.SaveAsync(AuditEntry.Mandatory(action.Code,token.AccountId,"Course",course.CourseId,operation,now,new(old,State(course))),ct);
        await replay.CompleteAsync(token.AccountId,input.RequestId,input.Intent,operation,new Dictionary<string,int>{{"courseId",course.CourseId}},ct);
        await new CurrentSessionService(repository,clock).TouchApprovedInteractiveActivityAsync(token,ct);
        await AuthorizeAsync(token,ct);
        if(!clock.IsBefore(current.EffectiveDeadline))throw new ApiErrorException(ApiErrorCode.AUTHENTICATION_REQUIRED);
        return new(CourseView.Map(course),operation);
    }
    public async Task<SafeCommandCompletion> LookupAsync(VerifiedSessionToken token,string action,Guid request,CancellationToken ct)
    {
        CanonicalAction selected;try{selected=CanonicalActionRegistry.ForCode(action);}catch(ArgumentException){throw ApiErrorException.Validation("action");}
        if(selected.ApiId is not (75 or 77 or 78 or 79))throw new ApiErrorException(ApiErrorCode.FORBIDDEN);
        await markers.AcquireAsync(ReplayService.RequestResource(token.AccountId,selected,request),ct);
        await AuthorizeAsync(token,ct);
        return await new ReplayService(markers,clock).LookupAsync(90,"AcademicAffairs",token.AccountId,action,request,async(stored,cancel)=>{
            await AuthorizeAsync(token,cancel);return await repository.LockCourseAsync(stored.Target["courseId"],cancel) is not null;
        },ct);
    }
    private static JsonObject State(Course c)=>new(){["courseCode"]=c.CourseCode,["courseName"]=c.CourseName,["basePrice"]=c.BasePrice,["status"]=c.Status};
}
