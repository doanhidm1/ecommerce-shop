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

        public async Task<IActionResult> Index()
        {
            var students = await _studentService.GetStudentsAsync();
            return View(students);
        }

        public IActionResult CreateStudent()
        {
            return View();
        }
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Save(CreateStudentRequest request)
        {
            try
            {
                await _studentService.AddStudent(request);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError("[Home/Save]", ex);
                return RedirectToAction("Error");
            }
        }

        public async Task<IActionResult> UpdateStudent(Guid id)
        {
            return Ok(10);
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
