using MyFamilyTreeNet.Data.Models;

namespace MyFamilyTreeNet.Api.Contracts;

public interface IMemberService
{
    Task<IEnumerable<FamilyMember>> GetFamilyMembersAsync(int familyId);
    Task<FamilyMember?> GetMemberByIdAsync(int id);
    Task<FamilyMember> CreateMemberAsync(FamilyMember member);
    Task<FamilyMember?> UpdateMemberAsync(int id, FamilyMember member);
    Task<bool> DeleteMemberAsync(int id);
}