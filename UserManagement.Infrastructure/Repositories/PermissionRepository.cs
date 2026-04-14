using Microsoft.EntityFrameworkCore;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Interfaces;
using UserManagement.Infrastructure.Data;

namespace UserManagement.Infrastructure.Repositories
{
    /// <summary>
    /// Handles all database operations specific to Permissions.
    /// </summary>
    public class PermissionRepository : Repository<Permission>, IPermissionRepository
    {
        public PermissionRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<Permission>> GetPermissionsByGroupAsync(int groupId)
        {
            return await _context.Permissions
                .Where(p => p.GroupId == groupId)
                .ToListAsync();
        }
    }
}
