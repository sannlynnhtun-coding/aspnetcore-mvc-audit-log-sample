using System.Reflection;
using AuditLogSample.Database.AppDbContextModels;
using AuditLogSample.Domain.Features.AuditLog;
using AuditLogSample.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AuditLogSample.Domain.Features.Users;

public sealed class UserService(
    AppDbContext dbContext,
    IMediator mediator,
    ICurrentUser currentUser,
    IAuditService auditService,
    UserValidationPolicy validationPolicy) : IUserService
{
    public async Task<Result<IReadOnlyList<UserResponse>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var users = await dbContext.Users
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        IReadOnlyList<UserResponse> response = users.Select(MapToResponse).ToList();
        return Result<IReadOnlyList<UserResponse>>.Success(response);
    }

    public async Task<Result<UserDetailResponse>> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var user = await dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (user is null)
        {
            return Result<UserDetailResponse>.NotFound("User not found.");
        }

        var logs = await auditService.GetByTargetIdAsync(id, cancellationToken);
        return Result<UserDetailResponse>.Success(new UserDetailResponse(MapToResponse(user), logs));
    }

    public async Task<Result<UserResponse>> CreateAsync(
        CreateUserRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = await validationPolicy.ValidateCreateAsync(request, cancellationToken);
        if (!validation.IsSuccess)
        {
            return MapFailure<UserResponse>(validation);
        }

        var user = new TblUser
        {
            Id = Guid.NewGuid().ToString("N")[..12],
            Name = request.Name.Trim(),
            Email = request.Email.Trim(),
            MobileNo = string.IsNullOrWhiteSpace(request.MobileNo) ? null : request.MobileNo.Trim(),
            Limit = request.Limit,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        // Stage entity in scoped AppDbContext; AuditEventHandler + IAuditService.LogAsync
        // commits both TblUser and TblAuditLog atomically in a single SaveChangesAsync call.
        dbContext.Users.Add(user);

        await mediator.Publish(
            new AuditEvent(
                Action: "CREATE",
                ResourceType: "User",
                TargetId: user.Id,
                TargetUserId: user.Id,
                ActorUserId: currentUser.UserId,
                ActorRole: currentUser.Role,
                Reason: request.Reason,
                NewStatus: "Pending",
                TargetLookupKey: user.MobileNo ?? user.Id),
            cancellationToken);

        return Result<UserResponse>.Success(MapToResponse(user), "User created successfully (Pending approval).");
    }

    public async Task<Result<UserResponse>> UpdateAsync(
        string id,
        UpdateUserRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await dbContext.Users.FindAsync([id], cancellationToken);
        if (user is null)
        {
            return Result<UserResponse>.NotFound("User not found.");
        }

        var validation = await validationPolicy.ValidateUpdateAsync(id, request, cancellationToken);
        if (!validation.IsSuccess)
        {
            return MapFailure<UserResponse>(validation);
        }

        var dto = new
        {
            Name = request.Name.Trim(),
            Email = request.Email.Trim(),
            MobileNo = string.IsNullOrWhiteSpace(request.MobileNo) ? null : request.MobileNo.Trim(),
            request.Limit
        };

        var changes = TrackChanges(user, dto);
        if (changes.Count == 0)
        {
            return Result<UserResponse>.Success(MapToResponse(user), "No changes detected.");
        }

        await mediator.Publish(
            new AuditEvent(
                Action: "UPDATE",
                ResourceType: "User",
                TargetId: user.Id,
                TargetUserId: user.Id,
                ActorUserId: currentUser.UserId,
                ActorRole: currentUser.Role,
                Reason: request.Reason,
                Changes: changes,
                TargetLookupKey: user.MobileNo ?? user.Id),
            cancellationToken);

        return Result<UserResponse>.Success(MapToResponse(user), "User updated successfully.");
    }

    public async Task<Result> DeleteAsync(
        string id,
        string? reason = null,
        CancellationToken cancellationToken = default)
    {
        var user = await dbContext.Users.FindAsync([id], cancellationToken);
        if (user is null)
        {
            return Result.NotFound("User not found.");
        }

        dbContext.Users.Remove(user);

        await mediator.Publish(
            new AuditEvent(
                Action: "DELETE",
                ResourceType: "User",
                TargetId: id,
                TargetUserId: id,
                ActorUserId: currentUser.UserId,
                ActorRole: currentUser.Role,
                Reason: reason,
                TargetLookupKey: user.MobileNo ?? id),
            cancellationToken);

        return Result.Success("User deleted successfully.");
    }

    public async Task<Result<UserResponse>> ApproveAsync(
        string id,
        string? reason = null,
        CancellationToken cancellationToken = default)
    {
        var user = await dbContext.Users.FindAsync([id], cancellationToken);
        if (user is null)
        {
            return Result<UserResponse>.NotFound("User not found.");
        }

        var transition = validationPolicy.ValidateStatusTransition(user, "Approved");
        if (!transition.IsSuccess)
        {
            return MapFailure<UserResponse>(transition);
        }

        var oldStatus = user.Status;
        user.Status = "Approved";

        await mediator.Publish(
            new AuditEvent(
                Action: "APPROVE",
                ResourceType: "User",
                TargetId: id,
                TargetUserId: id,
                ActorUserId: currentUser.UserId,
                ActorRole: currentUser.Role,
                Reason: reason,
                OldStatus: oldStatus,
                NewStatus: "Approved",
                TargetLookupKey: user.MobileNo ?? id),
            cancellationToken);

        return Result<UserResponse>.Success(MapToResponse(user), "User approved successfully.");
    }

    public async Task<Result<UserResponse>> RejectAsync(
        string id,
        string? reason = null,
        CancellationToken cancellationToken = default)
    {
        var user = await dbContext.Users.FindAsync([id], cancellationToken);
        if (user is null)
        {
            return Result<UserResponse>.NotFound("User not found.");
        }

        var transition = validationPolicy.ValidateStatusTransition(user, "Rejected");
        if (!transition.IsSuccess)
        {
            return MapFailure<UserResponse>(transition);
        }

        var oldStatus = user.Status;
        user.Status = "Rejected";

        await mediator.Publish(
            new AuditEvent(
                Action: "REJECT",
                ResourceType: "User",
                TargetId: id,
                TargetUserId: id,
                ActorUserId: currentUser.UserId,
                ActorRole: currentUser.Role,
                Reason: reason,
                OldStatus: oldStatus,
                NewStatus: "Rejected",
                TargetLookupKey: user.MobileNo ?? id),
            cancellationToken);

        return Result<UserResponse>.Success(MapToResponse(user), "User rejected successfully.");
    }

    private static UserResponse MapToResponse(TblUser user) =>
        new(
            user.Id,
            user.Name,
            user.Email,
            user.MobileNo,
            user.Limit,
            user.Status,
            new DateTimeOffset(DateTime.SpecifyKind(user.CreatedAt, DateTimeKind.Utc)));

    private static Result<T> MapFailure<T>(Result failure) =>
        failure.ErrorType switch
        {
            ErrorType.NotFound => Result<T>.NotFound(failure.Message),
            ErrorType.Conflict => Result<T>.Conflict(failure.Message),
            _ => Result<T>.ValidationError(failure.Message, failure.Errors)
        };

    private static List<ChangeRecord> TrackChanges<T1, T2>(T1 obj1, T2 obj2, bool isTrackAll = false)
        where T1 : class
        where T2 : class
    {
        var changeLog = new List<ChangeRecord>();

        var properties1 = typeof(T1).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property1 in properties1)
        {
            if (!property1.CanWrite)
            {
                continue;
            }

            var property2 = typeof(T2).GetProperty(property1.Name, BindingFlags.Public | BindingFlags.Instance);
            if (property2 is null || !property2.CanRead)
            {
                continue;
            }

            var oldValue = property1.GetValue(obj1);
            var newValue = property2.GetValue(obj2);

            if (isTrackAll)
            {
                changeLog.Add(new ChangeRecord(property1.Name, oldValue, newValue));
                property1.SetValue(obj1, newValue);
            }
            else if (oldValue?.ToString() != newValue?.ToString())
            {
                if (newValue is null && oldValue is null)
                {
                    continue;
                }

                changeLog.Add(new ChangeRecord(property1.Name, oldValue, newValue));
                property1.SetValue(obj1, newValue);
            }
        }

        return changeLog;
    }
}
