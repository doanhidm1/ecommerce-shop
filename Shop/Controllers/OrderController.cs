using Application;
using Application.Helper;
using Application.Orders;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Shop.Controllers
{
    [Authorize(Roles = "Admin,Manager")]
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        public IActionResult Index()
        {
            var model = new OrderListingPageModel
            {
                Status = EnumHelper.GetList(typeof(EntityStatus)),
                OrderBy = EnumHelper.GetList(typeof(SortEnum)),
            };
            return View(model);
        }

        public async Task<IActionResult> OrderListPartial([FromBody] OrderPage model)
        {
            var result = await _orderService.GetOrders(model);
            return PartialView(result);
        }

        public IActionResult Detail(Guid orderId)
        {
            return View();
        }

        public IActionResult Update(OrderUpdateViewModel model)
        {
            return View();
        }
    }
}
