using System.ComponentModel.DataAnnotations;
using AuditLogSample.Domain.Features.AuditLog;

namespace AuditLogSample.Domain.Features.Users;

/// <summary>
/// Payload for creating a new user (Pending approval).
/// </summary>
public class CreateUserRequest
{
    [Required, StringLength(100)]
    public required string Name { get; set; }

    [Required, EmailAddress, StringLength(256)]
    public required string Email { get; set; }

    [Phone, StringLength(32)]
    public string? MobileNo { get; set; }

    [Range(0, 10_000_000)]
    public decimal Limit { get; set; }

    [StringLength(500)]
    public string? Reason { get; set; }
}

/// <summary>
/// Payload for updating an existing user.
/// </summary>
public class UpdateUserRequest
{
    [Required, StringLength(100)]
    public required string Name { get; set; }

    [Required, EmailAddress, StringLength(256)]
    public required string Email { get; set; }

    [Phone, StringLength(32)]
    public string? MobileNo { get; set; }

    [Range(0, 10_000_000)]
    public decimal Limit { get; set; }

    [StringLength(500)]
    public string? Reason { get; set; }
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
public sealed record UserResponse
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? MobileNo { get; init; } = string.Empty;
    public decimal Limit { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; }
    public List<AuditLogResponse>? AuditLogs { get; init; }
}
