using System.Linq;
using System.Collections.Generic;
using E_commerceWebsite.Data;
using E_commerceWebsite.ViewModels;
using E_commerceWebsite.Services;

public class ProductService : IProductService
{
    private readonly ApplicationDbContext _context;

    public ProductService(ApplicationDbContext context)
    {
        _context = context;
    }

    public List<ProductViewModel> GetAllProducts()
    {
        return _context.AdminProducts.Select(p => new ProductViewModel
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Price = p.Price ,
            DiscountedPrice = p.DiscountedPrice ?? 0.0m, 
            Category = p.Category,
            ImageUrl = p.ImageUrl,
            Colors = p.Colors,
            Sizes = p.Sizes
        }).ToList();
    }

    public ProductViewModel GetProductById(int id)
    {
        var product = _context.AdminProducts.FirstOrDefault(p => p.Id == id);
        if (product == null) return null;

        return new ProductViewModel
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,  
            DiscountedPrice = product.DiscountedPrice ?? 0.0m,
            Category = product.Category,
            ImageUrl = product.ImageUrl,
            Colors = product.Colors,
            Sizes = product.Sizes
        };
    }

}
