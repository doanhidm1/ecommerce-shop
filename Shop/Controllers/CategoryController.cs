using Application;
using Application.Categories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace Shop.Controllers
{
    [Authorize(Roles = "Admin,Manager")]
    public class CategoryController : Controller
    {
        private readonly ICategoryService _categoryService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        private const string ImageFolder = "CategoryImages";

        public CategoryController(ICategoryService categoryService, IWebHostEnvironment webHostEnvironment)
        {
            _categoryService = categoryService;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            var model = await _categoryService.GetCategories() ?? new List<CategoryViewModel>();
            return View(model);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CategoryCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(400, "New category data invalided!"));
                return RedirectToAction("Create");
            }
            model.ImageUrl = await SaveImage(model.Image);
            await _categoryService.CreateCategory(model);
            TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(200, "New category created successfully!"));
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Update(Guid id)
        {
            var category = await _categoryService.GetCategoryDetail(id);
            if (category == null)
            {
                TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(400, "Category not found!"));
                return RedirectToAction("Index");
            }
            var model = new CategoryUpdateViewModel
            {
                Id = category.Id,
                Name = category.Name,
                ImageUrl = category.Image
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Update(CategoryUpdateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(400, "Update category data invalided!"));
                return RedirectToAction("Update");
            }
            if (model.Image != null)
            {
                model.ImageUrl = await SaveImage(model.Image);
            }
            await _categoryService.UpdateCategory(model);
            TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(200, "Category updated successfully!"));
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var category = await _categoryService.GetCategoryDetail(id);
                await _categoryService.DeleteCategory(id);
                DeleteImage(category.Image);
                TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(200, "Category successfully deleted!"));
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(400, "Category failed to delete!"));
                return RedirectToAction("Index");
            }
        }

        private async Task<string> SaveImage(IFormFile image)
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
            return fileUrl;
        }

        private void DeleteImage(string image)
        {
            var uploadFolder = Path.Combine(_webHostEnvironment.WebRootPath, ShopConstants.UploadFolder);
            var productImageDir = Path.Combine(uploadFolder, ImageFolder);
            if (image.IsNullOrEmpty()) return;
            var fileName = image.Split('/').Last();
            var filePath = Path.Combine(productImageDir, fileName);
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
        }
    }
}