using System.Text;
using System.Text.Json;
using UserManagement.Web.Models;

namespace UserManagement.Web.Services
{
    /// <summary>
    /// Service responsible for all HTTP communication with the User Management API.
    /// Using a dedicated service class keeps controllers thin and focused on
    /// routing/view logic only. If the API URL changes, only this file needs updating.
    /// HttpClient is injected via DI — never instantiate it manually (socket exhaustion).
    /// </summary>
    public class UserApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<UserApiService> _logger;

        // Reusable JSON options — camelCase to match the API response format
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public UserApiService(HttpClient httpClient, ILogger<UserApiService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        // ─────────────────────────────────────────
        // GET all users
        // ─────────────────────────────────────────
        public async Task<List<UserViewModel>> GetAllUsersAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/users");
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<UserViewModel>>(json, _jsonOptions)
                       ?? new List<UserViewModel>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all users from API.");
                return new List<UserViewModel>();
            }
        }

        // ─────────────────────────────────────────
        // GET user by ID
        // ─────────────────────────────────────────
        public async Task<UserViewModel?> GetUserByIdAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/users/{id}");

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    return null;

                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<UserViewModel>(json, _jsonOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching user {UserId} from API.", id);
                return null;
            }
        }

        // ─────────────────────────────────────────
        // POST — create user
        // ─────────────────────────────────────────
        public async Task<(bool Success, string Message)> CreateUserAsync(
            CreateUserViewModel model)
        {
            try
            {
                var payload = new
                {
                    firstName = model.FirstName,
                    lastName = model.LastName,
                    email = model.Email
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(payload),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync("api/users", content);

                if (response.IsSuccessStatusCode)
                    return (true, "User created successfully.");

                var error = await response.Content.ReadAsStringAsync();
                return (false, error);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user via API.");
                return (false, "An unexpected error occurred.");
            }
        }

        // ─────────────────────────────────────────
        // PUT — update user
        // ─────────────────────────────────────────
        public async Task<(bool Success, string Message)> UpdateUserAsync(
            UpdateUserViewModel model)
        {
            try
            {
                var payload = new
                {
                    firstName = model.FirstName,
                    lastName = model.LastName,
                    email = model.Email,
                    isActive = model.IsActive
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(payload),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PutAsync($"api/users/{model.Id}", content);

                if (response.IsSuccessStatusCode)
                    return (true, "User updated successfully.");

                var error = await response.Content.ReadAsStringAsync();
                return (false, error);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {UserId} via API.", model.Id);
                return (false, "An unexpected error occurred.");
            }
        }

        // ─────────────────────────────────────────
        // DELETE — delete user
        // ─────────────────────────────────────────
        public async Task<(bool Success, string Message)> DeleteUserAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/users/{id}");

                if (response.IsSuccessStatusCode)
                    return (true, "User deleted successfully.");

                var error = await response.Content.ReadAsStringAsync();
                return (false, error);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user {UserId} via API.", id);
                return (false, "An unexpected error occurred.");
            }
        }

        // ─────────────────────────────────────────
        // GET all groups (for the dropdown list)
        // ─────────────────────────────────────────
        public async Task<List<GroupViewModel>> GetAllGroupsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/groups");
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<GroupViewModel>>(json, _jsonOptions)
                       ?? new List<GroupViewModel>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching groups from API.");
                return new List<GroupViewModel>();
            }
        }

        // ─────────────────────────────────────────
        // POST — add user to a group
        // ─────────────────────────────────────────
        public async Task<(bool Success, string Message)> AddUserToGroupAsync(
            int userId, int groupId)
        {
            try
            {
                var response = await _httpClient
                    .PostAsync($"api/groups/{groupId}/users/{userId}", null);

                if (response.IsSuccessStatusCode)
                    return (true, "User added to group.");

                var error = await response.Content.ReadAsStringAsync();
                return (false, error);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error adding user {UserId} to group {GroupId}.", userId, groupId);
                return (false, "An unexpected error occurred.");
            }
        }

        // ─────────────────────────────────────────
        // DELETE — remove user from a group
        // ─────────────────────────────────────────
        public async Task<(bool Success, string Message)> RemoveUserFromGroupAsync(
            int userId, int groupId)
        {
            try
            {
                var response = await _httpClient
                    .DeleteAsync($"api/groups/{groupId}/users/{userId}");

                if (response.IsSuccessStatusCode)
                    return (true, "User removed from group.");

                var error = await response.Content.ReadAsStringAsync();
                return (false, error);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error removing user {UserId} from group {GroupId}.", userId, groupId);
                return (false, "An unexpected error occurred.");
            }
        }


    }
}
