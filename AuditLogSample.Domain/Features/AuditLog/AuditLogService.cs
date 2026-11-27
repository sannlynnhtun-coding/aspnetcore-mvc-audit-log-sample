using AuditLogSample.Database.AppDbContextModels;
using Microsoft.EntityFrameworkCore;

namespace AuditLogSample.Domain.Features.AuditLog;

public sealed class AuditLogService(AppDbContext dbContext) : IAuditService
{
    public async Task LogAsync(TblAuditLog log, CancellationToken cancellationToken = default)
    {
        dbContext.AuditLogs.Add(log);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AuditLogResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var logs = await dbContext.AuditLogs
            .AsNoTracking()
            .OrderByDescending(x => x.ActionDateTime)
            .ToListAsync(cancellationToken);

        return logs.Select(MapToResponse).ToList();
    }

    public async Task<IReadOnlyList<AuditLogResponse>> GetByTargetIdAsync(
        string targetId,
        CancellationToken cancellationToken = default)
    {
        var logs = await dbContext.AuditLogs
            .AsNoTracking()
            .Where(x =>
                x.TargetId == targetId ||
                x.TargetUserId == targetId ||
                x.TargetLookupKey == targetId)
            .OrderByDescending(x => x.ActionDateTime)
            .ToListAsync(cancellationToken);

        return logs.Select(MapToResponse).ToList();
    }

    private static AuditLogResponse MapToResponse(TblAuditLog log)
    {
        var displayName = !string.IsNullOrWhiteSpace(log.TargetUserName)
            ? (string.IsNullOrWhiteSpace(log.TargetUserId)
                ? log.TargetUserName
                : $"{log.TargetUserName} ({log.TargetUserId})")
            : log.TargetUserId ?? log.TargetLookupKey ?? log.TargetId;

        return new AuditLogResponse(
            log.Id,
            log.ActorUserId,
            log.ActorRole,
            log.Action,
            log.ResourceType,
            log.TargetId,
            log.TargetLookupKey,
            log.TargetUserId,
            log.TargetUserName,
            log.TargetUserMobileNo,
            displayName,
            log.Changes,
            log.Reason,
            new DateTimeOffset(DateTime.SpecifyKind(log.ActionDateTime, DateTimeKind.Utc)));
    }
}
