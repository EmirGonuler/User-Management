namespace UserManagement.Web.Models
{
    /// <summary>
    /// Represents a user as displayed in the web interface.
    /// Mirrors the API response shape but is kept separate
    /// so UI concerns don't bleed into the API layer.
    /// </summary>
    public class UserViewModel
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<string> Groups { get; set; } = new();
    }
}
