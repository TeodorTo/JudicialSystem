using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Judicial_system.Models;

namespace Judicial_system.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    public IActionResult SetLanguage(string language)
    {
        if (!string.IsNullOrEmpty(language))
        {
            // Store the selected language in session
            HttpContext.Session.SetString("PreferredLanguage", language);

            // Redirect to Task page if C# is selected, otherwise back to home for now
            if (language.ToLower() == "csharp")
            {
                return RedirectToAction("Index", "Task");

            }
            // For other languages, return to home page (to be implemented later)
            return RedirectToAction("Index");
        }
        return RedirectToAction("Index");
    }

    

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}