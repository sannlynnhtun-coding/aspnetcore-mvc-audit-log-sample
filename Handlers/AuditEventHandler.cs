using AuditLogSample.Events;
using AuditLogSample.Models;
using AuditLogSample.Services;
using MediatR;

namespace AuditLogSample.Handlers;

public class AuditEventHandler : INotificationHandler<AuditEvent>
{
    private readonly IAuditLogService _auditLogService;
    private readonly IAuditTargetResolver _auditTargetResolver;

    public AuditEventHandler(IAuditLogService auditLogService, IAuditTargetResolver auditTargetResolver)
    {
        _auditLogService = auditLogService;
        _auditTargetResolver = auditTargetResolver;
    }

    public async Task Handle(AuditEvent notification, CancellationToken cancellationToken)
    {
        var changesText = notification.Changes?.Any() == true
            ? string.Join(" | ", notification.Changes.Select(c =>
                $"{c.FieldName}: {c.OldValue} → {c.NewValue}"))
            : null;

        if (!string.IsNullOrEmpty(notification.OldStatus) || !string.IsNullOrEmpty(notification.NewStatus))
        {
            var statusChange = $"Status: {notification.OldStatus} → {notification.NewStatus}";
            changesText = string.IsNullOrEmpty(changesText)
                ? statusChange
                : $"{changesText} | {statusChange}";
        }

        var target = await _auditTargetResolver.ResolveUserAsync(
            notification.TargetLookupKey,
            notification.TargetUserId,
            notification.TargetId);

        var log = new AuditLog
        {
            ActorUserId = notification.ActorUserId,
            ActorRole = notification.ActorRole,
            Action = notification.Action.ToUpperInvariant(),
            ResourceType = notification.ResourceType,
            TargetId = notification.TargetId,
            TargetLookupKey = notification.TargetLookupKey,
            TargetUserId = target?.UserId ?? notification.TargetUserId,
            TargetUserName = target?.UserName,
            TargetUserMobileNo = target?.MobileNo,
            Changes = changesText,
            Reason = notification.Reason,
            ActionDateTime = DateTime.UtcNow
        };

        await _auditLogService.LogAsync(log);
    }
}
