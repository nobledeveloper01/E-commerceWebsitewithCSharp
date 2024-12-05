using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace E_commerceWebsite.Data
{
    public class AdminProduct
    {
        public int Id { get; set; } 

        [Required, MaxLength(200)]
        public string Name { get; set; } 

        [Required, MaxLength(1000)]
        public string Description { get; set; } 

        [Required, Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? DiscountedPrice { get; set; }

        [Required, MaxLength(100)]
        public string Category { get; set; }

        [Required]
        public List<string> Colors { get; set; } = new List<string>(); 

        [Required]
        public List<string> Sizes { get; set; } = new List<string>(); 

        [Required, MaxLength(500)]
        public string ImageUrl { get; set; }



        // Add this navigation property for the relationship
        public ICollection<OrderDetail> OrderDetails { get; set; }
    }
}
