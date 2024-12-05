using E_commerceWebsite.Data;
using E_commerceWebsite.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using E_commerceWebsite.ViewModels;
using Microsoft.Extensions.Logging;
using System.Linq;
using System;
using System.Security.Claims;

namespace E_commerceWebsite.Controllers
{
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<OrderController> _logger;

        public OrderController(ApplicationDbContext context, ILogger<OrderController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        [Route("Customer/OrderDetails")]
        public IActionResult OrderDetails(string orderId)
        {
            _logger.LogInformation($"OrderDetails action called with orderId: {orderId}");

            var orderDetails = _context.OrderDetails
                .Include(od => od.Product)
                .Include(od => od.Customer)
                .Where(od => od.OrderId == orderId)
                .ToList();

            if (!orderDetails.Any())
            {
                _logger.LogInformation($"No order details found for orderId: {orderId}");
                return NotFound($"No order details found for orderId: {orderId}");
            }

            _logger.LogInformation($"Found {orderDetails.Count} order details for orderId: {orderId}");
            return View("~/Views/Customer/OrderDetails.cshtml", orderDetails);

        }



        [HttpGet]
        public IActionResult OrderHistory()
        {
            try
            {
                // Retrieve the user ID from the claims
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdClaim))
                {
                    _logger.LogWarning("User ID not found in claims.");
                    return Unauthorized("You are not authorized to view order history.");
                }

                // Convert userIdClaim to an integer
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    _logger.LogWarning($"Invalid user ID format: {userIdClaim}");
                    return BadRequest("Invalid user ID format.");
                }

                // Get all CustomerIds linked to the UserId
                var customerIds = _context.Customers
                    .Where(c => c.UserId == userId)
                    .Select(c => c.Id)
                    .ToList();

                if (!customerIds.Any())
                {
                    _logger.LogInformation($"No customers found linked to user {userId}.");
                    return View("~/Views/Customer/OrderHistory.cshtml", new List<OrderAdmin>());
                }

                // Fetch orders for all linked CustomerIds
                var customerOrders = _context.Orders
                    .Include(o => o.OrderDetails)
                    .Where(o => customerIds.Contains(o.CustomerId))
                    .ToList();

                if (!customerOrders.Any())
                {
                    _logger.LogInformation($"No orders found for user {userId}.");
                }

                return View("~/Views/Customer/OrderHistory.cshtml", customerOrders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching order history.");
                return StatusCode(500, "Internal server error");
            }
        }


        [HttpGet]
        public IActionResult OrderConfirmation(string orderId)
        {
            try
            {
                // Ensure the OrderId is a string, as per your updated model
                var orderDetails = _context.OrderDetails
                    .Include(od => od.Product)
                    .Include(od => od.Customer)
                    .Where(od => od.OrderId == orderId)  
                    .ToList();

                if (!orderDetails.Any())
                    return NotFound($"Order with ID {orderId} not found.");

                // Pass the orderId and orderDetails to the view
                var viewModel = new OrderConfirmationViewModel
                {
                    OrderId = orderId,
                    OrderDetails = orderDetails,
                    PickupOrDelivery = orderDetails.Any() ? orderDetails.First().PickupOrDelivery : "N/A"
                };

                return View("~/Views/Customer/OrderConfirmation.cshtml", viewModel);
            }
            catch (Exception ex)
            {
                // Log the error
                _logger.LogError(ex, "An error occurred while fetching order details.");
                return StatusCode(500, "Internal server error");
            }
        }

    }
}
