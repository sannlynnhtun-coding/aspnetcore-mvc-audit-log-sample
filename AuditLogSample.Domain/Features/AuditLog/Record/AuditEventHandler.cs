using AuditLogSample.Database.AppDbContextModels;
using MediatR;

namespace AuditLogSample.Domain.Features.AuditLog;

public sealed class AuditEventHandler : INotificationHandler<AuditEvent>
{
    private readonly IAuditService _auditService;
    private readonly IAffectedUserLookup _affectedUserLookup;

    public AuditEventHandler(
        IAuditService auditService,
        IAffectedUserLookup affectedUserLookup)
    {
        _auditService = auditService;
        _affectedUserLookup = affectedUserLookup;
    }

    public async Task Handle(AuditEvent notification, CancellationToken cancellationToken)
    {
        #region Prepare audit details

        var changesText = AuditChangeFormatter.Format(notification);

        var target = await _affectedUserLookup.FindUserAsync(
            cancellationToken,
            notification.TargetLookupKey,
            notification.TargetUserId,
            notification.TargetId);

        #endregion

        #region Write the audit log

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

        #endregion
    }
}
