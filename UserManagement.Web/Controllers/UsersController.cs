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
        public async Task<IActionResult> Create()
        {
            var model = new CreateUserViewModel
            {
                AvailableGroups = await _userApiService.GetAllGroupsAsync()
            };
            return View(model);
        }

        // ─────────────────────────────────────────
        // POST /users/create — Submit Create Form
        // ─────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
            // Remove navigation properties from validation
            ModelState.Remove(nameof(model.AvailableGroups));

            if (!ModelState.IsValid)
            {
                model.AvailableGroups = await _userApiService.GetAllGroupsAsync();
                return View(model);
            }

            try
            {
                // Step 1 — Create the user
                var (success, message) = await _userApiService.CreateUserAsync(model);

                if (!success)
                {
                    ModelState.AddModelError(string.Empty, message);
                    model.AvailableGroups = await _userApiService.GetAllGroupsAsync();
                    return View(model);
                }

                // Step 2 — Get the newly created user to find their ID
                var allUsers = await _userApiService.GetAllUsersAsync();
                var newUser = allUsers.FirstOrDefault(u =>
                    u.Email.Equals(model.Email, StringComparison.OrdinalIgnoreCase));

                // Step 3 — Assign selected groups if any were chosen
                if (newUser is not null && model.SelectedGroupIds.Any())
                {
                    foreach (var groupId in model.SelectedGroupIds)
                        await _userApiService.AddUserToGroupAsync(newUser.Id, groupId);
                }

                TempData["Success"] = "User created successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user.");
                ModelState.AddModelError(string.Empty, "An unexpected error occurred.");
                model.AvailableGroups = await _userApiService.GetAllGroupsAsync();
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
                // Load user and all available groups in parallel for performance
                var userTask = _userApiService.GetUserByIdAsync(id);
                var groupsTask = _userApiService.GetAllGroupsAsync();

                await Task.WhenAll(userTask, groupsTask);

                var user = userTask.Result;
                var groups = groupsTask.Result;

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
                    IsActive = user.IsActive,
                    AvailableGroups = groups,

                    // Match current group names against available groups to get IDs
                    CurrentGroupIds = groups
                        .Where(g => user.Groups.Contains(g.Name))
                        .Select(g => g.Id)
                        .ToList()
                };

                // Pre-select current groups
                model.SelectedGroupIds = new List<int>(model.CurrentGroupIds);

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
            // Remove navigation properties from validation
            ModelState.Remove(nameof(model.AvailableGroups));
            ModelState.Remove(nameof(model.CurrentGroupIds));

            if (!ModelState.IsValid)
            {
                model.AvailableGroups = await _userApiService.GetAllGroupsAsync();
                return View(model);
            }

            try
            {
                model.Id = id;

                // Step 1 — Save user details (name, email, isActive)
                var (success, message) = await _userApiService.UpdateUserAsync(model);

                if (!success)
                {
                    ModelState.AddModelError(string.Empty, message);
                    model.AvailableGroups = await _userApiService.GetAllGroupsAsync();
                    return View(model);
                }

                // Step 2 — Sync group memberships
                // Groups to add = selected but not currently in
                var groupsToAdd = model.SelectedGroupIds
                    .Except(model.CurrentGroupIds)
                    .ToList();

                // Groups to remove = currently in but not selected
                var groupsToRemove = model.CurrentGroupIds
                    .Except(model.SelectedGroupIds)
                    .ToList();

                foreach (var groupId in groupsToAdd)
                    await _userApiService.AddUserToGroupAsync(id, groupId);

                foreach (var groupId in groupsToRemove)
                    await _userApiService.RemoveUserFromGroupAsync(id, groupId);

                TempData["Success"] = "User updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {UserId}.", id);
                ModelState.AddModelError(string.Empty, "An unexpected error occurred.");
                model.AvailableGroups = await _userApiService.GetAllGroupsAsync();
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