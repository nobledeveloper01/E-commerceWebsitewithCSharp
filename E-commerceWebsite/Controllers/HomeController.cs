using E_commerceWebsite.Data;
using E_commerceWebsite.Models;
using E_commerceWebsite.Services;
using E_commerceWebsite.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace E_commerceWebsite.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IProductService _productService;
        private readonly ApplicationDbContext _context;

        // Fixed constructor syntax
        public HomeController(ILogger<HomeController> logger, IProductService productService, ApplicationDbContext context)
        {
            _logger = logger;
            _productService = productService;
            _context = context;
        }

        public IActionResult Index()
        {
            {
                var products = _productService.GetAllProducts();
                var model = new ProductModel { Products = products };
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult ProductDetails(int id)
        {
            var product = _productService.GetProductById(id);
            if (product == null) return NotFound();

            return View("~/Views/Home/HomeProductDetails.cshtml", product);
        }

        // Other actions remain unchanged
        public IActionResult PrivacyPolicy() => View();
        public IActionResult Contact() => View();
        public IActionResult About() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }

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

            // Logged-in User - Retrieve from Database
            var userCartItems = _context.CartItems.Where(ci => ci.UserId == userId).ToList();
            return View("~/Views/Customer/Cart.cshtml", userCartItems);
        }


        [HttpPost]
        public IActionResult AddToCart(int productId, string selectedColor, string selectedSize, int quantity)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var product = _context.AdminProducts.FirstOrDefault(p => p.Id == productId);

            if (product == null) return NotFound("Product not found");

            var unitPrice = product.DiscountedPrice > 0 ? product.DiscountedPrice : product.Price;

            if (string.IsNullOrEmpty(userId))  // Guest User Handling - Store in Cookies
            {
                _logger.LogInformation("Adding to cart as a guest.");

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
            {
                TempData["RedirectToCheckout"] = true;  // Flag to redirect after login
                return RedirectToAction("Login", "Account");
            }

            // Move session cart data to the database after login
            var cart = HttpContext.Session.GetString("Cart");
            if (!string.IsNullOrEmpty(cart))
            {
                var cartItems = JsonSerializer.Deserialize<List<CartItem>>(cart);

                foreach (var item in cartItems)
                {
                    var existingItem = _context.CartItems.FirstOrDefault(ci =>
                        ci.UserId == userId &&
                        ci.ProductId == item.ProductId &&
                        ci.SelectedColor == item.SelectedColor &&
                        ci.SelectedSize == item.SelectedSize);

                    if (existingItem != null)
                    {
                        existingItem.Quantity += item.Quantity;
                        existingItem.TotalPrice = existingItem.Quantity * item.UnitPrice;
                    }
                    else
                    {
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
                HttpContext.Session.Remove("Cart");
            }

            var userCartItems = _context.CartItems.Where(ci => ci.UserId == userId).ToList();
            return View("~/Views/Customer/Checkout.cshtml", userCartItems);
        }
    }
}