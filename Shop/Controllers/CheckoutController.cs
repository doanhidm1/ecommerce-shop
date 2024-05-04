using Application;
using Application.Checkout;
using Application.Helper;
using Application.Payment;
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
        private readonly IPayment _paymentService;
        private const decimal RATE = 25400;

        public CheckoutController(
            IBillService billService,
            IProductService productService,
            IPayment paymentService)
        {
            _billService = billService;
            _productService = productService;
            _paymentService = paymentService;
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

        public IActionResult PaymentResult([FromQuery] MomoRedirectVM model)
        {
            Console.WriteLine("redirect data from momo:", JsonConvert.SerializeObject(model));
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ReceiveIpn([FromBody] MomoIpnVM ipnData)
        {
            Console.WriteLine("ipn data from momo:", JsonConvert.SerializeObject(ipnData));
            await _paymentService.PostPaymentProcess(ipnData);
            return NoContent();
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
                Id = Guid.NewGuid(),
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
                    Id = Guid.NewGuid(),
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
                switch (model.PaymentMethod)
                {
                    case PaymentMethod.ONLINE_MOMO:
                        var data = new CreateMomoOrderVM
                        {
                            OrderId = billModel.Id,
                            Amount = (long)(billModel.BillDetails.Sum(s => s.Quantity * s.Price * RATE)),
                            items = billModel.BillDetails.Select(s => new OrderItems
                            {
                                id = s.Id.ToString(),
                                name = s.ProductName,
                                quantity = s.Quantity,
                                purchaseAmount = s.Price * RATE,
                                totalAmount = (long)(s.Price * s.Quantity * RATE)
                            }).ToList(),
                            userInfo = new UserInfo
                            {
                                name = model.CustomerName,
                                phoneNumber = model.PhoneNumber,
                                email = model.Email
                            }
                        };
                        var payUrl = await _paymentService.CreateOrder(data);
                        return Redirect(payUrl);
                    case PaymentMethod.PAY_WHEN_RECEIVE:
                        var response = new ResponseResult(200, "Place order success");
                        HttpContext.Session.Remove(ShopConstants.Cart);
                        TempData["checkout"] = JsonConvert.SerializeObject(response);
                        return RedirectToAction("Index");
                    default:
                        throw new Exception("Invalid payment method");
                }
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
