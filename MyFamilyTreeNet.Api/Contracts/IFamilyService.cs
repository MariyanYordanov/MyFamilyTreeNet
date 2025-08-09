using MyFamilyTreeNet.Data.Models;

namespace MyFamilyTreeNet.Api.Contracts;

public interface IFamilyService
{
    Task<IEnumerable<Family>> GetAllFamiliesAsync();
    Task<Family?> GetFamilyByIdAsync(int id);
    Task<Family> CreateFamilyAsync(Family family);
    Task<Family?> UpdateFamilyAsync(int id, Family family);
    Task<bool> DeleteFamilyAsync(int id);
    Task<bool> UserOwnsFamilyAsync(int familyId, string userId);
}