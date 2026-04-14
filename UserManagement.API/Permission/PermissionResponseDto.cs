namespace UserManagement.API.DTOs.Permission
{
    /// <summary>
    /// Data sent back to the client for a Permission.
    /// </summary>
    public class PermissionResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Level { get; set; } = string.Empty;
    }
}
