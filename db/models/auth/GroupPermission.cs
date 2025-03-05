using System.ComponentModel.DataAnnotations;
using db.models;
using Mapster;

namespace SS.Db.models.auth
{
    [AdaptTo("[name]Dto")]
    public class GroupPermission : BaseEntity
    {
        [Key]
        public int Id { get; set; }
        
        [AdaptIgnore]
        public virtual Group Group { get; set; }
        public int GroupId { get; set; }
        
        public virtual Permission Permission { get; set; }
        public int PermissionId { get; set; }
    }
} 