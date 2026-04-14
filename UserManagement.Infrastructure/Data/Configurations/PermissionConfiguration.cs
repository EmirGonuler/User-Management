using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserManagement.Domain.Entities;

namespace UserManagement.Infrastructure.Data.Configurations
{
    /// <summary>
    /// Defines the database schema configuration for the Permission entity.
    /// </summary>
    public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
    {
        public void Configure(EntityTypeBuilder<Permission> builder)
        {
            builder.ToTable("Permissions");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(p => p.Description)
                .HasMaxLength(500);

            // Store the enum as a string in the DB (e.g. "Level1")
            // This is far more readable than storing "1" in the database
            builder.Property(p => p.Level)
                .HasConversion<string>()
                .HasMaxLength(50);
        }
    }
}
