using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace Judicial_system.Services;

public class EmailSender : IEmailSender
{
    private readonly string _SmtpServer;
    private readonly int _Port;
    private readonly string _SenderEmail;
    private readonly string _SenderPassword;

    public EmailSender(IConfiguration configuration)
    {
        var emailSetting = configuration.GetSection("EmailSettings");
        _SmtpServer = emailSetting["SmtpServer"];
        _Port = int.Parse(emailSetting["Port"]);
        _SenderEmail = emailSetting["SenderEmail"];
        _SenderPassword = emailSetting["SenderPassword"];
    }
    
    
    public async Task SendEmailAsync(string email, string subject, string message)
    {
        var emailMessage = new MimeMessage();
        emailMessage.From.Add(new MailboxAddress("Judicial System", _SenderEmail));
        emailMessage.To.Add(MailboxAddress.Parse(email));
        emailMessage.Subject = subject;
        emailMessage.Body = new TextPart("plain")
        {
            Text = message
        };

        using var smtp = new SmtpClient();
        try
        {
            await smtp.ConnectAsync(_SmtpServer, _Port, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_SenderEmail, _SenderPassword); // App password
            await smtp.SendAsync(emailMessage);
            await smtp.DisconnectAsync(true);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error sending email: " + ex.Message);
            throw;
        }
    }
}