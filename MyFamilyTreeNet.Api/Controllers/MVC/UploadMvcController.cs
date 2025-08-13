using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyFamilyTreeNet.Data;
using System.Security.Claims;

namespace MyFamilyTreeNet.Api.Controllers.MVC
{
    [Authorize]
    public class UploadMvcController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<UploadMvcController> _logger;
        private readonly long _maxFileSize = 5 * 1024 * 1024; // 5MB
        private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

        public UploadMvcController(AppDbContext context, IWebHostEnvironment environment, ILogger<UploadMvcController> logger)
        {
            _context = context;
            _environment = environment;
            _logger = logger;
        }

        private string GetCurrentUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        }

        [HttpGet]
        public async Task<IActionResult> MemberPhoto(int memberId)
        {
            var currentUserId = GetCurrentUserId();
            var member = await _context.FamilyMembers
                .Include(m => m.Family)
                .FirstOrDefaultAsync(m => m.Id == memberId && m.Family.CreatedByUserId == currentUserId);

            if (member == null)
            {
                TempData["ErrorMessage"] = "Членът не е намерен или нямате достъп до него.";
                return RedirectToAction("Index", "MemberMvc");
            }

            ViewBag.Member = member;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MemberPhoto(int memberId, IFormFile photo)
        {
            var currentUserId = GetCurrentUserId();
            var member = await _context.FamilyMembers
                .Include(m => m.Family)
                .FirstOrDefaultAsync(m => m.Id == memberId && m.Family.CreatedByUserId == currentUserId);

            if (member == null)
            {
                TempData["ErrorMessage"] = "Членът не е намерен или нямате достъп до него.";
                return RedirectToAction("Index", "MemberMvc");
            }

            if (photo == null || photo.Length == 0)
            {
                TempData["ErrorMessage"] = "Моля изберете снимка за качване.";
                ViewBag.Member = member;
                return View();
            }

            // Validate file size
            if (photo.Length > _maxFileSize)
            {
                TempData["ErrorMessage"] = "Размерът на файла не може да надвишава 5MB.";
                ViewBag.Member = member;
                return View();
            }

            // Validate file extension
            var extension = Path.GetExtension(photo.FileName).ToLowerInvariant();
            if (!_allowedExtensions.Contains(extension))
            {
                TempData["ErrorMessage"] = "Позволени са само снимки с разширения: " + string.Join(", ", _allowedExtensions);
                ViewBag.Member = member;
                return View();
            }

            try
            {
                // Create uploads directory if it doesn't exist
                var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads", "members");
                Directory.CreateDirectory(uploadsPath);

                // Generate unique filename
                var fileName = $"member_{memberId}_{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadsPath, fileName);

                // Delete old photo if exists
                if (!string.IsNullOrEmpty(member.ProfilePictureUrl))
                {
                    var oldPhotoPath = Path.Combine(_environment.WebRootPath, member.ProfilePictureUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldPhotoPath))
                    {
                        System.IO.File.Delete(oldPhotoPath);
                    }
                }

                // Save new photo
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await photo.CopyToAsync(stream);
                }

                // Update member record
                member.ProfilePictureUrl = $"/uploads/members/{fileName}";
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Снимката беше качена успешно!";
                return RedirectToAction("Details", "MemberMvc", new { id = memberId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading member photo for member {MemberId}", memberId);
                TempData["ErrorMessage"] = "Възникна грешка при качването на снимката. Моля опитайте отново.";
                ViewBag.Member = member;
                return View();
            }
        }

        [HttpGet]
        public async Task<IActionResult> FamilyPhoto(int familyId)
        {
            var currentUserId = GetCurrentUserId();
            var family = await _context.Families
                .FirstOrDefaultAsync(f => f.Id == familyId && f.CreatedByUserId == currentUserId);

            if (family == null)
            {
                TempData["ErrorMessage"] = "Семейството не е намерено или нямате достъп до него.";
                return RedirectToAction("Index", "FamilyMvc");
            }

            ViewBag.Family = family;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FamilyPhoto(int familyId, IFormFile photo)
        {
            var currentUserId = GetCurrentUserId();
            var family = await _context.Families
                .FirstOrDefaultAsync(f => f.Id == familyId && f.CreatedByUserId == currentUserId);

            if (family == null)
            {
                TempData["ErrorMessage"] = "Семейството не е намерено или нямате достъп до него.";
                return RedirectToAction("Index", "FamilyMvc");
            }

            if (photo == null || photo.Length == 0)
            {
                TempData["ErrorMessage"] = "Моля изберете снимка за качване.";
                ViewBag.Family = family;
                return View();
            }

            // Validate file size
            if (photo.Length > _maxFileSize)
            {
                TempData["ErrorMessage"] = "Размерът на файла не може да надвишава 5MB.";
                ViewBag.Family = family;
                return View();
            }

            // Validate file extension
            var extension = Path.GetExtension(photo.FileName).ToLowerInvariant();
            if (!_allowedExtensions.Contains(extension))
            {
                TempData["ErrorMessage"] = "Позволени са само снимки с разширения: " + string.Join(", ", _allowedExtensions);
                ViewBag.Family = family;
                return View();
            }

            try
            {
                // Create uploads directory if it doesn't exist
                var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads", "families");
                Directory.CreateDirectory(uploadsPath);

                // Generate unique filename
                var fileName = $"family_{familyId}_{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadsPath, fileName);

                // Delete old photo if exists
                if (!string.IsNullOrEmpty(family.PhotoUrl))
                {
                    var oldPhotoPath = Path.Combine(_environment.WebRootPath, family.PhotoUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldPhotoPath))
                    {
                        System.IO.File.Delete(oldPhotoPath);
                    }
                }

                // Save new photo
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await photo.CopyToAsync(stream);
                }

                // Update family record
                family.PhotoUrl = $"/uploads/families/{fileName}";
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Снимката беше качена успешно!";
                return RedirectToAction("Details", "FamilyMvc", new { id = familyId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading family photo for family {FamilyId}", familyId);
                TempData["ErrorMessage"] = "Възникна грешка при качването на снимката. Моля опитайте отново.";
                ViewBag.Family = family;
                return View();
            }
        }
    }
}