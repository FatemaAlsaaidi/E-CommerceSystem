using E_CommerceSystem.Models;
using E_CommerceSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR.Protocol;
using System;
using System.IdentityModel.Tokens.Jwt;

using System.Security.Claims;

namespace E_CommerceSystem.Controllers
{
    public sealed class CancelOrderRequest { public string? Reason { get; set; } }
    public sealed class UpdateOrderStatusRequest { public OrderStatus NewStatus { get; set; } }

    [Authorize]
    [ApiController]
    [Route("api/[Controller]")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }
        //----------- --  dtos
        public sealed class CancelOrderRequest { public string? Reason { get; set; } }
        public sealed class UpdateOrderStatusRequest { public OrderStatus NewStatus { get; set; } }


        [HttpPost("PlaceOrder")]
        public async Task<IActionResult> PlaceOrder([FromBody] List<OrderItemDTO> items)
        {
            try
            {
                if (items == null || !items.Any())
                {
                    return BadRequest("Order items cannot be empty.");
                }

                // Retrieve the Authorization header from the request
                var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

                // Decode the token to check user role
                var userId = GetUserIdFromToken(token);

                // Extract user ID 
                int uid = int.Parse(userId);

                //await _orderService.PlaceOrder(items, uid);

                //return Ok("Order placed successfully.");

                var orderId = await _orderService.PlaceOrder(items, uid);
                var invoiceUrl = Url.ActionLink(
                action: nameof(DownloadInvoice),
                controller: "Order",
                values: new { orderId },
                protocol: Request.Scheme);
                                return Ok(new
                                {
                    message = "Order placed successfully.",
                    orderId,
                    invoiceUrl
               });
            }
            catch (Exception ex)
            {
                // Return a generic error response
                return StatusCode(500, $"An error occurred while placing order. {(ex.Message)}");

            }

        }




        [HttpGet("GetAllOrders")]
        public IActionResult GetAllOrders()
        {
            try
            {
                // Retrieve the Authorization header from the request
                var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

                // Decode the token to check user role
                var userId = GetUserIdFromToken(token);

                // Extract user ID 
                int uid = int.Parse(userId);

                return Ok(_orderService.GetAllOrders(uid));
            }
            catch (Exception ex)
            {
                // Return a generic error response
                return StatusCode(500, $"An error occurred while retrieving products. {(ex.Message)}");

            }
        }

        [HttpGet("GetOrderById/{OrderId}")]
        public IActionResult GetOrderById(int OrderId)
        {
            try
            {
                // Retrieve the Authorization header from the request
                var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

                // Decode the token to check user role
                var userId = GetUserIdFromToken(token);

                // Extract user ID 
                int uid = int.Parse(userId);

                return Ok(_orderService.GetOrderById(OrderId, uid));
            }
            catch (Exception ex)
            {
                // Return a generic error response
                return StatusCode(500, $"An error occurred while retrieving products. {(ex.Message)}");

            }
        }


        //------------------------

        // request bodies


        // read user id safely from claims
        private int GetUserId()
        {
            var idString = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                         ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(idString, out var uid))
                throw new UnauthorizedAccessException("Invalid or missing user id claim.");
            return uid;
        }

        // ----- ONLY ONE cancel endpoint with this route -----
        [Authorize] // owner can cancel; service re-checks ownership
        [HttpPost("{orderId:int}/cancel")]
        public async Task<IActionResult> Cancel(int orderId)
        {
            var uid = GetUserId();
            await _orderService.Cancel(orderId, uid);
            return Ok("Order cancelled.");
        }

        //// ----- ONLY ONE status endpoint with this route -----
        [Authorize(Roles = "Admin")] // or "admin" – match the exact role value in your JWT
        [HttpPost("{orderId:int}/status")]
        public IActionResult UpdateStatus(int orderId, [FromBody] UpdateOrderStatusRequest body)
        {
            var uid = GetUserId();
            _orderService.UpdateOrderStatus(orderId, body.NewStatus, uid);
            return Ok($"Order status set to {body.NewStatus}.");
        }



        //-------------------------

        [HttpGet("{orderId:int}/invoice")]
        public IActionResult DownloadInvoice(int orderId, [FromServices] IInvoiceService invoices)
        {
            var pdf = invoices.Generate(orderId);
            return File(pdf, "application/pdf", $"Invoice_{orderId}.pdf");
        }



        // Method to decode token to get user id
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
