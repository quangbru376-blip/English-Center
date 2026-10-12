namespace EnglishCenter.Api.Infrastructure.Classes;

using System.Text.Json.Nodes;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Common.Serialization;
using EnglishCenter.Api.Domain.Training;
using EnglishCenter.Api.Infrastructure.Identity;
using EnglishCenter.Api.Services.Classes;
using Microsoft.EntityFrameworkCore;

public sealed class SqlGradeRepository(SqlClassRepository classes)
{
    private EnglishCenterDbContext Db=>classes.Scope.Context;
    public Task<GradeState> State(int id,CancellationToken ct)=>Read(Db,id,ct);
    public async Task<GradeState> Lock(int id,int[] registrations,CancellationToken ct)
    {
        var state=await classes.LockState(id,registrations,null,ct);var ids=state.Assignments.Select(x=>x.ClassAssignmentId).ToArray();
        var scores=await Db.Scores.FromSqlInterpolated($"SELECT s.* FROM dbo.Score s WITH(UPDLOCK,HOLDLOCK) JOIN dbo.StudentClassAssignment a ON a.class_assignment_id=s.class_assignment_id WHERE a.class_id={id}").OrderBy(x=>x.ScoreId).ToArrayAsync(ct);
        var reports=await Db.GradeReports.FromSqlInterpolated($"SELECT r.* FROM dbo.GradeReport r WITH(UPDLOCK,HOLDLOCK) JOIN dbo.StudentClassAssignment a ON a.class_assignment_id=r.class_assignment_id WHERE a.class_id={id}").OrderBy(x=>x.ReportId).ToArrayAsync(ct);
        if(!reports.Select(x=>x.ClassAssignmentId).Order().SequenceEqual(ids.Order()))throw new ApiErrorException(ApiErrorCode.INTERNAL_ERROR);
        return await Finish(Db,state,reports,scores,ct);
    }
    public void Add(Score score)=>Db.Scores.Add(score);
    public void Touch(GradeReport report)=>Db.Entry(report).Property(x=>x.TeacherComment).IsModified=true;
    public static async Task<GradeState> Read(EnglishCenterDbContext db,int id,CancellationToken ct)
    {
        var state=await SqlClassRepository.ReadState(db,id,ct);var ids=state.Assignments.Select(x=>x.ClassAssignmentId).ToArray();
        var reports=await db.GradeReports.AsNoTracking().Where(x=>ids.Contains(x.ClassAssignmentId)).OrderBy(x=>x.ReportId).ToArrayAsync(ct);
        if(!reports.Select(x=>x.ClassAssignmentId).Order().SequenceEqual(ids.Order()))throw new ApiErrorException(ApiErrorCode.INTERNAL_ERROR);
        var scores=await db.Scores.AsNoTracking().Where(x=>ids.Contains(x.ClassAssignmentId)).OrderBy(x=>x.ScoreId).ToArrayAsync(ct);
        return await Finish(db,state,reports,scores,ct);
    }
    internal sealed class TotalRow
    {
        public int ClassAssignmentId{get;set;} public int RequiredComponentCount{get;set;} public int EnteredComponentCount{get;set;} public bool IsComplete{get;set;} public decimal? Total{get;set;}
    }
    internal static JsonObject Computation(ClassState state,Score[] scores,int id,int required,int entered,bool complete,decimal? total)
    {
        var missing=state.Program.Components.Where(x=>!scores.Any(s=>s.ClassAssignmentId==id&&s.ComponentId==x.ComponentId)).Select(x=>x.ComponentId).Order().ToArray();
        if(required!=state.Program.Components.Count||entered!=required-missing.Length||complete!=(required>0&&missing.Length==0)||complete!=(total is not null))throw new ApiErrorException(ApiErrorCode.INTERNAL_ERROR);
        return ClassWire.Object(new{requiredComponentCount=required,enteredComponentCount=entered,missingComponentIds=missing,isComplete=complete,total=total is {} t?ExactNumbers.FormatScore(t):null});
    }
    private static async Task<GradeState> Finish(EnglishCenterDbContext db,ClassState state,GradeReport[] reports,Score[] scores,CancellationToken ct)
    {
        var ids=state.Assignments.Select(x=>x.ClassAssignmentId).ToArray();
        var assignments=await db.StudentClassAssignments.AsNoTracking().Where(x=>ids.Contains(x.ClassAssignmentId)).Select(x=>new{x.ClassAssignmentId,x.RegistrationId,studentId=x.Registration!.StudentId,fullName=x.Registration.Student!.Account!.FullName}).ToArrayAsync(ct);
        var totals=await db.Database.SqlQuery<TotalRow>($"SELECT a.class_assignment_id AS ClassAssignmentId,f.requiredComponentCount AS RequiredComponentCount,f.enteredComponentCount AS EnteredComponentCount,f.isComplete AS IsComplete,f.total AS Total FROM dbo.StudentClassAssignment a CROSS APPLY dbo.ufn_GradeTotal(a.class_assignment_id) f WHERE a.class_id={state.Class.ClassId}").ToArrayAsync(ct);
        if(totals.Length!=ids.Length)throw new ApiErrorException(ApiErrorCode.INTERNAL_ERROR);
        return new(state,reports,scores,assignments.ToDictionary(x=>x.ClassAssignmentId,x=>ClassWire.Object(new{classAssignmentId=x.ClassAssignmentId,registrationId=x.RegistrationId,student=new{studentId=x.studentId,fullName=x.fullName}})),totals.ToDictionary(x=>x.ClassAssignmentId,x=>Computation(state,scores,x.ClassAssignmentId,x.RequiredComponentCount,x.EnteredComponentCount,x.IsComplete,x.Total)));
    }
}
