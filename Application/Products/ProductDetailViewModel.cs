using Domain.Entities;

namespace Application.Products
{
    public class ProductDetailViewModel
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public DateTime CreatedDate { get; set; }
        public int Stock { get; set; }
        public Guid? CategoryId { get; set; }
        public Guid? BrandId { get; set; }
        public decimal Price { get; set; }
        public decimal? DiscountPrice { get; set; }
        public bool IsFeatured { get; set; }
        public string? Description { get; set; }
        public List<ProductImage> Images { get; set; } = new();
        public int ReviewCount { get; set; }
        public double? Rating { get; set; }
    }
}
