using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using MyFamilyTreeNet.Data;
using MyFamilyTreeNet.Api.DTOs;

namespace MyFamilyTreeNet.Api.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(AppDbContext context, ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: /Admin/Home/Index
        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Admin Dashboard";

            try
            {
                // Get dashboard statistics
                var stats = new AdminDashboardDto
                {
                    TotalUsers = await _context.Users.CountAsync(),
                    TotalFamilies = await _context.Families.CountAsync(),
                    TotalMembers = await _context.FamilyMembers.CountAsync(),
                    TotalPhotos = await _context.Photos.CountAsync(),
                    TotalStories = await _context.Stories.CountAsync(),
                    
                    // Recent activity
                    NewUsersThisMonth = await _context.Users
                        .Where(u => u.CreatedAt >= DateTime.UtcNow.AddMonths(-1))
                        .CountAsync(),
                    
                    NewFamiliesToday = await _context.Families
                        .Where(f => f.CreatedAt >= DateTime.UtcNow.Date)
                        .CountAsync(),
                    
                    ActiveUsers = await _context.Users
                        .Where(u => u.LastLoginAt >= DateTime.UtcNow.AddDays(-7))
                        .CountAsync()
                };

                // Recent families
                var recentFamilies = await _context.Families
                    .OrderByDescending(f => f.CreatedAt)
                    .Take(5)
                    .Select(f => new FamilyDto
                    {
                        Id = f.Id,
                        Name = f.Name,
                        Description = f.Description,
                        CreatedAt = f.CreatedAt,
                        CreatedByUserId = f.CreatedByUserId,
                        MemberCount = f.FamilyMembers.Count(),
                        PhotoCount = f.Photos.Count(),
                        StoryCount = f.Stories.Count()
                    })
                    .ToListAsync();

                // Recent users
                var recentUsers = await _context.Users
                    .OrderByDescending(u => u.CreatedAt)
                    .Take(5)
                    .ToListAsync();

                ViewBag.Stats = stats;
                ViewBag.RecentFamilies = recentFamilies;
                ViewBag.RecentUsers = recentUsers;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading admin dashboard");
                TempData["ErrorMessage"] = "Възникна грешка при зареждането на dashboard-а.";
                
                // Return with empty data
                ViewBag.Stats = new AdminDashboardDto();
                ViewBag.RecentFamilies = new List<FamilyDto>();
                ViewBag.RecentUsers = new List<object>();
                
                return View();
            }
        }

        // GET: /Admin/Home/SystemInfo
        public IActionResult SystemInfo()
        {
            ViewData["Title"] = "Системна информация";

            var systemInfo = new
            {
                ServerTime = DateTime.Now,
                ServerTimeUtc = DateTime.UtcNow,
                Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
                MachineName = Environment.MachineName,
                ProcessorCount = Environment.ProcessorCount,
                WorkingSet = Environment.WorkingSet,
                FrameworkVersion = Environment.Version.ToString(),
                OSVersion = Environment.OSVersion.ToString()
            };

            ViewBag.SystemInfo = systemInfo;
            return View();
        }
    }
}