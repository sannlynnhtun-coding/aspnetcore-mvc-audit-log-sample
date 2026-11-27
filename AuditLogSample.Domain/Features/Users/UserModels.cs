using System.ComponentModel.DataAnnotations;
using AuditLogSample.Domain.Features.AuditLog;

namespace AuditLogSample.Domain.Features.Users;

/// <summary>
/// Payload for creating a new user (Pending approval).
/// </summary>
public sealed record CreateUserRequest
{
    [Required, StringLength(100)]
    public required string Name { get; init; }

    [Required, EmailAddress, StringLength(256)]
    public required string Email { get; init; }

    [Phone, StringLength(32)]
    public string? MobileNo { get; init; }

    [Range(0, 10_000_000)]
    public decimal Limit { get; init; }

    [StringLength(500)]
    public string? Reason { get; init; }
}

/// <summary>
/// Payload for updating an existing user.
/// </summary>
public sealed record UpdateUserRequest
{
    [Required, StringLength(100)]
    public required string Name { get; init; }

    [Required, EmailAddress, StringLength(256)]
    public required string Email { get; init; }

    [Phone, StringLength(32)]
    public string? MobileNo { get; init; }

    [Range(0, 10_000_000)]
    public decimal Limit { get; init; }

    [StringLength(500)]
    public string? Reason { get; init; }
}

/// <summary>
/// Optional reason payload for user workflow actions (Approve, Reject, Delete).
/// </summary>
public sealed record UserActionRequest
{
    [StringLength(500)]
    public string? Reason { get; init; }
}

/// <summary>
/// Represents a user returned by the API.
/// </summary>
public sealed record UserResponse(
    string Id,
    string Name,
    string Email,
    string? MobileNo,
    decimal Limit,
    string Status,
    DateTimeOffset CreatedAt);

/// <summary>
/// Represents a user together with their audit trail.
/// </summary>
public sealed record UserDetailResponse(
    UserResponse User,
    IReadOnlyList<AuditLogResponse> AuditLogs);
