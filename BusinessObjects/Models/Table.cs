using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObjects.Models
{
    public partial class Table
    {
        [Key]
        public int TableId { get; set; }

        [ForeignKey("Area")]
        public int AreaId { get; set; }


        [Required, StringLength(10)]
        public string TableName { get; set; } = null!;

        [Required, StringLength(20)]
        public string Status { get; set; } = null!;
        // Navigation property
        public Area? Area { get; set; } = null!;
        public ICollection<Order>? Orders { get; set; } = new List<Order>();
    }
}
