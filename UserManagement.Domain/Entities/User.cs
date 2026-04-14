namespace UserManagement.Domain.Entities
{
    /// <summary>
    /// Represents a user in the system.
    /// A user can belong to multiple groups via the UserGroup join table.
    /// </summary>
    public class User
    {
        public int Id { get; set; }

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Computed property — never stored in DB, always derived
        public string FullName => $"{FirstName} {LastName}";

        // Navigation property — many-to-many bridge to Groups
        public ICollection<UserGroup> UserGroups { get; set; } = new List<UserGroup>();
    }
}
