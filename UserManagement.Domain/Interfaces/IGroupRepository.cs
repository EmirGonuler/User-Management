using UserManagement.Domain.Entities;

namespace UserManagement.Domain.Interfaces
{
    /// <summary>
    /// Group-specific repository interface.
    /// Inherits generic CRUD from IRepository and adds Group-specific queries.
    /// </summary>
    public interface IGroupRepository : IRepository<Group>
    {
        Task<Group?> GetGroupWithUsersAsync(int groupId);
        Task<Group?> GetGroupWithPermissionsAsync(int groupId);
        Task AddUserToGroupAsync(int userId, int groupId);
        Task RemoveUserFromGroupAsync(int userId, int groupId);
    }
}
