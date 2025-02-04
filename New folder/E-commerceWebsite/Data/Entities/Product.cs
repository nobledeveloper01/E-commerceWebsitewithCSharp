using System.ComponentModel.DataAnnotations;

public class Product
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; }

    [Required, Range(1, int.MaxValue)]
    public int Quantity { get; set; }

    [Required, Range(0, double.MaxValue)]
    public decimal Price { get; set; }

    public ICollection<OrderDetail> OrderDetails { get; set; }
}
