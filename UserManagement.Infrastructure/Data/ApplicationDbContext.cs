using Microsoft.EntityFrameworkCore;
using UserManagement.Domain.Entities;
using UserManagement.Infrastructure.Data.Configurations;

namespace UserManagement.Infrastructure.Data
{
    /// <summary>
    /// The main Entity Framework Core database context for the application.
    /// Acts as the bridge between your C# entities and the SQL Server database.
    /// Each DbSet represents a table in the database.
    /// Configurations are applied via IEntityTypeConfiguration classes
    /// to keep this file clean and focused.
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // --- DbSets (Tables) ---
        public DbSet<User> Users => Set<User>();
        public DbSet<Group> Groups => Set<Group>();
        public DbSet<Permission> Permissions => Set<Permission>();
        public DbSet<UserGroup> UserGroups => Set<UserGroup>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply all IEntityTypeConfiguration classes in this assembly
            // automatically — no need to call each one manually
            modelBuilder.ApplyConfigurationsFromAssembly(
                typeof(ApplicationDbContext).Assembly
            );
        }
    }
}
