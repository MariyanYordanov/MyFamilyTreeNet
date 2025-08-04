using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyFamilyTreeNet.Data;

namespace MyFamilyTreeNet.Api.Controlers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class FamilyController : ControllerBase
    {
        private readonly AppDbContext _context;

        public FamilyController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllFamilies()
        {
            try
            {
                var families = await _context.Families
                .Select(f => new
                {
                    f.Id,
                    f.Name,
                    f.Description,
                    f.IsPublic,
                    f.CreatedAt,
                    f.CreatedByUserId,
                    members = f.FamilyMembers.Count()
                })
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();

                return Ok(new
                {
                    families,
                    total = families.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Database error",
                    error = ex.Message
                });
            }
        }
        
        [HttpGet("{id}")]
        public async Task<IActionResult> GetFamily(int id)
        {
            try
            {
                var family = await _context.Families
                    .Include(f => f.FamilyMembers)
                    .Where(f => f.Id == id)
                    .Select(f => new
                    {
                        f.Id,
                        f.Name,
                        f.Description,
                        f.IsPublic,
                        f.CreatedAt,
                        Members = f.FamilyMembers.Select(m => new
                        {
                            m.Id,
                            m.FirstName,
                            m.MiddleName,
                            m.LastName,
                            m.DateOfBirth,
                            m.DateOfDeath,
                            m.Gender
                        }).ToList()
                    })
                    .FirstOrDefaultAsync();

                if (family == null)
                {
                    return NotFound(new { message = $"Family with ID {id} not found" });
                }

                return Ok(family);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Failed to retrieve family",
                    error = ex.Message
                });
            }
        }
    }
}