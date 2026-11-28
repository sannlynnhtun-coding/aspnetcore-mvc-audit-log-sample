using AuditLogSample.Database.AppDbContextModels;
using MediatR;

namespace AuditLogSample.Domain.Features.AuditLog;

public sealed class AuditEventHandler : INotificationHandler<AuditEvent>
{
    private readonly IAuditService _auditService;
    private readonly IAuditTargetResolver _auditTargetResolver;

    public AuditEventHandler(
        IAuditService auditService,
        IAuditTargetResolver auditTargetResolver)
    {
        _auditService = auditService;
        _auditTargetResolver = auditTargetResolver;
    }

    public async Task Handle(AuditEvent notification, CancellationToken cancellationToken)
    {
        var changesText = notification.Changes?.Count > 0
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
            cancellationToken,
            notification.TargetLookupKey,
            notification.TargetUserId,
            notification.TargetId);

        var log = new TblAuditLog
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

        await _auditService.LogAsync(log, cancellationToken);
    }
}
