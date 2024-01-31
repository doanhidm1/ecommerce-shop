namespace Application.Products
{
    public class ProductPage : Page
    {
        public string KeyWord { get; set; } = string.Empty;
        public string CategoryId { get; set; } = string.Empty;
        public decimal? FromPrice { get; set; }
        public decimal? ToPrice { get; set; }
        public int? Rating { get; set; }
        public SortEnum SortBy { get; set; }
        public List<Guid> SelectedBrandIds { get; set; } = new List<Guid>();
    }
}