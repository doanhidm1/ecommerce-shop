using Application;
using Application.Checkout;
using Application.Helper;
using Application.Products;
using Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Shop.Controllers
{
    public class CartController : Controller
    {
        private readonly IProductService _productService;

        public CartController(IProductService productService)
        {
            _productService = productService;
        }

        public IActionResult Index()
        {
            var model = new CheckoutViewModel();
            var cart = HttpContext.Session.GetT<CartItemViewModel>(ShopConstants.Cart);
            model.Items = cart ?? new List<CartItemViewModel>();
            model.ShippingMethod = EnumHelper.GetList(typeof(PaymentMethod));
            return View(model);
        }

        public IActionResult CartPartial()
        {
            var cart = HttpContext.Session.GetT<CartItemViewModel>(ShopConstants.Cart);
            return PartialView(cart ?? new List<CartItemViewModel>());
        }

        public async Task<IActionResult> AddToCart(Guid productId, int qty)
        {
            if(qty <= 0)
            {
                return Json(new ResponseResult(400, "Quantity must be greater than 0"));
            }
            var cartItem = await _productService.GetProductDetailForCart(productId);
            if (cartItem == null)
            {
                return Json(new ResponseResult(404, "Product is not found"));
            }
            if (cartItem.Stock < qty)
            {
                return Json(new ResponseResult(400, "Product is out of stock"));
            }
            cartItem.Quantity = qty;
            var cart = HttpContext.Session.GetT<CartItemViewModel>(ShopConstants.Cart);
            if (cart == null)
            {
                HttpContext.Session.SetT<CartItemViewModel>(ShopConstants.Cart, new List<CartItemViewModel>() { cartItem });
            }
            else
            {
                UpdateCartItemQuantity(cart, cartItem);
                HttpContext.Session.SetT<CartItemViewModel>(ShopConstants.Cart, cart);
            }
            return Json(new ResponseResult(200, $"Add {cartItem.ProductName} to cart success!"));
        }

        private void UpdateCartItemQuantity(List<CartItemViewModel> cart, CartItemViewModel cartItem)
        {
            var item = cart.FirstOrDefault(s => s.ProductId == cartItem.ProductId);
            if (item != null)
            {
                item.Quantity += cartItem.Quantity;
            }
            else
            {
                cart.Add(cartItem);
            }
        }

        public async Task<IActionResult> RemoveFromCart(Guid productId)
        {
            var cartItem = await _productService.GetProductDetailForCart(productId);
            if (cartItem == null)
            {
                return Json(new ResponseResult(404, "Product is not found"));
            }
            var cart = HttpContext.Session.GetT<CartItemViewModel>(ShopConstants.Cart);
            if (cart == null)
            {
                return Json(new ResponseResult(404, "Cart is empty"));
            }
            else
            {
                cart.RemoveAll(s => s.ProductId == productId);
                HttpContext.Session.SetT<CartItemViewModel>(ShopConstants.Cart, cart);
                return Json(new ResponseResult(200, $"Remove {cartItem.ProductName} success!"));
            }
        }

        [HttpPost]
        public IActionResult UpdateCart([FromBody] List<CartItemViewModel> updatedCart)
        {
            if (updatedCart == null || !updatedCart.Any())
            {
                return Json(new ResponseResult(400, "Invalid cart data"));
            }

            // Lấy giỏ hàng từ Session (hoặc cơ sở dữ liệu nếu bạn lưu giỏ hàng ở đó)
            var cart = HttpContext.Session.GetT<CartItemViewModel>(ShopConstants.Cart);
            if (cart == null)
            {
                return Json(new ResponseResult(404, "Cart is empty"));
            }

            // 1. Lấy danh sách sản phẩm chỉ chứa những sản phẩm cùng có mặt trong cả updatedCart và session
            var updatedProducts = cart.Where(item => updatedCart.Any(updatedItem => updatedItem.ProductId == item.ProductId)).ToList();

            // 2. Cập nhật số lượng sản phẩm trong danh sách
            foreach (var updatedItem in updatedProducts)
            {
                var matchingCartItem = updatedCart.FirstOrDefault(item => item.ProductId == updatedItem.ProductId);
                if (matchingCartItem != null)
                {
                    updatedItem.Quantity = matchingCartItem.Quantity;
                }
            }

            // 3. Set lại giỏ hàng trong Session (hoặc cơ sở dữ liệu nếu bạn lưu giỏ hàng ở đó)
            HttpContext.Session.SetT<CartItemViewModel>(ShopConstants.Cart, updatedProducts);

            return Json(new ResponseResult(200, "Cart updated successfully"));
        }
    }
}
