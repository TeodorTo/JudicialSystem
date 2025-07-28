using Microsoft.AspNetCore.Identity.UI.Services;

namespace Judicial_system.Services
{
    // Това е системният интерфейс от Microsoft
    public class IdentityEmailSenderAdapter : IEmailSender, Microsoft.AspNetCore.Identity.UI.Services.IEmailSender
    {
        private readonly EmailBackgroundService _emailBackgroundService;

        public IdentityEmailSenderAdapter(EmailBackgroundService emailBackgroundService)
        {
            _emailBackgroundService = emailBackgroundService;
        }

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            _emailBackgroundService.QueueEmail(email, subject, htmlMessage);
            return Task.CompletedTask;
        }
    }
}