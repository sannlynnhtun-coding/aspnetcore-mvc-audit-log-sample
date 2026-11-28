namespace AuditLogSample.Domain.Features.AuditLog;

public interface IFindAffectedUser
{
    Task<AuditTarget?> FindUserAsync(CancellationToken cancellationToken, params string?[] lookupKeys);
}
