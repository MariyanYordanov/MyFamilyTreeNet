using System.ComponentModel.DataAnnotations;

namespace MyFamilyTreeNet.Api.Models.ViewModels
{
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Текущата парола е задължителна.")]
        [DataType(DataType.Password)]
        [Display(Name = "Текуща парола")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Новата парола е задължителна.")]
        [StringLength(100, ErrorMessage = "Паролата трябва да бъде поне {2} символа.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Нова парола")]
        public string NewPassword { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Потвърдете новата парола")]
        [Compare("NewPassword", ErrorMessage = "Новата парола и потвърждението не съвпадат.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class ProfileViewModel
    {
        [Display(Name = "Имейл")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Името е задължително.")]
        [StringLength(50, ErrorMessage = "Името не може да надвишава 50 символа.")]
        [Display(Name = "Име")]
        public string FirstName { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "Презимето не може да надвишава 50 символа.")]
        [Display(Name = "Презиме")]
        public string? MiddleName { get; set; }

        [Required(ErrorMessage = "Фамилията е задължителна.")]
        [StringLength(50, ErrorMessage = "Фамилията не може да надвишава 50 символа.")]
        [Display(Name = "Фамилия")]
        public string LastName { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        [Display(Name = "Дата на раждане")]
        public DateTime? DateOfBirth { get; set; }

        [StringLength(500, ErrorMessage = "Биографията не може да надвишава 500 символа.")]
        [Display(Name = "Биография")]
        public string? Bio { get; set; }

        [Display(Name = "Регистриран на")]
        public DateTime CreatedAt { get; set; }
    }
}