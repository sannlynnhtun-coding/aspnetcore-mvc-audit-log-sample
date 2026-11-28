namespace AuditLogSample.Database.AppDbContextModels;

public class TblUser
{
    public string Id { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? MobileNo { get; set; }
    public decimal Limit { get; set; }
    public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected, Active
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
