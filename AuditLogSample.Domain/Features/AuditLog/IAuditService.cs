using AuditLogSample.Database.AppDbContextModels;

namespace AuditLogSample.Domain.Features.AuditLog;

public interface IAuditService
{
    Task LogAsync(TblAuditLog log, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AuditLogResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AuditLogResponse>> GetByTargetIdAsync(string targetId, CancellationToken cancellationToken = default);
}
