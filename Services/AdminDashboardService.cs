using System;
using System.Collections.Generic;
using System.Linq;
using E_CommerceSystem.Models;
using E_CommerceSystem.Repositories;

namespace E_CommerceSystem.AdminDashboard
{
    public sealed class AdminDashboardService : IAdminDashboardService
    {
        private readonly IOrderRepo _orders;
        private readonly IOrderProductsRepo _items;
        private readonly IProductRepo _products;
        private readonly IUserRepo _users;
        private readonly IReviewRepo _reviews;

        public AdminDashboardService(
            IOrderRepo orders,
            IOrderProductsRepo items,
            IProductRepo products,
            IUserRepo users,
            IReviewRepo reviews)
            => (_orders, _items, _products, _users, _reviews) = (orders, items, products, users, reviews);

        public List<BestSellingProductDto> BestSelling(DateTime fromUtc, DateTime toUtc, int take = 10)
        {
            var ordersInRange = _orders.GetAllOrders()
                                       .Where(o => o.OrderDate >= fromUtc && o.OrderDate <= toUtc
                                                && o.Status != OrderStatus.Cancelled)
                                       .Select(o => o.OID)
                                       .ToHashSet();

            var items = _items.GetAllOrders().Where(i => ordersInRange.Contains(i.OID));
            var prods = _products.GetAllProducts().ToDictionary(p => p.PID);

            return items
                .GroupBy(i => i.PID)
                .Select(g =>
                {
                    prods.TryGetValue(g.Key, out var p);
                    var units = g.Sum(x => x.Quantity);
                    var revenue = g.Sum(x => (p?.Price ?? 0m) * x.Quantity);
                    return new BestSellingProductDto(g.Key, p?.ProductName ?? $"#{g.Key}", units, revenue);
                })
                .OrderByDescending(x => x.UnitsSold)
                .ThenByDescending(x => x.Revenue)
                .Take(Math.Max(1, take))
                .ToList();
        }

        public List<DailyRevenueDto> RevenueByDay(DateTime fromUtc, DateTime toUtc)
        {
            var orders = _orders.GetAllOrders()
                                .Where(o => o.OrderDate >= fromUtc && o.OrderDate <= toUtc
                                         && o.Status != OrderStatus.Cancelled);

            return orders
                .GroupBy(o => DateOnly.FromDateTime(o.OrderDate.Date))
                .Select(g => new DailyRevenueDto(g.Key, g.Sum(x => x.TotalAmount), g.Count()))
                .OrderBy(x => x.Day)
                .ToList();
        }

        public List<MonthlyRevenueDto> RevenueByMonth(DateTime fromUtc, DateTime toUtc)
        {
            var orders = _orders.GetAllOrders()
                                .Where(o => o.OrderDate >= fromUtc && o.OrderDate <= toUtc
                                         && o.Status != OrderStatus.Cancelled);

            return orders
                .GroupBy(o => new { o.OrderDate.Year, o.OrderDate.Month })
                .Select(g => new MonthlyRevenueDto(g.Key.Year, g.Key.Month, g.Sum(x => x.TotalAmount), g.Count()))
                .OrderBy(x => x.Year).ThenBy(x => x.Month)
                .ToList();
        }

        public List<TopRatedProductDto> TopRatedProducts(int take = 10, int minReviews = 5)
        {
            var prods = _products.GetAllProducts().ToDictionary(p => p.PID);

            return _reviews.GetAllReviews()
                           .GroupBy(r => r.PID)
                           .Select(g =>
                           {
                               prods.TryGetValue(g.Key, out var p);
                               var count = g.Count();
                               var avg = count > 0 ? g.Average(x => (double)x.Rating) : 0d;
                               return new TopRatedProductDto(g.Key, p?.ProductName ?? $"#{g.Key}",
                                                             (decimal)Math.Round(avg, 2), count);
                           })
                           .Where(x => x.ReviewsCount >= Math.Max(1, minReviews))
                           .OrderByDescending(x => x.AverageRating)
                           .ThenByDescending(x => x.ReviewsCount)
                           .Take(Math.Max(1, take))
                           .ToList();
        }

        public List<ActiveCustomerDto> MostActiveCustomers(DateTime fromUtc, DateTime toUtc, int take = 10)
        {
            var users = _users.GetAllUsers().ToDictionary(u => u.UID);

            var orders = _orders.GetAllOrders()
                                .Where(o => o.OrderDate >= fromUtc && o.OrderDate <= toUtc
                                         && o.Status != OrderStatus.Cancelled);

            return orders
                .GroupBy(o => o.UID)
                .Select(g =>
                {
                    users.TryGetValue(g.Key, out var u);
                    return new ActiveCustomerDto(
                        g.Key,
                        u?.UName ?? $"User#{g.Key}",
                        u?.Email ?? "",
                        g.Count(),
                        g.Sum(x => x.TotalAmount));
                })
                .OrderByDescending(x => x.OrdersCount)
                .ThenByDescending(x => x.TotalSpend)
                .Take(Math.Max(1, take))
                .ToList();
        }
    }
}
