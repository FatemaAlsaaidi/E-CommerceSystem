using System.ComponentModel.DataAnnotations;

namespace E_CommerceSystem.Models
{
    public class SupplierDTO
    {
        [Required]
        public string Name { get; set; }
        public string? ContactInfo { get; set; }
        [Required]
        public int SupplierId { get; set; }
    }
}