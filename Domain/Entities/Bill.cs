using Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("Bills")]
    public class Bill : BaseEntity
    {
        [Column(TypeName = "nvarchar(100)")]
        public string CustomerName { get; set; } = string.Empty;

        [Column(TypeName = "nvarchar(10)")]
        public string PhoneNumber { get; set; }

        [Column(TypeName = "nvarchar(100)")]
        public string Email { get; set; }

        [Column(TypeName = "nvarchar(100)")]
        public string CityProvince { get; set; }

        [Column(TypeName = "nvarchar(100)")]
        public string DistrictTown { get; set; }

        [Column(TypeName = "nvarchar(100)")]
        public string WardCommune { get; set; }

        [Column(TypeName = "nvarchar(200)")]
        public string ExactAddress { get; set; }

        [Column(TypeName = "ntext")]
        public string? Note { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }
        public PaymentMethod PaymentMethod { get; set; }

        public ICollection<OrderDetail> BillDetails { get; set; }
    }
}
