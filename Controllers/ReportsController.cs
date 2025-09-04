using E_CommerceSystem.Models;
using E_CommerceSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using System.IdentityModel.Tokens.Jwt;

namespace E_CommerceSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportsController : ControllerBase
    {
        private readonly IOrderSummaryService _svc;
        public ReportsController(IOrderSummaryService svc) => _svc = svc;

        [Authorize(Roles = "Admin,Manager")]   // OR بين الأدوار
        [HttpGet("OrderSummary")]
        public ActionResult<OrderSummaryDto> OrderSummary([FromQuery] DateTime fromUtc, [FromQuery] DateTime toUtc)
        
            => Ok(_svc.GetSummary(fromUtc, toUtc));
    }

}