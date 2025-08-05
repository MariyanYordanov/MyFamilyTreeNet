using Microsoft.EntityFrameworkCore;
using MyFamilyTreeNet.Api.Contracts;
using MyFamilyTreeNet.Data;
using MyFamilyTreeNet.Data.Models;

namespace MyFamilyTreeNet.Api.Services;

public class MemberService : IMemberService
{
    private readonly AppDbContext _context;

    public MemberService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<FamilyMember>> GetFamilyMembersAsync(int familyId)
    {
        return await _context.FamilyMembers
            .Include(m => m.Family)
            .Where(m => m.FamilyId == familyId)
            .ToListAsync();
    }

    public async Task<FamilyMember?> GetMemberByIdAsync(int id)
    {
        return await _context.FamilyMembers
            .Include(m => m.Family)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<FamilyMember> CreateMemberAsync(FamilyMember member)
    {
        _context.FamilyMembers.Add(member);
        await _context.SaveChangesAsync();
        return member;
    }

    public async Task<FamilyMember?> UpdateMemberAsync(int id, FamilyMember member)
    {
        var existingMember = await _context.FamilyMembers.FindAsync(id);
        if (existingMember == null)
            return null;

        existingMember.FirstName = member.FirstName;
        existingMember.MiddleName = member.MiddleName;
        existingMember.LastName = member.LastName;
        existingMember.DateOfBirth = member.DateOfBirth;
        existingMember.DateOfDeath = member.DateOfDeath;
        existingMember.Gender = member.Gender;
        existingMember.Biography = member.Biography;
        existingMember.PlaceOfBirth = member.PlaceOfBirth;
        existingMember.PlaceOfDeath = member.PlaceOfDeath;

        await _context.SaveChangesAsync();
        return existingMember;
    }

    public async Task<bool> DeleteMemberAsync(int id)
    {
        var member = await _context.FamilyMembers.FindAsync(id);
        if (member == null)
            return false;

        _context.FamilyMembers.Remove(member);
        await _context.SaveChangesAsync();
        return true;
    }
}