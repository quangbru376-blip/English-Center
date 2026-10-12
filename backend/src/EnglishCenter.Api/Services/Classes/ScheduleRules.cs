namespace EnglishCenter.Api.Services.Classes;

using System.Text.Json;
using System.Text.Json.Nodes;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Common.Replay;
using EnglishCenter.Api.Common.Serialization;
using EnglishCenter.Api.Common.Validation;
using EnglishCenter.Api.Domain.Training;
using EnglishCenter.Api.Infrastructure.Classes;

public sealed class ScheduleWorkBudget(int limit)
{
    private int remaining = limit > 0 ? limit : throw new ArgumentOutOfRangeException(nameof(limit));
    public int Remaining => remaining;
    public void Consume(int count = 1)
    {
        if (count < 0 || count > remaining) throw new ApiErrorException(ApiErrorCode.SERVICE_UNAVAILABLE);
        remaining -= count;
    }
}

public sealed record WeeklyRule(DateOnly FromDate, DateOnly ToDate, int[] Weekdays, TimeOnly StartTime, TimeOnly EndTime, string Room)
{
    public static WeeklyRule Parse(JsonElement body)
    {
        try
        {
            var names = new[] { "fromDate", "toDate", "weekdays", "startTime", "endTime", "room" };
            if (body.ValueKind != JsonValueKind.Object || body.EnumerateObject().Count() != 6 ||
                body.EnumerateObject().Select(x => x.Name).Distinct().Count() != 6 || body.EnumerateObject().Any(x => !names.Contains(x.Name))) throw new ArgumentException();
            var from = StrictScalars.ParseLocalDate(body.GetProperty("fromDate").GetString());
            var to = StrictScalars.ParseLocalDate(body.GetProperty("toDate").GetString());
            var days = body.GetProperty("weekdays").EnumerateArray().Select(x => StrictScalars.ParseInt32(x, 1, 7)).ToArray();
            var start = StrictScalars.ParseMinuteTime(body.GetProperty("startTime").GetString());
            var end = StrictScalars.ParseMinuteTime(body.GetProperty("endTime").GetString());
            if (from > to || days.Length == 0 || days.Distinct().Count() != days.Length || start >= end) throw new ArgumentException();
            return new(from, to, days.Order().ToArray(), start, end, ClassWire.Code(body.GetProperty("room").GetString()));
        }
        catch (Exception e) when (e is ArgumentException or InvalidOperationException or KeyNotFoundException)
        { throw ApiErrorException.Validation("rule"); }
    }
    public ScheduleCandidate[] Expand(ScheduleWorkBudget budget)
    {
        var result = new List<ScheduleCandidate>();
        for (var day = FromDate; ; day = day.AddDays(1))
        {
            budget.Consume();
            if (Weekdays.Contains(((int)day.DayOfWeek + 6) % 7 + 1))
            {
                var candidate = new ScheduleCandidate("LESSON", day, StartTime, EndTime, Room);
                try { _ = candidate.StartAt; _ = candidate.EndAt; }
                catch (ArgumentException) { throw ApiErrorException.Validation("rule"); }
                result.Add(candidate);
            }
            if (day == ToDate) break;
        }
        return result.ToArray();
    }
    public JsonObject Wire() => ClassWire.Object(new { fromDate = FromDate.ToString("yyyy-MM-dd"), toDate = ToDate.ToString("yyyy-MM-dd"), weekdays = Weekdays, startTime = StartTime.ToString("HH:mm"), endTime = EndTime.ToString("HH:mm"), room = Room });
}

