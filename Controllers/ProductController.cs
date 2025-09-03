using E_CommerceSystem.Models;
using E_CommerceSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using static E_CommerceSystem.Models.ProductDTO;


namespace E_CommerceSystem.Controllers
{

    [Authorize]
    [ApiController]
    [Route("api/[Controller]")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;

        public ProductController(IProductService productService, IConfiguration configuration, IMapper mapper)
        {
            _productService = productService;
            _configuration = configuration;
            _mapper = mapper;
        }

        [HttpPost("AddProduct")]
        public IActionResult AddNewProduct(ProductDTO productInput)
        {
            try
            {
                // Retrieve the Authorization header from the request
                var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

                // Decode the token to check user role
                var userRole = GetUserRoleFromToken(token);

                // Only allow Admin users to add products
                if (userRole != "admin")
                {
                    return BadRequest("You are not authorized to perform this action.");
                }

                // Check if input data is null
                if (productInput == null)
                {
                    return BadRequest("Product data is required.");
                }

                // Create a new product
                var product = new Product
                {
                    ProductName = productInput.ProductName,
                    Price = productInput.Price,
                    Description = productInput.Description,
                    Stock = productInput.Stock,
                    OverallRating = 0
                };

                // Add the new product to the database/service layer
                _productService.AddProduct(product);

                return Ok(product);
            }
            catch (Exception ex)
            {
                // Return a generic error response
                return StatusCode(500, $"An error occurred while adding the product: {ex.Message}");
            }
        }

        [HttpPut("UpdateProduct/{productId}")]
        public IActionResult UpdateProduct(int productId, ProductDTO productInput)
        {
            try
            {
                // Retrieve the Authorization header from the request
                var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

                // Decode the token to check user role
                var userRole = GetUserRoleFromToken(token);

                // Only allow Admin users to add products
                if (userRole != "admin")
                {
                    return BadRequest("You are not authorized to perform this action.");
                }

                if (productInput == null)
                    return BadRequest("Product data is required.");

                var product = _productService.GetProductById(productId);
                
                product.ProductName = productInput.ProductName;
                product.Price = productInput.Price;
                product.Description = productInput.Description;
                product.Stock = productInput.Stock;
                 
                _productService.UpdateProduct(product);

                return Ok(product);
            }
            catch (Exception ex)
            {
                // Return a generic error response
                return StatusCode(500, $"An error occurred while updte product. {(ex.Message)}");
            }
        }

       
        [AllowAnonymous]
        [HttpGet("GetAllProducts")]
        public IActionResult GetAllProducts([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10,
                                [FromQuery] string? name = null, [FromQuery] decimal? minPrice = null,
                                [FromQuery] decimal? maxPrice = null)
        {
            var products = _productService.GetAllProducts(pageNumber, pageSize, name, minPrice, maxPrice);
            var dtos = _mapper.Map<List<ProductReadDto>>(products);
            return Ok(dtos);
        }

        [AllowAnonymous]
        [HttpGet("GetProductByID")]
        public IActionResult GetProductByID(int id)
        {
            var product = _productService.GetProductById(id);
            var dto = _mapper.Map<ProductReadDto>(product);   
            return Ok(dto);
        }
        private string? GetUserRoleFromToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();

            if (handler.CanReadToken(token))
            {
                var jwtToken = handler.ReadJwtToken(token);

                // Extract the 'role' claim
                var roleClaim = jwtToken.Claims.FirstOrDefault (c => c.Type == "role" || c.Type == "unique_name" );
                

                return roleClaim?.Value; // Return the role or null if not found
            }

            throw new UnauthorizedAccessException("Invalid or unreadable token.");
        }
    }
}
