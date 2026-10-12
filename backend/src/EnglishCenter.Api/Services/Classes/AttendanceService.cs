namespace EnglishCenter.Api.Services.Classes;

using System.Text.Json.Nodes;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Common.Security;
using EnglishCenter.Api.Common.Time;
using EnglishCenter.Api.Domain.Training;
using EnglishCenter.Api.Infrastructure.Classes;
using EnglishCenter.Api.Services.Academics;
using EnglishCenter.Api.Services.Auth;
using EnglishCenter.Api.Services.Enrollment;
using EnglishCenter.Api.Services.Replay;


public sealed class AttendanceService(SqlClassRepository repository,IReplayRepository markers,BusinessClock clock)
{
    public async Task<ClassCommandOutcome> Command(VerifiedSessionToken token,AttendanceInput input,CancellationToken ct)
    {
        await markers.AcquireAsync(ReplayService.RequestResource(token.AccountId,input.Intent.Action,input.Request),ct);
        var reference=await repository.Discover(input.ClassId,ct)??throw new ApiErrorException(ApiErrorCode.NOT_FOUND);
        var registrations=await repository.Registrations(input.ClassId,null,ct);
        var gates=registrations.Select(x=>"EC:REG:"+x).Concat(new[]{"EC:CLASS:"+input.ClassId,"EC:PROGRAM:"+reference.ProgramId});
        foreach(var gate in gates.Distinct().Order(StringComparer.Ordinal))await markers.AcquireAsync(gate,ct);
        await repository.PrepareAccountsAsync(token,await repository.Accounts(registrations,reference.TeacherId,null,ct),ct);
        await Require(token,input.ClassId,ct);
        var replay=new ReplayService(markers,clock);
        var repeated=await replay.BeginAsync(token.AccountId,input.Request,input.Intent,async(stored,c)=>{
            await Require(token,input.ClassId,c);return stored.Target["classId"]==input.ClassId&&stored.Target["scheduleId"]==input.ScheduleId&&await repository.Matches(stored.Target,c);
        },ct);
        if(repeated is not null)return new(await Result(token,input,repeated.OperationId,[],ct),true);
        var state=await repository.LockState(input.ClassId,registrations,null,ct);
        if(state.Class.TeacherId!=reference.TeacherId||state.Class.TeacherId!=token.AccountId)throw new ApiErrorException(ApiErrorCode.NOT_FOUND);
        var lesson=SqlAttendanceReads.Lesson(state,input.ScheduleId);
        var changes=input.Body.GetProperty("changes").EnumerateArray().Select(x=>(Id:x.GetProperty("classAssignmentId").GetInt32(),Present:x.GetProperty("present").GetInt32()==1)).ToArray();
        input.RequireManifest(AttendanceWire.SaveManifest(state,lesson,changes.Select(x=>x.Id).ToArray()));
        var now=clock.GetUtcNow();
        if(!AttendanceWire.CanEdit(state,lesson,now.UtcDateTime))throw AcademicGuard.Conflict();
        var saved=new List<Attendance>();
        foreach(var change in changes)
        {
            if(!state.Assignments.Any(x=>x.ClassAssignmentId==change.Id))throw AcademicGuard.Conflict();
            var row=(state.AttendanceRows??[]).SingleOrDefault(x=>x.ScheduleId==input.ScheduleId&&x.ClassAssignmentId==change.Id);
            if(row is not null&&row.Present==change.Present)continue;
            if(row is null){row=new(){ScheduleId=input.ScheduleId,ClassAssignmentId=change.Id};repository.Add(row);}
            row.Present=change.Present;row.EnteredBy=token.AccountId;row.EnteredAt=now.UtcDateTime;saved.Add(row);
        }
        if(saved.Count==0)throw ApiErrorException.Validation("changes");
        await repository.SaveAsync(ct);var operation=Guid.NewGuid();
        await replay.CompleteAsync(token.AccountId,input.Request,input.Intent,operation,new Dictionary<string,int>{{"classId",input.ClassId},{"scheduleId",input.ScheduleId}},ct);
        await new CurrentSessionService(repository,clock).TouchApprovedInteractiveActivityAsync(token,ct);
        var result=await Result(token,input,operation,saved.ToArray(),ct);
        if(!AttendanceWire.CanEdit(state,lesson,clock.GetUtcNow().UtcDateTime))throw AcademicGuard.Conflict();
        return new(result,false);
    }
    private async Task Require(VerifiedSessionToken token,int id,CancellationToken ct)
    {
        var live=await EnrollmentAuthority.RequireAsync(repository,token,clock,"Teacher",ct);
        if(!await repository.TeacherTask(id,token.AccountId,ct))throw new ApiErrorException(ApiErrorCode.NOT_FOUND);
        if(!clock.IsBefore(live.EffectiveDeadline))throw new ApiErrorException(ApiErrorCode.AUTHENTICATION_REQUIRED);
    }
    private async Task<JsonObject> Result(VerifiedSessionToken token,AttendanceInput input,Guid operation,Attendance[] saved,CancellationToken ct)
    {
        var state=await repository.State(input.ClassId,ct);
        var sheet=await SqlAttendanceReads.Sheet(repository.Scope.Context,state,input.ScheduleId,true,clock.GetUtcNow().UtcDateTime,ct);
        var result=ClassWire.Object(new{classId=input.ClassId,scheduleId=input.ScheduleId,saved=saved.OrderBy(x=>x.ClassAssignmentId).Select(x=>new{classAssignmentId=x.ClassAssignmentId,attendanceId=x.AttendanceId,version=ClassWire.Rv(x.RowVersion)}),sheet,operationId=operation});
        await Require(token,input.ClassId,ct);return result;
    }
}
