using System.ComponentModel.DataAnnotations;

namespace MyFamilyTreeNet.Data.Models
{
    public class Story
    {
        public int Id { get; set; }

        [Required]
        public int FamilyId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;



        public bool IsPublic { get; set; } = true;

        [Required]
        public string AuthorUserId { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


        public virtual Family Family { get; set; } = null!;
        public virtual User Author { get; set; } = null!;
    }
}