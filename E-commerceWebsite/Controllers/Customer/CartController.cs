using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Security.Claims;
using E_commerceWebsite.Data;
using E_commerceWebsite.Data.Entities;
using E_commerceWebsite.ViewModels;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;

namespace E_commerceWebsite.Areas.Customer.Controllers
{
   
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }
        [Authorize]
        [HttpGet]
        public IActionResult Cart()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId)) // Guest User - Retrieve from Cookies
            {
                var cartCookie = Request.Cookies["Cart"];
                List<CartItem> cartItems = string.IsNullOrEmpty(cartCookie)
                    ? new List<CartItem>()
                    : JsonSerializer.Deserialize<List<CartItem>>(cartCookie);

                return View("~/Views/Customer/Cart.cshtml", cartItems);
            }

            // 1. Migrate guest cart to user cart in DB before fetching items
            var guestCartCookie = Request.Cookies["Cart"];
            if (!string.IsNullOrEmpty(guestCartCookie))
            {
                var guestCartItems = JsonSerializer.Deserialize<List<CartItem>>(guestCartCookie);

                foreach (var item in guestCartItems)
                {
                    var existingItem = _context.CartItems.FirstOrDefault(ci =>
                        ci.UserId == userId &&
                        ci.ProductId == item.ProductId &&
                        ci.SelectedColor == item.SelectedColor &&
                        ci.SelectedSize == item.SelectedSize);

                    if (existingItem != null)
                    {
                        // Merge quantities and update total price
                        existingItem.Quantity += item.Quantity;
                        existingItem.TotalPrice = existingItem.Quantity * existingItem.UnitPrice;
                    }
                    else
                    {
                        // Add guest item to user's cart
                        _context.CartItems.Add(new CartItem
                        {
                            UserId = userId,
                            ProductId = item.ProductId,
                            ProductName = item.ProductName,
                            SelectedColor = item.SelectedColor,
                            SelectedSize = item.SelectedSize,
                            Quantity = item.Quantity,
                            UnitPrice = item.UnitPrice,
                            TotalPrice = item.TotalPrice
                        });
                    }
                }

                _context.SaveChanges();

                // Remove guest cart from cookies after merging
                Response.Cookies.Delete("Cart");
            }

            // 2. Fetch updated cart AFTER migrating guest cart
            var userCartItems = _context.CartItems.Where(ci => ci.UserId == userId).ToList();

            return View("~/Views/Customer/Cart.cshtml", userCartItems);
        }

        [Authorize]
        [HttpPost]
        public IActionResult AddToCart(int productId, string selectedColor, string selectedSize, int quantity)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var product = _context.AdminProducts.FirstOrDefault(p => p.Id == productId);

            if (product == null) return NotFound("Product not found");

            var unitPrice = product.DiscountedPrice > 0 ? product.DiscountedPrice : product.Price;

            if (string.IsNullOrEmpty(userId))  // Guest User Handling - Store in Cookies
            {
                // Retrieve existing cart from cookies
                var cartCookie = Request.Cookies["Cart"];
                List<CartItem> cartItems = string.IsNullOrEmpty(cartCookie)
                    ? new List<CartItem>()
                    : JsonSerializer.Deserialize<List<CartItem>>(cartCookie);

                var existingItem = cartItems.FirstOrDefault(ci =>
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
                    cartItems.Add(new CartItem
                    {
                        ProductId = productId,
                        ProductName = product.Name,
                        SelectedColor = selectedColor,
                        SelectedSize = selectedSize,
                        Quantity = quantity,
                        UnitPrice = (decimal)unitPrice,
                        TotalPrice = quantity * (decimal)unitPrice
                    });
                }

                // Serialize cart to JSON
                var cartJson = JsonSerializer.Serialize(cartItems);

                // Store updated cart in cookies (set expiration as needed)
                Response.Cookies.Append("Cart", cartJson, new CookieOptions
                {
                    HttpOnly = true, // Prevent JavaScript access for security
                    Secure = true,   // Use HTTPS
                    Expires = DateTime.UtcNow.AddDays(7) // Set expiration
                });
            }
            else
            {
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

                // Remove cart from cookies after login and add to database
                Response.Cookies.Delete("Cart");
            }

            return Json(new { success = true, message = "Product added to cart successfully!"});
        }

        [Authorize]
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

        [Authorize]
        [HttpGet]
        public IActionResult Checkout()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account");

            var cartItems = _context.CartItems.Where(ci => ci.UserId == userId).ToList();


            return View("~/Views/Customer/Checkout.cshtml", cartItems);
        }


        [Authorize]
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
                    Status = "Pending Payment",
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
