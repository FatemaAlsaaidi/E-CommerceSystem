using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace E_CommerceSystem.Models

{
    public class Product
    {
        [Key]
        public int PID { get; set; }

        [Required]
        public string ProductName { get; set; }

        public string Description { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public decimal Price { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int Stock {  get; set; }

        public decimal OverallRating { get; set; }

        [JsonIgnore]
        public virtual ICollection<OrderProducts> OrderProducts { get;set; }

        [JsonIgnore]
        public virtual ICollection<Review> Reviews { get; set; } // use virtual for lazy loading

        [ForeignKey("Category")]
        public int? CategoryId { get; set; }
        public virtual Category? Category { get; set; } // use virtual for lazy loading

        [ForeignKey("Supplier")]
        public int? SupplierId { get; set; }
        public virtual Supplier? Supplier { get; set; } // use virtual for lazy loading
    }
}
