using Microsoft.AspNetCore.Mvc;
using UserManagement.API.DTOs.User;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Interfaces;

namespace UserManagement.API.Controllers
{
    /// <summary>
    /// Handles all HTTP requests related to User management.
    /// Follows RESTful conventions — URLs are nouns, HTTP verbs define the action.
    /// All actions are async to avoid blocking threads under load.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<UsersController> _logger;

        public UsersController(
            IUserRepository userRepository,
            ILogger<UsersController> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        // ─────────────────────────────────────────
        // GET api/users
        // ─────────────────────────────────────────
        /// <summary>Gets all users in the system.</summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<UserResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var users = await _userRepository.GetAllAsync();
                var result = users.Select(MapToResponseDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all users.");
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        // ─────────────────────────────────────────
        // GET api/users/{id}
        // ─────────────────────────────────────────
        /// <summary>Gets a single user by their ID.</summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(id);

                if (user is null)
                {
                    _logger.LogWarning("User with ID {UserId} was not found.", id);
                    return NotFound($"User with ID {id} was not found.");
                }

                return Ok(MapToResponseDto(user));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user with ID {UserId}.", id);
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        // ─────────────────────────────────────────
        // GET api/users/count
        // ─────────────────────────────────────────
        /// <summary>Gets the total count of active users.</summary>
        [HttpGet("count")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUserCount()
        {
            try
            {
                var count = await _userRepository.GetTotalUserCountAsync();
                return Ok(new { TotalActiveUsers = count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user count.");
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        // ─────────────────────────────────────────
        // GET api/users/count-per-group
        // ─────────────────────────────────────────
        /// <summary>Gets the number of active users in each group.</summary>
        [HttpGet("count-per-group")]
        [ProducesResponseType(typeof(IEnumerable<UserGroupCountDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCountPerGroup()
        {
            try
            {
                var result = await _userRepository.GetUserCountPerGroupAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user count per group.");
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        // ─────────────────────────────────────────
        // POST api/users
        // ─────────────────────────────────────────
        /// <summary>Creates a new user.</summary>
        [HttpPost]
        [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Create([FromBody] CreateUserDto dto)
        {
            try
            {
                // Check for duplicate email before creating
                var existingUser = await _userRepository.GetByEmailAsync(dto.Email);

                if (existingUser is not null)
                {
                    return Conflict($"A user with email '{dto.Email}' already exists.");
                }

                var user = new User
                {
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    Email = dto.Email,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                await _userRepository.AddAsync(user);

                _logger.LogInformation("User '{FullName}' created with ID {UserId}.",
                    user.FullName, user.Id);

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = user.Id },
                    MapToResponseDto(user));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user.");
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        // ─────────────────────────────────────────
        // PUT api/users/{id}
        // ─────────────────────────────────────────
        /// <summary>Updates an existing user.</summary>
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateUserDto dto)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(id);

                if (user is null)
                {
                    return NotFound($"User with ID {id} was not found.");
                }

                // If email is changing, check the new one isn't already taken
                if (!user.Email.Equals(dto.Email, StringComparison.OrdinalIgnoreCase))
                {
                    var emailTaken = await _userRepository.GetByEmailAsync(dto.Email);

                    if (emailTaken is not null)
                    {
                        return Conflict($"Email '{dto.Email}' is already in use.");
                    }
                }

                // Update only the fields the client is allowed to change
                user.FirstName = dto.FirstName;
                user.LastName = dto.LastName;
                user.Email = dto.Email;
                user.IsActive = dto.IsActive;

                await _userRepository.UpdateAsync(user);

                _logger.LogInformation("User with ID {UserId} updated.", id);

                return Ok(MapToResponseDto(user));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user with ID {UserId}.", id);
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        // ─────────────────────────────────────────
        // DELETE api/users/{id}
        // ─────────────────────────────────────────
        /// <summary>Deletes a user by ID.</summary>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var exists = await _userRepository.ExistsAsync(id);

                if (!exists)
                {
                    return NotFound($"User with ID {id} was not found.");
                }

                await _userRepository.DeleteAsync(id);

                _logger.LogInformation("User with ID {UserId} deleted.", id);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user with ID {UserId}.", id);
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        // ─────────────────────────────────────────
        // PRIVATE HELPER — Entity → DTO Mapping
        // ─────────────────────────────────────────
        /// <summary>
        /// Maps a User entity to a UserResponseDto.
        /// Keeping mapping logic here avoids a third-party dependency like AutoMapper
        /// while still keeping it DRY — one place to update if fields change.
        /// </summary>
        private static UserResponseDto MapToResponseDto(User user)
        {
            return new UserResponseDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                FullName = user.FullName,
                Email = user.Email,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                Groups = user.UserGroups?
                    .Select(ug => ug.Group?.Name ?? string.Empty)
                    .ToList() ?? new List<string>()
            };
        }
    }
}
