// Controllers/EmailTestController.cs
using Microsoft.AspNetCore.Mvc;
using Judicial_system.Models;
using Judicial_system.Services;
using Microsoft.AspNetCore.Authorization;

namespace Judicial_system.Controllers
{
    [Authorize(Roles = "Admin")]
    public class EmailTestController : Controller
    {
        private readonly IEmailSender _emailSender;
        private readonly ILogger<EmailTestController> _logger;

        public EmailTestController(IEmailSender emailSender, ILogger<EmailTestController> logger)
        {
            _emailSender = emailSender;
            _logger = logger;
        }

        [HttpGet]

        public IActionResult Index()
        {
            return View(new EmailTestViewModel());
        }

        [HttpPost]

        public async Task<IActionResult> Index(EmailTestViewModel model)
        {
            try
            {
                await _emailSender.SendEmailAsync(model.Receiver, model.Subject, model.Message);
                model.StatusMessage = "✅ Имейлът беше изпратен успешно!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Неуспешно изпращане на имейл.");
                model.StatusMessage = $"❌ Грешка: {ex.Message}";
            }

            return View(model);
        }
    }
}