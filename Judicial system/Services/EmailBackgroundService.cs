using System.Collections.Concurrent;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Judicial_system.Services;

public class EmailBackgroundService : BackgroundService
{
    private readonly IEmailSender _emailSender;
    private readonly ILogger<EmailBackgroundService> _logger;
    private readonly ConcurrentQueue<(string email, string subject, string message, int retryCount)> _emailQueue = new();

    public EmailBackgroundService(IEmailSender emailSender, ILogger<EmailBackgroundService> logger)
    {
        _emailSender = emailSender;
        _logger = logger;
    }

    // Метод за добавяне на имейл в опашката
    public void QueueEmail(string email, string subject, string message)
    {
        _emailQueue.Enqueue((email, subject, message, 0)); // Първоначално retryCount е 0
        _logger.LogInformation("Имейл добавен в опашката за изпращане до {Email}", email);
    }

    // Метод, който се изпълнява на заден план и обработва опашката
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (_emailQueue.TryDequeue(out var emailData))
            {
                try
                {
                    await _emailSender.SendEmailAsync(emailData.email, emailData.subject, emailData.message);
                    _logger.LogInformation("Имейлът е изпратен успешно до {Email}", emailData.email);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Грешка при изпращане на имейл до {Email}", emailData.email);
                    if (emailData.retryCount < 3) // Максимум 3 опита
                    {
                        _emailQueue.Enqueue((emailData.email, emailData.subject, emailData.message, emailData.retryCount + 1));
                        _logger.LogInformation("Имейлът е добавен за повторен опит ({RetryCount}/3) за {Email}", emailData.retryCount + 1, emailData.email);
                    }
                    else
                    {
                        _logger.LogWarning("Имейлът не можа да бъде изпратен след 3 опита до {Email}", emailData.email);
                        // Тук можеш да добавиш допълнителна логика, напр. да запишеш неуспешния имейл в база данни
                    }
                }
            }
            await Task.Delay(1000, stoppingToken); // Чака 1 секунда преди да провери за следващ имейл
        }
    }
}