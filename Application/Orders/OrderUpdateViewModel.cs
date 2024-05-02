using Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Application.Orders
{
    public class OrderUpdateViewModel
    {
        [Required]
        public Guid OrderId { get; set; }

        [Required]
        public EntityStatus Status { get; set; }
    }
}
