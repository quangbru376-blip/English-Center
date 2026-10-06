namespace EnglishCenter.Api.Common.Replay;

using System.Collections.ObjectModel;

public sealed class CanonicalAction
{
    public int ApiId { get; }
    public string Code { get; }
    public string Method { get; }
    public string Path { get; }
    public string Role { get; }
    public string Transaction { get; }
    public IReadOnlyList<string> TargetKeys { get; }
    public IReadOnlyList<string> ResultKeys { get; }
    public int LookupApiId { get; }
    public string NoOpGroup { get; }
    public string ResultMode { get; }
    internal CanonicalAction(int api, string code, string method, string path, string role, string transaction, string targets, string results, int lookup, string noOp, string mode)
    {
        ApiId = api; Code = code; Method = method; Path = path; Role = role; Transaction = transaction;
        TargetKeys = Array.AsReadOnly(targets.Split(',', StringSplitOptions.RemoveEmptyEntries));
        ResultKeys = Array.AsReadOnly(results.Split(',', StringSplitOptions.RemoveEmptyEntries));
        LookupApiId = lookup; NoOpGroup = noOp; ResultMode = mode;
    }
}
public static class CanonicalActionRegistry
{
    public static IReadOnlyList<CanonicalAction> All { get; } = Array.AsReadOnly<CanonicalAction>([
        new(8, "ADMIN.STAFF.CREATE", "POST", "/admin/staff-accounts", "Admin", "COMMAND; TX01; UUID", "", "accountId", 14, "C", "SECRET_RESULT_UNAVAILABLE"),
        new(9, "ADMIN.ACCOUNT.RESET_PASSWORD", "POST", "/admin/accounts/{id}/reset-password", "Admin", "COMMAND; TX13; UUID", "accountId", "accountId", 14, "K", "SECRET_RESULT_UNAVAILABLE"),
        new(10, "ADMIN.ACCOUNT.LOCK", "POST", "/admin/accounts/{id}/lock", "Admin", "COMMAND; TX13; UUID", "accountId", "accountId", 14, "S", "SAFE_REFERENCE"),
        new(11, "ADMIN.ACCOUNT.UNLOCK", "POST", "/admin/accounts/{id}/unlock", "Admin", "COMMAND; TX13; UUID", "accountId", "accountId", 14, "S", "SAFE_REFERENCE"),
        new(12, "ADMIN.ACCOUNT.DISABLE", "POST", "/admin/accounts/{id}/disable", "Admin", "COMMAND; TX13; UUID", "accountId", "accountId", 14, "S", "SAFE_REFERENCE"),
        new(13, "ADMIN.ACCOUNT.RESTORE", "POST", "/admin/accounts/{id}/restore", "Admin", "COMMAND; TX13; UUID", "accountId", "accountId", 14, "S", "SAFE_REFERENCE"),
        new(25, "STUDENT.REGISTRATION.CREATE", "POST", "/registrations", "Student", "COMMAND; W05 atomic; UUID", "", "registrationId", 37, "C", "SAFE_REFERENCE"),
        new(28, "STUDENT.REGISTRATION.CANCEL", "POST", "/registrations/{rId}/cancel", "Student", "COMMAND; TX06; UUID", "registrationId", "registrationId", 37, "S", "SAFE_REFERENCE"),
        new(30, "STUDENT.ENTRY_BOOKING.CREATE", "POST", "/registrations/{rId}/entry-test-bookings", "Student", "COMMAND; TX02; UUID", "registrationId", "registrationId,bookingId", 37, "C", "SAFE_REFERENCE"),
        new(31, "STUDENT.ENTRY_BOOKING.CANCEL", "POST", "/registrations/{rId}/entry-test-bookings/{bId}/cancel", "Student", "COMMAND; TX02; UUID", "registrationId,bookingId", "registrationId,bookingId", 37, "S", "SAFE_REFERENCE"),
        new(32, "STUDENT.ENTRY_BOOKING.RESCHEDULE", "POST", "/registrations/{rId}/entry-test-bookings/{bId}/reschedule", "Student", "COMMAND; TX02; UUID", "registrationId,bookingId", "registrationId,oldBookingId,newBookingId", 37, "P", "SAFE_REFERENCE"),
        new(36, "STUDENT.REGISTRATION.CONFIRM", "POST", "/registrations/{rId}/confirm", "Student", "COMMAND; W09 atomic; UUID", "registrationId", "registrationId", 37, "S", "SAFE_REFERENCE"),
        new(41, "ADMISSIONS.REGISTRATION.APPROVE", "POST", "/admissions/registrations/{id}/approve", "Admissions", "COMMAND; TX04; UUID", "registrationId", "registrationId,invoiceId", 45, "S", "SAFE_REFERENCE"),
        new(42, "ADMISSIONS.REGISTRATION.REJECT", "POST", "/admissions/registrations/{id}/reject", "Admissions", "COMMAND; W10 atomic; UUID", "registrationId", "registrationId", 45, "S", "SAFE_REFERENCE"),
        new(44, "ADMISSIONS.REGISTRATION.CANCEL", "POST", "/admissions/registrations/{id}/cancel", "Admissions", "COMMAND; TX06; UUID", "registrationId", "registrationId,invoiceId*", 45, "S", "SAFE_REFERENCE"),
        new(51, "FINANCE.INVOICE.COLLECT", "POST", "/finance/invoices/{id}/collect", "Accountant", "COMMAND; TX05; UUID", "invoiceId", "invoiceId,paymentId", 53, "C", "SAFE_REFERENCE"),
        new(52, "FINANCE.PAYMENT.UPDATE_METADATA", "PUT", "/finance/payments/{id}/metadata", "Accountant", "COMMAND; W12 atomic metadata; UUID", "paymentId", "invoiceId,paymentId", 53, "P", "SAFE_REFERENCE"),
        new(55, "ACADEMIC.ENTRY_SESSION.CREATE", "POST", "/admissions/entry-test-sessions", "AcademicAffairs", "COMMAND; W06 atomic; UUID", "", "sessionId", 45, "C", "SAFE_REFERENCE"),
        new(57, "ACADEMIC.ENTRY_SESSION.UPDATE_SCHEDULE", "PUT", "/admissions/entry-test-sessions/{sId}/schedule", "AcademicAffairs", "COMMAND; W06 atomic; UUID", "sessionId", "sessionId", 45, "P", "SAFE_REFERENCE"),
        new(58, "ACADEMIC.ENTRY_SESSION.OPEN", "POST", "/admissions/entry-test-sessions/{sId}/open", "AcademicAffairs", "COMMAND; W06 atomic; UUID", "sessionId", "sessionId", 45, "S", "SAFE_REFERENCE"),
        new(59, "ACADEMIC.ENTRY_SESSION.CLOSE", "POST", "/admissions/entry-test-sessions/{sId}/close", "AcademicAffairs", "COMMAND; W06 atomic; UUID", "sessionId", "sessionId", 45, "S", "SAFE_REFERENCE"),
        new(61, "ACADEMIC.ENTRY_SESSION.CANCEL", "POST", "/admissions/entry-test-sessions/{sId}/cancel", "AcademicAffairs", "COMMAND; W06 atomic; UUID", "sessionId", "sessionId", 45, "S", "SAFE_REFERENCE"),
        new(64, "ACADEMIC.ENTRY_SESSION.CHANGE_GRADER", "POST", "/admissions/entry-test-sessions/{sId}/change-grader", "AcademicAffairs", "COMMAND; W06 atomic; UUID", "sessionId", "sessionId", 45, "P", "SAFE_REFERENCE"),
        new(70, "TEACHER.ENTRY_RESULT.CREATE", "POST", "/admissions/teachers/me/entry-test-bookings/{bId}/score", "Teacher", "COMMAND; TX03; UUID", "bookingId", "bookingId,attemptId,registrationId", 73, "C", "SAFE_REFERENCE"),
        new(71, "TEACHER.ENTRY_BOOKING.ABSENCE", "POST", "/admissions/teachers/me/entry-test-bookings/{bId}/absence", "Teacher", "COMMAND; TX03; UUID", "bookingId", "bookingId,attemptId,registrationId", 73, "S", "SAFE_REFERENCE"),
        new(72, "TEACHER.ENTRY_RESULT.CORRECT", "PUT", "/admissions/teachers/me/entry-test-attempts/{tId}/score", "Teacher", "COMMAND; TX03; UUID", "attemptId", "bookingId,attemptId,registrationId", 73, "P", "SAFE_REFERENCE"),
        new(75, "ACADEMIC.COURSE.CREATE", "POST", "/academics/courses", "AcademicAffairs", "COMMAND; W13 atomic / config gates; UUID", "", "courseId", 90, "C", "SAFE_REFERENCE"),
        new(77, "ACADEMIC.COURSE.UPDATE_DETAILS", "PUT", "/academics/courses/{cId}/details", "AcademicAffairs", "COMMAND; W13 atomic / config gates; UUID", "courseId", "courseId", 90, "P", "SAFE_REFERENCE"),
        new(78, "ACADEMIC.COURSE.DEACTIVATE", "POST", "/academics/courses/{cId}/deactivate", "AcademicAffairs", "COMMAND; W13 atomic / config gates; UUID", "courseId", "courseId", 90, "S", "SAFE_REFERENCE"),
        new(79, "ACADEMIC.COURSE.ACTIVATE", "POST", "/academics/courses/{cId}/activate", "AcademicAffairs", "COMMAND; W13 atomic / config gates; UUID", "courseId", "courseId", 90, "S", "SAFE_REFERENCE"),
        new(82, "ACADEMIC.PROGRAM.CREATE", "POST", "/academics/training-programs", "AcademicAffairs", "COMMAND; W13 atomic / config gates; UUID", "", "programId", 90, "C", "SAFE_REFERENCE"),
        new(84, "ACADEMIC.PROGRAM.UPDATE_CONFIGURATION", "PUT", "/academics/training-programs/{pId}/configuration", "AcademicAffairs", "COMMAND; W13 atomic / config gates; UUID", "programId", "programId", 90, "P", "SAFE_REFERENCE"),
        new(85, "ACADEMIC.PROGRAM.RENAME", "PUT", "/academics/training-programs/{pId}/name", "AcademicAffairs", "COMMAND; W13 atomic / config gates; UUID", "programId", "programId", 90, "P", "SAFE_REFERENCE"),
        new(87, "ACADEMIC.PROGRAM.ACTIVATE", "POST", "/academics/training-programs/{pId}/activate", "AcademicAffairs", "COMMAND; W13 atomic / config gates; UUID", "programId", "programId", 90, "S", "SAFE_REFERENCE"),
        new(88, "ACADEMIC.PROGRAM.RETURN_TO_DRAFT", "POST", "/academics/training-programs/{pId}/return-to-draft", "AcademicAffairs", "COMMAND; W13 atomic / config gates; UUID", "programId", "programId", 90, "S", "SAFE_REFERENCE"),
        new(89, "ACADEMIC.PROGRAM.DEACTIVATE", "POST", "/academics/training-programs/{pId}/deactivate", "AcademicAffairs", "COMMAND; W13 atomic / config gates; UUID", "programId", "programId", 90, "S", "SAFE_REFERENCE"),
        new(92, "ACADEMIC.CLASS.CREATE", "POST", "/academics/classes", "AcademicAffairs", "COMMAND; TX07; UUID", "", "classId", 90, "C", "SAFE_REFERENCE"),
        new(94, "ACADEMIC.CLASS.UPDATE_CAPACITY", "PUT", "/academics/classes/{cId}/capacity", "AcademicAffairs", "COMMAND; W14 atomic; UUID", "classId", "classId", 90, "P", "SAFE_REFERENCE"),
        new(97, "ACADEMIC.CLASS.CHANGE_TEACHER", "POST", "/academics/classes/{cId}/change-teacher", "AcademicAffairs", "COMMAND; W14 atomic; UUID", "classId", "classId", 90, "P", "SAFE_REFERENCE"),
        new(99, "ACADEMIC.CLASS.OPEN", "POST", "/academics/classes/{cId}/open", "AcademicAffairs", "COMMAND; W14 atomic; UUID", "classId", "classId", 90, "S", "SAFE_REFERENCE"),
        new(100, "ACADEMIC.CLASS.START", "POST", "/academics/classes/{cId}/start", "AcademicAffairs", "COMMAND; W14 atomic; UUID", "classId", "classId", 90, "S", "SAFE_REFERENCE"),
        new(101, "ACADEMIC.CLASS.COMPLETE", "POST", "/academics/classes/{cId}/complete", "AcademicAffairs", "COMMAND; W14 atomic; UUID", "classId", "classId", 90, "S", "SAFE_REFERENCE"),
        new(102, "ACADEMIC.CLASS.CANCEL", "POST", "/academics/classes/{cId}/cancel", "AcademicAffairs", "COMMAND; W14 atomic; UUID", "classId", "classId", 90, "S", "SAFE_REFERENCE"),
        new(108, "ACADEMIC.CLASS.PLACE_REGISTRATION", "POST", "/academics/classes/{cId}/assignments", "AcademicAffairs", "COMMAND; TX08; UUID", "classId", "classId,registrationId,classAssignmentId", 90, "C", "SAFE_REFERENCE"),
        new(118, "ACADEMIC.SCHEDULE.CREATE", "POST", "/academics/classes/{cId}/schedules", "AcademicAffairs", "COMMAND; W16 atomic / same Schedule protocol; UUID", "classId", "classId,scheduleId", 90, "C", "SAFE_REFERENCE"),
        new(120, "ACADEMIC.SCHEDULE.CREATE_WEEKLY", "POST", "/academics/classes/{cId}/weekly-schedules", "AcademicAffairs", "COMMAND; TX09; UUID", "classId", "classId", 90, "C", "SAFE_REFERENCE"),
        new(122, "ACADEMIC.SCHEDULE.MOVE", "PUT", "/academics/schedules/{sId}/time-room", "AcademicAffairs", "COMMAND; W16 atomic / same Schedule protocol; UUID", "scheduleId", "classId,scheduleId", 90, "P", "SAFE_REFERENCE"),
        new(124, "ACADEMIC.SCHEDULE.REPLACE", "POST", "/academics/schedules/{sId}/replace", "AcademicAffairs", "COMMAND; TX09; UUID", "scheduleId", "classId,oldScheduleId,newScheduleId", 90, "C", "SAFE_REFERENCE"),
        new(126, "ACADEMIC.SCHEDULE.CONFIRM_HELD", "POST", "/academics/schedules/{sId}/confirm-held", "AcademicAffairs", "COMMAND; W16 atomic / same Schedule protocol; UUID", "scheduleId", "classId,scheduleId", 90, "S", "SAFE_REFERENCE"),
        new(133, "TEACHER.ATTENDANCE.SAVE_DELTA", "PUT", "/academics/teachers/me/classes/{cId}/lessons/{sId}/attendance", "Teacher", "COMMAND; TX10; UUID", "classId,scheduleId", "classId,scheduleId", 90, "G", "SAFE_REFERENCE"),
        new(139, "TEACHER.GRADE_DRAFT.SAVE_DELTA", "PUT", "/academics/teachers/me/classes/{cId}/assignments/{aId}/grade-draft", "Teacher", "COMMAND; TX11; UUID", "classId,classAssignmentId", "classId,classAssignmentId", 90, "G", "SAFE_REFERENCE"),
        new(141, "TEACHER.CLASS.SUBMIT_GRADES", "POST", "/academics/teachers/me/classes/{id}/submit-grades", "Teacher", "COMMAND; TX12; UUID", "classId", "classId", 90, "S", "SAFE_REFERENCE"),
        new(142, "TEACHER.CLASS.RESUBMIT_GRADES", "POST", "/academics/teachers/me/classes/{id}/resubmit-grades", "Teacher", "COMMAND; TX12; UUID", "classId", "classId", 90, "S", "SAFE_REFERENCE"),
        new(146, "ACADEMIC.CLASS.RETURN_GRADES", "POST", "/academics/grade-reviews/classes/{id}/return", "AcademicAffairs", "COMMAND; TX12; UUID", "classId", "classId", 90, "S", "SAFE_REFERENCE"),
        new(147, "ACADEMIC.CLASS.FINALIZE_GRADES", "POST", "/academics/grade-reviews/classes/{id}/finalize", "AcademicAffairs", "COMMAND; TX12; UUID", "classId", "classId", 90, "S", "SAFE_REFERENCE"),
    ]);
    public static CanonicalAction ForApi(int api) => All.SingleOrDefault(a => a.ApiId == api) ?? throw new ArgumentException("Unknown canonical API.");
    public static CanonicalAction ForCode(string code) => All.SingleOrDefault(a => string.Equals(a.Code, code, StringComparison.Ordinal)) ?? throw new ArgumentException("Unknown canonical action.");
    public static bool AllowsLookup(int lookupApi, string role, string code) => All.Any(a => a.LookupApiId == lookupApi && a.Role == role && a.Code == code);
}
