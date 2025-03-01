using E_commerceWebsite.Data;
using E_commerceWebsite.Services;
using E_commerceWebsite.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace E_commerceWebsite.Controllers.Customer
{
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly ApplicationDbContext _context;

        public ProductController(IProductService productService, ApplicationDbContext context)
        {
            _productService = productService;
            _context = context;
        }
        [Authorize]
        [HttpGet]
        public IActionResult Index()
        {
            return View("~/Views/Customer/Index.cshtml");
        }

        [Authorize]
        [HttpGet]
        public IActionResult ProductDetails(int id)
        {
            var product = _productService.GetProductById(id);
            if (product == null) return NotFound();

            return View("~/Views/Customer/ProductDetails.cshtml", product);
        }

        [Authorize]
        [HttpGet]
        public IActionResult ProductCatalog(string category, string searchQuery, int pageNumber = 1)
        {
            int pageSize = 12;

            var productsQuery = _context.AdminProducts.AsQueryable();

            if (!string.IsNullOrEmpty(category))
            {
                productsQuery = productsQuery.Where(p => p.Category == category);
            }

            if (!string.IsNullOrEmpty(searchQuery))
            {
                productsQuery = productsQuery.Where(p => EF.Functions.Like(p.Name, $"%{searchQuery}%") || EF.Functions.Like(p.Description, $"%{searchQuery}%"));
            }

            var totalProducts = productsQuery.Count();

            var paginatedProducts = productsQuery.Skip((pageNumber - 1) * pageSize).Take(pageSize)
                .Select(p => new ProductViewModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    DiscountedPrice = p.DiscountedPrice ?? 0m,
                    ImageUrl = p.ImageUrl,
                    Category = p.Category
                }).ToList();

            var viewModel = new ProductCatalogViewModel
            {
                Products = paginatedProducts,
                ActiveCategory = category,
                PageNumber = pageNumber,
                TotalPages = (int)Math.Ceiling(totalProducts / (double)pageSize)
            };

            ViewBag.SearchQuery = searchQuery;

            return View("~/Views/Customer/ProductCatalog.cshtml", viewModel);
        }


    }
}
