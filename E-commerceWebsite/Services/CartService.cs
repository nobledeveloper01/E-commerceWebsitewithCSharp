using System.Linq;
using E_commerceWebsite.Data;
using E_commerceWebsite.Models;

namespace E_commerceWebsite.Services
{
    public class CartService
    {
        private readonly ApplicationDbContext _context;

        public CartService(ApplicationDbContext context)
        {
            _context = context;
        }

        public void AddToCart(string userId, int productId, string productName, string selectedColor, string selectedSize, int quantity, decimal unitPrice)
        {
            var existingItem = _context.CartItems
                .FirstOrDefault(ci => ci.UserId == userId && ci.ProductId == productId && ci.SelectedColor == selectedColor && ci.SelectedSize == selectedSize);

            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
                existingItem.TotalPrice = existingItem.Quantity * unitPrice;
            }
            else
            {
                _context.CartItems.Add(new CartItem
                {
                    UserId = userId,
                    ProductId = productId,
                    ProductName = productName,
                    SelectedColor = selectedColor,
                    SelectedSize = selectedSize,
                    Quantity = quantity,
                    UnitPrice = unitPrice,
                    TotalPrice = quantity * unitPrice
                });
            }

            _context.SaveChanges();
        }
    }
}
