using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using E_commerceWebsite.Data;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;

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

        public IActionResult Dashboard()
        {
            return View("~/Views/Admin/Index.cshtml");
        }

        public IActionResult ProductManagement()
        {
            return View("~/Views/Admin/ProductManagement.cshtml");
        }

        [HttpPost]
        public async Task<IActionResult> UploadProducts([FromForm] List<IFormFile> images, [FromForm] List<string> products)
        {
            if (images.Count != products.Count)
                return BadRequest("Each product must have one image.");

            var uploadedProducts = new List<AdminProduct>();

            for (int i = 0; i < products.Count; i++)
            {
                // Deserialize the product data
                var productData = JsonConvert.DeserializeObject<ProductUploadModel>(products[i]);

                // Ensure the image is valid
                var image = images[i];
                if (image == null || image.Length == 0)
                    return BadRequest($"Image for product {productData.Name} is invalid.");

                // Save the image
                var fileName = Path.GetFileName(image.FileName);
                var filePath = Path.Combine(_imageFolderPath, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }

                // Update the ImageUrl with the saved path
                productData.ImageUrl = $"/uploads/{fileName}";

                // Create product entity
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

            // Save to database
            _context.AdminProducts.AddRange(uploadedProducts);
            await _context.SaveChangesAsync();

            return Ok("Products uploaded successfully.");
        }

        public async Task<IActionResult> GetProductMetrics()
        {
            // Fetch the metrics from the database
            var metrics = new
            {
                MensWear = await _context.AdminProducts.CountAsync(p => p.Category == "men-Wear"),
                WomensWear = await _context.AdminProducts.CountAsync(p => p.Category == "women-wear"),
                KidsWear = await _context.AdminProducts.CountAsync(p => p.Category == "kids-wear"),
                Accessories = await _context.AdminProducts.CountAsync(p => p.Category == "accessories")
            };

            return Json(metrics);  // Return metrics as JSON
        }




    }
}
