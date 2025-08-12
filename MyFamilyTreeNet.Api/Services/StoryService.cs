
using Microsoft.EntityFrameworkCore;
using MyFamilyTreeNet.Api.Contracts;
using MyFamilyTreeNet.Data;
using MyFamilyTreeNet.Data.Models;

namespace MyFamilyTreeNet.Api.Services
{
    public class StoryService : IStoryService
    {
        private readonly AppDbContext _context;

        public StoryService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Story>> GetFamilyStoriesAsync(int familyId)
        {
            return await _context.Stories
                .Where(s => s.FamilyId == familyId)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
        }

        public async Task<Story?> GetStoryByIdAsync(int id)
        {
            return await _context.Stories
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<Story> CreateStoryAsync(Story story)
        {
            story.CreatedAt = DateTime.UtcNow;
            _context.Stories.Add(story);
            await _context.SaveChangesAsync();
            return story;
        }

        public async Task<Story?> UpdateStoryAsync(int id, Story story)
        {
            var existing = await _context.Stories.FindAsync(id);
            if (existing == null)
            {
                return null;
            }

            existing.Title = story.Title;
            existing.Content = story.Content;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteStoryAsync(int id)
        {
            var story = await _context.Stories.FindAsync(id);
            if (story == null)
            {
                return false;
            }

            _context.Stories.Remove(story);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}