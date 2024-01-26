using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
	[Table("Categories")]
	public class Category : BaseEntity
	{
		[Column(TypeName = "nvarchar(100)")]
		public string Name { get; set; } = string.Empty;

		[Column(TypeName = "ntext")]
		public string Image { get; set; } = string.Empty;

		public ICollection<Product>? Products { get; set; } 
	}
}
