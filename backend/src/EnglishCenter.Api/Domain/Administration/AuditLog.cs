using EnglishCenter.Api.Domain.Identity;

namespace EnglishCenter.Api.Domain.Administration;

public sealed class AuditLog
{
    public int AuditId { get; set; }
    public int? ActorAccountId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string? EntityId { get; set; }
    public string Result { get; set; } = string.Empty;
    public string? Details { get; set; }
    public DateTime OccurredAt { get; set; }
    public string? IpAddress { get; set; }
    public Guid OperationId { get; set; }
    public string AuditCategory { get; set; } = string.Empty;
    public DateTime? SecurityExpiresAt { get; set; }
    public DateTime? IpExpiresAt { get; set; }

    public Account? ActorAccount { get; set; }
}
