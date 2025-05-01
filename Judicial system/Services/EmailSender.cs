using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Logging;

namespace Judicial_system.Services;

public class EmailSender : IEmailSender
{
    private readonly string _SmtpServer;
    private readonly int _Port;
    private readonly string _SenderEmail;
    private readonly string _SenderPassword;
    private readonly ILogger<EmailSender> _logger;

    public EmailSender(IConfiguration configuration, ILogger<EmailSender> logger)
    {
        var emailSetting = configuration.GetSection("EmailSettings");
        _SmtpServer = "smtp.gmail.com";
        _Port = 587;
        _SenderEmail = emailSetting["SenderEmail"];
        _SenderPassword = emailSetting["SenderPassword"];
        _logger = logger;
        _logger.LogInformation("EmailSender initialized with SMTP server: {SmtpServer}, Port: {Port}, Sender: {SenderEmail}", _SmtpServer, _Port, _SenderEmail);
    }

    public async Task SendEmailAsync(string email, string subject, string message)
    {
        _logger.LogInformation("Preparing to send email to {Email}. Subject: {Subject}", email, subject);

        var emailMessage = new MimeMessage();
        emailMessage.From.Add(new MailboxAddress("Judicial System", _SenderEmail));
        emailMessage.To.Add(MailboxAddress.Parse(email));
        emailMessage.Subject = subject;
        emailMessage.Body = new TextPart("html")
        {
            Text = message
        };


        using var smtp = new SmtpClient();
        _logger.LogInformation("Connecting to SMTP server {SmtpServer}:{Port}...", _SmtpServer, _Port);
        await smtp.ConnectAsync(_SmtpServer, _Port, SecureSocketOptions.StartTls);
        _logger.LogInformation("Connected to SMTP server. Authenticating...");
        await smtp.AuthenticateAsync(_SenderEmail, _SenderPassword);
        _logger.LogInformation("Authenticated successfully. Sending email...");
        await smtp.SendAsync(emailMessage);
        _logger.LogInformation("Email sent successfully to {Email}", email);
        _logger.LogInformation("Disconnecting from SMTP server...");
        await smtp.DisconnectAsync(true);
        _logger.LogInformation("Disconnected from SMTP server.");
    }
}