using Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
	public class BaseEntity
	{
		[Key]
		public Guid Id { get; set; }

		[Column(TypeName = "datetime")]
		public DateTime CreatedDate { get; set; }
		public EntityStatus Status { get; set; }
	}
}
