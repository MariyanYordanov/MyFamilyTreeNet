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

        private Relationship? CreateReverseRelationship(Relationship originalRelationship, string userId)
        {
            RelationshipType? reverseType = GetReverseRelationshipType(originalRelationship.RelationshipType);
            
            if (reverseType == null)
                return null;

            return new Relationship
            {
                PrimaryMemberId = originalRelationship.RelatedMemberId,
                RelatedMemberId = originalRelationship.PrimaryMemberId,
                RelationshipType = reverseType.Value,
                Notes = !string.IsNullOrEmpty(originalRelationship.Notes) 
                    ? $"Автоматично създадена обратна връзка: {originalRelationship.Notes}"
                    : "Автоматично създадена обратна връзка",
                CreatedByUserId = userId,
                CreatedAt = DateTime.UtcNow
            };
        }

        private bool IsSymmetricRelationship(RelationshipType type)
        {
            return type switch
            {
                RelationshipType.Spouse => true,
                RelationshipType.Sibling => true,
                RelationshipType.Cousin => true,
                RelationshipType.StepSibling => true,
                RelationshipType.HalfSibling => true,
                _ => false
            };
        }

        private RelationshipType? GetReverseRelationshipType(RelationshipType originalType)
        {
            return originalType switch
            {
                RelationshipType.Parent => RelationshipType.Child,
                RelationshipType.Child => RelationshipType.Parent,
                RelationshipType.Spouse => RelationshipType.Spouse, // Spouse is bidirectional
                RelationshipType.Sibling => RelationshipType.Sibling, // Sibling is bidirectional
                RelationshipType.Grandparent => RelationshipType.Grandchild,
                RelationshipType.Grandchild => RelationshipType.Grandparent,
                RelationshipType.Uncle => RelationshipType.Nephew, // Assuming male, could be more complex
                RelationshipType.Aunt => RelationshipType.Niece, // Assuming female, could be more complex
                RelationshipType.Nephew => RelationshipType.Uncle, // Simplified - could be Uncle or Aunt
                RelationshipType.Niece => RelationshipType.Aunt, // Simplified - could be Uncle or Aunt
                RelationshipType.Cousin => RelationshipType.Cousin, // Cousin is bidirectional
                RelationshipType.GreatGrandparent => RelationshipType.GreatGrandchild,
                RelationshipType.GreatGrandchild => RelationshipType.GreatGrandparent,
                RelationshipType.StepParent => RelationshipType.StepChild,
                RelationshipType.StepChild => RelationshipType.StepParent,
                RelationshipType.StepSibling => RelationshipType.StepSibling, // Bidirectional
                RelationshipType.HalfSibling => RelationshipType.HalfSibling, // Bidirectional
                RelationshipType.Other => null, // Don't auto-create reverse for "Other"
                _ => null
            };
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
            _logger.LogInformation("=== RELATIONSHIP CREATE POST STARTED ===");
            _logger.LogInformation("PrimaryMemberId: {Primary}, RelatedMemberId: {Related}, Type: {Type}", 
                relationship.PrimaryMemberId, relationship.RelatedMemberId, relationship.RelationshipType);
            
            var currentUserId = GetCurrentUserId();
            _logger.LogInformation("Current User ID: {UserId}", currentUserId);

            // Проверка дали членовете принадлежат на потребителя
            _logger.LogInformation("Checking primary member access...");
            var primaryMember = await _context.FamilyMembers
                .Include(m => m.Family)
                .FirstOrDefaultAsync(m => m.Id == relationship.PrimaryMemberId && 
                                         m.Family.CreatedByUserId == currentUserId);
            
            _logger.LogInformation("Primary member found: {Found}", primaryMember != null);
            
            _logger.LogInformation("Checking related member access...");
            var relatedMember = await _context.FamilyMembers
                .Include(m => m.Family)
                .FirstOrDefaultAsync(m => m.Id == relationship.RelatedMemberId && 
                                         m.Family.CreatedByUserId == currentUserId);
            
            _logger.LogInformation("Related member found: {Found}", relatedMember != null);

            if (primaryMember == null || relatedMember == null)
            {
                _logger.LogWarning("Member access check failed");
                ModelState.AddModelError("", "Членовете не са намерени или нямате достъп до тях.");
            }

            // Проверка за дублирана връзка (в двете посоки)
            _logger.LogInformation("Checking for existing relationships...");
            var existingRelationship = await _context.Relationships
                .AnyAsync(r => (r.PrimaryMemberId == relationship.PrimaryMemberId && 
                               r.RelatedMemberId == relationship.RelatedMemberId) ||
                              (r.PrimaryMemberId == relationship.RelatedMemberId &&
                               r.RelatedMemberId == relationship.PrimaryMemberId));

            _logger.LogInformation("Existing relationship found: {Found}", existingRelationship);
            
            if (existingRelationship)
            {
                _logger.LogWarning("Duplicate relationship detected");
                ModelState.AddModelError("", "Връзка между тези членове вече съществува.");
            }

            _logger.LogInformation("ModelState.IsValid: {Valid}", ModelState.IsValid);
            
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ModelState validation failed. Errors:");
                foreach (var error in ModelState)
                {
                    if (error.Value.Errors.Any())
                    {
                        _logger.LogWarning("Field {Field}: {Errors}", error.Key, 
                            string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage)));
                    }
                }
            }
            
            // Remove validation errors for navigation properties and set required fields
            relationship.CreatedByUserId = currentUserId;
            ModelState.Remove("CreatedBy");
            ModelState.Remove("PrimaryMember");
            ModelState.Remove("RelatedMember");
            ModelState.Remove("CreatedByUserId");
            
            if (ModelState.IsValid)
            {
                try
                {
                    _logger.LogInformation("Starting relationship creation process...");
                    relationship.CreatedAt = DateTime.UtcNow;

                    _logger.LogInformation("Adding relationship to context...");
                    _context.Relationships.Add(relationship);

                    // Create reverse relationship automatically only for asymmetric relationships
                    if (!IsSymmetricRelationship(relationship.RelationshipType))
                    {
                        var reverseRelationship = CreateReverseRelationship(relationship, currentUserId);
                        if (reverseRelationship != null)
                        {
                            _logger.LogInformation("Adding reverse relationship to context...");
                            _context.Relationships.Add(reverseRelationship);
                        }
                    }
                    else
                    {
                        _logger.LogInformation("Symmetric relationship - no reverse relationship needed");
                    }

                    _logger.LogInformation("Saving changes to database...");
                    await _context.SaveChangesAsync();
                    
                    _logger.LogInformation("Relationships saved successfully. Creating redirect...");

                    TempData["SuccessMessage"] = "Връзката беше добавена успешно!";
                    
                    _logger.LogInformation("SUCCESS: Relationship created, redirecting to tree view");
                    
                    // Get family ID for redirect - use primaryMember already loaded
                    if (primaryMember?.Family != null)
                    {
                        _logger.LogInformation("Redirecting to FamilyMvc/Details/{FamilyId}?tab=tree", primaryMember.Family.Id);
                        return RedirectToAction("Details", "FamilyMvc", new { id = primaryMember.Family.Id, tab = "tree" });
                    }
                    
                    _logger.LogInformation("Fallback redirect to MemberMvc/Details/{MemberId}", relationship.PrimaryMemberId);
                    return RedirectToAction("Details", "MemberMvc", new { id = relationship.PrimaryMemberId });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating relationship: {Message}", ex.Message);
                    ModelState.AddModelError("", $"Грешка при създаване на връзката: {ex.Message}");
                }
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
                    .ThenInclude(m => m!.Family)
                .Include(r => r.RelatedMember)
                .FirstOrDefaultAsync(r => r.Id == id && 
                                        r.PrimaryMember!.Family!.CreatedByUserId == currentUserId);

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
                    .ThenInclude(m => m!.Family)
                .FirstOrDefaultAsync(r => r.Id == id && 
                                        r.PrimaryMember!.Family!.CreatedByUserId == currentUserId);

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