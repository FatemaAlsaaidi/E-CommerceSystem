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

        [Required]
        [Range(0, int.MaxValue)]
        public int? CategoryId { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int? SupplierId { get; set; }
        public record ProductReadDto(
            int PID, string ProductName, string? Description, decimal Price, int Stock,
            int? CategoryId, string ? CategoryName, string? SupplierName,
            int? SupplierId
        );
        public record ProductCreateDto(
            string ProductName, string? Description, decimal Price, int Stock,
            int? CategoryId, int? SupplierId
        );
        public record ProductUpdateDto(
            string ProductName, string? Description, decimal Price, int Stock,
            int? CategoryId, int? SupplierId,
            byte[] RowVersion
        );

      
    }
    public class ProductQuery
    {
        public string? Name { get; set; }
        [Range(1, int.MaxValue)] public int PageNumber { get; set; } = 1;
        [Range(1, 200)] public int PageSize { get; set; } = 10;
        [Range(0, double.MaxValue)] public decimal? MinPrice { get; set; }
        [Range(0, double.MaxValue)] public decimal? MaxPrice { get; set; }


    }
}
