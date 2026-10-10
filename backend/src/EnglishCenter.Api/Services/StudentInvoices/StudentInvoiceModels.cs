namespace EnglishCenter.Api.Services.StudentInvoices;

using EnglishCenter.Api.Services.Enrollment;

public sealed record StudentInvoiceView(int Id, int RegistrationId, OwnCourseReference Course, string Number, string Amount,
    string IssuedAt, string DueDate, string Status, bool IsOverdue, int OverdueDays);
public sealed record StudentPaymentGuidance(string Message);
public sealed record StudentInvoiceDetail(int Id, int RegistrationId, OwnCourseReference Course, string Number, string Amount,
    string IssuedAt, string DueDate, string Status, bool IsOverdue, int OverdueDays, string CurrentStudentName, StudentPaymentGuidance PaymentGuidance);
