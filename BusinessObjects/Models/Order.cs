using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObjects.Models
{
    public partial class Order
    {
        [Key]
        public int OrderId { get; set; }

        [ForeignKey("Table")]
        public int? TableId { get; set; }

        [ForeignKey("Customer")]
        public int? CustomerId { get; set; }

        [Required]
        public DateTime OrderTime { get; set; } = DateTime.Now;

        /// <summary>
        /// Thời gian thanh toán (NULL nếu chưa thanh toán)
        /// </summary>
        public DateTime? PaymentTime { get; set; }

        [Required, StringLength(15)]
        public string Status { get; set; } = null!;

        [Required, Column(TypeName = "decimal(10,2)")]
        public decimal TotalAmount { get; set; } = 0;

        // Navigation properties
        public Table? Table { get; set; }
        public Customer? Customer { get; set; }
        public ICollection<OrderDetail>? OrderDetails { get; set; } = new List<OrderDetail>();
    }
}
