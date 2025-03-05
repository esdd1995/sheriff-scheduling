using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using SS.Api.infrastructure.authorization;
using SS.Api.models.dto;
using SS.Api.models.dto.generated;
using SS.Api.services.usermanagement;
using SS.Db.models.auth;

namespace SS.Api.controllers.usermanagement
{
    /// <summary>
    /// This was made abstract, so it can be reused. The idea is you could take the User object and reuse with minimal changes in another project. 
    /// </summary>
    /// 
    public abstract class UserController : ControllerBase
    {
        private UserService UserService { get; }

        protected UserController(UserService userUserService)
        {
            UserService = userUserService;
        }

        [HttpPut]
        [Route("assignRoles")]
        [PermissionClaimAuthorize(perm: Permission.CreateAndAssignRoles)]
        public async Task<ActionResult> AssignRoles(List<AssignRoleDto> assignRole)
        {
            var entity = assignRole.Adapt<List<UserRole>>();
            await UserService.AssignRolesToUser(entity);
            return NoContent();
        }

        [HttpPut]
        [Route("unassignRoles")]
        [PermissionClaimAuthorize(perm: Permission.CreateAndAssignRoles)]
        public async Task<ActionResult> UnassignRoles(List<UnassignRoleDto> unassignRole)
        {
            var entity = unassignRole.Adapt<List<UserRole>>();
            await UserService.UnassignRoleFromUser(entity);
            return NoContent();
        }
        [HttpPut]
        [Route("assignGroups")]
        [PermissionClaimAuthorize(perm: Permission.CreateAndAssignGroups)]
        public async Task<ActionResult> AssignGroups(List<AssignGroupDto> assignGroup)
        {
            var entity = assignGroup.Adapt<List<UserGroup>>();
            await UserService.AssignGroupsToUser(entity);
            return NoContent();
        }

        [HttpPut]
        [Route("unassignGroups")]
        [PermissionClaimAuthorize(perm: Permission.CreateAndAssignGroups)]
        public async Task<ActionResult> UnassignGroups(List<UnassignGroupDto> unassignGroup)
        {
            var entity = unassignGroup.Adapt<List<UserGroup>>();
            await UserService.UnassignGroupFromUser(entity);
            return NoContent();
        }

        [HttpPut]
        [Route("{id}/enable")]
        [PermissionClaimAuthorize(perm: Permission.ExpireUsers)]
        public async Task<ActionResult<SheriffDto>> EnableUser(Guid id)
        {
            var user = await UserService.EnableUser(id);
            return Ok(user.Adapt<SheriffDto>());
        }

        [HttpPut]
        [Route("{id}/disable")]
        [PermissionClaimAuthorize(perm: Permission.ExpireUsers)]
        public async Task<ActionResult<SheriffDto>> DisableUser(Guid id)
        {
            var user = await UserService.DisableUser(id);
            return Ok(user.Adapt<SheriffDto>());
        }
    }
}
