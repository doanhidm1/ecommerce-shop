using Microsoft.AspNetCore.Mvc;

namespace Shop.Controllers
{
    public class PagesController : Controller
    {
        public IActionResult FAQ()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }
    }
}
