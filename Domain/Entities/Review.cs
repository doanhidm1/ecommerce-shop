using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
	[Table("Reviews")]
	public class Review : BaseEntity
	{

		[Column(TypeName = "nvarchar(100)")]
		public string ReviewerName { get; set; }

		[Column(TypeName = "nvarchar(100)")]
		public string? Email { get; set; }

		[Column(TypeName = "ntext")]
		public string Content { get; set; }
		public int Rating { get; set; }

		[ForeignKey("Review-Product")]
		public Guid ProductId { get; set; }
		public Product Product { get; set; }

	}
}

