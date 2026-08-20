using System.ComponentModel.DataAnnotations;

namespace AuditLogSample.Models;

public class CreateUserViewModel
{
    [Required, StringLength(100)]
    public string Name { get; set; } = null!;

    [Required, EmailAddress]
    public string Email { get; set; } = null!;

    [Phone, StringLength(32)]
    public string? MobileNo { get; set; }

    [Required, Range(0, 10_000_000)]
    public decimal Limit { get; set; }

    public string? Reason { get; set; }
}

public class UpdateUserViewModel
{
    public string Id { get; set; } = null!;

    [Required, StringLength(100)]
    public string Name { get; set; } = null!;

    [Required, EmailAddress]
    public string Email { get; set; } = null!;

    [Phone, StringLength(32)]
    public string? MobileNo { get; set; }

    [Required, Range(0, 10_000_000)]
    public decimal Limit { get; set; }

    public string? Reason { get; set; }
}

public class ActionReasonViewModel
{
    public string Id { get; set; } = null!;
    public string? Reason { get; set; }
}
