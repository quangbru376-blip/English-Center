namespace EnglishCenter.Api.Services.Enrollment;

using EnglishCenter.Api.Services.Academics;

public static class StudentHomeProjection
{
    public static StudentHomeCard Home(RegistrationState state, DateTimeOffset now)
    {
        var r = state.Registration; var detail = EnrollmentProjection.Detail(state, now);
        var placement = state.Set.Placements.SingleOrDefault(x => x.RegistrationId == r.RegistrationId);
        object? published = null;
        if (placement is not null && state.Set.Reports.SingleOrDefault(x => x.ClassAssignmentId == placement.ClassAssignmentId && x.Status == "FINALIZED") is { } report)
            published = new { gradeReportId = report.ReportId, registrationId = r.RegistrationId, classId = placement.ClassId };
        var events = new List<(DateTime Start, string Kind, int Id, object Value)>();
        if (r.Status is not ("CANCELLED" or "REJECTED"))
        {
            foreach (var b in state.Bookings.Where(x => x.Status == "SCHEDULED"))
            {
                var s = state.Session(b.SessionId); if (s.Status == "CANCELLED" || s.EndAt <= now.UtcDateTime) continue;
                events.Add((s.StartAt, "ENTRY_TEST", b.BookingId, new { kind = "ENTRY_TEST", registrationId = r.RegistrationId, bookingId = b.BookingId, startAt = AcademicGuard.Instant(s.StartAt), endAt = AcademicGuard.Instant(s.EndAt), room = s.Room }));
            }
            if (placement is not null && state.Set.Classes.Single(x => x.ClassId == placement.ClassId).Status != "CANCELLED")
            {
                foreach (var s in state.Set.Schedules.Where(x => x.ClassId == placement.ClassId && x.IsCurrent && x.OccurrenceState == "PLANNED"))
                {
                    var start = Utc(s.SessionDate, s.StartTime); var end = Utc(s.SessionDate, s.EndTime); if (end <= now.UtcDateTime) continue;
                    events.Add((start, "CLASS_SESSION", s.ScheduleId, new { kind = "CLASS_SESSION", registrationId = r.RegistrationId, scheduleId = s.ScheduleId, classId = s.ClassId, startAt = AcademicGuard.Instant(start), endAt = AcademicGuard.Instant(end), room = s.Room }));
                }
            }
        }
        var priority = new[] { "CONFIRM_REGISTRATION", "BOOK_ENTRY_TEST", "VIEW_FINALIZED_REPORT", "VIEW_INVOICE", "VIEW_CLASS", "VIEW_ENTRY_RESULTS" };
        var action = priority.FirstOrDefault(x => detail.AllowedActions.Contains(x));
        object? next = action is null ? null : new { action, target = Target(action, r.RegistrationId, state.Invoice?.InvoiceId, placement?.ClassId, published is null ? null : state.Set.Reports.Single(x => x.ClassAssignmentId == placement!.ClassAssignmentId).ReportId) };
        var step = r.Status switch { "PENDING" => "Chọn lịch kiểm tra đầu vào", "TESTING" => "Kiểm tra đầu vào", "BAND_ASSIGNED" => "Xác nhận kết quả đầu vào", "CONFIRMED" => "Chờ duyệt hồ sơ", "ADMITTED" => placement is null ? "Chờ xếp lớp" : "Học tập", "CANCELLED" => "Hồ sơ đã hủy", _ => "Hồ sơ bị từ chối" };
        var waiting = r.Status switch { "CONFIRMED" => "Chờ bộ phận tuyển sinh duyệt hồ sơ.", "CANCELLED" => "Xem lại lịch sử hồ sơ đã hủy.", "REJECTED" => "Xem lý do từ chối trong hồ sơ.", "TESTING" when state.Bookings.Any(x => x.Status == "SCHEDULED") => "Theo dõi lịch thi và chờ giáo viên ghi kết quả.", "ADMITTED" when state.Invoice?.Status == "UNPAID" => "Chờ thanh toán học phí.", "ADMITTED" when placement is null => "Chờ xếp lớp.", _ => (string?)null };
        var invoice = EnrollmentProjection.Invoice(state, now);
        return new(r.RegistrationId, EnrollmentProjection.Course(state), r.Status, AcademicGuard.Instant(r.CreatedAt), step, next, waiting,
            events.OrderBy(x => x.Start).ThenBy(x => x.Kind, StringComparer.Ordinal).ThenBy(x => x.Id).Select(x => x.Value).FirstOrDefault(),
            invoice?.Status == "UNPAID" ? new { invoice.Id, invoice.Number, invoice.Amount, invoice.DueDate, invoice.IsOverdue } : null, published);
    }

    private static object Target(string action, int registration, int? invoice, int? cls, int? report) => action switch
    {
        "VIEW_INVOICE" => new { registrationId = registration, invoiceId = invoice!.Value },
        "VIEW_CLASS" => new { registrationId = registration, classId = cls!.Value },
        "VIEW_FINALIZED_REPORT" => new { registrationId = registration, classId = cls!.Value, gradeReportId = report!.Value },
        _ => new { registrationId = registration }
    };
    public static DateTime Utc(DateOnly date, TimeOnly time) => new DateTimeOffset(date.ToDateTime(time), TimeSpan.FromHours(7)).UtcDateTime;
}
