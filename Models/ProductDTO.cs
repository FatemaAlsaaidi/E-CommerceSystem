using System.ComponentModel.DataAnnotations;

namespace E_CommerceSystem.Models
{
    public class ProductDTO
    {
        [Required]
        public string ProductName { get; set; }

        public string Description { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public decimal Price { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int Stock { get; set; }

        public record ProductReadDto(
            int PID, string ProductName, string? Description, decimal Price, int Stock,
            int? CategoryId, string? CategoryName,
            int? SupplierId, string? SupplierName
        );
        public record ProductCreateDto(
            string ProductName, string? Description, decimal Price, int Stock,
            int? CategoryId, int? SupplierId
        );
        public record ProductUpdateDto(
            string ProductName, string? Description, decimal Price, int Stock,
            int? CategoryId, int? SupplierId
        );
    }
}
