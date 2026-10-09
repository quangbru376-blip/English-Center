namespace EnglishCenter.Api.Common.Auditing;

using System.Globalization;
using System.Text.Json.Nodes;
using EnglishCenter.Api.Common.Validation;

internal sealed record AuditActionRule(string RootEntity, string Entities, bool RequiresReason, bool NewOnly);

internal static class AuditPolicy
{
    internal static readonly IReadOnlyDictionary<string,AuditActionRule> Actions = new Dictionary<string,AuditActionRule>(StringComparer.Ordinal)
    {
        ["ADMIN.STAFF.CREATE"] = new("Account", "Account", false, true),
        ["ADMIN.ACCOUNT.RESET_PASSWORD"] = new("Account", "Account", false, false),
        ["ADMIN.ACCOUNT.LOCK"] = new("Account", "Account", false, false),
        ["ADMIN.ACCOUNT.UNLOCK"] = new("Account", "Account", false, false),
        ["ADMIN.ACCOUNT.DISABLE"] = new("Account", "Account", false, false),
        ["ADMIN.ACCOUNT.RESTORE"] = new("Account", "Account", false, false),
        ["STUDENT.REGISTRATION.CREATE"] = new("Registration", "Registration EntryTestBooking Invoice", false, false),
        ["STUDENT.REGISTRATION.CANCEL"] = new("Registration", "Registration EntryTestBooking Invoice", false, false),
        ["STUDENT.REGISTRATION.CONFIRM"] = new("Registration", "Registration EntryTestBooking Invoice", false, false),
        ["ADMISSIONS.REGISTRATION.APPROVE"] = new("Registration", "Registration EntryTestBooking Invoice", false, false),
        ["ADMISSIONS.REGISTRATION.REJECT"] = new("Registration", "Registration EntryTestBooking Invoice", true, false),
        ["ADMISSIONS.REGISTRATION.CANCEL"] = new("Registration", "Registration EntryTestBooking Invoice", true, false),
        ["STUDENT.ENTRY_BOOKING.CREATE"] = new("EntryTestBooking", "EntryTestBooking EntryTestAttempt Registration", false, false),
        ["STUDENT.ENTRY_BOOKING.CANCEL"] = new("EntryTestBooking", "EntryTestBooking EntryTestAttempt Registration", false, false),
        ["STUDENT.ENTRY_BOOKING.RESCHEDULE"] = new("EntryTestBooking", "EntryTestBooking EntryTestAttempt Registration", false, false),
        ["FINANCE.INVOICE.COLLECT"] = new("Invoice", "Invoice Payment", false, false),
        ["FINANCE.PAYMENT.UPDATE_METADATA"] = new("Payment", "Payment", true, false),
        ["ACADEMIC.ENTRY_SESSION.CREATE"] = new("EntryTestSession", "EntryTestSession EntryTestBooking", false, false),
        ["ACADEMIC.ENTRY_SESSION.UPDATE_SCHEDULE"] = new("EntryTestSession", "EntryTestSession EntryTestBooking", false, false),
        ["ACADEMIC.ENTRY_SESSION.OPEN"] = new("EntryTestSession", "EntryTestSession EntryTestBooking", false, false),
        ["ACADEMIC.ENTRY_SESSION.CLOSE"] = new("EntryTestSession", "EntryTestSession EntryTestBooking", false, false),
        ["ACADEMIC.ENTRY_SESSION.CANCEL"] = new("EntryTestSession", "EntryTestSession EntryTestBooking", true, false),
        ["ACADEMIC.ENTRY_SESSION.CHANGE_GRADER"] = new("EntryTestSession", "EntryTestSession EntryTestBooking", true, false),
        ["TEACHER.ENTRY_RESULT.CREATE"] = new("EntryTestAttempt", "EntryTestAttempt EntryTestBooking BandAssignment Registration", false, false),
        ["TEACHER.ENTRY_RESULT.CORRECT"] = new("EntryTestAttempt", "EntryTestAttempt EntryTestBooking BandAssignment Registration", true, false),
        ["TEACHER.ENTRY_BOOKING.ABSENCE"] = new("EntryTestBooking", "EntryTestBooking", false, false),
        ["ACADEMIC.COURSE.CREATE"] = new("Course", "Course", false, false),
        ["ACADEMIC.COURSE.UPDATE_DETAILS"] = new("Course", "Course", false, false),
        ["ACADEMIC.COURSE.DEACTIVATE"] = new("Course", "Course", false, false),
        ["ACADEMIC.COURSE.ACTIVATE"] = new("Course", "Course", false, false),
        ["ACADEMIC.PROGRAM.CREATE"] = new("TrainingProgram", "TrainingProgram ScoreComponentConfig", false, false),
        ["ACADEMIC.PROGRAM.UPDATE_CONFIGURATION"] = new("TrainingProgram", "TrainingProgram ScoreComponentConfig", false, false),
        ["ACADEMIC.PROGRAM.RENAME"] = new("TrainingProgram", "TrainingProgram ScoreComponentConfig", false, false),
        ["ACADEMIC.PROGRAM.ACTIVATE"] = new("TrainingProgram", "TrainingProgram ScoreComponentConfig", false, false),
        ["ACADEMIC.PROGRAM.RETURN_TO_DRAFT"] = new("TrainingProgram", "TrainingProgram ScoreComponentConfig", false, false),
        ["ACADEMIC.PROGRAM.DEACTIVATE"] = new("TrainingProgram", "TrainingProgram ScoreComponentConfig", false, false),
        ["ACADEMIC.CLASS.CREATE"] = new("Class", "Class Schedule", false, false),
        ["ACADEMIC.CLASS.UPDATE_CAPACITY"] = new("Class", "Class Schedule", false, false),
        ["ACADEMIC.CLASS.CHANGE_TEACHER"] = new("Class", "Class Schedule", false, false),
        ["ACADEMIC.CLASS.OPEN"] = new("Class", "Class Schedule", false, false),
        ["ACADEMIC.CLASS.START"] = new("Class", "Class Schedule", false, false),
        ["ACADEMIC.CLASS.COMPLETE"] = new("Class", "Class Schedule", false, false),
        ["ACADEMIC.CLASS.CANCEL"] = new("Class", "Class Schedule", true, false),
        ["ACADEMIC.CLASS.PLACE_REGISTRATION"] = new("StudentClassAssignment", "StudentClassAssignment GradeReport", false, false),
        ["ACADEMIC.SCHEDULE.CREATE"] = new("Schedule", "Schedule", false, false),
        ["ACADEMIC.SCHEDULE.CREATE_WEEKLY"] = new("Schedule", "Schedule", false, false),
        ["ACADEMIC.SCHEDULE.MOVE"] = new("Schedule", "Schedule", false, false),
        ["ACADEMIC.SCHEDULE.REPLACE"] = new("Schedule", "Schedule", true, false),
        ["ACADEMIC.SCHEDULE.CONFIRM_HELD"] = new("Schedule", "Schedule", false, false),
        ["TEACHER.GRADE_DRAFT.SAVE_DELTA"] = new("GradeReport", "GradeReport Score", false, false),
        ["TEACHER.CLASS.SUBMIT_GRADES"] = new("Class", "Class GradeReport Registration", false, false),
        ["TEACHER.CLASS.RESUBMIT_GRADES"] = new("Class", "Class GradeReport Registration", false, false),
        ["ACADEMIC.CLASS.RETURN_GRADES"] = new("Class", "Class GradeReport Registration", true, false),
        ["ACADEMIC.CLASS.FINALIZE_GRADES"] = new("Class", "Class GradeReport Registration", false, false),
    };
    private static readonly IReadOnlyDictionary<string,string> Fields = new Dictionary<string,string>(StringComparer.Ordinal)
    {
        ["Account"] = "roleId status mustChangePassword tempIssuedAt tempExpiresAt securityVersion revokedSessionCount",
        ["Registration"] = "status isUnfinished admissionConfirmedAt admissionApprovedAt cancelledAt rejectedAt",
        ["EntryTestBooking"] = "attemptId sessionId status bookedAt cancelledAt cancelledBy cancelledFromStatus cancellationOperationId absentBy absentAt absentOperationId",
        ["EntryTestAttempt"] = "attemptNumber score gradedBy gradedAt completedBookingId",
        ["BandAssignment"] = "bandId minScore bestAttemptId",
        ["Invoice"] = "invoiceNumber status amount dueDate",
        ["Payment"] = "invoiceId amountPaid method transactionReference paidAt",
        ["EntryTestSession"] = "startAt endAt room capacity graderId status",
        ["Course"] = "courseCode courseName basePrice status",
        ["TrainingProgram"] = "courseId bandId programName status expectedLessonCount",
        ["ScoreComponentConfig"] = "componentName kind weight",
        ["Class"] = "programId teacherId maxSize status",
        ["StudentClassAssignment"] = "classId registrationId assignedBy assignedAt",
        ["Schedule"] = "classId sessionType sessionDate startTime endTime room obligationKey replacesScheduleId occurrenceState isCurrent heldBy heldAt notHeldBy notHeldAt replacementOperationId",
        ["GradeReport"] = "status teacherComment submittedBy submittedAt submittedOperationId reviewedBy reviewedAt reviewOperationId finalizedAt finalScore",
        ["Score"] = "componentId scheduleId scoreValue enteredBy enteredAt",
    };

