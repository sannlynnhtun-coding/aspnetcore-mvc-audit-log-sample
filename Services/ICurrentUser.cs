namespace AuditLogSample.Services;

public interface ICurrentUser
{
    string UserId { get; }
    string? Role { get; }
}
