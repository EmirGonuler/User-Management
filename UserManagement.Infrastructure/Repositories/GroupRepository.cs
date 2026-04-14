using Microsoft.EntityFrameworkCore;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Interfaces;
using UserManagement.Infrastructure.Data;

namespace UserManagement.Infrastructure.Repositories
{
    /// <summary>
    /// Handles all database operations specific to Groups.
    /// Includes eager loading of related Users and Permissions.
    /// </summary>
    public class GroupRepository : Repository<Group>, IGroupRepository
    {
        public GroupRepository(ApplicationDbContext context) : base(context) { }

        public async Task<Group?> GetGroupWithUsersAsync(int groupId)
        {
            return await _context.Groups
                .Include(g => g.UserGroups)
                    .ThenInclude(ug => ug.User)
                .FirstOrDefaultAsync(g => g.Id == groupId);
        }

        public async Task<Group?> GetGroupWithPermissionsAsync(int groupId)
        {
            return await _context.Groups
                .Include(g => g.Permissions)
                .FirstOrDefaultAsync(g => g.Id == groupId);
        }

        public async Task AddUserToGroupAsync(int userId, int groupId)
        {
            var alreadyExists = await _context.UserGroups
                .AnyAsync(ug => ug.UserId == userId && ug.GroupId == groupId);

            if (!alreadyExists)
            {
                var userGroup = new UserGroup
                {
                    UserId = userId,
                    GroupId = groupId,
                    JoinedAt = DateTime.UtcNow
                };

                await _context.UserGroups.AddAsync(userGroup);
                await _context.SaveChangesAsync();
            }
        }

        public async Task RemoveUserFromGroupAsync(int userId, int groupId)
        {
            var userGroup = await _context.UserGroups
                .FirstOrDefaultAsync(ug => ug.UserId == userId && ug.GroupId == groupId);

            if (userGroup is not null)
            {
                _context.UserGroups.Remove(userGroup);
                await _context.SaveChangesAsync();
            }
        }
    }
}
