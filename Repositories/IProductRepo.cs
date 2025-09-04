using E_CommerceSystem.Models;

namespace E_CommerceSystem.Repositories
{
    public interface IProductRepo
    {
        void AddProduct(Product product);
        IEnumerable<Product> GetAllProducts();
        Product GetProductById(int pid);
        void UpdateProduct(Product product);
        Product GetProductByName(string productName);
        IEnumerable<Product> Search(string? name, decimal? minPrice, decimal? maxPrice,
                               int pageNumber = 1, int pageSize = 10);
    }
}