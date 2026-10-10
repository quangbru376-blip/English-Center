namespace EnglishCenter.Api.Services.Enrollment;

using EnglishCenter.Api.Domain.Identity;
using EnglishCenter.Api.Domain.Academics;
using EnglishCenter.Api.Domain.Admissions;
using EnglishCenter.Api.Domain.Training;
using EnglishCenter.Api.Domain.Finance;
using EnglishCenter.Api.Services.Academics;

// Internal coherent read/locked state; none of these entities is serialized.
public sealed record EnrollmentSet(StudentProfile Student, Account Contact, IReadOnlyList<Course> Courses,
 IReadOnlyList<Registration> Registrations, IReadOnlyList<EntryTestSession> Sessions,
 IReadOnlyList<EntryTestAttempt> Attempts, IReadOnlyList<EntryTestBooking> Bookings,
 IReadOnlyList<BandAssignment> Assignments, IReadOnlyList<Band> Bands,
 IReadOnlyList<Invoice> Invoices, IReadOnlyList<StudentClassAssignment> Placements,
 IReadOnlyList<Class> Classes, IReadOnlyList<Schedule> Schedules)
{ public IReadOnlyList<GradeReport> Reports { get; init; } = []; }
public sealed record RegistrationState(EnrollmentSet Set, Registration Registration)
{
    public IReadOnlyList<EntryTestAttempt> Attempts => Set.Attempts.Where(x => x.RegistrationId == Registration.RegistrationId).OrderBy(x => x.AttemptNumber).ToArray();
    public IReadOnlyList<EntryTestBooking> Bookings => Set.Bookings.Where(x => Attempts.Any(a => a.AttemptId == x.AttemptId)).ToArray();
    public BandAssignment? Band => Set.Assignments.SingleOrDefault(x => x.RegistrationId == Registration.RegistrationId);
    public Invoice? Invoice => Set.Invoices.SingleOrDefault(x => x.RegistrationId == Registration.RegistrationId);
    public EntryTestSession Session(int id) => Set.Sessions.Single(x => x.SessionId == id);
    public EntryTestAttempt Attempt(int id) => Attempts.SingleOrDefault(x => x.AttemptId == id) ?? throw new Common.Errors.ApiErrorException(Common.Errors.ApiErrorCode.NOT_FOUND);
    public EntryTestBooking Booking(int id) => Bookings.SingleOrDefault(x => x.BookingId == id) ?? throw new Common.Errors.ApiErrorException(Common.Errors.ApiErrorCode.NOT_FOUND);
}
public sealed record OwnProfileView(int AccountId, int StudentId, string FullName, string Email, string DateOfBirth, string Phone, string? Address, object Versions);
public sealed record CourseChoice(int Id, string Code, string Name, string Tuition, string Status);
public sealed record OwnCourseReference(int Id, string Code, string Name, string Status);
public sealed record OwnBandReference(int Id, string Code);
public sealed record OwnSessionView(int Id, string StartAt, string EndAt, string Room, string Status, string EffectiveStatus, string Version);
public sealed record OwnBookingView(int Id, int RegistrationId, int AttemptId, int SessionId, int AttemptNumber, string Status, string BookedAt, string Version, OwnSessionView Session);
public sealed record OwnAttemptView(int Id, int AttemptNumber, string Version, string? Score, string? GradedAt, int? CompletedBookingId, OwnBookingView? CurrentBooking, OwnBookingView? LatestBooking);
public sealed record OwnBandAssignment(int Id, int BestAttemptId, OwnBandReference Band, string AssignedAt, string Version);
public sealed record OwnEntryContext(IReadOnlyList<OwnAttemptView> Attempts, OwnBandAssignment? BandAssignment, int? BestAttemptId);
public sealed record OwnRegistrationSummary(int Id, OwnCourseReference Course, string Status, string CreatedAt, string? AdmissionConfirmedAt, OwnBandReference? Band, int? InvoiceId, int? ClassId, string Version);
public sealed record OwnInvoiceReference(int Id, string Number, string Amount, string IssuedAt, string DueDate, string Status, bool IsOverdue);
public sealed record OwnRegistrationDetail(int Id, OwnCourseReference Course, string Status, string CreatedAt, string? AdmissionConfirmedAt, OwnBandReference? Band, int? InvoiceId, int? ClassId, string Version, OwnEntryContext Entry, OwnInvoiceReference? Invoice, object? Class, object? Rejection, object? Cancellation, string? AdmissionApprovedAt, IReadOnlyList<string> AllowedActions, IReadOnlyDictionary<string, string> DisabledReasons);
public sealed record OwnRegistrationResult(OwnRegistrationDetail Registration, Guid OperationId)
{ [System.Text.Json.Serialization.JsonIgnore] public bool Replayed { get; init; } }
public sealed record OwnBookingResult(OwnBookingView Booking, OwnEntryContext Entry, string RegistrationStatus, string RegistrationVersion, Guid OperationId)
{ [System.Text.Json.Serialization.JsonIgnore] public bool Replayed { get; init; } }
public sealed record OwnRescheduleResult(OwnBookingView OldBooking, OwnBookingView NewBooking, OwnEntryContext Entry, string RegistrationStatus, string RegistrationVersion, Guid OperationId);
public sealed record BookContext(int AttemptNumber, int? AttemptId, string? AttemptVersion, int? CurrentBookingId = null);
public sealed record EntryOption(int Id, string StartAt, string EndAt, string Room, string Status, string EffectiveStatus, string Version, int AvailableSlots);
public sealed record EntryOptions(int RegistrationId, string RegistrationVersion, bool Eligible, IReadOnlyList<string> BlockedReasons, BookContext? AttemptContext, IReadOnlyList<EntryOption> Items, int Page, int PageSize, long TotalCount);
public sealed record OwnCancellationPreview(bool Eligible, IReadOnlyList<string> BlockedReasons, string AsOfUtc, object Effects, object? Manifest);
public sealed record OwnConfirmationPreview(bool Eligible, IReadOnlyList<string> BlockedReasons, string AsOfUtc, OwnEntryContext Entry, object? Manifest);
public sealed record RelatedAttempt(int Id, int AttemptNumber, string Version, string? Score, string? GradedAt, int? CompletedBookingId);
public sealed record TeacherSessionView(int Id, string Date, string StartTime, string EndTime, string StartAt, string EndAt, string Room, string Status, string EffectiveStatus, object TaskCounts, string Version);
public sealed record TeacherSessionDetail(int Id, string Date, string StartTime, string EndTime, string StartAt, string EndAt, string Room, string Status, string EffectiveStatus, object TaskCounts, string Version, object? Cancellation);
public sealed record TeacherTaskDetail(OwnBookingView Booking, StudentContactReference Student, OwnCourseReference Course, string RegistrationStatus, string RegistrationVersion, IReadOnlyList<RelatedAttempt> RelatedAttempts, OwnBandAssignment? BandAssignment, object? TargetResult, object? Absence, object? Cancellation, IReadOnlyList<string> AllowedActions, IReadOnlyDictionary<string, string> DisabledReasons, object? GradeManifest, object? AbsenceManifest, object? CorrectionManifest);
public sealed record TeacherCommandResult(TeacherTaskDetail Task, Guid OperationId);
public sealed record TeacherRosterRow(int BookingId, int AttemptId, int RegistrationId, int AttemptNumber, string BookedAt, string BookingStatus, string RegistrationStatus, StudentContactReference Student, OwnCourseReference Course, string? TargetScore, string? TargetGradedAt, object? CurrentBand);
public sealed record StudentHomeCard(int RegistrationId, OwnCourseReference Course, string Status, string CreatedAt, string CurrentStep, object? NextAction, string? WaitingReason, object? NearestEvent, object? UnpaidInvoice, object? FinalizedReport);
