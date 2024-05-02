namespace Application.Orders
{
    public class OrderListingPageModel
    {
        public Dictionary<int, string> Status { get; set; } = [];
        public Dictionary<int, string> OrderBy { get; set; } = [];
    }
}
