using System;
using System.Collections.Generic;

namespace E_CommerceSystem.AdminDashboard
{
    public interface IAdminDashboardService
    {
        List<BestSellingProductDto> BestSelling(DateTime fromUtc, DateTime toUtc, int take = 10);
        List<DailyRevenueDto> RevenueByDay(DateTime fromUtc, DateTime toUtc);
        List<MonthlyRevenueDto> RevenueByMonth(DateTime fromUtc, DateTime toUtc);
        List<TopRatedProductDto> TopRatedProducts(int take = 10, int minReviews = 5);
        List<ActiveCustomerDto> MostActiveCustomers(DateTime fromUtc, DateTime toUtc, int take = 10);
    }
}
