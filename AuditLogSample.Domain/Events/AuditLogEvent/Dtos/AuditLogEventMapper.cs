namespace AuditLogSample.Domain.Events.AuditLogEvent.Dtos;

internal static class AuditLogEventMapper
{
    internal static string? Change(AuditLogEventRequest auditLogEventRequest)
    {
        #region Format field and status changes

        var auditChanges = auditLogEventRequest.Changes?.Count > 0
            ? string.Join(" | ", auditLogEventRequest.Changes.Select(c =>
                $"{c.FieldName}: {c.OldValue} → {c.NewValue}"))
            : null;

        if (!string.IsNullOrEmpty(auditLogEventRequest.OldStatus) || !string.IsNullOrEmpty(auditLogEventRequest.NewStatus))
        {
            var statusChange = $"Status: {auditLogEventRequest.OldStatus} → {auditLogEventRequest.NewStatus}";
            auditChanges = string.IsNullOrEmpty(auditChanges)
                ? statusChange
                : $"{auditChanges} | {statusChange}";
        }

        return auditChanges;

        #endregion
    }
}
