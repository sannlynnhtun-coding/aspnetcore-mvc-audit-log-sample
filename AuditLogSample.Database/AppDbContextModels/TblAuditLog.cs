namespace AuditLogSample.Database.AppDbContextModels;

public class TblAuditLog
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string ActorUserId { get; set; } = null!;
    public string? ActorRole { get; set; }
    public string Action { get; set; } = null!;
    public string ResourceType { get; set; } = null!;
    public string TargetId { get; set; } = null!;
    public string? TargetLookupKey { get; set; }
    public string? TargetUserId { get; set; }
    public string? TargetUserName { get; set; }
    public string? TargetUserMobileNo { get; set; }
    public string? Changes { get; set; }
    public string? Reason { get; set; }
    public DateTime ActionDateTime { get; set; } = DateTime.UtcNow;
}
