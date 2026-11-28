namespace AuditLogSample.Domain.Events.AuditLogEvent.Dtos;

public sealed record AuditLogEventRequest(
    string Action,
    string ResourceType,
    string TargetId,
    string? TargetUserId,
    string ActorUserId,
    string? ActorRole,
    string? Reason = null,
    IReadOnlyList<ChangeRecord>? Changes = null,
    string? OldStatus = null,
    string? NewStatus = null,
    string? TargetLookupKey = null
) : INotification;
