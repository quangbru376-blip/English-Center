namespace EnglishCenter.Api.Infrastructure.Classes;

using System.Data;
using System.Data.Common;
using System.Text.Json.Nodes;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Domain.Training;
using EnglishCenter.Api.Infrastructure.Identity;
using EnglishCenter.Api.Services.Classes;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

public static class SqlGradeReviewReads
{
    // SP03 supplies every review value. Parent-only metadata stays in the same caller SNAPSHOT.
    public static async Task<GradeState> Read(EnglishCenterDbContext db,int id,int actor,int seconds,CancellationToken ct)
    {
        var state=await SqlClassRepository.ReadState(db,id,ct);
        var assignments=new Dictionary<int,JsonObject>();var computations=new Dictionary<int,JsonObject>();var reports=new List<GradeReport>();var scores=new List<Score>();var totals=new List<(int Id,int Required,int Entered,bool Complete,decimal? Total)>();
        await using var command=db.Database.GetDbConnection().CreateCommand();command.Transaction=db.Database.CurrentTransaction!.GetDbTransaction();command.CommandTimeout=seconds;command.CommandText="dbo.usp_GetClassGradeReview";command.CommandType=CommandType.StoredProcedure;
        command.Parameters.Add(new SqlParameter("@classId",id));command.Parameters.Add(new SqlParameter("@actorId",actor));
        try
        {
            await using var reader=await command.ExecuteReaderAsync(ct);
            if(!await reader.ReadAsync(ct)||I(reader,"classId")!=id||I(reader,"cohortSize")!=state.Assignments.Length)throw new ApiErrorException(ApiErrorCode.INTERNAL_ERROR);
            var visible=I(reader,"visibleReportCount");if(await reader.ReadAsync(ct))throw new ApiErrorException(ApiErrorCode.INTERNAL_ERROR);
            await Next(reader,ct);var componentIds=new List<int>();while(await reader.ReadAsync(ct)){var component=state.Program.Components.SingleOrDefault(x=>x.ComponentId==I(reader,"componentId"));if(component is null||component.ComponentName!=S(reader,"name")||component.Kind!=S(reader,"kind")||component.Weight!=reader.GetDecimal(reader.GetOrdinal("weight"))||!component.RowVersion.SequenceEqual(B(reader,"version")))throw new ApiErrorException(ApiErrorCode.INTERNAL_ERROR);componentIds.Add(component.ComponentId);}
            if(!componentIds.Order().SequenceEqual(state.Program.Components.Select(x=>x.ComponentId).Order()))throw new ApiErrorException(ApiErrorCode.INTERNAL_ERROR);
            await Next(reader,ct);while(await reader.ReadAsync(ct))
            {
                var assignment=I(reader,"classAssignmentId");var status=S(reader,"status");if(status is not("SUBMITTED" or "RETURNED" or "FINALIZED"))throw new ApiErrorException(ApiErrorCode.INTERNAL_ERROR);
                assignments.Add(assignment,ClassWire.Object(new{classAssignmentId=assignment,registrationId=I(reader,"registrationId"),student=new{studentId=I(reader,"studentId"),fullName=S(reader,"fullName")}}));
                reports.Add(new(){ReportId=I(reader,"reportId"),ClassAssignmentId=assignment,Status=status,TeacherComment=NS(reader,"teacherComment"),SubmittedAt=DT(reader,"submittedAt"),ReviewedAt=DT(reader,"reviewedAt"),FinalizedAt=DT(reader,"finalizedAt"),ReturnedReason=NS(reader,"latestReturnReason"),FinalScore=D(reader,"finalScore"),RowVersion=B(reader,"version")});
                totals.Add((assignment,I(reader,"requiredComponentCount"),I(reader,"enteredComponentCount"),reader.GetBoolean(reader.GetOrdinal("isComplete")),D(reader,"total")));
            }
            if(reports.Count!=visible)throw new ApiErrorException(ApiErrorCode.INTERNAL_ERROR);
            await Next(reader,ct);while(await reader.ReadAsync(ct))scores.Add(new(){ClassAssignmentId=I(reader,"classAssignmentId"),ScoreId=I(reader,"scoreId"),ComponentId=I(reader,"componentId"),ScoreValue=reader.GetDecimal(reader.GetOrdinal("score")),ScheduleId=NI(reader,"scheduleId"),EnteredBy=I(reader,"enteredByAccountId"),EnteredAt=reader.GetDateTime(reader.GetOrdinal("enteredAt")),RowVersion=B(reader,"version")});
            await Next(reader,ct);var schedules=new List<int>();while(await reader.ReadAsync(ct)){var schedule=state.Schedules.SingleOrDefault(x=>x.ScheduleId==I(reader,"scheduleId"));if(schedule is null||schedule.ClassId!=I(reader,"classId")||!schedule.RowVersion.SequenceEqual(B(reader,"version")))throw new ApiErrorException(ApiErrorCode.INTERNAL_ERROR);schedules.Add(schedule.ScheduleId);}
            if(!schedules.Order().SequenceEqual(scores.Where(x=>x.ScheduleId!=null).Select(x=>x.ScheduleId!.Value).Distinct().Order())||await reader.NextResultAsync(ct))throw new ApiErrorException(ApiErrorCode.INTERNAL_ERROR);
        }
        catch(SqlException ex)when(ex.Number==51102){throw new ApiErrorException(ApiErrorCode.NOT_FOUND);}
        catch(SqlException ex)when(ex.Number==51101){throw new ApiErrorException(ApiErrorCode.AUTHENTICATION_REQUIRED);}
        foreach(var t in totals)computations.Add(t.Id,SqlGradeRepository.Computation(state,scores.ToArray(),t.Id,t.Required,t.Entered,t.Complete,t.Total));
        return new(state,reports.ToArray(),scores.ToArray(),assignments,computations);
    }
    private static async Task Next(DbDataReader r,CancellationToken ct){if(!await r.NextResultAsync(ct))throw new ApiErrorException(ApiErrorCode.INTERNAL_ERROR);}
    private static int I(DbDataReader r,string n)=>r.GetInt32(r.GetOrdinal(n));private static int? NI(DbDataReader r,string n)=>r.IsDBNull(r.GetOrdinal(n))?null:I(r,n);
    private static string S(DbDataReader r,string n)=>r.GetString(r.GetOrdinal(n));private static string? NS(DbDataReader r,string n)=>r.IsDBNull(r.GetOrdinal(n))?null:S(r,n);
    private static byte[] B(DbDataReader r,string n)=>(byte[])r[n];private static decimal? D(DbDataReader r,string n)=>r.IsDBNull(r.GetOrdinal(n))?null:r.GetDecimal(r.GetOrdinal(n));private static DateTime? DT(DbDataReader r,string n)=>r.IsDBNull(r.GetOrdinal(n))?null:r.GetDateTime(r.GetOrdinal(n));
}
