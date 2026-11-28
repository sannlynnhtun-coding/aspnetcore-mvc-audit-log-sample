using AuditLogSample.Shared;

namespace AuditLogSample.Api;

/// <summary>
/// Resolves the acting user from X-User-Id and X-User-Role request headers,
/// falling back to "maker-001" / "Admin" when omitted.
/// </summary>
public sealed class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string UserId => ReadHeader("X-User-Id") ?? "maker-001";

    public string? Role => ReadHeader("X-User-Role") ?? "Admin";

    private string? ReadHeader(string headerName)
    {
        var headers = _httpContextAccessor.HttpContext?.Request.Headers;
        if (headers is null || !headers.TryGetValue(headerName, out var values))
        {
            return null;
        }

        var value = values.FirstOrDefault()?.Trim();
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }
}
