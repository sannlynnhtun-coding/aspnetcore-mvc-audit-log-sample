using AuditLogSample.Database.AppDbContextModels;

namespace AuditLogSample.Domain.Features.AuditLog;

public interface IAuditService
{
    Task LogAsync(TblAuditLog log, CancellationToken cancellationToken = default);
    Task<List<AuditLogResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<List<AuditLogResponse>> GetByTargetIdAsync(string targetId, CancellationToken cancellationToken = default);
}
