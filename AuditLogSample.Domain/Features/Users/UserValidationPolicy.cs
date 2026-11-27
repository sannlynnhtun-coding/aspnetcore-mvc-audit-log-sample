using AuditLogSample.Database.AppDbContextModels;
using AuditLogSample.Shared;

namespace AuditLogSample.Domain.Features.Users;

public sealed class UserValidationPolicy
{
    public Result ValidateCreate(CreateUserRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return Result.Failure("Name is required.");

        if (string.IsNullOrWhiteSpace(request.Email))
            return Result.Failure("Email is required.");

        if (request.Limit < 0)
            return Result.Failure("Limit cannot be negative.");

        return Result.Success();
    }

    public Result ValidateUpdate(UpdateUserRequest request)
    {
        if (request.Name is not null && string.IsNullOrWhiteSpace(request.Name))
            return Result.Failure("Name cannot be empty.");

        if (request.Email is not null && string.IsNullOrWhiteSpace(request.Email))
            return Result.Failure("Email cannot be empty.");

        if (request.Limit < 0)
            return Result.Failure("Limit cannot be negative.");

        return Result.Success();
    }

    public Result ValidateStatusTransition(TblUser user, string targetStatus)
    {
        if (string.Equals(user.Status, targetStatus, StringComparison.OrdinalIgnoreCase))
            return Result.Failure($"User is already {targetStatus}.");

        return Result.Success();
    }
}
