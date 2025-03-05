using System;
using System.ComponentModel.DataAnnotations;
using db.models;
using Mapster;

namespace SS.Db.models.auth
{
    [AdaptTo("[name]Dto")]
    public class UserGroup : BaseEntity
    {
        [Key]
        public int Id { get; set; }
        public Guid UserId { get; set; }
        [AdaptIgnore]
        public virtual User User { get; set; }
        public int GroupId { get; set; }
        public virtual Group Group { get; set; }
        [Required]
        public DateTimeOffset EffectiveDate { get; set; }
        public DateTimeOffset? ExpiryDate { get; set; }
        public string ExpiryReason { get; set; }
    }
} 