using System.ComponentModel.DataAnnotations;
using AuditLogSample.Database.AppDbContextModels;
using AuditLogSample.Shared;
using Microsoft.EntityFrameworkCore;

namespace AuditLogSample.Domain.Features.Users;

public sealed class UserValidationPolicy(AppDbContext dbContext)
{
    private static readonly EmailAddressAttribute EmailValidator = new();

    public async Task<Result> ValidateCreateAsync(
        CreateUserRequest request,
        CancellationToken cancellationToken = default)
    {
        var inputResult = ValidateBasicFields(request.Name, request.Email, request.Limit);
        if (!inputResult.IsSuccess)
        {
            return inputResult;
        }

        return await CheckDuplicatesAsync(request.Email, request.MobileNo, excludeUserId: null, cancellationToken);
    }

    public async Task<Result> ValidateUpdateAsync(
        string userId,
        UpdateUserRequest request,
        CancellationToken cancellationToken = default)
    {
        var inputResult = ValidateBasicFields(request.Name, request.Email, request.Limit);
        if (!inputResult.IsSuccess)
        {
            return inputResult;
        }

        return await CheckDuplicatesAsync(request.Email, request.MobileNo, excludeUserId: userId, cancellationToken);
    }

    public Result ValidateStatusTransition(TblUser user, string targetStatus)
    {
        if (string.Equals(user.Status, targetStatus, StringComparison.OrdinalIgnoreCase))
        {
            return Result.Conflict($"User is already {targetStatus}.");
        }

        return Result.Success();
    }

    private static Result ValidateBasicFields(string? name, string? email, decimal limit)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(name))
        {
            errors.Add("Name is required.");
        }
        else if (name.Trim().Length > 100)
        {
            errors.Add("Name must not exceed 100 characters.");
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            errors.Add("Email is required.");
        }
        else if (!EmailValidator.IsValid(email.Trim()))
        {
            errors.Add("Email format is invalid.");
        }

        if (limit is < 0 or > 10_000_000)
        {
            errors.Add("Limit must be between 0 and 10,000,000.");
        }

        return errors.Count > 0
            ? Result.ValidationError(errors[0], errors)
            : Result.Success();
    }

    private async Task<Result> CheckDuplicatesAsync(
        string email,
        string? mobileNo,
        string? excludeUserId,
        CancellationToken cancellationToken)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var emailExists = await dbContext.Users
            .AsNoTracking()
            .AnyAsync(
                u => (excludeUserId == null || u.Id != excludeUserId) &&
                     u.Email.ToLower() == normalizedEmail,
                cancellationToken);

        if (emailExists)
        {
            return Result.Conflict($"A user with email '{email.Trim()}' already exists.");
        }

        if (!string.IsNullOrWhiteSpace(mobileNo))
        {
            var normalizedMobile = mobileNo.Trim().ToLowerInvariant();
            var mobileExists = await dbContext.Users
                .AsNoTracking()
                .AnyAsync(
                    u => (excludeUserId == null || u.Id != excludeUserId) &&
                         u.MobileNo != null &&
                         u.MobileNo.ToLower() == normalizedMobile,
                    cancellationToken);

            if (mobileExists)
            {
                return Result.Conflict($"A user with mobile number '{mobileNo.Trim()}' already exists.");
            }
        }

        return Result.Success();
    }
}
