using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyFamilyTreeNet.Data;
using MyFamilyTreeNet.Data.Models;
using System.Security.Claims;

namespace MyFamilyTreeNet.Api.Controllers.MVC
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;

        public HomeController(AppDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Welcome to MyFamilyTreeNet";
            
            // Get platform statistics
            var totalFamilies = await _context.Families.CountAsync();
            var totalMembers = await _context.FamilyMembers.CountAsync();
            var totalStories = await _context.Stories.CountAsync();
            
            ViewBag.TotalFamilies = totalFamilies;
            ViewBag.TotalMembers = totalMembers;
            ViewBag.TotalStories = totalStories;
            
            // Get featured families (recent or popular ones) - only public families
            var featuredFamilies = await _context.Families
                .Where(f => !string.IsNullOrEmpty(f.Name) && f.IsPublic)
                .OrderByDescending(f => f.CreatedAt)
                .Take(6)
                .ToListAsync();
                
            ViewBag.FeaturedFamilies = featuredFamilies;
            
            return View();
        }

        [Authorize]
        public async Task<IActionResult> Dashboard()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (currentUserId == null)
                return RedirectToAction("Index");
                
            var user = await _userManager.FindByIdAsync(currentUserId);

            if (user == null)
                return RedirectToAction("Index");

            // Get user statistics
            var familyCount = await _context.Families
                .CountAsync(f => f.CreatedByUserId == currentUserId);

            var memberCount = await _context.FamilyMembers
                .Include(m => m.Family)
                .CountAsync(m => m.Family.CreatedByUserId == currentUserId);

            var photoCount = await _context.Photos
                .Include(p => p.Family)
                .CountAsync(p => p.Family.CreatedByUserId == currentUserId);

            var storyCount = await _context.Stories
                .Include(s => s.Family)
                .CountAsync(s => s.Family.CreatedByUserId == currentUserId);

            ViewBag.FamilyCount = familyCount;
            ViewBag.MemberCount = memberCount;
            ViewBag.PhotoCount = photoCount;
            ViewBag.StoryCount = storyCount;
            ViewBag.UserName = $"{user.FirstName} {user.MiddleName} {user.LastName}";
            ViewBag.UserEmail = user.Email;
            ViewBag.RegistrationDate = user.CreatedAt.ToString("dd.MM.yyyy");

            // Get recent activities
            var recentActivities = new List<dynamic>();

            // Recent families
            var recentFamilies = await _context.Families
                .Where(f => f.CreatedByUserId == currentUserId)
                .OrderByDescending(f => f.CreatedAt)
                .Take(3)
                .Select(f => new { 
                    Type = "family", 
                    Name = f.Name, 
                    CreatedAt = f.CreatedAt,
                    Icon = "fas fa-plus",
                    BgClass = "bg-primary",
                    Action = "Създадохте семейство"
                })
                .ToListAsync();

            // Recent members
            var recentMembers = await _context.FamilyMembers
                .Include(m => m.Family)
                .Where(m => m.Family.CreatedByUserId == currentUserId)
                .OrderByDescending(m => m.CreatedAt)
                .Take(3)
                .Select(m => new {
                    Type = "member",
                    Name = $"{m.FirstName} {m.LastName}",
                    CreatedAt = m.CreatedAt,
                    Icon = "fas fa-user-plus",
                    BgClass = "bg-success",
                    Action = "Добавихте член"
                })
                .ToListAsync();

            // Combine and sort activities
            var allActivities = recentFamilies.Cast<dynamic>()
                .Concat(recentMembers.Cast<dynamic>())
                .OrderByDescending(a => a.CreatedAt)
                .Take(5)
                .ToList();

            ViewBag.RecentActivities = allActivities;

            return View();
        }

        public IActionResult About()
        {
            ViewData["Title"] = "За нас - MyFamilyTreeNet";
            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Title"] = "Контакти - MyFamilyTreeNet";
            return View();
        }

        public IActionResult Help()
        {
            ViewData["Title"] = "Помощ - MyFamilyTreeNet";
            return View();
        }
    }
}