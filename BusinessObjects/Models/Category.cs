using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Models
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; } // Khai vị, Món chính, Tráng miệng, Đồ uống, ...

        [StringLength(250)]
        public string? Description { get; set; }
        // Navigation property
        public ICollection<Food>? Foods { get; set; }
    }
}
