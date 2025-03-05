using System;

namespace SS.Api.models.dto
{
    public class UnassignGroupDto
    {
        public Guid UserId { get; set; }
        public int GroupId { get; set; }
        public string ExpiryReason { get; set; }
    }
}
