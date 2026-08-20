using AuditLogSample.Data;
using AuditLogSample.Models;
using Microsoft.EntityFrameworkCore;

namespace AuditLogSample.Services;

public interface IAuditTargetResolver
{
    Task<AuditTarget?> ResolveUserAsync(params string?[] lookupKeys);
}

public sealed record AuditTarget(
    string UserId,
    string UserName,
    string? Email,
    string? MobileNo);

public class AuditTargetResolver : IAuditTargetResolver
{
    private readonly AuditLogDbContext _dbContext;

    public AuditTargetResolver(AuditLogDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AuditTarget?> ResolveUserAsync(params string?[] lookupKeys)
    {
        foreach (var lookupKey in lookupKeys.Select(Normalize).Where(key => key is not null))
        {
            var user = FindTrackedUser(lookupKey!);

            user ??= await _dbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(candidate =>
                    candidate.Id.ToLower() == lookupKey ||
                    candidate.Email.ToLower() == lookupKey ||
                    (candidate.MobileNo != null && candidate.MobileNo.ToLower() == lookupKey));

            if (user is not null)
            {
                return new AuditTarget(user.Id, user.Name, user.Email, user.MobileNo);
            }
        }

        return null;
    }

    private User? FindTrackedUser(string lookupKey)
    {
        return _dbContext.ChangeTracker
            .Entries<User>()
            .Where(entry => entry.State != EntityState.Detached)
            .Select(entry => entry.Entity)
            .FirstOrDefault(user =>
                string.Equals(user.Id, lookupKey, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(user.Email, lookupKey, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(user.MobileNo, lookupKey, StringComparison.OrdinalIgnoreCase));
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim().ToLowerInvariant();
    }
}
