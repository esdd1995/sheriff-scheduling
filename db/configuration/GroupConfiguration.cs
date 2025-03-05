using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SS.DB.Configuration;
using SS.Db.models.auth;

namespace SS.Db.configuration
{
    public class GroupConfiguration : BaseEntityConfiguration<Group>
    {
        public override void Configure(EntityTypeBuilder<Group> builder)
        {
            builder.Property(b => b.Id).HasIdentityOptions(startValue: 50);
            
            // Seed initial groups
            builder.HasData(
                new Group 
                { 
                    CreatedById = User.SystemUser, 
                    Id = 1, 
                    Name = "Training Section",
                    Description = "Training Section Team",
                },
                new Group 
                { 
                    CreatedById = User.SystemUser, 
                    Id = 2, 
                    Name = "Central Programs",
                    Description = "Central Programs Team",
                },
                new Group 
                { 
                    CreatedById = User.SystemUser, 
                    Id = 3, 
                    Name = "Sheriff Provincial Support",
                    Description = "Sheriff Provincial Support Team",
                }
            );

            base.Configure(builder);
        }
    }
} 