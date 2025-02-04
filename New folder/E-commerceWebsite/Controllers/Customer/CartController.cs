using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Security.Claims;
using E_commerceWebsite.Data;
using E_commerceWebsite.Data.Entities;
using E_commerceWebsite.ViewModels;
using Newtonsoft.Json;

namespace E_commerceWebsite.Areas.Customer.Controllers
{
   
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Cart()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account");

            var cartItems = _context.CartItems.Where(ci => ci.UserId == userId).ToList();
            return View("~/Views/Customer/Cart.cshtml", cartItems);
        }

        [HttpPost]
        public IActionResult AddToCart(int productId, string selectedColor, string selectedSize, int quantity)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account");

            var product = _context.AdminProducts.FirstOrDefault(p => p.Id == productId);
            if (product == null)
                return NotFound("Product not found");

            var unitPrice = product.DiscountedPrice > 0 ? product.DiscountedPrice : product.Price;

            var existingItem = _context.CartItems.FirstOrDefault(ci =>
                ci.UserId == userId &&
                ci.ProductId == productId &&
                ci.SelectedColor == selectedColor &&
                ci.SelectedSize == selectedSize);

            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
                existingItem.TotalPrice = existingItem.Quantity * (decimal)unitPrice;
            }
            else
            {
                _context.CartItems.Add(new CartItem
                {
                    UserId = userId,
                    ProductId = productId,
                    ProductName = product.Name,
                    SelectedColor = selectedColor,
                    SelectedSize = selectedSize,
                    Quantity = quantity,
                    UnitPrice = (decimal)unitPrice,
                    TotalPrice = quantity * (decimal)unitPrice
                });
            }

            _context.SaveChanges();
            return RedirectToAction("Cart");
        }

        [HttpPost]
        public IActionResult RemoveFromCart(int cartItemId)
        {
            var cartItem = _context.CartItems.FirstOrDefault(ci => ci.Id == cartItemId);
            if (cartItem == null)
                return NotFound("Cart item not found");

            _context.CartItems.Remove(cartItem);
            _context.SaveChanges();
            return RedirectToAction("Cart");
        }

        [HttpGet]
        public IActionResult Checkout()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account");

            var cartItems = _context.CartItems.Where(ci => ci.UserId == userId).ToList();


            return View("~/Views/Customer/Checkout.cshtml", cartItems);
        }



        [HttpPost]
        public async Task<IActionResult> PlaceOrder([FromBody] OrderViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList();
                return Json(new { success = false, message = "Invalid data!", errors });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Json(new { success = false, message = "User not authenticated." });

            var cartItems = _context.CartItems.Where(ci => ci.UserId == userId).ToList();
            if (!cartItems.Any())
                return Json(new { success = false, message = "No items in cart." });

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var totalAmount = cartItems.Sum(ci => ci.TotalPrice);
                if (model.PickupOrDelivery == "delivery")
                {
                    totalAmount += 4000; // Delivery charge
                }

                var orderId = GenerateOrderId();

                // Ensure customer exists or create new
                var customer = _context.Customers.FirstOrDefault(c => c.Email == model.Email);
                if (customer == null)
                {
                    customer = new CustomerEntity
                    {
                        Name = model.Name,
                        Email = model.Email,
                        ShippingAddress = model.ShippingAddress,
                        UserId = int.Parse(userId)
                    };
                    _context.Customers.Add(customer);
                    await _context.SaveChangesAsync();
                }

                // Create order
                var order = new OrderAdmin
                {
                    OrderId = orderId,
                    CustomerName = customer.Name,
                    OrderDate = DateTime.UtcNow,
                    Status = "Pending",
                    TotalAmount = totalAmount,
                    CustomerId = customer.Id,
                    PickupOrDelivery = model.PickupOrDelivery
                };
                _context.Orders.Add(order);

                // Create order details
                foreach (var cartItem in cartItems)
                {
                    _context.OrderDetails.Add(new OrderDetail
                    {
                        OrderId = orderId,
                        ProductId = cartItem.ProductId,
                        Quantity = cartItem.Quantity,
                        UnitPrice = cartItem.UnitPrice,
                        CustomerId = customer.Id,
                        PickupOrDelivery = model.PickupOrDelivery
                    });
                }

                // Remove cart items
                _context.CartItems.RemoveRange(cartItems);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Json(new { success = true, message = "Your order has been placed successfully!", orderId });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"Error placing order: {ex.Message}");
                return Json(new { success = false, message = "Error placing order, please try again later." });
            }
        }


        private string GenerateOrderId()
        {
            var random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Range(0, 7).Select(_ => chars[random.Next(chars.Length)]).ToArray());
        }
    }
}
