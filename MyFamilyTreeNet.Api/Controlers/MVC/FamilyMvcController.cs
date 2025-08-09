using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyFamilyTreeNet.Data;
using MyFamilyTreeNet.Data.Models;
using System.Security.Claims;

namespace MyFamilyTreeNet.Api.Controlers.MVC
{
    [Authorize]
    public class FamilyMvcController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<FamilyMvcController> _logger;

        public FamilyMvcController(AppDbContext context, ILogger<FamilyMvcController> logger)
        {
            _context = context;
            _logger = logger;
        }

        private string GetCurrentUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        }

        public async Task<IActionResult> Index()
        {
            var currentUserId = GetCurrentUserId();
            var families = await _context.Families
                .Where(f => f.CreatedByUserId == currentUserId)
                .Include(f => f.FamilyMembers)
                .Include(f => f.Photos)
                .Include(f => f.Stories)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();

            return View(families);
        }

        public async Task<IActionResult> Details(int id)
        {
            var currentUserId = GetCurrentUserId();
            var family = await _context.Families
                .Where(f => f.Id == id && f.CreatedByUserId == currentUserId)
                .Include(f => f.FamilyMembers)
                .Include(f => f.Photos)
                .Include(f => f.Stories)
                .FirstOrDefaultAsync();

            if (family == null)
            {
                TempData["ErrorMessage"] = "Семейството не е намерено или нямате достъп до него.";
                return RedirectToAction(nameof(Index));
            }

            var relationships = await _context.Relationships
                .Include(r => r.PrimaryMember)
                .Include(r => r.RelatedMember)
                .Where(r => r.PrimaryMember.FamilyId == id || r.RelatedMember.FamilyId == id)
                .ToListAsync();

            ViewBag.Relationships = relationships;
            ViewBag.Statistics = CalculateStatistics(family.FamilyMembers.ToList());

            return View(family);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Family family)
        {
            if (ModelState.IsValid)
            {
                var currentUserId = GetCurrentUserId();
                family.CreatedByUserId = currentUserId;
                family.CreatedAt = DateTime.UtcNow;

                _context.Families.Add(family);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Семейството беше създадено успешно!";
                return RedirectToAction(nameof(Details), new { id = family.Id });
            }

            return View(family);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var currentUserId = GetCurrentUserId();
            var family = await _context.Families
                .FirstOrDefaultAsync(f => f.Id == id && f.CreatedByUserId == currentUserId);

            if (family == null)
            {
                TempData["ErrorMessage"] = "Семейството не е намерено или нямате достъп до него.";
                return RedirectToAction(nameof(Index));
            }

            return View(family);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Family family)
        {
            if (id != family.Id)
            {
                return NotFound();
            }

            var currentUserId = GetCurrentUserId();
            var existingFamily = await _context.Families
                .FirstOrDefaultAsync(f => f.Id == id && f.CreatedByUserId == currentUserId);

            if (existingFamily == null)
            {
                TempData["ErrorMessage"] = "Семейството не е намерено или нямате достъп до него.";
                return RedirectToAction(nameof(Index));
            }

            if (ModelState.IsValid)
            {
                try
                {
                    existingFamily.Name = family.Name;
                    existingFamily.Description = family.Description;
                    existingFamily.IsPublic = family.IsPublic;

                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Семейството беше обновено успешно!";
                    return RedirectToAction(nameof(Details), new { id = family.Id });
                }
                catch (DbUpdateConcurrencyException)
                {
                    TempData["ErrorMessage"] = "Възникна грешка при обновяването. Моля опитайте отново.";
                }
            }

            return View(family);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var currentUserId = GetCurrentUserId();
            var family = await _context.Families
                .Include(f => f.FamilyMembers)
                .FirstOrDefaultAsync(f => f.Id == id && f.CreatedByUserId == currentUserId);

            if (family == null)
            {
                TempData["ErrorMessage"] = "Семейството не е намерено или нямате достъп до него.";
                return RedirectToAction(nameof(Index));
            }

            return View(family);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var currentUserId = GetCurrentUserId();
            var family = await _context.Families
                .FirstOrDefaultAsync(f => f.Id == id && f.CreatedByUserId == currentUserId);

            if (family != null)
            {
                _context.Families.Remove(family);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Семейството беше изтрито успешно!";
            }
            else
            {
                TempData["ErrorMessage"] = "Семейството не е намерено или нямате достъп до него.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> GetFamilyTreeData(int id)
        {
            var currentUserId = GetCurrentUserId();
            var family = await _context.Families
                .Where(f => f.Id == id && f.CreatedByUserId == currentUserId)
                .Include(f => f.FamilyMembers)
                .FirstOrDefaultAsync();

            if (family == null)
            {
                return Json(new { error = "Семейството не е намерено" });
            }

            var relationships = await _context.Relationships
                .Include(r => r.PrimaryMember)
                .Include(r => r.RelatedMember)
                .Where(r => r.PrimaryMember.FamilyId == id)
                .ToListAsync();

            var treeData = BuildTreeData(family.FamilyMembers.ToList(), relationships);
            return Json(treeData);
        }

        private object BuildTreeData(List<FamilyMember> members, List<Relationship> relationships)
        {
            if (!members.Any())
                return new { name = "Няма членове", children = new object[0] };

            var memberDict = new Dictionary<int, dynamic>();
            foreach (var member in members)
            {
                memberDict[member.Id] = new 
                {
                    id = member.Id,
                    name = $"{member.FirstName} {member.LastName}",
                    birthYear = member.DateOfBirth?.Year,
                    deathYear = member.DateOfDeath?.Year,
                    isAlive = member.DateOfDeath == null,
                    age = CalculateAge(member.DateOfBirth, member.DateOfDeath),
                    children = new List<object>()
                };
            }

            var rootMember = members.OrderBy(m => m.DateOfBirth ?? DateTime.MaxValue).First();
            dynamic root = memberDict[rootMember.Id];

            BuildChildrenRecursive(root, members, relationships, memberDict, new HashSet<int>());

            return root;
        }

        private void BuildChildrenRecursive(dynamic node, List<FamilyMember> members, List<Relationship> relationships, Dictionary<int, dynamic> memberMap, HashSet<int> visited)
        {
            if (visited.Contains(node.id)) return;
            visited.Add(node.id);

            var childRelationships = relationships
                .Where(r => r.PrimaryMemberId == node.id && r.RelationshipType == RelationshipType.Child)
                .ToList();

            foreach (var rel in childRelationships)
            {
                if (memberMap.ContainsKey(rel.RelatedMemberId) && !visited.Contains(rel.RelatedMemberId))
                {
                    var childNode = memberMap[rel.RelatedMemberId];
                    ((List<object>)node.children).Add(childNode);
                    BuildChildrenRecursive(childNode, members, relationships, memberMap, visited);
                }
            }
        }

        private object CalculateStatistics(List<FamilyMember> members)
        {
            var totalMembers = members.Count;
            var aliveMembers = members.Count(m => m.DateOfDeath == null);
            var deceasedMembers = totalMembers - aliveMembers;

            var ages = members
                .Where(m => m.DateOfBirth.HasValue)
                .Select(m => CalculateAge(m.DateOfBirth, m.DateOfDeath))
                .Where(age => age.HasValue)
                .Select(age => age!.Value)
                .ToList();

            var averageAge = ages.Any() ? (int)Math.Round(ages.Average()) : 0;

            var generations = members
                .Where(m => m.DateOfBirth.HasValue)
                .Select(m => (DateTime.Now.Year - m.DateOfBirth!.Value.Year) / 25)
                .DefaultIfEmpty(0)
                .Max() + 1;

            return new
            {
                TotalMembers = totalMembers,
                AliveMembers = aliveMembers,
                DeceasedMembers = deceasedMembers,
                AverageAge = averageAge,
                Generations = generations
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