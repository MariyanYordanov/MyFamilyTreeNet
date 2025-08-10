/*
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using MyFamilyTreeNet.Api.Contracts;
using MyFamilyTreeNet.Api.DTOs;

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
        public async Task<ActionResult<List<RelationshipDto>>> GetAllRelationships()
        {
            try
            {
                var relationships = await _relationshipService.GetAllRelationshipsAsync();
                return Ok(relationships);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching relationships", error = ex.Message });
            }
        }

        [HttpGet("member/{memberId}")]
        public async Task<ActionResult<List<RelationshipDto>>> GetRelationshipsByMember(int memberId)
        {
            try
            {
                var relationships = await _relationshipService.GetRelationshipsByMemberAsync(memberId);
                return Ok(relationships);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching member relationships", error = ex.Message });
            }
        }

        [HttpGet("family/{familyId}")]
        public async Task<ActionResult<List<RelationshipDto>>> GetRelationshipsByFamily(int familyId)
        {
            try
            {
                var relationships = await _relationshipService.GetRelationshipsByFamilyAsync(familyId);
                return Ok(relationships);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching family relationships", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<RelationshipDto>> GetRelationshipById(int id)
        {
            try
            {
                var relationship = await _relationshipService.GetRelationshipByIdAsync(id);
                if (relationship == null)
                {
                    return NotFound(new { message = "Relationship not found" });
                }
                return Ok(relationship);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching the relationship", error = ex.Message });
            }
        }

        [HttpGet("member/{memberId}/tree")]
        public async Task<ActionResult<MemberRelationshipsDto>> GetMemberRelationshipsTree(int memberId)
        {
            try
            {
                var tree = await _relationshipService.GetMemberRelationshipsTreeAsync(memberId);
                return Ok(tree);
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
        [Authorize]
        public async Task<ActionResult<RelationshipDto>> CreateRelationship([FromBody] CreateRelationshipDto dto)
        {
            try
            {
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

                var relationship = await _relationshipService.CreateRelationshipAsync(dto, userId);
                return CreatedAtAction(nameof(GetRelationshipById), new { id = relationship.Id }, relationship);
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
        [Authorize]
        public async Task<ActionResult<RelationshipDto>> UpdateRelationship(int id, [FromBody] UpdateRelationshipDto dto)
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

                var relationship = await _relationshipService.UpdateRelationshipAsync(id, dto);
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
        [Authorize]
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
        public async Task<ActionResult<bool>> CheckRelationshipExists(int primaryMemberId, int relatedMemberId)
        {
            try
            {
                var exists = await _relationshipService.RelationshipExistsAsync(primaryMemberId, relatedMemberId);
                return Ok(new { exists });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while checking relationship existence", error = ex.Message });
            }
        }
    }
}
*/