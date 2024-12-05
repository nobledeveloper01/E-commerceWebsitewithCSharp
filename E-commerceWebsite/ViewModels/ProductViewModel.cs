namespace E_commerceWebsite.ViewModels
{
    public class ProductViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }
        public decimal DiscountedPrice { get; set; }
        public string ImageUrl { get; set; }
        public string Category { get; set; }
        public List<string> Images { get; set; }
        public List<string> Colors { get; set; }
        public List<string> Sizes { get; set; }
    }
}
