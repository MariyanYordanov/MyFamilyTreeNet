using System.ComponentModel.DataAnnotations;

namespace MyFamilyTreeNet.Data.Models
{
    public class FamilyMember
    {
        public int Id { get; set; }

        [Required]
        public int FamilyId { get; set; }

        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; } = "";

        [Required]
        [MaxLength(50)]
        public string MiddleName { get; set; } = "";

        [Required]
        [MaxLength(50)]
        public string LastName { get; set; } = "";

        public DateTime? DateOfBirth { get; set; }

        public DateTime? DateOfDeath { get; set; }

        [MaxLength(100)]
        public string? PlaceOfBirth { get; set; }

        [MaxLength(100)]
        public string? PlaceOfDeath { get; set; }

        public Gender Gender { get; set; } = Gender.Unknown;

        [MaxLength(2000)]
        public string? Biography { get; set; }

        public string? ProfileImageUrl { get; set; }

        [Required]
        public string AddedByUserId { get; set; } = "";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
      
        public virtual Family Family { get; set; } = null!;
        public virtual User AddedBy { get; set; } = null!;

        // Relationships where this member is the primary person
        public virtual ICollection<Relationship> RelationshipsAsPrimary { get; set; } = new List<Relationship>();

        // Relationships where this member is the related person
        public virtual ICollection<Relationship> RelationshipsAsRelated { get; set; } = new List<Relationship>();
    }

    public enum Gender
    {
        Unknown = 0,
        Male = 1,
        Female = 2,
        Other = 3
    }
}