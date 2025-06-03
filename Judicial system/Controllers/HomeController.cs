using System.Diagnostics;
using Judicial_system.Data;
using Microsoft.AspNetCore.Authorization; // Добавете това
using Microsoft.AspNetCore.Mvc;
using Judicial_system.Models;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace Judicial_system.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IEmailSender _emailSender;
        public HomeController(ILogger<HomeController> logger, IEmailSender emailSender)
        {
            _logger = logger;
            _emailSender = emailSender;
        }
        
        
        
        
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Index()
        {
            ViewBag.MaintenanceMode = AppState.MaintenanceMode; // Предаваме статуса на изгледа

            var receiver = "theodore130802@gmail.com";
            var subject = "test";
            var message = "Hello World!";
            await _emailSender.SendEmailAsync(receiver, subject, message);
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
        
        [AllowAnonymous] 
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