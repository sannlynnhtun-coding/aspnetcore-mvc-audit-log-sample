using AuditLogSample.Domain.Features.Users;
using AuditLogSample.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace AuditLogSample.Api.Controllers.Features.Users;

[ApiController]
[Route("api/users")]
public sealed class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    /// Retrieves all users ordered by creation date descending.
    /// </summary>
    [HttpGet]
    [ProducesResponseType<Result<List<UserResponse>>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _userService.GetAllAsync(cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>
    /// Retrieves a single user by ID along with their audit history.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType<Result<UserResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result<UserResponse>>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetById(string id, CancellationToken cancellationToken)
    {
        var result = await _userService.GetByIdAsync(id, cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>
    /// Creates a new user in Pending status and records a CREATE audit log.
    /// </summary>
    [HttpPost]
    [ProducesResponseType<Result<UserResponse>>(StatusCodes.Status201Created)]
    [ProducesResponseType<Result<UserResponse>>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request, CancellationToken cancellationToken)
    {
        var result = await _userService.CreateAsync(request, cancellationToken);
        if (!result.IsSuccess)
        {
            return ToActionResult(result);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }

    /// <summary>
    /// Updates an existing user and records changed fields in an UPDATE audit log.
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType<Result<UserResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result<UserResponse>>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(
        string id,
        [FromBody] UpdateUserRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _userService.UpdateAsync(id, request, cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>
    /// Deletes a user and records a DELETE audit log.
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType<Result>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Delete(
        string id,
        [FromQuery] string? reason = null,
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] UserActionRequest? body = null,
        CancellationToken cancellationToken = default)
    {
        var resolvedReason = !string.IsNullOrWhiteSpace(body?.Reason) ? body.Reason : reason;
        var result = await _userService.DeleteAsync(id, resolvedReason, cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>
    /// Approves a user and records an APPROVE audit log.
    /// </summary>
    [HttpPost("{id}/approve")]
    [ProducesResponseType<Result<UserResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result<UserResponse>>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Approve(
        string id,
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] UserActionRequest? request = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _userService.ApproveAsync(id, request?.Reason, cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>
    /// Rejects a user and records a REJECT audit log.
    /// </summary>
    [HttpPost("{id}/reject")]
    [ProducesResponseType<Result<UserResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result<UserResponse>>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Reject(
        string id,
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] UserActionRequest? request = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _userService.RejectAsync(id, request?.Reason, cancellationToken);
        return ToActionResult(result);
    }

    private IActionResult ToActionResult(Result result) =>
        result.IsSuccess ? Ok(result) : BadRequest(result);
}
