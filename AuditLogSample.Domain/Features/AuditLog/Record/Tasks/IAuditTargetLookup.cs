namespace AuditLogSample.Domain.Features.AuditLog;

public interface IAuditTargetLookup
{
    Task<AuditTarget?> FindUserAsync(CancellationToken cancellationToken, params string?[] lookupKeys);
}
