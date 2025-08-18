using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyFamilyTreeNet.Data;

namespace MyFamilyTreeNet.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StatisticsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public StatisticsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("platform")]
        public async Task<IActionResult> GetPlatformStatistics()
        {
            var totalFamilies = await _context.Families.CountAsync();
            var totalMembers = await _context.FamilyMembers.CountAsync();
            var totalStories = await _context.Stories.CountAsync();

            return Ok(new
            {
                totalFamilies,
                totalMembers,
                totalStories
            });
        }
    }
}