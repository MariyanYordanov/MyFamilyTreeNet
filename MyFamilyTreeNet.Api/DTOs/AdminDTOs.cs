using System.ComponentModel.DataAnnotations;

namespace MyFamilyTreeNet.Api.DTOs
{
    public class AdminDashboardDto
    {
        public int TotalUsers { get; set; }
        public int TotalFamilies { get; set; }
        public int TotalMembers { get; set; }
        public int TotalPhotos { get; set; }
        public int TotalStories { get; set; }
        public int NewUsersThisMonth { get; set; }
        public int NewFamiliesToday { get; set; }
        public int ActiveUsers { get; set; }
    }

    public class AdminUserDto
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string MiddleName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
        public int FamiliesCount { get; set; }
        public bool IsActive { get; set; }
    }

    public class UserRoleUpdateDto
    {
        [Required]
        public string UserId { get; set; } = string.Empty;
        
        [Required]
        public string Role { get; set; } = string.Empty;
    }
}