namespace Application.Products
{
    public class ProductViewModel
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public DateTime CreatedDate { get; set; }
        public Guid CategoryId { get; set; }
        public string ImageUrl { get; set; }
        public decimal Price { get; set; }
        public decimal? DiscountPrice { get; set; }
        public int? Rating { get; set; }
        public int ReviewCount { get; set; }
    }
}
