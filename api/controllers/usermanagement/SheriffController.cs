using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SS.Api.helpers;
using SS.Api.helpers.extensions;
using SS.Api.infrastructure.authorization;
using SS.Api.infrastructure.exceptions;
using SS.Api.models.dto;
using SS.Api.models.dto.generated;
using SS.Api.services.scheduling;
using SS.Api.services.usermanagement;
using SS.Db.models;
using SS.Db.models.auth;
using SS.Db.models.sheriff;

namespace SS.Api.controllers.usermanagement
{
    [Route("api/[controller]")]
    [ApiController]
    public class SheriffController : UserController
    {
        public const string CouldNotFindSheriffError = "Couldn't find sheriff.";
        public const string CouldNotFindSheriffEventError = "Couldn't find sheriff event.";
        private SheriffService SheriffService { get; }
        private ShiftService ShiftService { get; }
        private DutyRosterService DutyRosterService { get; }
        private SheriffDbContext Db { get; }
        private TrainingService TrainingService { get; }

        // ReSharper disable once InconsistentNaming
        private readonly long _uploadPhotoSizeLimitKB;

        public SheriffController(SheriffService sheriffService, DutyRosterService dutyRosterService, ShiftService shiftService, UserService userUserService, TrainingService trainingService, IConfiguration configuration, SheriffDbContext db) : base(userUserService)
        {
            SheriffService = sheriffService;
            ShiftService = shiftService;
            DutyRosterService = dutyRosterService;
            TrainingService = trainingService;
            Db = db;
            _uploadPhotoSizeLimitKB = Convert.ToInt32(configuration.GetNonEmptyValue("UploadPhotoSizeLimitKB"));
        }

        #region Sheriff

        [HttpPost]
        [PermissionClaimAuthorize(perm: Permission.CreateUsers)]
        public async Task<ActionResult<SheriffDto>> AddSheriff(SheriffWithIdirDto addSheriff)
        {
            if (!PermissionDataFiltersExtensions.HasAccessToLocation(User, Db, addSheriff.HomeLocationId)) return Forbid();

            var sheriff = addSheriff.Adapt<Sheriff>();
            sheriff = await SheriffService.AddSheriff(sheriff);
            return Ok(sheriff.Adapt<SheriffDto>());
        }

        /// <summary>
        /// This gets a general list of Sheriffs. Includes Training, AwayLocation, Leave data within 7 days.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [PermissionClaimAuthorize(perm: Permission.Login)]
        public async Task<ActionResult<SheriffDto>> GetSheriffsForTeams()
        {
            var sheriffs = await SheriffService.GetFilteredSheriffsForTeams();
            return Ok(sheriffs.Adapt<List<SheriffDto>>());
        }

        /// <summary>
        /// This call includes all SheriffAwayLocation, SheriffLeave, SheriffTraining.
        /// </summary>
        /// <param name="id">Guid of the userid.</param>
        /// <returns>SheriffDto</returns>
        [HttpGet]
        [PermissionClaimAuthorize(perm: Permission.Login)]
        [Route("{id}")]
        public async Task<ActionResult<SheriffWithIdirDto>> GetSheriffForTeam(Guid id)
        {
            var sheriff = await SheriffService.GetFilteredSheriffForTeams(id);
            if (sheriff == null) return NotFound(CouldNotFindSheriffError);
            if (!PermissionDataFiltersExtensions.HasAccessToLocation(User, Db, sheriff.HomeLocationId)) return Forbid();

            var sheriffDto = sheriff.Adapt<SheriffWithIdirDto>();
            //Prevent exposing Idirs to regular users.
            sheriffDto.IdirName = User.HasPermission(Permission.EditIdir) ? sheriff.IdirName : null;
            return Ok(sheriffDto);
        }

        /// <summary>
        /// Get Sheriff Identification data.
        /// </summary>
        /// <param name="id">Guid of the userid.</param>
        /// <returns>SheriffWithIdirDto</returns>
        [HttpGet]
        [PermissionClaimAuthorize(perm: Permission.Login)]
        [Route("{id}/identification")]
        public async Task<ActionResult<SheriffWithIdirDto>> GetSheriffIdentification(Guid id)
        {
            Sheriff sheriffIdentification = await SheriffService.GetSheriffIdentification(id);
            if (sheriffIdentification == null) return NotFound(CouldNotFindSheriffError);
            if (!PermissionDataFiltersExtensions.HasAccessToLocation(User, Db, sheriffIdentification.HomeLocationId)) return Forbid();

            SheriffWithIdirDto sheriffDto = sheriffIdentification.Adapt<SheriffWithIdirDto>();
            //Prevent exposing Idirs to regular users.
            sheriffDto.IdirName = User.HasPermission(Permission.EditIdir) ? sheriffIdentification.IdirName : null;
            return Ok(sheriffDto);
        }

