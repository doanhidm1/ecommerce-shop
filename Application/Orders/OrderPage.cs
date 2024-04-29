using Domain.Enums;

namespace Application.Orders
{
    public class OrderPage : Page
    {
        public string? CustomerName { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? CityProvince { get; set; }
        public string? DistrictTown { get; set; }
        public string? WardCommune { get; set; }
        public EntityStatus? Status { get; set; }
        public DateOnly? FromDate { get; set; }
        public DateOnly? ToDate { get; set; }
    }
}
