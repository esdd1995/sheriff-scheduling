using System.Collections.Generic;
using SS.Api.models.dto.generated;

namespace SS.Api.models.dto
{
    public class AddGroupDto
    {
        public GroupDto Group { get; set; }
        public List<int> PermissionIds { get; set; } = new List<int>();
    }
} 