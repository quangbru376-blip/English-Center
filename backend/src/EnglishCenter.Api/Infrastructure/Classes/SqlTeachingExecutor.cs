namespace EnglishCenter.Api.Infrastructure.Classes;

using System.Text.Json.Nodes;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Common.Replay;
using EnglishCenter.Api.Common.Security;
using EnglishCenter.Api.Infrastructure.Replay;
using EnglishCenter.Api.Services.Classes;
using EnglishCenter.Api.Services.Enrollment;
using EnglishCenter.Api.Services.Replay;
using Microsoft.EntityFrameworkCore;

public sealed partial class SqlClassExecutor
{
    public Task<JsonObject> AttendanceSummary(VerifiedSessionToken token,int id,string role,IQueryCollection query,CancellationToken ct)=>
        ScopedRead(token,id,role,async(db,c)=>SqlAttendanceReads.Summary(await SqlClassRepository.ReadState(db,id,c),query,role=="Teacher",Now),ct);
    public Task<JsonObject> AttendanceSheet(VerifiedSessionToken token,int id,int schedule,string role,CancellationToken ct)=>
        ScopedRead(token,id,role,async(db,c)=>await SqlAttendanceReads.Sheet(db,await SqlClassRepository.ReadState(db,id,c),schedule,role=="Teacher",Now,c),ct);
    public async Task<ClassCommandOutcome> AttendanceCommand(VerifiedSessionToken token,int id,int schedule,Guid request,string json,int bytes,CancellationToken ct)
    {
        await ScopedRead(token,id,"Teacher",async(db,c)=>{var s=await SqlClassRepository.ReadState(db,id,c);_=SqlAttendanceReads.Lesson(s,schedule);return true;},ct);
        var input=new AttendanceInput(id,schedule,request,json,bytes);
        return await tx.Write((s,c)=>new AttendanceService(new(s),new SqlReplayRepository(s,lockMs),clock).Command(token,input,c),ct);
    }
    public Task<JsonObject> HeldPreview(VerifiedSessionToken token,int id,CancellationToken ct)=>tx.Read(token,"AcademicAffairs",async(db,c)=>
    {
        var schedule=await db.Schedules.AsNoTracking().SingleOrDefaultAsync(x=>x.ScheduleId==id,c)??throw new ApiErrorException(ApiErrorCode.NOT_FOUND);
        var state=await SqlClassRepository.ReadState(db,schedule.ClassId,c);
        var eligible=TeachingHeld.CanConfirm(state,schedule,Now);
        return ClassWire.Object(new{schedule=await SqlClassReads.ScheduleDetail(db,id,Now,c),eligible,
            blockedReasons=eligible?Array.Empty<string>():new[]{"Chỉ xác nhận buổi hiện hành khi lớp đang dạy và đã đến giờ kết thúc."},
            effects=new[]{"Xác nhận thực tế buổi đã tổ chức; không tự điểm danh hoặc hoàn thành lớp."},manifest=eligible?TeachingHeld.Manifest(state,schedule):null});
    },ct);
    public Task<SafeCommandCompletion> TeachingLookup(VerifiedSessionToken token,string role,string action,Guid request,CancellationToken ct)=>tx.Write(async(s,c)=>
    {
        CanonicalAction selected;try{selected=CanonicalActionRegistry.ForCode(action);}catch(ArgumentException){throw ApiErrorException.Validation("action");}
        if(selected.ApiId is not(100 or 101 or 126 or 133)||selected.Role!=role)throw new ApiErrorException(ApiErrorCode.FORBIDDEN);
        var markers=new SqlReplayRepository(s,lockMs);
        await markers.AcquireAsync(ReplayService.RequestResource(token.AccountId,selected,request),c);
        // Completion targets are discovered before domain gates, never treated as current rights.
        var stored=await markers.FindAsync(token.AccountId,selected.Code,request,c);
        if(stored is not null)await markers.AcquireAsync("EC:CLASS:"+SafeCommandCompletion.Decode(selected,stored.OperationId,stored.ResultMode,stored.ResultReference).Target["classId"],c);
        var repo=new SqlClassRepository(s);await repo.PrepareAccountsAsync(token,[],c);
        await EnrollmentAuthority.RequireAsync(repo,token,clock,role,c);
        var result=await new ReplayService(markers,clock).LookupAsync(90,role,token.AccountId,action,request,async(found,cancel)=>
        {
            await Scope(s.Context,found.Target["classId"],role,token.AccountId,cancel);
            return await SqlClassRepository.Matches(s.Context,found.Target,cancel);
        },c);
        var live=await EnrollmentAuthority.RequireAsync(repo,token,clock,role,c);
        if(!clock.IsBefore(live.EffectiveDeadline))throw new ApiErrorException(ApiErrorCode.AUTHENTICATION_REQUIRED);
        return result;
    },ct);
}

