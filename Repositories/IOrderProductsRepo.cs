using E_CommerceSystem.Models;

namespace E_CommerceSystem.Repositories
{
    public interface IOrderProductsRepo
    {
        void AddOrderProducts(OrderProducts product);
        IEnumerable<OrderProducts> GetAllOrders();
        IQueryable<OrderProducts> GetOrdersByOrderId(int oid);

    }
}