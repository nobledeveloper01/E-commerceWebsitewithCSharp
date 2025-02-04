using E_commerceWebsite.Services;
using E_commerceWebsite.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;

namespace E_commerceWebsite.Controllers.Customer
{
    public class ProductController : Controller
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View("~/Views/Customer/Index.cshtml");
        }

        
        [HttpGet]
        public IActionResult ProductDetails(int id)
        {
            var product = _productService.GetProductById(id);
            if (product == null) return NotFound();

            return View("~/Views/Customer/ProductDetails.cshtml", product);
        }

        [HttpGet]
        public IActionResult ProductCatalog(string category = "men-wear", int pageNumber = 1)
        {
            const int pageSize = 9;
            var allProducts = _productService.GetAllProducts()
                                             .Where(p => string.IsNullOrEmpty(category) ||
                                                         p.Category.Equals(category, StringComparison.OrdinalIgnoreCase))
                                             .ToList();

            var paginatedProducts = allProducts.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            var viewModel = new ProductCatalogViewModel
            {
                Products = paginatedProducts,
                SelectedCategory = category,
                ActiveCategory = category,
                PageNumber = pageNumber,
                TotalPages = (int)Math.Ceiling((double)allProducts.Count() / pageSize)
            };

            return View("~/Views/Customer/ProductCatalog.cshtml", viewModel);
        }

       

       
    }
}
