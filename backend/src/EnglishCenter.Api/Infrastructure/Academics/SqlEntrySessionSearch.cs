namespace EnglishCenter.Api.Infrastructure.Academics;

using EnglishCenter.Api.Services.Academics;
using Microsoft.EntityFrameworkCore;

internal static class SqlEntrySessionSearch
{
    public static async Task<IReadOnlyList<AcademicSessionView>> Project(EnglishCenterDbContext db, int[] ids, DateTimeOffset now, CancellationToken ct)
    {
        if (ids.Length == 0) return [];
        var sessions = await db.EntryTestSessions.AsNoTracking().Include(x => x.Grader).Where(x => ids.Contains(x.SessionId)).ToListAsync(ct);
        var teacherRoles = await db.Roles.Where(x => x.RoleCode == "Teacher").Select(x => x.RoleId).ToListAsync(ct);
        var counts = await db.EntryTestBookings.Where(x => ids.Contains(x.SessionId)).GroupBy(x => x.SessionId).Select(g => new { Id = g.Key, Scheduled = g.Count(x => x.Status == "SCHEDULED"), Completed = g.Count(x => x.Status == "COMPLETED"), Absent = g.Count(x => x.Status == "ABSENT"), Cancelled = g.Count(x => x.Status == "CANCELLED") }).ToDictionaryAsync(x => x.Id, ct);
        var earliest = sessions.Min(x => x.StartAt); var latest = sessions.Max(x => x.EndAt); var teachers = sessions.Select(x => x.GraderId).Distinct().ToArray();
        var peers = await db.EntryTestSessions.AsNoTracking().Where(x => x.Status != "CANCELLED" && x.StartAt < latest && x.EndAt > earliest && teachers.Contains(x.GraderId)).Select(x => new { x.SessionId, x.GraderId, x.StartAt, x.EndAt }).ToListAsync(ct);
        var first = DateOnly.FromDateTime(earliest.AddHours(7)); var last = DateOnly.FromDateTime(latest.AddHours(7));
        var schedules = await db.Schedules.AsNoTracking().Where(x => x.SessionDate >= first && x.SessionDate <= last && x.IsCurrent && (x.OccurrenceState == "PLANNED" || x.OccurrenceState == "HELD") && x.Class!.Status != "CANCELLED" && teachers.Contains(x.Class.TeacherId)).Select(x => new { x.SessionDate, x.StartTime, x.EndTime, x.Class!.TeacherId }).ToListAsync(ct);
        var result = new Dictionary<int, AcademicSessionView>();
        foreach (var s in sessions)
        {
            var window = SessionWindow.From(s); var count = counts.GetValueOrDefault(s.SessionId); var scheduled = count?.Scheduled ?? 0;
            var available = s.Grader!.Status == "ACTIVE" && teacherRoles.Contains(s.Grader.RoleId) && !peers.Any(x => x.SessionId != s.SessionId && x.GraderId == s.GraderId && x.StartAt < s.EndAt && x.EndAt > s.StartAt) && !schedules.Any(x => x.TeacherId == s.GraderId && x.SessionDate == window.Date && x.StartTime < window.EndTime && x.EndTime > window.StartTime);
            result.Add(s.SessionId, new(s.SessionId, window.Date.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture), window.StartTime.ToString("HH:mm", System.Globalization.CultureInfo.InvariantCulture), window.EndTime.ToString("HH:mm", System.Globalization.CultureInfo.InvariantCulture), AcademicGuard.Instant(s.StartAt), AcademicGuard.Instant(s.EndAt), s.Room, s.Capacity, s.Status, s.Status == "CANCELLED" ? "CANCELLED" : s.StartAt <= now.UtcDateTime ? "CLOSED" : s.Status, new(s.GraderId, s.Grader.FullName), available, available ? [] : ["Giáo viên không hoạt động hoặc trùng lịch."], scheduled, Math.Max(0, s.Capacity - scheduled), new(scheduled, count?.Completed ?? 0, count?.Absent ?? 0, count?.Cancelled ?? 0), Common.Serialization.RowVersionCodec.Encode(s.RowVersion)));
        }
        return ids.Select(id => result[id]).ToArray();
    }
}
