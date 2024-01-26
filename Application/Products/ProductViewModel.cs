namespace Application.Products
{
    public class ProductViewModel
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public Guid CategoryId { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string ImageAlt { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal? DiscountPrice { get; set; }
        public double? Rating { get; set; }
        public int ReviewCount { get; set; }
    }
}
