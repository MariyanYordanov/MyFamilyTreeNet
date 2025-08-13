using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyFamilyTreeNet.Api.DTOs;
using MyFamilyTreeNet.Data;
using MyFamilyTreeNet.Data.Models;
using System.Security.Claims;

namespace MyFamilyTreeNet.Api.Controllers.MVC
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

        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            var currentUserId = GetCurrentUserId();
            var isAuthenticated = !string.IsNullOrEmpty(currentUserId);
            
            var family = await _context.Families
                .Where(f => f.Id == id)
                .Include(f => f.FamilyMembers)
                .Include(f => f.Photos)
                .Include(f => f.Stories)
                .FirstOrDefaultAsync();

            if (family == null)
            {
                TempData["ErrorMessage"] = "Семейството не е намерено.";
                return RedirectToAction("Index", "Home");
            }
            
            // Check if user has access to view this family
            if (!family.IsPublic && (!isAuthenticated || family.CreatedByUserId != currentUserId))
            {
                TempData["ErrorMessage"] = "Нямате достъп до това частно семейство.";
                return RedirectToAction("Index", "Home");
            }
            
            // Check if user can edit (only owner can edit)
            ViewBag.CanEdit = isAuthenticated && family.CreatedByUserId == currentUserId;

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
            return View(new CreateFamilyDto { Name = "" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateFamilyDto dto)
        {
            _logger.LogInformation($"Create POST called with Name: {dto.Name}, IsPublic: {dto.IsPublic}");
            
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ModelState is invalid:");
                foreach (var error in ModelState)
                {
                    _logger.LogWarning($"Key: {error.Key}, Errors: {string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage))}");
                }
            }
            
            if (ModelState.IsValid)
            {
                try
                {
                    var currentUserId = GetCurrentUserId();
                    _logger.LogInformation($"Creating family for user: {currentUserId}");
                    
                    var family = new Family
                    {
                        Name = dto.Name,
                        Description = dto.Description,
                        IsPublic = dto.IsPublic,
                        CreatedByUserId = currentUserId,
                        CreatedAt = DateTime.UtcNow
                    };

                    _logger.LogInformation($"Family object created: {family.Name}");
                    
                    _context.Families.Add(family);
                    var result = await _context.SaveChangesAsync();
                    
                    _logger.LogInformation($"SaveChanges result: {result} records affected, Family ID: {family.Id}");

                    TempData["SuccessMessage"] = "Семейството беше създадено успешно!";
                    return RedirectToAction(nameof(Details), new { id = family.Id });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating family");
                    ModelState.AddModelError("", "Възникна грешка при запазването. Моля опитайте отново.");
                }
            }

            return View(dto);
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
                _logger.LogWarning("Family not found for GetFamilyTreeData: {FamilyId}, User: {UserId}", id, currentUserId);
                return Json(new { error = "Семейството не е намерено" });
            }

            _logger.LogInformation("Found family with {MemberCount} members", family.FamilyMembers.Count);

            var relationships = await _context.Relationships
                .Include(r => r.PrimaryMember)
                .Include(r => r.RelatedMember)
                .Where(r => r.PrimaryMember.FamilyId == id)
                .ToListAsync();

            _logger.LogInformation("Found {RelationshipCount} relationships", relationships.Count);

            var treeData = BuildTreeData(family.FamilyMembers.ToList(), relationships);
            _logger.LogInformation("Tree data built: {TreeData}", System.Text.Json.JsonSerializer.Serialize(treeData));
            
            return Json(treeData);
        }

        private object BuildTreeData(List<FamilyMember> members, List<Relationship> relationships)
        {
            if (!members.Any())
                return new { 
                    id = "empty",
                    name = "Няма членове", 
                    children = new object[0] 
                };

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

            // Find root member (oldest by birth or first created)
            var rootMember = members.OrderBy(m => m.DateOfBirth ?? DateTime.MaxValue)
                                   .ThenBy(m => m.CreatedAt)
                                   .First();
            dynamic root = memberDict[rootMember.Id];

            // If we have relationships, build hierarchical tree
            if (relationships.Any())
            {
                BuildChildrenRecursive(root, members, relationships, memberDict, new HashSet<int>());
            }
            else
            {
                // If no relationships, add all other members as siblings at root level
                foreach (var member in members.Where(m => m.Id != rootMember.Id))
                {
                    ((List<object>)root.children).Add(memberDict[member.Id]);
                }
            }

            return root;
        }

        private void BuildChildrenRecursive(dynamic node, List<FamilyMember> members, List<Relationship> relationships, Dictionary<int, dynamic> memberMap, HashSet<int> visited)
        {
            if (visited.Contains(node.id)) return;
            visited.Add(node.id);

            // Find all relationships where this node is the primary member
            var nodeRelationships = relationships
                .Where(r => r.PrimaryMemberId == node.id || r.RelatedMemberId == node.id)
                .ToList();

            foreach (var rel in nodeRelationships)
            {
                int relatedId = rel.PrimaryMemberId == node.id ? rel.RelatedMemberId : rel.PrimaryMemberId;
                
                if (memberMap.ContainsKey(relatedId) && !visited.Contains(relatedId))
                {
                    var relatedNode = memberMap[relatedId];
                    
                    // Add relationship type information to the related node
                    relatedNode.relationshipType = rel.RelationshipType.ToString();
                    relatedNode.relationshipFromPrimary = rel.PrimaryMemberId == node.id;
                    
                    // For tree structure, add as child if it's a child relationship
                    if (rel.RelationshipType == RelationshipType.Child && rel.PrimaryMemberId == node.id)
                    {
                        ((List<object>)node.children).Add(relatedNode);
                        BuildChildrenRecursive(relatedNode, members, relationships, memberMap, visited);
                    }
                    // For parent relationships, add current node as child of parent
                    else if (rel.RelationshipType == RelationshipType.Parent && rel.RelatedMemberId == node.id)
                    {
                        // This will be handled when we process the parent node
                    }
                    // For siblings, spouses etc., add them at the same level (as children of same parent or special handling)
                    else if (rel.RelationshipType == RelationshipType.Sibling || rel.RelationshipType == RelationshipType.Spouse)
                    {
                        ((List<object>)node.children).Add(relatedNode);
                    }
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