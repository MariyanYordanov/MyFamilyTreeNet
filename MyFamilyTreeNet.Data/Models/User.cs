using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using static MyFamilyTreeNet.Data.Constants;

namespace MyFamilyTreeNet.Data.Models
{
    public class User : IdentityUser<string>
    {
        public User()
        {
            Id = Guid.NewGuid().ToString();
            CreatedAt = DateTime.UtcNow;
        }

        [Required]
        [MaxLength(NameLenght)]
        [Required(ErrorMessage = RequireField)]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = RequireField)]
        [MaxLength(NameLenght)]
        public string MiddleName { get; set; } = string.Empty;

        [Required(ErrorMessage = RequireField)]
        [MaxLength(NameLenght)]
        public string LastName { get; set; } = string.Empty;

        public string? ProfilePictureUrl { get; set; }

        public DateTime? DateOfBirth { get; set; }
        
        [MaxLength(BioLenght)]
        public string? Bio { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? LastLoginAt { get; set; }

        public virtual ICollection<Family> CreatedFamilies { get; set; } = new List<Family>();
        public virtual ICollection<FamilyMember> FamilyMembers { get; set; } = new List<FamilyMember>();
        public virtual ICollection<Story> Stories { get; set; } = new List<Story>();
    }
}