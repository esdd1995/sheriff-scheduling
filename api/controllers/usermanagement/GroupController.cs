using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using SS.Api.helpers.extensions;
using SS.Api.infrastructure.authorization;
using SS.Api.infrastructure.exceptions;
using SS.Api.models.dto;
using SS.Api.models.dto.generated;
using SS.Api.services.usermanagement;
using SS.Db.models.auth;

namespace SS.Api.controllers.usermanagement
{
    /// <summary>
    /// Used to manage groups.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class GroupController : ControllerBase
    {
        private GroupService GroupService { get; }
        public GroupController(GroupService groupService) { GroupService = groupService; }

        [HttpGet]
        [PermissionClaimAuthorize(perm: Permission.ViewGroups)]
        public async Task<ActionResult<List<GroupDto>>> Groups()
        {
            var groups = await GroupService.Groups();
            return Ok(groups.Adapt<List<GroupDto>>());
        }

        [HttpGet]
        [Route("{id}")]
        [PermissionClaimAuthorize(perm: Permission.ViewGroups)]
        public async Task<ActionResult<GroupDto>> GetGroup(int id)
        {
            var group = await GroupService.Group(id);
            if (group == null) return NotFound();
            return Ok(group.Adapt<GroupDto>());
        }

        [HttpPost]
        [PermissionClaimAuthorize(perm: Permission.CreateAndAssignGroups)]
        public async Task<ActionResult<GroupDto>> AddGroup(AddGroupDto addGroup)
        {
            addGroup.ThrowBusinessExceptionIfNull("AddGroup was null");
            addGroup.Group.ThrowBusinessExceptionIfNull("Group was null");
            if (!addGroup.PermissionIds.Any()) throw new BusinessLayerException("Permission Ids was empty");

            var entity = addGroup.Group.Adapt<Group>();
            var createdGroup = await GroupService.AddGroup(entity, addGroup.PermissionIds);
            return Ok(createdGroup.Adapt<GroupDto>());
        }

        [HttpPut]
        [PermissionClaimAuthorize(perm: Permission.EditGroups)]
        public async Task<ActionResult<GroupDto>> UpdateGroup(UpdateGroupDto updateGroup)
        {
            updateGroup.ThrowBusinessExceptionIfNull("UpdateGroup was null");
            updateGroup.Group.ThrowBusinessExceptionIfNull("Group was null");

            var entity = updateGroup.Group.Adapt<Group>();
            var updatedGroup = await GroupService.UpdateGroup(entity, updateGroup.PermissionIds);
            return Ok(updatedGroup.Adapt<GroupDto>());
        }

        [HttpDelete]
        [PermissionClaimAuthorize(perm: Permission.EditGroups)]
        public async Task<ActionResult> RemoveGroup(int id)
        {
            await GroupService.RemoveGroup(id);
            return NoContent();
        }
    }
}