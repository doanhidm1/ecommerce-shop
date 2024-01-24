using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
	[Table("Categories")]
	public class Category : BaseEntity
	{
		[Column(TypeName = "nvarchar(100)")]
		public string Name { get; set; }

		[Column(TypeName = "nvarchar(100)")]
		public string? Image { get; set; }

		public ICollection<Product>? Products { get; set; }
	}
}
