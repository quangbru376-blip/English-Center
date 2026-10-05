namespace EnglishCenter.Api.Common.Auditing;

using System.Net;
using System.Text.Json.Nodes;

public sealed record AuditChanges(JsonObject? Old = null, JsonObject? New = null, JsonObject? Parent = null, string? Reason = null);

public sealed class AuditEntry
{
    public int? ActorAccountId { get; }
    public string Action { get; }
    public string EntityType { get; }
    public string? EntityId { get; }
    public string Result { get; }
    public string? Details { get; }
    public DateTime OccurredAt { get; }
    public string? IpAddress { get; }
    public Guid OperationId { get; }
    public string Category { get; }
    public DateTime? SecurityExpiresAt { get; }
    public DateTime? IpExpiresAt { get; }

    private AuditEntry(int? actor, string action, string entity, string? id, string result, string? details,
        DateTimeOffset at, Guid operation, string category, string? ip)
    {
        if (operation == Guid.Empty || actor is <= 0) throw new ArgumentException("Invalid audit reference.");
        var utc = at.UtcDateTime;
        OccurredAt = new DateTime(utc.Ticks - utc.Ticks % TimeSpan.TicksPerMillisecond,DateTimeKind.Utc);
        ActorAccountId=actor;Action=action;EntityType=entity;EntityId=id;Result=result;Details=details;
        OperationId=operation;Category=category;IpAddress=ip;
        SecurityExpiresAt = category == "SECURITY" ? OccurredAt.AddDays(90) : null;
        IpExpiresAt = ip is null ? null : OccurredAt.AddDays(90);
    }

    public static AuditEntry Mandatory(string action,int verifiedActorAccountId,string entity,int entityId,
        Guid operation,DateTimeOffset occurredAt,AuditChanges changes)
    {
        ArgumentNullException.ThrowIfNull(changes);
        ArgumentNullException.ThrowIfNull(action);ArgumentNullException.ThrowIfNull(entity);
        if (entityId <= 0) throw new ArgumentException("Invalid audit reference.");
        return new(verifiedActorAccountId,action,entity,entityId.ToString(System.Globalization.CultureInfo.InvariantCulture),"SUCCESS",
            AuditPolicy.Project(action,entity,changes),occurredAt,operation,"MANDATORY",null);
    }

    public static AuditEntry Security(SecurityAuditKind kind,int? verifiedActorAccountId,bool success,
        Guid operation,DateTimeOffset occurredAt,string? ipAddress = null)
    {
        var action = kind switch
        {
            SecurityAuditKind.Login => "AUTH.LOGIN", SecurityAuditKind.Logout => "AUTH.LOGOUT",
            SecurityAuditKind.AccessDenied => "AUTH.ACCESS_DENIED", SecurityAuditKind.Throttled => "AUTH.THROTTLED",
            _ => throw new ArgumentOutOfRangeException(nameof(kind))
        };
        if (ipAddress is not null && (ipAddress.Length > 45 || !IPAddress.TryParse(ipAddress,out _))) throw new ArgumentException("Invalid audit IP address.");
        return new(verifiedActorAccountId,action,"Account",verifiedActorAccountId?.ToString(System.Globalization.CultureInfo.InvariantCulture),
            success ? "SUCCESS" : "FAILURE",null,occurredAt,operation,"SECURITY",ipAddress);
    }
}
