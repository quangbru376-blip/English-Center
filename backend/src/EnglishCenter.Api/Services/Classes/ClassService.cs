namespace EnglishCenter.Api.Services.Classes;
using System.Text.Json;
using System.Text.Json.Nodes;
using EnglishCenter.Api.Common.Auditing;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Common.Replay;
using EnglishCenter.Api.Common.Security;
using EnglishCenter.Api.Common.Time;
using EnglishCenter.Api.Domain.Training;
using EnglishCenter.Api.Infrastructure.Classes;

using EnglishCenter.Api.Services.Academics;
using EnglishCenter.Api.Services.Auth;
using EnglishCenter.Api.Services.Replay;

public sealed class ClassService(SqlClassRepository repository,IReplayRepository markers,BusinessClock clock)
{
    public async Task<ClassCommandOutcome> Command(VerifiedSessionToken token,ClassInput input,CancellationToken ct)
    {
        var b=input.Body;var api=input.Api;var candidate=api==108?b.GetProperty("registrationId").GetInt32():(int?)null;
        await markers.AcquireAsync(ReplayService.RequestResource(token.AccountId,input.Intent.Action,input.Request),ct);
        var discovered=api==92?null:(await repository.Discover(input.Id,ct)??throw new ApiErrorException(ApiErrorCode.NOT_FOUND));
        var program=api==92?b.GetProperty("programId").GetInt32():discovered!.ProgramId;var teacher=api==92?b.GetProperty("teacherId").GetInt32():discovered!.TeacherId;
        var regs=await repository.Registrations(input.Id,candidate,ct);var gates=regs.Select(x=>"EC:REG:"+x).Concat(new[]{"EC:BANDSET","EC:PROGRAM:"+program,"EC:TIMETABLE"}).ToList();if(input.Id>0)gates.Add("EC:CLASS:"+input.Id);
        if(candidate is { } rid){var invoice=await repository.InvoiceId(rid,ct);if(invoice is { } iid)gates.Add("EC:INVOICE:"+iid);}
        foreach(var gate in gates.Distinct().Order(StringComparer.Ordinal))await markers.AcquireAsync(gate,ct);
        var proposed=api==97?b.GetProperty("teacherId").GetInt32():(int?)null;await repository.PrepareAccountsAsync(token,await repository.Accounts(regs,teacher,proposed,ct),ct);await AcademicAuthority.RequireAsync(repository,token,clock,ct);
        var replay=new ReplayService(markers,clock);var repeated=await replay.BeginAsync(token.AccountId,input.Request,input.Intent,async(stored,c)=>{await AcademicAuthority.RequireAsync(repository,token,clock,c);return (api==92||stored.Target["classId"]==input.Id)&&await repository.Matches(stored.Target,c);},ct);
        if(repeated is not null)return new(await Result(token,repeated.Target, repeated.OperationId,api,ct),true);
        var operation=Guid.NewGuid();var now=clock.GetUtcNow();ClassState? state=null;Class entity;var target=new Dictionary<string,int>();
        if(api==92)
        {
            var p=await repository.Program(program,ct);var a=await repository.Teacher(teacher,ct);
            var current=ClassWire.Object(new{expectedVersions=new{program=ClassWire.Rv(p.Program.RowVersion),teacher=ClassWire.Rv(a.RowVersion),components=ProgramProjection.References(p)},expectedSet=new{componentIds=p.Components.Select(x=>x.ComponentId)}});RequireManifest(input,current);
            if(p.Program.Status!="ACTIVE"||!ProgramProjection.Validate(p).MeetsActivationRules||a.Status!="ACTIVE")throw AcademicGuard.Conflict();
            entity=new(){ProgramId=program,TeacherId=teacher,ClassCode=b.GetProperty("classCode").GetString()!,MaxSize=b.GetProperty("maxSize").GetInt32(),Status="PLANNED",CreatedAt=now.UtcDateTime};repository.Add(entity);await repository.SaveAsync(ct);
            await Audit(input,token,"Class",entity.ClassId,null,ClassSnapshot(entity),operation,now,null,null,ct);
        }
        else
        {
            state=await repository.LockState(input.Id,regs,candidate,ct);entity=state.Class;if(entity.TeacherId!=teacher||entity.ProgramId!=program)throw AcademicGuard.Conflict();var before=ClassSnapshot(entity);
            if(api==94)
            {
                RequireManifest(input,ClassWire.Object(new{expectedVersions=new{@class=ClassWire.Rv(entity.RowVersion)},expectedSet=new{assignmentIds=state.Assignments.Select(x=>x.ClassAssignmentId)}}));var size=b.GetProperty("maxSize").GetInt32();
                if(!ClassWire.BeforeTeaching(state,now.UtcDateTime)||size<state.Assignments.Length)throw AcademicGuard.Conflict();if(size==entity.MaxSize)throw ApiErrorException.Validation("maxSize");entity.MaxSize=size;
            }
            else if(api==97)
            {
                var preview=await repository.TeacherPreview(state,proposed!.Value,now.UtcDateTime,ct);
                // Same-Teacher new intent is a no-op only after exact current manifest validation.
                var manifest=ClassWire.BaseManifest(state);var versions=manifest["expectedVersions"]!.AsObject();versions.Remove("teacher");versions["currentTeacher"]=ClassWire.Rv(state.Teacher.RowVersion);var a=await repository.Teacher(proposed!.Value,ct);versions["proposedTeacher"]=ClassWire.Rv(a.RowVersion);manifest["expectedSet"]!["hasAttendance"]=false;manifest["expectedSet"]!["hasScores"]=false;RequireManifest(input,manifest);
                if(!ClassWire.BeforeTeaching(state,now.UtcDateTime))throw AcademicGuard.Conflict();if(proposed==entity.TeacherId)throw ApiErrorException.Validation("teacherId");if(!preview["eligible"]!.GetValue<bool>())throw AcademicGuard.Conflict();entity.TeacherId=proposed.Value;
            }
            else if(api is 99 or 100 or 101 or 102)
            {
                var action=api==99?"OPEN":api==100?"START":api==101?"COMPLETE":"CANCEL";var preview=await repository.TransitionPreview(state,action,now.UtcDateTime,ct);var manifest=api==101?ClassWire.CompleteManifest(state):ClassWire.BaseManifest(state);if(api==102){manifest["expectedSet"]!["hasAttendance"]=false;manifest["expectedSet"]!["hasScores"]=false;}RequireManifest(input,manifest);if(!preview["eligible"]!.GetValue<bool>())throw AcademicGuard.Conflict();entity.Status=api==99?"OPEN":api==100?"ONGOING":api==101?"COMPLETED":"CANCELLED";if(api==102){entity.CancelledAt=now.UtcDateTime;entity.CancelledBy=token.AccountId;entity.CancellationReason=b.GetProperty("reason").GetString();entity.CancellationOperationId=operation;}
            }
            else if(api==108)
            {
                var source=await repository.PlacementCandidate(candidate!.Value,ct);var preview=await repository.PlacementPreview(state,source,now.UtcDateTime,ct);if(preview["manifest"] is not JsonObject m)throw AcademicGuard.Conflict();RequireManifest(input,m);
                var assignment=new StudentClassAssignment{ClassId=entity.ClassId,RegistrationId=candidate!.Value,AssignedBy=token.AccountId,AssignedAt=now.UtcDateTime};repository.Add(assignment);await repository.SaveAsync(ct);var report=new GradeReport{ClassAssignmentId=assignment.ClassAssignmentId,Status="DRAFT"};repository.Add(report);await repository.SaveAsync(ct);
                var parent=ClassWire.Object(new{classId=entity.ClassId,registrationId=candidate!.Value,classAssignmentId=assignment.ClassAssignmentId});await Audit(input,token,"StudentClassAssignment",assignment.ClassAssignmentId,null,ClassWire.Object(new{classId=entity.ClassId,registrationId=candidate!.Value,assignedBy=token.AccountId,assignedAt=ClassWire.At(now.UtcDateTime)}),operation,now,parent,null,ct);await Audit(input,token,"GradeReport",report.ReportId,null,new(){["status"]="DRAFT"},operation,now,parent,null,ct,true);target["registrationId"]=candidate!.Value;target["classAssignmentId"]=assignment.ClassAssignmentId;
            }
            else if(api==118)
            {
                var window=ScheduleCandidate.Parse(b.GetProperty("candidate"));var preview=await repository.SchedulePreview(state,window,now.UtcDateTime,ct);if(preview["manifest"] is not JsonObject m)throw AcademicGuard.Conflict();RequireManifest(input,m);
                var schedule=new Schedule{ClassId=entity.ClassId,SessionType=window.Type,ObligationKey=Guid.NewGuid(),IsCurrent=true,SessionDate=window.Date,StartTime=window.StartTime,EndTime=window.EndTime,Room=window.Room,OccurrenceState="PLANNED"};repository.Add(schedule);await repository.SaveAsync(ct);await Audit(input,token,"Schedule",schedule.ScheduleId,null,ClassWire.Object(new{classId=entity.ClassId,sessionType=schedule.SessionType,sessionDate=window.Date.ToString("yyyy-MM-dd"),startTime=window.StartTime.ToString("HH:mm"),endTime=window.EndTime.ToString("HH:mm"),room=window.Room,obligationKey=schedule.ObligationKey.ToString("D"),replacesScheduleId=(int?)null,occurrenceState="PLANNED",isCurrent=true}),operation,now,new(){["classId"]=entity.ClassId},null,ct);target["scheduleId"]=schedule.ScheduleId;
            }
            if(api is 94 or 97 or 99 or 100 or 101 or 102){await repository.SaveAsync(ct);await Audit(input,token,"Class",entity.ClassId,before,ClassSnapshot(entity),operation,now,null,api==102?entity.CancellationReason:null,ct);}
        }
        target["classId"]=entity.ClassId;await replay.CompleteAsync(token.AccountId,input.Request,input.Intent,operation,target,ct);await new CurrentSessionService(repository,clock).TouchApprovedInteractiveActivityAsync(token,ct);var result=await Result(token,target,operation,api,ct);
        var finalNow=clock.GetUtcNow().UtcDateTime;
        if(api==100&&state!.Schedules.Where(x=>x.IsCurrent&&x.SessionType=="LESSON").Min(x=>ClassWire.Window(x).StartAt)>finalNow)throw AcademicGuard.Conflict();
        if(api==118&&ScheduleCandidate.Parse(b.GetProperty("candidate")).StartAt<=finalNow)throw AcademicGuard.Conflict();
        if(state is not null&&api is 94 or 97 or 99 or 102 or 108&&state.Schedules.Any(x=>x.IsCurrent&&x.SessionType=="LESSON"&&ClassWire.Window(x).StartAt<=finalNow))throw AcademicGuard.Conflict();
        return new(result,false);
    }
    private Task Audit(ClassInput i,VerifiedSessionToken t,string type,int id,JsonObject? before,JsonObject after,Guid operation,DateTimeOffset now,JsonObject? parent,string? reason,CancellationToken ct,bool effect=false)=>repository.AuditAsync(AuditEntry.Mandatory(i.Intent.Action.Code+(effect?".EFFECT":""),t.AccountId,type,id,operation,now,new(before,after,parent,reason)),ct);
    private static JsonObject ClassSnapshot(Class c)=>ClassWire.Object(new{programId=c.ProgramId,teacherId=c.TeacherId,maxSize=c.MaxSize,status=c.Status});
    public static void RequireManifest(ClassInput i,JsonObject current)
    {var body=JsonNode.Parse(i.Body.GetRawText())!.AsObject();body["expectedVersions"]=current["expectedVersions"]!.DeepClone();body["expectedSet"]=current["expectedSet"]!.DeepClone();var expected=CanonicalIntent.Bind(i.Api,body.ToJsonString(),i.Api==92?new Dictionary<string,int>():new(){{"classId",i.Id}},int.MaxValue);if(!expected.CopyBytes().AsSpan().SequenceEqual(i.Intent.CopyBytes()))throw AcademicGuard.Conflict();}
    private async Task<JsonObject> Result(VerifiedSessionToken token,IReadOnlyDictionary<string,int> target,Guid operation,int api,CancellationToken ct)
    {
        var s=await repository.State(target["classId"],ct);JsonObject r;
        if(api==108){var a=await repository.Assignment(target["classAssignmentId"],ct);r=ClassWire.Object(new{assignment=new{classAssignmentId=a.ClassAssignmentId,classId=a.ClassId,registrationId=a.RegistrationId,assignedAt=ClassWire.At(a.AssignedAt)},@class=ClassWire.Summary(s),operationId=operation});}
        else if(api==118)r=ClassWire.Object(new{schedule=await repository.ScheduleDetail(target["scheduleId"],clock.GetUtcNow().UtcDateTime,ct),operationId=operation});
        else r=ClassWire.Object(new{@class=ClassWire.Detail(s,clock.GetUtcNow().UtcDateTime),operationId=operation});
        var live=await AcademicAuthority.RequireAsync(repository,token,clock,ct);if(!clock.IsBefore(live.EffectiveDeadline))throw new ApiErrorException(ApiErrorCode.AUTHENTICATION_REQUIRED);return r;
    }
}
