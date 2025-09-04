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


        [Authorize(Roles = "Admin")]
        [HttpPost("AddProduct")]
        public IActionResult AddNewProduct([FromBody] ProductDTO productInput)
        {
            if (productInput is null) return BadRequest("Product data is required.");
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var product = _mapper.Map<Product>(productInput);   
            _productService.AddProduct(product);

            var readDto = _mapper.Map<ProductReadDto>(product);
            return CreatedAtAction(nameof(GetProductByID), new { id = product.PID }, readDto);
        }

        [Authorize]
        [HttpPut("UpdateProduct/{productId:int}")]
        [Consumes("application/json")]
        public IActionResult UpdateProduct(int productId, [FromBody] ProductUpdateDto productInput)
        {
            // ModelState يُتحقق تلقائيًا مع [ApiController]، لكن نضيف حماية:
            if (productInput is null) return BadRequest("Product data is required.");

            var product = _productService.GetProductById(productId);
            if (product is null) return NotFound($"Product {productId} not found.");

            _mapper.Map(productInput, product);


            _productService.UpdateProduct(product);

           
            var readDto = _mapper.Map<ProductReadDto>(product);
            return Ok(readDto);
        }

        [AllowAnonymous]
        [HttpGet("GetAllProducts")]
        public IActionResult GetAllProducts([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 5,
                                    [FromQuery] string? name = null,
                                    [FromQuery] decimal? minPrice = null,
                                    [FromQuery] decimal? maxPrice = null)
        {
            var products = _productService.GetAllProducts(pageNumber, pageSize, name, minPrice, maxPrice);
            return Ok(_mapper.Map<List<ProductReadDto>>(products));
        }

        [AllowAnonymous]
        [HttpGet("GetProductByID")]
        public IActionResult GetProductByID(int id)
        {
            var product = _productService.GetProductById(id);
            var dto = _mapper.Map<ProductReadDto>(product);   
            return Ok(dto);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("{productId:int}/image")]
        [RequestSizeLimit(10_000_000)] // ~10 MB
        public async Task<IActionResult> UploadImage(int productId, IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest("No file uploaded.");
            var allowed = new[] { "image/jpeg", "image/png", "image/webp" };
            if (!allowed.Contains(file.ContentType)) return BadRequest("Unsupported file type.");

            var product = _productService.GetProductById(productId);
            var ext = Path.GetExtension(file.FileName);
            var fileName = $"{Guid.NewGuid():N}{ext}";
            var saveDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "products");
            Directory.CreateDirectory(saveDir);
            var savePath = Path.Combine(saveDir, fileName);

            using (var fs = new FileStream(savePath, FileMode.Create))
                await file.CopyToAsync(fs);

            var publicUrl = $"/uploads/products/{fileName}";
            product.ImageUrl = publicUrl;
            _productService.UpdateProduct(product);

            return Ok(new { imageUrl = publicUrl });
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
