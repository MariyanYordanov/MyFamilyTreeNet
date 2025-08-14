using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MyFamilyTreeNet.Data.Models;
using MyFamilyTreeNet.Api.Models.ViewModels;
using System.Security.Claims;

namespace MyFamilyTreeNet.Api.Controllers.MVC
{
    [Authorize]
    public class AccountMvcController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ILogger<AccountMvcController> _logger;

        public AccountMvcController(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            ILogger<AccountMvcController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        private string GetCurrentUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            var model = new ChangePasswordViewModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Потребителят не е намерен.";
                return RedirectToAction("Index", "Home");
            }

            var changePasswordResult = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (!changePasswordResult.Succeeded)
            {
                foreach (var error in changePasswordResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }

            await _signInManager.RefreshSignInAsync(user);
            TempData["SuccessMessage"] = "Паролата беше променена успешно!";
            
            _logger.LogInformation("User {UserId} changed their password successfully.", user.Id);
            
            return RedirectToAction("Profile");
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Потребителят не е намерен.";
                return RedirectToAction("Index", "Home");
            }

            var model = new ProfileViewModel
            {
                Email = user.Email ?? "",
                FirstName = user.FirstName,
                MiddleName = user.MiddleName,
                LastName = user.LastName,
                DateOfBirth = user.DateOfBirth,
                Bio = user.Bio,
                CreatedAt = user.CreatedAt
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Потребителят не е намерен.";
                return RedirectToAction("Index", "Home");
            }

            // Update user properties
            user.FirstName = model.FirstName ?? string.Empty;
            user.MiddleName = model.MiddleName ?? string.Empty;
            user.LastName = model.LastName ?? string.Empty;
            user.DateOfBirth = model.DateOfBirth;
            user.Bio = model.Bio;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }

            TempData["SuccessMessage"] = "Профилът беше обновен успешно!";
            _logger.LogInformation("User {UserId} updated their profile successfully.", user.Id);

            return RedirectToAction("Profile");
        }
    }
}