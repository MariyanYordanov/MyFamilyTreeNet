using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MyFamilyTreeNet.Data;
using MyFamilyTreeNet.Data.Models;
using System.Security.Claims;

namespace MyFamilyTreeNet.Api.Controllers.MVC
{
    [Authorize]
    public class RelationshipMvcController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<RelationshipMvcController> _logger;

        public RelationshipMvcController(AppDbContext context, ILogger<RelationshipMvcController> logger)
        {
            _context = context;
            _logger = logger;
        }

        private string GetCurrentUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        }

        // GET: RelationshipMvc/Create?primaryMemberId=1
        public async Task<IActionResult> Create(int primaryMemberId)
        {
            var currentUserId = GetCurrentUserId();
            
            // Проверка дали членът принадлежи на потребителя
            var primaryMember = await _context.FamilyMembers
                .Include(m => m.Family)
                .FirstOrDefaultAsync(m => m.Id == primaryMemberId && m.Family.CreatedByUserId == currentUserId);

            if (primaryMember == null)
            {
                TempData["ErrorMessage"] = "Членът не е намерен или нямате достъп до него.";
                return RedirectToAction("Index", "MemberMvc");
            }

            // Вземи всички членове от същото семейство
            var familyMembers = await _context.FamilyMembers
                .Where(m => m.FamilyId == primaryMember.FamilyId && m.Id != primaryMemberId)
                .OrderBy(m => m.FirstName)
                .ThenBy(m => m.LastName)
                .ToListAsync();

            ViewBag.PrimaryMember = primaryMember;
            ViewBag.RelatedMembers = new SelectList(
                familyMembers.Select(m => new 
                { 
                    m.Id, 
                    FullName = $"{m.FirstName} {m.MiddleName} {m.LastName}".Trim() 
                }),
                "Id",
                "FullName"
            );

            var model = new Relationship
            {
                PrimaryMemberId = primaryMemberId
            };

            return View(model);
        }

        // POST: RelationshipMvc/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Relationship relationship)
        {
            var currentUserId = GetCurrentUserId();

            // Проверка дали членовете принадлежат на потребителя
            var primaryMember = await _context.FamilyMembers
                .Include(m => m.Family)
                .FirstOrDefaultAsync(m => m.Id == relationship.PrimaryMemberId && 
                                         m.Family.CreatedByUserId == currentUserId);

            var relatedMember = await _context.FamilyMembers
                .Include(m => m.Family)
                .FirstOrDefaultAsync(m => m.Id == relationship.RelatedMemberId && 
                                         m.Family.CreatedByUserId == currentUserId);

            if (primaryMember == null || relatedMember == null)
            {
                ModelState.AddModelError("", "Членовете не са намерени или нямате достъп до тях.");
            }

            // Проверка за дублирана връзка
            var existingRelationship = await _context.Relationships
                .AnyAsync(r => r.PrimaryMemberId == relationship.PrimaryMemberId && 
                              r.RelatedMemberId == relationship.RelatedMemberId);

            if (existingRelationship)
            {
                ModelState.AddModelError("", "Тази връзка вече съществува.");
            }

            if (ModelState.IsValid)
            {
                relationship.CreatedAt = DateTime.UtcNow;
                relationship.CreatedByUserId = currentUserId;

                _context.Relationships.Add(relationship);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Връзката беше добавена успешно!";
                
                // Redirect to family tree visualization
                var family = await _context.FamilyMembers
                    .Where(m => m.Id == relationship.PrimaryMemberId)
                    .Select(m => m.Family)
                    .FirstOrDefaultAsync();
                
                if (family != null)
                {
                    return RedirectToAction("Details", "FamilyMvc", new { id = family.Id, tab = "tree" });
                }
                
                return RedirectToAction("Details", "MemberMvc", new { id = relationship.PrimaryMemberId });
            }

            // Reload data for the view
            if (primaryMember != null)
            {
                var familyMembers = await _context.FamilyMembers
                    .Where(m => m.FamilyId == primaryMember.FamilyId && m.Id != relationship.PrimaryMemberId)
                    .OrderBy(m => m.FirstName)
                    .ThenBy(m => m.LastName)
                    .ToListAsync();

                ViewBag.PrimaryMember = primaryMember;
                ViewBag.RelatedMembers = new SelectList(
                    familyMembers.Select(m => new 
                    { 
                        m.Id, 
                        FullName = $"{m.FirstName} {m.MiddleName} {m.LastName}".Trim() 
                    }),
                    "Id",
                    "FullName"
                );
            }

            return View(relationship);
        }

        // GET: RelationshipMvc/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var currentUserId = GetCurrentUserId();
            
            var relationship = await _context.Relationships
                .Include(r => r.PrimaryMember)
                    .ThenInclude(m => m.Family)
                .Include(r => r.RelatedMember)
                .FirstOrDefaultAsync(r => r.Id == id && 
                                        r.PrimaryMember.Family.CreatedByUserId == currentUserId);

            if (relationship == null)
            {
                TempData["ErrorMessage"] = "Връзката не е намерена или нямате достъп до нея.";
                return RedirectToAction("Index", "MemberMvc");
            }

            return View(relationship);
        }

        // POST: RelationshipMvc/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var currentUserId = GetCurrentUserId();
            
            var relationship = await _context.Relationships
                .Include(r => r.PrimaryMember)
                    .ThenInclude(m => m.Family)
                .FirstOrDefaultAsync(r => r.Id == id && 
                                        r.PrimaryMember.Family.CreatedByUserId == currentUserId);

            if (relationship != null)
            {
                var primaryMemberId = relationship.PrimaryMemberId;
                _context.Relationships.Remove(relationship);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Връзката беше премахната успешно!";
                return RedirectToAction("Details", "MemberMvc", new { id = primaryMemberId });
            }

            TempData["ErrorMessage"] = "Връзката не е намерена или нямате достъп до нея.";
            return RedirectToAction("Index", "MemberMvc");
        }
    }
}