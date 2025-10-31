using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Models
{
    public partial class Customer
    {
        [Key]
        public int CustomerId { get; set; }

        [Required, StringLength(100)]
        public string Fullname { get; set; } = null!;

        [StringLength(20)]
        public string? Phone { get; set; }

        // Navigation property
        public ICollection<Order>? Orders { get; set; } = new List<Order>();
    }
}
