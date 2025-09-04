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
    public class SupplierController : ControllerBase
    {
        private readonly ISupplierService _svc;
        public SupplierController(ISupplierService svc) => _svc = svc;

        [AllowAnonymous]
        [HttpGet("GetAllSuppliers")]
        public IActionResult GetAllSuppliers([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 100)
            => Ok(_svc.GetAllSuppliers(pageNumber, pageSize));

        [AllowAnonymous]
        [HttpGet("GetSupplierById/{sid:int}")]
        public IActionResult GetSupplier([FromRoute] int sid)
            => Ok(_svc.GetSupplierById(sid));

        [HttpPost("AddSupplier")]

        public IActionResult AddSupplier([FromBody] SupplierDTO supplierDTO)
        {
            _svc.AddSupplier(supplierDTO);
            return Ok("Supplier added successfully.");
        }

        [HttpPut("UpdateSupplier/{sid:int}")]
        public IActionResult UpdateSupplier([FromRoute] int sid, [FromBody] SupplierDTO supplierDTO)
        {
            _svc.UpdateSupplier(sid, supplierDTO);
            return Ok("Supplier updated successfully.");
        }

        [HttpDelete("DeleteSupplier/{sid:int}")]
        public IActionResult DeleteSupplier([FromRoute] int sid)
        {
            _svc.DeleteSupplier(sid);
            return Ok("Supplier deleted successfully.");
        }
    }
}
