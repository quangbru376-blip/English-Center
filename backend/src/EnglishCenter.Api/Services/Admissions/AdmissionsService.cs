namespace EnglishCenter.Api.Services.Admissions;

using System.Text.Json.Nodes;
using EnglishCenter.Api.Common.Auditing;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Common.Security;
using EnglishCenter.Api.Common.Time;
using EnglishCenter.Api.Domain.Admissions;
using EnglishCenter.Api.Domain.Finance;
using EnglishCenter.Api.Infrastructure.Admissions;
using EnglishCenter.Api.Infrastructure.Enrollment;
using EnglishCenter.Api.Services.Academics;
using EnglishCenter.Api.Services.Auth;
using EnglishCenter.Api.Services.Enrollment;
using EnglishCenter.Api.Services.Replay;

public sealed class AdmissionsService(SqlAdmissionsRepository repository, IReplayRepository markers, BusinessClock clock)
{
    public async Task<AdmissionsCommandResult> CommandAsync(VerifiedSessionToken token, AdmissionsInput input, CancellationToken ct)
    {
        var shared = repository.Shared;
        await markers.AcquireAsync(ReplayService.RequestResource(token.AccountId, input.Intent.Action, input.RequestId), ct);
        await repository.RequireAsync(input.RegistrationId, ct);
        var resources = await shared.ResourcesAsync(input.RegistrationId, null, true, ct);
        var gates = new List<string> { "EC:REG:" + input.RegistrationId };
        if (input.Api == 44) gates.Add("EC:TIMETABLE");
        if (resources.Invoice is { } invoice) gates.Add("EC:INVOICE:" + invoice);
        gates.AddRange(resources.Classes.Select(x => "EC:CLASS:" + x));
        foreach (var gate in gates.Order(StringComparer.Ordinal)) await markers.AcquireAsync(gate, ct);
        var studentAccount = await shared.StudentAccountAsync(input.RegistrationId, ct);
        await shared.PrepareAccountsAsync(token, resources.Graders.Append(studentAccount), ct);
        await EnrollmentAuthority.RequireAsync(shared, token, clock, "Admissions", ct);
        var state = await shared.LockStateAsync(input.RegistrationId, [], false, ct, true);
        SqlEnrollmentRepository.RequireResources(state, resources, true);
        var replay = new ReplayService(markers, clock);
        var repeated = await replay.BeginAsync(token.AccountId, input.RequestId, input.Intent, async (stored, c) =>
        {
            await EnrollmentAuthority.RequireAsync(shared, token, clock, "Admissions", c);
            await repository.RequireAsync(stored.Target["registrationId"], c);
            return stored.Target["registrationId"] == input.RegistrationId && (!stored.Target.TryGetValue("invoiceId", out var id) || state.Invoice?.InvoiceId == id);
        }, ct);
        if (repeated is not null) return await Result(token, input, repeated.OperationId, true, ct);
        await shared.RequireUnfinishedAsync(state.Registration, ct);
        var operation = Guid.NewGuid(); var now = clock.GetUtcNow(); var before = Registration(state.Registration);
        var refs = new Dictionary<string, int> { ["registrationId"] = input.RegistrationId };
        var reason = input.Api == 41 ? null : input.Body.GetProperty("reason").GetString();
        if (input.Api == 41) await Approve(token, state, input, now, operation, refs, ct);
        else if (input.Api == 42)
        {
            AcademicGuard.Version(input.Body.GetProperty("expectedVersions").GetProperty("registration"), state.Registration.RowVersion);
            if (!AdmissionsProjection.CanDecide(state) || await repository.PaymentAsync(input.RegistrationId, ct) ||
                state.Band!.BandId != await shared.BandForScoreAsync(TeacherTaskProjection.BestCompleted(state).Score!.Value, ct)) throw AcademicGuard.Conflict();
            var r = state.Registration; r.RejectedFromStatus = r.Status; r.Status = "REJECTED"; r.IsUnfinished = false;
            r.RejectedBy = token.AccountId; r.RejectedAt = now.UtcDateTime; r.RejectionReason = reason; r.RejectionOperationId = operation;
            await shared.SaveAsync(ct);
        }
        else await Cancel(token, state, input, now, operation, reason!, refs, ct);
        await shared.AuditAsync(AuditEntry.Mandatory(input.Intent.Action.Code, token.AccountId, "Registration", input.RegistrationId, operation, now,
            new(before, Registration(state.Registration), Reason: reason)), ct);
        await replay.CompleteAsync(token.AccountId, input.RequestId, input.Intent, operation, refs, ct);
        await new CurrentSessionService(shared, clock).TouchApprovedInteractiveActivityAsync(token, ct);
        return await Result(token, input, operation, false, ct);
    }

