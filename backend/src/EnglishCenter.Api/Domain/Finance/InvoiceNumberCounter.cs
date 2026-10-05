namespace EnglishCenter.Api.Domain.Finance;

public sealed class InvoiceNumberCounter
{
    public short IssueYear { get; set; }

    public int LastValue { get; set; }

    public byte[] RowVersion { get; set; } = [];
}
