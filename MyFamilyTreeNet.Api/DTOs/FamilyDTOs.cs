using System.ComponentModel.DataAnnotations;
using MyFamilyTreeNet.Api.Validation;

namespace MyFamilyTreeNet.Api.DTOs
{
    public class CreateFamilyDto
    {
        [Required]
        [ValidFamilyName]
        public required string Name { get; set; }
        
        [MaxLength(1000)]
        [NoHtml]
        public string? Description { get; set; }
    }

    public class UpdateFamilyDto
    {
        [Required]
        [ValidFamilyName]
        public required string Name { get; set; }
        
        [MaxLength(1000)]
        [NoHtml]
        public string? Description { get; set; }
    }

    public class FamilyDto
    {
        public int Id { get; set; }
        
        [ValidFamilyName]
        public required string Name { get; set; }
        
        [MaxLength(1000)]
        public string? Description { get; set; }
        
        public DateTime CreatedAt { get; set; }
        public int MemberCount { get; set; }
        public int PhotoCount { get; set; }
        public int StoryCount { get; set; }
        public required string CreatedByUserId { get; set; }
    }
}