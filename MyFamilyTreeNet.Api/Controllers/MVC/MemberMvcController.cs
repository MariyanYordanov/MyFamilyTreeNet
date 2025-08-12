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
    public class MemberMvcController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<MemberMvcController> _logger;

        public MemberMvcController(AppDbContext context, ILogger<MemberMvcController> logger)
        {
            _context = context;
            _logger = logger;
        }

        private string GetCurrentUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        }

        public async Task<IActionResult> Index(int? familyId, string? search, int page = 1, int pageSize = 20)
        {
            var currentUserId = GetCurrentUserId();
            var query = _context.FamilyMembers
                .Include(m => m.Family)
                .Where(m => m.Family.CreatedByUserId == currentUserId);

            if (familyId.HasValue)
            {
                query = query.Where(m => m.FamilyId == familyId.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchLower = search.ToLower();
                query = query.Where(m => 
                    m.FirstName.ToLower().Contains(searchLower) ||
                    m.MiddleName.ToLower().Contains(searchLower) ||
                    m.LastName.ToLower().Contains(searchLower) ||
                    (m.Biography != null && m.Biography.ToLower().Contains(searchLower)));
            }

            var totalCount = await query.CountAsync();
            var members = await query
                .OrderBy(m => m.FirstName)
                .ThenBy(m => m.LastName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var families = await _context.Families
                .Where(f => f.CreatedByUserId == currentUserId)
                .OrderBy(f => f.Name)
                .ToListAsync();

            ViewBag.Families = new SelectList(families, "Id", "Name", familyId);
            ViewBag.Search = search;
            ViewBag.FamilyId = familyId;
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalCount = totalCount;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            return View(members);
        }

        public async Task<IActionResult> Details(int id)
        {
            var currentUserId = GetCurrentUserId();
            var member = await _context.FamilyMembers
                .Include(m => m.Family)
                .Include(m => m.AddedBy)
                .FirstOrDefaultAsync(m => m.Id == id && m.Family.CreatedByUserId == currentUserId);

            if (member == null)
            {
                TempData["ErrorMessage"] = "Членът не е намерен или нямате достъп до него.";
                return RedirectToAction(nameof(Index));
            }

            var relationships = await _context.Relationships
                .Include(r => r.PrimaryMember)
                .Include(r => r.RelatedMember)
                .Where(r => r.PrimaryMemberId == id || r.RelatedMemberId == id)
                .ToListAsync();

            ViewBag.Relationships = relationships;
            ViewBag.Statistics = CalculateMemberStatistics(member, relationships);

            return View(member);
        }

        public async Task<IActionResult> Create(int? familyId)
        {
            var currentUserId = GetCurrentUserId();
            var families = await _context.Families
                .Where(f => f.CreatedByUserId == currentUserId)
                .OrderBy(f => f.Name)
                .ToListAsync();

            if (!families.Any())
            {
                TempData["ErrorMessage"] = "Първо трябва да създадете семейство преди да добавяте членове.";
                return RedirectToAction("Index", "FamilyMvc");
            }

            ViewBag.Families = new SelectList(families, "Id", "Name", familyId);

            var member = new FamilyMember();
            if (familyId.HasValue)
            {
                member.FamilyId = familyId.Value;
            }

            return View(member);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FamilyMember member)
        {
            var currentUserId = GetCurrentUserId();

            // Verify family ownership
            var family = await _context.Families
                .FirstOrDefaultAsync(f => f.Id == member.FamilyId && f.CreatedByUserId == currentUserId);

            if (family == null)
            {
                ModelState.AddModelError("FamilyId", "Избраното семейство не е валидно.");
            }

            if (member.DateOfBirth.HasValue && member.DateOfDeath.HasValue && member.DateOfBirth > member.DateOfDeath)
            {
                ModelState.AddModelError("DateOfDeath", "Датата на смърт не може да бъде преди датата на раждане.");
            }

            if (ModelState.IsValid)
            {
                member.AddedByUserId = currentUserId;
                member.CreatedAt = DateTime.UtcNow;

                _context.FamilyMembers.Add(member);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Членът беше добавен успешно!";
                return RedirectToAction(nameof(Details), new { id = member.Id });
            }

            // Reload families for the view
            var families = await _context.Families
                .Where(f => f.CreatedByUserId == currentUserId)
                .OrderBy(f => f.Name)
                .ToListAsync();
            ViewBag.Families = new SelectList(families, "Id", "Name", member.FamilyId);

            return View(member);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var currentUserId = GetCurrentUserId();
            var member = await _context.FamilyMembers
                .Include(m => m.Family)
                .FirstOrDefaultAsync(m => m.Id == id && m.Family.CreatedByUserId == currentUserId);

            if (member == null)
            {
                TempData["ErrorMessage"] = "Членът не е намерен или нямате достъп до него.";
                return RedirectToAction(nameof(Index));
            }

            var families = await _context.Families
                .Where(f => f.CreatedByUserId == currentUserId)
                .OrderBy(f => f.Name)
                .ToListAsync();
            ViewBag.Families = new SelectList(families, "Id", "Name", member.FamilyId);

            return View(member);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, FamilyMember member)
        {
            if (id != member.Id)
            {
                return NotFound();
            }

            var currentUserId = GetCurrentUserId();
            var existingMember = await _context.FamilyMembers
                .Include(m => m.Family)
                .FirstOrDefaultAsync(m => m.Id == id && m.Family.CreatedByUserId == currentUserId);

            if (existingMember == null)
            {
                TempData["ErrorMessage"] = "Членът не е намерен или нямате достъп до него.";
                return RedirectToAction(nameof(Index));
            }

            // Verify family ownership
            var family = await _context.Families
                .FirstOrDefaultAsync(f => f.Id == member.FamilyId && f.CreatedByUserId == currentUserId);

            if (family == null)
            {
                ModelState.AddModelError("FamilyId", "Избраното семейство не е валидно.");
            }

            if (member.DateOfBirth.HasValue && member.DateOfDeath.HasValue && member.DateOfBirth > member.DateOfDeath)
            {
                ModelState.AddModelError("DateOfDeath", "Датата на смърт не може да бъде преди датата на раждане.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    existingMember.FirstName = member.FirstName;
                    existingMember.MiddleName = member.MiddleName;
                    existingMember.LastName = member.LastName;
                    existingMember.DateOfBirth = member.DateOfBirth;
                    existingMember.DateOfDeath = member.DateOfDeath;
                    existingMember.PlaceOfBirth = member.PlaceOfBirth;
                    existingMember.PlaceOfDeath = member.PlaceOfDeath;
                    existingMember.Gender = member.Gender;
                    existingMember.Biography = member.Biography;
                    existingMember.FamilyId = member.FamilyId;

                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Членът беше обновен успешно!";
                    return RedirectToAction(nameof(Details), new { id = member.Id });
                }
                catch (DbUpdateConcurrencyException)
                {
                    TempData["ErrorMessage"] = "Възникна грешка при обновяването. Моля опитайте отново.";
                }
            }

            // Reload families for the view
            var families = await _context.Families
                .Where(f => f.CreatedByUserId == currentUserId)
                .OrderBy(f => f.Name)
                .ToListAsync();
            ViewBag.Families = new SelectList(families, "Id", "Name", member.FamilyId);

            return View(member);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var currentUserId = GetCurrentUserId();
            var member = await _context.FamilyMembers
                .Include(m => m.Family)
                .FirstOrDefaultAsync(m => m.Id == id && m.Family.CreatedByUserId == currentUserId);

            if (member == null)
            {
                TempData["ErrorMessage"] = "Членът не е намерен или нямате достъп до него.";
                return RedirectToAction(nameof(Index));
            }

            // Check for relationships
            var hasRelationships = await _context.Relationships
                .AnyAsync(r => r.PrimaryMemberId == id || r.RelatedMemberId == id);
            ViewBag.HasRelationships = hasRelationships;

            return View(member);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var currentUserId = GetCurrentUserId();
            var member = await _context.FamilyMembers
                .Include(m => m.Family)
                .FirstOrDefaultAsync(m => m.Id == id && m.Family.CreatedByUserId == currentUserId);

            if (member != null)
            {
                // Check for relationships
                var hasRelationships = await _context.Relationships
                    .AnyAsync(r => r.PrimaryMemberId == id || r.RelatedMemberId == id);

                if (hasRelationships)
                {
                    TempData["ErrorMessage"] = "Не може да се изтрие член, който има семейни връзки. Моля премахнете връзките първо.";
                    return RedirectToAction(nameof(Delete), new { id });
                }

                _context.FamilyMembers.Remove(member);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Членът беше изтрит успешно!";
            }
            else
            {
                TempData["ErrorMessage"] = "Членът не е намерен или нямате достъп до него.";
            }

            return RedirectToAction(nameof(Index));
        }

        private object CalculateMemberStatistics(FamilyMember member, List<Relationship> relationships)
        {
            var age = CalculateAge(member.DateOfBirth, member.DateOfDeath);
            var relationshipCount = relationships.Count;

            var relationshipTypes = relationships
                .GroupBy(r => r.RelationshipType)
                .ToDictionary(g => g.Key.ToString(), g => g.Count());

            return new
            {
                Age = age,
                RelationshipCount = relationshipCount,
                RelationshipTypes = relationshipTypes,
                IsAlive = member.DateOfDeath == null,
                YearsInSystem = (DateTime.Now - member.CreatedAt).Days / 365
            };
        }

        private int? CalculateAge(DateTime? birthDate, DateTime? deathDate)
        {
            if (!birthDate.HasValue) return null;

            var endDate = deathDate ?? DateTime.Now;
            var age = endDate.Year - birthDate.Value.Year;

            if (endDate < birthDate.Value.AddYears(age))
                age--;

            return age > 0 ? age : 0;
        }
    }
}