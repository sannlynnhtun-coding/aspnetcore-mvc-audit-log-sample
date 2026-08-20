using AuditLogSample.Models;

namespace AuditLogSample.Services;

public interface IUserService
{
    Task<IReadOnlyList<User>> GetAllAsync();
    Task<User?> GetByIdAsync(string id);
    Task<string> CreateAsync(CreateUserViewModel model);
    Task UpdateAsync(UpdateUserViewModel model);
    Task DeleteAsync(string id, string? reason = null);
    Task ApproveAsync(string id, string? reason = null);
    Task RejectAsync(string id, string? reason = null);
}
