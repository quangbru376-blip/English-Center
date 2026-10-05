using EnglishCenter.Api.Domain.Identity;

namespace EnglishCenter.Api.Domain.Finance;

public sealed class Payment
{
    public int PaymentId { get; set; }
    public int InvoiceId { get; set; }
    public decimal AmountPaid { get; set; }
    public string Method { get; set; } = string.Empty;
    public string? TransactionReference { get; set; }
    public int ConfirmedBy { get; set; }
    public DateTime PaidAt { get; set; }
    public int? LastCorrectedBy { get; set; }
    public DateTime? LastCorrectedAt { get; set; }
    public string? LastCorrectionReason { get; set; }
    public byte[] RowVersion { get; set; } = [];

    public Invoice? Invoice { get; set; }
    public Account? ConfirmedByAccount { get; set; }
    public Account? LastCorrectedByAccount { get; set; }
}
