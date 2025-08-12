using System.ComponentModel.DataAnnotations;
using MyFamilyTreeNet.Api.Validation;
using static MyFamilyTreeNet.Data.Constants;

namespace MyFamilyTreeNet.Api.DTOs
{
    public class CreatePhotoDto
    {
        [Required]
        [Url]
        [MaxLength(ProfilePictureUrlLength)]
        public required string ImageUrl { get; set; }
        
        [MaxLength(TitleLength)]
        [NoHtml]
        public string? Title { get; set; }
        
        [MaxLength(DescriptionLength)]
        [NoHtml]
        public string? Description { get; set; }
        
        public DateTime? DateTaken { get; set; }
        
        [MaxLength(TitleLength)]
        [NoHtml]
        public string? Location { get; set; }
        
        [Required]
        [Range(MinPositiveId, MaxIntValue)]
        public int FamilyId { get; set; }
    }

    public class UploadPhotoDto
    {
        [Required]
        public required IFormFile File { get; set; }
        
        [MaxLength(TitleLength)]
        [NoHtml]
        public string? Title { get; set; }
        
        [MaxLength(DescriptionLength)]
        [NoHtml]
        public string? Description { get; set; }
        
        public DateTime? DateTaken { get; set; }
        
        [MaxLength(TitleLength)]
        [NoHtml]
        public string? Location { get; set; }
        
        [Required]
        [Range(MinPositiveId, MaxIntValue)]
        public int FamilyId { get; set; }
    }

    public class UpdatePhotoDto
    {
        [MaxLength(TitleLength)]
        [NoHtml]
        public string? Title { get; set; }
        
        [MaxLength(DescriptionLength)]
        [NoHtml]
        public string? Description { get; set; }
        
        public DateTime? DateTaken { get; set; }
        
        [MaxLength(TitleLength)]
        [NoHtml]
        public string? Location { get; set; }
    }

    public class PhotoDto
    {
        public int Id { get; set; }
        
        [Url]
        [MaxLength(ProfilePictureUrlLength)]
        public required string PhotoUrl { get; set; }
        
        [MaxLength(TitleLength)]
        public string? Title { get; set; }
        
        [MaxLength(DescriptionLength)]
        public string? Description { get; set; }
        
        public DateTime? DateTaken { get; set; }
        
        [MaxLength(TitleLength)]
        public string? Location { get; set; }
        
        public DateTime UploadedAt { get; set; }
        public int FamilyId { get; set; }
        
        [ValidFamilyName]
        public required string FamilyName { get; set; }
        
        public required string UploadedByUserId { get; set; }
        
        [ValidPersonName]
        public required string UploadedByName { get; set; }
    }
}