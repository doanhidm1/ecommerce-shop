using Application;
using Application.Brands;
using Application.Categories;
using Application.Helper;
using Application.Products;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Shop.Controllers
{
    [Authorize(Roles = "Admin,Manager")]
    public class AdminProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IBrandService _brandService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        private const string ImageFolder = "ProductImages";

        public AdminProductController
        (
            IProductService productService,
            ICategoryService categoryService,
            IBrandService brandService,
            IWebHostEnvironment webHostEnvironment
        )
        {
            _productService = productService;
            _categoryService = categoryService;
            _brandService = brandService;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            var model = new ProductListingPageModel
            {
                Categories = await _categoryService.GetCategories(),
                Brands =  await _brandService.GetBrands(),
                OrderBys = EnumHelper.GetList(typeof(SortEnum))
            };
            return View(model);
        }

        public async Task<IActionResult> Create()
        {
            var model = new ProductListingPageModel
            {
                Categories = await _categoryService.GetCategories(),
                Brands = await _brandService.GetBrands(),
            };
            return View(model);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Create(ProductCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(400, "New product data invalided!"));
                return RedirectToAction("Create");
            }
            try
            {
                model.ImageUrls = await SaveImage(model.Images);
                await _productService.CreateProduct(model);
                TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(200, $"Created {model.ProductName} successfully"));
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(400, $"Create product failed!"));
                return RedirectToAction("Create");
            }
        }

        public async Task<IActionResult> ProductListPartial([FromBody] ProductPage model)
        {
            var result = await _productService.GetProducts(model);
            return PartialView(result);
        }

        private async Task<List<string>> SaveImage(List<IFormFile> images)
        {
            var imageLinks = new List<string>();
            foreach (var image in images)
            {
                string uploadFolder = Path.Combine(_webHostEnvironment.WebRootPath, ShopConstants.UploadFolder);
                string productImageDir = Path.Combine(uploadFolder, ImageFolder);
                if (!Directory.Exists(productImageDir))
                {
                    Directory.CreateDirectory(productImageDir);
                }
                string fileName = $"{Guid.NewGuid()}-{image.FileName}";
                string fileUrl = $"/{ShopConstants.UploadFolder}/{ImageFolder}/{fileName}";
                using var stream = new FileStream(Path.Combine(productImageDir, fileName), FileMode.Create);
                await image.CopyToAsync(stream);
                imageLinks.Add(fileUrl);
            }
            return imageLinks;
        }

        private void DeleteImage(List<ImageViewModel> images)
        {
            var uploadFolder = Path.Combine(_webHostEnvironment.WebRootPath, ShopConstants.UploadFolder);
            var productImageDir = Path.Combine(uploadFolder, ImageFolder);
            foreach (var image in images)
            {
                if (image.ImageLink == null) continue;
                var fileName = image.ImageLink.Split('/').Last();
                var filePath = Path.Combine(productImageDir, fileName);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }
        }

        public async Task<IActionResult> Update(Guid id)
        {
            var product = await _productService.GetProductDetail(id);
            if (product == null)
            {
                TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(400, "Product not found!"));
                return RedirectToAction("Index");
            }
            var model = new ProductUpdateViewModel
            {
                ProductId = product.ProductId,
                ProductName = product.ProductName,
                Price = product.Price,
                DiscountPrice = product.DiscountPrice,
                Quantity = product.Stock,
                Detail = product.Detail,
                Description = product.Description,
                IsFeatured = product.IsFeatured,
                CategoryId = product.CategoryId,
                BrandId = product.BrandId,
            };
            var pageModel = new ProductListingPageModel
            {
                Categories = await _categoryService.GetCategories(),
                Brands = await _brandService.GetBrands(),
            };
            ViewBag.Data = pageModel;
            return View(model);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Update(ProductUpdateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(400, "Update product data invalided!"));
                return RedirectToAction("Update", new { id = model.ProductId });
            }
            try
            {
                await _productService.UpdateProduct(model);
                TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(200, $"Updated {model.ProductName} successfully"));
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(400, $"Update product failed!"));
                return RedirectToAction("Update", new { id = model.ProductId });
            }
        }

        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var product = await _productService.GetProductDetail(id);
                if (product == null)
                {
                    TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(400, "Product not found!"));
                    return RedirectToAction("Index");
                }
                if (product.Images.Any())
                {
                    DeleteImage(product.Images);
                }
                await _productService.DeleteProduct(id);
                TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(200, "Product successfully deleted"));
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(400, "Product failed to delete!"));
                return RedirectToAction("Index");
            }
        }
    }
}
