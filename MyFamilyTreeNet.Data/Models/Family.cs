using System.ComponentModel.DataAnnotations;
using static MyFamilyTreeNet.Data.Constants;

namespace MyFamilyTreeNet.Data.Models
{
    public class Family
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(NameLength)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(DescriptionLength)]
        public string? Description { get; set; }

        public bool IsPublic { get; set; } = true;

        [Required]
        public string CreatedByUserId { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual User CreatedBy { get; set; } = null!;
        public virtual ICollection<FamilyMember> FamilyMembers { get; set; } = new List<FamilyMember>();
        public virtual ICollection<Photo> Photos { get; set; } = new List<Photo>();
        public virtual ICollection<Story> Stories { get; set; } = new List<Story>();
    }
}