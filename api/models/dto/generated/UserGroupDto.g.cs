using System;
using SS.Api.models.dto.generated;

namespace SS.Api.models.dto.generated
{
    public partial class UserGroupDto
    {
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public int GroupId { get; set; }
        public GroupDto Group { get; set; }
        public DateTimeOffset EffectiveDate { get; set; }
        public DateTimeOffset? ExpiryDate { get; set; }
        public string ExpiryReason { get; set; }
        public uint ConcurrencyToken { get; set; }
    }
}