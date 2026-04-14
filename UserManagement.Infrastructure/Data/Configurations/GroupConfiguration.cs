using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserManagement.Domain.Entities;

namespace UserManagement.Infrastructure.Data.Configurations
{
    /// <summary>
    /// Defines the database schema configuration for the Group entity.
    /// </summary>
    public class GroupConfiguration : IEntityTypeConfiguration<Group>
    {
        public void Configure(EntityTypeBuilder<Group> builder)
        {
            builder.ToTable("Groups");

            builder.HasKey(g => g.Id);

            builder.Property(g => g.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(g => g.Description)
                .HasMaxLength(500);

            builder.Property(g => g.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            // One Group has many Permissions
            // If the Group is deleted, its Permissions are deleted too (Cascade)
            builder.HasMany(g => g.Permissions)
                .WithOne(p => p.Group)
                .HasForeignKey(p => p.GroupId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}