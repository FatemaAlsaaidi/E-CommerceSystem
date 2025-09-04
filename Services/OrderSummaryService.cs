using E_CommerceSystem.Models;
using E_CommerceSystem.Repositories;
namespace E_CommerceSystem.Services
{
    public class OrderSummaryService : IOrderSummaryService
    {
        private readonly IOrderRepo _orders;
        private readonly IOrderProductsRepo _orderItems;
        private readonly IProductRepo _products;
        private readonly IUserRepo _users;

        public OrderSummaryService(IOrderRepo o, IOrderProductsRepo oi, IProductRepo p, IUserRepo u)
            => (_orders, _orderItems, _products, _users) = (o, oi, p, u);

        public OrderSummaryDto GetSummary(DateTime fromUtc, DateTime toUtc)
        {
            // base data (NoTracking already ideal down in repos)
            var orders = _orders.GetAllOrders()
                                .Where(o => o.OrderDate >= fromUtc && o.OrderDate < toUtc)
                                .ToList();
            var orderIds = orders.Select(o => o.OID).ToList();
            var items = _orderItems.GetAllOrders().Where(x => orderIds.Contains(x.OID)).ToList();
            var products = _products.GetAllProducts().ToDictionary(p => p.PID);
            var users = _users.GetAllUsers().ToDictionary(u => u.UID);

            // counts by status
            int pending = orders.Count(o => o.Status == OrderStatus.Pending);
            int paid = orders.Count(o => o.Status == OrderStatus.Paid);
            int shipped = orders.Count(o => o.Status == OrderStatus.Shipped);
            int delivered = orders.Count(o => o.Status == OrderStatus.Delivered);
            int cancelled = orders.Count(o => o.Status == OrderStatus.Cancelled);

            // revenue (exclude Cancelled)
            var revenueOrders = orders.Where(o => o.Status != OrderStatus.Cancelled).ToList();
            decimal totalRevenue = revenueOrders.Sum(o => o.TotalAmount);

            // top products
            var topProducts =
                items.Where(it => orders.Any(o => o.OID == it.OID && o.Status != OrderStatus.Cancelled))
                     .GroupBy(it => it.PID)
                     .Select(g =>
                     {
                         var p = products[g.Key];
                         var units = g.Sum(x => x.Quantity);
                         var rev = units * p.Price;
                         return new TopProductDto(p.PID, p.ProductName, units, rev);
                     })
                     .OrderByDescending(x => x.Revenue)
                     .Take(10)
                     .ToList();

            // top customers by spend
            var byUser = revenueOrders.GroupBy(o => o.UID)
                .Select(g => new UserSpendDto(g.Key, users[g.Key].UName, users[g.Key].Email, g.Sum(x => x.TotalAmount)))
                .OrderByDescending(x => x.TotalSpend)
                .Take(10).ToList();

            // sales by day
            var byDay = revenueOrders
                .GroupBy(o => DateOnly.FromDateTime(o.OrderDate))
                .Select(g => new DailySalesDto(g.Key, g.Sum(x => x.TotalAmount), g.Count()))
                .OrderBy(x => x.Day)
                .ToList();

            return new OrderSummaryDto(orders.Count, pending, paid, shipped, delivered, cancelled,
                                       totalRevenue, topProducts, byUser, byDay);
        }
    }
}