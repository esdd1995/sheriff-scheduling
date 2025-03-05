using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SS.Api.infrastructure.authorization;
using SS.Api.models.dto;
using SS.Api.models.dto.generated;
using SS.Api.services.usermanagement;
using SS.Db.models;
using SS.Db.models.auth;

namespace SS.Api.controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuditController : ControllerBase
    {
        public SheriffService SheriffService { get; }
        public SheriffDbContext Db { get; }
        public const string CouldNotFindSheriffError = "Couldn't find sheriff.";

        public AuditController(SheriffService sheriffService, SheriffDbContext db)
        {
            SheriffService = sheriffService;
            Db = db;
        }

        [HttpGet("roleHistory")]
        [PermissionClaimAuthorize(perm: Permission.CreateAndAssignRoles)]
        public async Task<ActionResult<List<AuditDto>>> ViewRoleHistory(Guid sheriffId)
        {
            var sheriff = await SheriffService.GetSheriff(sheriffId, null);
            if (sheriff == null) return NotFound(CouldNotFindSheriffError);
            if (!PermissionDataFiltersExtensions.HasAccessToLocation(User, Db, sheriff.HomeLocationId)) return Forbid();

            var userRoleIds = Db.UserRole.AsNoTracking().Where(ur => ur.UserId == sheriffId).Select(ur => ur.Id);
            var roleHistory = Db.Audit.AsNoTracking().Include(a => a.CreatedBy).Where(e => e.TableName == "UserRole" &&
                                                  userRoleIds.Contains(e.KeyValues.RootElement.GetProperty("Id")
                                                      .GetInt32()))
                .ToList();

            //Have to select, because we have adapt ignore on these properties. 
            return Ok(roleHistory.Select(s =>
            {
                var audit = s.Adapt<AuditDto>();
                audit.CreatedBy = s.CreatedBy.Adapt<SheriffDto>();
                audit.CreatedOn = s.CreatedOn;
                audit.CreatedById = s.CreatedById;
                return audit;
            }));
        }
        [HttpGet("groupHistory")]
        [PermissionClaimAuthorize(perm: Permission.CreateAndAssignGroups)]
        public async Task<ActionResult<List<AuditDto>>> ViewGroupHistory(Guid sheriffId)
        {
            var sheriff = await SheriffService.GetSheriff(sheriffId, null);
            if (sheriff == null) return NotFound(CouldNotFindSheriffError);
            if (!PermissionDataFiltersExtensions.HasAccessToLocation(User, Db, sheriff.HomeLocationId)) return Forbid();

            var userGroupIds = Db.UserGroup.AsNoTracking().Where(ur => ur.UserId == sheriffId).Select(ur => ur.Id);
            var groupHistory = Db.Audit.AsNoTracking().Include(a => a.CreatedBy).Where(e => e.TableName == "UserGroup" &&
                                                  userGroupIds.Contains(e.KeyValues.RootElement.GetProperty("Id")
                                                      .GetInt32()))
                .ToList();

            //Have to select, because we have adapt ignore on these properties. 
            return Ok(groupHistory.Select(s =>
            {
                var audit = s.Adapt<AuditDto>();
                audit.CreatedBy = s.CreatedBy.Adapt<SheriffDto>();
                audit.CreatedOn = s.CreatedOn;
                audit.CreatedById = s.CreatedById;
                return audit;
            }));
        }
    }
}
