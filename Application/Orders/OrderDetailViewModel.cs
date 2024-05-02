using Domain.Entities;
using Domain.Enums;

namespace Application.Orders
{
    public class OrderDetailViewModel
    {
        public List<OrderDetail> OrderDetailItems { get; set; }
        public decimal TotalPrice { get; set; }
        public EntityStatus Status { get; set; }
        public DateTime OrderDate { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
    }
}