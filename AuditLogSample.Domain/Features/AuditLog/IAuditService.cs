namespace AuditLogSample.Domain.Features.AuditLog;

public interface IAuditService
{
    Task LogAsync(TblAuditLog log, CancellationToken cancellationToken = default);
    Task<Result<List<AuditLogResponse>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<List<AuditLogResponse>>> GetByTargetIdAsync(string targetId, CancellationToken cancellationToken = default);
}
