
namespace E_commerceWebsite.Data.Entities
{

    
    using System.ComponentModel.DataAnnotations;

    public class OrderAdmin
    {
        [Key]
        public string OrderId { get; set; }

        public string CustomerName { get; set; }

        [Required]
        public int CustomerId { get; set; }

        public CustomerEntity Customer { get; set; }

        [Required]
        public DateTime OrderDate { get; set; }

        [Required, MaxLength(50)]
        public string Status { get; set; }

        [Required, Range(0, double.MaxValue)]
        public decimal TotalAmount { get; set; }

        [Required]
        public string PickupOrDelivery { get; set; }

        public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    }
}