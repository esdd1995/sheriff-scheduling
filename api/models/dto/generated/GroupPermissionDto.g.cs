using SS.Api.models.dto.generated;

namespace SS.Api.models.dto.generated
{
    public partial class GroupPermissionDto
    {
        public int Id { get; set; }
        public int GroupId { get; set; }
        public PermissionDto Permission { get; set; }
        public int PermissionId { get; set; }
        public uint ConcurrencyToken { get; set; }
    }
}