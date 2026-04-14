using Microsoft.EntityFrameworkCore;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Enums;
using UserManagement.Infrastructure.Data;

namespace UserManagement.Infrastructure.Seeds
{
    /// <summary>
    /// Seeds the database with initial sample data on first run.
    /// This gives developers a working dataset immediately after
    /// cloning and running migrations — no manual data entry needed.
    /// Only seeds if the tables are empty to prevent duplicate data.
    /// </summary>
    public static class DatabaseSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            try
            {
                // Apply any pending migrations automatically on startup
                await context.Database.MigrateAsync();

                await SeedGroupsAndPermissionsAsync(context);
                await SeedUsersAsync(context);
                await SeedUserGroupsAsync(context);
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while seeding the database: {ex.Message}", ex);
            }
        }

        private static async Task SeedGroupsAndPermissionsAsync(ApplicationDbContext context)
        {
            // Only seed if no groups exist
            if (await context.Groups.AnyAsync()) return;

            var groups = new List<Group>
            {
                new Group
                {
                    Name = "Administrators",
                    Description = "Full system access",
                    Permissions = new List<Permission>
                    {
                        new Permission { Name = "Full Access",   Description = "All system permissions", Level = PermissionLevel.Level5 },
                        new Permission { Name = "User Manager",  Description = "Manage all users",       Level = PermissionLevel.Level4 }
                    }
                },
                new Group
                {
                    Name = "Managers",
                    Description = "Department management access",
                    Permissions = new List<Permission>
                    {
                        new Permission { Name = "Report Access", Description = "View all reports",       Level = PermissionLevel.Level3 },
                        new Permission { Name = "Team Manager",  Description = "Manage team members",    Level = PermissionLevel.Level2 }
                    }
                },
                new Group
                {
                    Name = "Standard Users",
                    Description = "Basic read access",
                    Permissions = new List<Permission>
                    {
                        new Permission { Name = "Read Only",     Description = "View own profile only",  Level = PermissionLevel.Level1 }
                    }
                }
            };

            await context.Groups.AddRangeAsync(groups);
            await context.SaveChangesAsync();
        }

        private static async Task SeedUsersAsync(ApplicationDbContext context)
        {
            if (await context.Users.AnyAsync()) return;

            var users = new List<User>
            {
                new User { FirstName = "Alice",  LastName = "Smith",   Email = "alice@example.com",  IsActive = true  },
                new User { FirstName = "Bob",    LastName = "Johnson", Email = "bob@example.com",    IsActive = true  },
                new User { FirstName = "Carol",  LastName = "White",   Email = "carol@example.com",  IsActive = true  },
                new User { FirstName = "David",  LastName = "Brown",   Email = "david@example.com",  IsActive = false }
            };

            await context.Users.AddRangeAsync(users);
            await context.SaveChangesAsync();
        }

        private static async Task SeedUserGroupsAsync(ApplicationDbContext context)
        {
            if (await context.UserGroups.AnyAsync()) return;

            var alice = await context.Users.FirstAsync(u => u.Email == "alice@example.com");
            var bob = await context.Users.FirstAsync(u => u.Email == "bob@example.com");
            var carol = await context.Users.FirstAsync(u => u.Email == "carol@example.com");

            var admins = await context.Groups.FirstAsync(g => g.Name == "Administrators");
            var managers = await context.Groups.FirstAsync(g => g.Name == "Managers");
            var standard = await context.Groups.FirstAsync(g => g.Name == "Standard Users");

            var userGroups = new List<UserGroup>
            {
                // Alice is an Admin AND a Manager (demonstrates many-to-many)
                new UserGroup { UserId = alice.Id, GroupId = admins.Id   },
                new UserGroup { UserId = alice.Id, GroupId = managers.Id },

                // Bob is a Manager
                new UserGroup { UserId = bob.Id,   GroupId = managers.Id },

                // Carol is a Standard User
                new UserGroup { UserId = carol.Id, GroupId = standard.Id }
            };

            await context.UserGroups.AddRangeAsync(userGroups);
            await context.SaveChangesAsync();
        }
    }
}
