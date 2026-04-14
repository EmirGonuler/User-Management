using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserManagement.Domain.Entities;

namespace UserManagement.Infrastructure.Data.Configurations
{
    /// <summary>
    /// Configures the UserGroup join table for the many-to-many
    /// relationship between Users and Groups.
    /// The composite primary key (UserId + GroupId) ensures a user
    /// cannot be added to the same group twice.
    /// </summary>
    public class UserGroupConfiguration : IEntityTypeConfiguration<UserGroup>
    {
        public void Configure(EntityTypeBuilder<UserGroup> builder)
        {
            builder.ToTable("UserGroups");

            // Composite primary key — combination of both foreign keys
            builder.HasKey(ug => new { ug.UserId, ug.GroupId });

            builder.Property(ug => ug.JoinedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            // UserGroup → User relationship
            builder.HasOne(ug => ug.User)
                .WithMany(u => u.UserGroups)
                .HasForeignKey(ug => ug.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // UserGroup → Group relationship
            builder.HasOne(ug => ug.Group)
                .WithMany(g => g.UserGroups)
                .HasForeignKey(ug => ug.GroupId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}