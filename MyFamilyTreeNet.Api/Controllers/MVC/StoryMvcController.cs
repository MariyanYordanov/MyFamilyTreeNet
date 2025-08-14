using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyFamilyTreeNet.Data;
using MyFamilyTreeNet.Data.Models;
using System.Security.Claims;

namespace MyFamilyTreeNet.Api.Controllers.MVC
{
    [Authorize]
    public class StoryMvcController : Controller
    {
        private readonly AppDbContext _context;

        public StoryMvcController(AppDbContext context)
        {
            _context = context;
        }

        private string GetCurrentUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        }

        public async Task<IActionResult> Create(int familyId)
        {
            var currentUserId = GetCurrentUserId();
            var family = await _context.Families
                .FirstOrDefaultAsync(f => f.Id == familyId && f.CreatedByUserId == currentUserId);

            if (family == null)
            {
                TempData["ErrorMessage"] = "Семейството не е намерено или нямате достъп до него.";
                return RedirectToAction("Index", "FamilyMvc");
            }

            var story = new Story 
            { 
                FamilyId = familyId,
                AuthorUserId = currentUserId
            };
            
            ViewBag.FamilyName = family.Name;
            return View(story);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Story story)
        {
            var currentUserId = GetCurrentUserId();
            var family = await _context.Families
                .FirstOrDefaultAsync(f => f.Id == story.FamilyId && f.CreatedByUserId == currentUserId);

            if (family == null)
            {
                TempData["ErrorMessage"] = "Семейството не е намерено или нямате достъп до него.";
                return RedirectToAction("Index", "FamilyMvc");
            }

            ModelState.Remove("Family");
            ModelState.Remove("Author");
            ModelState.Remove("AuthorUserId");

            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState)
                {
                    if (error.Value?.Errors.Count > 0)
                    {
                        foreach (var err in error.Value.Errors)
                        {
                            TempData["ErrorMessage"] = $"Грешка в {error.Key}: {err.ErrorMessage}";
                            break;
                        }
                        break;
                    }
                }
            }

            if (ModelState.IsValid)
            {
                story.AuthorUserId = currentUserId;
                story.CreatedAt = DateTime.Now;

                _context.Stories.Add(story);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Историята беше добавена успешно!";
                return RedirectToAction("Details", "FamilyMvc", new { id = story.FamilyId });
            }

            ViewBag.FamilyName = family.Name;
            return View(story);
        }
    }
}