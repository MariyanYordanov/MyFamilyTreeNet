using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using System.ComponentModel.DataAnnotations;
using MyFamilyTreeNet.Data.Models;
using MyFamilyTreeNet.Api.DTOs;

namespace MyFamilyTreeNet.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    [IgnoreAntiforgeryToken]
    public class ProfileController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly IMapper _mapper;
        private readonly ILogger<ProfileController> _logger;

        public ProfileController(
            UserManager<User> userManager,
            IMapper mapper,
            ILogger<ProfileController> logger)
        {
            _userManager = userManager;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Получаване на профила на текущия потребител
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<UserDto>> GetProfile()
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("Потребителят не е аутентикиран.");
                }

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return NotFound("Потребителят не е намерен.");
                }

                var userDto = _mapper.Map<UserDto>(user);
                return Ok(userDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Грешка при получаване на профил за потребител");
                return StatusCode(500, "Възникна вътрешна грешка.");
            }
        }

        /// <summary>
        /// Редактиране на профила на текущия потребител
        /// </summary>
        [HttpPut]
        public async Task<ActionResult<UserDto>> UpdateProfile([FromBody] UpdateProfileDto updateProfileDto)
        {
            try
            {
                if (updateProfileDto == null)
                {
                    return BadRequest("Profile data is required.");
                }

                _logger.LogInformation("UpdateProfile called with raw data: {@UpdateProfileDto}", updateProfileDto);
                _logger.LogInformation("UpdateProfile - FirstName: {FirstName}, MiddleName: {MiddleName}, LastName: {LastName}", 
                    updateProfileDto.FirstName, updateProfileDto.MiddleName, updateProfileDto.LastName);
                _logger.LogInformation("UpdateProfile - DateOfBirth as string: '{DateOfBirth}', Bio: '{Bio}'", 
                    updateProfileDto.DateOfBirth, updateProfileDto.Bio);
                
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("ModelState validation failed: {@ModelState}", ModelState);
                    return BadRequest(ModelState);
                }

                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("Потребителят не е аутентикиран.");
                }

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return NotFound("Потребителят не е намерен.");
                }

                user.FirstName = updateProfileDto.FirstName;
                user.MiddleName = updateProfileDto.MiddleName;
                user.LastName = updateProfileDto.LastName;
                
                // Parse DateOfBirth от string към DateTime
                if (!string.IsNullOrEmpty(updateProfileDto.DateOfBirth))
                {
                    if (DateTime.TryParse(updateProfileDto.DateOfBirth, out DateTime parsedDate))
                    {
                        user.DateOfBirth = parsedDate;
                    }
                    else
                    {
                        ModelState.AddModelError("DateOfBirth", "Невалиден формат на дата");
                        return BadRequest(ModelState);
                    }
                }
                else
                {
                    user.DateOfBirth = null;
                }
                
                user.Bio = updateProfileDto.Bio;
                user.ProfilePictureUrl = updateProfileDto.ProfilePictureUrl;

                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return BadRequest(ModelState);
                }

                var userDto = _mapper.Map<UserDto>(user);
                _logger.LogInformation("Профилът на потребител {UserId} е актуализиран успешно", userId);
                
                return Ok(userDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Грешка при актуализация на профил");
                return StatusCode(500, "Възникна вътрешна грешка.");
            }
        }

        /// <summary>
        /// Смяна на парола
        /// </summary>
        [HttpPost("change-password")]
        public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("Потребителят не е аутентикиран.");
                }

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return NotFound("Потребителят не е намерен.");
                }

                var result = await _userManager.ChangePasswordAsync(user, changePasswordDto.CurrentPassword, changePasswordDto.NewPassword);
                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return BadRequest(ModelState);
                }

                _logger.LogInformation("Паролата на потребител {UserId} е променена успешно", userId);
                return Ok("Паролата е променена успешно.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Грешка при смяна на парола");
                return StatusCode(500, "Възникна вътрешна грешка.");
            }
        }
    }

}