namespace EnglishCenter.Api.Common.Replay;

using System.Text.Json;
using System.Text.Json.Nodes;
using EnglishCenter.Api.Common.Serialization;
using EnglishCenter.Api.Common.Validation;

internal static class CanonicalSchemas
{
    internal sealed record Rule(Func<JsonElement, JsonNode?> Read);
    private sealed record Field(string Name, Rule Type, bool Optional = false, bool FillNull = false);
    private static Field F(string name, Rule rule) => new(name, rule);
    private static Field Optional(string name, Rule rule, bool fill = false) => new(name, rule, true, fill);
    private static Rule O(params Field[] fields) => new(value =>
    {
        if (value.ValueKind != JsonValueKind.Object) throw Invalid();
        var supplied = new Dictionary<string, JsonElement>(StringComparer.Ordinal);
        foreach (var p in value.EnumerateObject()) if (!supplied.TryAdd(p.Name, p.Value) || !fields.Any(f => f.Name == p.Name)) throw Invalid();
        var obj = new JsonObject(); foreach (var f in fields)
        {
            if (supplied.TryGetValue(f.Name, out var raw)) obj[f.Name] = f.Type.Read(raw);
            else if (!f.Optional) throw Invalid(); else if (f.FillNull) obj[f.Name] = null;
        }
        return obj;
    });
    private static Rule Nullable(Rule rule) => new(e => e.ValueKind == JsonValueKind.Null ? null : rule.Read(e));
    private static Rule Str(Func<string, string?> normalize) => new(e => e.ValueKind == JsonValueKind.String ? JsonValue.Create(normalize(e.GetString()!)) : throw Invalid());
    private static Rule Text(int max) => Str(s => NonsecretText.NormalizeRequired(s, max));
    private static Rule OptionalText(int max) => Nullable(Str(s => NonsecretText.NormalizeOptional(s, max)));
    private static Rule Int(int min, int max) => new(e => e.ValueKind == JsonValueKind.Number && !e.GetRawText().StartsWith("-", StringComparison.Ordinal) ? JsonValue.Create(StrictScalars.ParseInt32(e, min, max)) : throw Invalid());
    private static readonly Rule Id = Int(1, int.MaxValue), Null = new(e => e.ValueKind == JsonValueKind.Null ? null : throw Invalid());
    private static readonly Rule Bool = new(e => e.ValueKind is JsonValueKind.True or JsonValueKind.False ? JsonValue.Create(e.GetBoolean()) : throw Invalid());
    private static readonly Rule False = new(e => e.ValueKind == JsonValueKind.False ? JsonValue.Create(false) : throw Invalid());
    private static readonly Rule Rv = Str(s => RowVersionCodec.Encode(RowVersionCodec.Decode(s)));
    private static readonly Rule Uuid = Str(StrictScalars.NormalizeUuid);
    private static readonly Rule Score = Str(s => ExactNumbers.FormatScore(ExactNumbers.ParseScore(s)));
    private static readonly Rule Weight = Str(s => ExactNumbers.FormatWeight(ExactNumbers.ParseWeight(s)));
    private static readonly Rule Money = Str(s => ExactNumbers.FormatMoney(ExactNumbers.ParseMoney(s)));
    private static readonly Rule Date = Str(s => StrictScalars.ParseLocalDate(s).ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture));
    private static readonly Rule Time = Str(s => StrictScalars.ParseMinuteTime(s).ToString("HH:mm", System.Globalization.CultureInfo.InvariantCulture));
    private static readonly Rule Code = Str(s =>
    {
        var code = s.Trim().ToUpperInvariant(); if (code.Length is < 1 or > 30 || code.Any(c => !char.IsAsciiLetterUpper(c) && !char.IsAsciiDigit(c) && c != '_' && c != '-')) throw Invalid(); return code;
    });
    private static Rule Enum(params string[] values) => Str(s => values.Contains(s, StringComparer.Ordinal) ? s : throw Invalid());
    private static Rule A(Rule item, string identity = "", bool nonempty = false, bool ordered = false) => new(e =>
    {
        if (e.ValueKind != JsonValueKind.Array) throw Invalid(); var nodes = e.EnumerateArray().Select(item.Read).ToList(); if (nonempty && nodes.Count == 0) throw Invalid();
        if (!ordered)
        {
            var keys = new HashSet<string>(StringComparer.Ordinal);
            foreach (var n in nodes) if (!keys.Add(Key(n, identity))) throw Invalid();
            nodes.Sort((a, b) => Compare(a, b, identity));
        }
        return new JsonArray(nodes.ToArray());
    });
    private static string Key(JsonNode? n, string identity)
    {
        if (n is not JsonObject o) return n!.ToJsonString();
        var keys = identity.Length > 0 ? identity.Split(',') : o.Where(x => x.Key.EndsWith("Id", StringComparison.Ordinal) || x.Key == "id").Select(x => x.Key).Order(StringComparer.Ordinal).ToArray();
        return string.Join("|", keys.Select(k => o[k]?.ToJsonString() ?? "null"));
    }
    private static int Compare(JsonNode? a, JsonNode? b, string identity)
    {
        if (a is not JsonObject x) return a!.GetValue<int>().CompareTo(b!.GetValue<int>());
        var y = (JsonObject)b!; var primary = identity.Length > 0 ? identity.Split(',').Take(1) : Enumerable.Empty<string>();
        var keys = primary.Concat(x.Select(p => p.Key).Where(k => k.EndsWith("Id", StringComparison.Ordinal) || k == "id").Order(StringComparer.Ordinal)).Distinct();
        foreach (var k in keys) { var av = x[k]; var bv = y[k]; var c = av is null ? (bv is null ? 0 : -1) : bv is null ? 1 : av.GetValue<int>().CompareTo(bv.GetValue<int>()); if (c != 0) return c; }
        return 0;
    }
    private static readonly Rule Ref = O(F("id", Id), F("version", Rv)), Refs = A(Ref, "id"), Ids = A(Id);
    private static Rule V(params string[] names) => O(names.Select(n => F(n, Rv)).ToArray());
    private static Rule EV(params string[] names) => O(F("expectedVersions", V(names)));
    private static readonly Rule Binding = O(F("id", Id), F("classId", Id), F("obligationKey", Uuid), F("sessionType", Enum("LESSON", "FINAL_EXAM")), F("isCurrent", Bool), F("replacesScheduleId", Nullable(Id)));
    private static readonly Rule Bindings = A(Binding, "id");
    private static readonly Rule StudentTimetable = O(
        F("bookings", A(O(F("bookingId", Id), F("attemptId", Id), F("sessionId", Id), F("bookingVersion", Rv), F("sessionVersion", Rv)))),
        F("assignments", A(O(F("classAssignmentId", Id), F("classId", Id), F("classVersion", Rv)), "classAssignmentId")),
        F("schedules", A(O(F("scheduleId", Id), F("classId", Id), F("version", Rv)))));
    private static readonly Rule Resources = O(F("classSchedules", A(O(F("scheduleId", Id), F("classId", Id), F("scheduleVersion", Rv), F("classVersion", Rv)))), F("entrySessions", A(O(F("sessionId", Id), F("version", Rv)))));
    private static readonly Rule Cohorts = A(O(F("classAssignmentId", Id), F("timetable", StudentTimetable)), "classAssignmentId");
    private static readonly Rule Candidate = O(F("type", Enum("LESSON", "FINAL_EXAM")), F("date", Date), F("startTime", Time), F("endTime", Time), F("room", Code));
    private static readonly Rule Weekly = O(F("fromDate", Date), F("toDate", Date), F("weekdays", A(Int(1, 7), nonempty: true)), F("startTime", Time), F("endTime", Time), F("room", Code));
    private static Field Versions(params Field[] fields) => F("expectedVersions", O(fields));
    private static Field Set(params Field[] fields) => F("expectedSet", O(fields));
    private static Field[] ClassVersions(bool attendance = false) => [F("class", Rv), F("program", Rv), F("teacher", Rv), F("schedules", Refs), .. attendance ? new[] { F("attendances", Refs) } : Array.Empty<Field>()];
    private static Field[] ClassSet() => [F("assignmentIds", Ids), F("scheduleBindings", Bindings)];
    private static Field[] ScheduleSet(bool resources = false) => [.. ClassSet(), .. resources ? new[] { F("resources", Resources), F("cohortTimetables", Cohorts) } : Array.Empty<Field>()];
    private static Rule Cancellation(bool reason) => O([.. reason ? new[] { F("reason", Text(1000)) } : Array.Empty<Field>(), Versions(F("registration", Rv), F("bookings", Refs), F("sessions", Refs), F("invoice", Nullable(Ref))), Set(F("futureBookingIds", Ids), F("invoiceId", Nullable(Id)))]);
    private static Rule SessionMembers(bool grader)
    {
        var fields = new List<Field> { F("reason", Text(1000)) };
        var versions = new List<Field> { F("session", Rv), F("bookings", Refs), F("attempts", Refs), F("registrations", Refs) };
        var set = new List<Field> { F("scheduledBookingIds", Ids), F("bindings", A(O(F("bookingId", Id), F("attemptId", Id), F("registrationId", Id)))) };
        if (grader) { fields.Add(F("graderId", Id)); versions.Add(F("grader", Rv)); set.Add(F("currentGraderId", Id)); }
        fields.Add(Versions([.. versions])); fields.Add(Set([.. set])); return O([.. fields]);
    }
    private static Rule EntryResult(int api)
    {
        var fields = new List<Field>(); if (api != 71) fields.Add(F("score", Score)); if (api == 72) fields.Add(F("reason", Text(1000)));
        var versions = new List<Field> { F("session", Rv), F("registration", Rv), F("attempt", Rv), F("booking", Rv), F("relatedAttempts", Refs) };
        var set = new List<Field> { F("attemptIds", Ids) };
        if (api != 71) { versions.Add(F("bandAssignment", api == 70 ? Nullable(Ref) : Ref)); set.Add(F("bandAssignmentId", api == 70 ? Nullable(Id) : Id)); }
        set.Add(F("currentBookingId", api == 72 ? Null : Id)); set.Add(F("completedBookingId", api == 72 ? Id : Null));
        fields.Add(Versions([.. versions])); fields.Add(Set([.. set])); return O([.. fields]);
    }
    private static Rule Component(bool existing) => O([.. existing ? new[] { F("id", Nullable(Id)) } : Array.Empty<Field>(), F("name", Text(100)), F("kind", Enum("GENERAL", "LESSON", "FINAL_EXAM")), F("weight", Weight)]);
    private static Rule Components(bool existing) => new(e =>
    {
        if (e.ValueKind != JsonValueKind.Array) throw Invalid(); var nodes = e.EnumerateArray().Select(Component(existing).Read).Cast<JsonObject>().ToList();
        var ids = new HashSet<int>(); var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var n in nodes) { if (!names.Add(n["name"]!.GetValue<string>())) throw Invalid(); if (existing && n["id"] is not null && !ids.Add(n["id"]!.GetValue<int>())) throw Invalid(); }
        if (nodes.Count(n => n["kind"]!.GetValue<string>() == "FINAL_EXAM") > 1) throw Invalid();
        nodes.Sort((a, b) =>
        {
            if (existing) { if (a["id"] is not null && b["id"] is not null) return a["id"]!.GetValue<int>().CompareTo(b["id"]!.GetValue<int>()); if (a["id"] is not null) return -1; if (b["id"] is not null) return 1; }
            var c = StringComparer.Ordinal.Compare(a["name"]!.GetValue<string>(), b["name"]!.GetValue<string>()); if (c != 0) return c; c = StringComparer.Ordinal.Compare(a["kind"]!.GetValue<string>(), b["kind"]!.GetValue<string>()); return c != 0 ? c : StringComparer.Ordinal.Compare(a["weight"]!.GetValue<string>(), b["weight"]!.GetValue<string>());
        });
        return new JsonArray(nodes.Cast<JsonNode?>().ToArray());
    });
    private static Rule Schedule(int api)
    {
        var move = api is 122 or 124; var create = api is 118 or 120;
        var extras = new List<Field>(); if (api == 118) extras.Add(F("candidate", Candidate)); if (api == 120) { extras.Add(F("rule", Weekly)); extras.Add(F("candidates", A(Candidate, nonempty: true, ordered: true))); }
        if (move) { extras.Add(F("newDate", Date)); extras.Add(F("startTime", Time)); extras.Add(F("endTime", Time)); extras.Add(F("room", Code)); }
        if (api == 124) extras.Add(F("reason", Text(1000)));
        var versions = ClassVersions().ToList(); if (!create) versions.Add(F("schedule", Rv));
        var set = ScheduleSet(create || move).ToList(); if (!create) { set.Add(F("targetScheduleId", Id)); set.Add(F("obligationKey", Uuid)); }
        if (move) set.Add(F("dependencies", O(F("scheduleId", Id), F("hasAttendance", False), F("hasLinkedScore", False))));
        return O([.. extras, Versions([.. versions]), Set([.. set])]);
    }
    private static Rule GradeWorkflow(int api)
    {
        var versions = new List<Field> { F("class", Rv), F("program", Rv), F("teacher", Rv), F("components", Refs), F("reports", Refs), F("scores", Refs), F("schedules", Refs) };
        var set = new List<Field> { F("assignmentIds", Ids), F("componentIds", Ids), F("reportBindings", A(O(F("classAssignmentId", Id), F("reportId", Id), F("status", api is 146 or 147 ? Enum("SUBMITTED") : Enum("DRAFT", "SUBMITTED", "RETURNED", "FINALIZED"))), "classAssignmentId")), F("scoreBindings", A(O(F("classAssignmentId", Id), F("componentId", Id), F("scoreId", Id), F("scheduleId", Nullable(Id))), "classAssignmentId,componentId")), F("scheduleBindings", Bindings) };
        if (api == 147) { versions.Add(F("registrations", Refs)); set.Add(F("registrationBindings", A(O(F("classAssignmentId", Id), F("registrationId", Id)), "classAssignmentId"))); }
        return O([.. api == 146 ? new[] { F("reason", Text(1000)) } : Array.Empty<Field>(), Versions([.. versions]), Set([.. set])]);
    }
    internal static JsonObject Normalize(int api, JsonElement value)
    {
        Rule rule = api switch
        {
            8 => O(F("fullName", Text(100)), F("email", Str(ContactNormalizer.NormalizeEmail)), F("role", Enum("Admin", "Admissions", "AcademicAffairs", "Teacher", "Accountant", "Director")), Optional("phone", Nullable(Str(ContactNormalizer.NormalizeOptionalPhone)), true)),
            9 or 10 or 11 or 12 or 13 => EV("account"),
            25 => O(F("courseId", Id)),
            28 => Cancellation(false),
            44 => Cancellation(true),
            30 => O(F("targetSessionId", Id), F("attemptContext", O(F("attemptNumber", Int(1, 2)), F("attemptId", Nullable(Id)), F("attemptVersion", Nullable(Rv)), F("currentBookingId", Null))), Versions(F("registration", Rv), F("targetSession", Rv))),
            31 => EV("registration", "attempt", "booking", "sourceSession"),
            32 => O(F("targetSessionId", Id), Versions(F("registration", Rv), F("attempt", Rv), F("booking", Rv), F("sourceSession", Rv), F("targetSession", Rv))),
            36 => O(Versions(F("registration", Rv), F("bandAssignment", Rv), F("attempts", Refs), F("bookings", Refs)), Set(F("bandAssignmentId", Id), F("bestAttemptId", Id), F("attemptIds", Ids), F("scheduledBookingIds", Ids))),
            41 => O(Versions(F("registration", Rv), F("course", Rv), F("bandAssignment", Rv)), Set(F("courseId", Id), F("bandAssignmentId", Id), F("invoiceId", Null))),
            42 => O(F("reason", Text(1000)), Versions(F("registration", Rv))),
            51 => O(F("amount", Money), F("method", Enum("CASH", "TRANSFER")), Optional("transactionReference", OptionalText(100), true), Versions(F("registration", Rv), F("invoice", Rv)), Set(F("registrationId", Id), F("paymentId", Null))),
            52 => O(F("method", Enum("CASH", "TRANSFER")), F("transactionReference", OptionalText(100)), F("reason", Text(1000)), Versions(F("payment", Rv), F("invoice", Rv))),
            55 => O(F("date", Date), F("startTime", Time), F("endTime", Time), F("room", Code), F("graderId", Id), F("capacity", Int(1, 20))),
            57 => O(F("date", Date), F("startTime", Time), F("endTime", Time), F("room", Code), Versions(F("session", Rv))),
            58 or 59 => EV("session"),
            61 => SessionMembers(false),
            64 => SessionMembers(true),
            70 or 71 or 72 => EntryResult(api),
            75 => O(F("code", Code), F("name", Text(150)), F("basePrice", Money), F("status", Enum("ACTIVE", "INACTIVE"))),
            77 => O(F("name", Text(150)), F("basePrice", Money), Versions(F("course", Rv))),
            78 or 79 => EV("course"),
            82 => O(F("courseId", Id), F("bandId", Id), F("name", Text(150)), F("expectedLessonCount", Id), F("components", Components(false)), Versions(F("course", Rv), F("band", Rv))),
            84 => O(F("courseId", Id), F("bandId", Id), F("name", Text(150)), F("expectedLessonCount", Id), F("components", Components(true)), Versions(F("program", Rv), F("course", Rv), F("band", Rv), F("existingComponents", Refs)), Set(F("existingComponentIds", Ids))),
            85 => O(F("name", Text(150)), Versions(F("program", Rv))),
            87 => O(Versions(F("program", Rv), F("course", Rv), F("band", Rv), F("components", Refs)), Set(F("courseId", Id), F("bandId", Id), F("componentIds", Ids))),
            88 or 89 => EV("program"),
            92 => O(F("classCode", Code), F("programId", Id), F("teacherId", Id), F("maxSize", Int(1, 15)), Versions(F("program", Rv), F("teacher", Rv), F("components", Refs)), Set(F("componentIds", Ids))),
            94 => O(F("maxSize", Int(1, 15)), Versions(F("class", Rv)), Set(F("assignmentIds", Ids))),
            97 => O(F("teacherId", Id), Versions(F("class", Rv), F("program", Rv), F("currentTeacher", Rv), F("proposedTeacher", Rv), F("schedules", Refs)), Set([.. ClassSet(), F("hasAttendance", False), F("hasScores", False)])),
            99 or 100 => O(Versions(ClassVersions()), Set(ClassSet())),
            101 => O(Versions(ClassVersions(true)), Set([.. ClassSet(), F("attendanceBindings", A(O(F("attendanceId", Id), F("scheduleId", Id), F("classAssignmentId", Id)), "classAssignmentId,scheduleId"))])),
            102 => O(F("reason", Text(1000)), Versions(ClassVersions()), Set([.. ClassSet(), F("hasAttendance", False), F("hasScores", False)])),
            108 => O(F("registrationId", Id), Versions(F("registration", Rv), F("bandAssignment", Rv), F("invoice", Rv), F("class", Rv), F("program", Rv), F("teacher", Rv), F("classSchedules", Refs)), Set(F("bandAssignmentId", Id), F("invoiceId", Id), F("registrationAssignmentId", Null), F("classAssignmentIds", Ids), F("classScheduleBindings", Bindings), F("studentTimetable", StudentTimetable))),
            118 or 120 or 122 or 124 or 126 => Schedule(api),
            133 => O(F("changes", A(O(F("classAssignmentId", Id), F("present", Int(0, 1))), "classAssignmentId", true)), Versions(F("class", Rv), F("teacher", Rv), F("schedule", Rv), F("attendance", Refs)), Set(F("assignmentIds", Ids), F("attendanceBindings", A(O(F("classAssignmentId", Id), F("attendanceId", Id)), "classAssignmentId")), F("unrecordedAssignmentIds", Ids))),
            139 => O(F("scores", A(O(F("componentId", Id), F("score", Score), F("scheduleId", Nullable(Id))), "componentId")), Optional("teacherComment", OptionalText(1000)), Versions(F("class", Rv), F("program", Rv), F("teacher", Rv), F("report", Rv), F("components", Refs), F("scores", Refs), F("schedules", Refs)), Set(F("classAssignmentId", Id), F("reportId", Id), F("componentIds", Ids), F("scoreBindings", A(O(F("componentId", Id), F("scoreId", Id), F("scheduleId", Nullable(Id))), "componentId")), F("unrecordedComponentIds", Ids), F("scheduleBindings", Bindings))),
            141 or 142 or 146 or 147 => GradeWorkflow(api),
            _ => throw Invalid()
        };
        return (JsonObject)rule.Read(value)!;
    }
    private static ArgumentException Invalid() => new("Invalid canonical input.");
}
