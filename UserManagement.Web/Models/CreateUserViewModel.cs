using System.ComponentModel.DataAnnotations;

namespace UserManagement.Web.Models
{
    /// <summary>
    /// ViewModel for the Create User form.
    /// Includes group selection so a user can be assigned
    /// to groups at the point of creation.
    /// </summary>
    public class CreateUserViewModel
    {
        [Required(ErrorMessage = "First name is required.")]
        [StringLength(100, ErrorMessage = "First name cannot exceed 100 characters.")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required.")]
        [StringLength(100, ErrorMessage = "Last name cannot exceed 100 characters.")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [StringLength(255)]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        // All available groups for the checkboxes
        public List<GroupViewModel> AvailableGroups { get; set; } = new();

        // IDs selected on the form
        public List<int> SelectedGroupIds { get; set; } = new();
    }
}