using E_CommerceSystem.Models;
using E_CommerceSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;

using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using static E_CommerceSystem.Models.ReviewDTO;

namespace E_CommerceSystem.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[Controller]")]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewService _reviewService;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;


        public ReviewController(IReviewService reviewService, IConfiguration configuration, IMapper mapper)
        {
            _reviewService = reviewService;
            _configuration = configuration;
            _mapper = mapper;
        }
        [HttpPost("AddReview")]
        public IActionResult AddReview(int pid, ReviewDTO review)
        {
            try
            {
                // Retrieve the Authorization header from the request
                var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

                // Decode the token to check user role
                var userId = GetUserIdFromToken(token);

                // Extract user ID 
                int uid = int.Parse(userId);

                _reviewService.AddReview(uid, pid, review);

                return Ok("Review added successfully.");
            }
            catch (Exception ex)
            {
                // Return a generic error response
                return StatusCode(500, $"An error occurred while adding review {(ex.Message)}");
            }
        }

        [AllowAnonymous]
        [HttpGet("GetAllReviews")]
        public IActionResult GetAllReviews([FromQuery] int productId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var reviews = _reviewService.GetAllReviews(pageNumber, pageSize, productId);
            var dtos = _mapper.Map<List<ReviewReadDto>>(reviews);
            return Ok(dtos);
        }

        [HttpDelete("DeleteReview/{ReviewId}")]
        public IActionResult DeleteReview(int ReviewId)
        {

            try
            {
                var review = _reviewService.GetReviewById(ReviewId); 
                if (review == null)
                    return NotFound($"Review with ID {ReviewId} not found.");

                // Retrieve the Authorization header from the request
                var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

                // Decode the token to check user role
                var userId = GetUserIdFromToken(token);

                // Extract user ID 
                int uid = int.Parse(userId);

                if(review.UID == uid)
                {
                    _reviewService.DeleteReview(ReviewId);

                    return Ok($"Review whith ReviewId {ReviewId} Deleted successfully.");
                }
                else
                    return BadRequest("You are not authorized to delete this review.");
            }
            catch (Exception ex)
            {
                // Return a generic error response
                return StatusCode(500, $"An error occurred while deleting review. {(ex.Message)}");

            }
        }

        [HttpPut("UpdateReview/{ReviewId}")]
        public IActionResult UpdateReview(int ReviewId, ReviewDTO reviewDTO)
        {

            try
            {
                if (reviewDTO == null)
                    return BadRequest("Review data is required.");

                var review = _reviewService.GetReviewById(ReviewId);
                if (review == null)
                    return NotFound($"Review with ID {ReviewId} not found.");

                // Retrieve the Authorization header from the request
                var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

                // Decode the token to check user role
                var userId = GetUserIdFromToken(token);

                // Extract user ID 
                int uid = int.Parse(userId);

                //update review
                if(review.UID == uid)
                {
                    _reviewService.UpdateReview(ReviewId, reviewDTO);
                    return Ok($"Review whith ReviewId {ReviewId} updated successfully.");
                }
                else
                    return BadRequest("You are not authorized to update this review.");
            }
            catch (Exception ex)
            {
                // Return a generic error response
                return StatusCode(500, $"An error occurred while updted review. {(ex.Message)}");

            }
        }
        private string? GetUserIdFromToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();

            if (handler.CanReadToken(token))
            {
                var jwtToken = handler.ReadJwtToken(token);

                // Extract the 'sub' claim
                var subClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "sub");


                return (subClaim?.Value); // Return both values as a tuple
            }

            throw new UnauthorizedAccessException("Invalid or unreadable token.");
        }
    }

    
}
