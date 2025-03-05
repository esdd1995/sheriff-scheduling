using System;
using System.Collections.Generic;
using SS.Api.models.dto.generated;
using SS.Db.models.sheriff;

namespace SS.Api.models.dto.generated
{
    public partial class SheriffDto
    {
        public Gender Gender { get; set; }
        public string BadgeNumber { get; set; }
        public string Rank { get; set; }
        public List<SheriffAwayLocationDto> AwayLocation { get; set; }
        public List<SheriffActingRankDto> ActingRank { get; set; }
        public List<SheriffLeaveDto> Leave { get; set; }
        public List<SheriffTrainingDto> Training { get; set; }
        public string PhotoUrl { get; set; }
        public DateTimeOffset LastPhotoUpdate { get; set; }
        public bool Excused { get; set; }
        public Guid Id { get; set; }
        public bool IsEnabled { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public int? HomeLocationId { get; set; }
        public LocationDto HomeLocation { get; set; }
        public ICollection<ActiveRoleWithExpiryDto> ActiveRoles { get; set; }
        public ICollection<RoleWithExpiryDto> Roles { get; set; }
        public ICollection<ActiveGroupWithExpiryDto> ActiveGroups { get; set; }
        public uint ConcurrencyToken { get; set; }
    }
}