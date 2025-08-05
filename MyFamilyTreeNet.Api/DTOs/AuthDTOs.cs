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
        public required string Email { get; set; }
        public required string Password { get; set; }
        public bool RememberMe { get; set; }
    }

    public class UserDto
    {
        public required string Id { get; set; }
        public required string Email { get; set; }
        public required string FirstName { get; set; }
        public required string MiddleName { get; set; }
        public required string LastName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Bio { get; set; }
        public string? ProfilePictureUrl { get; set; }
    }

    public class UpdateProfileDto
    {
        [Required]
        [MaxLength(50)]
        public required string FirstName { get; set; }
        
        [Required]
        [MaxLength(50)]
        public required string MiddleName { get; set; }
        
        [Required]
        [MaxLength(50)]
        public required string LastName { get; set; }
        
        public string? DateOfBirth { get; set; }
        
        [MaxLength(1000)]
        public string? Bio { get; set; }
        
        [MaxLength(255)]
        public string? ProfilePictureUrl { get; set; }
    }
}