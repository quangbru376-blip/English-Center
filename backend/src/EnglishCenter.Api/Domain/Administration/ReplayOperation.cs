using EnglishCenter.Api.Domain.Identity;

namespace EnglishCenter.Api.Domain.Administration;

public sealed class ReplayOperation
{
    public int ReplayId { get; set; }
    public int ActorAccountId { get; set; }
    public string ActionCode { get; set; } = string.Empty;
    public Guid RequestId { get; set; }
    public byte[] InputDigest { get; set; } = [];
    public Guid OperationId { get; set; }
    public string ResultMode { get; set; } = string.Empty;
    public string ResultReference { get; set; } = string.Empty;
    public DateTime CompletedAt { get; set; }
    public DateTime ExpiresAt { get; set; }

    public Account? ActorAccount { get; set; }
}
