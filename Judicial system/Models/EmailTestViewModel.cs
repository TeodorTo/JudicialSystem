// Models/EmailTestViewModel.cs
namespace Judicial_system.Models
{
    public class EmailTestViewModel
    {
        public string Receiver { get; set; } = "theodore130802@gmail.com";
        public string Subject { get; set; } = "Тестов имейл";
        public string Message { get; set; } = "Здравей! Това е тест от EmailSender в главното приложение.";
        public string? StatusMessage { get; set; }
    }
}
