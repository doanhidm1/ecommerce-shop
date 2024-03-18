using Domain.Enums;

namespace Application.Checkout
{
    public class BillCreateViewModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string Street { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public int ZipCode { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string? Note { get; set; }

        public PaymentMethod PaymentMethod { get; set; }
        public List<BillDetailCreateViewModel> BillDetails { get; set; }
    }
    public class BillDetailCreateViewModel
    {
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }

}
