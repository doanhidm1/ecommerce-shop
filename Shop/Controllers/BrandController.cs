using Application;
using Application.Brands;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Shop.Controllers
{
    [Authorize(Roles = "Admin,Manager")]
    public class BrandController : Controller
    {
        private readonly IBrandService _brandService;

        public BrandController(IBrandService brandService)
        {
            _brandService = brandService;
        }

        public async Task<IActionResult> Index()
        {
            var model = await _brandService.GetBrands() ?? new List<BrandViewModel>();
            return View(model);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(BrandCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(400, "New brand data invalided!"));
                return RedirectToAction("Create");
            }
            await _brandService.CreateBrand(model);
            TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(200, "New brand created successfully!"));
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Update(Guid id)
        {
            var brand = await _brandService.GetBrandDetail(id);
            if (brand == null)
            {
                TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(400, "Brand not found!"));
                return RedirectToAction("Index");
            }
            var model = new BrandUpdateViewModel
            {
                Id = brand.Id,
                Name = brand.Name
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Update(BrandUpdateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(400, "Update brand data invalided!"));
                return RedirectToAction("Update");
            }
            await _brandService.UpdateBrand(model);
            TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(200, "Brand updated successfully!"));
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                await _brandService.DeleteBrand(id);
                TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(200, "Brand successfully deleted!"));
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(400, "Brand failed to delete!"));
                return RedirectToAction("Index");
            }
        }
    }
}