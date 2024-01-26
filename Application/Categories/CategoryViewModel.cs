namespace Application.Categories
{
	public class CategoryViewModel
	{
		public Guid Id { get; set; }
		public string Name { get; set; } = string.Empty;
		public string Image { get; set; } = string.Empty;
		public int ProductCount { get; set; }
	}
}