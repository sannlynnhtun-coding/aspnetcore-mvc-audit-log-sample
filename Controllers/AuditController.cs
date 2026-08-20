using AuditLogSample.Services;
using Microsoft.AspNetCore.Mvc;

namespace AuditLogSample.Controllers;

public class AuditController : Controller
{
    private readonly IAuditLogService _auditLogService;

    public AuditController(IAuditLogService auditLogService)
    {
        _auditLogService = auditLogService;
    }

    public async Task<IActionResult> Index()
    {
        var logs = await _auditLogService.GetAllAsync();
        return View(logs);
    }
}
