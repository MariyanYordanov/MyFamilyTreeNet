using System.ComponentModel.DataAnnotations;
using MyFamilyTreeNet.Api.Validation;

namespace MyFamilyTreeNet.Api.DTOs
{
    public class CreatePhotoDto
    {
        [Required]
        [Url]
        [MaxLength(500)]
        public required string ImageUrl { get; set; }
        
        [MaxLength(200)]
        [NoHtml]
        public string? Title { get; set; }
        
        [MaxLength(1000)]
        [NoHtml]
        public string? Description { get; set; }
        
        public DateTime? DateTaken { get; set; }
        
        [MaxLength(200)]
        [NoHtml]
        public string? Location { get; set; }
        
        [Required]
        [Range(1, int.MaxValue)]
        public int FamilyId { get; set; }
    }

    public class UploadPhotoDto
    {
        [Required]
        public required IFormFile File { get; set; }
        
        [MaxLength(200)]
        [NoHtml]
        public string? Title { get; set; }
        
        [MaxLength(1000)]
        [NoHtml]
        public string? Description { get; set; }
        
        public DateTime? DateTaken { get; set; }
        
        [MaxLength(200)]
        [NoHtml]
        public string? Location { get; set; }
        
        [Required]
        [Range(1, int.MaxValue)]
        public int FamilyId { get; set; }
    }

    public class UpdatePhotoDto
    {
        [MaxLength(200)]
        [NoHtml]
        public string? Title { get; set; }
        
        [MaxLength(1000)]
        [NoHtml]
        public string? Description { get; set; }
        
        public DateTime? DateTaken { get; set; }
        
        [MaxLength(200)]
        [NoHtml]
        public string? Location { get; set; }
    }

    public class PhotoDto
    {
        public int Id { get; set; }
        
        [Url]
        [MaxLength(500)]
        public required string PhotoUrl { get; set; }
        
        [MaxLength(200)]
        public string? Title { get; set; }
        
        [MaxLength(1000)]
        public string? Description { get; set; }
        
        public DateTime? DateTaken { get; set; }
        
        [MaxLength(200)]
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