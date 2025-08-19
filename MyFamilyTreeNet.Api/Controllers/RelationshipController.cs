
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using MyFamilyTreeNet.Api.Contracts;
using MyFamilyTreeNet.Api.DTOs;
using MyFamilyTreeNet.Data.Models;

namespace MyFamilyTreeNet.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [IgnoreAntiforgeryToken]
    public class RelationshipController : ControllerBase
    {
        private readonly IRelationshipService _relationshipService;
        private readonly IMemberService _memberService;
        private readonly IFamilyService _familyService;

        public RelationshipController(IRelationshipService relationshipService, IMemberService memberService, IFamilyService familyService)
        {
            _relationshipService = relationshipService;
            _memberService = memberService;
            _familyService = familyService;
        }

        [HttpGet]
        public ActionResult<List<Relationship>> GetAllRelationships()
        {
            try
            {
                // For now, return empty list - this method needs to be implemented in service if needed
                return Ok(new List<Relationship>());
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching relationships", error = ex.Message });
            }
        }

        [HttpGet("member/{memberId}")]
        public async Task<ActionResult<List<Relationship>>> GetRelationshipsByMember(int memberId)
        {
            try
            {
                var relationships = await _relationshipService.GetMemberRelationshipsAsync(memberId);
                
                // Return DTOs with proper casing and structure for Angular frontend
                var result = relationships.Select(r => new
                {
                    id = r.Id,
                    primaryMemberId = r.PrimaryMemberId,
                    relatedMemberId = r.RelatedMemberId,
                    relationshipType = (int)r.RelationshipType,
                    notes = r.Notes,
                    createdAt = r.CreatedAt,
                    primaryMember = r.PrimaryMember != null ? new
                    {
                        id = r.PrimaryMember.Id,
                        firstName = r.PrimaryMember.FirstName,
                        middleName = r.PrimaryMember.MiddleName,
                        lastName = r.PrimaryMember.LastName,
                        dateOfBirth = r.PrimaryMember.DateOfBirth?.ToString("yyyy-MM-dd"),
                        dateOfDeath = r.PrimaryMember.DateOfDeath?.ToString("yyyy-MM-dd"),
                        gender = r.PrimaryMember.Gender,
                        familyId = r.PrimaryMember.FamilyId,
                        familyName = r.PrimaryMember.Family?.Name ?? "",
                        createdAt = r.PrimaryMember.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss"),
                        updatedAt = r.PrimaryMember.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss"),
                        biography = r.PrimaryMember.Biography,
                        placeOfBirth = r.PrimaryMember.PlaceOfBirth,
                        placeOfDeath = r.PrimaryMember.PlaceOfDeath,
                        profileImageUrl = r.PrimaryMember.ProfilePictureUrl,
                        isAlive = !r.PrimaryMember.DateOfDeath.HasValue
                    } : null,
                    relatedMember = r.RelatedMember != null ? new
                    {
                        id = r.RelatedMember.Id,
                        firstName = r.RelatedMember.FirstName,
                        middleName = r.RelatedMember.MiddleName,
                        lastName = r.RelatedMember.LastName,
                        dateOfBirth = r.RelatedMember.DateOfBirth?.ToString("yyyy-MM-dd"),
                        dateOfDeath = r.RelatedMember.DateOfDeath?.ToString("yyyy-MM-dd"),
                        gender = r.RelatedMember.Gender,
                        familyId = r.RelatedMember.FamilyId,
                        familyName = r.RelatedMember.Family?.Name ?? "",
                        createdAt = r.RelatedMember.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss"),
                        updatedAt = r.RelatedMember.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss"),
                        biography = r.RelatedMember.Biography,
                        placeOfBirth = r.RelatedMember.PlaceOfBirth,
                        placeOfDeath = r.RelatedMember.PlaceOfDeath,
                        profileImageUrl = r.RelatedMember.ProfilePictureUrl,
                        isAlive = !r.RelatedMember.DateOfDeath.HasValue
                    } : null
                }).ToList();
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching member relationships", error = ex.Message });
            }
        }

        [HttpGet("family/{familyId}")]
        public ActionResult<List<Relationship>> GetRelationshipsByFamily(int familyId)
        {
            try
            {
                // For now, return empty list - this method needs to be implemented in service if needed
                return Ok(new List<Relationship>());
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching family relationships", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Relationship>> GetRelationshipById(int id)
        {
            try
            {
                var relationship = await _relationshipService.GetRelationshipByIdAsync(id);
                if (relationship == null)
                {
                    return NotFound(new { message = "Relationship not found" });
                }
                
                // Return simple DTO to avoid circular references
                var result = new
                {
                    Id = relationship.Id,
                    PrimaryMemberId = relationship.PrimaryMemberId,
                    RelatedMemberId = relationship.RelatedMemberId,
                    RelationshipType = relationship.RelationshipType,
                    Notes = relationship.Notes,
                    CreatedAt = relationship.CreatedAt,
                    PrimaryMember = relationship.PrimaryMember != null ? new
                    {
                        Id = relationship.PrimaryMember.Id,
                        FirstName = relationship.PrimaryMember.FirstName,
                        MiddleName = relationship.PrimaryMember.MiddleName,
                        LastName = relationship.PrimaryMember.LastName,
                        DateOfBirth = relationship.PrimaryMember.DateOfBirth,
                        DateOfDeath = relationship.PrimaryMember.DateOfDeath,
                        Gender = relationship.PrimaryMember.Gender,
                        FamilyId = relationship.PrimaryMember.FamilyId
                    } : null,
                    RelatedMember = relationship.RelatedMember != null ? new
                    {
                        Id = relationship.RelatedMember.Id,
                        FirstName = relationship.RelatedMember.FirstName,
                        MiddleName = relationship.RelatedMember.MiddleName,
                        LastName = relationship.RelatedMember.LastName,
                        DateOfBirth = relationship.RelatedMember.DateOfBirth,
                        DateOfDeath = relationship.RelatedMember.DateOfDeath,
                        Gender = relationship.RelatedMember.Gender,
                        FamilyId = relationship.RelatedMember.FamilyId
                    } : null
                };
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching the relationship", error = ex.Message });
            }
        }

        [HttpGet("member/{memberId}/tree")]
        public async Task<ActionResult<List<Relationship>>> GetMemberRelationshipsTree(int memberId)
        {
            try
            {
                var relationships = await _relationshipService.GetMemberRelationshipsAsync(memberId);
                
                // Return simple DTOs to avoid circular references
                var result = relationships.Select(r => new
                {
                    Id = r.Id,
                    PrimaryMemberId = r.PrimaryMemberId,
                    RelatedMemberId = r.RelatedMemberId,
                    RelationshipType = r.RelationshipType,
                    Notes = r.Notes,
                    CreatedAt = r.CreatedAt,
                    PrimaryMember = r.PrimaryMember != null ? new
                    {
                        Id = r.PrimaryMember.Id,
                        FirstName = r.PrimaryMember.FirstName,
                        MiddleName = r.PrimaryMember.MiddleName,
                        LastName = r.PrimaryMember.LastName,
                        DateOfBirth = r.PrimaryMember.DateOfBirth,
                        DateOfDeath = r.PrimaryMember.DateOfDeath,
                        Gender = r.PrimaryMember.Gender,
                        FamilyId = r.PrimaryMember.FamilyId
                    } : null,
                    RelatedMember = r.RelatedMember != null ? new
                    {
                        Id = r.RelatedMember.Id,
                        FirstName = r.RelatedMember.FirstName,
                        MiddleName = r.RelatedMember.MiddleName,
                        LastName = r.RelatedMember.LastName,
                        DateOfBirth = r.RelatedMember.DateOfBirth,
                        DateOfDeath = r.RelatedMember.DateOfDeath,
                        Gender = r.RelatedMember.Gender,
                        FamilyId = r.RelatedMember.FamilyId
                    } : null
                }).ToList();
                
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching the relationship tree", error = ex.Message });
            }
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<ActionResult<Relationship>> CreateRelationship([FromBody] CreateRelationshipDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null)
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                var primaryMember = await _memberService.GetMemberByIdAsync(dto.PrimaryMemberId);
                var relatedMember = await _memberService.GetMemberByIdAsync(dto.RelatedMemberId);
                
                if (primaryMember == null || relatedMember == null)
                {
                    return BadRequest(new { message = "One or both members not found" });
                }

                var primaryFamily = await _familyService.GetFamilyByIdAsync(primaryMember.FamilyId);
                var relatedFamily = await _familyService.GetFamilyByIdAsync(relatedMember.FamilyId);
                
                if (primaryFamily?.CreatedByUserId != userId || relatedFamily?.CreatedByUserId != userId)
                {
                    return Forbid("You can only create relationships between members of your own families");
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
                
                relationship = await _relationshipService.CreateRelationshipAsync(relationship);
                
                // Return simple DTO to avoid circular references
                var result = new
                {
                    Id = relationship.Id,
                    PrimaryMemberId = relationship.PrimaryMemberId,
                    RelatedMemberId = relationship.RelatedMemberId,
                    RelationshipType = relationship.RelationshipType,
                    Notes = relationship.Notes,
                    CreatedAt = relationship.CreatedAt
                };
                
                return CreatedAtAction(nameof(GetRelationshipById), new { id = relationship.Id }, result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the relationship", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<ActionResult<Relationship>> UpdateRelationship(int id, [FromBody] UpdateRelationshipDto dto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null)
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                var existingRelationship = await _relationshipService.GetRelationshipByIdAsync(id);
                if (existingRelationship == null)
                {
                    return NotFound(new { message = "Relationship not found" });
                }

                var primaryMember = await _memberService.GetMemberByIdAsync(existingRelationship.PrimaryMemberId);
                var relatedMember = await _memberService.GetMemberByIdAsync(existingRelationship.RelatedMemberId);
                
                if (primaryMember == null || relatedMember == null)
                {
                    return BadRequest(new { message = "One or both members not found" });
                }

                var primaryFamily = await _familyService.GetFamilyByIdAsync(primaryMember.FamilyId);
                var relatedFamily = await _familyService.GetFamilyByIdAsync(relatedMember.FamilyId);
                
                if (primaryFamily?.CreatedByUserId != userId || relatedFamily?.CreatedByUserId != userId)
                {
                    return Forbid("You can only update relationships between members of your own families");
                }

                var updateRelationship = new Relationship
                {
                    RelationshipType = dto.RelationshipType,
                    Notes = dto.Notes
                };
                
                var relationship = await _relationshipService.UpdateRelationshipAsync(id, updateRelationship);
                return Ok(relationship);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the relationship", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<ActionResult> DeleteRelationship(int id)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null)
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                var existingRelationship = await _relationshipService.GetRelationshipByIdAsync(id);
                if (existingRelationship == null)
                {
                    return NotFound(new { message = "Relationship not found" });
                }

                var primaryMember = await _memberService.GetMemberByIdAsync(existingRelationship.PrimaryMemberId);
                var relatedMember = await _memberService.GetMemberByIdAsync(existingRelationship.RelatedMemberId);
                
                if (primaryMember == null || relatedMember == null)
                {
                    return BadRequest(new { message = "One or both members not found" });
                }

                var primaryFamily = await _familyService.GetFamilyByIdAsync(primaryMember.FamilyId);
                var relatedFamily = await _familyService.GetFamilyByIdAsync(relatedMember.FamilyId);
                
                if (primaryFamily?.CreatedByUserId != userId || relatedFamily?.CreatedByUserId != userId)
                {
                    return Forbid("You can only delete relationships between members of your own families");
                }

                var result = await _relationshipService.DeleteRelationshipAsync(id);
                if (!result)
                {
                    return NotFound(new { message = "Relationship not found" });
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the relationship", error = ex.Message });
            }
        }

        [HttpGet("check/{primaryMemberId}/{relatedMemberId}")]
        public ActionResult<bool> CheckRelationshipExists(int primaryMemberId, int relatedMemberId)
        {
            try
            {
                // For now, always return false - this method needs to be implemented in service if needed
                var exists = false;
                return Ok(new { exists });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while checking relationship existence", error = ex.Message });
            }
        }
    }
}
