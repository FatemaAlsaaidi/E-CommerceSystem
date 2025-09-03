using E_CommerceSystem.Models;
using E_CommerceSystem.Repositories;

using AutoMapper;
using AutoMapper.QueryableExtensions; // <-- needed for ProjectTo
using Microsoft.EntityFrameworkCore;  // <-- for ToListAsync

namespace E_CommerceSystem.Services
{
    public class OrderProductsService : IOrderProductsService
    {
        private readonly IOrderProductsRepo _orderProductsRepo;
        public OrderProductsService(IOrderProductsRepo orderProductsRepo)
        {
            _orderProductsRepo = orderProductsRepo;
        }

        public void AddOrderProducts(OrderProducts product)
        {
            _orderProductsRepo.AddOrderProducts(product);
        }

        public IEnumerable<OrderProducts> GetAllOrders()
        {
            return _orderProductsRepo.GetAllOrders();
        }

        public List<OrderProducts> GetOrdersByOrderId(int oid)
        {
            return _orderProductsRepo.GetOrdersByOrderId(oid);
        }
    }
}
