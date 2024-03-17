using Application;
using Application.Brands;
using Application.Categories;
using Application.Helper;
using Application.Products;
using Microsoft.AspNetCore.Mvc;

namespace Shop.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IBrandService _brandService;

        public ProductController
        (
            IProductService productService,
            ICategoryService categoryService,
            IBrandService brandService
        )
        {
            _productService = productService;
            _categoryService = categoryService;
            _brandService = brandService;
        }
        public async Task<IActionResult> Index()
        {
            var model = new ProductListingPageModel
            {
                Categories = await _categoryService.GetCategories(),
                Brands = await _brandService.GetBrands(),
                OrderBys = EnumHelper.GetList(typeof(SortEnum))
            };
            return View(model);
        }

        public async Task<IActionResult> ProductListPartial([FromBody] ProductPage model)
        {
            var result = await _productService.GetProducts(model);
            return PartialView(result);
        }

        public async Task<IActionResult> FeaturedProductsPartial()
        {
            var result = await _productService.GetProducts(new ProductPage { IsFeatured = true });
            return PartialView(result);
        }

        public async Task<IActionResult> Detail(Guid id)
        {
            var productViewModel = await _productService.GetProductDetail(id);
            // if product = null return view error in shared
            if (productViewModel == null)
            {
                return PartialView("PageNotFound");
            }
            return View(productViewModel);
        }

        public async Task<IActionResult> DetailPopup(Guid id)
        {
            var productViewModel = await _productService.GetProductDetail(id);
            // if product = null return view error in shared
            if (productViewModel == null)
            {
                ViewBag.IsPopup = true;
                return PartialView("PageNotFound");
            }
            return PartialView(productViewModel);
        }
    }
}