public sealed class ScheduleInput
{
    public int Api { get; }
    public int Id { get; }
    public Guid Request { get; }
    public CanonicalIntent Intent { get; }
    public JsonElement Body { get; }
    public ScheduleInput(int api, int id, Guid request, string json, int bytes, int weeklyBudget=10000)
    {
        if (api is not (120 or 122 or 124 or 126) || request == Guid.Empty || id <= 0) throw ApiErrorException.Validation();
        Api = api; Id = id; Request = request;
        if(api==120)
        {
            try { using var raw=JsonDocument.Parse(json);_=WeeklyRule.Parse(raw.RootElement.GetProperty("rule")).Expand(new(weeklyBudget)); }
            catch(Exception e) when(e is JsonException or KeyNotFoundException or InvalidOperationException){throw ApiErrorException.Validation("rule");}
        }
        Intent = CanonicalIntent.Bind(api, json, new Dictionary<string, int> { [api == 120 ? "classId" : "scheduleId"] = id }, bytes);
        using var doc = JsonDocument.Parse(Intent.CopyBytes()); Body = doc.RootElement.GetProperty("body").Clone();
        if(Body.GetProperty("expectedSet").GetProperty("scheduleBindings").EnumerateArray().Select(x=>x.GetProperty("classId").GetInt32()).Distinct().Count()>1)throw ApiErrorException.Validation("expectedSet");
    }
    public void RequireManifest(JsonObject current)
    {
        var body = JsonNode.Parse(Body.GetRawText())!.AsObject();
        body["expectedVersions"] = current["expectedVersions"]!.DeepClone();
        body["expectedSet"] = current["expectedSet"]!.DeepClone();
        var fresh = CanonicalIntent.Bind(Api, body.ToJsonString(), new Dictionary<string, int> { [Api == 120 ? "classId" : "scheduleId"] = Id }, int.MaxValue);
        if (!fresh.CopyBytes().AsSpan().SequenceEqual(Intent.CopyBytes())) throw EnglishCenter.Api.Services.Academics.AcademicGuard.Conflict();
    }
}

public static class ScheduleRules
{
    public static ScheduleCandidate Destination(JsonElement body, string type, bool replacement)
    {
        var fields = replacement ? new[] { "newDate", "startTime", "endTime", "room", "reason" } : new[] { "newDate", "startTime", "endTime", "room" };
        if (body.ValueKind != JsonValueKind.Object || body.EnumerateObject().Count() != fields.Length || body.EnumerateObject().Select(x => x.Name).Distinct().Count() != fields.Length || body.EnumerateObject().Any(x => !fields.Contains(x.Name))) throw ApiErrorException.Validation();
        var candidate = new JsonObject { ["type"] = type, ["date"] = JsonNode.Parse(body.GetProperty("newDate").GetRawText()), ["startTime"] = JsonNode.Parse(body.GetProperty("startTime").GetRawText()), ["endTime"] = JsonNode.Parse(body.GetProperty("endTime").GetRawText()), ["room"] = JsonNode.Parse(body.GetProperty("room").GetRawText()) };
        using var doc = JsonDocument.Parse(candidate.ToJsonString()); return ScheduleCandidate.Parse(doc.RootElement);
    }
    public static string Reason(JsonElement body)
    {
        try { return NonsecretText.NormalizeRequired(body.GetProperty("reason").GetString(), 1000); }
        catch (Exception e) when (e is ArgumentException or InvalidOperationException or KeyNotFoundException) { throw ApiErrorException.Validation("reason"); }
    }
    public static bool Order(ClassState state, ScheduleCandidate candidate, int? substitute = null)
    {
        var windows = state.Schedules.Where(x => x.IsCurrent && x.ScheduleId != substitute).Select(ClassWire.Window).Append(candidate).ToArray();
        var lessons = windows.Where(x => x.Type == "LESSON").ToArray(); var exams = windows.Where(x => x.Type == "FINAL_EXAM").ToArray();
        return lessons.Length <= state.Program.Program.ExpectedLessonCount && exams.Length <= 1 &&
            (exams.Length == 0 || lessons.Length > 0 && exams[0].StartAt >= lessons.Max(x => x.EndAt));
    }
    public static bool Editable(ClassState state, Schedule old, ScheduleCandidate proposed, DateTime now, bool replacement, bool attendance, bool score)
        => state.Class.Status is not ("COMPLETED" or "CANCELLED") && ClassWire.TeacherValid(state) && old.IsCurrent && old.OccurrenceState == "PLANNED" &&
            !attendance && !score && (replacement ? now >= ClassWire.Window(old).StartAt : now < ClassWire.Window(old).StartAt) && proposed.StartAt > now && Order(state, proposed, old.ScheduleId);
}
