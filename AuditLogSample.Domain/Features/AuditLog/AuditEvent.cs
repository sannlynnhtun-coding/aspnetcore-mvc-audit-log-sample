using MediatR;

namespace AuditLogSample.Domain.Features.AuditLog;

public sealed record AuditEvent(
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
