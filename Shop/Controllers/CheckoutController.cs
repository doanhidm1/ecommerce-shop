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
        private readonly IProductService _productService;
        public CheckoutController(IBillService billService, IProductService productService)
        {
            _billService = billService;
            _productService = productService;
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

        // check item quantity before place order
        private async Task<bool> CheckQuantity()
        {
            var cart = HttpContext.Session.GetT<CartItemViewModel>(ShopConstants.Cart);
            foreach (var item in cart)
            {
                var product = await _productService.GetProductDetail(item.ProductId);
                if (product.Stock < item.Quantity)
                {
                    cart.Remove(item);
                    HttpContext.Session.SetT(ShopConstants.Cart, cart);
                    return false;
                }
            }
            return true;
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
            var checkQuantity = await CheckQuantity();
            if (!checkQuantity)
            {
                var response = new ResponseResult(400, "Some items are out of stock");
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
                CustomerName = model.CustomerName,
                CityProvince = model.CityProvince,
                DistrictTown = model.DistrictTown,
                WardCommune = model.WardCommune,
                ExactAddress = model.ExactAddress,
                PhoneNumber = model.PhoneNumber,
                Email = model.Email,
                Note = model.Note,
                PaymentMethod = model.PaymentMethod,
                BillDetails = cart.Select(s => new BillDetailCreateViewModel
                {
                    Price = s.Price,
                    ProductName = s.ProductName,
                    Quantity = s.Quantity,
                }).ToList()
            };
            try
            {
                await _billService.CreateBill(billModel);
                foreach (var item in cart)
                {
                    await _productService.UpdateQuantity(item.ProductId, item.Quantity);
                }
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
