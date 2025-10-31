using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Models
{
    public class Area
    {
        [Key]
        public int AreaId { get; set; }

        [StringLength(100)]
        public string AreaName { get; set; }

        [StringLength(50)]
        public string AreaStatus { get; set; }

        // Navigation property
        public ICollection<Table>? Tables { get; set; }
    }
}