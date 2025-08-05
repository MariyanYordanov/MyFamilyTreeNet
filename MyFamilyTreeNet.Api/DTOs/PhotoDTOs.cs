namespace MyFamilyTreeNet.Api.DTOs
{
    public class CreatePhotoDto
    {
        public required string ImageUrl { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public DateTime? DateTaken { get; set; }
        public string? Location { get; set; }
        public int FamilyId { get; set; }
    }

    public class UploadPhotoDto
    {
        public required IFormFile File { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public DateTime? DateTaken { get; set; }
        public string? Location { get; set; }
        public int FamilyId { get; set; }
    }

    public class UpdatePhotoDto
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public DateTime? DateTaken { get; set; }
        public string? Location { get; set; }
    }

    public class PhotoDto
    {
        public int Id { get; set; }
        public required string PhotoUrl { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public DateTime? DateTaken { get; set; }
        public string? Location { get; set; }
        public DateTime UploadedAt { get; set; }
        public int FamilyId { get; set; }
        public required string FamilyName { get; set; }
        public required string UploadedByUserId { get; set; }
        public required string UploadedByName { get; set; }
    }
}