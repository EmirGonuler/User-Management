using System.ComponentModel.DataAnnotations;

namespace UserManagement.API.DTOs.User
{
    /// <summary>
    /// Data received from the client when updating an existing user.
    /// Includes IsActive so admins can deactivate users without deleting them.
    /// </summary>
    public class UpdateUserDto
    {
        [Required(ErrorMessage = "First name is required.")]
        [StringLength(100, ErrorMessage = "First name cannot exceed 100 characters.")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required.")]
        [StringLength(100, ErrorMessage = "Last name cannot exceed 100 characters.")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        [StringLength(255, ErrorMessage = "Email cannot exceed 255 characters.")]
        public string Email { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
    }
}
