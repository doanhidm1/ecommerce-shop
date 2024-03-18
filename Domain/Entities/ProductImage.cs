using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("ProductImages")]
    public class ProductImage : BaseEntity
    {
        [Column(TypeName = "nvarchar(max)")]
        public string ImageLink { get; set; } = string.Empty;

        [Column(TypeName = "nvarchar(100)")]
        public string Alt { get; set; } = string.Empty;

        [ForeignKey("Image-Product")]
        public Guid ProductId { get; set; }
        public Product Product { get; set; }
    }
}
