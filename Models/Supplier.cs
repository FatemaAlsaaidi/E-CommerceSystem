using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace E_CommerceSystem.Models
{
    public class Supplier
    {
        [Key]
        public int SupplierId { get; set; }
        public string Name { get; set; } = default!;
        public string? ContactEmail { get; set; } // ? means this attribute accept null value
        public string? Phone { get; set; }
        [JsonIgnore]
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }

}
