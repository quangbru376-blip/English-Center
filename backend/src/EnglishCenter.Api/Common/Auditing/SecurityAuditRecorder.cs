namespace EnglishCenter.Api.Common.Auditing;

public interface ISecurityAuditStore
{
    // Independent of the mutation transaction. Never compensate the security outcome.
    Task AppendAsync(AuditEntry entry,CancellationToken cancellationToken);
}

public sealed class SecurityAuditRecorder(ISecurityAuditStore store,AuditIncidentCounter incidents)
{
    private readonly ISecurityAuditStore _store=store ?? throw new ArgumentNullException(nameof(store));
    private readonly AuditIncidentCounter _incidents=incidents ?? throw new ArgumentNullException(nameof(incidents));
    public async Task<bool> TryRecordAsync(SecurityAuditKind kind,int? verifiedActorAccountId,bool success,
        Guid operation,DateTimeOffset occurredAt,string? ipAddress,CancellationToken cancellationToken)
    {
        var entry = AuditEntry.Security(kind,verifiedActorAccountId,success,operation,occurredAt,ipAddress);
        try { await _store.AppendAsync(entry,cancellationToken); return true; }
        catch (Exception)
        {
            // Accepted ADR-115: security logging failure never reverses logout/access checks.
            // No exception/request payload is retained, and no user notification is emitted.
            _incidents.RecordFailure(kind);
            return false;
        }
    }
}
