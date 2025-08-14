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

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var currentUserId = GetCurrentUserId();
            var isAuthenticated = !string.IsNullOrEmpty(currentUserId);
            
            var families = await _context.Families
                .Where(f => !isAuthenticated ? f.IsPublic : (f.CreatedByUserId == currentUserId || f.IsPublic))
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
                .Where(r => r.PrimaryMember!.FamilyId == id || r.RelatedMember!.FamilyId == id)
                .ToListAsync();

            ViewBag.Relationships = relationships;
            var statistics = CalculateStatistics(family.FamilyMembers.ToList());
            _logger.LogInformation("Statistics calculated: {Statistics}", System.Text.Json.JsonSerializer.Serialize(statistics));
            ViewBag.Statistics = statistics;

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
                        CreatedAt = DateTime.Now
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
            _logger.LogInformation("=== EDIT GET ACTION STARTED === ID: {FamilyId}", id);
            var currentUserId = GetCurrentUserId();
            _logger.LogInformation("Current User ID: {UserId}", currentUserId);
            
            var family = await _context.Families
                .Include(f => f.FamilyMembers)
                .Include(f => f.Photos)
                .Include(f => f.Stories)
                .FirstOrDefaultAsync(f => f.Id == id && f.CreatedByUserId == currentUserId);

            if (family == null)
            {
                _logger.LogWarning("Family not found or no access - ID: {FamilyId}, User: {UserId}", id, currentUserId);
                TempData["ErrorMessage"] = "Семейството не е намерено или нямате достъп до него.";
                return RedirectToAction(nameof(Index));
            }

            _logger.LogInformation("Edit GET successful - Family: {FamilyName}", family.Name);
            return View(family);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Family family)
        {
            _logger.LogInformation("=== EDIT POST ACTION STARTED === ID: {FamilyId}, Family Name: {FamilyName}", id, family?.Name ?? "null");
            _logger.LogInformation("Received Family data: ID={Id}, Name={Name}, CreatedByUserId={CreatedByUserId}, CreatedAt={CreatedAt}", 
                family?.Id, family?.Name, family?.CreatedByUserId, family?.CreatedAt);
            
            if (id != family?.Id)
            {
                _logger.LogWarning("ID mismatch - Route ID: {RouteId}, Model ID: {ModelId}", id, family?.Id);
                return NotFound();
            }

            var currentUserId = GetCurrentUserId();
            _logger.LogInformation("Current User ID: {UserId}", currentUserId);
            
            var existingFamily = await _context.Families
                .FirstOrDefaultAsync(f => f.Id == id && f.CreatedByUserId == currentUserId);

            if (existingFamily == null)
            {
                _logger.LogWarning("Existing family not found for edit - ID: {FamilyId}, User: {UserId}", id, currentUserId);
                TempData["ErrorMessage"] = "Семейството не е намерено или нямате достъп до него.";
                return RedirectToAction(nameof(Index));
            }

            // Remove navigation properties from ModelState validation
            ModelState.Remove("CreatedBy");
            ModelState.Remove("FamilyMembers");
            ModelState.Remove("Photos");
            ModelState.Remove("Stories");
            
            _logger.LogInformation("ModelState.IsValid: {IsValid}", ModelState.IsValid);
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ModelState validation errors:");
                foreach (var error in ModelState)
                {
                    if (error.Value?.Errors.Count > 0)
                    {
                        _logger.LogWarning("Field: {Field}, Errors: {Errors}", error.Key, string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage)));
                    }
                }
            }
            
            if (ModelState.IsValid)
            {
                try
                {
                    _logger.LogInformation("Updating family - Old Name: {OldName}, New Name: {NewName}", existingFamily.Name, family.Name);
                    existingFamily.Name = family.Name;
                    existingFamily.Description = family.Description;
                    existingFamily.IsPublic = family.IsPublic;

                    var result = await _context.SaveChangesAsync();
                    _logger.LogInformation("Edit SaveChanges result: {Result} records affected", result);
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
        public async Task<IActionResult> DeleteConfirmed(int id, bool deleteAll = false)
        {
            var currentUserId = GetCurrentUserId();
            var family = await _context.Families
                .Include(f => f.FamilyMembers)
                .FirstOrDefaultAsync(f => f.Id == id && f.CreatedByUserId == currentUserId);

            if (family != null)
            {
                // Check if family has members
                if (family.FamilyMembers.Any() && !deleteAll)
                {
                    var memberCount = family.FamilyMembers.Count;
                    TempData["ErrorMessage"] = $"Не може да се изтрие семейство с {memberCount} членове. Моля потвърдете изтриването на всички данни.";
                    ViewBag.HasMembers = true;
                    ViewBag.MemberCount = memberCount;
                    return View("Delete", family);
                }

                // Delete all relationships for family members
                var memberIds = family.FamilyMembers.Select(m => m.Id).ToList();
                if (memberIds.Any())
                {
                    var relationships = await _context.Relationships
                        .Where(r => memberIds.Contains(r.PrimaryMemberId) || memberIds.Contains(r.RelatedMemberId))
                        .ToListAsync();
                    
                    if (relationships.Any())
                    {
                        _context.Relationships.RemoveRange(relationships);
                        _logger.LogInformation("Deleted {Count} relationships for family {FamilyId}", relationships.Count, id);
                    }
                }

                // Delete all family members
                if (family.FamilyMembers.Any())
                {
                    _context.FamilyMembers.RemoveRange(family.FamilyMembers);
                    _logger.LogInformation("Deleted {Count} members for family {FamilyId}", family.FamilyMembers.Count, id);
                }

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
                .Where(f => f.Id == id && (f.IsPublic || f.CreatedByUserId == currentUserId))
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
                .Where(r => r.PrimaryMember!.FamilyId == id)
                .ToListAsync();

            _logger.LogInformation("Found {RelationshipCount} relationships", relationships.Count);

            var treeData = BuildTreeData(family.FamilyMembers.ToList(), relationships);
            
            // Log detailed tree structure
            var jsonOptions = new System.Text.Json.JsonSerializerOptions { WriteIndented = true };
            var treeDataJson = System.Text.Json.JsonSerializer.Serialize(treeData, jsonOptions);
            _logger.LogInformation("Tree data built: {TreeData}", treeDataJson);
            
            return Json(treeData);
        }

        private string GetRelationshipDescription(RelationshipType type)
        {
            return type switch
            {
                RelationshipType.Parent => "Родител",
                RelationshipType.Child => "Дете", 
                RelationshipType.Spouse => "Съпруг/а",
                RelationshipType.Sibling => "Брат/Сестра",
                RelationshipType.Grandparent => "Дядо/Баба",
                RelationshipType.Grandchild => "Внук/Внучка",
                RelationshipType.Uncle => "Чичо/Вуйчо",
                RelationshipType.Aunt => "Леля/Тетка",
                RelationshipType.Nephew => "Племенник",
                RelationshipType.Niece => "Племенничка",
                RelationshipType.Cousin => "Братовчед/Сестричка",
                RelationshipType.GreatGrandparent => "Прадядо/Прабаба",
                RelationshipType.GreatGrandchild => "Правнук/Правнучка",
                RelationshipType.StepParent => "Доведен родител",
                RelationshipType.StepChild => "Доведено дете",
                RelationshipType.StepSibling => "Доведен брат/сестра",
                RelationshipType.HalfSibling => "Полубрат/Полусестра",
                RelationshipType.Other => "Друго",
                _ => "Неизвестна връзка"
            };
        }

        private string GetGenderAwareRelationshipDescription(RelationshipType type, FamilyMember person, FamilyMember relatedPerson)
        {
            return type switch
            {
                RelationshipType.Parent => person.Gender switch
                {
                    Gender.Male => "Баща",
                    Gender.Female => "Майка", 
                    _ => "Родител"
                },
                RelationshipType.Child => person.Gender switch
                {
                    Gender.Male => "Син",
                    Gender.Female => "Дъщеря",
                    _ => "Дете"
                },
                RelationshipType.Spouse => person.Gender switch
                {
                    Gender.Male => "Съпруг",
                    Gender.Female => "Съпруга",
                    _ => "Съпруг/а"
                },
                RelationshipType.Sibling => person.Gender switch
                {
                    Gender.Male => "Брат",
                    Gender.Female => "Сестра",
                    _ => "Брат/Сестра"
                },
                RelationshipType.Grandparent => person.Gender switch
                {
                    Gender.Male => "Дядо",
                    Gender.Female => "Баба",
                    _ => "Дядо/Баба"
                },
                RelationshipType.Grandchild => person.Gender switch
                {
                    Gender.Male => "Внук",
                    Gender.Female => "Внучка",
                    _ => "Внук/Внучка"
                },
                RelationshipType.Uncle => "Чичо/Вуйчо",
                RelationshipType.Aunt => "Леля/Тетка", 
                RelationshipType.Nephew => "Племенник",
                RelationshipType.Niece => "Племенничка",
                RelationshipType.Cousin => person.Gender switch
                {
                    Gender.Male => "Братовчед",
                    Gender.Female => "Сестричка", 
                    _ => "Братовчед/Сестричка"
                },
                RelationshipType.GreatGrandparent => person.Gender switch
                {
                    Gender.Male => "Прадядо",
                    Gender.Female => "Прабаба",
                    _ => "Прадядо/Прабаба"
                },
                RelationshipType.GreatGrandchild => person.Gender switch
                {
                    Gender.Male => "Правнук",
                    Gender.Female => "Правнучка",
                    _ => "Правнук/Правнучка"
                },
                RelationshipType.StepParent => person.Gender switch
                {
                    Gender.Male => "Доведен баща",
                    Gender.Female => "Доведена майка",
                    _ => "Доведен родител"
                },
                RelationshipType.StepChild => person.Gender switch
                {
                    Gender.Male => "Доведен син",
                    Gender.Female => "Доведена дъщеря",
                    _ => "Доведено дете"
                },
                RelationshipType.StepSibling => person.Gender switch
                {
                    Gender.Male => "Доведен брат",
                    Gender.Female => "Доведена сестра",
                    _ => "Доведен брат/сестра"
                },
                RelationshipType.HalfSibling => person.Gender switch
                {
                    Gender.Male => "Полубрат",
                    Gender.Female => "Полусестра",
                    _ => "Полубрат/Полусестра"
                },
                RelationshipType.Other => "Друго",
                _ => "Неизвестна връзка"
            };
        }

        private RelationshipType? GetReverseRelationshipTypeForDisplay(RelationshipType originalType)
        {
            return originalType switch
            {
                RelationshipType.Parent => RelationshipType.Child,
                RelationshipType.Child => RelationshipType.Parent,
                RelationshipType.Spouse => RelationshipType.Spouse,
                RelationshipType.Sibling => RelationshipType.Sibling,
                RelationshipType.Grandparent => RelationshipType.Grandchild,
                RelationshipType.Grandchild => RelationshipType.Grandparent,
                RelationshipType.Uncle => RelationshipType.Nephew, // Simplified
                RelationshipType.Aunt => RelationshipType.Niece, // Simplified
                RelationshipType.Nephew => RelationshipType.Uncle, // Simplified
                RelationshipType.Niece => RelationshipType.Aunt, // Simplified
                RelationshipType.Cousin => RelationshipType.Cousin,
                RelationshipType.GreatGrandparent => RelationshipType.GreatGrandchild,
                RelationshipType.GreatGrandchild => RelationshipType.GreatGrandparent,
                RelationshipType.StepParent => RelationshipType.StepChild,
                RelationshipType.StepChild => RelationshipType.StepParent,
                RelationshipType.StepSibling => RelationshipType.StepSibling,
                RelationshipType.HalfSibling => RelationshipType.HalfSibling,
                RelationshipType.Other => null,
                _ => null
            };
        }

        private object BuildTreeData(List<FamilyMember> members, List<Relationship> relationships)
        {
            if (!members.Any())
            {
                dynamic empty = new System.Dynamic.ExpandoObject();
                empty.id = "empty";
                empty.name = "Няма членове";
                empty.children = new object[0];
                return empty;
            }

            var memberDict = new Dictionary<int, dynamic>();
            foreach (var member in members)
            {
                dynamic node = new System.Dynamic.ExpandoObject();
                node.id = member.Id;
                node.name = $"{member.FirstName} {member.LastName}";
                node.birthYear = member.DateOfBirth?.Year;
                node.deathYear = member.DateOfDeath?.Year;
                node.isAlive = member.DateOfDeath == null;
                node.age = CalculateAge(member.DateOfBirth, member.DateOfDeath);
                node.relationshipType = null;
                node.children = new List<object>();
                memberDict[member.Id] = node;
            }

            // Find root member - someone who has no children (is the youngest generation)
            var parentIds = relationships
                .Where(r => r.RelationshipType == RelationshipType.Child)
                .Select(r => r.PrimaryMemberId) // Primary member is the parent in Child relationships
                .Distinct()
                .ToHashSet();
            
            // Also find who has parents (to identify children)
            var childIds = relationships
                .Where(r => r.RelationshipType == RelationshipType.Child)
                .Select(r => r.RelatedMemberId) // Related member is the child in Child relationships
                .Distinct()
                .ToHashSet();
            
            // Root should be someone who is a child but not a parent (youngest generation)
            var rootMember = members
                .OrderByDescending(m => childIds.Contains(m.Id) && !parentIds.Contains(m.Id)) // Prefer children who are not parents
                .ThenByDescending(m => !parentIds.Contains(m.Id)) // Then non-parents
                .ThenByDescending(m => m.DateOfBirth ?? DateTime.MinValue) // Then youngest
                .ThenBy(m => m.CreatedAt)
                .First();
            
            _logger.LogInformation("Selected root member: {Name} (Id: {Id}, IsParent: {IsParent}, IsChild: {IsChild})", 
                rootMember.FirstName, rootMember.Id, parentIds.Contains(rootMember.Id), childIds.Contains(rootMember.Id));
            
            dynamic root = memberDict[rootMember.Id];

            // If we have relationships, build hierarchical tree
            if (relationships.Any())
            {
                _logger.LogInformation("Building tree with {Count} relationships", relationships.Count);
                BuildChildrenRecursive(root, members, relationships, memberDict, new HashSet<int>());
            }
            else
            {
                // If no relationships, add all other members as siblings at root level
                foreach (var member in members.Where(m => m.Id != rootMember.Id))
                {
                    var memberNode = memberDict[member.Id];
                    memberNode.relationshipType = GetGenderAwareRelationshipDescription(RelationshipType.Sibling, member, rootMember);
                    ((List<object>)root.children).Add(memberNode);
                }
            }

            return root;
        }

        private void BuildChildrenRecursive(dynamic node, List<FamilyMember> members, List<Relationship> relationships, Dictionary<int, dynamic> memberMap, HashSet<int> visited)
        {
            if (visited.Contains(node.id)) return;
            visited.Add(node.id);

            // Find all relationships where this node is involved - but avoid duplicates
            // Only take one relationship per pair of members (to avoid processing both A->B and B->A)
            var nodeRelationships = relationships
                .Where(r => r.PrimaryMemberId == node.id || r.RelatedMemberId == node.id)
                .GroupBy(r => new { 
                    Member1 = Math.Min(r.PrimaryMemberId, r.RelatedMemberId),
                    Member2 = Math.Max(r.PrimaryMemberId, r.RelatedMemberId)
                })
                .Select(g => g.First()) // Take only the first relationship from each pair
                .ToList();

            // Group parents together on the same level
            var parents = new Dictionary<int, dynamic>(); // Use dictionary to avoid duplicates
            var otherRelatives = new Dictionary<int, dynamic>();

            foreach (var rel in nodeRelationships)
            {
                int relatedId = rel.PrimaryMemberId == node.id ? rel.RelatedMemberId : rel.PrimaryMemberId;
                
                if (memberMap.ContainsKey(relatedId) && !visited.Contains(relatedId) && relatedId != node.id)
                {
                    var relatedNode = memberMap[relatedId];
                    
                    // Get the FamilyMember objects for gender-aware descriptions
                    var currentMember = members.First(m => m.Id == node.id);
                    var relatedMember = members.First(m => m.Id == relatedId);
                    
                    // Set correct relationship type for the label
                    string relationshipTypeForLabel;
                    RelationshipType displayType;
                    
                    // Check if this is a parent relationship
                    // If current node is child and related node is parent
                    bool isParent = (rel.RelationshipType == RelationshipType.Child && rel.RelatedMemberId == node.id && rel.PrimaryMemberId == relatedId) ||
                                   (rel.RelationshipType == RelationshipType.Parent && rel.PrimaryMemberId == relatedId && rel.RelatedMemberId == node.id);
                    
                    // Check if this is a spouse relationship
                    bool isSpouse = rel.RelationshipType == RelationshipType.Spouse;
                    
                    if (rel.PrimaryMemberId == node.id)
                    {
                        // This node is primary, show what the related node is to this node
                        displayType = rel.RelationshipType;
                        // First param is the person being described, second is the reference person
                        relationshipTypeForLabel = GetGenderAwareRelationshipDescription(displayType, relatedMember, currentMember);
                    }
                    else
                    {
                        // This node is related, show the reverse relationship
                        var reverseType = GetReverseRelationshipTypeForDisplay(rel.RelationshipType);
                        displayType = reverseType ?? rel.RelationshipType;
                        // For reverse, the related member is being described relative to current
                        relationshipTypeForLabel = GetGenderAwareRelationshipDescription(displayType, relatedMember, currentMember);
                    }
                    
                    _logger.LogInformation("Setting relationship: {NodeName} -> {RelatedName} = {Label} (Type: {Type}, IsParent: {IsParent}, IsSpouse: {IsSpouse})", 
                        currentMember.FirstName, relatedMember.FirstName, relationshipTypeForLabel, displayType, isParent, isSpouse);
                    
                    relatedNode.relationshipType = relationshipTypeForLabel;
                    
                    // Only add as parent if this is a direct parent-child relationship
                    if (isParent && !isSpouse)
                    {
                        parents[relatedId] = relatedNode;
                    }
                    else if (!parents.ContainsKey(relatedId))
                    {
                        otherRelatives[relatedId] = relatedNode;
                    }
                }
            }
            
            // Add parents first (they'll appear below in inverted tree)
            foreach (var parent in parents.Values)
            {
                ((List<object>)node.children).Add(parent);
                BuildChildrenRecursive(parent, members, relationships, memberMap, visited);
            }
            
            // Then add other relatives
            foreach (var relative in otherRelatives.Values)
            {
                ((List<object>)node.children).Add(relative);
                BuildChildrenRecursive(relative, members, relationships, memberMap, visited);
            }
            
            // Mark spouses to be shown on same level
            if (parents.Count == 2)
            {
                // Find spouse relationship between parents
                var parentIds = parents.Keys.ToList();
                var parent1Id = parentIds[0];
                var parent2Id = parentIds[1];
                var spouseRel = relationships.FirstOrDefault(r => 
                    (r.RelationshipType == RelationshipType.Spouse) &&
                    ((r.PrimaryMemberId == parent1Id && r.RelatedMemberId == parent2Id) ||
                     (r.PrimaryMemberId == parent2Id && r.RelatedMemberId == parent1Id)));
                     
                if (spouseRel != null)
                {
                    _logger.LogInformation("Found spouse relationship between parents");
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