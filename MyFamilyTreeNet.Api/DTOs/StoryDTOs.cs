using System.ComponentModel.DataAnnotations;
using MyFamilyTreeNet.Api.Validation;
using static MyFamilyTreeNet.Data.Constants;

namespace MyFamilyTreeNet.Api.DTOs
{
    public class CreateStoryDto
    {
        [Required]
        [MaxLength(TitleLength)]
        [NoHtml]
        public required string Title { get; set; }
        
        [Required]
        [MaxLength(StoryContentLength)]
        public required string Content { get; set; }
        
        [Required]
        [Range(MinPositiveId, MaxIntValue)]
        public int FamilyId { get; set; }
    }

    public class UpdateStoryDto
    {
        [Required]
        [MaxLength(TitleLength)]
        [NoHtml]
        public required string Title { get; set; }
        
        [Required]
        [MaxLength(StoryContentLength)]
        public required string Content { get; set; }
    }

    public class StoryDto
    {
        public int Id { get; set; }
        
        [MaxLength(TitleLength)]
        public required string Title { get; set; }
        
        [MaxLength(StoryContentLength)]
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