using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Judicial_system.Controllers;

[Authorize(Roles = "Admin")]
public class ChatController : Controller
{
    public IActionResult Chat()
    {
        return View();
    }
}