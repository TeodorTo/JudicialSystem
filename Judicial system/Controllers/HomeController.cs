using System.Diagnostics;
using Microsoft.AspNetCore.Authorization; // Добавете това
using Microsoft.AspNetCore.Mvc;
using Judicial_system.Models;

namespace Judicial_system.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Index()
        {
            ViewBag.MaintenanceMode = AppState.MaintenanceMode; // Предаваме статуса на изгледа
            return View();
        }

        [HttpPost]
        public IActionResult SetLanguage(string language)
        {
            if (!string.IsNullOrEmpty(language))
            {
                HttpContext.Session.SetString("PreferredLanguage", language);
                if (language.ToLower() == "csharp")
                {
                    return RedirectToAction("Index", "Task");
                }
                return RedirectToAction("Index");
            }
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult ToggleMaintenanceMode()
        {
            AppState.MaintenanceMode = !AppState.MaintenanceMode;
            _logger.LogInformation($"MaintenanceMode toggled to: {AppState.MaintenanceMode}");
            return RedirectToAction("Index");
        }
        
        [AllowAnonymous] // Позволява достъп на не-логнати потребители
        public IActionResult Maintenance()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        
        
    }
}