using System.ComponentModel.DataAnnotations;
using MyFamilyTreeNet.Api.Validation;
using static MyFamilyTreeNet.Data.Constants;

namespace MyFamilyTreeNet.Api.DTOs
{
    public class RegisterDto
    {
        [Required]
        [EmailAddress]
        public required string Email { get; set; }
        
        [Required]
        [StrongPassword]
        public required string Password { get; set; }
        
        [Required]
        [MaxLength(NameLength)]
        [ValidPersonName]
        public required string FirstName { get; set; }
        
        [Required]
        [MaxLength(NameLength)]
        [ValidPersonName]
        public required string MiddleName { get; set; }
        
        [Required]
        [MaxLength(NameLength)]
        [ValidPersonName]
        public required string LastName { get; set; }
    }

    public class LoginDto
    {
        [Required]
        [EmailAddress]
        public required string Email { get; set; }
        
        [Required]
        public required string Password { get; set; }
        
        public bool RememberMe { get; set; }
    }

    public class UserDto
    {
        public required string Id { get; set; }
        
        [EmailAddress]
        public required string Email { get; set; }
        
        [MaxLength(NameLength)]
        [ValidPersonName]
        public required string FirstName { get; set; }
        
        [MaxLength(NameLength)]
        [ValidPersonName]
        public required string MiddleName { get; set; }
        
        [MaxLength(NameLength)]
        [ValidPersonName]
        public required string LastName { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        public DateTime? DateOfBirth { get; set; }
        
        [MaxLength(BioLength)]
        public string? Bio { get; set; }
        
        [MaxLength(ProfilePictureUrlLength)]
        [Url]
        public string? ProfilePictureUrl { get; set; }
    }

    public class UpdateProfileDto
    {
        [Required]
        [MaxLength(NameLength)]
        [ValidPersonName]
        public required string FirstName { get; set; }
        
        [Required]
        [MaxLength(NameLength)]
        [ValidPersonName]
        public required string MiddleName { get; set; }
        
        [Required]
        [MaxLength(NameLength)]
        [ValidPersonName]
        public required string LastName { get; set; }
        
        public string? DateOfBirth { get; set; }
        
        [MaxLength(BioLength)]
        [NoHtml]
        public string? Bio { get; set; }
        
        [MaxLength(ShortProfilePictureUrlLength)]
        public string? ProfilePictureUrl { get; set; }
    }


    public class ChangePasswordDto
    {
        [Required]
        public required string CurrentPassword { get; set; }
        
        [Required]
        [StrongPassword]
        public required string NewPassword { get; set; }
        
        [Required]
        [Compare("NewPassword")]
        public required string ConfirmNewPassword { get; set; }
    }
}