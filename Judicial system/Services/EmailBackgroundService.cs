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
        _logger.LogInformation("EmailBackgroundService initialized.");
    }

    public void QueueEmail(string email, string subject, string message)
    {
        _emailQueue.Enqueue((email, subject, message, 0));
        _logger.LogInformation("Email queued for sending to {Email}. Subject: {Subject}. Queue count: {QueueCount}", email, subject, _emailQueue.Count);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("EmailBackgroundService started. Entering main loop.");
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogDebug("Checking queue for emails. Queue count: {QueueCount}", _emailQueue.Count);
            if (_emailQueue.TryDequeue(out var emailData))
            {
                _logger.LogInformation("Dequeued email for {Email}. Attempt {RetryCount}/3", emailData.email, emailData.retryCount + 1);
                try
                {
                    _logger.LogInformation("Attempting to send email to {Email}...", emailData.email);
                    await _emailSender.SendEmailAsync(emailData.email, emailData.subject, emailData.message);
                    _logger.LogInformation("Email sent successfully to {Email}", emailData.email);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send email to {Email}. Error: {ErrorMessage}", emailData.email, ex.Message);
                    if (emailData.retryCount < 3)
                    {
                        _emailQueue.Enqueue((emailData.email, emailData.subject, emailData.message, emailData.retryCount + 1));
                        _logger.LogInformation("Email re-queued for retry ({RetryCount}/3) for {Email}", emailData.retryCount + 1, emailData.email);
                    }
                    else
                    {
                        _logger.LogWarning("Email failed to send after 3 attempts to {Email}", emailData.email);
                    }
                }
            }
            else
            {
                _logger.LogDebug("No emails in queue. Waiting...");
            }
            await Task.Delay(1000, stoppingToken);
        }
        _logger.LogInformation("EmailBackgroundService stopped.");
    }
}