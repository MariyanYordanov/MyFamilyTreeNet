using System.ComponentModel.DataAnnotations;
using MyFamilyTreeNet.Api.Validation;
using static MyFamilyTreeNet.Data.Constants;

namespace MyFamilyTreeNet.Api.DTOs
{
    public class CreateMemberDto
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
        
        [ValidBirthDate]
        public DateTime? DateOfBirth { get; set; }
        
        [ValidDeathDate("DateOfBirth")]
        public DateTime? DateOfDeath { get; set; }
        
        public string? Gender { get; set; }
        
        [MaxLength(BioLength)]
        [NoHtml]
        public string? Biography { get; set; }
        [MaxLength(PlaceLength)]
        [NoHtml]
        public string? PlaceOfBirth { get; set; }
        
        [MaxLength(PlaceLength)]
        [NoHtml]
        public string? PlaceOfDeath { get; set; }
        
        [Required]
        [Range(MinPositiveId, MaxIntValue)]
        public int FamilyId { get; set; }
    }

    public class UpdateMemberDto
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
        
        [ValidBirthDate]
        public DateTime? DateOfBirth { get; set; }
        
        [ValidDeathDate("DateOfBirth")]
        public DateTime? DateOfDeath { get; set; }
        
        public string? Gender { get; set; }
        
        [MaxLength(BioLength)]
        [NoHtml]
        public string? Biography { get; set; }
        [MaxLength(PlaceLength)]
        [NoHtml]
        public string? PlaceOfBirth { get; set; }
        
        [MaxLength(PlaceLength)]
        [NoHtml]
        public string? PlaceOfDeath { get; set; }
    }

    public class FamilyMemberDto
    {
        public int Id { get; set; }
        
        [MaxLength(NameLength)]
        [ValidPersonName]
        public required string FirstName { get; set; }
        
        [MaxLength(NameLength)]
        [ValidPersonName]
        public required string MiddleName { get; set; }
        
        [MaxLength(NameLength)]
        [ValidPersonName]
        public required string LastName { get; set; }
        
        public DateTime? DateOfBirth { get; set; }
        public DateTime? DateOfDeath { get; set; }
        
        public string? Gender { get; set; }
        
        [MaxLength(BioLength)]
        public string? Biography { get; set; }
        
        [MaxLength(PlaceLength)]
        public string? PlaceOfBirth { get; set; }
        
        [MaxLength(PlaceLength)]
        public string? PlaceOfDeath { get; set; }
        
        [MaxLength(NotesLength)]
        [Url]
        public string? ProfileImageUrl { get; set; }
        
        public int? Age { get; set; }
        public int FamilyId { get; set; }
        
        [ValidFamilyName]
        public required string FamilyName { get; set; }
    }

    public class CreateRelationshipDto
    {
        [Required]
        public int PrimaryMemberId { get; set; }

        [Required]
        public int RelatedMemberId { get; set; }

        [Required]
        public MyFamilyTreeNet.Data.Models.RelationshipType RelationshipType { get; set; }

        [MaxLength(NotesLength)]
        [NoHtml]
        public string? Notes { get; set; }
    }

    public class RelationshipDto
    {
        public int Id { get; set; }
        public int PrimaryMemberId { get; set; }
        public int RelatedMemberId { get; set; }
        public MyFamilyTreeNet.Data.Models.RelationshipType RelationshipType { get; set; }
        
        [MaxLength(NotesLength)]
        public string? Notes { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        // Additional fields for display
        [ValidPersonName]
        public string? PrimaryMemberName { get; set; }
        
        [ValidPersonName]
        public string? RelatedMemberName { get; set; }
        
        [MaxLength(RelationshipTypeNameLength)]
        public string? RelationshipTypeName { get; set; }
    }

    public class UpdateRelationshipDto
    {
        [Required]
        public MyFamilyTreeNet.Data.Models.RelationshipType RelationshipType { get; set; }

        [MaxLength(NotesLength)]
        [NoHtml]
        public string? Notes { get; set; }
    }

    public class MemberRelationshipsDto
    {
        public int MemberId { get; set; }
        
        [ValidPersonName]
        public string MemberName { get; set; } = string.Empty;
        
        public List<RelationshipDto> Relationships { get; set; } = new();
    }
}