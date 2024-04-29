using Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Application.Checkout
{
    public class CustomerInfoModel
    {
        [Required]
        public string CustomerName { get; set; }
        [Required]
        public string PhoneNumber { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string CityProvince { get; set; }
        [Required]
        public string DistrictTown { get; set; }
        [Required]
        public string WardCommune { get; set; }
        [Required]
        public string ExactAddress { get; set; }
        public string? Note { get; set; }
        [Required]
        public PaymentMethod PaymentMethod { get; set; }
    }
}
