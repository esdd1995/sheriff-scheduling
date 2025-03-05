using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.EntityFrameworkCore;
using SS.Api.helpers.extensions;
using SS.Api.infrastructure.exceptions;
using SS.Db.models;
using SS.Db.models.auth;

namespace SS.Api.services.usermanagement
{
    public class GroupService
    {
        private SheriffDbContext Db { get; }

        public GroupService(SheriffDbContext dbContext)
        {
            Db = dbContext;
        }

        public async Task<List<Group>> Groups() => await Db.Group.AsNoTracking().ToListAsync();

        public async Task<Group> Group(int id) =>
            await Db.Group.AsNoTracking()
                .AsSingleQuery()
                .Include(g => g.GroupPermissions)
                .ThenInclude(gp => gp.Permission)
                .SingleOrDefaultAsync(g => g.Id == id);

        public async Task<Group> AddGroup(Group group, List<int> permissionIds)
        {
            var groupExists = await Db.Group.AnyAsync(g => g.Name.ToLower() == group.Name.ToLower());
            if (groupExists)
                throw new BusinessLayerException($"{nameof(Group)} with name {group.Name} already exists.");

            using TransactionScope scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            group.GroupPermissions = null;
            group.UserGroups = null;
            await Db.Group.AddAsync(group);
            await Db.SaveChangesAsync();
            await AssignPermissionsToGroup(group.Id, permissionIds);
            await Db.SaveChangesAsync();
            scope.Complete();
            return group;
        }

        public async Task RemoveGroup(int groupId)
        {
            var group = await Db.Group.FindAsync(groupId);
            Db.Group.Remove(group);
            await Db.SaveChangesAsync();
        }

        public async Task<Group> UpdateGroup(Group group, List<int> permissionIds)
        {
            var savedGroup = await Db.Group.AsSingleQuery().Include(g => g.GroupPermissions)
                                          .FirstOrDefaultAsync(g => g.Id == group.Id);
            if (savedGroup.Name != group.Name)
            {
                var groupExists = await Db.Group.AnyAsync(g => g.Name.ToLower() == group.Name.ToLower());
                if (groupExists)
                    throw new BusinessLayerException($"{nameof(Group)} with name {group.Name} already exists.");
            }

            var permissionIdsToRemove =
                savedGroup.GroupPermissions.Select(gp => gp.PermissionId).Except(permissionIds).ToList();

            Db.Entry(savedGroup).CurrentValues.SetValues(group);

            await AssignPermissionsToGroup(group.Id, permissionIds);
            await UnassignPermissionsFromGroup(group.Id, permissionIdsToRemove);
            await Db.SaveChangesAsync();

            return savedGroup;
        }

        private async Task AssignPermissionsToGroup(int groupId, IEnumerable<int> permissionIds)
        {
            var group = await Db.Group.AsSingleQuery().Include(g => g.GroupPermissions)
                                    .ThenInclude(p => p.Permission)
                                    .FirstOrDefaultAsync(g => g.Id == groupId);
            group.ThrowBusinessExceptionIfNull($"{nameof(Group)} with id {groupId} does not exist.");

            foreach (var permissionId in permissionIds)
            {
                var permission = await Db.Permission.FindAsync(permissionId);
                if (permission == null || group.GroupPermissions.Any(gp => gp.Group.Id == groupId && gp.Permission?.Id == permissionId))
                    continue;

                group.GroupPermissions.Add(new GroupPermission
                {
                    Group = group,
                    Permission = permission
                });
            }
        }

        private async Task UnassignPermissionsFromGroup(int groupId, ICollection<int> permissionIds)
        {
            var group = await Db.Group.AsSingleQuery().Include(g => g.GroupPermissions)
                                     .FirstOrDefaultAsync(g => g.Id == groupId);
            group.ThrowBusinessExceptionIfNull($"{nameof(Group)} with id {groupId} does not exist.");

            Db.RemoveRange(group.GroupPermissions.Where(gp => permissionIds.Contains(gp.PermissionId) && gp.Group.Id == groupId));
        }
    }
}
