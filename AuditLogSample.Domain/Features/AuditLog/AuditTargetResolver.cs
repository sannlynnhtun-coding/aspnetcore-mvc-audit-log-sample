using AuditLogSample.Database.AppDbContextModels;
using Microsoft.EntityFrameworkCore;

namespace AuditLogSample.Domain.Features.AuditLog;

public sealed class AuditTargetResolver(AppDbContext dbContext) : IAuditTargetResolver
{
    public async Task<AuditTarget?> ResolveUserAsync(CancellationToken cancellationToken, params string?[] lookupKeys)
    {
        foreach (var lookupKey in lookupKeys.Select(Normalize).Where(key => key is not null))
        {
            var user = FindTrackedUser(lookupKey!);

            user ??= await dbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    candidate =>
                        candidate.Id.ToLower() == lookupKey ||
                        candidate.Email.ToLower() == lookupKey ||
                        (candidate.MobileNo != null && candidate.MobileNo.ToLower() == lookupKey),
                    cancellationToken);

            if (user is not null)
            {
                return new AuditTarget(user.Id, user.Name, user.Email, user.MobileNo);
            }
        }

        return null;
    }

    private TblUser? FindTrackedUser(string lookupKey)
    {
        return dbContext.ChangeTracker
            .Entries<TblUser>()
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
