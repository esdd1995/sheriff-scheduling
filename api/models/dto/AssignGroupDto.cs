using System;

namespace SS.Api.models.dto
{
    public class AssignGroupDto
    {
        public Guid UserId { get; set; }
        public int GroupId { get; set; }
        public DateTimeOffset EffectiveDate { get; set; }
        public DateTimeOffset? ExpiryDate { get; set; }
    }
}
