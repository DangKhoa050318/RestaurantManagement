using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Models
{
    public class Table
    {
        [Key]
        public int TableId { get; set; }

        [Required, StringLength(10)]
        public string? TypeTable { get; set; } // 'VIP' | 'Normal'

        [Required]
        public int Capacity { get; set; }

        [Required, StringLength(20)]
        public string? Status { get; set; } // 'Empty', 'Booked', 'Using', 'Maintenance'

        // Navigation property
        public ICollection<Order>? Orders { get; set; }
    }
}
