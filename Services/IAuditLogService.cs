using AuditLogSample.Models;

namespace AuditLogSample.Services;

public interface IAuditLogService
{
    Task LogAsync(AuditLog log);
    Task<IReadOnlyList<AuditLog>> GetAllAsync();
    Task<IReadOnlyList<AuditLog>> GetByTargetIdAsync(string targetId);
}
