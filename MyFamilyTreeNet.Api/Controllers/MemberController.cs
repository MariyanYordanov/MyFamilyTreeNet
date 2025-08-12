using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyFamilyTreeNet.Api.DTOs;
using MyFamilyTreeNet.Data;
using MyFamilyTreeNet.Data.Models;
using System.Security.Claims;

namespace MyFamilyTreeNet.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [IgnoreAntiforgeryToken]
    [Authorize]
    public class MemberController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<MemberController> _logger;

        public MemberController(
            AppDbContext context,
            IMapper mapper,
            ILogger<MemberController> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        private string GetCurrentUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<FamilyMemberDto>>> GetMembers(
            [FromQuery] int? familyId = null,
            [FromQuery] string? search = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var isAuthenticated = !string.IsNullOrEmpty(currentUserId);
                _logger.LogInformation("Member API - GetMembers called by user: {UserId}, authenticated: {IsAuth}", currentUserId, isAuthenticated);
                
                IQueryable<FamilyMember> query;
                
                if (familyId.HasValue)
                {
                    // If specific family is requested, check if it's public or user owns it
                    var family = await _context.Families.FindAsync(familyId.Value);
                    if (family == null)
                    {
                        return NotFound("Family not found");
                    }
                    
                    if (!family.IsPublic && (!isAuthenticated || family.CreatedByUserId != currentUserId))
                    {
                        return Forbid("You don't have permission to view members of this private family");
                    }
                    
                    query = _context.FamilyMembers
                        .Include(m => m.Family)
                        .Where(m => m.FamilyId == familyId.Value);
                }
                else if (isAuthenticated)
                {
                    // If no specific family and user is authenticated, show their families
                    query = _context.FamilyMembers
                        .Include(m => m.Family)
                        .Where(m => m.Family.CreatedByUserId == currentUserId);
                }
                else
                {
                    // If no specific family and not authenticated, show public families only
                    query = _context.FamilyMembers
                        .Include(m => m.Family)
                        .Where(m => m.Family.IsPublic);
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

                var memberDtos = _mapper.Map<List<FamilyMemberDto>>(members);

                Response.Headers["X-Total-Count"] = totalCount.ToString();
                Response.Headers["X-Page"] = page.ToString();
                Response.Headers["X-Page-Size"] = pageSize.ToString();

                return Ok(memberDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting members");
                return StatusCode(500, "Възникна грешка при зареждане на членовете");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<FamilyMemberDto>> GetMember(int id)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var member = await _context.FamilyMembers
                    .Include(m => m.Family)
                    .FirstOrDefaultAsync(m => m.Id == id && m.Family.CreatedByUserId == currentUserId);

                if (member == null)
                {
                    return NotFound("Членът не е намерен");
                }

                var memberDto = _mapper.Map<FamilyMemberDto>(member);
                return Ok(memberDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting member {MemberId}", id);
                return StatusCode(500, "Възникна грешка при зареждане на члена");
            }
        }

        [HttpPost]
        public async Task<ActionResult<FamilyMemberDto>> CreateMember([FromBody] CreateMemberDto createMemberDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var currentUserId = GetCurrentUserId();
                _logger.LogInformation("Member API - CreateMember called by user: {UserId} for family: {FamilyId}", currentUserId, createMemberDto.FamilyId);
                
                // Verify family ownership
                var family = await _context.Families
                    .FirstOrDefaultAsync(f => f.Id == createMemberDto.FamilyId && f.CreatedByUserId == currentUserId);

                if (family == null)
                {
                    return NotFound("Семейството не е намерено");
                }

                var member = _mapper.Map<FamilyMember>(createMemberDto);
                member.CreatedAt = DateTime.UtcNow;
                member.AddedByUserId = currentUserId;

                _context.FamilyMembers.Add(member);
                await _context.SaveChangesAsync();

                // Load the created member with family data
                var createdMember = await _context.FamilyMembers
                    .Include(m => m.Family)
                    .FirstAsync(m => m.Id == member.Id);

                var memberDto = _mapper.Map<FamilyMemberDto>(createdMember);
                return CreatedAtAction(nameof(GetMember), new { id = member.Id }, memberDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating member");
                return StatusCode(500, "Възникна грешка при създаване на члена");
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<FamilyMemberDto>> UpdateMember(int id, [FromBody] UpdateMemberDto updateMemberDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var currentUserId = GetCurrentUserId();
                var member = await _context.FamilyMembers
                    .Include(m => m.Family)
                    .FirstOrDefaultAsync(m => m.Id == id && m.Family.CreatedByUserId == currentUserId);

                if (member == null)
                {
                    return NotFound("Членът не е намерен");
                }

                _mapper.Map(updateMemberDto, member);

                await _context.SaveChangesAsync();

                var memberDto = _mapper.Map<FamilyMemberDto>(member);
                return Ok(memberDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating member {MemberId}", id);
                return StatusCode(500, "Възникна грешка при актуализиране на члена");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMember(int id)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var member = await _context.FamilyMembers
                    .Include(m => m.Family)
                    .FirstOrDefaultAsync(m => m.Id == id && m.Family.CreatedByUserId == currentUserId);

                if (member == null)
                {
                    return NotFound("Членът не е намерен");
                }

                // Check if member has relationships
                var hasRelationships = await _context.Relationships
                    .AnyAsync(r => r.PrimaryMemberId == id || r.RelatedMemberId == id);

                if (hasRelationships)
                {
                    return BadRequest("Не може да се изтрие член, който има семейни връзки. Моля премахнете връзките първо.");
                }

                _context.FamilyMembers.Remove(member);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting member {MemberId}", id);
                return StatusCode(500, "Възникна грешка при изтриване на члена");
            }
        }

        [HttpGet("{id}/relationships")]
        public async Task<ActionResult<MemberRelationshipsDto>> GetMemberRelationships(int id)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var member = await _context.FamilyMembers
                    .Include(m => m.Family)
                    .FirstOrDefaultAsync(m => m.Id == id && m.Family.CreatedByUserId == currentUserId);

                if (member == null)
                {
                    return NotFound("Членът не е намерен");
                }

                var relationships = await _context.Relationships
                    .Include(r => r.PrimaryMember)
                    .Include(r => r.RelatedMember)
                    .Where(r => r.PrimaryMemberId == id || r.RelatedMemberId == id)
                    .ToListAsync();

                var relationshipDtos = _mapper.Map<List<RelationshipDto>>(relationships);

                var result = new MemberRelationshipsDto
                {
                    MemberId = id,
                    MemberName = $"{member.FirstName} {member.MiddleName} {member.LastName}".Replace("  ", " ").Trim(),
                    Relationships = relationshipDtos
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting member relationships for {MemberId}", id);
                return StatusCode(500, "Възникна грешка при зареждане на семейните връзки");
            }
        }

        [HttpGet("family/{familyId}/tree")]
        [AllowAnonymous]
        public async Task<ActionResult<object>> GetFamilyTree(int familyId)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var isAuthenticated = !string.IsNullOrEmpty(currentUserId);
                
                // Find the family
                var family = await _context.Families
                    .FirstOrDefaultAsync(f => f.Id == familyId);

                if (family == null)
                {
                    return NotFound("Семейството не е намерено");
                }
                
                // Check permissions - allow if public or user owns it
                if (!family.IsPublic && (!isAuthenticated || family.CreatedByUserId != currentUserId))
                {
                    return Forbid("Нямате достъп до това частно семейство");
                }

                var members = await _context.FamilyMembers
                    .Where(m => m.FamilyId == familyId)
                    .ToListAsync();

                var relationships = await _context.Relationships
                    .Include(r => r.PrimaryMember)
                    .Include(r => r.RelatedMember)
                    .Where(r => r.PrimaryMember.FamilyId == familyId)
                    .ToListAsync();

                var memberDtos = _mapper.Map<List<FamilyMemberDto>>(members);
                var relationshipDtos = _mapper.Map<List<RelationshipDto>>(relationships);

                var treeData = new
                {
                    FamilyId = familyId,
                    FamilyName = family.Name,
                    Members = memberDtos,
                    Relationships = relationshipDtos
                };

                return Ok(treeData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting family tree for family {FamilyId}", familyId);
                return StatusCode(500, "Възникна грешка при зареждане на семейното дърво");
            }
        }
    }
}