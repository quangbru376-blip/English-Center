namespace EnglishCenter.Api.Infrastructure.Classes;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Services.Academics;
using EnglishCenter.Api.Services.Classes;
using Microsoft.EntityFrameworkCore;
internal static class SqlClassStateReads
{
    private sealed class ProgressRow
    {
        public int ClassId{get;set;}
        public int ExpectedLessonCount{get;set;}
        public long CurrentLessonCount{get;set;}
        public int CurrentFinalExamCount{get;set;}
    }
    public static async Task<Dictionary<int,ClassState>> Read(EnglishCenterDbContext db,int[] ids,CancellationToken ct)
    {
        if(ids.Length==0)return [];
        var classes=await db.Classes.AsNoTracking().Where(x=>ids.Contains(x.ClassId)).Include(x=>x.Program!).ThenInclude(x=>x.Course).Include(x=>x.Program!).ThenInclude(x=>x.Band).Include(x=>x.Teacher).ToArrayAsync(ct);
        var programs=classes.Select(x=>x.ProgramId).Distinct().ToArray();
        var components=await db.ScoreComponentConfigs.AsNoTracking().Where(x=>programs.Contains(x.ProgramId)).OrderBy(x=>x.ComponentId).ToArrayAsync(ct);
        var assignments=await db.StudentClassAssignments.AsNoTracking().Where(x=>ids.Contains(x.ClassId)).OrderBy(x=>x.ClassAssignmentId).ToArrayAsync(ct);
        var schedules=await db.Schedules.AsNoTracking().Where(x=>ids.Contains(x.ClassId)).OrderBy(x=>x.ScheduleId).ToArrayAsync(ct);
        var teacherRole=await db.Roles.Where(x=>x.RoleCode=="Teacher").Select(x=>x.RoleId).SingleAsync(ct);
        var attendance=await (from row in db.Attendances.AsNoTracking() join a in db.StudentClassAssignments on row.ClassAssignmentId equals a.ClassAssignmentId join schedule in db.Schedules on row.ScheduleId equals schedule.ScheduleId where ids.Contains(a.ClassId) select new{a.ClassId,schedule.IsCurrent,schedule.SessionType}).ToArrayAsync(ct);
        var scores=await (from row in db.Scores.AsNoTracking() join a in db.StudentClassAssignments on row.ClassAssignmentId equals a.ClassAssignmentId where ids.Contains(a.ClassId) select a.ClassId).Distinct().ToArrayAsync(ct);
        var progress=await db.Database.SqlQuery<ProgressRow>($"SELECT c.class_id AS ClassId,p.expectedLessonCount AS ExpectedLessonCount,p.currentLessonCount AS CurrentLessonCount,p.currentFinalExamCount AS CurrentFinalExamCount FROM dbo.Class c CROSS APPLY dbo.ufn_ClassObligationProgress(c.class_id) p").Where(x=>ids.Contains(x.ClassId)).ToDictionaryAsync(x=>x.ClassId,ct);
        var states=new Dictionary<int,ClassState>();
        foreach(var cl in classes)
        {
            var rows=schedules.Where(x=>x.ClassId==cl.ClassId).ToArray();var p=cl.Program!;var counts=progress[cl.ClassId];
            if(counts.ExpectedLessonCount!=p.ExpectedLessonCount||counts.CurrentLessonCount!=rows.Count(x=>x.IsCurrent&&x.SessionType=="LESSON")||counts.CurrentFinalExamCount!=rows.Count(x=>x.IsCurrent&&x.SessionType=="FINAL_EXAM"))throw new ApiErrorException(ApiErrorCode.INTERNAL_ERROR);
            var program=new ProgramState(p,p.Course!,p.Band!,components.Where(x=>x.ProgramId==p.ProgramId).ToArray(),true);
            states[cl.ClassId]=new(cl,program,cl.Teacher!,cl.Teacher!.RoleId==teacherRole,assignments.Where(x=>x.ClassId==cl.ClassId).ToArray(),rows,attendance.Count(x=>x.ClassId==cl.ClassId&&x.IsCurrent&&x.SessionType=="LESSON"),scores.Contains(cl.ClassId),attendance.Any(x=>x.ClassId==cl.ClassId));
        }
        return states;
    }
}
