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
                _logger.LogInformation("ChangePassword called with: CurrentPassword length={CurrentLength}, NewPassword length={NewLength}, ConfirmNewPassword length={ConfirmLength}", 
                    changePasswordDto?.CurrentPassword?.Length ?? 0,
                    changePasswordDto?.NewPassword?.Length ?? 0, 
                    changePasswordDto?.ConfirmNewPassword?.Length ?? 0);

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("ModelState validation errors:");
                    foreach (var error in ModelState)
                    {
                        _logger.LogWarning("Key: {Key}, Errors: {Errors}", error.Key, string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage)));
                    }
                    return BadRequest(ModelState);
                }

                var userId = _userManager.GetUserId(User);
                _logger.LogInformation("Authenticated user ID: {UserId}", userId);
                
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("No authenticated user found in request");
                    return Unauthorized("Потребителят не е аутентикиран.");
                }

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("User not found for ID: {UserId}", userId);
                    return NotFound("Потребителят не е намерен.");
                }

                _logger.LogInformation("Attempting to change password for user: {Email}, UserID: {UserId}", user.Email, user.Id);
                
                // First, let's verify the current password
                var isCurrentPasswordValid = await _userManager.CheckPasswordAsync(user, changePasswordDto.CurrentPassword);
                _logger.LogInformation("Current password validation result: {IsValid}", isCurrentPasswordValid);
                
                if (!isCurrentPasswordValid)
                {
                    _logger.LogWarning("Current password is incorrect for user {Email}", user.Email);
                    ModelState.AddModelError(string.Empty, "Incorrect password.");
                    return BadRequest(ModelState);
                }

                var result = await _userManager.ChangePasswordAsync(user, changePasswordDto.CurrentPassword, changePasswordDto.NewPassword);
                if (!result.Succeeded)
                {
                    _logger.LogWarning("Password change failed. Errors: {Errors}", string.Join(", ", result.Errors.Select(e => $"{e.Code}: {e.Description}")));
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return BadRequest(ModelState);
                }

                _logger.LogInformation("Паролата на потребител {UserId} ({Email}) е променена успешно", userId, user.Email);
                return Ok(new { message = "Паролата е променена успешно." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Грешка при смяна на парола");
                return StatusCode(500, "Възникна вътрешна грешка.");
            }
        }

        /// <summary>
        /// Reset password for current user (for debugging)
        /// </summary>
        [HttpPost("reset-password-debug")]
        public async Task<ActionResult> ResetPasswordDebug([FromBody] string newPassword)
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

                // First test if we can validate Admin123!
                var testResult = await _userManager.CheckPasswordAsync(user, "Admin123!");
                _logger.LogInformation("Test password 'Admin123!' validation result: {Result}", testResult);

                // Remove current password and set new one
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
                
                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return BadRequest(ModelState);
                }

                // Verify the new password works
                var newPasswordCheck = await _userManager.CheckPasswordAsync(user, newPassword);
                _logger.LogInformation("New password validation result: {Result}", newPasswordCheck);

                _logger.LogInformation("Password reset for user {UserId} ({Email})", userId, user.Email);
                return Ok(new { 
                    message = "Паролата е сменена успешно чрез reset.",
                    oldPasswordWasAdmin123 = testResult,
                    newPasswordWorks = newPasswordCheck
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Грешка при reset на парола");
                return StatusCode(500, "Възникна вътрешна грешка.");
            }
        }

        /// <summary>
        /// Get current user info for debugging
        /// </summary>
        [HttpGet("debug-user-info")]
        public async Task<ActionResult> GetCurrentUserInfo()
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

                // Test password check with known passwords
                var testPasswords = new[] { "Admin123!", "Demo123!", "Password123!" };
                var passwordTests = new Dictionary<string, bool>();
                
                foreach (var testPass in testPasswords)
                {
                    passwordTests[testPass] = await _userManager.CheckPasswordAsync(user, testPass);
                }

                return Ok(new { 
                    userId = user.Id,
                    email = user.Email, 
                    firstName = user.FirstName,
                    lastName = user.LastName,
                    hasPassword = await _userManager.HasPasswordAsync(user),
                    passwordHashFirst10 = user.PasswordHash?.Substring(0, Math.Min(10, user.PasswordHash.Length)) + "...",
                    passwordTests = passwordTests,
                    authClaims = User.Claims.Select(c => new { c.Type, c.Value }).ToList()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Грешка при получаване на информация за потребител");
                return StatusCode(500, "Възникна вътрешна грешка.");
            }
        }
        
        /// <summary>
        /// Test specific password - DEBUG ONLY
        /// </summary>
        [HttpPost("test-password")]
        public async Task<ActionResult> TestPassword([FromBody] string password)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("No user authenticated");
                }

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return NotFound("User not found");
                }

                var isValid = await _userManager.CheckPasswordAsync(user, password);
                
                _logger.LogInformation("Password test for user {Email}: password='{Password}' result={Result}", 
                    user.Email, password, isValid);

                return Ok(new { 
                    email = user.Email,
                    passwordTested = password,
                    isValid = isValid
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing password");
                return StatusCode(500, "Internal error");
            }
        }
        
        /// <summary>
        /// Force reset admin password - EMERGENCY DEBUG ONLY
        /// </summary>
        [HttpPost("force-reset-admin")]
        public async Task<ActionResult> ForceResetAdmin()
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                var currentUser = await _userManager.FindByIdAsync(userId);
                
                if (currentUser?.Email != "admin@myfamilytreenet.com")
                {
                    return Unauthorized("This operation is only available for admin user");
                }

                // Force password reset
                var token = await _userManager.GeneratePasswordResetTokenAsync(currentUser);
                var resetResult = await _userManager.ResetPasswordAsync(currentUser, token, "Admin123!");
                
                if (!resetResult.Succeeded)
                {
                    return BadRequest(new { 
                        message = "Failed to reset password",
                        errors = resetResult.Errors.Select(e => e.Description)
                    });
                }
                
                // Verify it worked
                var verifyResult = await _userManager.CheckPasswordAsync(currentUser, "Admin123!");
                
                _logger.LogWarning("EMERGENCY: Admin password was force reset to default!");
                
                return Ok(new { 
                    message = "Admin password has been reset to Admin123!",
                    verified = verifyResult
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in force reset");
                return StatusCode(500, "Internal error");
            }
        }
    }

}