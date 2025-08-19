using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MyFamilyTreeNet.Data.Models;
using MyFamilyTreeNet.Api.DTOs;

namespace MyFamilyTreeNet.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [IgnoreAntiforgeryToken]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IConfiguration configuration,
            IMapper mapper,
            ILogger<AuthController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            if (model == null)
                return BadRequest("Model cannot be null");
                
            _logger.LogInformation("Register attempt for email: {Email}", model.Email);
            _logger.LogInformation("Register model: FirstName={FirstName}, MiddleName={MiddleName}, LastName={LastName}", 
                model.FirstName, model.MiddleName, model.LastName);
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = new User
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                MiddleName = model.MiddleName,
                LastName = model.LastName
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "User");
                
                // Auto-login after registration
                var token = await GenerateJwtToken(user);
                var userDto = _mapper.Map<UserDto>(user);
                
                return Ok(new 
                { 
                    token = token,
                    user = userDto,
                    message = "Registration successful"
                });
            }

            return BadRequest(result.Errors);
        }

        [HttpGet("test")]
        public IActionResult Test()
        {
            _logger.LogInformation("TEST endpoint called!");
            return Ok(new { message = "Test endpoint works!" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            if (model == null)
                return BadRequest("Model cannot be null");
                
            _logger.LogInformation("Login attempt for email: {Email}", model.Email);
            
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Login ModelState validation failed: {@ModelState}", ModelState);
                return BadRequest(ModelState);
            }

            var result = await _signInManager.PasswordSignInAsync(
                model.Email, 
                model.Password, 
                model.RememberMe, 
                lockoutOnFailure: false);

            if (result.Succeeded)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                    return BadRequest("User not found");
                    
                var token = await GenerateJwtToken(user);
                
                var userDto = _mapper.Map<UserDto>(user);
                return Ok(new 
                { 
                    token = token,
                    user = userDto
                });
            }

            return Unauthorized(new { message = "Invalid login attempt" });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok(new { message = "Logged out successfully" });
        }

        private async Task<string> GenerateJwtToken(User user)
        {
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id),
                new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new(ClaimTypes.NameIdentifier, user.Id),
                new(ClaimTypes.Name, user.UserName ?? string.Empty)
            };

            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var secretKey = _configuration["JwtSettings:SecretKey"] ?? throw new InvalidOperationException("JWT Secret Key not configured");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expirationHours = Convert.ToDouble(_configuration["JwtSettings:ExpirationInHours"]);
            var expirationTime = DateTime.Now.AddHours(expirationHours);
            
            _logger.LogInformation("JWT Token generation - ExpirationInHours: {Hours}", expirationHours);
            _logger.LogInformation("JWT Token generation - Expiration time: {ExpirationTime}", expirationTime);
            
            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: expirationTime,
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// Admin emergency password reset - FOR DEBUG ONLY
        /// </summary>
        [HttpPost("admin-emergency-reset")]
        [AllowAnonymous]
        public async Task<ActionResult> AdminEmergencyReset([FromBody] string newPassword)
        {
            try
            {
                var adminUser = await _userManager.FindByEmailAsync("admin@myfamilytreenet.com");
                if (adminUser == null)
                {
                    return NotFound("Admin user not found");
                }

                // Remove current password and set new one
                var token = await _userManager.GeneratePasswordResetTokenAsync(adminUser);
                var result = await _userManager.ResetPasswordAsync(adminUser, token, newPassword);
                
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return BadRequest($"Password reset failed: {errors}");
                }

                _logger.LogWarning("EMERGENCY: Admin password was reset!");
                return Ok(new { message = "Admin password reset successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during admin emergency reset");
                return StatusCode(500, "Internal server error");
            }
        }
    }

}