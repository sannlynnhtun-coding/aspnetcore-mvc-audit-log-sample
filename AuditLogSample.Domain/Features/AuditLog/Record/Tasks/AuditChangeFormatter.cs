namespace AuditLogSample.Domain.Features.AuditLog;

internal static class AuditChangeFormatter
{
    internal static string? Format(AuditEvent notification)
    {
        #region Format field and status changes

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

        return changesText;

        #endregion
    }
}
