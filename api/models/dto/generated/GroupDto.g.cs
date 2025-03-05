using System.Collections.Generic;
using SS.Api.models.dto.generated;

namespace SS.Api.models.dto.generated
{
    public partial class GroupDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public ICollection<GroupPermissionDto> GroupPermissions { get; set; }
        public uint ConcurrencyToken { get; set; }
    }
}