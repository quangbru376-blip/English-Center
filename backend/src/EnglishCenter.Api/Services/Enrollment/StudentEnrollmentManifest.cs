namespace EnglishCenter.Api.Services.Enrollment;

using System.Text.Json;
using EnglishCenter.Api.Services.Academics;

public static class StudentEnrollmentManifest
{
    public static void Booking(RegistrationState state, JsonElement body, int api, int booking)
    {
        var v = body.GetProperty("expectedVersions");
        AcademicGuard.Version(v.GetProperty("registration"), state.Registration.RowVersion);
        if (api == 30)
        {
            var current = EnrollmentProjection.BookAttempt(state) ?? throw AcademicGuard.Conflict();
            var supplied = body.GetProperty("attemptContext");
            if (supplied.GetProperty("attemptNumber").GetInt32() != current.AttemptNumber ||
                Id(supplied.GetProperty("attemptId")) != current.AttemptId ||
                supplied.GetProperty("attemptVersion").GetString() != current.AttemptVersion ||
                supplied.GetProperty("currentBookingId").ValueKind != JsonValueKind.Null) throw AcademicGuard.Conflict();
        }
        else
        {
            var b = state.Booking(booking);
            AcademicGuard.Version(v.GetProperty("attempt"), state.Attempt(b.AttemptId).RowVersion);
            AcademicGuard.Version(v.GetProperty("booking"), b.RowVersion);
            AcademicGuard.Version(v.GetProperty("sourceSession"), state.Session(b.SessionId).RowVersion);
        }
        if (api is 30 or 32) AcademicGuard.Version(v.GetProperty("targetSession"), state.Session(body.GetProperty("targetSessionId").GetInt32()).RowVersion);
    }

    public static void Cancellation(RegistrationState state, JsonElement body, DateTimeOffset now)
    {
        var v = body.GetProperty("expectedVersions"); var set = body.GetProperty("expectedSet");
        AcademicGuard.Version(v.GetProperty("registration"), state.Registration.RowVersion);
        var future = EnrollmentProjection.Future(state, now);
        AcademicGuard.Set(set.GetProperty("futureBookingIds"), future.Select(x => x.BookingId));
        AcademicGuard.Versions(v.GetProperty("bookings"), future.Select(x => new VersionedId(x.BookingId, EnrollmentProjection.Rv(x.RowVersion))));
        AcademicGuard.Versions(v.GetProperty("sessions"), future.Select(x => state.Session(x.SessionId)).DistinctBy(x => x.SessionId).Select(x => new VersionedId(x.SessionId, EnrollmentProjection.Rv(x.RowVersion))));
        var invoice = v.GetProperty("invoice");
        if (Id(set.GetProperty("invoiceId")) != state.Invoice?.InvoiceId || (invoice.ValueKind == JsonValueKind.Null) != (state.Invoice is null)) throw AcademicGuard.Conflict();
        if (state.Invoice is { } i)
        {
            if (invoice.GetProperty("id").GetInt32() != i.InvoiceId) throw AcademicGuard.Conflict();
            AcademicGuard.Version(invoice.GetProperty("version"), i.RowVersion);
        }
    }

    public static void Confirmation(RegistrationState state, JsonElement body)
    {
        var v = body.GetProperty("expectedVersions"); var set = body.GetProperty("expectedSet");
        var band = state.Band ?? throw AcademicGuard.Conflict();
        AcademicGuard.Version(v.GetProperty("registration"), state.Registration.RowVersion);
        AcademicGuard.Version(v.GetProperty("bandAssignment"), band.RowVersion);
        AcademicGuard.Set(set.GetProperty("attemptIds"), state.Attempts.Select(x => x.AttemptId));
        AcademicGuard.Set(set.GetProperty("scheduledBookingIds"), state.Bookings.Where(x => x.Status == "SCHEDULED").Select(x => x.BookingId));
        AcademicGuard.Versions(v.GetProperty("attempts"), state.Attempts.Select(x => new VersionedId(x.AttemptId, EnrollmentProjection.Rv(x.RowVersion))));
        AcademicGuard.Versions(v.GetProperty("bookings"), state.Bookings.Where(x => x.Status == "SCHEDULED").Select(x => new VersionedId(x.BookingId, EnrollmentProjection.Rv(x.RowVersion))));
        if (set.GetProperty("bandAssignmentId").GetInt32() != band.BandAssignmentId || set.GetProperty("bestAttemptId").GetInt32() != band.BestAttemptId) throw AcademicGuard.Conflict();
    }

    private static int? Id(JsonElement value) => value.ValueKind == JsonValueKind.Null ? null : value.GetInt32();
}
