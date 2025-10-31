using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Models
{
    public partial class Category
    {
        [Key]
        public int CategoryId { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; } = null!;

        [StringLength(250)]
        public string? Description { get; set; }
        // Navigation property
        public ICollection<Dish>? Dishes { get; set; } = new List<Dish>();
    }
}
