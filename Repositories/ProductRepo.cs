using E_CommerceSystem.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace E_CommerceSystem.Repositories
{
    public class ProductRepo : IProductRepo
    {
        public ApplicationDbContext _context;
        public ProductRepo(ApplicationDbContext context)
        {
            _context = context;
        }

        public IEnumerable<Product> GetAllProducts()
        {
            try
            {
                return _context.Products.ToList();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Database error: {ex.Message}");
            }
        }

        public Product GetProductById(int pid)
        {
            try
            {
                return _context.Products.FirstOrDefault(p => p.PID == pid);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Database error: {ex.Message}");
            }
        }

        public void AddProduct(Product product)
        {
            try
            {
                _context.Products.Add(product);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Database error: {ex.Message}");
            }
        }

        public void UpdateProduct(Product product)
        {
            try
            {
                _context.Products.Update(product);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Database error: {ex.Message}");
            }
        }

        public Product GetProductByName( string productName)
        {
            try
            {
                return _context.Products.FirstOrDefault(p => p.ProductName == productName);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Database error: {ex.Message}");
            }
           
        }

        private IQueryable<Product> Query() => _context.Products
    .AsNoTracking()
    .Include(p => p.Category)
    .Include(p => p.Supplier);

        public IEnumerable<Product> Search(string? name, decimal? minPrice, decimal? maxPrice, int pageNumber, int pageSize)
        {
            var q = Query();
            if (!string.IsNullOrWhiteSpace(name)) q = q.Where(p => p.ProductName.Contains(name));
            if (minPrice.HasValue) q = q.Where(p => p.Price >= minPrice.Value);
            if (maxPrice.HasValue) q = q.Where(p => p.Price <= maxPrice.Value);

            return q.OrderBy(p => p.PID)
                    .Skip((Math.Max(1, pageNumber) - 1) * Math.Clamp(pageSize, 1, 200))
                    .Take(Math.Clamp(pageSize, 1, 200))
                    .ToList();
        }

    }
}
