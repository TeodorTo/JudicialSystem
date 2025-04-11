namespace Judicial_system.Services;

public interface IEmailSender
{
    Task SendEmailAsync(string email, string subject, string message);
}