namespace EnglishCenter.Api.Services.Finance;

using System.Text.Json;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Common.Replay;
using EnglishCenter.Api.Services.Admissions;
using EnglishCenter.Api.Services.Enrollment;

public sealed record PaymentActorReference(int AccountId, string FullName);
public sealed record PaymentCorrectionReference(PaymentActorReference By, string At, string Reason);
public sealed record FinancePaymentView(int Id, int InvoiceId, string AmountPaid, string Method, string? TransactionReference,
    PaymentActorReference ConfirmedBy, string PaidAt, PaymentCorrectionReference? LastCorrection, string Version);
public sealed record FinanceVersions(string Registration, string Invoice);
public sealed record CollectionSet(int RegistrationId, int? PaymentId);
public sealed record CollectionManifest(FinanceVersions ExpectedVersions, CollectionSet ExpectedSet);
public sealed record InvoiceAccountantView(int Id, int RegistrationId, OwnCourseReference Course, string Number, string Amount,
    string IssuedAt, string DueDate, string Status, bool IsOverdue, int OverdueDays, AdmissionsContact Student,
    string RegistrationStatus, FinancePaymentView? Payment, FinanceVersions Versions, IReadOnlyList<string> AllowedActions,
    IReadOnlyDictionary<string, string> DisabledReasons);
public sealed record CollectionPreview(int InvoiceId, bool Eligible, IReadOnlyList<string> BlockedReasons, string AsOfUtc, string Amount, CollectionManifest? Manifest);
public sealed record FinanceCommandResult(InvoiceAccountantView Invoice, FinancePaymentView Payment, Guid OperationId);
public sealed record FinanceCommandOutcome(FinanceCommandResult Result, bool Replayed);

public sealed class FinanceInput
{
    public int Api { get; }
    public int TargetId { get; }
    public Guid RequestId { get; }
    public CanonicalIntent Intent { get; }
    internal JsonElement Body { get; }
    private FinanceInput(int api, int id, Guid request, CanonicalIntent intent)
    {
        Api = api; TargetId = id; RequestId = request; Intent = intent;
        using var doc = JsonDocument.Parse(intent.CopyBytes()); Body = doc.RootElement.GetProperty("body").Clone();
    }
    public static FinanceInput Parse(int api, int id, Guid request, string json, int bytes)
    {
        if (api is not (51 or 52) || request == Guid.Empty) throw ApiErrorException.Validation();
        return new(api, id, request, CanonicalIntent.Bind(api, json, new Dictionary<string, int> { [api == 51 ? "invoiceId" : "paymentId"] = id }, bytes));
    }
    public override string ToString() => nameof(FinanceInput);
}
