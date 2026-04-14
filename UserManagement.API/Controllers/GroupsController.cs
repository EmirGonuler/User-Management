using Microsoft.AspNetCore.Mvc;
using UserManagement.API.DTOs.Group;
using UserManagement.API.DTOs.Permission;
using UserManagement.Domain.Interfaces;

namespace UserManagement.API.Controllers
{
    /// <summary>
    /// Handles all HTTP requests related to Group management.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class GroupsController : ControllerBase
    {
        private readonly IGroupRepository _groupRepository;
        private readonly ILogger<GroupsController> _logger;

        public GroupsController(
            IGroupRepository groupRepository,
            ILogger<GroupsController> logger)
        {
            _groupRepository = groupRepository;
            _logger = logger;
        }

        // ─────────────────────────────────────────
        // GET api/groups
        // ─────────────────────────────────────────
        /// <summary>Gets all groups.</summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<GroupResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var groups = await _groupRepository.GetAllAsync();
                var result = groups.Select(MapToResponseDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all groups.");
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        // ─────────────────────────────────────────
        // GET api/groups/{id}
        // ─────────────────────────────────────────
        /// <summary>Gets a single group by ID, including its permissions.</summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(GroupResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var group = await _groupRepository.GetGroupWithPermissionsAsync(id);

                if (group is null)
                {
                    _logger.LogWarning("Group with ID {GroupId} was not found.", id);
                    return NotFound($"Group with ID {id} was not found.");
                }

                return Ok(MapToResponseDto(group));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving group with ID {GroupId}.", id);
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        // ─────────────────────────────────────────
        // GET api/groups/{id}/users
        // ─────────────────────────────────────────
        /// <summary>Gets all users belonging to a specific group.</summary>
        [HttpGet("{id:int}/users")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetGroupUsers(int id)
        {
            try
            {
                var group = await _groupRepository.GetGroupWithUsersAsync(id);

                if (group is null)
                {
                    return NotFound($"Group with ID {id} was not found.");
                }

                var users = group.UserGroups.Select(ug => new
                {
                    ug.User.Id,
                    ug.User.FullName,
                    ug.User.Email,
                    ug.User.IsActive,
                    JoinedAt = ug.JoinedAt
                });

                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving users for group ID {GroupId}.", id);
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        // ─────────────────────────────────────────
        // POST api/groups/{groupId}/users/{userId}
        // ─────────────────────────────────────────
        /// <summary>Adds a user to a group.</summary>
        [HttpPost("{groupId:int}/users/{userId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddUserToGroup(int groupId, int userId)
        {
            try
            {
                var groupExists = await _groupRepository.ExistsAsync(groupId);

                if (!groupExists)
                {
                    return NotFound($"Group with ID {groupId} was not found.");
                }

                await _groupRepository.AddUserToGroupAsync(userId, groupId);

                _logger.LogInformation(
                    "User {UserId} added to Group {GroupId}.", userId, groupId);

                return Ok(new { Message = $"User {userId} successfully added to group {groupId}." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error adding user {UserId} to group {GroupId}.", userId, groupId);
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        // ─────────────────────────────────────────
        // DELETE api/groups/{groupId}/users/{userId}
        // ─────────────────────────────────────────
        /// <summary>Removes a user from a group.</summary>
        [HttpDelete("{groupId:int}/users/{userId:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RemoveUserFromGroup(int groupId, int userId)
        {
            try
            {
                var groupExists = await _groupRepository.ExistsAsync(groupId);

                if (!groupExists)
                {
                    return NotFound($"Group with ID {groupId} was not found.");
                }

                await _groupRepository.RemoveUserFromGroupAsync(userId, groupId);

                _logger.LogInformation(
                    "User {UserId} removed from Group {GroupId}.", userId, groupId);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error removing user {UserId} from group {GroupId}.", userId, groupId);
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        // ─────────────────────────────────────────
        // PRIVATE HELPER — Entity → DTO Mapping
        // ─────────────────────────────────────────
        private static GroupResponseDto MapToResponseDto(
            UserManagement.Domain.Entities.Group group)
        {
            return new GroupResponseDto
            {
                Id = group.Id,
                Name = group.Name,
                Description = group.Description,
                CreatedAt = group.CreatedAt,
                MemberCount = group.UserGroups?.Count ?? 0,
                Permissions = group.Permissions?
                    .Select(p => new PermissionResponseDto
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Description = p.Description,
                        Level = p.Level.ToString()
                    })
                    .ToList() ?? new List<PermissionResponseDto>()
            };
        }
    }
}
