/*
using Microsoft.EntityFrameworkCore;
using MyFamilyTreeNet.Api.Contracts;
using MyFamilyTreeNet.Api.DTOs;
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

        public async Task<List<RelationshipDto>> GetAllRelationshipsAsync()
        {
            return await _context.Relationships
                .Include(r => r.PrimaryMember)
                .Include(r => r.RelatedMember)
                .Select(r => MapToDto(r))
                .ToListAsync();
        }

        public async Task<List<RelationshipDto>> GetRelationshipsByMemberAsync(int memberId)
        {
            return await _context.Relationships
                .Include(r => r.PrimaryMember)
                .Include(r => r.RelatedMember)
                .Where(r => r.PrimaryMemberId == memberId || r.RelatedMemberId == memberId)
                .Select(r => MapToDto(r))
                .ToListAsync();
        }

        public async Task<List<RelationshipDto>> GetRelationshipsByFamilyAsync(int familyId)
        {
            var familyMemberIds = await _context.FamilyMembers
                .Where(fm => fm.FamilyId == familyId)
                .Select(fm => fm.Id)
                .ToListAsync();

            return await _context.Relationships
                .Include(r => r.PrimaryMember)
                .Include(r => r.RelatedMember)
                .Where(r => familyMemberIds.Contains(r.PrimaryMemberId) && 
                           familyMemberIds.Contains(r.RelatedMemberId))
                .Select(r => MapToDto(r))
                .ToListAsync();
        }

        public async Task<RelationshipDto?> GetRelationshipByIdAsync(int id)
        {
            var relationship = await _context.Relationships
                .Include(r => r.PrimaryMember)
                .Include(r => r.RelatedMember)
                .FirstOrDefaultAsync(r => r.Id == id);

            return relationship != null ? MapToDto(relationship) : null;
        }

        public async Task<RelationshipDto> CreateRelationshipAsync(CreateRelationshipDto dto, string userId)
        {
            // Validate that both members exist
            var primaryMember = await _context.FamilyMembers.FindAsync(dto.PrimaryMemberId);
            var relatedMember = await _context.FamilyMembers.FindAsync(dto.RelatedMemberId);

            if (primaryMember == null || relatedMember == null)
            {
                throw new ArgumentException("One or both members not found");
            }

            // Check they're in the same family
            if (primaryMember.FamilyId != relatedMember.FamilyId)
            {
                throw new ArgumentException("Members must be from the same family");
            }

            // Check if relationship already exists
            if (await RelationshipExistsAsync(dto.PrimaryMemberId, dto.RelatedMemberId))
            {
                throw new ArgumentException("Relationship already exists between these members");
            }

            var relationship = new Relationship
            {
                PrimaryMemberId = dto.PrimaryMemberId,
                RelatedMemberId = dto.RelatedMemberId,
                RelationshipType = dto.RelationshipType,
                Notes = dto.Notes,
                CreatedByUserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Relationships.Add(relationship);
            
            // Optionally create reverse relationship
            await CreateReverseRelationshipIfNeeded(relationship, userId);
            
            await _context.SaveChangesAsync();

            // Reload with navigation properties
            return await GetRelationshipByIdAsync(relationship.Id) ?? MapToDto(relationship);
        }

        public async Task<RelationshipDto> UpdateRelationshipAsync(int id, UpdateRelationshipDto dto)
        {
            var relationship = await _context.Relationships.FindAsync(id);
            if (relationship == null)
            {
                throw new ArgumentException("Relationship not found");
            }

            relationship.RelationshipType = dto.RelationshipType;
            relationship.Notes = dto.Notes;

            await _context.SaveChangesAsync();

            return await GetRelationshipByIdAsync(id) ?? MapToDto(relationship);
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

        public async Task<bool> RelationshipExistsAsync(int primaryMemberId, int relatedMemberId)
        {
            return await _context.Relationships
                .AnyAsync(r => (r.PrimaryMemberId == primaryMemberId && r.RelatedMemberId == relatedMemberId) ||
                              (r.PrimaryMemberId == relatedMemberId && r.RelatedMemberId == primaryMemberId));
        }

        public async Task<MemberRelationshipsDto> GetMemberRelationshipsTreeAsync(int memberId)
        {
            var member = await _context.FamilyMembers.FindAsync(memberId);
            if (member == null)
            {
                throw new ArgumentException("Member not found");
            }

            var relationships = await GetRelationshipsByMemberAsync(memberId);

            return new MemberRelationshipsDto
            {
                MemberId = memberId,
                MemberName = $"{member.FirstName} {member.MiddleName} {member.LastName}".Trim(),
                Relationships = relationships
            };
        }

        private async Task CreateReverseRelationshipIfNeeded(Relationship relationship, string userId)
        {
            var reverseType = GetReverseRelationshipType(relationship.RelationshipType);
            if (reverseType.HasValue)
            {
                var reverseExists = await _context.Relationships
                    .AnyAsync(r => r.PrimaryMemberId == relationship.RelatedMemberId &&
                                  r.RelatedMemberId == relationship.PrimaryMemberId);

                if (!reverseExists)
                {
                    var reverseRelationship = new Relationship
                    {
                        PrimaryMemberId = relationship.RelatedMemberId,
                        RelatedMemberId = relationship.PrimaryMemberId,
                        RelationshipType = reverseType.Value,
                        Notes = relationship.Notes,
                        CreatedByUserId = userId,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.Relationships.Add(reverseRelationship);
                }
            }
        }

        private RelationshipType? GetReverseRelationshipType(RelationshipType type)
        {
            return type switch
            {
                RelationshipType.Parent => RelationshipType.Child,
                RelationshipType.Child => RelationshipType.Parent,
                RelationshipType.Spouse => RelationshipType.Spouse,
                RelationshipType.Sibling => RelationshipType.Sibling,
                RelationshipType.Grandparent => RelationshipType.Grandchild,
                RelationshipType.Grandchild => RelationshipType.Grandparent,
                RelationshipType.Uncle => RelationshipType.Nephew,
                RelationshipType.Aunt => RelationshipType.Niece,
                RelationshipType.Nephew => RelationshipType.Uncle,
                RelationshipType.Niece => RelationshipType.Aunt,
                RelationshipType.Cousin => RelationshipType.Cousin,
                RelationshipType.StepParent => RelationshipType.StepChild,
                RelationshipType.StepChild => RelationshipType.StepParent,
                RelationshipType.StepSibling => RelationshipType.StepSibling,
                RelationshipType.HalfSibling => RelationshipType.HalfSibling,
                _ => null
            };
        }

        private static RelationshipDto MapToDto(Relationship r)
        {
            return new RelationshipDto
            {
                Id = r.Id,
                PrimaryMemberId = r.PrimaryMemberId,
                RelatedMemberId = r.RelatedMemberId,
                RelationshipType = r.RelationshipType,
                Notes = r.Notes,
                CreatedAt = r.CreatedAt,
                PrimaryMemberName = r.PrimaryMember != null ? 
                    $"{r.PrimaryMember.FirstName} {r.PrimaryMember.MiddleName} {r.PrimaryMember.LastName}".Trim() : "",
                RelatedMemberName = r.RelatedMember != null ? 
                    $"{r.RelatedMember.FirstName} {r.RelatedMember.MiddleName} {r.RelatedMember.LastName}".Trim() : "",
                RelationshipTypeName = GetRelationshipTypeName(r.RelationshipType)
            };
        }

        private static string GetRelationshipTypeName(RelationshipType type)
        {
            return type switch
            {
                RelationshipType.Parent => "Родител",
                RelationshipType.Child => "Дете",
                RelationshipType.Spouse => "Съпруг/Съпруга",
                RelationshipType.Sibling => "Брат/Сестра",
                RelationshipType.Grandparent => "Баба/Дядо",
                RelationshipType.Grandchild => "Внук/Внучка",
                RelationshipType.Uncle => "Чичо/Вуйчо",
                RelationshipType.Aunt => "Леля",
                RelationshipType.Nephew => "Племенник",
                RelationshipType.Niece => "Племенница",
                RelationshipType.Cousin => "Братовчед/Братовчедка",
                RelationshipType.GreatGrandparent => "Прабаба/Прадядо",
                RelationshipType.GreatGrandchild => "Правнук/Правнучка",
                RelationshipType.StepParent => "Доведен родител",
                RelationshipType.StepChild => "Доведено дете",
                RelationshipType.StepSibling => "Доведен брат/сестра",
                RelationshipType.HalfSibling => "Полубрат/Полусестра",
                _ => "Друго"
            };
        }
    }
}
*/