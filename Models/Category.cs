using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace E_CommerceSystem.Models
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        [JsonIgnore]
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}