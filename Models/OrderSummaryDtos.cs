using System.ComponentModel.DataAnnotations;

namespace E_CommerceSystem.Models
{
    
    public record OrderSummaryDto(
    int TotalOrders,
    int Pending,
    int Paid,
    int Shipped,
    int Delivered,
    int Cancelled,
    decimal TotalRevenue,                // usually exclude Cancelled
    List<TopProductDto> TopProducts,
    List<UserSpendDto> TopCustomers,
    List<DailySalesDto> SalesByDay);

        public record TopProductDto(int PID, string Name, int UnitsSold, decimal Revenue);
        public record UserSpendDto(int UID, string Name, string Email, decimal TotalSpend);
        public record DailySalesDto(DateOnly Day, decimal Revenue, int Orders);

    
}