    internal static string Project(string action, string entity, AuditChanges changes)
    {
        var effect = action.EndsWith(".EFFECT",StringComparison.Ordinal);
        var canonical = effect ? action[..^7] : action;
        if (!Actions.TryGetValue(canonical,out var rule) ||
            !(effect ? rule.Entities.Split(' ').Contains(entity,StringComparer.Ordinal) : entity == rule.RootEntity))
            throw Invalid();
        if (effect && (changes.Parent is null || changes.Parent.Count == 0)) throw Invalid();
        if (rule.NewOnly && changes.Old is { Count: > 0 }) throw Invalid();
        if ((!effect && rule.RequiresReason && string.IsNullOrWhiteSpace(changes.Reason)) ||
            (!rule.RequiresReason && changes.Reason is not null)) throw Invalid();
        var allowed = Fields[entity];
        if (entity == "Account") allowed = canonical == "ADMIN.STAFF.CREATE" ? "roleId status mustChangePassword tempIssuedAt tempExpiresAt" : "status mustChangePassword tempIssuedAt tempExpiresAt securityVersion revokedSessionCount";
        if (canonical == "FINANCE.PAYMENT.UPDATE_METADATA") allowed = "method transactionReference";
        if (canonical == "TEACHER.ENTRY_BOOKING.ABSENCE") allowed = "status absentBy absentAt absentOperationId";
        var details = new JsonObject();
        if (changes.Old is not null) details["old"] = ProjectFields(changes.Old,allowed);
        if (changes.New is not null) details["new"] = ProjectFields(changes.New,allowed);
        if (changes.Parent is not null)
        {
            var parentAllowed = rule.Entities switch
            {
                "Account" => "",
                "Registration EntryTestBooking Invoice" => "courseId registrationId invoiceId",
                "EntryTestBooking EntryTestAttempt Registration" => "registrationId attemptId oldBookingId newBookingId",
                "Invoice Payment" or "Payment" => "invoiceId paymentId",
                "EntryTestSession EntryTestBooking" => "sessionId",
                "EntryTestAttempt EntryTestBooking BandAssignment Registration" or "EntryTestBooking" => "registrationId bookingId attemptId",
                "Course" => "courseId",
                "TrainingProgram ScoreComponentConfig" => "programId courseId bandId",
                "Class Schedule" => "classId programId",
                "StudentClassAssignment GradeReport" => "classId registrationId classAssignmentId",
                "Schedule" => "classId oldScheduleId newScheduleId",
                "GradeReport Score" => "classId classAssignmentId gradeReportId",
                "Class GradeReport Registration" => "classId classAssignmentId registrationId",
                _ => throw Invalid()
            };
            var parent=ProjectFields(changes.Parent,parentAllowed,true);
            if (effect && parent.Count == 0) throw Invalid();
            details["parent"] = parent;
        }
        if (changes.Reason is not null) details["reason"] = NonsecretText.NormalizeRequired(changes.Reason,1000);
        return details.ToJsonString();
    }

