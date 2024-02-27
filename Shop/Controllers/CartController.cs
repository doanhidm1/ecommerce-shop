using Application;
using Application.Products;
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
    }
}
