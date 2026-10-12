namespace EnglishCenter.Api.Infrastructure.Classes;

using System.Text.Json.Nodes;
using EnglishCenter.Api.Domain.Training;
using EnglishCenter.Api.Services.Classes;
using Microsoft.EntityFrameworkCore;

public static class SqlSchedulePreviews
{
    public static async Task CheckStateSize(EnglishCenterDbContext db,int classId,int limit,CancellationToken ct)
    {
        var schedules=await db.Schedules.LongCountAsync(x=>x.ClassId==classId,ct);
        var assignments=await db.StudentClassAssignments.LongCountAsync(x=>x.ClassId==classId,ct);
        var components=await db.ScoreComponentConfigs.LongCountAsync(x=>db.Classes.Any(c=>c.ClassId==classId&&c.ProgramId==x.ProgramId),ct);
        if(schedules+assignments+components+1>limit)throw new Common.Errors.ApiErrorException(Common.Errors.ApiErrorCode.SERVICE_UNAVAILABLE);
    }
    public static async Task<JsonObject> Weekly(EnglishCenterDbContext db, ClassState state, WeeklyRule rule,
        DateTime now, int limit, CancellationToken ct)
    {
        var budget = new ScheduleWorkBudget(limit);
        var candidates = rule.Expand(budget);
        budget.Consume(state.Schedules.Length + state.Assignments.Length + state.Program.Components.Count + 1);
        var data = await SqlClassTimetable.ResourceSnapshot(db, [state.Class.ClassId], candidates, [state.Class.TeacherId], ct, budget);
        var resources = SqlClassTimetable.Resources(data, state, candidates, state.Class.TeacherId, false, budget: budget);
        var cohort = await SqlClassTimetable.Cohort(db, state, candidates, now, ct, budget: budget);
        return SchedulePreviews.Weekly(state,rule,candidates,now,resources,cohort);
    }

    public static async Task<JsonObject> Edit(EnglishCenterDbContext db, ClassState state, Schedule old,
        ScheduleCandidate proposed, string? reason, DateTime now, CancellationToken ct)
    {
        var attendance = await db.Attendances.AnyAsync(x => x.ScheduleId == old.ScheduleId, ct);
        var score = await db.Scores.AnyAsync(x => x.ScheduleId == old.ScheduleId, ct);
        var both = new[] { ClassWire.Window(old), proposed };
        var data = await SqlClassTimetable.ResourceSnapshot(db, [state.Class.ClassId], both, [state.Class.TeacherId], ct);
        var guard = SqlClassTimetable.Resources(data, state, both, state.Class.TeacherId, false, old.ScheduleId);
        var resources = SqlClassTimetable.Resources(data, state, [proposed], state.Class.TeacherId, false, old.ScheduleId);
        var cohort = await SqlClassTimetable.Cohort(db, state, [proposed], now, ct, old.ScheduleId);
        return SchedulePreviews.Edit(state,old,proposed,reason,now,attendance,score,await SqlClassReads.ScheduleDetail(db,old.ScheduleId,now,ct),guard,resources,cohort);
    }
}
