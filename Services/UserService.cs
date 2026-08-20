using AuditLogSample.Data;
using AuditLogSample.Events;
using AuditLogSample.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AuditLogSample.Services;

/// <summary>
/// Simple Service + DI for CRUD.
/// Only uses MediatR to publish AuditEvent.
/// </summary>
public class UserService : IUserService
{
    private readonly AuditLogDbContext _dbContext;
    private readonly IMediator _mediator;
    private readonly ICurrentUser _currentUser;

    public UserService(AuditLogDbContext dbContext, IMediator mediator, ICurrentUser currentUser)
    {
        _dbContext = dbContext;
        _mediator = mediator;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<User>> GetAllAsync()
    {
        return await _dbContext.Users
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<User?> GetByIdAsync(string id)
    {
        return await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(user => user.Id == id);
    }

    public async Task<string> CreateAsync(CreateUserViewModel model)
    {
        var user = new User
        {
            Id = Guid.NewGuid().ToString("N")[..12],
            Name = model.Name,
            Email = model.Email,
            MobileNo = model.MobileNo,
            Limit = model.Limit,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Users.Add(user);

        // Publish audit via MediatR only
        await _mediator.Publish(new AuditEvent(
            Action: "CREATE",
            ResourceType: "User",
            TargetId: user.Id,
            TargetUserId: user.Id,
            ActorUserId: _currentUser.UserId,
            ActorRole: _currentUser.Role,
            Reason: model.Reason,
            NewStatus: "Pending",
            TargetLookupKey: user.MobileNo ?? user.Id
        ));

        return user.Id;
    }

    public async Task UpdateAsync(UpdateUserViewModel model)
    {
        var user = await _dbContext.Users.FindAsync(model.Id);
        if (user is null)
            throw new KeyNotFoundException("User not found");

        var dto = new { model.Name, model.Email, model.MobileNo, model.Limit };
        var changes = user.TrackChanges(dto);

        if (!changes.Any()) return;

        // Publish audit via MediatR
        await _mediator.Publish(new AuditEvent(
            Action: "UPDATE",
            ResourceType: "User",
            TargetId: model.Id,
            TargetUserId: model.Id,
            ActorUserId: _currentUser.UserId,
            ActorRole: _currentUser.Role,
            Reason: model.Reason,
            Changes: changes,
            TargetLookupKey: user.MobileNo ?? user.Id
        ));
    }

    public async Task DeleteAsync(string id, string? reason = null)
    {
        var user = await _dbContext.Users.FindAsync(id);
        if (user is null)
            throw new KeyNotFoundException("User not found");

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
        ));
    }

    public async Task ApproveAsync(string id, string? reason = null)
    {
        var user = await _dbContext.Users.FindAsync(id);
        if (user is null)
            throw new KeyNotFoundException("User not found");

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
        ));
    }

    public async Task RejectAsync(string id, string? reason = null)
    {
        var user = await _dbContext.Users.FindAsync(id);
        if (user is null)
            throw new KeyNotFoundException("User not found");

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
        ));
    }
}
