namespace EnglishCenter.Api.Infrastructure.Classes;

using System.Text.Json.Nodes;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Domain.Identity;
using EnglishCenter.Api.Services.Classes;
using Microsoft.EntityFrameworkCore;

public static class SqlClassPreviewQueries
{
    public static ScheduleCandidate[] Windows(ClassState state) => state.Schedules
        .Where(x => x.IsCurrent && x.OccurrenceState != "NOT_HELD").Select(ClassWire.Window).ToArray();

    public static async Task<JsonObject> Transition(EnglishCenterDbContext db, ClassState state,
        string action, DateTime now, CancellationToken ct)
    {
        var resources = await SqlClassTimetable.Resources(db, state, Windows(state), state.Class.TeacherId, true, ct);
        return ClassPreviews.Transition(state, action, now, resources);
    }

    public static async Task<JsonObject> Teacher(EnglishCenterDbContext db, ClassState state,
        int proposed, DateTime now, CancellationToken ct)
    {
        var teacher = await db.Accounts.AsNoTracking().SingleOrDefaultAsync(x => x.AccountId == proposed, ct)
            ?? throw new ApiErrorException(ApiErrorCode.NOT_FOUND);
        if (!await db.Roles.AnyAsync(x => x.RoleId == teacher.RoleId && x.RoleCode == "Teacher", ct))
            throw new ApiErrorException(ApiErrorCode.NOT_FOUND);
        var resources = await SqlClassTimetable.Resources(db, state, Windows(state), proposed, true, ct);
        return ClassPreviews.Teacher(state, teacher, now, resources);
    }

    public static async Task<JsonObject> Placement(EnglishCenterDbContext db, ClassState state,
        PlacementSource source, DateTime now, CancellationToken ct)
    {
        var windows = Windows(state);
        var student = await SqlClassTimetable.Student(db, source.Registration.StudentId, windows, now, ct);
        var resources = await SqlClassTimetable.Resources(db, state, windows, state.Class.TeacherId, true, ct);
        var assigned = await db.StudentClassAssignments.AnyAsync(x => x.RegistrationId == source.Registration.RegistrationId, ct);
        return ClassPreviews.Placement(state, source, now, student, resources, assigned);
    }

    public static async Task<JsonObject> Schedule(EnglishCenterDbContext db, ClassState state,
        ScheduleCandidate candidate, DateTime now, CancellationToken ct)
    {
        var resources = await SqlClassTimetable.Resources(db, state, [candidate], state.Class.TeacherId, false, ct);
        var cohort = await SqlClassTimetable.Cohort(db, state, [candidate], now, ct);
        return ClassPreviews.Schedule(state, candidate, now, resources, cohort);
    }
}
