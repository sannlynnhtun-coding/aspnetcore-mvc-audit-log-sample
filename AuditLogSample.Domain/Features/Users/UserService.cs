using AuditLogSample.Database.AppDbContextModels;
using AuditLogSample.Domain.Features.AuditLog;
using AuditLogSample.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AuditLogSample.Domain.Features.Users;

public sealed class UserService : IUserService
{
    private readonly AppDbContext _dbContext;
    private readonly IMediator _mediator;
    private readonly ICurrentUser _currentUser;
    private readonly IAuditService _auditService;
    private readonly UserValidationPolicy _validationPolicy;

    public UserService(
        AppDbContext dbContext,
        IMediator mediator,
        ICurrentUser currentUser,
        IAuditService auditService,
        UserValidationPolicy validationPolicy)
    {
        _dbContext = dbContext;
        _mediator = mediator;
        _currentUser = currentUser;
        _auditService = auditService;
        _validationPolicy = validationPolicy;
    }

    #region Get All Users
    public async Task<Result<List<UserResponse>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var users = await _dbContext.Users
            .AsNoTracking()
            .OrderByDescending(u => u.CreatedAt)
            .Select(u => new UserResponse
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                MobileNo = u.MobileNo,
                Limit = u.Limit,
                Status = u.Status,
                CreatedAt = new DateTimeOffset(DateTime.SpecifyKind(u.CreatedAt, DateTimeKind.Utc))
            })
            .ToListAsync(cancellationToken);

        return Result<List<UserResponse>>.Success(users);
    }
    #endregion

    #region Get User By Id
    public async Task<Result<UserResponse>> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

        if (user is null)
            return Result<UserResponse>.Failure("User not found.");

        var response = new UserResponse
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            MobileNo = user.MobileNo,
            Limit = user.Limit,
            Status = user.Status,
            CreatedAt = new DateTimeOffset(DateTime.SpecifyKind(user.CreatedAt, DateTimeKind.Utc)),
        };
        return Result<UserResponse>.Success(response);
    }
    #endregion

    #region Create User
    public async Task<Result<UserResponse>> CreateAsync(
        CreateUserRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = _validationPolicy.ValidateCreate(request);
        if (validation.IsFailure)
            return Result<UserResponse>.Failure(validation.Message);

        var duplicateCheck = await CheckDuplicatesAsync(request.Email, request.MobileNo, excludeUserId: null, cancellationToken);
        if (duplicateCheck.IsFailure)
            return Result<UserResponse>.Failure(duplicateCheck.Message);

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

        _dbContext.Users.Add(user);

        await _mediator.Publish(new AuditEvent(
            Action: "CREATE",
            ResourceType: "User",
            TargetId: user.Id,
            TargetUserId: user.Id,
            ActorUserId: _currentUser.UserId,
            ActorRole: _currentUser.Role,
            Reason: request.Reason,
            NewStatus: "Pending",
            TargetLookupKey: user.MobileNo ?? user.Id
        ), cancellationToken);

        var response = new UserResponse
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            MobileNo = user.MobileNo,
            Limit = user.Limit,
            Status = user.Status,
            CreatedAt = new DateTimeOffset(DateTime.SpecifyKind(user.CreatedAt, DateTimeKind.Utc))
        };
        return Result<UserResponse>.Success(response, "User created successfully (Pending approval).");
    }
    #endregion

    #region Update User
    public async Task<Result<UserResponse>> UpdateAsync(
        string id,
        UpdateUserRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = _validationPolicy.ValidateUpdate(request);
        if (validation.IsFailure)
            return Result<UserResponse>.Failure(validation.Message);

        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        if (user is null)
            return Result<UserResponse>.Failure("User not found.");

        var duplicateCheck = await CheckDuplicatesAsync(request.Email, request.MobileNo, excludeUserId: id, cancellationToken);
        if (duplicateCheck.IsFailure)
            return Result<UserResponse>.Failure(duplicateCheck.Message);

        var changes = CollectChanges(user, request);
        if (changes.Count == 0)
        {
            var unchangedResponse = new UserResponse
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                MobileNo = user.MobileNo,
                Limit = user.Limit,
                Status = user.Status,
                CreatedAt = new DateTimeOffset(DateTime.SpecifyKind(user.CreatedAt, DateTimeKind.Utc))
            };
            return Result<UserResponse>.Success(unchangedResponse, "No changes detected.");
        }

        user.Name = request.Name.Trim();
        user.Email = request.Email.Trim();
        user.MobileNo = string.IsNullOrWhiteSpace(request.MobileNo) ? null : request.MobileNo.Trim();
        user.Limit = request.Limit;

        await _mediator.Publish(new AuditEvent(
            Action: "UPDATE",
            ResourceType: "User",
            TargetId: user.Id,
            TargetUserId: user.Id,
            ActorUserId: _currentUser.UserId,
            ActorRole: _currentUser.Role,
            Reason: request.Reason,
            Changes: changes,
            TargetLookupKey: user.MobileNo ?? user.Id
        ), cancellationToken);

        var response = new UserResponse
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            MobileNo = user.MobileNo,
            Limit = user.Limit,
            Status = user.Status,
            CreatedAt = new DateTimeOffset(DateTime.SpecifyKind(user.CreatedAt, DateTimeKind.Utc))
        };
        return Result<UserResponse>.Success(response, "User updated successfully.");
    }
    #endregion

    #region Delete User
    public async Task<Result> DeleteAsync(
        string id,
        string? reason = null,
        CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        if (user is null)
            return Result.Failure("User not found.");

        _dbContext.Users.Remove(user);

        await _mediator.Publish(new AuditEvent(
            Action: "DELETE",
            ResourceType: "User",
            TargetId: id,
            TargetUserId: id,
            ActorUserId: _currentUser.UserId,
            ActorRole: _currentUser.Role,
            Reason: reason,
            TargetLookupKey: user.MobileNo ?? id
        ), cancellationToken);

        return Result.Success("User deleted successfully.");
    }
    #endregion

    #region Approve User
    public async Task<Result<UserResponse>> ApproveAsync(
        string id,
        string? reason = null,
        CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        if (user is null)
            return Result<UserResponse>.Failure("User not found.");

        var transition = _validationPolicy.ValidateStatusTransition(user, "Approved");
        if (transition.IsFailure)
            return Result<UserResponse>.Failure(transition.Message);

        var oldStatus = user.Status;
        user.Status = "Approved";

        await _mediator.Publish(new AuditEvent(
            Action: "APPROVE",
            ResourceType: "User",
            TargetId: id,
            TargetUserId: id,
            ActorUserId: _currentUser.UserId,
            ActorRole: _currentUser.Role,
            Reason: reason,
            OldStatus: oldStatus,
            NewStatus: "Approved",
            TargetLookupKey: user.MobileNo ?? id
        ), cancellationToken);

        var response = new UserResponse
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            MobileNo = user.MobileNo,
            Limit = user.Limit,
            Status = user.Status,
            CreatedAt = new DateTimeOffset(DateTime.SpecifyKind(user.CreatedAt, DateTimeKind.Utc))
        };
        return Result<UserResponse>.Success(response, "User approved successfully.");
    }
    #endregion

    #region Reject User
    public async Task<Result<UserResponse>> RejectAsync(
        string id,
        string? reason = null,
        CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        if (user is null)
            return Result<UserResponse>.Failure("User not found.");

        var transition = _validationPolicy.ValidateStatusTransition(user, "Rejected");
        if (transition.IsFailure)
            return Result<UserResponse>.Failure(transition.Message);

        var oldStatus = user.Status;
        user.Status = "Rejected";

        await _mediator.Publish(new AuditEvent(
            Action: "REJECT",
            ResourceType: "User",
            TargetId: id,
            TargetUserId: id,
            ActorUserId: _currentUser.UserId,
            ActorRole: _currentUser.Role,
            Reason: reason,
            OldStatus: oldStatus,
            NewStatus: "Rejected",
            TargetLookupKey: user.MobileNo ?? id
        ), cancellationToken);

        var response = new UserResponse
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            MobileNo = user.MobileNo,
            Limit = user.Limit,
            Status = user.Status,
            CreatedAt = new DateTimeOffset(DateTime.SpecifyKind(user.CreatedAt, DateTimeKind.Utc))
        };
        return Result<UserResponse>.Success(response, "User rejected successfully.");
    }
    #endregion

    private async Task<Result> CheckDuplicatesAsync(
        string email,
        string? mobileNo,
        string? excludeUserId,
        CancellationToken cancellationToken)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var emailExists = await _dbContext.Users
            .AsNoTracking()
            .AnyAsync(
                u => (excludeUserId == null || u.Id != excludeUserId) &&
                     u.Email.ToLower() == normalizedEmail,
                cancellationToken);

        if (emailExists)
            return Result.Failure($"A user with email '{email.Trim()}' already exists.");

        if (!string.IsNullOrWhiteSpace(mobileNo))
        {
            var normalizedMobile = mobileNo.Trim().ToLowerInvariant();
            var mobileExists = await _dbContext.Users
                .AsNoTracking()
                .AnyAsync(
                    u => (excludeUserId == null || u.Id != excludeUserId) &&
                         u.MobileNo != null &&
                         u.MobileNo.ToLower() == normalizedMobile,
                    cancellationToken);

            if (mobileExists)
                return Result.Failure($"A user with mobile number '{mobileNo.Trim()}' already exists.");
        }

        return Result.Success();
    }

    private static List<ChangeRecord> CollectChanges(TblUser existing, UpdateUserRequest updated)
    {
        var changes = new List<ChangeRecord>();

        var name = updated.Name.Trim();
        if (existing.Name != name)
            changes.Add(new("Name", existing.Name, name));

        var email = updated.Email.Trim();
        if (existing.Email != email)
            changes.Add(new("Email", existing.Email, email));

        var mobileNo = string.IsNullOrWhiteSpace(updated.MobileNo) ? null : updated.MobileNo.Trim();
        if (existing.MobileNo != mobileNo)
            changes.Add(new("MobileNo", existing.MobileNo, mobileNo));

        if (existing.Limit != updated.Limit)
            changes.Add(new("Limit", existing.Limit.ToString(), updated.Limit.ToString()));

        return changes;
    }
}
