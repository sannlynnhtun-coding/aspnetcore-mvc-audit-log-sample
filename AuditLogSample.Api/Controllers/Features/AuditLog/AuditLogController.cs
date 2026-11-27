using AuditLogSample.Domain.Features.AuditLog;
using AuditLogSample.Shared;
using Microsoft.AspNetCore.Mvc;

namespace AuditLogSample.Api.Controllers.Features.AuditLog;

[ApiController]
[Route("api/audit-logs")]
public sealed class AuditLogController : ControllerBase
{
    private readonly IAuditService _auditService;

    public AuditLogController(IAuditService auditService)
    {
        _auditService = auditService;
    }

    /// <summary>
    /// Retrieves all audit logs ordered by most recent action first.
    /// </summary>
    [HttpGet]
    [ProducesResponseType<Result<List<AuditLogResponse>>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var logs = await _auditService.GetAllAsync(cancellationToken);
        return Ok(Result<List<AuditLogResponse>>.Success(logs));
    }

    /// <summary>
    /// Retrieves audit logs associated with a specific target ID, user ID, email, or mobile lookup key.
    /// </summary>
    [HttpGet("target/{targetId}")]
    [ProducesResponseType<Result<List<AuditLogResponse>>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByTargetId(string targetId, CancellationToken cancellationToken)
    {
        var logs = await _auditService.GetByTargetIdAsync(targetId, cancellationToken);
        return Ok(Result<List<AuditLogResponse>>.Success(logs));
    }
}
