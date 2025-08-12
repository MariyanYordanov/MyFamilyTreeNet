using MyFamilyTreeNet.Data.Models;

namespace MyFamilyTreeNet.Api.Contracts
{
    public interface IRelationshipService
    {
        Task<List<Relationship>> GetMemberRelationshipsAsync(int memberId);
        Task<Relationship?> GetRelationshipByIdAsync(int id);
        Task<Relationship> CreateRelationshipAsync(Relationship relationship);
        Task<Relationship?> UpdateRelationshipAsync(int id, Relationship relationship);
        Task<bool> DeleteRelationshipAsync(int id);
    }
}