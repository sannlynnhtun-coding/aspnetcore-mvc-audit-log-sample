namespace AuditLogSample.Shared;

/// <summary>
/// Provides the actor identity for the current request.
/// </summary>
public interface ICurrentUser
{
    string UserId { get; }
    string? Role { get; }
}
