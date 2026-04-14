using UserManagement.API.DTOs.Permission;

namespace UserManagement.API.DTOs.Group
{
    /// <summary>
    /// Data sent back to the client for a Group,
    /// including its associated permissions and member count.
    /// </summary>
    public class GroupResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int MemberCount { get; set; }
        public List<PermissionResponseDto> Permissions { get; set; } = new();
    }
}
