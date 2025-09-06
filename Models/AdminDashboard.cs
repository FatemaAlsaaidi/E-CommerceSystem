using System;

namespace E_CommerceSystem.AdminDashboard
{
    public record BestSellingProductDto(int PID, string Name, int UnitsSold, decimal Revenue);

    public record DailyRevenueDto(DateOnly Day, decimal Revenue, int Orders);

    public record MonthlyRevenueDto(int Year, int Month, decimal Revenue, int Orders);

    public record TopRatedProductDto(int PID, string Name, decimal AverageRating, int ReviewsCount);

    public record ActiveCustomerDto(int UID, string Name, string Email, int OrdersCount, decimal TotalSpend);
}
