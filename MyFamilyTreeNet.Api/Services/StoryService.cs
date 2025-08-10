/*
using Microsoft.EntityFrameworkCore;
using MyFamilyTreeNet.Api.Contracts;
using MyFamilyTreeNet.Data;
using MyFamilyTreeNet.Data.Models;

namespace MyFamilyTreeNet.Api.Services;

public class StoryService : IStoryService
{
    private readonly AppDbContext _context;

    public StoryService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Story>> GetStoriesAsync(int? familyId = null)
    {
        var query = _context.Stories.Include(s => s.Family).AsQueryable();

        if (familyId.HasValue)
        {
            query = query.Where(s => s.FamilyId == familyId);
        }

        return await query.OrderByDescending(s => s.CreatedAt).ToListAsync();
    }

    public async Task<Story?> GetStoryByIdAsync(int id)
    {
        return await _context.Stories
            .Include(s => s.Family)
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
        var existingStory = await _context.Stories.FindAsync(id);
        if (existingStory == null)
            return null;

        existingStory.Title = story.Title;
        existingStory.Content = story.Content;

        await _context.SaveChangesAsync();
        return existingStory;
    }

    public async Task<bool> DeleteStoryAsync(int id)
    {
        var story = await _context.Stories.FindAsync(id);
        if (story == null)
            return false;

        _context.Stories.Remove(story);
        await _context.SaveChangesAsync();
        return true;
    }
}
*/