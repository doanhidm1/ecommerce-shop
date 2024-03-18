using Application;
using Application.Checkout;
using Application.Helper;
using Application.Products;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Shop.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly IBillService _billService;
        public CheckoutController(IBillService billService)
        {
            _billService = billService;
        }
        public IActionResult Index(int PaymentMethod)
        {
            var model = new CheckoutViewModel();
            var cart = HttpContext.Session.GetT<CartItemViewModel>(ShopConstants.Cart);
            model.Items = cart ?? new List<CartItemViewModel>();
            model.ShippingMethod = EnumHelper.GetList(typeof(PaymentMethod));
            ViewBag.PaymentMethod = PaymentMethod;
            return View(model);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> PlaceOrder(CustomerInfoModel model)
        {
            if (!ModelState.IsValid)
            {
                var response = new ResponseResult(400, "Invalid order information");
                TempData["checkout"] = JsonConvert.SerializeObject(response);
                return RedirectToAction("Index");
            }
            var cart = HttpContext.Session.GetT<CartItemViewModel>(ShopConstants.Cart);
            if (cart == null)
            {
                return Json(new ResponseResult(400, "cart is empty"));
            }
            var billModel = new BillCreateViewModel
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Street = model.Street,
                City = model.City,
                Country = model.Country,
                ZipCode = model.ZipCode,
                PhoneNumber = model.PhoneNumber,
                Email = model.Email,
                Note = model.Note,
                PaymentMethod = model.PaymentMethod
            };

            billModel.BillDetails = cart.Select(s => new BillDetailCreateViewModel
            {
                Price = s.Price,
                ProductName = s.ProductName,
                Quantity = s.Quantity,
            }).ToList();
            try
            {
                await _billService.CreateBill(billModel);
                var response = new ResponseResult(200, "Place order success");
                HttpContext.Session.Remove(ShopConstants.Cart);
                TempData["checkout"] = JsonConvert.SerializeObject(response);
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                var response = new ResponseResult(400, "Some thing went wrong");
                TempData["checkout"] = JsonConvert.SerializeObject(response);
                return RedirectToAction("Index");
            }
        }
    }

}
