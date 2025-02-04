namespace E_commerceWebsite.ViewModels
{
    public class OrderConfirmationViewModel
    {
        public string OrderId { get; set; }
        public List<OrderDetail> OrderDetails { get; set; }
        public string PickupOrDelivery { get; set; }
    }

}
