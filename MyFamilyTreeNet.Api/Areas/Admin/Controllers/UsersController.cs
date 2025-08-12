using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyFamilyTreeNet.Data;
using MyFamilyTreeNet.Data.Models;

namespace MyFamilyTreeNet.Api.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AppDbContext _context;
        private readonly ILogger<UsersController> _logger;

        public UsersController(
            UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager,
            AppDbContext context,
            ILogger<UsersController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _logger = logger;
        }

        // GET: /Admin/Users
        public async Task<IActionResult> Index(string search = "", int page = 1)
        {
            ViewData["Title"] = "Управление на потребители";

            try
            {
                var query = _context.Users.AsQueryable();

                // Apply search filter
                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(u => 
                        u.FirstName.Contains(search) ||
                        (u.MiddleName != null && u.MiddleName.Contains(search)) ||
                        u.LastName.Contains(search) ||
                        (u.Email != null && u.Email.Contains(search)));
                }

                // Order by creation date (newest first)
                query = query.OrderByDescending(u => u.CreatedAt);

                // Pagination
                const int pageSize = 10;
                var totalCount = await query.CountAsync();
                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                var users = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                // Get roles for each user
                var usersWithRoles = new List<object>();
                foreach (var user in users)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    usersWithRoles.Add(new
                    {
                        User = user,
                        Roles = roles,
                        FamilyCount = await _context.Families.CountAsync(f => f.CreatedByUserId == user.Id)
                    });
                }

                ViewBag.Users = usersWithRoles;
                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = totalPages;
                ViewBag.SearchTerm = search;
                ViewBag.TotalCount = totalCount;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading users");
                TempData["ErrorMessage"] = "Възникна грешка при зареждането на потребителите.";
                ViewBag.Users = new List<object>();
                return View();
            }
        }

        // GET: /Admin/Users/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return NotFound();
                }

                ViewData["Title"] = $"Детайли за {user.FirstName} {user.LastName}";

                var roles = await _userManager.GetRolesAsync(user);
                var familiesCreated = await _context.Families
                    .Where(f => f.CreatedByUserId == user.Id)
                    .CountAsync();

                var userDetails = new
                {
                    User = user,
                    Roles = roles,
                    FamiliesCreated = familiesCreated,
                    LastActivity = user.LastLoginAt?.ToString("dd.MM.yyyy HH:mm") ?? "Никога"
                };

                ViewBag.UserDetails = userDetails;
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading user details for ID: {UserId}", id);
                TempData["ErrorMessage"] = "Възникна грешка при зареждането на потребителя.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: /Admin/Users/ToggleRole
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleRole(string userId, string role)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "Потребителят не е намерен.";
                    return RedirectToAction(nameof(Index));
                }

                var userRoles = await _userManager.GetRolesAsync(user);
                
                if (userRoles.Contains(role))
                {
                    await _userManager.RemoveFromRoleAsync(user, role);
                    TempData["SuccessMessage"] = $"Ролята {role} беше премахната от {user.FirstName} {user.LastName}.";
                }
                else
                {
                    await _userManager.AddToRoleAsync(user, role);
                    TempData["SuccessMessage"] = $"Ролята {role} беше добавена към {user.FirstName} {user.LastName}.";
                }

                return RedirectToAction(nameof(Details), new { id = userId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling role {Role} for user {UserId}", role, userId);
                TempData["ErrorMessage"] = "Възникна грешка при промяната на ролята.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: /Admin/Users/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "Потребителят не е намерен.";
                    return RedirectToAction(nameof(Index));
                }

                // Check if user has created families
                var hasContent = await _context.Families.AnyAsync(f => f.CreatedByUserId == user.Id);
                if (hasContent)
                {
                    TempData["ErrorMessage"] = "Не можете да изтриете потребител, който е създал семейства.";
                    return RedirectToAction(nameof(Details), new { id = userId });
                }

                var result = await _userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    TempData["SuccessMessage"] = $"Потребителят {user.FirstName} {user.LastName} беше изтрит успешно.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Възникна грешка при изтриването на потребителя.";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user {UserId}", userId);
                TempData["ErrorMessage"] = "Възникна грешка при изтриването на потребителя.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}