using System.ComponentModel.DataAnnotations;

namespace MyFamilyTreeNet.Api.ViewModels
{
    public class LoginViewModel
{
    [Required(ErrorMessage = "Email адресът е задължителен")]
    [EmailAddress(ErrorMessage = "Невалиден email адрес")]
    public required string Email { get; set; }

    [Required(ErrorMessage = "Паролата е задължителна")]
    public required string Password { get; set; }

    public bool RememberMe { get; set; }
}
}