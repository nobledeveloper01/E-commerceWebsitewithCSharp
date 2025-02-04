using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using E_commerceWebsite.Data;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System;
using Azure;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace E_commerceWebsite.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/[controller]/[action]")]
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly string _imageFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

        public ProductController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult ProductManagement()
        {
            return View("~/Views/Admin/ProductManagement.cshtml");
        }

        [HttpPost]
        public async Task<IActionResult> UploadProducts([FromForm] List<IFormFile> images, [FromForm] List<string> products)
        {
            if (images == null || products == null || images.Count != products.Count)
                return BadRequest("Each product must have one associated image.");

            var uploadedProducts = new List<AdminProduct>();

            for (int i = 0; i < products.Count; i++)
            {
                try
                {
                    var productData = JsonConvert.DeserializeObject<ProductUploadModel>(products[i]);
                    if (productData == null)
                        return BadRequest($"Invalid product data at index {i}.");

                    var image = images[i];
                    if (image == null || image.Length == 0)
                        return BadRequest($"Image for product {productData.Name} is invalid.");

                    // Ensure the uploads folder exists
                    if (!Directory.Exists(_imageFolderPath))
                    {
                        Directory.CreateDirectory(_imageFolderPath);
                    }

                    // Save the image
                    var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(image.FileName)}";
                    var filePath = Path.Combine(_imageFolderPath, fileName);
                    await using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await image.CopyToAsync(stream);
                    }

                    productData.ImageUrl = $"/uploads/{fileName}";

                    var newProduct = new AdminProduct
                    {
                        Name = productData.Name,
                        Description = productData.Description,
                        Price = productData.Price,
                        DiscountedPrice = productData.DiscountedPrice,
                        Category = productData.Category,
                        Colors = productData.Colors?.Split(',').ToList(),
                        Sizes = productData.Sizes?.Split(',').ToList(),
                        ImageUrl = productData.ImageUrl,
                    };

                    uploadedProducts.Add(newProduct);
                }
                catch (Exception ex)
                {
                    return StatusCode(500, $"Error processing product at index {i}: {ex.Message}");
                }
            }

            await _context.AdminProducts.AddRangeAsync(uploadedProducts);
            await _context.SaveChangesAsync();

            return Ok("Products uploaded successfully.");
        }

        
        public async Task<IActionResult> GetProductMetrics()
        {
            var metrics = new
            {
                MensWear = await _context.AdminProducts.CountAsync(p => p.Category.ToLower() == "men-wear"),
                WomensWear = await _context.AdminProducts.CountAsync(p => p.Category.ToLower() == "women-wear"),
                KidsWear = await _context.AdminProducts.CountAsync(p => p.Category.ToLower() == "kids-wear"),
                Accessories = await _context.AdminProducts.CountAsync(p => p.Category.ToLower() == "accessories")
            };

            return Json(metrics);
        }


        public async Task<IActionResult> GetAllProducts(
    int page = 1,
    int pageSize = 10,
    string searchTerm = "",
    string category = "",
    decimal? minPrice = null,
    decimal? maxPrice = null)
        {
            try
            {
                var query = _context.AdminProducts.AsQueryable();

                // Apply filters
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(p => p.Name.Contains(searchTerm));
                }

                if (!string.IsNullOrEmpty(category))
                {
                    query = query.Where(p => p.Category == category);
                }

                if (minPrice.HasValue)
                {
                    query = query.Where(p => p.Price >= minPrice.Value);
                }

                if (maxPrice.HasValue)
                {
                    query = query.Where(p => p.Price <= maxPrice.Value);
                }

                var totalCount = await query.CountAsync();
                var items = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return Ok(new { items, totalCount });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while fetching products: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.AdminProducts.FindAsync(id);
            if (product == null)
                return NotFound(new { message = "Product not found" });

            _context.AdminProducts.Remove(product);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Product deleted successfully" });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductById(int id)
        {
            var product = await _context.AdminProducts.FindAsync(id);
            return product != null ? Ok(product) : NotFound(new { message = "Product not found" });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> EditProduct(int id, [FromForm] ProductUpdateModel model)
        {
            if (model == null)
                return BadRequest(new { message = "Invalid product data." });

            var product = await _context.AdminProducts.FindAsync(id);
            if (product == null)
                return NotFound(new { message = "Product not found" });

            product.Name = model.Name;
            product.Description = model.Description;
            product.Price = model.Price;
            product.DiscountedPrice = model.DiscountedPrice;
            product.Category = model.Category;
            product.Colors = model.Colors?.Split(',').ToList();
            product.Sizes = model.Sizes?.Split(',').ToList();

            if (model.Image != null && model.Image.Length > 0)
            {
                if (!Directory.Exists(_imageFolderPath))
                {
                    Directory.CreateDirectory(_imageFolderPath);
                }

                var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(model.Image.FileName)}";
                var filePath = Path.Combine(_imageFolderPath, fileName);
                await using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.Image.CopyToAsync(stream);
                }

                product.ImageUrl = $"/uploads/{fileName}";
            }

            _context.AdminProducts.Update(product);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Product updated successfully" });
        }
    }

    public class ProductUpdateModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public decimal? DiscountedPrice { get; set; }
        public string Category { get; set; }
        public string Colors { get; set; }
        public string Sizes { get; set; }
        public IFormFile Image { get; set; }
    }
}
