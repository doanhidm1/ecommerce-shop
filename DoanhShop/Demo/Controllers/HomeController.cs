using Application.Students;
using Demo.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Demo.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IStudentService _studentService;
        public HomeController(ILogger<HomeController> logger, IStudentService studentService)
        {
            _logger = logger;
            _studentService = studentService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> StudentPartial([FromBody] Page model)
        {
            var students = await _studentService.GetStudentsAsync(model);
            return PartialView(students);
        }

        public IActionResult CreateStudent()
        {
            return View();
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> CreateStudent(CreateStudentRequest request)
        {
            try
            {
                await _studentService.AddStudent(request);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError("[Home/AddStudent]", ex);
                return RedirectToAction("Error");
            }
        }

        public async Task<IActionResult> UpdateStudent(Guid id)
        {
            var student = await _studentService.GetStudentsByIdAsync(id);
            return View(student);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> UpdateStudent(UpdateStudentRequest request)
        {
            try
            {
                await _studentService.UpdateStudent(request);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError("[Home/DeleteStudent]", ex);
                return RedirectToAction("Error");
            }

        }

        public async Task<IActionResult> DeleteStudent(Guid id)
        {
            var student = await _studentService.GetStudentsByIdAsync(id);
            return View(student);
        }


        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> RemoveStudent(Guid id)
        {
            try
            {
                await _studentService.DeleteStudent(id);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError("[Home/RemoveStudent]", ex);
                return RedirectToAction("Error");
            }

        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Hello()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
