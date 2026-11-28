namespace AuditLogSample.Domain.Events.AuditLogEvent.Tasks;

public interface IAuditLogDbService
{
    Task<AuditTarget?> FindUserAsync(CancellationToken cancellationToken, params string?[] lookupKeys);
}
