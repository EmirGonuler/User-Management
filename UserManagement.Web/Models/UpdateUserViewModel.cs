using System.ComponentModel.DataAnnotations;

namespace UserManagement.Web.Models
{
    /// <summary>
    /// ViewModel for the Edit User form.
    /// Includes group assignment so users can be added/removed
    /// from groups directly on the edit page.
    /// </summary>
    public class UpdateUserViewModel
    {
        public int Id { get; set; }

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

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        // All groups available in the system (for the checkboxes)
        public List<GroupViewModel> AvailableGroups { get; set; } = new();

        // IDs of groups the user currently belongs to
        public List<int> CurrentGroupIds { get; set; } = new();

        // IDs submitted from the form checkboxes
        public List<int> SelectedGroupIds { get; set; } = new();
    }
}