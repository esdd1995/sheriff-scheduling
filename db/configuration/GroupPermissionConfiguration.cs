using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SS.DB.Configuration;
using SS.Db.models.auth;

namespace SS.Db.configuration
{
    public class GroupPermissionConfiguration : BaseEntityConfiguration<GroupPermission>
    {
        public override void Configure(EntityTypeBuilder<GroupPermission> builder)
        {
            builder.Property(b => b.Id).HasIdentityOptions(startValue: 100);

            // Add index for faster lookups
            builder.HasIndex(b => new { b.GroupId, b.PermissionId }).IsUnique();

            // Configure relationships
            builder.HasOne(b => b.Group)
                .WithMany(g => g.GroupPermissions)
                .HasForeignKey(b => b.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(b => b.Permission)
                .WithMany()
                .HasForeignKey(b => b.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);

            base.Configure(builder);
        }
    }
} 