namespace UserManagement.API.DTOs.User
{
    /// <summary>
    /// Data sent back to the client after a User request.
    /// Notice we never send back navigation properties or 
    /// sensitive internal fields — only what the client needs.
    /// </summary>
    public class UserResponseDto
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
