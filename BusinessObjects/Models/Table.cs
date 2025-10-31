using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObjects.Models
{
    public class Table
    {
        [Key]
        public int TableId { get; set; }

        [ForeignKey("Area")]
        public int AreaId { get; set; }


        [Required, StringLength(10)]
        public string TableName { get; set; }

        [Required, StringLength(20)]
        public string Status { get; set; }
        // Navigation property
        public Area? Area { get; set; }
        public ICollection<Order>? Orders { get; set; }
    }
}
