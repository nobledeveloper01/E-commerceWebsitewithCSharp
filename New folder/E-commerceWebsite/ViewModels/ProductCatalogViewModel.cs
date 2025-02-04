using E_commerceWebsite.Models;
using System.Collections.Generic;

namespace E_commerceWebsite.ViewModels
{
    public class ProductCatalogViewModel
    {
        public List<ProductViewModel> Products { get; set; }

        // Filtering properties
        public string SelectedCategory { get; set; }
        public string ActiveCategory { get; set; } 

        // Pagination properties
        public int PageNumber { get; set; } = 1;
        public int TotalPages { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }

    }
}
