using AuditLogSample.Models;
using MediatR;

namespace AuditLogSample.Events;

public record AuditEvent(
    string Action,
    string ResourceType,
    string TargetId,
    string? TargetUserId,
    string ActorUserId,
    string? ActorRole,
    string? Reason = null,
    List<ChangeRecord>? Changes = null,
    string? OldStatus = null,
    string? NewStatus = null,
    string? TargetLookupKey = null
) : INotification;
