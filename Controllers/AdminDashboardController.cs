using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E_CommerceSystem.AdminDashboard
{
    [Authorize(Roles = "Admin,Manager")]
    [ApiController]
    [Route("api/admin")]
    public class AdminDashboardController : ControllerBase
    {
        private readonly IAdminDashboardService _svc;

        public AdminDashboardController(IAdminDashboardService svc) => _svc = svc;

        // Helpers: default range = last 30 days when not provided
        private static (DateTime fromUtc, DateTime toUtc) RangeOrDefault(DateTime? fromUtc, DateTime? toUtc)
        {
            var to = toUtc?.ToUniversalTime() ?? DateTime.UtcNow;
            var from = fromUtc?.ToUniversalTime() ?? to.AddDays(-30);
            return (from, to);
        }

        [HttpGet("best-selling")]
        public ActionResult<List<BestSellingProductDto>> BestSelling(
            [FromQuery] DateTime? fromUtc, [FromQuery] DateTime? toUtc, [FromQuery] int take = 10)
        {
            var (from, to) = RangeOrDefault(fromUtc, toUtc);
            return Ok(_svc.BestSelling(from, to, take));
        }

        [HttpGet("revenue/day")]
        public ActionResult<List<DailyRevenueDto>> RevenueByDay(
            [FromQuery] DateTime? fromUtc, [FromQuery] DateTime? toUtc)
        {
            var (from, to) = RangeOrDefault(fromUtc, toUtc);
            return Ok(_svc.RevenueByDay(from, to));
        }

        [HttpGet("revenue/month")]
        public ActionResult<List<MonthlyRevenueDto>> RevenueByMonth(
            [FromQuery] DateTime? fromUtc, [FromQuery] DateTime? toUtc)
        {
            var (from, to) = RangeOrDefault(fromUtc, toUtc);
            return Ok(_svc.RevenueByMonth(from, to));
        }

        [HttpGet("top-rated")]
        public ActionResult<List<TopRatedProductDto>> TopRated(
            [FromQuery] int take = 10, [FromQuery] int minReviews = 5)
            => Ok(_svc.TopRatedProducts(take, minReviews));

        [HttpGet("active-customers")]
        public ActionResult<List<ActiveCustomerDto>> ActiveCustomers(
            [FromQuery] DateTime? fromUtc, [FromQuery] DateTime? toUtc, [FromQuery] int take = 10)
        {
            var (from, to) = RangeOrDefault(fromUtc, toUtc);
            return Ok(_svc.MostActiveCustomers(from, to, take));
        }
    }
}