        /// <summary>
        /// Get  Sheriff Leaves.
        /// </summary>
        /// <param name="id">Guid of the userid.</param>
        /// <returns>SheriffLeaveDto[]</returns>
        [HttpGet]
        [PermissionClaimAuthorize(perm: Permission.Login)]
        [Route("{id}/leaves")]
        public async Task<ActionResult<List<SheriffLeaveDto>>> GetSheriffLeaves(Guid id)
        {
            List<SheriffLeave> sheriffLeave = await SheriffService.GetSheriffLeaves(id);
            return Ok(sheriffLeave.Adapt<List<SheriffLeaveDto>>());
        }

        /// <summary>
        /// Get Sheriff AwayLocations.
        /// </summary>
        /// <param name="id">Guid of the userid.</param>
        /// <returns>SheriffAwayLocationDto[]</returns>
        [HttpGet]
        [PermissionClaimAuthorize(perm: Permission.Login)]
        [Route("{id}/awaylocations")]
        public async Task<ActionResult<List<SheriffAwayLocationDto>>> GetSheriffAwayLocations(Guid id)
        {
            List<SheriffAwayLocation> sheriffAwayLocations = await SheriffService.GetSheriffAwayLocations(id);
            return Ok(sheriffAwayLocations.Adapt<List<SheriffAwayLocationDto>>());
        }

        /// <summary>
        /// Get Sheriff Ranks.
        /// </summary>
        /// <param name="id">Guid of the userid.</param>
        /// <returns>SheriffActingRankDto[]</returns>
        [HttpGet]
        [PermissionClaimAuthorize(perm: Permission.Login)]
        [Route("{id}/actingranks")]
        public async Task<ActionResult<List<SheriffActingRankDto>>> GetSheriffActingRanks(Guid id)
        {
            List<SheriffActingRank> sheriffActingRanks = await SheriffService.GetSheriffActingRanks(id);
            return Ok(sheriffActingRanks.Adapt<List<SheriffActingRankDto>>());
        }

        /// <summary>
        ///  Get Sheriff Roles.
        /// </summary>
        /// <param name="id">Guid of the userid.</param>
        /// <returns>UserRoleDto[]</returns>
        [HttpGet]
        [PermissionClaimAuthorize(perm: Permission.Login)]
        [Route("{id}/roles")]
        public async Task<ActionResult<List<UserRoleDto>>> GetSheriffRoles(Guid id)
        {
            List<UserRole> sheriffRoles = await SheriffService.GetSheriffRoles(id);
            return Ok(sheriffRoles.Adapt<List<UserRoleDto>>());
        }

        /// <summary>
        /// Get Sheriff Trainings.
        /// </summary>
        /// <param name="id">Guid of the userid.</param>
        /// <returns>SheriffTrainingDto[]</returns>
        [HttpGet]
        [PermissionClaimAuthorize(perm: Permission.Login)]
        [Route("{id}/trainings")]
        public async Task<ActionResult<List<SheriffTrainingDto>>> GetSheriffTrainings(Guid id)
        {
            List<SheriffTraining> sheriffTrainings = await SheriffService.GetSheriffTrainings(id);
            return Ok(sheriffTrainings.Adapt<List<SheriffTrainingDto>>());
        }

        /// <summary>
        /// Development route, do not use this in application.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [PermissionClaimAuthorize(perm: Permission.Login)]
        [Route("self")]
        public async Task<ActionResult<SheriffDto>> GetSelfSheriff()
        {
            var sheriff = await SheriffService.GetFilteredSheriffForTeams(User.CurrentUserId());
            if (sheriff == null) return NotFound(CouldNotFindSheriffError);
            return Ok(sheriff.Adapt<SheriffDto>());
        }

        [HttpPut]
        [PermissionClaimAuthorize(perm: Permission.EditUsers)]
        public async Task<ActionResult<SheriffDto>> UpdateSheriff(SheriffWithIdirDto updateSheriff)
        {
            await CheckForAccessToSheriffByLocation(updateSheriff.Id);

            var canEditIdir = User.HasPermission(Permission.EditIdir);
            var sheriff = updateSheriff.Adapt<Sheriff>();
            sheriff = await SheriffService.UpdateSheriff(sheriff, canEditIdir);

            return Ok(sheriff.Adapt<SheriffDto>());
        }

