using Microsoft.AspNetCore.Mvc;
using E_commerceWebsite.Data;
using E_commerceWebsite.Data.Entities;
using System.Linq;

namespace E_commerceWebsite.Controllers.Admin
{
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrderController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Order/OrderManagement
        public IActionResult OrderManagement()
        {
            var orders = _context.Orders.ToList();
            return View("~/Views/Admin/OrderManagement.cshtml", orders);
        }

        // POST: Order/UpdateOrderStatus
        // POST: Order/UpdateOrderStatus
        [HttpPost]
        public IActionResult UpdateOrderStatus([FromBody] UpdateOrderStatusModel request)
        {
            if (string.IsNullOrWhiteSpace(request.OrderId) || string.IsNullOrWhiteSpace(request.Status))
            {
                return BadRequest(new { message = "Order ID and status are required." });
            }

            var validStatuses = new[] { "Pending", "Completed", "Cancelled" };
            if (!validStatuses.Contains(request.Status))
            {
                return BadRequest(new { message = "Invalid status value." });
            }

            var order = _context.Orders.FirstOrDefault(o => o.OrderId == request.OrderId);
            if (order == null)
            {
                return NotFound(new { message = "Order not found." });
            }

            order.Status = request.Status;
            _context.SaveChanges();

            return Ok(new { message = $"Order status updated to '{request.Status}' successfully." });
        }

        public class UpdateOrderStatusModel
        {
            public string OrderId { get; set; }
            public string Status { get; set; }
        }

        // POST: Order/FilterOrders
        [HttpPost]
        public IActionResult FilterOrders([FromBody] FilterRequestModel filter)
        {
            // Validate filter request
            if (filter == null ||
                (string.IsNullOrWhiteSpace(filter.Status) &&
                 string.IsNullOrWhiteSpace(filter.CustomerName) &&
                 !filter.StartDate.HasValue &&
                 !filter.EndDate.HasValue))
            {
                return BadRequest(new { message = "Please provide at least one filter parameter." });
            }

            // Debug incoming filter data
            Console.WriteLine($"Filter Request: Status={filter.Status}, CustomerName={filter.CustomerName}, StartDate={filter.StartDate}, EndDate={filter.EndDate}");

            try
            {
                var query = _context.Orders.AsQueryable();

                if (!string.IsNullOrWhiteSpace(filter.Status))
                {
                    query = query.Where(o => o.Status == filter.Status);
                }

                if (!string.IsNullOrWhiteSpace(filter.CustomerName))
                {
                    query = query.Where(o => o.CustomerName.Contains(filter.CustomerName));
                }

                if (filter.StartDate.HasValue)
                {
                    query = query.Where(o => o.OrderDate >= filter.StartDate.Value);
                }

                if (filter.EndDate.HasValue)
                {
                    query = query.Where(o => o.OrderDate <= filter.EndDate.Value);
                }

                var filteredOrders = query.ToList();
                return PartialView("~/Views/Admin/Partial/OrderTableBody.cshtml", filteredOrders);
            }
            catch (Exception ex)
            {
                // Log the exception for debugging
                Console.WriteLine($"Error in FilterOrders: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred while filtering orders." });
            }
        }


        public class FilterRequestModel
        {
            public string? Status { get; set; }
            public string? CustomerName { get; set; }
            public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }
        }

    }
}
