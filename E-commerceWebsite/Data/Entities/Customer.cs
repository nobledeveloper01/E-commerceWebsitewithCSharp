using System.ComponentModel.DataAnnotations;

namespace E_commerceWebsite.Data.Entities
{
    public class CustomerEntity
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; }

        public int UserId { get; set; }

        [Required, EmailAddress, MaxLength(100)]
        public string Email { get; set; }

        [Required, MaxLength(500)]
        public string ShippingAddress { get; set; }

        // Navigation properties
        public ICollection<OrderAdmin> Orders { get; set; }
        public ICollection<OrderDetail> OrderDetails { get; set; }
        public User User { get; set; }
    }
}
