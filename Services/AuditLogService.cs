using AuditLogSample.Data;
using AuditLogSample.Models;
using Microsoft.EntityFrameworkCore;

namespace AuditLogSample.Services;

public class AuditLogService : IAuditLogService
{
    private readonly AuditLogDbContext _dbContext;

    public AuditLogService(AuditLogDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task LogAsync(AuditLog log)
    {
        _dbContext.AuditLogs.Add(log);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<AuditLog>> GetAllAsync()
    {
        return await _dbContext.AuditLogs
            .AsNoTracking()
            .OrderByDescending(x => x.ActionDateTime)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<AuditLog>> GetByTargetIdAsync(string targetId)
    {
        return await _dbContext.AuditLogs
            .AsNoTracking()
            .Where(x =>
                x.TargetId == targetId ||
                x.TargetUserId == targetId ||
                x.TargetLookupKey == targetId)
            .OrderByDescending(x => x.ActionDateTime)
            .ToListAsync();
    }
}
