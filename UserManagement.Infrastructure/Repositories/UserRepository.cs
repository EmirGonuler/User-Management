using Microsoft.EntityFrameworkCore;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Interfaces;
using UserManagement.Infrastructure.Data;

namespace UserManagement.Infrastructure.Repositories
{
    /// <summary>
    /// Handles all database operations specific to Users.
    /// Inherits standard CRUD from Repository<User> and adds
    /// User-specific queries like fetching by email or group.
    /// GetAllAsync is overridden here to eagerly load UserGroups
    /// so group names are available in API responses.
    /// </summary>
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(ApplicationDbContext context) : base(context) { }

        /// <summary>
        /// Overrides the base GetAllAsync to include group memberships.
        /// Without this, UserGroups is always an empty collection.
        /// </summary>
        public override async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _context.Users
                .Include(u => u.UserGroups)
                    .ThenInclude(ug => ug.Group)
                .ToListAsync();
        }

        /// <summary>
        /// Overrides GetByIdAsync to include group memberships for a single user.
        /// </summary>
        public override async Task<User?> GetByIdAsync(int id)
        {
            return await _context.Users
                .Include(u => u.UserGroups)
                    .ThenInclude(ug => ug.Group)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
        }

        public async Task<IEnumerable<User>> GetUsersByGroupAsync(int groupId)
        {
            return await _context.UserGroups
                .Where(ug => ug.GroupId == groupId)
                .Select(ug => ug.User)
                .ToListAsync();
        }

        public async Task<int> GetTotalUserCountAsync()
        {
            return await _context.Users
                .Where(u => u.IsActive)
                .CountAsync();
        }

        public async Task<IEnumerable<UserGroupCountDto>> GetUserCountPerGroupAsync()
        {
            return await _context.Groups
                .Select(g => new UserGroupCountDto
                {
                    GroupName = g.Name,
                    UserCount = g.UserGroups.Count(ug => ug.User.IsActive)
                })
                .ToListAsync();
        }
    }
}