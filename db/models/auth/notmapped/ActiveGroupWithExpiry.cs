using System;
using System.ComponentModel.DataAnnotations.Schema;
using Mapster;

namespace SS.Db.models.auth.notmapped
{
    /// <summary>
    /// Needed two separate classes, so DTO generation would work correctly. 
    /// </summary>
    [AdaptTo("[name]Dto")]
    public class ActiveGroupWithExpiry
    {
        [NotMapped]
        public Group Group { get; set; }
        [NotMapped]
        public DateTimeOffset EffectiveDate { get; set; }
        [NotMapped]
        public DateTimeOffset? ExpiryDate { get; set; }
    }
}
