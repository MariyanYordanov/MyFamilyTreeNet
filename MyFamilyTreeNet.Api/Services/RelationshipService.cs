
using Microsoft.EntityFrameworkCore;
using MyFamilyTreeNet.Api.Contracts;
using MyFamilyTreeNet.Data;
using MyFamilyTreeNet.Data.Models;

namespace MyFamilyTreeNet.Api.Services
{
    public class RelationshipService : IRelationshipService
    {
        private readonly AppDbContext _context;

        public RelationshipService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Relationship>> GetMemberRelationshipsAsync(int memberId)
        {
            return await _context.Relationships
                .Include(r => r.PrimaryMember)
                .Include(r => r.RelatedMember)
                .Where(r => r.PrimaryMemberId == memberId || r.RelatedMemberId == memberId)
                .ToListAsync();
        }

        public async Task<Relationship?> GetRelationshipByIdAsync(int id)
        {
            return await _context.Relationships
                .Include(r => r.PrimaryMember)
                .Include(r => r.RelatedMember)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<Relationship> CreateRelationshipAsync(Relationship relationship)
        {
            _context.Relationships.Add(relationship);
            await _context.SaveChangesAsync();
            
            return await GetRelationshipByIdAsync(relationship.Id) ?? relationship;
        }

        public async Task<Relationship?> UpdateRelationshipAsync(int id, Relationship relationship)
        {
            var existing = await _context.Relationships.FindAsync(id);
            if (existing == null)
            {
                return null;
            }

            existing.RelationshipType = relationship.RelationshipType;
            existing.Notes = relationship.Notes;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteRelationshipAsync(int id)
        {
            var relationship = await _context.Relationships.FindAsync(id);
            if (relationship == null)
            {
                return false;
            }

            _context.Relationships.Remove(relationship);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
