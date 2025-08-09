using Microsoft.EntityFrameworkCore;
using MyFamilyTreeNet.Api.Contracts;
using MyFamilyTreeNet.Data;
using MyFamilyTreeNet.Data.Models;

namespace MyFamilyTreeNet.Api.Services;

public class FamilyService : IFamilyService
{
    private readonly AppDbContext _context;

    public FamilyService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Family>> GetAllFamiliesAsync()
    {
        return await _context.Families
            .Include(f => f.FamilyMembers)
            .Include(f => f.Photos)
            .Include(f => f.Stories)
            .ToListAsync();
    }

    public async Task<Family?> GetFamilyByIdAsync(int id)
    {
        return await _context.Families
            .Include(f => f.FamilyMembers)
            .Include(f => f.Photos)
            .Include(f => f.Stories)
            .FirstOrDefaultAsync(f => f.Id == id);
    }

    public async Task<Family> CreateFamilyAsync(Family family)
    {
        _context.Families.Add(family);
        await _context.SaveChangesAsync();
        return family;
    }

    public async Task<Family?> UpdateFamilyAsync(int id, Family family)
    {
        var existingFamily = await _context.Families.FindAsync(id);
        if (existingFamily == null)
            return null;

        existingFamily.Name = family.Name;
        existingFamily.Description = family.Description;

        await _context.SaveChangesAsync();
        return existingFamily;
    }

    public async Task<bool> DeleteFamilyAsync(int id)
    {
        var family = await _context.Families.FindAsync(id);
        if (family == null)
            return false;

        _context.Families.Remove(family);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UserOwnsFamilyAsync(int familyId, string userId)
    {
        var family = await _context.Families.FindAsync(familyId);
        return family != null && family.CreatedByUserId == userId;
    }
}