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
    [Authorize]
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
        public async Task<IActionResult> GetAllFamilies()
        {
            var families = await _familyService.GetAllFamiliesAsync();
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
            return Ok(familyDtos);
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
    }
}