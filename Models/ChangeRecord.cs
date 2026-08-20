namespace AuditLogSample.Models;

public class ChangeRecord
{
    public string FieldName { get; set; } = null!;
    public object? OldValue { get; set; }
    public object? NewValue { get; set; }
}
