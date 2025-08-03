using System.ComponentModel.DataAnnotations;

namespace MyFamilyTreeNet.Data.Models
{
    public class Relationship
    {
        public int Id { get; set; }

        [Required]
        public int PrimaryMemberId { get; set; }

        [Required]
        public int RelatedMemberId { get; set; }

        [Required]
        public RelationshipType RelationshipType { get; set; }

        public string? Notes { get; set; }

        [Required]
        public string CreatedByUserId { get; set; } = string.Empty; 

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


        public virtual FamilyMember PrimaryMember { get; set; } = null!;
        public virtual FamilyMember RelatedMember { get; set; } = null!;
        public virtual User CreatedBy { get; set; } = null!;
    }

    public enum RelationshipType
    {
        Parent = 1,
        Child = 2,
        Spouse = 3,
        Sibling = 4,
        Grandparent = 5,
        Grandchild = 6,
        Uncle = 7,
        Aunt = 8,
        Nephew = 9,
        Niece = 10,
        Cousin = 11,
        GreatGrandparent = 12,
        GreatGrandchild = 13,
        StepParent = 14,
        StepChild = 15,
        StepSibling = 16,
        HalfSibling = 17,
        Other = 99
    }
}