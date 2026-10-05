using EnglishCenter.Api.Domain.Admissions;
using EnglishCenter.Api.Domain.Identity;

namespace EnglishCenter.Api.Domain.Finance;

public sealed class Invoice
{
    public int InvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public int RegistrationId { get; set; }
    public decimal Amount { get; set; }
    public DateTime IssuedAt { get; set; }
    public DateOnly DueDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public int? CancelledBy { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string? CancellationReason { get; set; }
    public string? CancelledFromStatus { get; set; }
    public Guid? CancellationOperationId { get; set; }
    public byte[] RowVersion { get; set; } = [];

    public Registration? Registration { get; set; }
    public Account? CancelledByAccount { get; set; }
}
