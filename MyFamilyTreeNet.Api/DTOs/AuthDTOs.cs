using System.ComponentModel.DataAnnotations;
using MyFamilyTreeNet.Api.Validation;

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
        [MaxLength(50)]
        [ValidPersonName]
        public required string FirstName { get; set; }
        
        [Required]
        [MaxLength(50)]
        [ValidPersonName]
        public required string MiddleName { get; set; }
        
        [Required]
        [MaxLength(50)]
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
        
        [MaxLength(50)]
        [ValidPersonName]
        public required string FirstName { get; set; }
        
        [MaxLength(50)]
        [ValidPersonName]
        public required string MiddleName { get; set; }
        
        [MaxLength(50)]
        [ValidPersonName]
        public required string LastName { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        public DateTime? DateOfBirth { get; set; }
        
        [MaxLength(1000)]
        public string? Bio { get; set; }
        
        [MaxLength(500)]
        [Url]
        public string? ProfilePictureUrl { get; set; }
    }

    public class UpdateProfileDto
    {
        [Required]
        [MaxLength(50)]
        [ValidPersonName]
        public required string FirstName { get; set; }
        
        [Required]
        [MaxLength(50)]
        [ValidPersonName]
        public required string MiddleName { get; set; }
        
        [Required]
        [MaxLength(50)]
        [ValidPersonName]
        public required string LastName { get; set; }
        
        public string? DateOfBirth { get; set; }
        
        [MaxLength(1000)]
        [NoHtml]
        public string? Bio { get; set; }
        
        [MaxLength(255)]
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