using UserManagement.Domain.Enums;

namespace UserManagement.Domain.Entities
{
    /// <summary>
    /// Represents a permission that can be assigned to a group.
    /// A group can have multiple permissions (e.g. Level1, Level2).
    /// </summary>
    public class Permission
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public PermissionLevel Level { get; set; }

        // Navigation property — one Permission belongs to one Group
        public int GroupId { get; set; }
        public Group Group { get; set; } = null!;
    }
}
