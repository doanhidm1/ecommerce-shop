using Application;
using Application.Helper;
using Application.Orders;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

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

        public async Task<IActionResult> Detail(Guid id)
        {
            var reult = await _orderService.GetOrderDetail(id);
            return View(reult);
        }

        [HttpPost]
        public async Task<IActionResult> Update(OrderUpdateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(400, "Update order status data invalided!"));
                return RedirectToAction("Update");
            }
            await _orderService.UpdateOrder(model);
            TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(200, "Order status updated successfully!"));
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Update(Guid id)
        {
            var order = await _orderService.GetOrderDetail(id);
            if (order == null)
            {
                TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(400, "Order not found!"));
                return RedirectToAction("Index");
            }
            var model = new OrderUpdateViewModel
            {
                OrderId = order.OrderId,
                Status = order.Status,
            };
            return View(model);
        }
    }
}
