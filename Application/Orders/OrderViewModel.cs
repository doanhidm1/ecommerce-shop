using Domain.Enums;

namespace Application.Orders
{
    public class OrderViewModel
    {
        public Guid OrderId { get; set; }
        public string CustomerName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public DateTime OrderDate { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public EntityStatus Status { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Note { get; set; }
    }
}