        [HttpPut]
        [Route("updateLocation")]
        [PermissionClaimAuthorize(perm: Permission.EditUsers)]
        public async Task<ActionResult<SheriffDto>> UpdateSheriffHomeLocation(Guid id, int locationId)
        {
            await CheckForAccessToSheriffByLocation(id);

            await SheriffService.UpdateSheriffHomeLocation(id, locationId);
            return NoContent();
        }

        [HttpGet]
        [Route("getPhoto/{id}")]
        [PermissionClaimAuthorize(perm: Permission.Login)]
        [ResponseCache(Duration = 15552000, Location = ResponseCacheLocation.Client)]
        public async Task<IActionResult> GetPhoto(Guid id) => File(await SheriffService.GetPhoto(id), "image/jpeg");

        [HttpPost]
        [Route("uploadPhoto")]
        [PermissionClaimAuthorize(perm: Permission.EditUsers)]
        [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Client)]
        public async Task<ActionResult<SheriffDto>> UploadPhoto(Guid? id, string badgeNumber, IFormFile file)
        {
            await CheckForAccessToSheriffByLocation(id, badgeNumber);

            if (file.Length == 0) return BadRequest("File length = 0");
            if (file.Length >= _uploadPhotoSizeLimitKB * 1024) return BadRequest($"File length: {file.Length / 1024} KB, Maximum upload size: {_uploadPhotoSizeLimitKB} KB");

            await using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            var fileBytes = ms.ToArray();

            if (!fileBytes.IsImage()) return BadRequest("The uploaded file was not a valid GIF/JPEG/PNG.");

            var sheriff = await SheriffService.UpdateSheriffPhoto(id, badgeNumber, fileBytes);
            return Ok(sheriff.Adapt<SheriffDto>());
        }

        [HttpPut]
        [Route("updateExcused")]
        [PermissionClaimAuthorize(perm: Permission.GenerateReports)]
        public async Task<ActionResult<SheriffDto>> UpdateExcused(Sheriff excusedSheriff)
        {
            var sheriff = await SheriffService.UpdateSheriffExcused(excusedSheriff);
            return Ok(sheriff.Adapt<SheriffDto>());
        }

        #endregion Sheriff

        #region SheriffAwayLocation

        [HttpPost]
        [Route("awayLocation")]
        [PermissionClaimAuthorize(perm: Permission.EditUsers)]
        public async Task<ActionResult<SheriffAwayLocationDto>> AddSheriffAwayLocation(SheriffAwayLocationDto sheriffAwayLocationDto, bool overrideConflicts = false)
        {
            await CheckForAccessToSheriffByLocation(sheriffAwayLocationDto.SheriffId);

            var sheriffAwayLocation = sheriffAwayLocationDto.Adapt<SheriffAwayLocation>();
            var createdSheriffAwayLocation = await SheriffService.AddSheriffAwayLocation(DutyRosterService, ShiftService, sheriffAwayLocation, overrideConflicts);
            return Ok(createdSheriffAwayLocation.Adapt<SheriffAwayLocationDto>());
        }

        [HttpPut]
        [Route("awayLocation")]
        [PermissionClaimAuthorize(perm: Permission.EditUsers)]
        public async Task<ActionResult<SheriffAwayLocationDto>> UpdateSheriffAwayLocation(SheriffAwayLocationDto sheriffAwayLocationDto, bool overrideConflicts = false)
        {
            await CheckForAccessToSheriffByLocation<SheriffAwayLocation>(sheriffAwayLocationDto.Id);

            var sheriffAwayLocation = sheriffAwayLocationDto.Adapt<SheriffAwayLocation>();
            var updatedSheriffAwayLocation = await SheriffService.UpdateSheriffAwayLocation(DutyRosterService, ShiftService, sheriffAwayLocation, overrideConflicts);
            return Ok(updatedSheriffAwayLocation.Adapt<SheriffAwayLocationDto>());
        }

        [HttpDelete]
        [Route("awayLocation")]
        [PermissionClaimAuthorize(perm: Permission.EditUsers)]
        public async Task<ActionResult> RemoveSheriffAwayLocation(int id, string expiryReason)
        {
            await CheckForAccessToSheriffByLocation<SheriffAwayLocation>(id);

            await SheriffService.RemoveSheriffAwayLocation(id, expiryReason);
            return NoContent();
        }

