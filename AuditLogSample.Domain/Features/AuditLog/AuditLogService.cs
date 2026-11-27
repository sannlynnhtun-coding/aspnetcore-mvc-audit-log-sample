using AuditLogSample.Database.AppDbContextModels;
using Microsoft.EntityFrameworkCore;

namespace AuditLogSample.Domain.Features.AuditLog;

public sealed class AuditLogService : IAuditService
{
    private readonly AppDbContext _dbContext;

    public AuditLogService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task LogAsync(TblAuditLog log, CancellationToken cancellationToken = default)
    {
        _dbContext.AuditLogs.Add(log);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<AuditLogResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var logs = await _dbContext.AuditLogs
            .AsNoTracking()
            .OrderByDescending(x => x.ActionDateTime)
            .ToListAsync(cancellationToken);

        return logs.Select(log =>
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
        }).ToList();
    }

    public async Task<List<AuditLogResponse>> GetByTargetIdAsync(
        string targetId,
        CancellationToken cancellationToken = default)
    {
        var logs = await _dbContext.AuditLogs
            .AsNoTracking()
            .Where(x =>
                x.TargetId == targetId ||
                x.TargetUserId == targetId ||
                x.TargetLookupKey == targetId)
            .OrderByDescending(x => x.ActionDateTime)
            .ToListAsync(cancellationToken);

        return logs.Select(log =>
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
        }).ToList();
    }
}
