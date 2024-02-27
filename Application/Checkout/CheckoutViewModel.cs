using Application.Products;

namespace Application.Checkout
{
    public class CheckoutViewModel
    {
        public List<CartItemViewModel> Items { get; set; }
        public Dictionary<int, string> ShippingMethod { get; set; }
    }
}
