using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyFamilyTreeNet.Data;

namespace MyFamilyTreeNet.Api.Controlers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class TestController : ControllerBase
    {
        [Authorize]
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new
            {
                message = "Api works!",

            });
        }

        [HttpGet("database")]
        public IActionResult TestDatabase([FromServices] AppDbContext context )
        {
            try
            {
                var familyCount = context.Families.Count();
                return Ok(new
                {
                    message = "Database works!",
                    families = familyCount
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
    }
}