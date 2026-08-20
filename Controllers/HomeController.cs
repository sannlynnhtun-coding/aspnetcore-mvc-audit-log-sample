using Microsoft.AspNetCore.Mvc;

namespace AuditLogSample.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return RedirectToAction("Index", "User");
    }

    public IActionResult Error()
    {
        return View();
    }
}
