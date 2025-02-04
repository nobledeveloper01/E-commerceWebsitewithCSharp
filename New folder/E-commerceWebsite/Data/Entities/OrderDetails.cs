using E_commerceWebsite.Data.Entities;
using E_commerceWebsite.Data;
using System.ComponentModel.DataAnnotations;

public class OrderDetail
{
    public int Id { get; set; }

    [Required]
    public string OrderId { get; set; }
    public OrderAdmin Order { get; set; }

    [Required]
    public int CustomerId { get; set; }
    public CustomerEntity Customer { get; set; }

    [Required]
    public int ProductId { get; set; }
    public AdminProduct Product { get; set; }
    public int? ProductId1 { get; set; }

    [Required]
    public int Quantity { get; set; }

    [Required]
    [DataType(DataType.Currency)]
    public decimal UnitPrice { get; set; }

    public decimal TotalPrice => Quantity * UnitPrice;

    [Required]
    public string PickupOrDelivery { get; set; } // New property
}
