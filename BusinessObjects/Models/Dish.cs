using BusinessObjects.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObjects.Models
{
    public partial class Dish
    {
        [Key]
        public int DishId { get; set; }

        [ForeignKey("Category")]
        public int? CategoryId { get; set; }

        [Required, StringLength(150)]
        public string Name { get; set; } = null!;

        [Required, Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        [StringLength(20)]
        public string? UnitOfCalculation { get; set; } = null!;

        [StringLength(250)]
        public string? Description { get; set; }

        public string? ImgUrl { get; set; }

        // Navigation properties
        public Category? Category { get; set; }
        public ICollection<OrderDetail>? OrderDetails { get; set; } = new List<OrderDetail>();
    }
}
