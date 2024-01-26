using Application;
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

		public ProductController(IProductService productService, ICategoryService categoryService)
		{
			_productService = productService;
			_categoryService = categoryService;
		}
		public IActionResult Index()
		{
            var model = new ProductListingPageModel
            {
                Categories = _categoryService.GetCategories(),
                SelectPageSize = new List<int> { 6, 9, 18 },
                OrderBys = EnumHelper.GetList(typeof(SortEnum))
            };
            return View(model);
		}

		public IActionResult ProductListPartial([FromBody] ProductPage model)
		{
			var result = _productService.GetProducts(model);
			return PartialView(result);
		}

		public IActionResult Detail(Guid id)
		{
			return View();
		}
	}
}

