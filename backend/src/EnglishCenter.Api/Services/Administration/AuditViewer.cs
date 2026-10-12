namespace EnglishCenter.Api.Services.Administration;
using System.Text.Json.Nodes;
using EnglishCenter.Api.Common.Security;

public sealed record AuditEventSummary(int Id,string OccurredAt,int? ActorId,string Action,string EntityType,string? EntityId,string Result,Guid OperationId);
public sealed record AuditFieldChange(string Field,JsonNode? OldValue,JsonNode? NewValue);
public sealed record AuditEventDetail(int Id,string OccurredAt,int? ActorId,string Action,string EntityType,string? EntityId,string Result,Guid OperationId,string? Reason,IReadOnlyList<AuditFieldChange> Changes,string? IpAddress);
public sealed record AuditEventPage(IReadOnlyList<AuditEventSummary> Items,int Page,int PageSize,long TotalCount);
public interface IAuditViewerExecutor
{
    Task<AuditEventPage> List(VerifiedSessionToken token,IQueryCollection query,CancellationToken ct);
    Task<AuditEventDetail> Detail(VerifiedSessionToken token,int id,CancellationToken ct);
    Task<AuditEventPage> Operation(VerifiedSessionToken token,Guid operation,IQueryCollection query,CancellationToken ct);
}
