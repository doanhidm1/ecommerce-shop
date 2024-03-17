namespace Application.Products
{
    public class ProductDetailViewModel
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public DateTime CreatedDate { get; set; }
        public int Stock { get; set; }
        public Guid CategoryId { get; set; }
        public Guid BrandId { get; set; }
        public decimal Price { get; set; }
        public decimal? DiscountPrice { get; set; }
        public bool IsFeatured { get; set; }
        public string Detail { get; set; }
        public string Description { get; set; }
        public List<ImageViewModel> Images { get; set; } = new();
        public List<ReviewViewModel> Reviews { get; set; } = new();
        public int ReviewCount { get; set; }
        public double? Rating { get; set; }
    }

    public class ImageViewModel
    {
        public Guid Id { get; set; }
        public string? ImageLink { get; set; }
        public string? Alt { get; set; }
    }

    public class ReviewViewModel
    {
        public Guid Id { get; set; }
        public string ReviewerName { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Content { get; set; }
        public int Rating { get; set; }
    }
}
