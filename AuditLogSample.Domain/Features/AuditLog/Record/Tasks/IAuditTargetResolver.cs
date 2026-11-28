namespace AuditLogSample.Domain.Features.AuditLog;

public interface IAuditTargetResolver
{
    Task<AuditTarget?> ResolveUserAsync(CancellationToken cancellationToken, params string?[] lookupKeys);
}
