using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SS.DB.Configuration;
using SS.Db.models.auth;

namespace SS.Db.configuration
{
    public class UserGroupConfiguration : BaseEntityConfiguration<UserGroup>
    {
        public override void Configure(EntityTypeBuilder<UserGroup> builder)
        {
            builder.Property(b => b.Id).HasIdentityOptions(startValue: 5000);

            // Add index for faster lookups and prevent duplicate assignments
            builder.HasIndex(b => new { b.UserId, b.GroupId }).IsUnique();

            // Configure relationships
            builder.HasOne(b => b.User)
                .WithMany()
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(b => b.Group)
                .WithMany(g => g.UserGroups)
                .HasForeignKey(b => b.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            base.Configure(builder);
        }
    }
} 