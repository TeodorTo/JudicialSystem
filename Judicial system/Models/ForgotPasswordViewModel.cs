using System.ComponentModel.DataAnnotations;

namespace Judicial_system.Models;

public class ForgotPasswordViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }
}
