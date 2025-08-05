using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyFamilyTreeNet.Api.Contracts;
using MyFamilyTreeNet.Api.DTOs;
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

        public FamilyController(IFamilyService familyService, IMapper mapper)
        {
            _familyService = familyService;
            _mapper = mapper;
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

    }
}