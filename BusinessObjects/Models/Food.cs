using BusinessObjects.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObjects.Models
{
    public class Food
    {
        [Key]
        public int FoodId { get; set; }

        [ForeignKey("Category")]
        public int? CategoryId { get; set; }

        [Required, StringLength(150)]
        public string? Name { get; set; }

        [Required, Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        [StringLength(250)]
        public string? Description { get; set; }

        public string? ImgURL { get; set; }

        // Navigation properties
        public Category? Category { get; set; }
        public ICollection<OrderDetail>? OrderDetails { get; set; }
    }
}
