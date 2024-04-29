using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("BillDetails")]
    public class OrderDetail : BaseEntity
    {
        [Column(TypeName = "nvarchar(100)")]
        public string ProductName { get; set; }
        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        [ForeignKey("Bill-BillDetail")]
        public Guid BillId { get; set; }
        public Bill Bill { get; set; }

    }
}
