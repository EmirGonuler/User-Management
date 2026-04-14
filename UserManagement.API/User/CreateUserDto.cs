using System.ComponentModel.DataAnnotations;

namespace UserManagement.API.DTOs.User
{
    /// <summary>
    /// Data received from the client when creating a new user.
    /// DataAnnotations provide automatic model validation —
    /// invalid requests are rejected before hitting the controller logic.
    /// </summary>
    public class CreateUserDto
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
    }
}
