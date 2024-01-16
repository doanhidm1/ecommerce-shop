using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    //TNT91
    [Table("Categories")]
    public class Category : BaseEntity
    {
        [Column(TypeName = "nvarchar(1000)")]
        public string Name { get; set; }

        [Column(TypeName = "nvarchar(1000)")]
        public string Image { get; set; }

        public ICollection<Product> Products { get; set; }
    }
}
