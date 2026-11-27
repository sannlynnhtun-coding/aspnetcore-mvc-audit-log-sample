using AuditLogSample.Shared;

namespace AuditLogSample.Domain.Features.Users;

public interface IUserService
{
    Task<Result<List<UserResponse>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<UserResponse>> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<Result<UserResponse>> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default);
    Task<Result<UserResponse>> UpdateAsync(string id, UpdateUserRequest request, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(string id, string? reason = null, CancellationToken cancellationToken = default);
    Task<Result<UserResponse>> ApproveAsync(string id, string? reason = null, CancellationToken cancellationToken = default);
    Task<Result<UserResponse>> RejectAsync(string id, string? reason = null, CancellationToken cancellationToken = default);
}
