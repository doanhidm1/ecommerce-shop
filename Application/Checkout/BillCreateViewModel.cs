using Domain.Enums;

namespace Application.Checkout
{
    public class BillCreateViewModel
    {
        public string CustomerName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string CityProvince { get; set; }
        public string DistrictTown { get; set; }
        public string WardCommune { get; set; }
        public string ExactAddress { get; set; }
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
