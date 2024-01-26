using Application.Categories;

namespace Application.Products
{
    public class ProductListingPageModel
    {
        public List<CategoryViewModel> Categories { get; set; } = new();
        public Dictionary<int, string> OrderBys { get; set; } = new();
        public List<int> SelectPageSize { get; set; } = new();
        public string CategoryId { get; set; } = string.Empty;
        public string KeyWord { get; set; } = string.Empty;
    }
}