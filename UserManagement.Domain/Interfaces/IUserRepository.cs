using UserManagement.Domain.Entities;

namespace UserManagement.Domain.Interfaces
{
    /// <summary>
    /// User-specific repository interface.
    /// Inherits generic CRUD from IRepository and adds User-specific queries.
    /// </summary>
    public interface IUserRepository : IRepository<User>
    {
        Task<User?> GetByEmailAsync(string email);
        Task<IEnumerable<User>> GetUsersByGroupAsync(int groupId);
        Task<int> GetTotalUserCountAsync();
        Task<IEnumerable<UserGroupCountDto>> GetUserCountPerGroupAsync();
    }

    /// <summary>
    /// DTO (Data Transfer Object) used to return user count per group.
    /// Kept in this file for proximity to the interface that uses it.
    /// </summary>
    public class UserGroupCountDto
    {
        public string GroupName { get; set; } = string.Empty;
        public int UserCount { get; set; }
    }
}
