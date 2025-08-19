using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyFamilyTreeNet.Api.Contracts;
using MyFamilyTreeNet.Api.DTOs;
using MyFamilyTreeNet.Data;
using MyFamilyTreeNet.Data.Models;

namespace MyFamilyTreeNet.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [IgnoreAntiforgeryToken]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class FamilyController : ControllerBase
    {
        private readonly IFamilyService _familyService;
        private readonly IMapper _mapper;
        private readonly AppDbContext _context;

        public FamilyController(IFamilyService familyService, IMapper mapper, AppDbContext context)
        {
            _familyService = familyService;
            _mapper = mapper;
            _context = context;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllFamilies([FromQuery] string? createdByUserId = null)
        {
            var families = await _familyService.GetAllFamiliesAsync();
            
            // Filter by user if specified
            if (!string.IsNullOrEmpty(createdByUserId))
            {
                families = families.Where(f => f.CreatedByUserId == createdByUserId);
            }
            
            var familyDtos = families.Select(f => new FamilyDto
            {
                Id = f.Id,
                Name = f.Name,
                Description = f.Description,
                CreatedAt = f.CreatedAt,
                CreatedByUserId = f.CreatedByUserId,
                MemberCount = f.FamilyMembers?.Count ?? 0,
                PhotoCount = f.Photos?.Count ?? 0,
                StoryCount = f.Stories?.Count ?? 0
            });
            
            var response = new
            {
                families = familyDtos,
                totalCount = familyDtos.Count(),
                page = 1,
                pageSize = familyDtos.Count(),
                totalPages = 1
            };
            
            return Ok(response);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetFamilyById(int id)
        {
            var family = await _familyService.GetFamilyByIdAsync(id);
            if (family == null)
                return NotFound(new { message = "Family not found" });

            var familyDto = new FamilyDto
            {
                Id = family.Id,
                Name = family.Name,
                Description = family.Description,
                CreatedAt = family.CreatedAt,
                CreatedByUserId = family.CreatedByUserId,
                MemberCount = family.FamilyMembers?.Count ?? 0,
                PhotoCount = family.Photos?.Count ?? 0,
                StoryCount = family.Stories?.Count ?? 0
            };
            return Ok(familyDto);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateFamily([FromBody] CreateFamilyDto createFamilyDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var family = new Family
            {
                Name = createFamilyDto.Name,
                Description = createFamilyDto.Description,
                IsPublic = createFamilyDto.IsPublic,
                CreatedByUserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            var createdFamily = await _familyService.CreateFamilyAsync(family);
            
            var familyDto = new FamilyDto
            {
                Id = createdFamily.Id,
                Name = createdFamily.Name,
                Description = createdFamily.Description,
                CreatedAt = createdFamily.CreatedAt,
                CreatedByUserId = createdFamily.CreatedByUserId,
                MemberCount = 0,
                PhotoCount = 0,
                StoryCount = 0
            };

            return CreatedAtAction(nameof(GetFamilyById), new { id = createdFamily.Id }, familyDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateFamily(int id, [FromBody] UpdateFamilyDto updateFamilyDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            if (!await _familyService.UserOwnsFamilyAsync(id, userId))
                return Forbid();

            var family = new Family
            {
                Name = updateFamilyDto.Name,
                Description = updateFamilyDto.Description,
                IsPublic = updateFamilyDto.IsPublic
            };

            var updatedFamily = await _familyService.UpdateFamilyAsync(id, family);
            if (updatedFamily == null)
                return NotFound(new { message = "Family not found" });

            return Ok(updatedFamily);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFamily(int id)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            if (!await _familyService.UserOwnsFamilyAsync(id, userId))
                return Forbid();

            var result = await _familyService.DeleteFamilyAsync(id);
            if (!result)
                return NotFound(new { message = "Family not found" });

            return NoContent();
        }

        [HttpGet("{id}/tree-data")]
        public async Task<IActionResult> GetFamilyTreeData(int id)
        {
            var family = await _context.Families
                .Include(f => f.FamilyMembers)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (family == null)
            {
                return NotFound();
            }

            var treeData = family.FamilyMembers.Select(m => new
            {
                id = m.Id,
                firstName = m.FirstName,
                lastName = m.LastName,
                birthDate = m.DateOfBirth?.ToString("yyyy-MM-dd"),
                deathDate = m.DateOfDeath?.ToString("yyyy-MM-dd"),
                gender = m.Gender.ToString()
            }).ToList();

            return Ok(treeData);
        }

        [HttpGet("{id}/tree")]
        [AllowAnonymous]
        public async Task<IActionResult> GetFamilyTree(int id)
        {
            try
            {
                var family = await _context.Families
                    .Where(f => f.Id == id && (f.IsPublic || f.CreatedByUserId == (User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier) != null ? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value : null)))
                    .Include(f => f.FamilyMembers)
                    .FirstOrDefaultAsync();

                if (family == null)
                {
                    return Ok(new { error = "Семейството не е намерено" });
                }

                var relationships = await _context.Relationships
                    .Include(r => r.PrimaryMember)
                    .Include(r => r.RelatedMember)
                    .Where(r => r.PrimaryMember!.FamilyId == id)
                    .ToListAsync();

                var treeData = BuildTreeData(family.FamilyMembers.ToList(), relationships);
                
                return Ok(treeData);
            }
            catch (Exception ex)
            {
                return Ok(new { error = "Грешка при построяване на дървото", details = ex.Message });
            }
        }

        private object BuildTreeData(List<FamilyMember> members, List<Relationship> relationships)
        {
            if (!members.Any())
            {
                return new
                {
                    id = "empty",
                    name = "Няма членове",
                    children = new object[0]
                };
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
                node.spouseId = null;
                memberDict[member.Id] = node;
            }

            // Find root member - someone who has parents but is not a parent themselves
            var hasParents = new HashSet<int>();
            var isParent = new HashSet<int>();
            
            foreach (var rel in relationships)
            {
                if (rel.RelationshipType == RelationshipType.Parent)
                {
                    // PrimaryMember says RelatedMember is their parent
                    hasParents.Add(rel.PrimaryMemberId);
                }
                else if (rel.RelationshipType == RelationshipType.Child)
                {
                    // PrimaryMember says RelatedMember is their child (so Primary is parent)
                    isParent.Add(rel.PrimaryMemberId);
                }
            }
            
            // Root should be someone who has parents but is not a parent
            var rootMember = members
                .OrderByDescending(m => hasParents.Contains(m.Id) && !isParent.Contains(m.Id))
                .ThenByDescending(m => m.DateOfBirth ?? DateTime.MinValue)
                .First();
                
            dynamic root = memberDict[rootMember.Id];
            
            // First, find all spouse relationships and mark them
            var spouseRelations = relationships.Where(r => r.RelationshipType == RelationshipType.Spouse).ToList();
            foreach (var spouseRel in spouseRelations)
            {
                if (memberDict.ContainsKey(spouseRel.PrimaryMemberId) && memberDict.ContainsKey(spouseRel.RelatedMemberId))
                {
                    memberDict[spouseRel.PrimaryMemberId].spouseId = spouseRel.RelatedMemberId;
                    memberDict[spouseRel.RelatedMemberId].spouseId = spouseRel.PrimaryMemberId;
                }
            }

            // Build tree with maximum levels based on family size
            var maxLevels = members.Count > 8 ? 5 : (members.Count > 5 ? 4 : 3); // More levels for larger families
            BuildTreeLevels(root, rootMember, members, relationships, memberDict, 0, maxLevels);

            return root;
        }

        private void BuildTreeLevels(dynamic node, FamilyMember member, List<FamilyMember> allMembers, List<Relationship> relationships, Dictionary<int, dynamic> memberDict, int currentLevel, int maxLevels)
        {
            if (currentLevel >= maxLevels) return;

            // Find parent relationships for this member
            var parentRelations = relationships
                .Where(r => (r.PrimaryMemberId == member.Id && r.RelationshipType == RelationshipType.Child) ||
                           (r.RelatedMemberId == member.Id && r.RelationshipType == RelationshipType.Parent))
                .ToList();

            foreach (var rel in parentRelations)
            {
                int parentId;
                if (rel.RelationshipType == RelationshipType.Child && rel.PrimaryMemberId == member.Id)
                {
                    parentId = rel.RelatedMemberId;
                }
                else if (rel.RelationshipType == RelationshipType.Parent && rel.RelatedMemberId == member.Id)
                {
                    parentId = rel.PrimaryMemberId;
                }
                else
                {
                    continue;
                }

                if (memberDict.ContainsKey(parentId))
                {
                    var parentNode = memberDict[parentId];
                    var parentMember = allMembers.First(m => m.Id == parentId);
                    
                    // Check if already added
                    if (!((List<object>)node.children).Any(c => ((dynamic)c).id == parentId))
                    {
                        parentNode.relationshipType = GetGenderAwareRelationshipDescription(RelationshipType.Parent, parentMember, member);
                        ((List<object>)node.children).Add(parentNode);
                        
                        // Add spouse of this parent
                        var spouseRel = relationships
                            .Where(r => r.RelationshipType == RelationshipType.Spouse && 
                                       (r.PrimaryMemberId == parentId || r.RelatedMemberId == parentId))
                            .FirstOrDefault();
                            
                        if (spouseRel != null)
                        {
                            int spouseId = spouseRel.PrimaryMemberId == parentId ? 
                                          spouseRel.RelatedMemberId : spouseRel.PrimaryMemberId;
                                          
                            if (memberDict.ContainsKey(spouseId) && !((List<object>)node.children).Any(c => ((dynamic)c).id == spouseId))
                            {
                                var spouseNode = memberDict[spouseId];
                                var spouseMember = allMembers.First(m => m.Id == spouseId);
                                spouseNode.relationshipType = GetGenderAwareRelationshipDescription(RelationshipType.Parent, spouseMember, member);
                                ((List<object>)node.children).Add(spouseNode);
                            }
                        }
                        
                        // Continue to next level
                        BuildTreeLevels(parentNode, parentMember, allMembers, relationships, memberDict, currentLevel + 1, maxLevels);
                    }
                }
            }
        }

        private int? CalculateAge(DateTime? birthDate, DateTime? deathDate)
        {
            if (!birthDate.HasValue) return null;

            var endDate = deathDate ?? DateTime.Now;
            var age = endDate.Year - birthDate.Value.Year;
            
            if (endDate.Date < birthDate.Value.AddYears(age).Date)
                age--;
                
            return age >= 0 ? age : null;
        }

        private string GetGenderAwareRelationshipDescription(RelationshipType type, FamilyMember relatedMember, FamilyMember currentMember)
        {
            return type switch
            {
                RelationshipType.Parent => relatedMember.Gender == Gender.Male ? "Баща" : "Майка",
                RelationshipType.Child => relatedMember.Gender == Gender.Male ? "Син" : "Дъщеря",
                RelationshipType.Spouse => relatedMember.Gender == Gender.Male ? "Съпруг" : "Съпруга",
                RelationshipType.Sibling => relatedMember.Gender == Gender.Male ? "Брат" : "Сестра",
                RelationshipType.Grandparent => relatedMember.Gender == Gender.Male ? "Дядо" : "Баба",
                RelationshipType.Grandchild => relatedMember.Gender == Gender.Male ? "Внук" : "Внучка",
                RelationshipType.Uncle => "Чичо/Вуйчо",
                RelationshipType.Aunt => "Леля/Тетка",
                RelationshipType.Nephew => "Племенник",
                RelationshipType.Niece => "Племенница",
                RelationshipType.Cousin => relatedMember.Gender == Gender.Male ? "Братовчед" : "Сестричина",
                _ => "Роднина"
            };
        }

    }
}