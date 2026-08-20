using AuditLogSample.Models;
using AuditLogSample.Services;
using Microsoft.AspNetCore.Mvc;

namespace AuditLogSample.Controllers;

public class UserController : Controller
{
    private readonly IUserService _userService;
    private readonly IAuditLogService _auditLogService;

    public UserController(IUserService userService, IAuditLogService auditLogService)
    {
        _userService = userService;
        _auditLogService = auditLogService;
    }

    // GET: /User
    public async Task<IActionResult> Index()
    {
        var users = await _userService.GetAllAsync();
        return View(users);
    }

    // GET: /User/Create
    public IActionResult Create()
    {
        return View(new CreateUserViewModel());
    }

    // POST: /User/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateUserViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        await _userService.CreateAsync(model);
        TempData["Success"] = "User created successfully (Pending approval).";
        return RedirectToAction(nameof(Index));
    }

    // GET: /User/Edit/5
    public async Task<IActionResult> Edit(string id)
    {
        var user = await _userService.GetByIdAsync(id);
        if (user == null) return NotFound();

        var vm = new UpdateUserViewModel
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            MobileNo = user.MobileNo,
            Limit = user.Limit
        };
        return View(vm);
    }

    // POST: /User/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UpdateUserViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            await _userService.UpdateAsync(model);
            TempData["Success"] = "User updated successfully.";
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }

        return RedirectToAction(nameof(Index));
    }

    // GET: /User/Details/5
    public async Task<IActionResult> Details(string id)
    {
        var user = await _userService.GetByIdAsync(id);
        if (user == null) return NotFound();

        ViewBag.AuditLogs = await _auditLogService.GetByTargetIdAsync(id);
        return View(user);
    }

    // POST: /User/Delete
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id, string? reason)
    {
        try
        {
            await _userService.DeleteAsync(id, reason);
            TempData["Success"] = "User deleted.";
        }
        catch (KeyNotFoundException)
        {
            TempData["Error"] = "User not found.";
        }

        return RedirectToAction(nameof(Index));
    }

    // POST: /User/Approve
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(string id, string? reason)
    {
        try
        {
            await _userService.ApproveAsync(id, reason);
            TempData["Success"] = "User approved.";
        }
        catch (KeyNotFoundException)
        {
            TempData["Error"] = "User not found.";
        }

        return RedirectToAction(nameof(Index));
    }

    // POST: /User/Reject
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(string id, string? reason)
    {
        try
        {
            await _userService.RejectAsync(id, reason);
            TempData["Success"] = "User rejected.";
        }
        catch (KeyNotFoundException)
        {
            TempData["Error"] = "User not found.";
        }

        return RedirectToAction(nameof(Index));
    }
}