        #endregion SheriffAwayLocation

        #region SheriffActingRank

        [HttpPost]
        [Route("actingRank")]
        [PermissionClaimAuthorize(perm: Permission.EditUsers)]
        public async Task<ActionResult<SheriffActingRankDto>> AddSheriffActingRank(SheriffActingRankDto sheriffActingRankDto, bool overrideConflicts = false)
        {
            var sheriffActingRank = sheriffActingRankDto.Adapt<SheriffActingRank>();
            var createdSheriffActingRank = await SheriffService.AddSheriffActingRank(DutyRosterService, ShiftService, sheriffActingRank, overrideConflicts);
            return Ok(createdSheriffActingRank.Adapt<SheriffActingRankDto>());
        }

        [HttpPut]
        [Route("actingRank")]
        [PermissionClaimAuthorize(perm: Permission.EditUsers)]
        public async Task<ActionResult<SheriffActingRankDto>> UpdateSheriffActingRank(SheriffActingRankDto sheriffActingRankDto, bool overrideConflicts = false)
        {
            var sheriffActingRank = sheriffActingRankDto.Adapt<SheriffActingRank>();
            var updatedSheriffActingRank = await SheriffService.UpdateSheriffActingRank(DutyRosterService, ShiftService, sheriffActingRank, overrideConflicts);
            return Ok(updatedSheriffActingRank.Adapt<SheriffActingRankDto>());
        }

        [HttpDelete]
        [Route("actingRank")]
        [PermissionClaimAuthorize(perm: Permission.EditUsers)]
        public async Task<ActionResult> RemoveSheriffActingRank(int id, string expiryReason)
        {
            await SheriffService.RemoveSheriffActingRank(id, expiryReason);
            return NoContent();
        }

        #endregion SheriffActingRank

        #region SheriffLeave

        [HttpPost]
        [Route("leave")]
        [PermissionClaimAuthorize(perm: Permission.EditUsers)]
        public async Task<ActionResult<SheriffLeaveDto>> AddSheriffLeave(SheriffLeaveDto sheriffLeaveDto, bool overrideConflicts = false)
        {
            await CheckForAccessToSheriffByLocation(sheriffLeaveDto.SheriffId);

            var sheriffLeave = sheriffLeaveDto.Adapt<SheriffLeave>();
            var createdSheriffLeave = await SheriffService.AddSheriffLeave(DutyRosterService, ShiftService, sheriffLeave, overrideConflicts);
            return Ok(createdSheriffLeave.Adapt<SheriffLeaveDto>());
        }

        [HttpPut]
        [Route("leave")]
        [PermissionClaimAuthorize(perm: Permission.EditUsers)]
        public async Task<ActionResult<SheriffLeaveDto>> UpdateSheriffLeave(SheriffLeaveDto sheriffLeaveDto, bool overrideConflicts = false)
        {
            await CheckForAccessToSheriffByLocation<SheriffLeave>(sheriffLeaveDto.Id);

            var sheriffLeave = sheriffLeaveDto.Adapt<SheriffLeave>();
            var updatedSheriffLeave = await SheriffService.UpdateSheriffLeave(DutyRosterService, ShiftService, sheriffLeave, overrideConflicts);
            return Ok(updatedSheriffLeave.Adapt<SheriffLeaveDto>());
        }

        [HttpDelete]
        [Route("leave")]
        [PermissionClaimAuthorize(perm: Permission.EditUsers)]
        public async Task<ActionResult> RemoveSheriffLeave(int id, string expiryReason)
        {
            await CheckForAccessToSheriffByLocation<SheriffLeave>(id);

            await SheriffService.RemoveSheriffLeave(id, expiryReason);
            return NoContent();
        }

        #endregion SheriffLeave

        #region SheriffTrainingReports

        [HttpPost]
        [Route("training/reports")]
        [PermissionClaimAuthorize(perm: Permission.GenerateReports)]
        public async Task<ActionResult<TrainingReportDto>> GetSheriffsTrainingReports(TrainingReportSearchDto trainingReportSearch)
        {
            var sheriffs = await TrainingService.GetSheriffsTrainingReports(trainingReportSearch);
            return Ok(sheriffs.Adapt<List<TrainingReportDto>>());
        }

