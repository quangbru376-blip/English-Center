namespace EnglishCenter.Api.Services.Classes;

using System.Globalization;
using System.Text.Json.Nodes;
using EnglishCenter.Api.Common.Auditing;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Common.Security;
using EnglishCenter.Api.Common.Time;
using EnglishCenter.Api.Domain.Training;
using EnglishCenter.Api.Infrastructure.Classes;
using EnglishCenter.Api.Services.Academics;
using EnglishCenter.Api.Services.Auth;
using EnglishCenter.Api.Services.Enrollment;
using EnglishCenter.Api.Services.Replay;

public sealed class GradeService(SqlClassRepository classes,SqlGradeRepository grades,IReplayRepository markers,BusinessClock clock)
{
    public async Task<ClassCommandOutcome> Command(VerifiedSessionToken token,GradeInput input,CancellationToken ct)
    {
        var role=input.Api is 146 or 147?"AcademicAffairs":"Teacher";
        await markers.AcquireAsync(ReplayService.RequestResource(token.AccountId,input.Intent.Action,input.Request),ct);
        var reference=await classes.Discover(input.ClassId,ct)??throw new ApiErrorException(ApiErrorCode.NOT_FOUND);
        var registrations=await classes.Registrations(input.ClassId,null,ct);
        var gates=registrations.Select(x=>"EC:REG:"+x).Concat(new[]{"EC:CLASS:"+input.ClassId,"EC:PROGRAM:"+reference.ProgramId});
        foreach(var gate in gates.Distinct().Order(StringComparer.Ordinal))await markers.AcquireAsync(gate,ct);
        await classes.PrepareAccountsAsync(token,await classes.Accounts(registrations,reference.TeacherId,null,ct),ct);
        var deadline=await Require(token,input.ClassId,role,ct);
        var replay=new ReplayService(markers,clock);
        var repeated=await replay.BeginAsync(token.AccountId,input.Request,input.Intent,async(stored,c)=>{
            await Require(token,input.ClassId,role,c);return stored.Target["classId"]==input.ClassId&&await classes.Matches(stored.Target,c);
        },ct);
        if(repeated is not null)return new(await Result(token,input,role,repeated.OperationId,ct),true);
        var state=await grades.Lock(input.ClassId,registrations,ct);
        if(state.Class.Class.ProgramId!=reference.ProgramId||state.Class.Class.TeacherId!=reference.TeacherId||!state.Class.Assignments.Select(x=>x.RegistrationId).Order().SequenceEqual(registrations.Order()))throw AcademicGuard.Conflict();
        var operation=Guid.NewGuid();var now=clock.GetUtcNow();
        if(input.Api==139)await SaveDraft(token,input,state,operation,now,ct);
        else await Transition(token,input,state,operation,now,ct);
        var target=new Dictionary<string,int>{{"classId",input.ClassId}};if(input.Api==139)target["classAssignmentId"]=input.AssignmentId;
        await replay.CompleteAsync(token.AccountId,input.Request,input.Intent,operation,target,ct);
        await new CurrentSessionService(classes,clock).TouchApprovedInteractiveActivityAsync(token,ct);
        var result=await Result(token,input,role,operation,ct);
        // A clock rewind after result projection must not make saved linked scores premature.
        var final=await grades.State(input.ClassId,ct);
        if(!final.Scores.Where(x=>input.Api!=139||x.ClassAssignmentId==input.AssignmentId).All(x=>GradeWire.ScheduleValid(final.Class,x,clock.GetUtcNow().UtcDateTime)))throw AcademicGuard.Conflict();
        await Require(token,input.ClassId,role,ct);if(!clock.IsBefore(deadline))throw new ApiErrorException(ApiErrorCode.AUTHENTICATION_REQUIRED);return new(result,false);
    }
    private async Task SaveDraft(VerifiedSessionToken token,GradeInput input,GradeState state,Guid operation,DateTimeOffset now,CancellationToken ct)
    {
        var report=state.Reports.SingleOrDefault(x=>x.ClassAssignmentId==input.AssignmentId)??throw new ApiErrorException(ApiErrorCode.NOT_FOUND);
        var deltas=input.Body.GetProperty("scores").EnumerateArray().Select(x=>new Score{ClassAssignmentId=input.AssignmentId,ComponentId=x.GetProperty("componentId").GetInt32(),ScoreValue=decimal.Parse(x.GetProperty("score").GetString()!,CultureInfo.InvariantCulture),ScheduleId=x.GetProperty("scheduleId").ValueKind==System.Text.Json.JsonValueKind.Null?null:x.GetProperty("scheduleId").GetInt32()}).ToArray();
        input.RequireManifest(GradeWire.DraftManifest(state,input.AssignmentId,deltas.Select(x=>x.ComponentId).ToArray(),deltas.Where(x=>x.ScheduleId!=null).Select(x=>x.ScheduleId!.Value).ToArray()));
        foreach(var delta in deltas)
        {
            var component=state.Class.Program.Components.SingleOrDefault(x=>x.ComponentId==delta.ComponentId);
            var schedule=state.Class.Schedules.SingleOrDefault(x=>x.ScheduleId==delta.ScheduleId);
            if(component is null||component.Kind=="GENERAL"&&delta.ScheduleId is not null||component.Kind!="GENERAL"&&(schedule is null||schedule.SessionType!=component.Kind))throw ApiErrorException.Validation("scores");
        }
        if(!GradeWire.Editable(state.Class,report)||!ProgramProjection.Validate(state.Class.Program).MeetsActivationRules||!state.Scores.Where(x=>x.ClassAssignmentId==input.AssignmentId).Concat(deltas).All(x=>GradeWire.ScheduleValid(state.Class,x,now.UtcDateTime)))throw AcademicGuard.Conflict();
        var oldComment=report.TeacherComment;var hasComment=input.Body.TryGetProperty("teacherComment",out var comment);var newComment=hasComment?(comment.ValueKind==System.Text.Json.JsonValueKind.Null?null:comment.GetString()):oldComment;
        var changed=new List<(Score Row,JsonObject? Before)>();
        foreach(var delta in deltas)
        {
            var row=state.Scores.SingleOrDefault(x=>x.ClassAssignmentId==input.AssignmentId&&x.ComponentId==delta.ComponentId);
            if(row is not null&&row.ScoreValue==delta.ScoreValue&&row.ScheduleId==delta.ScheduleId)continue;
            var before=row is null?null:ScoreSnapshot(row);
            if(row is null){row=delta;grades.Add(row);}
            row.ScoreValue=delta.ScoreValue;row.ScheduleId=delta.ScheduleId;row.EnteredBy=token.AccountId;row.EnteredAt=now.UtcDateTime;changed.Add((row,before));
        }
        if(changed.Count==0&&newComment==oldComment)throw new ApiErrorException(ApiErrorCode.NO_CHANGE,"scores");
        report.TeacherComment=newComment;grades.Touch(report);await classes.SaveAsync(ct);
        var parent=ClassWire.Object(new{classId=input.ClassId,classAssignmentId=input.AssignmentId,gradeReportId=report.ReportId});
        await Audit(input,token,"GradeReport",report.ReportId,new(){["teacherComment"]=oldComment},new(){["teacherComment"]=newComment},parent,null,operation,now,ct);
        foreach(var change in changed)await Audit(input,token,"Score",change.Row.ScoreId,change.Before,ScoreSnapshot(change.Row),parent,null,operation,now,ct);
        // Check new rows as well as the previously saved aggregate at the final lower clock cutoff.
        if(!changed.All(x=>GradeWire.ScheduleValid(state.Class,x.Row,clock.GetUtcNow().UtcDateTime)))throw AcademicGuard.Conflict();
    }
    private async Task Transition(VerifiedSessionToken token,GradeInput input,GradeState state,Guid operation,DateTimeOffset now,CancellationToken ct)
    {
        input.RequireManifest(input.Api==147?await grades.FinalizeManifest(state,ct):GradeWire.WorkflowManifest(state));if(!GradeWire.WorkflowEligible(state,input.Api is 146 or 147,now.UtcDateTime))throw AcademicGuard.Conflict();
        if(input.Api==147)await grades.ValidatePublication(state,ct);
        var reason=input.Api==146?input.Body.GetProperty("reason").GetString():null;
        foreach(var report in state.Reports)
        {
            var before=ReportSnapshot(report);
            if(input.Api==147){report.Status="FINALIZED";report.FinalScore=decimal.Parse(state.Computations[report.ClassAssignmentId]["total"]!.GetValue<string>(),CultureInfo.InvariantCulture);report.FinalizedAt=now.UtcDateTime;report.ReviewedBy=token.AccountId;report.ReviewedAt=now.UtcDateTime;report.ReviewOperationId=operation;}
            else if(input.Api==146){report.Status="RETURNED";report.ReviewedBy=token.AccountId;report.ReviewedAt=now.UtcDateTime;report.ReviewOperationId=operation;report.ReturnedReason=reason;}
            else{report.Status="SUBMITTED";report.SubmittedBy=token.AccountId;report.SubmittedAt=now.UtcDateTime;report.SubmittedOperationId=operation;}
            await classes.SaveAsync(ct);
            var assignment=state.Class.Assignments.Single(x=>x.ClassAssignmentId==report.ClassAssignmentId);
            await Audit(input,token,"GradeReport",report.ReportId,before,ReportSnapshot(report),ClassWire.Object(new{classId=input.ClassId,classAssignmentId=assignment.ClassAssignmentId,registrationId=assignment.RegistrationId}),reason,operation,now,ct);
        }
        if(input.Api==147)foreach(var change in await grades.ApplyUnfinished(state,ct))await Audit(input,token,"Registration",change.Id,ClassWire.Object(new{status=change.Status,isUnfinished=change.Before}),ClassWire.Object(new{status=change.Status,isUnfinished=change.After}),ClassWire.Object(new{classId=input.ClassId,registrationId=change.Id,classAssignmentId=change.Assignment}),null,operation,now,ct);
        await Audit(input,token,"Class",input.ClassId,new(){["status"]="COMPLETED"},new(){["status"]="COMPLETED"},new(){["classId"]=input.ClassId},reason,operation,now,ct);
    }
    private Task Audit(GradeInput input,VerifiedSessionToken token,string entity,int id,JsonObject? old,JsonObject next,JsonObject parent,string? reason,Guid operation,DateTimeOffset now,CancellationToken ct)=>classes.AuditAsync(AuditEntry.Mandatory(input.Intent.Action.Code+(entity==(input.Api==139?"GradeReport":"Class")?"":".EFFECT"),token.AccountId,entity,id,operation,now,new(old,next,parent,reason)),ct);
    private static JsonObject ScoreSnapshot(Score s)=>ClassWire.Object(new{componentId=s.ComponentId,scheduleId=s.ScheduleId,scoreValue=s.ScoreValue,enteredBy=s.EnteredBy,enteredAt=ClassWire.At(s.EnteredAt)});
    private static JsonObject ReportSnapshot(GradeReport r)=>ClassWire.Object(new{status=r.Status,submittedBy=r.SubmittedBy,submittedAt=r.SubmittedAt is {} a?ClassWire.At(a):null,submittedOperationId=r.SubmittedOperationId,reviewedBy=r.ReviewedBy,reviewedAt=r.ReviewedAt is {} b?ClassWire.At(b):null,reviewOperationId=r.ReviewOperationId,finalScore=r.FinalScore,finalizedAt=r.FinalizedAt is {} f?ClassWire.At(f):null});
    private async Task<DateTimeOffset> Require(VerifiedSessionToken token,int id,string role,CancellationToken ct)
    {
        var live=await EnrollmentAuthority.RequireAsync(classes,token,clock,role,ct);
        if(role=="AcademicAffairs"&&!await classes.ReviewTask(id,ct))throw new ApiErrorException(ApiErrorCode.NOT_FOUND);
        if(role=="Teacher"&&!await classes.TeacherTask(id,token.AccountId,ct))throw new ApiErrorException(ApiErrorCode.NOT_FOUND);
        if(!clock.IsBefore(live.EffectiveDeadline))throw new ApiErrorException(ApiErrorCode.AUTHENTICATION_REQUIRED);
        return live.EffectiveDeadline;
    }
    private async Task<JsonObject> Result(VerifiedSessionToken token,GradeInput input,string role,Guid operation,CancellationToken ct)
    {
        var state=await grades.State(input.ClassId,ct);var result=input.Api==139?ClassWire.Object(new{detail=GradeWire.Detail(state,input.AssignmentId),operationId=operation}):GradeWire.WorkflowResult(state,operation);
        await Require(token,input.ClassId,role,ct);return result;
    }
}
