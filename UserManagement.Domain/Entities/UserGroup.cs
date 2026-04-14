namespace UserManagement.Domain.Entities
{
    /// <summary>
    /// Join table entity that links Users to Groups (many-to-many).
    /// Having this as an explicit class allows us to add extra fields
    /// in future (e.g. DateJoined, Role within group) without a migration headache.
    /// </summary>
    public class UserGroup
    {
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public int GroupId { get; set; }
        public Group Group { get; set; } = null!;

        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    }
}
