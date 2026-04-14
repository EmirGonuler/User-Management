namespace UserManagement.Web.Models
{
    /// <summary>
    /// Represents a Group as returned by the API.
    /// Used to populate the group assignment section on the Edit User page.
    /// </summary>
    public class GroupViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<PermissionViewModel> Permissions { get; set; } = new();
    }

    /// <summary>
    /// Represents a Permission belonging to a Group.
    /// </summary>
    public class PermissionViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Level { get; set; } = string.Empty;
    }
}
