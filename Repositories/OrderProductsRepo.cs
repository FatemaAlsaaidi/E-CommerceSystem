using E_CommerceSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace E_CommerceSystem.Repositories
{
    public class OrderProductsRepo : IOrderProductsRepo
    {
        private readonly ApplicationDbContext _context;

        public OrderProductsRepo(ApplicationDbContext context)
        {
            _context = context;
        }

        public void AddOrderProducts(OrderProducts product)
        {
            try
            {
                _context.OrderProducts.Add(product);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Database error: {ex.Message}");
            }
        }

        public IEnumerable<OrderProducts> GetAllOrders()
        {
            try
            {
                return _context.OrderProducts.AsNoTracking(); // no Include needed when using ProjectTo
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Database error: {ex.Message}");
            }
        }
        public IQueryable<OrderProducts> GetOrdersByOrderId(int oid)
        {
            try
            {
                return _context.OrderProducts.AsNoTracking(); // no Include needed when using ProjectTo
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Database error: {ex.Message}");
            }
        }
    }
}
