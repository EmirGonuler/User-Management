namespace UserManagement.Domain.Entities
{
    /// <summary>
    /// Represents a group that users can belong to.
    /// A group holds multiple permissions and can have many users.
    /// </summary>
    public class Group
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property — a Group has many Permissions
        public ICollection<Permission> Permissions { get; set; } = new List<Permission>();

        // Navigation property — many-to-many bridge to Users
        public ICollection<UserGroup> UserGroups { get; set; } = new List<UserGroup>();
    }
}
