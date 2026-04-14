using UserManagement.Domain.Entities;

namespace UserManagement.Domain.Interfaces
{
    /// <summary>
    /// Permission-specific repository interface.
    /// Inherits generic CRUD from IRepository and adds Permission-specific queries.
    /// </summary>
    public interface IPermissionRepository : IRepository<Permission>
    {
        Task<IEnumerable<Permission>> GetPermissionsByGroupAsync(int groupId);
    }
}