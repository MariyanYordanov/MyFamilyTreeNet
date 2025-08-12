using System.ComponentModel.DataAnnotations;
using MyFamilyTreeNet.Api.Validation;

namespace MyFamilyTreeNet.Api.DTOs
{
    public class CreateStoryDto
    {
        [Required]
        [MaxLength(200)]
        [NoHtml]
        public required string Title { get; set; }
        
        [Required]
        [MaxLength(10000)]
        public required string Content { get; set; }
        
        [Required]
        [Range(1, int.MaxValue)]
        public int FamilyId { get; set; }
    }

    public class UpdateStoryDto
    {
        [Required]
        [MaxLength(200)]
        [NoHtml]
        public required string Title { get; set; }
        
        [Required]
        [MaxLength(10000)]
        public required string Content { get; set; }
    }

    public class StoryDto
    {
        public int Id { get; set; }
        
        [MaxLength(200)]
        public required string Title { get; set; }
        
        [MaxLength(10000)]
        public required string Content { get; set; }
        
        public DateTime? DateOccurred { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int FamilyId { get; set; }
        
        [ValidFamilyName]
        public required string FamilyName { get; set; }
        
        public required string AuthorId { get; set; }
        
        [ValidPersonName]
        public required string AuthorName { get; set; }
    }

}