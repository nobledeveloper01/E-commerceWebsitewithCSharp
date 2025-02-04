using System.ComponentModel.DataAnnotations;

public class OrderViewModel
{
    [Required]
    public string Name { get; set; }

    [Required, EmailAddress]
    public string Email { get; set; }

    [Required]
    public string ShippingAddress { get; set; }

    [Required]
    [RegularExpression("pickup|delivery", ErrorMessage = "Must be 'pickup' or 'delivery'")]
    public string PickupOrDelivery { get; set; }
}
