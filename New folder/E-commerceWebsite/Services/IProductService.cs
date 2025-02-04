using E_commerceWebsite.ViewModels;

namespace E_commerceWebsite.Services
{
    public interface IProductService
    {
        List<ProductViewModel> GetAllProducts();
        ProductViewModel GetProductById(int id);
    }
}
