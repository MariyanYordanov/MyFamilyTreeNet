using MyFamilyTreeNet.Data.Models;

namespace MyFamilyTreeNet.Api.Services
{
    public interface IStoryService
    {
        Task<List<Story>> GetFamilyStoriesAsync(int familyId);
        Task<Story?> GetStoryByIdAsync(int id);
        Task<Story> CreateStoryAsync(Story story);
        Task<Story?> UpdateStoryAsync(int id, Story story);
        Task<bool> DeleteStoryAsync(int id);
    }
}