using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Judicial_system.Controllers;

public class VoiceController : Controller
{
    [Authorize] 
    public IActionResult Voice()
    {
        var userId = User.Identity?.Name ?? "anonymous";
        ViewBag.UserId = userId;
        return View();
    }
}