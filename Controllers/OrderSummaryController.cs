using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using E_CommerceSystem.Models;
using E_CommerceSystem.Services;

namespace E_CommerceSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderSummaryController : ControllerBase
    {
        private readonly IOrderSummaryService _svc;
        public OrderSummaryController(IOrderSummaryService svc) => _svc = svc;

        [Authorize(Roles = "Admin,Manager")]
        [HttpGet("OrderSummary")]
        public ActionResult<OrderSummaryDto> OrderSummary([FromQuery] DateTime fromUtc, [FromQuery] DateTime toUtc)
            => Ok(_svc.GetSummary(fromUtc, toUtc));

       
    }
}
