using System.ComponentModel.DataAnnotations;

namespace MyFamilyTreeNet.Data.Models
{
    public class Photo
    {
        public int Id { get; set; }

        [Required]
        public int FamilyId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Required]
        public string ImageUrl { get; set; } = string.Empty;

        public DateTime? DateTaken { get; set; }

        [MaxLength(100)]
        public string? Location { get; set; }



        [Required]
        public string UploadedByUserId { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        public virtual Family Family { get; set; } = null!;
        public virtual User UploadedBy { get; set; } = null!;
    }
}