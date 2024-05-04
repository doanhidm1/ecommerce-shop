using System.ComponentModel.DataAnnotations;

namespace Application.Payment
{
    public class CreateMomoOrderVM
    {
        [Required]
        [Range(0.04, 1967.43d)]
        public long Amount { get; set; }

        [Required]
        public Guid OrderId { get; set; }

        [Required]
        public List<OrderItems> items { get; set; }

        [Required]
        public UserInfo userInfo { get; set; }
    }

    public class OrderItems
    {
        [Required]
        public string id { get; set; }

        [Required]
        public string name { get; set; }

        [Required]
        public string currency { get; set; } = "VND";

        [Required]
        public int quantity { get; set; }

        [Required]
        public decimal purchaseAmount { get; set; }

        [Required]
        public long totalAmount { get; set; }
    }

    public class UserInfo
    {
        [Required]
        public string name { get; set; }

        [Required]
        public string phoneNumber { get; set; }

        [Required]
        public string email { get; set; }
    }
}