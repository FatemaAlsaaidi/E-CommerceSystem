using E_CommerceSystem.Models;

namespace E_CommerceSystem.Services
{
    public interface IOrderSummaryService
    {
        OrderSummaryDto GetSummary(DateTime fromUtc, DateTime toUtc);

       
    }
}