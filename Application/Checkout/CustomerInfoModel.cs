using Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Application.Checkout
{
    public class CustomerInfoModel
    {
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        public string Street { get; set; }
        [Required]
        public string City { get; set; }
        [Required]
        public string Country { get; set; }
        [Required]
        public int ZipCode { get; set; }
        [Required]
        public string PhoneNumber { get; set; }
        [Required]
        public string Email { get; set; }
        public string? Note { get; set; }
        [Required]
        public PaymentMethod PaymentMethod { get; set; }
    }
}
