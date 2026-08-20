namespace AuditLogSample.Services;

/// <summary>
/// Demo implementation. In real app inject IHttpContextAccessor and read claims.
/// </summary>
public class CurrentUser : ICurrentUser
{
    public string UserId => "maker-001";
    public string? Role => "Admin";
}
