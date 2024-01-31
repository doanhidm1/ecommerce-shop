using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class Brand : BaseEntity
    {
        [Column(TypeName = ("nvarchar(100)"))]
        public string Name { get; set; } = string.Empty;
        public ICollection<Product>? Products { get; set; }
    }
}
