using System.ComponentModel.DataAnnotations;
using static MyFamilyTreeNet.Data.Constants;

namespace MyFamilyTreeNet.Data.Models
{
    public class FamilyMember
    {
        public int Id { get; set; }

        [Required(ErrorMessage = RequireField)]
        public int FamilyId { get; set; }

        [Required(ErrorMessage = RequireField)]
        [MaxLength(NameLength)]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = RequireField)]
        [MaxLength(NameLength)]
        public string MiddleName { get; set; } = string.Empty;

        [Required(ErrorMessage = RequireField)]
        [MaxLength(NameLength)]
        public string LastName { get; set; } = string.Empty;

        public DateTime? DateOfBirth { get; set; }

        public DateTime? DateOfDeath { get; set; }

        [MaxLength(PlacesLength)]
        public string? PlaceOfBirth { get; set; }

        [MaxLength(PlacesLength)]
        public string? PlaceOfDeath { get; set; }

        public Gender Gender { get; set; } = Gender.Unknown;

        [MaxLength(BioLength)]
        public string? Biography { get; set; }

        [MaxLength(500)]
        public string? ProfilePictureUrl { get; set; }

        [Required]
        public string AddedByUserId { get; set; } = string.Empty;

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