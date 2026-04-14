using Microsoft.AspNetCore.Mvc;
using UserManagement.Web.Models;
using UserManagement.Web.Services;

namespace UserManagement.Web.Controllers
{
    /// <summary>
    /// MVC Controller for the User Management web interface.
    /// Stays thin — all API communication is delegated to UserApiService.
    /// Follows the PRG (Post-Redirect-Get) pattern on form submissions
    /// to prevent duplicate submissions on browser refresh.
    /// </summary>
    public class UsersController : Controller
    {
        private readonly UserApiService _userApiService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(
            UserApiService userApiService,
            ILogger<UsersController> logger)
        {
            _userApiService = userApiService;
            _logger = logger;
        }

        // ─────────────────────────────────────────
        // GET /users — User List
        // ─────────────────────────────────────────
        public async Task<IActionResult> Index()
        {
            try
            {
                var users = await _userApiService.GetAllUsersAsync();
                return View(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading user list.");
                TempData["Error"] = "Unable to load users. Is the API running?";
                return View(new List<UserViewModel>());
            }
        }

        // ─────────────────────────────────────────
        // GET /users/create — Show Create Form
        // ─────────────────────────────────────────
        public IActionResult Create()
        {
            return View(new CreateUserViewModel());
        }

        // ─────────────────────────────────────────
        // POST /users/create — Submit Create Form
        // ─────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var (success, message) = await _userApiService.CreateUserAsync(model);

                if (success)
                {
                    TempData["Success"] = "User created successfully!";
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError(string.Empty, message);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user.");
                ModelState.AddModelError(string.Empty, "An unexpected error occurred.");
                return View(model);
            }
        }

        // ─────────────────────────────────────────
        // GET /users/edit/{id} — Show Edit Form
        // ─────────────────────────────────────────
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var user = await _userApiService.GetUserByIdAsync(id);

                if (user is null)
                {
                    TempData["Error"] = $"User with ID {id} was not found.";
                    return RedirectToAction(nameof(Index));
                }

                var model = new UpdateUserViewModel
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    IsActive = user.IsActive
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading edit form for user {UserId}.", id);
                TempData["Error"] = "Unable to load user details.";
                return RedirectToAction(nameof(Index));
            }
        }

        // ─────────────────────────────────────────
        // POST /users/edit/{id} — Submit Edit Form
        // ─────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateUserViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                model.Id = id;
                var (success, message) = await _userApiService.UpdateUserAsync(model);

                if (success)
                {
                    TempData["Success"] = "User updated successfully!";
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError(string.Empty, message);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {UserId}.", id);
                ModelState.AddModelError(string.Empty, "An unexpected error occurred.");
                return View(model);
            }
        }

        // ─────────────────────────────────────────
        // GET /users/delete/{id} — Show Delete Confirmation
        // ─────────────────────────────────────────
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var user = await _userApiService.GetUserByIdAsync(id);

                if (user is null)
                {
                    TempData["Error"] = $"User with ID {id} was not found.";
                    return RedirectToAction(nameof(Index));
                }

                return View(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading delete page for user {UserId}.", id);
                TempData["Error"] = "Unable to load user details.";
                return RedirectToAction(nameof(Index));
            }
        }

        // ─────────────────────────────────────────
        // POST /users/delete/{id} — Confirm Delete
        // ─────────────────────────────────────────
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var (success, message) = await _userApiService.DeleteUserAsync(id);

                if (success)
                {
                    TempData["Success"] = "User deleted successfully!";
                    return RedirectToAction(nameof(Index));
                }

                TempData["Error"] = message;
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user {UserId}.", id);
                TempData["Error"] = "An unexpected error occurred.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}