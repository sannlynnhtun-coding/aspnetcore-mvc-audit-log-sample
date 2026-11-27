namespace AuditLogSample.Domain.Features.AuditLog;

public sealed record ChangeRecord(
    string FieldName,
    object? OldValue,
    object? NewValue);

public sealed record AuditTarget(
    string UserId,
    string UserName,
    string? Email,
    string? MobileNo);

/// <summary>
/// Represents an audit log entry returned by the API.
/// </summary>
public sealed record AuditLogResponse(
    string Id,
    string ActorUserId,
    string? ActorRole,
    string Action,
    string ResourceType,
    string TargetId,
    string? TargetLookupKey,
    string? TargetUserId,
    string? TargetUserName,
    string? TargetUserMobileNo,
    string TargetDisplayName,
    string? Changes,
    string? Reason,
    DateTimeOffset ActionDateTime);