    private static JsonObject ProjectFields(JsonObject source,string allowed,bool parent = false)
    {
        var result = new JsonObject();
        var names = allowed.Split(' ',StringSplitOptions.RemoveEmptyEntries).ToHashSet(StringComparer.Ordinal);
        foreach (var (key,node) in source)
        {
            if (!names.Contains(key)) continue;
            if (node is null) { if (parent) throw Invalid(); result[key] = null; continue; }
            if (node is not JsonValue value) throw Invalid();
            bool valid;
            if (key.EndsWith("OperationId",StringComparison.Ordinal) || key == "obligationKey") valid = value.TryGetValue<string>(out var operation) && Guid.TryParseExact(operation,"D",out var operationId) && operationId != Guid.Empty;
            else if (key.EndsWith("Id",StringComparison.Ordinal) || key.EndsWith("By",StringComparison.Ordinal) || key is "capacity" or "maxSize" or "expectedLessonCount" or "attemptNumber")
                valid = value.TryGetValue<int>(out var id) && id > 0;
            else if (key is "securityVersion" or "revokedSessionCount") valid = (value.TryGetValue<long>(out var count) && count >= 0) || (value.TryGetValue<int>(out var smallCount) && smallCount >= 0);
            else if (key is "isUnfinished" or "mustChangePassword" or "isCurrent") valid = value.TryGetValue<bool>(out _);
            else if (key is "score" or "scoreValue" or "finalScore" or "minScore" or "weight" or "basePrice" or "amount" or "amountPaid") valid = value.TryGetValue<decimal>(out _);
            else if (key.EndsWith("At",StringComparison.Ordinal)) valid = value.TryGetValue<string>(out var instant) && DateTimeOffset.TryParse(instant,CultureInfo.InvariantCulture,DateTimeStyles.None,out var at) && at.Offset == TimeSpan.Zero && at.Ticks % TimeSpan.TicksPerMillisecond == 0;
            else if (key == "status" || key == "cancelledFromStatus") valid = value.TryGetValue<string>(out var status) && "ACTIVE INACTIVE LOCKED DISABLED PENDING ADMITTED REJECTED CANCELLED CONFIRMED BOOKED COMPLETED ABSENT UNPAID PAID DRAFT OPEN CLOSED ONGOING ENDED SUBMITTED RETURNED FINALIZED".Split(' ').Contains(status,StringComparer.Ordinal);
            else if (key == "kind") valid = value.TryGetValue<string>(out var kind) && "GENERAL LESSON FINAL_EXAM".Split(' ').Contains(kind,StringComparer.Ordinal);
            else if (key == "method") valid = value.TryGetValue<string>(out var method) && "CASH TRANSFER".Split(' ').Contains(method,StringComparer.Ordinal);
            else if (key == "sessionType") valid = value.TryGetValue<string>(out var sessionType) && "LESSON FINAL_EXAM".Split(' ').Contains(sessionType,StringComparer.Ordinal);
            else if (key == "occurrenceState") valid = value.TryGetValue<string>(out var state) && "PLANNED HELD NOT_HELD CANCELLED".Split(' ').Contains(state,StringComparer.Ordinal);
            else valid = value.TryGetValue<string>(out var text) && text.EnumerateRunes().Count() <= (key == "transactionReference" ? 100 : 1000);
            if (!valid) throw Invalid();
            result[key] = node.DeepClone();
        }
        return result;
    }
    private static ArgumentException Invalid() => new("Invalid audit projection.");
}
