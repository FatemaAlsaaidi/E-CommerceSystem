using E_CommerceSystem.Models;

namespace E_CommerceSystem.Services
{
    public interface IOrderService
    {
        List<OrderProducts> GetAllOrders(int uid);
        IEnumerable<OrdersOutputOTD> GetOrderById(int oid, int uid);
        IEnumerable<Order> GetOrderByUserId(int uid);
        void DeleteOrder(int oid);

        void AddOrder(Order order);
        void UpdateOrder(Order order);
        Task<int> PlaceOrder(List<OrderItemDTO> items, int uid);
       Task Cancel(int oid, int uid);

        void UpdateOrderStatus(int oid, OrderStatus newStatus, int requesterUid);




    }
}