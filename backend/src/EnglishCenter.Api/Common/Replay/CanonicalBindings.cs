namespace EnglishCenter.Api.Common.Replay;

using System.Text.Json.Nodes;
using EnglishCenter.Api.Common.Serialization;

internal static class CanonicalBindings
{
    internal static void Validate(int api, JsonObject body, JsonObject target)
    {
        var v = body["expectedVersions"] as JsonObject; var set = body["expectedSet"] as JsonObject;
        if (api is 51 or 52 && body["method"]!.GetValue<string>() == "TRANSFER" && body["transactionReference"] is null) throw Invalid();
        if (api == 30)
        {
            var c = (JsonObject)body["attemptContext"]!; if ((c["attemptId"] is null) != (c["attemptVersion"] is null)) throw Invalid();
        }
        if (api is 28 or 44)
        {
            Match(Ids(v!, "bookings", "id"), Ids(set!, "futureBookingIds"));
            var invoice = v!["invoice"] as JsonObject; if (invoice is null ? set!["invoiceId"] is not null : set!["invoiceId"] is null || Number(invoice["id"]) != Number(set["invoiceId"])) throw Invalid();
        }
        if (api == 36) { Match(Ids(v!, "attempts", "id"), Ids(set!, "attemptIds")); Match(Ids(v!, "bookings", "id"), Ids(set!, "scheduledBookingIds")); if (!Ids(set!, "attemptIds").Contains(Number(set!["bestAttemptId"]))) throw Invalid(); }
        if (api is 61 or 64)
        {
            Match(Ids(v!, "bookings", "id"), Ids(set!, "scheduledBookingIds")); Match(Ids(v!, "bookings", "id"), Ids(set!, "bindings", "bookingId"));
            Match(Ids(v!, "attempts", "id"), Ids(set!, "bindings", "attemptId", false)); Match(Ids(v!, "registrations", "id"), Ids(set!, "bindings", "registrationId", false));
        }
        if (api is 70 or 71 or 72)
        {
            Match(Ids(v!, "relatedAttempts", "id"), Ids(set!, "attemptIds"));
            if (api != 71) { var band = v!["bandAssignment"] as JsonObject; if (band is null ? set!["bandAssignmentId"] is not null : set!["bandAssignmentId"] is null || Number(band["id"]) != Number(set["bandAssignmentId"])) throw Invalid(); }
            if (api is 70 or 71 && Number(set!["currentBookingId"]) != Number(target["bookingId"])) throw Invalid();
            if (api == 72 && !Ids(set!, "attemptIds").Contains(Number(target["attemptId"]))) throw Invalid();
        }
        if (api == 84) { Match(Ids(v!, "existingComponents", "id"), Ids(set!, "existingComponentIds")); foreach (var c in (JsonArray)body["components"]!) if (c!["id"] is not null && !Ids(set!, "existingComponentIds").Contains(Number(c["id"]))) throw Invalid(); }
        if (api is 87 or 92 or 139 or 141 or 142 or 146 or 147) Match(Ids(v!, "components", "id"), Ids(set!, "componentIds"));
        if (api is 97 or 99 or 100 or 101 or 102 or 118 or 120 or 122 or 124 or 126 or 139 or 141 or 142 or 146 or 147)
        {
            Match(Ids(v!, "schedules", "id"), Ids(set!, "scheduleBindings", "id"));
            foreach (var b in (JsonArray)set!["scheduleBindings"]!) if (target["classId"] is not null && Number(b!["classId"]) != Number(target["classId"])) throw Invalid();
        }
        if (api == 108) { Match(Ids(v!, "classSchedules", "id"), Ids(set!, "classScheduleBindings", "id")); foreach (var b in (JsonArray)set!["classScheduleBindings"]!) if (Number(b!["classId"]) != Number(target["classId"])) throw Invalid(); }
        if (api == 101) { Match(Ids(v!, "attendances", "id"), Ids(set!, "attendanceBindings", "attendanceId")); var assignments = Ids(set!, "assignmentIds"); var schedules = Ids(set!, "scheduleBindings", "id"); foreach (var b in (JsonArray)set!["attendanceBindings"]!) if (!assignments.Contains(Number(b!["classAssignmentId"])) || !schedules.Contains(Number(b["scheduleId"]))) throw Invalid(); }
        if (api is 118 or 120 or 122 or 124) Match(Ids(set!, "assignmentIds"), Ids(set!, "cohortTimetables", "classAssignmentId"));
        if (api is 122 or 124 or 126)
        {
            var id = Number(target["scheduleId"]); if (id != Number(set!["targetScheduleId"])) throw Invalid();
            var binding = ((JsonArray)set["scheduleBindings"]!).SingleOrDefault(b => Number(b!["id"]) == id) ?? throw Invalid();
            if (binding["obligationKey"]!.GetValue<string>() != set["obligationKey"]!.GetValue<string>()) throw Invalid();
            var rv = ((JsonArray)v!["schedules"]!).Single(b => Number(b!["id"]) == id)!["version"]!.GetValue<string>(); if (rv != v["schedule"]!.GetValue<string>()) throw Invalid();
            if (api != 126 && Number(set["dependencies"]!["scheduleId"]) != id) throw Invalid();
        }
        if (api == 133)
        {
            Match(Ids(v!, "attendance", "id"), Ids(set!, "attendanceBindings", "attendanceId"));
            Delta(Ids(body, "changes", "classAssignmentId"), Ids(set!, "attendanceBindings", "classAssignmentId"), Ids(set!, "unrecordedAssignmentIds"), Ids(set!, "assignmentIds"));
        }
        if (api == 139)
        {
            if (Number(set!["classAssignmentId"]) != Number(target["classAssignmentId"])) throw Invalid();
            if (((JsonArray)body["scores"]!).Count == 0 && !body.ContainsKey("teacherComment")) throw Invalid();
            Match(Ids(v!, "scores", "id"), Ids(set!, "scoreBindings", "scoreId"));
            Delta(Ids(body, "scores", "componentId"), Ids(set!, "scoreBindings", "componentId"), Ids(set!, "unrecordedComponentIds"), Ids(set!, "componentIds"));
            var ids = Ids(set!, "scheduleBindings", "id"); foreach (var score in (JsonArray)body["scores"]!) if (score!["scheduleId"] is not null && !ids.Contains(Number(score["scheduleId"]))) throw Invalid();
        }
        if (api is 141 or 142 or 146 or 147)
        {
            Match(Ids(v!, "reports", "id"), Ids(set!, "reportBindings", "reportId")); Match(Ids(set!, "assignmentIds"), Ids(set!, "reportBindings", "classAssignmentId"));
            Match(Ids(v!, "scores", "id"), Ids(set!, "scoreBindings", "scoreId"));
            var assignments = Ids(set!, "assignmentIds"); var components = Ids(set!, "componentIds"); var schedules = Ids(set!, "scheduleBindings", "id");
            foreach (var b in (JsonArray)set!["scoreBindings"]!) if (!assignments.Contains(Number(b!["classAssignmentId"])) || !components.Contains(Number(b["componentId"])) || (b["scheduleId"] is not null && !schedules.Contains(Number(b["scheduleId"])))) throw Invalid();
            if (api == 147) { Match(Ids(v!, "registrations", "id"), Ids(set!, "registrationBindings", "registrationId")); Match(assignments, Ids(set!, "registrationBindings", "classAssignmentId")); }
        }
        CheckTimetables(body); CheckIntervals(body); if (api == 120) Weekly(body);
    }
    private static int Number(JsonNode? node) => node!.GetValue<int>();
    private static int[] Ids(JsonObject node, string name, string? key = null, bool unique = true)
    {
        var values = ((JsonArray)node[name]!).Select(n => Number(key is null ? n : n![key])).ToArray();
        if (unique && values.Distinct().Count() != values.Length) throw Invalid(); return values.Distinct().Order().ToArray();
    }
    private static void Match(int[] a, int[] b) { if (!a.SequenceEqual(b)) throw Invalid(); }
    private static void Delta(int[] changes, int[] existing, int[] absent, int[] full)
    { if (existing.Intersect(absent).Any() || changes.Except(full).Any()) throw Invalid(); Match(changes, existing.Concat(absent).Order().ToArray()); }
    private static void CheckTimetables(JsonNode? node)
    {
        if (node is JsonObject obj)
        {
            if (obj["bookings"] is JsonArray bookings && bookings.FirstOrDefault() is JsonObject booking && booking.ContainsKey("bookingId")) _ = Ids(obj, "bookings", "bookingId");
            if (obj["assignments"] is JsonArray) _ = Ids(obj, "assignments", "classAssignmentId");
            if (obj["schedules"] is JsonArray schedules && schedules.FirstOrDefault() is JsonObject schedule && schedule.ContainsKey("scheduleId")) _ = Ids(obj, "schedules", "scheduleId");
            if (obj["classSchedules"] is JsonArray classes && classes.FirstOrDefault() is JsonObject resource && resource.ContainsKey("scheduleId")) _ = Ids(obj, "classSchedules", "scheduleId");
            if (obj["entrySessions"] is JsonArray) _ = Ids(obj, "entrySessions", "sessionId");
            foreach (var p in obj) CheckTimetables(p.Value);
        }
        else if (node is JsonArray array) foreach (var item in array) CheckTimetables(item);
    }
    private static void CheckIntervals(JsonNode? node)
    {
        if (node is JsonObject obj)
        {
            if (obj["startTime"] is not null && obj["endTime"] is not null && StringComparer.Ordinal.Compare(obj["startTime"]!.GetValue<string>(), obj["endTime"]!.GetValue<string>()) >= 0) throw Invalid();
            if (obj["fromDate"] is not null && obj["toDate"] is not null && StringComparer.Ordinal.Compare(obj["fromDate"]!.GetValue<string>(), obj["toDate"]!.GetValue<string>()) > 0) throw Invalid();
            foreach (var p in obj) CheckIntervals(p.Value);
        }
        else if (node is JsonArray array) foreach (var n in array) CheckIntervals(n);
    }
    private static void Weekly(JsonObject body)
    {
        var rule = body["rule"]!; var from = StrictScalars.ParseLocalDate(rule["fromDate"]!.GetValue<string>()); var to = StrictScalars.ParseLocalDate(rule["toDate"]!.GetValue<string>());
        var weekdays = ((JsonArray)rule["weekdays"]!).Select(Number).ToHashSet(); var candidates = (JsonArray)body["candidates"]!; var index = 0;
        for (var date = from; ;)
        {
            var iso = date.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)date.DayOfWeek;
            if (weekdays.Contains(iso))
            {
                if (index >= candidates.Count) throw Invalid(); var c = candidates[index++]!;
                if (c["type"]!.GetValue<string>() != "LESSON" || c["date"]!.GetValue<string>() != date.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture) || c["startTime"]!.GetValue<string>() != rule["startTime"]!.GetValue<string>() || c["endTime"]!.GetValue<string>() != rule["endTime"]!.GetValue<string>() || c["room"]!.GetValue<string>() != rule["room"]!.GetValue<string>()) throw Invalid();
            }
            if (date == to) break; date = date.AddDays(1);
        }
        if (index != candidates.Count) throw Invalid();
    }
    private static ArgumentException Invalid() => new("Incoherent canonical bindings.");
}