    private async Task Approve(VerifiedSessionToken token, RegistrationState s, AdmissionsInput input, DateTimeOffset now, Guid operation, Dictionary<string, int> refs, CancellationToken ct)
    {
        var body = input.Body; var v = body.GetProperty("expectedVersions"); var set = body.GetProperty("expectedSet"); var course = s.Set.Courses.Single();
        AcademicGuard.Version(v.GetProperty("registration"), s.Registration.RowVersion); AcademicGuard.Version(v.GetProperty("course"), course.RowVersion);
        if (!AdmissionsProjection.CanDecide(s) || await repository.PaymentAsync(input.RegistrationId, ct)) throw AcademicGuard.Conflict();
        AcademicGuard.Version(v.GetProperty("bandAssignment"), s.Band!.RowVersion);
        if (set.GetProperty("courseId").GetInt32() != course.CourseId || set.GetProperty("bandAssignmentId").GetInt32() != s.Band.BandAssignmentId ||
            course.BasePrice <= 0 || s.Band.BandId != await repository.Shared.BandForScoreAsync(TeacherTaskProjection.BestCompleted(s).Score!.Value, ct)) throw AcademicGuard.Conflict();
        var today = DateOnly.FromDateTime(now.UtcDateTime.AddHours(7));
        var invoice = new Invoice { RegistrationId = input.RegistrationId, InvoiceNumber = await repository.AllocateAsync((short)today.Year, ct), Amount = course.BasePrice,
            IssuedAt = now.UtcDateTime, DueDate = today.AddDays(7), Status = "UNPAID" };
        repository.AddInvoice(invoice); s.Registration.Status = "ADMITTED"; s.Registration.AdmissionApprovedBy = token.AccountId; s.Registration.AdmissionApprovedAt = now.UtcDateTime;
        await repository.Shared.SaveAsync(ct); refs["invoiceId"] = invoice.InvoiceId;
        await repository.Shared.AuditAsync(AuditEntry.Mandatory(input.Intent.Action.Code + ".EFFECT", token.AccountId, "Invoice", invoice.InvoiceId, operation, now,
            new(null, new JsonObject { ["invoiceNumber"] = invoice.InvoiceNumber, ["amount"] = invoice.Amount, ["status"] = invoice.Status, ["dueDate"] = invoice.DueDate.ToString("yyyy-MM-dd") },
                new JsonObject { ["registrationId"] = input.RegistrationId, ["invoiceId"] = invoice.InvoiceId })), ct);
    }

    private async Task Cancel(VerifiedSessionToken token, RegistrationState s, AdmissionsInput input, DateTimeOffset now, Guid operation, string reason, Dictionary<string, int> refs, CancellationToken ct)
    {
        StudentEnrollmentManifest.Cancellation(s, input.Body, now);
        if (!AdmissionsProjection.CanCancel(s, await repository.PaymentAsync(input.RegistrationId, ct))) throw AcademicGuard.Conflict();
        var future = EnrollmentProjection.Future(s, now); var r = s.Registration;
        var previous = future.Select(b => (Booking: b, Before: new JsonObject { ["status"] = b.Status })).ToArray();
        r.CancelledFromStatus = r.Status; r.Status = "CANCELLED"; r.IsUnfinished = false; r.CancelledBy = token.AccountId;
        r.CancelledAt = now.UtcDateTime; r.CancellationReason = reason; r.CancellationOperationId = operation;
        foreach (var b in future) { b.CancelledFromStatus = b.Status; b.Status = "CANCELLED"; b.CancelledBy = token.AccountId; b.CancelledAt = now.UtcDateTime; b.CancellationOperationId = operation; }
        if (s.Invoice is { } i) { i.CancelledFromStatus = i.Status; i.Status = "CANCELLED"; i.CancelledBy = token.AccountId; i.CancelledAt = now.UtcDateTime; i.CancellationReason = reason; i.CancellationOperationId = operation; refs["invoiceId"] = i.InvoiceId; }
        await repository.Shared.SaveAsync(ct);
        var parent = new JsonObject { ["registrationId"] = input.RegistrationId };
        foreach (var (b, before) in previous) await repository.Shared.AuditAsync(AuditEntry.Mandatory(input.Intent.Action.Code + ".EFFECT", token.AccountId, "EntryTestBooking", b.BookingId, operation, now,
            new(before, new JsonObject { ["status"] = b.Status }, parent, reason)), ct);
        if (s.Invoice is { } invoice) await repository.Shared.AuditAsync(AuditEntry.Mandatory(input.Intent.Action.Code + ".EFFECT", token.AccountId, "Invoice", invoice.InvoiceId, operation, now,
            new(new JsonObject { ["status"] = "UNPAID" }, new JsonObject { ["status"] = invoice.Status }, parent, reason)), ct);
    }

    private async Task<AdmissionsCommandResult> Result(VerifiedSessionToken token, AdmissionsInput input, Guid operation, bool replayed, CancellationToken ct)
    {
        var state = await repository.CurrentAsync(input.RegistrationId, ct);
        var projected = AdmissionsProjection.Detail(state, clock.GetUtcNow(), await repository.PaymentAsync(input.RegistrationId, ct));
        var authority = await EnrollmentAuthority.RequireAsync(repository.Shared, token, clock, "Admissions", ct);
        if (!clock.IsBefore(authority.EffectiveDeadline)) throw new ApiErrorException(ApiErrorCode.AUTHENTICATION_REQUIRED);
        if (!replayed && input.Api == 44 && input.Body.GetProperty("expectedSet").GetProperty("futureBookingIds").EnumerateArray()
            .Any(x => state.Session(state.Booking(x.GetInt32()).SessionId).StartAt <= clock.GetUtcNow().UtcDateTime)) throw AcademicGuard.Conflict();
        return new(projected, operation);
    }
    private static JsonObject Registration(Registration r) => new() { ["status"] = r.Status, ["isUnfinished"] = r.IsUnfinished, ["admissionApprovedAt"] = EnrollmentProjection.At(r.AdmissionApprovedAt), ["rejectedAt"] = EnrollmentProjection.At(r.RejectedAt), ["cancelledAt"] = EnrollmentProjection.At(r.CancelledAt) };
}