        [HttpGet]
        [Route("training/adjust-expiry")]
        [PermissionClaimAuthorize(perm: Permission.AdjustTrainingExpiry)]
        public async Task<ActionResult> TrainingExpiryAdjustment()
        {
            await TrainingService.TrainingExpiryAdjustment();
            return Ok(new { result = "success" });
        }

        #endregion SheriffTrainingReports

        #region SheriffTraining

        [HttpGet]
        [Route("training")]
        [PermissionClaimAuthorize(perm: Permission.GenerateReports)]
        public async Task<ActionResult<SheriffDto>> GetSheriffsTraining()
        {
            var sheriffs = await SheriffService.GetSheriffsTraining();
            return Ok(sheriffs.Adapt<List<SheriffDto>>());
        }

        [HttpPost]
        [Route("training")]
        [PermissionClaimAuthorize(perm: Permission.EditUsers)]
        public async Task<ActionResult<SheriffTrainingDto>> AddSheriffTraining(SheriffTrainingDto sheriffTrainingDto, bool overrideConflicts = false)
        {
            await CheckForAccessToSheriffByLocation(sheriffTrainingDto.SheriffId);

            var sheriffTraining = sheriffTrainingDto.Adapt<SheriffTraining>();
            var createdSheriffTraining = await SheriffService.AddSheriffTraining(DutyRosterService, ShiftService, sheriffTraining, overrideConflicts);
            return Ok(createdSheriffTraining.Adapt<SheriffTrainingDto>());
        }

        [HttpPut]
        [Route("training")]
        [PermissionClaimAuthorize(perm: Permission.EditUsers)]
        public async Task<ActionResult<SheriffTrainingDto>> UpdateSheriffTraining(SheriffTrainingDto sheriffTrainingDto, bool overrideConflicts = false)
        {
            await CheckForAccessToSheriffByLocation<SheriffTraining>(sheriffTrainingDto.Id);

            var sheriffTraining = sheriffTrainingDto.Adapt<SheriffTraining>();
            if (!User.HasPermission(Permission.EditPastTraining))
            {
                var savedSheriffTraining = Db.SheriffTraining.AsNoTracking().FirstOrDefault(st => st.Id == sheriffTrainingDto.Id);
                if (savedSheriffTraining?.EndDate <= DateTimeOffset.UtcNow)
                    throw new BusinessLayerException("No permission to edit training that has completed.");
            }

            var updatedSheriffTraining = await SheriffService.UpdateSheriffTraining(DutyRosterService, ShiftService, sheriffTraining, overrideConflicts);
            return Ok(updatedSheriffTraining.Adapt<SheriffTrainingDto>());
        }

        [HttpDelete]
        [Route("training")]
        [PermissionClaimAuthorize(perm: Permission.EditUsers)]
        public async Task<ActionResult> RemoveSheriffTraining(int id, string expiryReason)
        {
            await CheckForAccessToSheriffByLocation<SheriffTraining>(id);

            if (!User.HasPermission(Permission.RemovePastTraining))
            {
                var sheriffTraining = Db.SheriffTraining.AsNoTracking().FirstOrDefault(st => st.Id == id);
                if (sheriffTraining?.EndDate <= DateTimeOffset.UtcNow)
                    throw new BusinessLayerException("No permission to remove training that has completed.");
            }

            await SheriffService.RemoveSheriffTraining(id, expiryReason);
            return NoContent();
        }

        #endregion SheriffTraining

        #region Access Helpers

        private async Task CheckForAccessToSheriffByLocation(Guid? id, string badgeNumber = null)
        {
            var savedSheriff = await SheriffService.GetSheriff(id, badgeNumber);
            if (savedSheriff == null) throw new NotFoundException(CouldNotFindSheriffError);
            if (!PermissionDataFiltersExtensions.HasAccessToLocation(User, Db, savedSheriff.HomeLocationId)) throw new NotAuthorizedException();
        }

        private async Task CheckForAccessToSheriffByLocation<T>(int id) where T : SheriffEvent
        {
            var sheriffEvent = await SheriffService.GetSheriffEvent<T>(id);
            if (sheriffEvent == null) throw new NotFoundException(CouldNotFindSheriffEventError);
            var savedSheriff = await SheriffService.GetSheriff(sheriffEvent.SheriffId, null);
            if (savedSheriff == null) throw new NotFoundException(CouldNotFindSheriffError);
            if (!PermissionDataFiltersExtensions.HasAccessToLocation(User, Db, savedSheriff.HomeLocationId)) throw new NotAuthorizedException();
        }

        #endregion Access Helpers
    }
}