using Microsoft.AspNetCore.Mvc;
using Judicial_system.Models;
using Judicial_system.Services;
using Microsoft.AspNetCore.Authorization;

namespace Judicial_system.Controllers
{
    [Authorize(Roles = "Admin")]
    public class EmailTestController : Controller
    {
        private readonly EmailBackgroundService _emailBackgroundService;
        private readonly ILogger<EmailTestController> _logger;

        public EmailTestController(EmailBackgroundService emailBackgroundService, ILogger<EmailTestController> logger)
        {
            _emailBackgroundService = emailBackgroundService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(new EmailTestViewModel());
        }

        [HttpPost]
        public IActionResult Index(EmailTestViewModel model)
        {
            try
            {
                _emailBackgroundService.QueueEmail(model.Receiver, model.Subject, model.Message);
                model.StatusMessage = "✅ Имейлът е добавен в опашката за изпращане!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Грешка при добавяне на имейл в опашката.");
                model.StatusMessage = $"❌ Грешка: {ex.Message}";
            }

            return View(model);
        }
    }
}