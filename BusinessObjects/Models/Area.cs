using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Models
{
    public partial class Area
    {
        [Key]
        public int AreaId { get; set; }

        [StringLength(50)]
        public string AreaName { get; set; } = null!;

        [StringLength(20)]
        public string AreaStatus { get; set; } = null!;

        // Navigation property - DON'T initialize here
        public ICollection<Table>? Tables { get; set; }
    }
}