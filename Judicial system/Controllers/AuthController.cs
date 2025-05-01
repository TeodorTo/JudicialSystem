using Judicial_system.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Judicial_system.Controllers;

public class AuthController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly EmailBackgroundService _emailBackgroundService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(UserManager<IdentityUser> userManager, EmailBackgroundService emailBackgroundService, ILogger<AuthController> logger)
    {
        _userManager = userManager;
        _emailBackgroundService = emailBackgroundService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Register(string email, string password)
    {
        _logger.LogInformation("Attempting to register user with email: {Email}", email);

        var user = new IdentityUser { UserName = email, Email = email };
        var result = await _userManager.CreateAsync(user, password);

        if (result.Succeeded)
        {
            _logger.LogInformation("User created successfully: {Email}", email);

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var confirmationLink = Url.Action("ConfirmEmail", "Auth", new
            {
                userId = user.Id,
                token = Uri.EscapeDataString(token) // За безопасен линк
            }, Request.Scheme);

            _logger.LogInformation("Generated confirmation link: {Link}", confirmationLink);

            _emailBackgroundService.QueueEmail(
                email,
                "Потвърждение на имейл",
                $"<p>Моля, потвърди своя имейл чрез този линк: <a href='{confirmationLink}'>Потвърди</a></p>"
            );

            return Content("✅ Регистрацията е успешна. Провери пощата си.");
        }

        // Логваме всяка грешка при неуспешна регистрация
        foreach (var error in result.Errors)
        {
            _logger.LogError("Registration error: {Code} - {Description}", error.Code, error.Description);
        }

        return BadRequest(result.Errors);
    }

    [HttpGet]
    public async Task<IActionResult> ConfirmEmail(string userId, string token)
    {
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
        {
            _logger.LogWarning("Missing userId or token in ConfirmEmail");
            return BadRequest("Липсва информация");
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("User not found for ID: {UserId}", userId);
            return NotFound("Потребителят не съществува");
        }

        var result = await _userManager.ConfirmEmailAsync(user, token);
        if (result.Succeeded)
        {
            _logger.LogInformation("Email confirmed successfully for: {Email}", user.Email);

            _emailBackgroundService.QueueEmail(
                user.Email,
                "Имейл потвърден",
                "<p>Благодарим ти, че потвърди имейла си!</p>"
            );

            return Content("✅ Имейлът е потвърден успешно.");
        }

        _logger.LogError("Failed to confirm email for {Email}", user.Email);
        return BadRequest("❌ Грешка при потвърждение.");
    }
}
