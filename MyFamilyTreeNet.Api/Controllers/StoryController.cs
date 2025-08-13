/*
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
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class StoryController : ControllerBase
    {
        private readonly IStoryService _storyService;
        private readonly IMapper _mapper;

        public StoryController(IStoryService storyService, IMapper mapper)
        {
            _storyService = storyService;
            _mapper = mapper;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllStories([FromQuery] int? familyId = null)
        {
            var stories = await _storyService.GetStoriesAsync(familyId);
            var storyDtos = stories.Select(s => new StoryDto
            {
                Id = s.Id,
                Title = s.Title,
                Content = s.Content,
                CreatedAt = s.CreatedAt,
                FamilyId = s.FamilyId,
                FamilyName = "",
                AuthorId = s.AuthorUserId,
                AuthorName = ""
            });
            return Ok(storyDtos);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetStory(int id)
        {
            var story = await _storyService.GetStoryByIdAsync(id);
            if (story == null)
                return NotFound();

            var storyDto = new StoryDto
            {
                Id = story.Id,
                Title = story.Title,
                Content = story.Content,
                CreatedAt = story.CreatedAt,
                FamilyId = story.FamilyId,
                FamilyName = "",
                AuthorId = story.AuthorUserId,
                AuthorName = ""
            };
            return Ok(storyDto);
        }

        [HttpGet("family/{familyId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetFamilyStories(int familyId)
        {
            var stories = await _storyService.GetStoriesAsync(familyId);
            var storyDtos = stories.Select(s => new StoryDto
            {
                Id = s.Id,
                Title = s.Title,
                Content = s.Content,
                CreatedAt = s.CreatedAt,
                FamilyId = s.FamilyId,
                FamilyName = "",
                AuthorId = s.AuthorUserId,
                AuthorName = ""
            });
            return Ok(storyDtos);
        }

        [HttpPost]
        public async Task<IActionResult> CreateStory([FromBody] CreateStoryDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var story = new Story
            {
                Title = model.Title,
                Content = model.Content,
                FamilyId = model.FamilyId,
                AuthorUserId = userId ?? string.Empty,
                CreatedAt = DateTime.UtcNow,
            };
            
            var createdStory = await _storyService.CreateStoryAsync(story);
            var storyDto = new StoryDto
            {
                Id = createdStory.Id,
                Title = createdStory.Title,
                Content = createdStory.Content,
                CreatedAt = createdStory.CreatedAt,
                FamilyId = createdStory.FamilyId,
                FamilyName = "",
                AuthorId = createdStory.AuthorUserId,
                AuthorName = ""
            };
            return CreatedAtAction(nameof(GetStory), new { id = createdStory.Id }, storyDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStory(int id, [FromBody] UpdateStoryDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var story = new Story
            {
                Id = id,
                Title = model.Title,
                Content = model.Content,
            };
            
            var updatedStory = await _storyService.UpdateStoryAsync(id, story);
            if (updatedStory == null)
                return NotFound();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStory(int id)
        {
            var success = await _storyService.DeleteStoryAsync(id);
            if (!success)
                return NotFound();

            return NoContent();
        }
    }
}
*/