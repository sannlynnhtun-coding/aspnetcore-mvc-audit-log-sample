namespace AuditLogSample.Domain.Events.AuditLogEvent;

public sealed class AuditLogEventService : INotificationHandler<AuditLogEventRequest>
{
    private readonly IAuditService _auditService;
    private readonly IAuditLogDbService _auditLogDbService;

    public AuditLogEventService(
        IAuditService auditService,
        IAuditLogDbService auditLogDbService)
    {
        _auditService = auditService;
        _auditLogDbService = auditLogDbService;
    }

    public async Task Handle(AuditLogEventRequest auditLogEventRequest, CancellationToken cancellationToken)
    {
        #region Prepare audit details

        var auditChanges = AuditLogEventMapper.Change(auditLogEventRequest);

        var affectedUser = await _auditLogDbService.FindUserAsync(
            cancellationToken,
            auditLogEventRequest.TargetLookupKey,
            auditLogEventRequest.TargetUserId,
            auditLogEventRequest.TargetId);

        #endregion

        #region Write the audit log

        var auditLog = new TblAuditLog
        {
            ActorUserId = auditLogEventRequest.ActorUserId,
            ActorRole = auditLogEventRequest.ActorRole,
            Action = auditLogEventRequest.Action.ToUpperInvariant(),
            ResourceType = auditLogEventRequest.ResourceType,
            TargetId = auditLogEventRequest.TargetId,
            TargetLookupKey = auditLogEventRequest.TargetLookupKey,
            TargetUserId = affectedUser?.UserId ?? auditLogEventRequest.TargetUserId,
            TargetUserName = affectedUser?.UserName,
            TargetUserMobileNo = affectedUser?.MobileNo,
            Changes = auditChanges,
            Reason = auditLogEventRequest.Reason,
            ActionDateTime = DateTime.UtcNow
        };

        await _auditService.LogAsync(auditLog, cancellationToken);

        #endregion
    }
}
