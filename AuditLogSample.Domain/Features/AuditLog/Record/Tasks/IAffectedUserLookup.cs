namespace AuditLogSample.Domain.Features.AuditLog;

public interface IAffectedUserLookup
{
    Task<AuditTarget?> FindUserAsync(CancellationToken cancellationToken, params string?[] lookupKeys);
}
