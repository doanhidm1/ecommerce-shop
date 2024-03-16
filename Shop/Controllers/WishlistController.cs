using Application;
using Application.Products;
using Microsoft.AspNetCore.Mvc;

namespace Shop.Controllers
{
    public class WishlistController : Controller
    {
        private readonly IProductService _productService;

        public WishlistController(IProductService productService)
        {
            _productService = productService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult WishlistPartial()
        {
            var wishlist = HttpContext.Session.GetT<WishlistItemViewModel>(ShopConstants.Wishlist);
            return PartialView(wishlist ?? new List<WishlistItemViewModel>());
        }

        public async Task<IActionResult> AddToWishlist(Guid productId)
        {
            var wishlistItem = await _productService.GetProductDetailForWishlist(productId);
            if (wishlistItem == null)
            {
                return Json(new ResponseResult(404, "Product is not found"));
            }
            var wishlist = HttpContext.Session.GetT<WishlistItemViewModel>(ShopConstants.Wishlist);

            if (wishlist == null)
            {
                HttpContext.Session.SetT<WishlistItemViewModel>(ShopConstants.Wishlist, new List<WishlistItemViewModel>() { wishlistItem });
                return Json(new ResponseResult(200, $"Add {wishlistItem.ProductName} to wishlist success!"));
            }
            var item = wishlist.FirstOrDefault(s => s.ProductId == wishlistItem.ProductId);
            if (item == null)
            {
                wishlist.Add(wishlistItem);
                HttpContext.Session.SetT<WishlistItemViewModel>(ShopConstants.Wishlist, wishlist);
                return Json(new ResponseResult(200, $"Add {wishlistItem.ProductName} to wishlist success!"));
            }
            return Json(new ResponseResult(200, $"{wishlistItem.ProductName} is already in wishlist!"));
        }

        public async Task<IActionResult> RemoveFromWishlist(Guid productId)
        {
            var wishlistItem = await _productService.GetProductDetailForWishlist(productId);
            if (wishlistItem == null)
            {
                return Json(new ResponseResult(404, "Product is not found"));
            }
            var wishlist = HttpContext.Session.GetT<WishlistItemViewModel>(ShopConstants.Wishlist);
            if (wishlist == null)
            {
                return Json(new ResponseResult(404, "Wishlist is empty"));
            }
            else
            {
                wishlist.RemoveAll(s => s.ProductId == productId);
                HttpContext.Session.SetT<WishlistItemViewModel>(ShopConstants.Wishlist, wishlist);
                return Json(new ResponseResult(200, $"Remove {wishlistItem.ProductName} success!"));
            }
        }
    }
}