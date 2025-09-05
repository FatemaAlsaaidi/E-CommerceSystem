using E_CommerceSystem.Models;
using E_CommerceSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using System.IdentityModel.Tokens.Jwt;

namespace E_CommerceSystem.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[Controller]")]

    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _svc;
        public CategoryController(ICategoryService svc) => _svc = svc;

        [HttpGet("GetAllCategories")]
        public IActionResult GetAllCategories()
        {
            try
            {
                var categories = _svc.GetAllCategories(1, 100);
                return Ok(categories);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while retrieving categories. {ex.Message}");
            }

        }

        [HttpGet("GetCategoryById/{cid}")]
        public IActionResult GetCategory(int id)
        {
            try
            {
                var category = _svc.GetCategoryById(id);
                return Ok(category);
            }
            catch (KeyNotFoundException knfEx)
            {
                return NotFound(knfEx.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while retrieving the category. {ex.Message}");
            }
        }


        [Authorize(Roles = "Admin")]
        [HttpPost("AddCategory")]
        [Consumes("application/json")]

        public IActionResult AddCategory([FromBody] CategoryDTO categoryDTO)
        {
            try
            {
                if (categoryDTO == null || string.IsNullOrWhiteSpace(categoryDTO.Name))
                {
                    return BadRequest("Category data is invalid.");
                }
                _svc.AddCategory(categoryDTO);
                return Ok("Category added successfully.");
            }
            catch (InvalidOperationException invOpEx)
            {
                return Conflict(invOpEx.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while adding the category. {ex.Message}");
            }
        }


        [Authorize(Roles = "Admin")]
        [HttpPut("DeleteCategory/{cid}")]
        public IActionResult DeleteCategory(int id)
        {
            try
            {
                _svc.DeleteCategory(id);
                return Ok("Category deleted successfully.");
            }
            catch (KeyNotFoundException knfEx)
            {
                return NotFound(knfEx.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while deleting the category. {ex.Message}");
            }
        }


        [Authorize(Roles = "Admin")]
        [HttpDelete("UpdateCategory/{cid}")]
        public IActionResult UpdateCategory(int id, [FromBody] CategoryDTO categoryDTO)
        {
            try
            {
                if (categoryDTO == null || string.IsNullOrWhiteSpace(categoryDTO.Name))
                {
                    return BadRequest("Category data is invalid.");
                }
                _svc.UpdateCategory(id, categoryDTO);
                return Ok("Category updated successfully.");
            }
            catch (KeyNotFoundException knfEx)
            {
                return NotFound(knfEx.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while updating the category. {ex.Message}");
            }
        }
    }

}