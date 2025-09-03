using E_CommerceSystem.Models;
using E_CommerceSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using System.IdentityModel.Tokens.Jwt;

namespace E_CommerceSystem.Controllers
{
    public class SupplierController : ControllerBase
    {
        private readonly ISupplierService _svc;
        public SupplierController(ISupplierService svc) => _svc = svc;

        [HttpGet("GetAllSuppliers")]
        public IActionResult GetAllSuppliers(int pageNumber = 1, int pageSize = 100)
        {
            try
            {
                var suppliers = _svc.GetAllSuppliers(pageNumber, pageSize);
                return Ok(suppliers);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while retrieving suppliers. {ex.Message}");
            }
        }
        [HttpGet("GetSupplierById/{sid}")]
        public IActionResult GetSupplier(int id)
        {
            try
            {
                var supplier = _svc.GetSupplierById(id);
                return Ok(supplier);
            }
            catch (KeyNotFoundException knfEx)
            {
                return NotFound(knfEx.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while retrieving the supplier. {ex.Message}");
            }
        }

        // Add Supplier
        public IActionResult AddSupplier([FromBody] SupplierDTO supplierDTO)
        {
            try
            {
                if (supplierDTO == null || string.IsNullOrWhiteSpace(supplierDTO.Name))
                {
                    return BadRequest("Supplier data is invalid.");
                }
                _svc.AddSupplier(supplierDTO);
                return Ok("Supplier added successfully.");
            }
            catch (InvalidOperationException invOpEx)
            {
                return Conflict(invOpEx.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while adding the supplier. {ex.Message}");
            }
        }

        // Update Supplier
        public IActionResult UpdateSupplier(int id, SupplierDTO supplierDTO)
        {
            try
            {
                if (supplierDTO == null || string.IsNullOrWhiteSpace(supplierDTO.Name))
                {
                    return BadRequest("Supplier data is invalid.");
                }
                _svc.UpdateSupplier(id, supplierDTO);
                return Ok("Supplier updated successfully.");
            }
            catch (KeyNotFoundException knfEx)
            {
                return NotFound(knfEx.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while updating the supplier. {ex.Message}");
            }
        }

        // Delete Supplier 
        public IActionResult DeleteSupplier(int id) 
        {
            try
            {
                _svc.DeleteSupplier(id);
                return Ok("Supplier deleted successfully.");
            }
            catch (KeyNotFoundException knfEx)
            {
                return NotFound(knfEx.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while deleting the supplier. {ex.Message}");
            }

        }

    }
}